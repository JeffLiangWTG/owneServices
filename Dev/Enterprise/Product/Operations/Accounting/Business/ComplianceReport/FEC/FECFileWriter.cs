using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Accounting.Business.ComplianceReport.FEC
{
	public sealed class FECFileWriter : IDisposable
	{
		public FECFileWriter(AccComplianceReport complianceReport, ZDate chunkStartDate, GLAccountToLocalAccountMapping accountMappingHelper = null)
		{
			ComplianceReport = Argument.NotNull(complianceReport, nameof(complianceReport));
			PeriodCalculator = new AccountingPeriodCalculator(ComplianceReport.Factory, ComplianceReport.Company);
			ChunkStartDate = chunkStartDate;

			LocalAccountMappingHelper = accountMappingHelper ?? new GLAccountToLocalAccountMapping(complianceReport, SharedConstants.Languages.French);
			LocalCurrencyCode = ComplianceReport.Company.LocalCurrency.Code;
			LocalCurrencyDecimals = ComplianceReport.Company.LocalCurrency.Decimals;

			LedgerTypes = new LedgerTypesList();
			TransactionTypes = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
			TransactionTypes.AddRange(new CodeDescriptionPairList(OLookUpEditType.WIPAccrualTransactionTypes));
			SpecialTransactionTypes = new CodeDescriptionPairList(OLookUpEditType.InvoicePrintingTransactionTypes);
			SpecialJournalCode = string.Join("", SpecialTransactionTypes.GetAllCodes());
			SpecialJournalLib = string.Join(", ", SpecialTransactionTypes.ToArray().Select(x => x.GetMultilingualDescription()).OrderByDescending(x => x));

			ReceivablesShareSequentialInvoiceTransactionNumbers = AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.GetValueWithoutFallback(complianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).Value;
			PayablesShareSequentialInvoiceTransactionNumbers = AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.GetValueWithoutFallback(complianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).Value;

			FecDataProvider = new FECDataProvider(ComplianceReport);

			ExchangeRate = new ExchangeRate(ComplianceReport.Company.GC_IsReciprocal, LocalCurrencyDecimals, new Guid(ComplianceReport.Company.PK.ToString()));
			ZeroString = new ZDecimal(0.00).ToString(LocalCurrencyDecimals, true);
		}

		AccComplianceReport ComplianceReport { get; }
		readonly GLAccountToLocalAccountMapping LocalAccountMappingHelper;
		FECDataProvider FecDataProvider { get; }

		TempDirectory tempDir;
		StreamWriter streamWriter;
		bool disposed;

		CodeDescriptionPairList LedgerTypes { get; }
		CodeDescriptionPairList TransactionTypes { get; }
		CodeDescriptionPairList SpecialTransactionTypes { get; }

		ZString SpecialJournalCode { get; }
		ZString SpecialJournalLib { get; }

		bool ReceivablesShareSequentialInvoiceTransactionNumbers { get; }
		bool PayablesShareSequentialInvoiceTransactionNumbers { get; }

		ExchangeRate ExchangeRate { get; }
		String ZeroString { get; }
		ZInt LocalCurrencyDecimals { get; }
		ZString LocalCurrencyCode { get; }

		readonly ZDate ChunkStartDate;
		internal ZDate ChunkEndDatePlusOne
			=> chunkEndDatePlusOne.IsEmpty
				? chunkEndDatePlusOne = PeriodCalculator.GetLastDayForPeriod(ChunkId).Date.AddDays(1)
				: chunkEndDatePlusOne;

		ZDate chunkEndDatePlusOne;
		internal ZInt ChunkId 
			=> chunkId = chunkId == 0
				? chunkId = PeriodCalculator.GetPeriodFromDate(ChunkStartDate)
				: chunkId;
		ZInt chunkId;

		readonly AccountingPeriodCalculator PeriodCalculator;

		public string GetFilename()
		{
			// generate file name
			// The France FEC file name format is SIREN + “FEC” + YYYYMMDD + “.csv”
			// siren is the last 9 digits of VAT registration number of the report company's organisation proxy
			// YYYYMMDD is the fiscal year end date
			// The files generated for each month are named "CHUNKxxxxxx.csv" where xxxxxx is the accounting period included in this file
			if (ComplianceReport.ACR_DateTo >= ComplianceReport.NextProcessingStepFromDate)
			{
				return $"PERIOD{ChunkId:D6}.csv";
			}

			var fiscalYearEndDate = PeriodCalculator.GetLastDayForPeriod(PeriodCalculator.GetLastPeriodForYear(ComplianceReport.ACR_DateTo.Year));
			var vatRegNo = ComplianceReport.Company.OrgProxy.CustomsCodes.GetCustomsRegNo(Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.France), Core.Constants.CountryCodes.France);
			var siren = vatRegNo.Right(9);
			return $"{siren}FEC{fiscalYearEndDate:yyyyMMdd}.csv";
		}

		string GetFullFilename()
		{
			return Path.Combine(tempDir, GetFilename());
		}

		void EnsureOpenStream(Stream stream = null, bool autoFlush = false)
		{
			if ((streamWriter != null) && (stream == null))
			{
				return;
			}

			// we always write the data to a temporary file first (to allow us to write more than 2 GB of data)
			// finally this file will be zipped and attached to eDocs.
			tempDir = new TempDirectory();
			Stream outputStream = stream ?? new FileStream(GetFullFilename(), FileMode.Create);

			streamWriter = new StreamWriter(outputStream)
			{
				AutoFlush = autoFlush,
			};
		}

		void CloseStream()
		{
			streamWriter?.Dispose();
		}

		public void RedirectOutput(Stream stream, bool autoFlush = false)
		{
			Argument.NotNull(stream, nameof(stream));

			CloseStream();
			tempDir?.Dispose();

			EnsureOpenStream(stream, autoFlush);
		}

		/// <summary>
		/// Zips the content.
		/// </summary>
		/// <remarks>This method closes all internal streams, do not call Write methods after.</remarks>
		/// <returns>Compressed file path.</returns>
		public string ZipContent()
		{
			CloseStream();

			var reportFilename = GetFilename();
			var uncompressedFilePath = GetFullFilename();
			var uncompressedFileDir = tempDir;

			using (var zipFileTempDir = new TempDirectory())
			{
				var zipFilename = Path.ChangeExtension(reportFilename, ".zip");
				var zipTempFullFilename = Path.Combine(zipFileTempDir, zipFilename);

				// Zip file should be created in a separate folder outside of the source (CSV files) folder, because ZipCompression compresses all files in the source folder.
				// It might cause an exception if we place Zip file in the source folder.
				// We create a new temp folder "zipTempDir" for a Zip file and move it after.
				if (ZipCompression.Zip(uncompressedFileDir, zipTempFullFilename))
				{
					var compressedFilePath = Path.Combine(uncompressedFileDir, zipFilename);

					File.Move(zipTempFullFilename, compressedFilePath);

					return compressedFilePath;
				}

				// Fallback logic to return a CSV filename in case of ZipCompression failure.
				return uncompressedFilePath;
			}
		}

		public void WriteStream(StreamReader reader)
		{
			EnsureOpenStream();

			reader.CopyTo(streamWriter);
		}

		public void WriteFECHeader()
		{
			EnsureOpenStream();

			const string columnNames = "JournalCode|JournalLib|EcritureNum|EcritureDate|CompteNum|CompteLib|CompAuxNum|CompAuxLib|PieceRef|PieceDate|EcritureLib|Debit|Credit|EcritureLet|DateLet|ValidDate|Montantdevise|Idevise";
			streamWriter.WriteLine(columnNames);
		}

		public void WriteFECOpeningBalances()
		{
			EnsureOpenStream();

			var openingBalances = FecDataProvider.RetrieveOpenBalances();
			foreach (var ob in openingBalances)
			{
				var (frenchAccountNo, frenchAccountDescription) = LocalAccountMappingHelper.GetLocalAccount(ob.AccountPK, "****OB-");
				var startDate = ZDateTimeToString(ComplianceReport.ACR_DateFrom);
				var validDate = ComplianceReport.ACR_DateFrom.ToString("ddMMyy", CultureInfo.InvariantCulture);
				var debitAmount = ob.Balance > 0m ? ob.Balance.ToString(LocalCurrencyDecimals, true) : ZeroString;
				var creditAmount = ob.Balance < 0m ? ((ZDecimal)(-ob.Balance)).ToString(LocalCurrencyDecimals, true) : ZeroString;
				var row = $"OPENING|Opening balance of the posting account|OPENING0001|{startDate}|{frenchAccountNo}|{frenchAccountDescription}" +
					$"|||Opening Balance|{startDate}|Opening Balance as at {validDate}|{debitAmount}|{creditAmount}|||{startDate}||";
				streamWriter.WriteLine(row);
			}
		}

		public void WriteFECTransactions()
		{
			EnsureOpenStream();

			// Use an extra DB connection to load the single transactions to avoid 
			// caching all currencies which might be used as transaction/foreign currency and/or GL account mapping to avoid exception
			// "There is already an open DataReader associated with this Command which must be closed first."
			// in the loop over all transactions (lines).
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (var command = FecDataProvider.OpenTransactionsDbCommand(ChunkStartDate, ChunkEndDatePlusOne, connection))
			using (var transactionsDataReader = command.ExecuteReader())
			{
				while (transactionsDataReader.Read())
				{
					var dataPerLine = new FECDataPerLine(transactionsDataReader);
					var transactionAsCsv = CreateTransactionAsCSV(dataPerLine);

					if (!string.IsNullOrEmpty(transactionAsCsv))
					{
						streamWriter.WriteLine(transactionAsCsv);
					}
				}
			}
		}

		internal string CreateTransactionAsCSV(FECDataPerLine dataPerLine)
		{
			// Performance note: The majority of the CPU time in this function is consumed by ZStringBuilder.ToStringWithDelimiterBetweenAppends.
			//                   If overall performance is too bad, then consider replacing ZStringBuilder with StringBuilder or using String.Join or write directly to the streamWriter.
			var (frenchAccountNumber, frenchAccountDescription) = LocalAccountMappingHelper.GetLocalAccount(dataPerLine.AccountPK, dataPerLine.ReportSubCode);
			var (journalCode, journalLib, ecritureNum) = GetJournalData(dataPerLine.Ledger, dataPerLine.TransactionType, dataPerLine.PieceRef, ReceivablesShareSequentialInvoiceTransactionNumbers, PayablesShareSequentialInvoiceTransactionNumbers, dataPerLine.InternalReference);
			var (debitAmount, creditAmount, montantDevise, idevise) = CalculateAmounts(dataPerLine.NetAmountLocal, dataPerLine.TaxAmountLocal, dataPerLine.ReportSubCode,
				dataPerLine.LineType, dataPerLine.VatBasis, dataPerLine.InputVatRecoverable, dataPerLine.IDevise, dataPerLine.GrossAmountForeign, dataPerLine.ExchangeRateForeign);

			if ((debitAmount == ZeroString) && (creditAmount == ZeroString))
			{
				return null;
			}

			var csvStringBuilder = new ZStringBuilder();
			csvStringBuilder.Append(journalCode);
			csvStringBuilder.Append(journalLib);
			csvStringBuilder.Append(ecritureNum);
			csvStringBuilder.Append(ZDateTimeToString(dataPerLine.PostDate));
			csvStringBuilder.Append(frenchAccountNumber);
			csvStringBuilder.Append(FormatStringValueForCsv(frenchAccountDescription));
			csvStringBuilder.Append(dataPerLine.OrganisationCode);
			csvStringBuilder.Append(FormatStringValueForCsv(dataPerLine.OrganisationName));
			csvStringBuilder.Append(dataPerLine.PieceRef);
			csvStringBuilder.Append(ZDateTimeToString(dataPerLine.PieceDate));
			csvStringBuilder.Append(FormatStringValueForCsv(dataPerLine.EcritureLib));
			csvStringBuilder.Append(debitAmount);
			csvStringBuilder.Append(creditAmount);
			csvStringBuilder.Append(dataPerLine.EcritureLet);
			csvStringBuilder.Append(ZDateTimeToString(dataPerLine.DateLet));
			csvStringBuilder.Append(ZDateTimeToString(dataPerLine.ValidDate));
			csvStringBuilder.Append(montantDevise);
			csvStringBuilder.Append(idevise);

			return csvStringBuilder.ToStringWithDelimiterBetweenAppends("|");
		}

		internal (ZString debitamount, ZString creditAmount, ZString montantDevise, ZString idevise) CalculateAmounts(ZDecimal netAmountLocal, ZDecimal taxAmountLocal, ZString reportSubCode, ZString lineType,
			ZString vatBasis, ZDecimal inputVatRecoverable, ZString foreignCurrency, ZDecimal grossAmountForeign, ZDecimal exchangeRate)
		{
			// Performance note: The majority of the CPU time in this function is consumed by ZDecimal.ToString. If overall performance is too bad, then consider replacing ZDecimal with Decimal.
			var amountLocal = ComplianceReport.CalculateGeneralLedgerAmountViaReportSubCode(netAmountLocal, taxAmountLocal, reportSubCode, lineType, vatBasis, inputVatRecoverable, LocalCurrencyCode, LocalCurrencyDecimals);
			var debitAmount = amountLocal > 0m ? amountLocal.ToString(LocalCurrencyDecimals, true) : ZeroString;
			var creditAmount = amountLocal < 0m ? ((ZDecimal)(-1m * amountLocal)).ToString(LocalCurrencyDecimals, true) : ZeroString;
			var montantDevise = string.Empty;
			var idevise = string.Empty;
			if (foreignCurrency != LocalCurrencyCode)
			{
				// calculate net and tax amount in foreign currency 
				var taxAmountForeign = ExchangeRate.LocalToForeign(taxAmountLocal, exchangeRate, foreignCurrency);
				var netAmountForeign = grossAmountForeign - taxAmountForeign;
				var foreignCurrencyDecimals = new Currency(foreignCurrency)?.Decimals ?? LocalCurrencyDecimals;
				var amountForeign = ComplianceReport.CalculateGeneralLedgerAmountViaReportSubCode(netAmountForeign, taxAmountForeign, reportSubCode, lineType, vatBasis, inputVatRecoverable, foreignCurrency, foreignCurrencyDecimals);
				montantDevise = amountForeign.ToString(foreignCurrencyDecimals, true);
				idevise = foreignCurrency;
			}
			return (debitAmount, creditAmount, montantDevise, idevise);
		}

		internal (string journalCode, string journalLib, string ecritureNum) GetJournalData(ZString ledger, ZString transactionType, ZString transactionNum, bool arShareSequentialInvoiceTransactionNumbers, bool apShareSequentialInvoiceTransactionNumbers, ZString internalRefefernce)
		{
			// Performance note: The majority of the CPU time in this function is consumed by ReadOnlyCodeDescriptionPairList.GetDescriptionFromCode and ReadOnlyCodeDescriptionPairList.ContainsCode
			//                   If overall performance is too bad, then consider replacing ReadOnlyCodeDescriptionPairList with Dictionary and HashSet.
			string journalCode, journalLib, ecritureNum;

			var isAR = ledger == LedgerTypesList.Codes.AccountsReceivable;
			var isAP = ledger == LedgerTypesList.Codes.AccountsPayable;
			var isSpecialTransactionType = SpecialTransactionTypes.ContainsCode(transactionType);
			var isContra = transactionType == nameof(TransactionType.CTR);

			// get description for ledger types and transaction types
			var ledgerDescription = LedgerTypes.GetDescriptionFromCode(ledger);
			if ((isAR && arShareSequentialInvoiceTransactionNumbers && isSpecialTransactionType)
				|| (isAP && apShareSequentialInvoiceTransactionNumbers && isSpecialTransactionType)
				)
			{
				journalCode = $"{ledger}{SpecialJournalCode}";
				journalLib = $"{ledgerDescription} {SpecialJournalLib}";
			}
			else
			{
				// omit ledger code/description for AR/AP contras
				journalCode = $"{((isAP || isAR) && isContra ? ZString.Empty : ledger)}{transactionType}";
				journalLib = $"{((isAP || isAR) && isContra ? string.Empty : ledgerDescription + ' ')}{TransactionTypes[transactionType]?.GetMultilingualDescription()}";
			}

			ecritureNum = $"{journalCode}{((isAP && isSpecialTransactionType) ? internalRefefernce : transactionNum)}";

			return (journalCode, journalLib, ecritureNum);
		}

		string FormatStringValueForCsv(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			var resultString = value;

			resultString = RemoveLineBreaks(resultString);
			resultString = WrapStringInQuotes(resultString);

			return resultString;
		}

		string RemoveLineBreaks(string value)
		{
			var replacement = " ";
			var resultString = value.Replace("\r\n", replacement).Replace("\r", replacement).Replace("\n", replacement);

			return resultString;
		}

		string WrapStringInQuotes(string value)
		{
			var resultString = value.Replace("\"", "\'");

			return $"\"{resultString}\"";
		}

		string ZDateTimeToString(ZDateTime dateTime) => dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

		void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				try
				{
					CloseStream();
					tempDir?.Dispose();
				}
				finally
				{
					streamWriter = null;
					tempDir = null;
				}
			}

			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~FECFileWriter()
		{
			Dispose(false);
		}
	}
}
