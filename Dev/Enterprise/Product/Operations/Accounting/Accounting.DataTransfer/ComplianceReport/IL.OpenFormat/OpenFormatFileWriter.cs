using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.DocumentEngine.PdfBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat
{
	public class OpenFormatFileWriter : IProgressFormSupportable
	{
		public OpenFormatFileWriter(AccComplianceReport report)
		{
			Report = report;
			SequenceNumber = 0;
		}

		readonly AccComplianceReport Report;
		int SequenceNumber;
		OpenFormatAdditionalDataCollector AdditionalData => additionalData ?? (additionalData = (OpenFormatAdditionalDataCollector)ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(Report, ComplianceReportDataCollectionMode.OpenFormatSimplified));
		OpenFormatAdditionalDataCollector additionalData;

		public OpenFormatInfo WriteBkmvdataToStream(Stream stream)
		{
			UpdateProgressStatus(Res.GetString("ab74e510-c405-4c88-b567-3092ddcfe67c", "Exporting open format files..."), 1, 4, false);

			var openFormatInfo = new OpenFormatInfo();
			using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, true))
			{
				writer.WriteLine(BuildA100Data());

				var transactionCounter = 0;
				foreach (var reportLinesGroupedByTransaction in Report.ReportLines.Cast<AccComplianceReportLine>().OrderBy(x => x.ACL_ReportSequence).GroupBy(x => x.AH_PK))
				{
					transactionCounter++;
					var firstTransactionLine = reportLinesGroupedByTransaction.First();
					var transactionHeader = AdditionalData.GetTransactionHeader(firstTransactionLine.AH_PK) as TransactionHeaderDetailsOFS;
					writer.WriteLine(BuildC100Data(transactionHeader, firstTransactionLine, transactionCounter));
					openFormatInfo.C100Counter++;

					if (transactionHeader.Ledger == LedgerTypes.AccountsReceivable && transactionHeader.TransactionType == TransactionTypes.Invoice)
					{
						openFormatInfo.ARInvoiceCounter++;
						openFormatInfo.ARInvoiceSum += transactionHeader.InvoiceAmount;
						openFormatInfo.ARGSTSum += transactionHeader.GSTAmount;
					}
					else if (transactionHeader.Ledger == LedgerTypes.AccountsPayable && transactionHeader.TransactionType == TransactionTypes.Invoice)
					{
						openFormatInfo.APInvoiceCounter++;
						openFormatInfo.APInvoiceSum += transactionHeader.InvoiceAmount;
						openFormatInfo.APGSTSum += transactionHeader.GSTAmount;
					}
					if (transactionHeader.Ledger == LedgerTypes.AccountsReceivable && transactionHeader.TransactionType == TransactionTypes.Payment)
					{
						openFormatInfo.ARPaymentCounter++;
						openFormatInfo.ARPaymentSum += transactionHeader.InvoiceAmount;
						openFormatInfo.ARPaymentGSTSum += transactionHeader.GSTAmount;
					}

					foreach (var line in reportLinesGroupedByTransaction)
					{
						writer.WriteLine(BuildD110Data(transactionHeader, line, transactionCounter));
						openFormatInfo.D110Counter++;
					}
				}

				writer.WriteLine(BuildZ900Data());
			}

			UpdateProgressStatus(Res.GetString("9a05733f-b053-4660-8e26-45d112aa46d7", "Open format files export completed."), 2, 4, true);

			return openFormatInfo;
		}

		public void WriteInidataToStream(Stream stream, OpenFormatInfo info, string outputPath, ZDateTime reportGenDateTime)
		{
			UpdateProgressStatus(Res.GetString("7e27db81-83cd-40cb-b230-c6b10beb8dc6", "Exporting open format content files..."), 3, 4, false);

			using (var writer = new StreamWriter(stream, Encoding.ASCII, 4096, true))
			{
				writer.WriteLine(BuildIniHeader(reportGenDateTime.ToDateTime(), info.C100Counter + info.D110Counter, outputPath));
				writer.WriteLine(BuildSummary("C100", info.C100Counter));
				writer.WriteLine(BuildSummary("D110", info.D110Counter));
			}

			UpdateProgressStatus(Res.GetString("0ae0d9a4-a815-4725-8232-aa8b3f9ede67", "Open format content files export completed."), 4, 4, true);
		}

		public void WritePdfFileToStream(Stream stream, OpenFormatInfo info, ZDateTime reportGenDateTime, string outputPath)
		{
			var registry = AccountingMasterFilesRegistry.Instance;

			var pdfDocument = new OpenFormatPdfDocument(stream, CreatePdfBuilder());

			pdfDocument.DateFrom = Report.ACR_DateFrom;
			pdfDocument.DateTo = Report.ACR_DateTo;
			pdfDocument.CompanyVATNumber = GetOrgProxyVatNumber();
			pdfDocument.CompanyName = Report.Company.OrgProxy.OH_FullName;
			pdfDocument.FileSavePath = outputPath;
			pdfDocument.AccountingSoftWareLicenseNumber = registry.ILAccountingSoftwareNumber.Value;
			pdfDocument.AccountingSoftWareName = softwareCompanyName;
			pdfDocument.ReportInfo = info;

			pdfDocument.Save(reportGenDateTime);
		}

		public ZString GetReportDataErrorMessage()
		{
			var vatNumber = GetOrgProxyVatNumber();
			if (vatNumber.IsEmpty || vatNumber.Length != 9)
			{
				return Res.GetString("fcc4127c-b3dc-4c6a-afb7-59739919484c", "Israel Open Format VAT number must be filled with 9 digits. Current value is: {0}.", vatNumber);
			}

			var uniqueID = GetUniqueIdentifier();
			if (uniqueID.IsEmpty || uniqueID.Length != 15)
			{
				return Res.GetString("bd607f4b-bc40-4100-91b8-9b7ac1dd5751", "Israel Open Format unique identifier must be filled with 15 digits. Current value is: {0}.", uniqueID);
			}

			var taxFileCode = GetTaxFileCode();
			if (taxFileCode.IsEmpty || taxFileCode.Length != 9)
			{
				return Res.GetString("44cc25aa-e848-4254-a8b6-7f9f8ccac27d", "Israel Tax File Code must be filled with 9 digits. Current value is: {0}.", taxFileCode);
			}

			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No translation needed")]
		string BuildC100Data(TransactionHeaderDetailsOFS transactionHeader, AccComplianceReportLine firstTransactionLine, int transactionCounter)
		{
			var result = new StringBuilder(248);
			result.Append("C100"
				+ GetNextSequenceNumber()
				+ GetOrgProxyVatNumber()
				+ GetDocumentType(transactionHeader)
				+ firstTransactionLine.AH_TransactionNum.Left(20).PadLeft(20)
				+ transactionHeader.CreateTime.ToString(dateFormat, Culture.Invariant)
				+ transactionHeader.CreateTime.ToString(timeFormat, Culture.Invariant)
				+ firstTransactionLine.OH_FullName.Left(50).PadLeft(50));

			var orgAddress = AdditionalData.OrgAddresses.TryGetValue(firstTransactionLine.OH_Code, out var orgDetails);
			if (orgDetails != null)
			{
				result.Append(FormatOrgAddressDetail(orgDetails.Address1, 50)
					+ new string(' ', 10)
					+ FormatOrgAddressDetail(orgDetails.City, 30)
					+ FormatOrgAddressDetail(orgDetails.Postcode, 8)
					+ FormatOrgAddressDetail(orgDetails.Country.Name, 30)
					+ FormatOrgAddressDetail(orgDetails.Country.Code, 2)
					+ FormatOrgAddressDetail(orgDetails.Phone, 15));
			}
			else
			{
				result.Append(new string(' ', 138));
			}

			var isForeignCurrency = transactionHeader.TransactionCurrency != Core.Constants.CurrencyCodes.Israel;
			result.Append(transactionHeader.VatNumber.Left(9).PadLeft(9)
				+ transactionHeader.DueDate.ToString(dateFormat, Culture.Invariant)
				+ (isForeignCurrency ? ApplyMoneyFormat(transactionHeader.OSTotal, transactionHeader.Ledger) : new string(' ', 15))
				+ (isForeignCurrency ? transactionHeader.TransactionCurrency : new string(' ', 3))
				+ ApplyMoneyFormat(transactionHeader.InvoiceAmount, transactionHeader.Ledger)
				+ ApplyMoneyFormat(decimal.Zero)
				+ ApplyMoneyFormat(transactionHeader.InvoiceAmount, transactionHeader.Ledger)
				+ ApplyMoneyFormat(transactionHeader.GSTAmount, transactionHeader.Ledger)
				+ ApplyMoneyFormat(transactionHeader.OutstandingAmount, transactionHeader.Ledger)
				+ "+00000000000"
				+ firstTransactionLine.OH_Code.Left(15).PadLeft(15)
				+ new string(' ', 10)
				+ (transactionHeader.IsCancelled ? "1" : " ")
				+ GetDocumentDate(transactionHeader)
				+ transactionHeader.BranchCode.Left(7).PadLeft(7)
				+ transactionHeader.CreateUserName.Left(9).PadLeft(9)
				+ transactionCounter.ToString(CultureInfo.InvariantCulture).PadLeft(7, '0')
				+ new string(' ', 13));

			return result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No translation needed")]
		string BuildD110Data(TransactionHeaderDetailsOFS transactionHeader, AccComplianceReportLine transactionLine, int transactionCounter)
		{
			var lineDetails = AdditionalData.GetTransactionLineData(transactionLine.ACL_ReportSequence);

			var result = new StringBuilder(103);
			result.Append("D110"
				+ GetNextSequenceNumber()
				+ GetOrgProxyVatNumber()
				+ GetDocumentType(transactionHeader)
				+ transactionLine.AH_TransactionNum.Left(20).PadLeft(20)
				+ (lineDetails != null ? ((ZString)lineDetails.LineSequence.ToString()).Right(4).PadLeft(4, '0') : new string('0', 4)) //TODO: warning LineSequence can be > 9999
				+ baseDocumentType
				+ new string(' ', 20)
				+ GetDealType(lineDetails)
				+ (lineDetails?.ChargeCode.Left(20).PadLeft(20) ?? new string(' ', 20))
				+ (lineDetails?.Description.Left(30).PadLeft(30) ?? new string(' ', 30))
				+ new string(' ', 50)
				+ new string(' ', 30)
				+ "unit".PadLeft(20) //it is supposed to be the hebrew translation of 'unit'
				+ "00000000000001");

			if (lineDetails != null)
			{
				var taxRate = (ZDecimal)lineDetails.TaxRateNumerator / lineDetails.TaxExtraRateDenominator;

				result.Append(ApplyMoneyFormat(lineDetails.LineAmount, transactionHeader.Ledger)
					+ "+00000000000000"
					+ ApplyMoneyFormat(lineDetails.LineAmount, transactionHeader.Ledger)
					+ taxRate.ToString("00.00").Replace(".", "")
					+ lineDetails.BranchCode.Left(7).PadLeft(7));
			}
			else
			{
				result.Append(new string(' ', 53));
			}

			result.Append(GetDocumentDate(transactionHeader)
				+ transactionCounter.ToString(CultureInfo.InvariantCulture).PadLeft(7, '0')
				+ new string(' ', 7)
				+ new string(' ', 21));

			return result.ToString();
		}

		string BuildA100Data()
			=> "A100"
				+ GetNextSequenceNumber()
				+ GetOrgProxyVatNumber()
				+ GetUniqueIdentifier()
				+ systemConstant
				+ new string(' ', 50);

		string BuildZ900Data()
			=> "Z900"
				+ GetNextSequenceNumber()
				+ GetOrgProxyVatNumber()
				+ GetUniqueIdentifier()
				+ systemConstant
				+ SequenceNumber.ToString(CultureInfo.InvariantCulture).PadLeft(15, '0')
				+ new string(' ', 50);

		string GetDealType(TransactionLineDetailsOFS lineDetails) => lineDetails != null && AdditionalData.ChargeCodes.TryGetValue(lineDetails.ChargeCode, out var chargeCodeDetails)
			? chargeCodeDetails.GoodsServiceType == GoodServiceTypes.Codes.GDS ? "2" : "1"
			: "1";

		string FormatOrgAddressDetail(ZString? orgAddressDetail, int maxSize)
			=> orgAddressDetail.HasValue ? orgAddressDetail.Value.Left(maxSize).PadLeft(maxSize) : new string(' ', maxSize);

		string GetNextSequenceNumber() => (++SequenceNumber).ToString(CultureInfo.InvariantCulture).PadLeft(9, '0');

		ZString GetOrgProxyVatNumber() => Report.Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty;

		ZString GetTaxFileCode() => Report.Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(MasterFiles.Business.OrgCusCode.CodeTypes.TaxFileCode, Core.Constants.CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty;

		ZString GetUniqueIdentifier() => Report.ACR_ReferenceNumber;

		string ApplyMoneyFormat(ZDecimal amount, string ledger = "")
		{
			amount = ledger == LedgerTypes.AccountsPayable ? -amount : amount;
			return amount.ToString("+000000000000.00;-000000000000.00").Replace(".", "");
		}

		string GetDocumentDate(TransactionHeaderDetailsOFS transactionHeader)
		{
			var date = transactionHeader.InvoiceDate.IsValid ? transactionHeader.InvoiceDate : transactionHeader.PostDate;
			return date.ToString(dateFormat, Culture.Invariant);
		}

		string GetDocumentType(TransactionHeaderDetailsOFS transactionHeader)
		{
			var result = "000";

			if (transactionHeader.Ledger == LedgerTypes.AccountsReceivable)
			{
				if (transactionHeader.TransactionType == TransactionTypes.Invoice)
				{
					switch (transactionHeader.TransactionCategory)
					{
						case InvoiceTypesList.Codes.FinalInvoice:
						case InvoiceTypesList.Codes.DisbursementInvoice:
						case InvoiceTypesList.Codes.ForeignCurrencyInvoice:
						case InvoiceTypesList.Codes.DisbursementInForeignCurrency:
						case InvoiceTypesList.Codes.FreightInvoice:
						case InvoiceTypesList.Codes.InvoicePerTaxCode:
						case InvoiceTypesList.Codes.DestinationChargesInvoice:
							result = "305";
							break;
						case InvoiceTypesList.Codes.FinalInvoice_Batching:
						case InvoiceTypesList.Codes.DisbursementInvoice_Batching:
						case InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching:
						case InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching:
						case InvoiceTypesList.Codes.FreightInvoice_Batching:
						case InvoiceTypesList.Codes.InvoicePerTaxCode_Batching:
						case InvoiceTypesList.Codes.DestinationChargesInvoice_Batching:
						case InvoiceTypesList.Codes.SelfBillingInvoice:
						case InvoiceTypesList.Codes.SelfBillingInvoice_Batching:
							result = "310";
							break;
						default:
							throw new ArgumentException(FormattableString.Invariant($"Invalid transaction category: {transactionHeader.TransactionCategory}."));
					}
				}
				else if (transactionHeader.TransactionType == TransactionTypes.AdjustmentNote)
				{
					result = "305";
				}
				else if (transactionHeader.TransactionType == TransactionTypes.CreditNote)
				{
					result = "330";
				}
			}
			else if (transactionHeader.Ledger == LedgerTypes.AccountsPayable)
			{
				if (transactionHeader.TransactionType == TransactionTypes.Invoice
					|| transactionHeader.TransactionType == TransactionTypes.AdjustmentNote)
				{
					result = "700";
				}
				else if (transactionHeader.TransactionType == TransactionTypes.CreditNote)
				{
					result = "710";
				}
			}

			return result;
		}

		string BuildIniHeader(DateTime processDate, int totalSectionCounter, ZString outputPath)
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var yearLastTwoDigits = Report.ACR_DateFrom.Year.ToString().Substring(2);

			var result = "A000"
				+ new string(' ', 5)
				+ totalSectionCounter.ToString(CultureInfo.InvariantCulture).PadLeft(15, '0')
				+ GetOrgProxyVatNumber()
				+ GetUniqueIdentifier()
				+ systemConstant
				+ registry.ILAccountingSoftwareNumber.Value.PadLeft(8, '0')
				+ softwareName.PadLeft(20)
				+ registry.ILAccountingSoftwareVersion.Value.PadLeft(20)
				+ softwareCompanyVAT
				+ softwareCompanyName.PadLeft(20)
				+ softwareType
				+ outputPath.Left(50).PadLeft(50)
				+ softwareType
				+ balanceType
				+ GetOrgProxyVatNumber()
				+ GetTaxFileCode()
				+ new string(' ', 10)
				+ Report.Company.OrgProxy.OH_FullName.Left(50).PadLeft(50)
				+ FormatOrgAddressDetail(Report.Company.OrgProxy.Addresses[0].Address1, 50)
				+ new string(' ', 10)
				+ FormatOrgAddressDetail(Report.Company.OrgProxy.Addresses[0].City, 30)
				+ FormatOrgAddressDetail(Report.Company.OrgProxy.Addresses[0].Postcode, 8)
				+ "0000"
				+ Report.ACR_DateFrom.ToString(dateFormat, Culture.Invariant)
				+ Report.ACR_DateTo.ToString(dateFormat, Culture.Invariant)
				+ processDate.ToString(dateFormat, Culture.Invariant)
				+ processDate.ToString(timeFormat, Culture.Invariant)
				+ languageCode
				+ charsetCode
				+ new string(' ', 20)
				+ Report.Company.GC_RX_NKLocalCurrency
				+ branchIndication
				+ new string(' ', 46);

			return result;
		}

		string BuildSummary(string code, int counter)
		{
			return code + counter.ToString(CultureInfo.InvariantCulture).PadLeft(15, '0');
		}

		IPdfBuilder CreatePdfBuilder()
		{
#if DEBUG
			return PDFProvider_TestOnly ?? new PdfBuilder();
#else
			return new PdfBuilder();
#endif
		}

#if DEBUG
		public IPdfBuilder PDFProvider_TestOnly { get; set; }
#endif

		const string systemConstant = "&OF1.31&";
		const string baseDocumentType = "000";
		const string dateFormat = "yyyyMMdd";
		const string timeFormat = "HHmm";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is hard-coded")]
		const string softwareName = "CargoWise Accounting";
		const string softwareCompanyVAT = "560038416";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is hard-coded")]
		const string softwareCompanyName = "WiseTech Global LTD";
		const string softwareType = "2";
		const string balanceType = "1";
		const string languageCode = "2";
		const string charsetCode = "1";
		const string branchIndication = "1";

		#region IProgressFormSupportable

		public string CurrentStatusText { get; private set; }
		public int CompletedItems { get; private set; }
		public int TotaItemsToComplete { get; private set; }

		public string Log => string.Empty;

		public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;

		void UpdateProgressStatus(string statusText, int processItem, int totalItemToProcess, bool isProcessCompleted)
		{
			CurrentStatusText = statusText;
			CompletedItems = processItem;
			TotaItemsToComplete = totalItemToProcess;

			RaiseProgressUpdateEvent?.Invoke(this, isProcessCompleted);
		}

		#endregion
	}
}
