using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ComplianceReport.IDEA
{
	public enum AuditFileTypes
	{
		AccountMovements = 1,
		Accounts = 2,
		Company = 3,
		Organisations = 4,
		BalanceSheet = 5,
		Index = 6,
		TypeDefinition = 7
	}

	public class IDEATaxAuditExport : NonPersistentBusinessObject
	{
		#region Constructor

		public IDEATaxAuditExport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "AccComplianceReport";
			public const string PK = "ACR_PK";
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return AccComplianceReportSchema.PK; }
		}

		#endregion

		#region Document Handling Variables

		public const int MaxStringBuilderLength = 16777216;  // prevent the StringBuilder from allocating more than 32 MB memory (2 bytes per char)
		public const string CSVDelimiter = ";";
		public const string CSVQuoteChar = "\"";
		public const string CSVLineTerminator = "\r\n";

		ZString DataReaderGetZString(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZString.Empty : (ZString)(string)record[columnName];
		ZInt DataReaderGetZInt(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZInt.Zero : (ZInt)(int)record[columnName];
		ZDecimal DataReaderGetZDecimal(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZDecimal.Zero : (ZDecimal)(decimal)record[columnName];
		ZDateTime DataReaderGetZDateTime(IDataRecord record, string columnName) => record[columnName] == DBNull.Value ? ZDateTime.Empty : (DateTime)record[columnName];

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = ComplianceReport.DocManagerInfo());
		DocManagerInfo docManagerInfo;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File description")]
		public const string FileDescription = "IDEA Tax Audit";

		#endregion

		const int MonthsPerStep = 1;
		const int DaysPerStep = 0;
		const int PeriodsPerYear = 12;

		AccComplianceReport ComplianceReport;
		ILogger ServiceLogger;
		IDEADataProvider DataProvider;
		IDEAeDocs EDocs;
		IDEAZipFile ZipCreator;
		IDEATempFiles TempFiles;
		IDEAPersistentData PersistentData;
		GLAccountToLocalAccountMapping AccountMappingHelper;
		readonly IEnumerable<string> ExcludedColumnsFromQuoting = new string[] { "ErfassungsDatumZeit", "PKUsedForGermanAccountMapping" };

		bool payablesShareSequentialInvoiceTransactionNumbers;
		bool receivablesShareSequentialInvoiceTransactionNumbers;
		readonly ControlAccountAndReportSubCodeMapping controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();

		#region Main Function

		public (bool isAllDataExported, string statusMessage) ExportData(AccComplianceReport report, ILogger serviceLogger, int maxChunkSize = MaxStringBuilderLength)
		{
			Init(report, serviceLogger);

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var reportStartDate = ComplianceReport.ACR_DateFrom;
			var reportEndDatePlusOne = ComplianceReport.ACR_DateTo.AddDays(1);
			var startDateThisStep = ComplianceReport.NextProcessingStepFromDate;
			ZDate startDateNextStep;
			var statusMessage = "";

			ServiceLogger.Log(LogType.Debug, "Export started for " + ComplianceReport.ACR_Description);
			TempFiles = report.Factory.SubscribeForDispose(new IDEATempFiles(ServiceLogger));
			var controlAccountsList = controlAccountAndReportSubCodeMapping.ControlAccountsNumbers;
			var balanceAccounts = DataProvider.GetBalanceAccountsWithNonZeroBalance();
			PersistentData.AllAccounts = PersistentData.AllAccounts.Concat(controlAccountsList).Concat(balanceAccounts).ToHashSet();
			AddBalanceAndControlAccountsToAccountList(balanceAccounts, controlAccountsList.ToHashSet());

			startDateNextStep = CreateTransactionFileForStep(startDateThisStep, MonthsPerStep, DaysPerStep, maxChunkSize);
			if (startDateNextStep >= reportEndDatePlusOne)
			{
				var noOfAccounts = CreateFileFromSQL("sachkontenstamm.csv", reportStartDate, reportEndDatePlusOne, AuditFileTypes.Accounts, maxChunkSize);
				CreateFileFromSQL("mandantendaten.csv", reportStartDate, reportEndDatePlusOne, AuditFileTypes.Company, maxChunkSize);
				CreateFileFromSQL("debitorenkreditorenstammdaten.csv", reportStartDate, reportEndDatePlusOne, AuditFileTypes.Organisations, maxChunkSize);
				CreateAccountBalanceSheet("mvz.csv");
				CreateFileFromEmbeddedResource("index.xml", AuditFileTypes.Index);
				CreateFileFromEmbeddedResource("gdpdu-01-08-2002.dtd", AuditFileTypes.TypeDefinition);

				statusMessage = CheckNoOfUnmappedAccounts(serviceLogger, statusMessage, noOfAccounts);

				PersistentData.Delete();
			}
			else
			{
				PersistentData.Store();
			}

			ZipCreator.CreateZipFileForEDocs(TempFiles.FolderName, EDocs);

			var elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000;
			stopwatch.Stop();
			ServiceLogger.Log(LogType.Debug,
				Invariant(
					$"Export finished for {ComplianceReport.ACR_Description} from {startDateThisStep:yyyy-MM-dd} to {startDateNextStep.AddDays(-1):yyyy-MM-dd} and took {elapsedSeconds} seconds to generate"));

			if (startDateNextStep < reportEndDatePlusOne)
			{
				ComplianceReport.NextProcessingStepFromDate = startDateNextStep;
			}

			return (startDateNextStep >= reportEndDatePlusOne, statusMessage);
		}

		string CheckNoOfUnmappedAccounts(ILogger serviceLogger, string statusMessage, int noOfAccounts)
		{
			ComplianceReport.DeleteNotes(Res.GetString("49FFE109-AEDB-40B0-8E74-44F48FA67024", "Missing Local Account Mapping"));
			var noOfUnmappedAccounts = AccountMappingHelper.UnmappedLocalAccounts.Count;
			if (noOfUnmappedAccounts > 0)
			{
				var notesText = new ZStringBuilder();

				// - if "No accounts have been mapped":
				if (noOfAccounts == noOfUnmappedAccounts)
				{
					notesText.Append((NoResString)@"No GL accounts have been mapped to German (account-numbers and -descriptions) using the GL Multi-Language Mapping.
This means that only the standard chart of accounts (account-numbers and -descriptions) will be exported in this IDEA export.");
				}
				// - if "Accounts have been mapped partially"
				else if (noOfAccounts > noOfUnmappedAccounts)
				{
					notesText.Append((NoResString)@"Part of the GL accounts have been mapped to German (account-numbers and -descriptions) using the GL Multi-Language Mapping. 
This means that the standard chart of accounts (account-numbers and -descriptions) will be exported in this IDEA export along with the German mapping of those GL accounts that have been mapped.
It should be considered to map the un-mapped accounts which are:");
				}
				ComplianceReport.ACR_StatusMessage = "";
				AccountMappingHelper.ValidateMappingAndUpdateNotesAndStatus(serviceLogger, notesText, setErrorStatus: false, addAccountNumbers: noOfAccounts != noOfUnmappedAccounts);
				statusMessage = Res.GetString("f8d7f94f-1a1e-4aa0-95a2-2881de6e6510", "Account mapping is incomplete. Please check the Notes tab for details.");
			}

			return statusMessage;
		}

		void Init(AccComplianceReport report, ILogger serviceLogger)
		{
			ComplianceReport = report;
			ServiceLogger = serviceLogger;
			PersistentData = IDEAPersistentData.Load(ComplianceReport);
			DataProvider = new IDEADataProvider(Factory, ComplianceReport, ServiceLogger);
			EDocs = new IDEAeDocs(ComplianceReport);
			ZipCreator = new IDEAZipFile(ComplianceReport, ServiceLogger);
			AccountMappingHelper = new GLAccountToLocalAccountMapping(report, Core.SharedConstants.Languages.German, PersistentData.AllUnmappedAccountPKs);

			payablesShareSequentialInvoiceTransactionNumbers = AccountingConfigurationRegistry.Instance
				.ShareSequentialInvoiceReferenceNumbers
				.GetValueWithoutFallback(ComplianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).Value;
			receivablesShareSequentialInvoiceTransactionNumbers = AccountingConfigurationRegistry.Instance
				.ShareSequentialInvoiceTransactionNumbers
				.GetValueWithoutFallback(ComplianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).Value;
		}

		#endregion

		#region Output Generation

		void CreateFileFromEmbeddedResource(string filename, AuditFileTypes fileType)
		{
			var filecontent = GetEmbeddedResource(filename);

			if (fileType == AuditFileTypes.Index)
			{
				filecontent = UpdateIndexWithDynamicContentMVZ(filecontent);
				filecontent = UpdateIndexWithDynamicContentTransactions(filecontent);
			}

			TempFiles.AddFile(filename, filecontent, IDEAeDocs.MaxEdocSize, true, ZipCreator, EDocs);
		}

		string GetEmbeddedResource(string filename)
		{
			var filecontent = string.Empty;

			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.Business.ComplianceReport.IDEA.Data." + filename))
			using (StreamReader sr = new StreamReader(stream))
			{
				filecontent = sr.ReadToEnd();
			}

			return filecontent;
		}

		ZDate CreateTransactionFileForStep(ZDate startDate, int months, int days, int maxChunkSize)
		{
			var endDate = startDate.AddMonths(months).AddDays(days - 1);
			var fileName = $"kontobuchungen-{startDate.Year:D4}{startDate.Month:D2}{startDate.Day:D2}-{endDate.Year:D4}{endDate.Month:D2}{endDate.Day:D2}.csv";
			CreateFileFromSQL(fileName, startDate, endDate, AuditFileTypes.AccountMovements, maxChunkSize);
			PersistentData.AllCSVFiles.Add(fileName);
			return endDate.AddDays(1);
		}

		int CreateFileFromSQL(string filename, ZDate startDateInclusive, ZDate endDateExclusive, AuditFileTypes fileType, int maxChunkSize)
		{
			DynamicBusinessObject[] rawData;
			ServiceLogger.Log(LogType.Debug, "Retrieving data for file " + filename);
			var csvdata = "";
			var result = 0;

			switch (fileType)
			{
				case AuditFileTypes.AccountMovements:
					csvdata = DataProvider.RetrieveTransactionDataAsCSV(this, startDateInclusive, endDateExclusive, filename, TempFiles, ZipCreator, EDocs, maxChunkSize);
					break;
				case AuditFileTypes.Accounts:
					rawData = DataProvider.RetrieveAccounts(PersistentData.AllAccounts).ToArray();
					result = rawData.Length;
					csvdata = DynamicBusinessObjectArrayToCSV(rawData);
					break;
				case AuditFileTypes.Organisations:
					rawData = DataProvider.RetrieveOrganizations(PersistentData.AllOrganisations);
					result = rawData.Length;
					csvdata = DynamicBusinessObjectArrayToCSV(rawData);
					break;
				case AuditFileTypes.Company:
					rawData = DataProvider.RetrieveCompany().ToArray();
					result = rawData.Length;
					csvdata = DynamicBusinessObjectArrayToCSV(rawData);
					break;
			}
			TempFiles.AddFile(filename, csvdata, IDEAeDocs.MaxEdocSize, true, ZipCreator, EDocs);
			return result;
		}

		static internal string FormatAmount(string amount, int requiredDigits)
		{
			var decimalPointPosition = amount.IndexOf('.');
			if (decimalPointPosition < 0)
			{
				return amount + "." + new string('0', requiredDigits);
			}
			var existingDigits = amount.Length - decimalPointPosition - 1;
			if (existingDigits < requiredDigits)
			{
				return amount + new string('0', requiredDigits - existingDigits);
			}
			return amount.Substring(0, decimalPointPosition + requiredDigits + 1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Timezone string")]
		string FormatDataReaderValue(IDataRecord record, int columnNumber)
		{
			if (record[columnNumber] == DBNull.Value)
			{
				return "";
			}
			var dataType = record[columnNumber].GetType();
			if (dataType.Equals(typeof(DateTime)))
			{
				var dateTimeValue = record.GetDateTime(columnNumber);
				if (record.GetName(columnNumber) == "ErfassungsDatumZeit")
				{
					dateTimeValue = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
					return string.Join(";", dateTimeValue.ToString("dd.MM.yyyy"), dateTimeValue.ToString("HH:mm:ss"));
				}
				return dateTimeValue.ToString("dd.MM.yyyy");
			}
			return record[columnNumber].ToString();
		}

		string FormatColumnValue(DynamicBusinessObject record, string column)
		{
			if (record[column] == DBNull.Value)
			{
				return "";
			}
			var dataType = record[column].GetType();
			if (dataType.Equals(typeof(ZDateTime)))
			{
				var dateTimeValue = (ZDateTime)record[column];
				return dateTimeValue.ToString("dd.MM.yyyy");
			}
			if (dataType.Equals(typeof(ZDate)))
			{
				var dateValue = (ZDate)record[column];
				return dateValue.ToString("dd.MM.yyyy");
			}
			return record[column].ToString();
		}

		void DataReaderToCSV(IDataRecord record,
			decimal? debitOverride, decimal? creditOverride, decimal? debitAccsysOverride, decimal? creditAccsysOverride,
			string ledgerType, string transactionType, StringBuilder stringBuilderToAppend, string sequenceGroupPlaceholder)
		{
			var columnCount = record.FieldCount;
			for (var columnNumber = 0; columnNumber < columnCount; columnNumber++)
			{
				var columnName = record.GetName(columnNumber);
				if (columnName != "ACQ_ReportSubCode")  // this column must not be exported
				{
					var data = new ZString(FormatDataReaderValue(record, columnNumber));
					switch (columnName)
					{
						#region SuppressResourceStringsCheckRegion

						case "Soll_Position":
							data = debitOverride == null ? FormatAmount(data, 2) : debitOverride.Value.ToString("F2", CultureInfo.InvariantCulture);
							break;
						case "Haben_Position":
							data = creditOverride == null ? FormatAmount(data, 2) : creditOverride.Value.ToString("F2", CultureInfo.InvariantCulture);
							break;
						case "Soll_Fibu":
							data = debitAccsysOverride == null ? FormatAmount(data, 2) : debitAccsysOverride.Value.ToString("F2", CultureInfo.InvariantCulture);
							break;
						case "Haben_Fibu":
							data = creditAccsysOverride == null ? FormatAmount(data, 2) : creditAccsysOverride.Value.ToString("F2", CultureInfo.InvariantCulture);
							break;
						case "Steuer_Fibu":
						case "Steuer_Position":
							data = !data.IsEmpty ? FormatAmount(data, 2) : "0.00";
							break;
						case "ErweiterteBelegnummer":
							// This logic is required to support Registry keys: "Share Sequential Invoice Reference Numbers" and "Share Sequential Invoice Transaction Numbers".
							data = ReplaceSequenceGroupPlaceholder(data, ledgerType, transactionType, sequenceGroupPlaceholder);
							break;

						#endregion
					}

					data = RemoveLineBreaks(data);
					data = data.Replace(CSVQuoteChar, CSVQuoteChar + CSVQuoteChar);
					if ((data.IndexOf(CSVQuoteChar) >= 0 || data.IndexOf(CSVDelimiter) >= 0) && !ExcludedColumnsFromQuoting.Any(x => x == columnName))
					{
						data = CSVQuoteChar + data + CSVQuoteChar;
					}
					stringBuilderToAppend.Append(data);
					stringBuilderToAppend.Append(CSVDelimiter);
				}
			}

			stringBuilderToAppend.Remove(stringBuilderToAppend.Length - 1, 1);
			stringBuilderToAppend.Append(CSVLineTerminator);
		}

		const string SharedSequenceGroup = "INVCRDADJ";
		string ReplaceSequenceGroupPlaceholder(string value, string ledgerType, string transactionType, string sequenceGroupPlaceholder)
		{
			var sequenceGroup = string.Empty;

			switch (ledgerType)
			{
				case LedgerTypes.AccountsPayable:
				{
					sequenceGroup = payablesShareSequentialInvoiceTransactionNumbers
						? SharedSequenceGroup
						: transactionType;

					break;
				}
				case LedgerTypes.AccountsReceivable:
				{
					sequenceGroup = receivablesShareSequentialInvoiceTransactionNumbers
						? SharedSequenceGroup
						: transactionType;

					break;
				}
			}

			return value.Replace(sequenceGroupPlaceholder, sequenceGroup);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Keyword")]
		internal void CreateAccountMovementsAsCSV(IDataRecord rawData, StringBuilder stringBuilderToAppend, string sequenceGroupPlaceholder)
		{
			#region SuppressResourceStringsCheckRegion

			var accountNumber = DataReaderGetZString(rawData, "Kontonr");
			var organisation = DataReaderGetZString(rawData, "RechnungsStellerEmpfaenger");
			var postDate = DataReaderGetZDateTime(rawData, "Belegdatum");
			var periodYear = DataReaderGetZInt(rawData, "Periode_Jahr");
			var periodMonth = DataReaderGetZInt(rawData, "Periode_Monat");
			var debitAmountLocal = DataReaderGetZDecimal(rawData, "Soll_Position");
			var creditAmountLocal = DataReaderGetZDecimal(rawData, "Haben_Position");
			var taxAmountLocal = DataReaderGetZDecimal(rawData, "Steuer_Position");
			var debitTaxLocal = debitAmountLocal > 0 ? taxAmountLocal : 0;
			var creditTaxLocal = creditAmountLocal > 0 ? taxAmountLocal : 0;
			var debitAmountAccounting = DataReaderGetZDecimal(rawData, "Soll_Fibu");
			var creditAmountAccounting = DataReaderGetZDecimal(rawData, "Haben_Fibu");
			var taxAmountAccounting = DataReaderGetZDecimal(rawData, "Steuer_Fibu");
			var debitTaxAccounting = debitAmountAccounting > 0 ? taxAmountAccounting : 0;
			var creditTaxAccounting = creditAmountAccounting > 0 ? taxAmountAccounting : 0;
			var subCode = DataReaderGetZString(rawData, "ACQ_ReportSubCode");
			var subCodeElements = subCode.Split('*');  // sub codes look like: "*AR*INV*ARCtrl*Total"
			var accountCode = subCodeElements.Length > 3 ? subCodeElements[3].ToString() : "";
			var modifierCode = subCodeElements.Length > 4 ? subCodeElements[4].ToString() : "";
			var ledgerType = DataReaderGetZString(rawData, "Rechnungstyp");
			var transactionType = DataReaderGetZString(rawData, "Transaktionsart");
			var enhancedTransactionNumber = DataReaderGetZString(rawData, "ErweiterteBelegnummer");
			CheckAccountNumber(subCode, accountNumber);

			#endregion

			var isTax = accountCode.IndexOf("GST") >= 0;
			decimal debitOverridePositionCurrency = creditAmountLocal;
			decimal creditOverridePositionCurrency = debitAmountLocal;
			decimal debitOverrideAccountingCurrency = creditAmountAccounting;
			decimal creditOverrideAccountingCurrency = debitAmountAccounting;
			if (modifierCode == "Total")
			{
				debitOverridePositionCurrency = creditAmountLocal + creditTaxLocal;
				creditOverridePositionCurrency = debitAmountLocal + debitTaxLocal;
				debitOverrideAccountingCurrency = creditAmountAccounting + creditTaxAccounting;
				creditOverrideAccountingCurrency = debitAmountAccounting + debitTaxAccounting;
			}
			if (modifierCode == "Rev")
			{
				debitOverridePositionCurrency = creditAmountLocal;
				creditOverridePositionCurrency = debitAmountLocal;
				debitOverrideAccountingCurrency = creditAmountAccounting;
				creditOverrideAccountingCurrency = debitAmountAccounting;
			}
			if (modifierCode == "Rev-")
			{
				debitOverridePositionCurrency = debitAmountLocal;
				creditOverridePositionCurrency = creditAmountLocal;
				debitOverrideAccountingCurrency = debitAmountAccounting;
				creditOverrideAccountingCurrency = creditAmountAccounting;
			}
			if (modifierCode == "-")
			{
				debitOverridePositionCurrency = isTax ? debitTaxLocal : debitAmountLocal;
				creditOverridePositionCurrency = isTax ? creditTaxLocal : creditAmountLocal;
				debitOverrideAccountingCurrency = isTax ? debitTaxAccounting : debitAmountAccounting;
				creditOverrideAccountingCurrency = isTax ? creditTaxAccounting : creditAmountAccounting;
			}

			DataReaderToCSV(rawData,
				debitOverridePositionCurrency, creditOverridePositionCurrency, debitOverrideAccountingCurrency,
				creditOverrideAccountingCurrency, ledgerType, transactionType, stringBuilderToAppend, sequenceGroupPlaceholder);

			if (!organisation.IsEmpty)
			{
				PersistentData.AllOrganisations.Add(organisation);
			}

			if (!string.IsNullOrEmpty(accountNumber))
			{
				CreateAccountBalance(accountNumber, periodYear, periodMonth, debitOverrideAccountingCurrency, creditOverrideAccountingCurrency, postDate);
				PersistentData.AllAccounts.Add(accountNumber);
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, Invariant($"No account entered for transaction number {enhancedTransactionNumber}"));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description strings")]
		bool CheckAccountNumber(string subCode, ZString accountNumber)
		{
			var description = "Missing control Account number";
			ComplianceReport.DeleteNotes(description);

			if (string.IsNullOrEmpty(accountNumber))
			{
				var message = Invariant($"No account number found for account code '{subCode}'");

				ComplianceReport.ACR_Status = AccComplianceReport.Status.ReportError;
				ComplianceReport.ACR_StatusMessage = message;

				ServiceLogger.Log(LogType.Error, message);
				ComplianceReport.Logs.AddNew(AutoEvents.StatusUpdated, AccComplianceReport.StatusKey + ComplianceReport.ACR_Status);

				ComplianceReport.AddNote(description, message);

				ComplianceReport.Factory.Save();

				throw new InvalidOperationException(message);
			}

			return true;
		}

		string BusinessObjectToCSV(DynamicBusinessObject record)
		{
			var stringBuilder = new StringBuilder();
			var columns = record.PropertyNames;
			var columnCount = columns.Length;
			for (var columnNumber = 0; columnNumber < columnCount; columnNumber++)
			{
				var column = columns[columnNumber];
				var data = FormatColumnValue(record, column);
				if (column == "PKUsedForGermanAccountMapping")
				{
					var accountPK = new ZGuid(data);
					var (localAccountNo, localAccountName) = AccountMappingHelper.GetLocalAccount(accountPK, "");
					data = string.Join(";", localAccountNo, localAccountName);
				}

				data = RemoveLineBreaks(data);
				data = data.Replace(CSVQuoteChar, CSVQuoteChar + CSVQuoteChar);
				if ((data.IndexOf(CSVQuoteChar) >= 0 || data.IndexOf(CSVDelimiter) >= 0) && !ExcludedColumnsFromQuoting.Any(x => x == column))
				{
					data = CSVQuoteChar + data + CSVQuoteChar;
				}
				stringBuilder.Append(data);
				stringBuilder.Append(columnNumber < columnCount - 1 ? CSVDelimiter : CSVLineTerminator);
			}

			return stringBuilder.ToString();
		}

		string DynamicBusinessObjectArrayToCSV(DynamicBusinessObject[] rawData)
		{
			var stringBuilder = new StringBuilder();

			foreach (DynamicBusinessObject current in rawData)
			{
				stringBuilder.Append(BusinessObjectToCSV(current));
			}
			return stringBuilder.ToString();
		}

		string RemoveLineBreaks(string input)
		{
			return input
				.Trim('\r', '\n')
				.Replace("\r\n", " ")
				.Replace("\n\r", " ")
				.Replace("\r", " ")
				.Replace("\n", " ");
		}

		#endregion

		#region Balance Calculation

		void AddBalanceAndControlAccountsToAccountList(HashSet<string> balanceAccounts, HashSet<string> controlAccounts)
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var reportStartPeriod = periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime(), ComplianceReport.ACR_GC_Company);
			foreach (var balanceAccount in balanceAccounts.Concat(controlAccounts))
			{
				CreateAccountBalance(balanceAccount, reportStartPeriod / 100, reportStartPeriod % 100, 0.0m, 0.0m, ComplianceReport.ACR_DateFrom);
			}
		}

		void CreateAccountBalance(string accountNumber, int periodYear, int periodMonth, decimal debitAmount, decimal creditAmount, ZDateTime postDate)
		{
			var period = periodYear * PeriodsPerYear + periodMonth;
			var key = (accountNumber, period);

			if (PersistentData.AccountBalancePerPeriod.TryGetValue(key, out var balance))
			{
				balance.debit += debitAmount;
				balance.credit += creditAmount;
				PersistentData.AccountBalancePerPeriod[key] = balance;
			}
			else
			{
				balance = (debitAmount, creditAmount);
				PersistentData.AccountBalancePerPeriod.Add(key, balance);
			}

			if (PersistentData.AccountLastPostDate.TryGetValue(accountNumber, out var lastPostDate))
			{
				if (lastPostDate < postDate)
				{
					PersistentData.AccountLastPostDate[accountNumber] = postDate.Date;
				}
			}
			else
			{
				PersistentData.AccountLastPostDate[accountNumber] = postDate.Date;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message builder")]
		string UpdateIndexWithDynamicContentMVZ(string filecontent)
		{
			var (periodMinimum, periodMaximum) = PersistentData.CalculateMinMaxPeriod();
			var stringBuilder = new StringBuilder();
			for (var period = periodMinimum; period <= periodMaximum; period++)
			{
				var year = (period - 1) / PeriodsPerYear;
				var month = (period % PeriodsPerYear) == 0 ? PeriodsPerYear : period % PeriodsPerYear;

				stringBuilder.AppendLine("        <VariableColumn>");
				stringBuilder.AppendLine("          <Name>MVZ_" + year.ToString("0000") + "_" + month.ToString("00") + "_S</Name>");
				stringBuilder.AppendLine("          <Description>Monatsverkehrszahlen " + year.ToString("0000") + "/" + month.ToString("00") + " Soll</Description>");
				stringBuilder.AppendLine("          <Numeric>");
				stringBuilder.AppendLine("            <Accuracy>2</Accuracy>");
				stringBuilder.AppendLine("          </Numeric>");
				stringBuilder.AppendLine("        </VariableColumn>");

				stringBuilder.AppendLine("        <VariableColumn>");
				stringBuilder.AppendLine("          <Name>MVZ_" + year.ToString("0000") + "_" + month.ToString("00") + "_H</Name>");
				stringBuilder.AppendLine("          <Description>Monatsverkehrszahlen " + year.ToString("0000") + "/" + month.ToString("00") + " Haben</Description>");
				stringBuilder.AppendLine("          <Numeric>");
				stringBuilder.AppendLine("            <Accuracy>2</Accuracy>");
				stringBuilder.AppendLine("          </Numeric>");
				stringBuilder.AppendLine("        </VariableColumn>");
			}
			if (stringBuilder.Length >= System.Environment.NewLine.Length)
			{
				stringBuilder.Remove(stringBuilder.Length - System.Environment.NewLine.Length, System.Environment.NewLine.Length);
			}

			return filecontent.Replace("<MVZ_Periods />", stringBuilder.ToString());
		}

		string UpdateIndexWithDynamicContentTransactions(string filecontent)
		{
			// repeat structure of Kontobuchungen.csv files per file
			var stringBuilder = new StringBuilder();
			var kontobuchungenStructure = GetEmbeddedResource("index_kontobuchungen.txt");
			foreach (var fileName in PersistentData.AllCSVFiles)
			{
				stringBuilder.AppendLine((NoResString)"    <Table>");
				stringBuilder.AppendLine($"      <URL>{fileName}</URL>");
				var name = $"{fileName.Substring(0, 14)} {fileName.Substring(15, 8)} - {fileName.Substring(24, 8)}";
				stringBuilder.AppendLine($"      <Name>{name}</Name>");
				stringBuilder.AppendLine($"      <Description>{name}</Description>");
				stringBuilder.AppendLine(kontobuchungenStructure);
			}
			if (stringBuilder.Length >= System.Environment.NewLine.Length)
			{
				stringBuilder.Remove(stringBuilder.Length - System.Environment.NewLine.Length, System.Environment.NewLine.Length);
			}

			return filecontent.Replace("<kontobuchungen-files />", stringBuilder.ToString());
		}

		void SkipEmptyPeriods(StringBuilder stringBuilder, int lastUsedPeriod, int skipToPeriod)
		{
			var count = skipToPeriod - lastUsedPeriod;
			stringBuilder.Insert(stringBuilder.Length, CSVDelimiter, count * 2);
		}

		void CreateAccountBalanceSheet(string filename)
		{
			var openingBalances = DataProvider.RetrieveOpeningBalanceForAccounts(PersistentData.AccountLastPostDate.Keys,
				controlAccountAndReportSubCodeMapping.GetAccountNumber(Guid.Parse(AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value.ToString())));
			var balanceByAccount = PersistentData.AccountBalancePerPeriod.OrderBy(v => v.Key).ToList();
			var stringBuilder = new StringBuilder();
			var (periodMinimum, periodMaximum) = PersistentData.CalculateMinMaxPeriod();
			var lastAccount = string.Empty;
			var lastPeriod = periodMinimum - 1;
			var closingBalance = 0.0m;
			var totalDebit = 0.0m;
			var totalCredit = 0.0m;
			var localCurrency = ComplianceReport.Company.GC_RX_NKLocalCurrency;

			if (periodMinimum != 9999999)
			{
				foreach (var data in balanceByAccount)
				{
					var key = data.Key;
					var account = key.accountNumber;
					var period = key.period;
					if (account != lastAccount)
					{
						if (!string.IsNullOrEmpty(lastAccount))
						{
							SkipEmptyPeriods(stringBuilder, lastPeriod, periodMaximum);
							stringBuilder.Append(totalDebit.ToString("F2", CultureInfo.InvariantCulture));
							stringBuilder.Append(CSVDelimiter);
							stringBuilder.Append(totalCredit.ToString("F2", CultureInfo.InvariantCulture));
							stringBuilder.Append(CSVDelimiter);
							stringBuilder.Append(closingBalance < 0 ? Math.Abs(closingBalance).ToString("F2", CultureInfo.InvariantCulture) : "0.00");
							stringBuilder.Append(CSVDelimiter);
							stringBuilder.Append(closingBalance > 0 ? closingBalance.ToString("F2", CultureInfo.InvariantCulture) : "0.00");
							stringBuilder.Append(CSVLineTerminator);
						}
						openingBalances.TryGetValue(account, out var openingBalance);
						var lastPostDate = PersistentData.AccountLastPostDate[account];
						totalDebit = 0;
						totalCredit = 0;
						closingBalance = openingBalance;
						stringBuilder.Append(account);
						stringBuilder.Append(CSVDelimiter);
						stringBuilder.Append(localCurrency);
						stringBuilder.Append(CSVDelimiter);
						stringBuilder.Append(lastPostDate.ToString("dd.MM.yyyy"));
						stringBuilder.Append(CSVDelimiter);
						stringBuilder.Append(openingBalance < 0 ? Math.Abs(openingBalance).ToString("F2", CultureInfo.InvariantCulture) : "0.00");
						stringBuilder.Append(CSVDelimiter);
						stringBuilder.Append(openingBalance > 0 ? openingBalance.ToString("F2", CultureInfo.InvariantCulture) : "0.00");
						stringBuilder.Append(CSVDelimiter);
						lastPeriod = periodMinimum - 1;
					}
					SkipEmptyPeriods(stringBuilder, lastPeriod + 1, period);
					var debit = data.Value.debit;
					var credit = data.Value.credit;
					totalDebit += debit;
					totalCredit += credit;
					closingBalance += credit - debit;
					stringBuilder.Append(debit.ToString("F2", CultureInfo.InvariantCulture));
					stringBuilder.Append(CSVDelimiter);
					stringBuilder.Append(credit.ToString("F2", CultureInfo.InvariantCulture));
					stringBuilder.Append(CSVDelimiter);
					lastAccount = account;
					lastPeriod = period;
				}
				SkipEmptyPeriods(stringBuilder, lastPeriod, periodMaximum);
				stringBuilder.Append(totalDebit.ToString("F2", CultureInfo.InvariantCulture));
				stringBuilder.Append(CSVDelimiter);
				stringBuilder.Append(totalCredit.ToString("F2", CultureInfo.InvariantCulture));
				stringBuilder.Append(CSVDelimiter);
				stringBuilder.Append(closingBalance < 0 ? Math.Abs(closingBalance).ToString("F2", CultureInfo.InvariantCulture) : "0.00");
				stringBuilder.Append(CSVDelimiter);
				stringBuilder.Append(closingBalance > 0 ? closingBalance.ToString("F2", CultureInfo.InvariantCulture) : "0.00");
				stringBuilder.Append(CSVLineTerminator);
			}

			var filecontent = stringBuilder.ToString();
			TempFiles.AddFile(filename, filecontent, IDEAeDocs.MaxEdocSize, true, ZipCreator, EDocs);
		}

		#endregion
	}
}
