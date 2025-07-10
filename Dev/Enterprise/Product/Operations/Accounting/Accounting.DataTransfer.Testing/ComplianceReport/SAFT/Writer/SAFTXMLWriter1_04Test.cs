using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Accounting.Business.RSACryptography;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport;
using static Enterprise.Core.Constants;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	sealed class SAFTXMLWriter1_04Test : SAFTXMLWriterBaseTest
	{
		protected override ISAFTXMLWriter GetWriter(AccComplianceReport report, AccComplianceReport[] reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector)
		{
			if (report == null)
			{
				return new SAFTXMLWriter1_04(reports, reportModeAndCreditorSelector, null);
			}
			else
			{
				return new SAFTXMLWriter1_04(report, reportModeAndCreditorSelector, null);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public override void TestWriteSingleReportXmlToStream()
		{
			AssertWriteSingleReportXmlToStream(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestWriteSingleReportXmlToStream_SAFTOnlyTransactions()
		{
			AssertWriteSingleReportXmlToStream(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertWriteSingleReportXmlToStream(string reportType, string reportTablePrefix, string reportLineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = CreateWriteSAFTReportData(reportType, reportTablePrefix, reportLineGrouping);
				AssertWriteSAFTReport(report);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportWithDuplicateInvoiceLines()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = ReportTypes.SAFT;
				report.ACR_DateFrom = ZDate.Today.AddDays(-7);
				report.ACR_DateTo = ZDate.Today;
				report.Company.GC_Phone = "012345678";
				report.Company.GC_Email = "email@company.com";

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVA6"));
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				taxMessage.A9_TaxGroupCode = "GRP";

				var shipment = Creator.CreateShipment("S001001", true);
				var job = Creator.CreateJob(shipment);
				var charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 110m, creditor: Creator.Creditor1, osSellAmt: 160m, debtor: Creator.Debtor1);
				Factory.Save();

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_ComplianceSubType = "TXI";
				Assert("Has Lines", invoice.Lines.Count > 0);
				var line = invoice.Lines[0];
				line.AL_AC = Creator.CC10.PK;
				line.AL_AT = taxRate.PK;
				line.AL_A9_VATClass = taxMessage.PK;

				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT123123123");
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, invoice, 1, "*AR*INV*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 3, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 3, "*AR*INV*ARSusp*Rev");

				Factory.Save();

				var replaceTags = new Dictionary<string, string>()
				{
					{ "<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>" }
				};

				var selector = new ReportModeAndCreditorSelector(Factory);
				AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_SingleLine.xml", replaceTags);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 3, 8)]
		public override void TestWriteAnnualReportXmlToStream()
		{
			TestWriteAnnualReportXmlToStream_Portugal(new Dictionary<string, string>(), AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2024, 1, 17, 23, 42, 59)]
		public void TestWriteSingleReportXmlToStream_WithTaxMessageGroupCode_SAFT()
		{
			TestWriteSingleReportXmlToStream_WithTaxMessageGroupCode(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2024, 1, 17, 23, 42, 59)]
		public void TestWriteSingleReportXmlToStream_WithTaxMessageGroupCode_SAFTOnlyTransactions()
		{
			TestWriteSingleReportXmlToStream_WithTaxMessageGroupCode(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}
		
		public void TestWriteSingleReportXmlToStream_WithTaxMessageGroupCode(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.Company.GC_Phone = "012345678";
				report.Company.GC_Email = "email@company.com";

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_ReportType = reportType;
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVAREV13"));
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				taxMessage.A9_TaxGroupCode = "M30";
				taxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxMessage.A9_LocalMsg = "Local Tax Message";
				Factory.Save();

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0003", Creator.AUD, 1m, 200m, 0m, 200m, 0m);
				invoice.AH_OH = Creator.AALSHI.PK;
				invoice.AH_ComplianceSubType = "TXI";
				invoice.AH_InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
				invoice.AH_InvoiceTermDays = 10;
				invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

				Assert("Has Lines", invoice.Lines.Count > 0);
				var line = invoice.Lines[0];
				line.AL_AC = Creator.CC10.PK;
				line.AL_AT = taxRate.PK;
				line.AL_A9_VATClass = taxMessage.PK;
				Creator.ABIGAS.MainAddress.Postcode = "";

				Factory.Save();

				if (reportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line, 1);
					AssertEquals(1, report.ReportLines.Count);
				}
				else
				{
					Creator.CreateComplianceReportTransactionPivot(report, invoice, 1, "*AR*INV*ARCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*GSTOut*-");
					AssertEquals(5, report.ReportLines.Count);
				}

				AssertEquals("IVA - autoliquidação", AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.Value.OfType<CodeDescriptionBoolRelatedItem>().FirstOrDefault(x => x.Code == taxMessage.A9_TaxGroupCode).Description);

				var replaceTags = new Dictionary<string, string>();
				replaceTags.Add("<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>");

				var selector = new ReportModeAndCreditorSelector(Factory);
				AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_Tax_Group.xml", replaceTags);

				report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);

				AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_Tax_Group.xml", replaceTags);
			}
		}

		[TestDate(2019, 3, 8)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteAnnualReportXmlToStream_SAFTOnlyTransactions()
		{
			TestWriteAnnualReportXmlToStream_Portugal(new Dictionary<string, string>(), AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		[TestDate(2023, 3, 8)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteAnnualReportXmlToStreamWithATCUD()
		{
			AssertWriteAnnualReportXmlToStreamWithATCUD(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[TestDate(2023, 3, 8)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteAnnualReportXmlToStreamWithATCUD_SAFTOnlyTransactions()
		{
			AssertWriteAnnualReportXmlToStreamWithATCUD(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertWriteAnnualReportXmlToStreamWithATCUD(string reportType, string reportTablePrefix, string reportLineGrouping)
		{
			var replaceTags = new Dictionary<string, string>
			{
				{ "<InvoiceNo>ref102</InvoiceNo><ATCUD>0</ATCUD>",
					@"<InvoiceNo>ref102</InvoiceNo>
				<ATCUD>Report1-ATCUD-01</ATCUD>" },
				{ "<InvoiceNo>ref103</InvoiceNo><ATCUD>0</ATCUD>",
					@"<InvoiceNo>ref103</InvoiceNo>
				<ATCUD>Report2-ATCUD-01</ATCUD>" },
				{ "<InvoiceNo>ref104</InvoiceNo><ATCUD>0</ATCUD>",
					@"<InvoiceNo>ref104</InvoiceNo>
				<ATCUD>Report3-ATCUD-01</ATCUD>" },
				{ "2019", "2023" },
			};
			TestWriteAnnualReportXmlToStream_Portugal(replaceTags, reportType, reportTablePrefix, reportLineGrouping);
		}

		[TestDate(2019, 3, 8)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteAnnualReportXmlToStream_ConsumidorFinal()
		{
			AssertWriteAnnualReportXmlToStream_ConsumidorFinal(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[TestDate(2019, 3, 8)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteAnnualReportXmlToStream_ConsumidorFinal_SAFTOnlyTransactions()
		{
			AssertWriteAnnualReportXmlToStream_ConsumidorFinal(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertWriteAnnualReportXmlToStream_ConsumidorFinal(string reportType, string reportTablePrefix, string reportLineGrouping)
		{
			var orgHeadersWithoutTaxNumber = new List<ZGuid> { Creator.ABIGAS.PK, Creator.AALSHI.PK, Creator.ZECTRA.PK };
			var replaceTags = new Dictionary<string, string>();
			replaceTags.Add("<CustomerTaxID>321321321</CustomerTaxID>", "<CustomerTaxID>999999990</CustomerTaxID>");
			replaceTags.Add("<CustomerTaxID>231231231</CustomerTaxID>", "<CustomerTaxID>999999990</CustomerTaxID>");
			replaceTags.Add("<CustomerTaxID>123123123</CustomerTaxID>", "<CustomerTaxID>999999990</CustomerTaxID>");
			replaceTags.Add(
@"</SystemEntryDate>
				<CustomerID>ABIGAS</CustomerID>",
@"</SystemEntryDate>
				<CustomerID>Consumidor final</CustomerID>");
			replaceTags.Add(
@"		<Customer>
			<CustomerID>ABIGAS</CustomerID>
			<AccountID>Desconhecido</AccountID>
			<CustomerTaxID>999999990</CustomerTaxID>
			<CompanyName>ABI GAS &amp; TOOLS</CompanyName>
			<BillingAddress>
				<AddressDetail>171 ABBOTSFORD ROAD</AddressDetail>
				<City />
				<PostalCode>4006</PostalCode>
				<Country>PT</Country>
			</BillingAddress>
			<SelfBillingIndicator>1</SelfBillingIndicator>
		</Customer>
		<Customer>
			<CustomerID>ZECTRA_WW</CustomerID>
			<AccountID>Desconhecido</AccountID>
			<CustomerTaxID>999999990</CustomerTaxID>
			<CompanyName>ZECO TRADING SA</CompanyName>
			<BillingAddress>
				<AddressDetail>VIA LIGORNETTO 14</AddressDetail>
				<City>STABIO</City>
				<PostalCode>Desconhecido</PostalCode>
				<Country>IT</Country>
			</BillingAddress>
			<SelfBillingIndicator>1</SelfBillingIndicator>
		</Customer>",
@"		<Customer>
			<CustomerID>ZECTRA_WW</CustomerID>
			<AccountID>Desconhecido</AccountID>
			<CustomerTaxID>999999990</CustomerTaxID>
			<CompanyName>ZECO TRADING SA</CompanyName>
			<BillingAddress>
				<AddressDetail>VIA LIGORNETTO 14</AddressDetail>
				<City>STABIO</City>
				<PostalCode>Desconhecido</PostalCode>
				<Country>IT</Country>
			</BillingAddress>
			<SelfBillingIndicator>1</SelfBillingIndicator>
		</Customer>
		<Customer>
			<CustomerID>Consumidor final</CustomerID>
			<AccountID>Desconhecido</AccountID>
			<CustomerTaxID>999999990</CustomerTaxID>
			<CompanyName>Desconhecido</CompanyName>
			<BillingAddress>
				<AddressDetail>Desconhecido</AddressDetail>
				<City>Desconhecido</City>
				<PostalCode>Desconhecido</PostalCode>
				<Country>Desconhecido</Country>
			</BillingAddress>
			<SelfBillingIndicator>0</SelfBillingIndicator>
		</Customer>");
			TestWriteAnnualReportXmlToStream_Portugal(replaceTags, reportType, reportTablePrefix, reportLineGrouping, orgHeadersWithoutTaxNumber);
		}

		[TestDate(2019, 3, 8)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		void TestWriteAnnualReportXmlToStream_Portugal(Dictionary<string, string> replaceTags, string reportType, string reportTablePrefix, string reportLineGrouping, IEnumerable<ZGuid> orgHeadersWithoutTaxNumber = null)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var filePath = @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT Annual.xml";
				TestWriteSAFTAnnualReportCore(replaceTags, filePath, reportType, reportTablePrefix, reportLineGrouping, orgHeadersWithoutTaxNumber);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_NoConsumidorFinal()
		{
			AssertWriteSAFTReportForConsumidorFinal(0, (actualXml) =>
			{
				AssertNotContains("CustomerID: Consumidor final should not appear", "<CustomerID>Consumidor final</CustomerID>", actualXml);
				AssertEquals("CustomerID: CustomerID ABIGAS should appear 3 times", 3, actualXml.Split(new string[] { "<CustomerID>ABIGAS</CustomerID>" }, StringSplitOptions.RemoveEmptyEntries).Length - 1);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_SingleConsumidorFinal()
		{
			AssertWriteSAFTReportForConsumidorFinal(1, (actualXml) =>
			{
				AssertEquals("CustomerID: Consumidor final should appear 2 times", 2, actualXml.Split(new string[] { "<CustomerID>Consumidor final</CustomerID>" }, StringSplitOptions.RemoveEmptyEntries).Length - 1);
				AssertEquals("CustomerID: ABIGAS should appear 2 times", 2, actualXml.Split(new string[] { "<CustomerID>ABIGAS</CustomerID>" }, StringSplitOptions.RemoveEmptyEntries).Length - 1);
				AssertContains("Conatin Customer node: Consumidor final",
@"			<Customer>
				<CustomerID>Consumidor final</CustomerID>
				<AccountID>Desconhecido</AccountID>
				<CustomerTaxID>999999990</CustomerTaxID>
				<CompanyName>Desconhecido</CompanyName>
				<BillingAddress>
					<AddressDetail>Desconhecido</AddressDetail>
					<City>Desconhecido</City>
					<PostalCode>Desconhecido</PostalCode>
					<Country>Desconhecido</Country>
				</BillingAddress>
				<SelfBillingIndicator>0</SelfBillingIndicator>
			</Customer>".Replace("\r", "").Replace("\n", "").Replace("\t", ""), actualXml);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_OnlyConsumidorFinal()
		{
			AssertWriteSAFTReportForConsumidorFinal(2, (actualXml) =>
			{
				AssertEquals("CustomerID: Consumidor final should appear 3 times", 3, actualXml.Split(new string[] { "<CustomerID>Consumidor final</CustomerID>" }, StringSplitOptions.RemoveEmptyEntries).Length - 1);
				AssertNotContains("CustomerID: ABIGAS should not appear", "<CustomerID>ABIGAS</CustomerID>", actualXml);
				AssertContains("Only conatin one Customer node: Consumidor final",
@"		<MasterFiles>
			<Customer>
				<CustomerID>Consumidor final</CustomerID>
				<AccountID>Desconhecido</AccountID>
				<CustomerTaxID>999999990</CustomerTaxID>
				<CompanyName>Desconhecido</CompanyName>
				<BillingAddress>
					<AddressDetail>Desconhecido</AddressDetail>
					<City>Desconhecido</City>
					<PostalCode>Desconhecido</PostalCode>
					<Country>Desconhecido</Country>
				</BillingAddress>
				<SelfBillingIndicator>0</SelfBillingIndicator>
			</Customer>
		</MasterFiles>".Replace("\r", "").Replace("\n", "").Replace("\t", ""), actualXml);
			});
		}

		void AssertWriteSAFTReportForConsumidorFinal(int inoviceCountWithoutPTIVA, Action<string> assertAction)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			{
				Creator.ABIGAS.OH_RL_NKClosestPort = "PTLIS";
				Creator.ABIGAS.OH_Code = "ABIGAS";
				AssertEquals("Precond: org is from Portugal", CountryCodes.Portugal, Creator.ABIGAS.CountryCode);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

				if (inoviceCountWithoutPTIVA == 0)
				{
					Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT111");
				}
				Factory.Save();

				var invoice1 = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.USD, 1m, Creator.ABIGAS);
				invoice1.AH_TransactionReference = "ref1";
				invoice1.AH_ComplianceSubType = "TXI";
				Factory.Save();

				var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.USD, 1m, Creator.ABIGAS);
				invoice2.AH_TransactionReference = "ref2";
				invoice2.AH_ComplianceSubType = "TXI";

				if (inoviceCountWithoutPTIVA == 1)
				{
					Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT111");
				}
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, invoice1, 1);
				Creator.CreateComplianceReportTransactionPivot(report, invoice2, 1);

				var selector = new ReportModeAndCreditorSelector(Factory);
				var writer = GetWriter(report, null, selector);
				var actualXml = GetGeneratedSaftXml(writer);
				assertAction(actualXml);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_SourceBilling()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR, "SourceBilling", "M");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV001", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX, "SourceBilling", "M");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM, "SourceBilling", "M");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV002", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM, "SourceBilling", "M");

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI, "SourceBilling", "P");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL, "SourceBilling", "P");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_HashControl()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR, "HashControl", "1-FTM a/CRD001");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV001", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX, "HashControl", "1-FTM a/INV001");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM, "HashControl", "1-FTM a/CRD002");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV002", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM, "HashControl", "1-FTM a/INV002");

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL, "HashControl", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR, "HashControl", "1");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_InvoiceType()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD, "InvoiceType", "ND");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD, "InvoiceType", "ND");

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR, "InvoiceType", "NC");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "InvoiceType", "NC");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR, "InvoiceType", "NC");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC, "InvoiceType", "NC");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD004", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM, "InvoiceType", "NC");

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI, "InvoiceType", "FT");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX, "InvoiceType", "FT");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "InvoiceType", "FT");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV004", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM, "InvoiceType", "FT");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL, "InvoiceType", "FT");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteSAFTReport_ThirdPartiesBillingIndicator()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD004", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "ThirdPartiesBillingIndicator", "0");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV004", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM, "ThirdPartiesBillingIndicator", "0");

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL, "ThirdPartiesBillingIndicator", "1");
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR, "ThirdPartiesBillingIndicator", "1");
		}

		void AssertSAFTReportXmlSingleSectionWithComplianceSubType(Type transactionType, string transactionNum, string complianceSubType, string expectedSectionName, string expectedSectionValue)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var invoice1 = Creator.CreateInvoice(transactionType, transactionNum, Creator.USD, 1m, Creator.ABIGAS);
				invoice1.AH_TransactionReference = "ref" + transactionNum;
				invoice1.AH_ComplianceSubType = complianceSubType;
				invoice1.SourceReference = $"FTM a/{transactionNum}";

				if (invoice1.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					invoice1.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
				}
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, invoice1, 1);

				var selector = new ReportModeAndCreditorSelector(Factory);
				if (invoice1.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					selector.GenerateSalesInvoices = false;
					selector.CreditorPK = Creator.ABIGAS.PK;
				}
				var writer = GetWriter(report, null, selector);
				var actualXml = GetGeneratedSaftXml(writer);

				AssertEquals($"{expectedSectionName} appear once", 1, actualXml.Split(new string[] { $"<{expectedSectionName}>" }, StringSplitOptions.RemoveEmptyEntries).Length - 1);
				AssertContains($"<{expectedSectionName}>{expectedSectionValue}</{expectedSectionName}>", actualXml);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2023, 9, 10, 23, 42, 59)]
		public void TestWriteSAFTReportWithATCUD()
		{
			AssertWriteSAFTReportWithATCUD(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2023, 9, 10, 23, 42, 59)]
		public void TestWriteSAFTReportWithATCUD_SAFTOnlyTransactions()
		{
			AssertWriteSAFTReportWithATCUD(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertWriteSAFTReportWithATCUD(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = CreateWriteSAFTReportData(reportType, tablePrefix, lineGrouping);
				var replaceTags = new Dictionary<string, string>
				{
					{ "<InvoiceNo>ref1</InvoiceNo>\r\n        <ATCUD>0</ATCUD>",
					  "<InvoiceNo>ref1</InvoiceNo>\r\n        <ATCUD>TEST-ATCUD-01</ATCUD>" },
					{ "<InvoiceNo>ref3</InvoiceNo>\r\n        <ATCUD>0</ATCUD>",
					  "<InvoiceNo>ref3</InvoiceNo>\r\n        <ATCUD>TEST-ATCUD-03</ATCUD>" },
					{ "2018", "2023" },
					{ "2019", "2024" },
				};
				AssertWriteSAFTReport(report, replaceTags);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionDisabled()
		{
			AssertSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionDisabled(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionDisabled_SAFTOnlyTransactions()
		{
			AssertSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionDisabled(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionDisabled(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CC10.AC_LocalLanguageDescription = "Local Charge Code 10";
				AssertNotEquals("preCond to test ChargeCode ProductDescription", Creator.CC10.AC_Desc, Creator.CC10.AC_LocalLanguageDescription);

				var report = CreateWriteSAFTReportData(reportType, tablePrefix, lineGrouping);

				using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertWriteSAFTReport(report);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionEnabled()
		{
			AssertSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionEnabled(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionEnabled_SAFTOnlyTransactions()
		{
			AssertSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionEnabled(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertSAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionEnabled(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CC10.AC_LocalLanguageDescription = "Local Charge Code 10";
				AssertNotEquals("preCond to test ChargeCode ProductDescription", Creator.CC10.AC_Desc, Creator.CC10.AC_LocalLanguageDescription);

				var report = CreateWriteSAFTReportData(reportType, tablePrefix, lineGrouping);

				var replaceTags = new Dictionary<string, string>
				{
					{ "<ProductDescription>Charge Code 10</ProductDescription>", "<ProductDescription>Local Charge Code 10</ProductDescription>" }
				};

				using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertWriteSAFTReport(report, replaceTags);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportEndDate()
		{
			AssertSAFTReportEndDate(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportEndDate_SAFTOnlyTransactions()
		{
			AssertSAFTReportEndDate(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		public void AssertSAFTReportEndDate(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = CreateWriteSAFTReportData(reportType, tablePrefix, lineGrouping);
				AssertEquals(new ZDate(2018, 9, 10), report.ACR_DateFrom);
				AssertEquals(new ZDate(2018, 10, 9), report.ACR_DateTo);

				AssertResult(new DateTime(2018, 10, 1, 12, 0, 0), "2018-10-01");
				AssertResult(new DateTime(2018, 10, 9, 12, 0, 0), "2018-10-09");
				AssertResult(new DateTime(2018, 10, 10, 12, 0, 0), "2018-10-09");

				void AssertResult(DateTime todayDate, string expectedEndDate)
				{
					TestDateAttribute.Date = todayDate;

					var replaceTags = new Dictionary<string, string>
					{
						{ "<DateCreated>2018-09-10</DateCreated>", $"<DateCreated>{todayDate.ToString("yyyy-MM-dd")}</DateCreated>" },
						{ "<EndDate>2018-09-10</EndDate>", $"<EndDate>{expectedEndDate}</EndDate>" }
					};

					AssertWriteSAFTReport(report, replaceTags);
				}
			}
		}

		void AssertWriteSAFTReport(AccComplianceReport report, Dictionary<string, string> replaceTags = null)
		{
			if (replaceTags == null)
			{
				replaceTags = new Dictionary<string, string>();
			}

			replaceTags.Add("<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>");

			var selector = new ReportModeAndCreditorSelector(Factory);
			AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT.xml", replaceTags);

			report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);

			AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT.xml", replaceTags);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportSignature()
		{
			AssertSAFTReportSignature(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTReportSignature_SAFTOnlyTransactions()
		{
			AssertSAFTReportSignature(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertSAFTReportSignature(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = Creator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "ref", 1, 100, 1);
				Factory.Save();

				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.Company.GC_Phone = "012345678";
				report.Company.GC_Email = "email@company.com";

				report.ACR_DateTo = ZDate.Today.AddDays(-1);
				Assert("preCondition to test DateEnd: report creation date must be later then report period end date", ZDate.Today > report.ACR_DateTo);

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_ReportType = reportType;
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVA6"));
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				taxMessage.A9_TaxGroupCode = "GRP";

				var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
				var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

				var openingPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today.AddMonths(-2));
				Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Factory.Save();

				var shipment = Creator.CreateShipment("S001001", true);
				var job = Creator.CreateJob(shipment);
				var charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 110m, creditor: Creator.Creditor1, osSellAmt: 160m, debtor: Creator.Debtor1);
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "*JC*ACR*ACRCtrl*-");
				Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "**JC*ACR**");
				Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "*JC*WIP*WIPCtrl*-");
				Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "**JC*WIP**");

				var originalInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0000", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
				originalInvoice.AH_TransactionReference = "originalRef1";
				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				Creator.ABIGAS.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(-1);
				invoice.AH_ComplianceSubType = "TXI";
				invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;
				invoice.AH_TransactionReference = "ref1";
				Assert("Has Lines", invoice.Lines.Count > 0);
				var line = invoice.Lines[0];
				line.AL_AC = Creator.CC10.PK;
				line.AL_AT = taxRate.PK;
				line.AL_A9_VATClass = taxMessage.PK;
				var line2 = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.USD);
				Creator.GLHeader1.AG_AccountNum = "8888.10.00";
				Creator.GLHeader1.AG_DebitCredit = Constants.DebitCredit.Debit;
				Creator.GLHeader1.AG_Description = "Test GL Account";
				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT123123123");
				(invoice as IAmending).AmendingReason = "AmendingReason1";

				var receipt = Creator.CreateARReceipt(1m, 150m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
				Factory.Save();

				if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line, 3);
					Creator.CreateComplianceReportTransactionPivot(report, line2, 4);
				}
				else
				{
					Creator.CreateComplianceReportTransactionPivot(report, invoice, 3, "*AR*INV*ARCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*GSTOut*-");
					Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*GSTOut*-");

					Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*Bank*-");
					Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*ARCtrl*");

					AssertEquals("GLOpeningBalanceDR is set for test", 123m, report.GLOpeningBalanceDR);
					AssertEquals("GLOpeningBalanceCR is set for test", 123m, report.GLOpeningBalanceCR);
				}

				var selector = new ReportModeAndCreditorSelector(Factory);
				report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);
				var saftXmlWriter = GetWriter(report, null, selector);
				var generatedXml = GetGeneratedSaftXml(saftXmlWriter);

				var xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(generatedXml);
				var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlnsManager.AddNamespace("n", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

				var invoiceSignature = xmlDocument.SelectSingleNode("/n:AuditFile/n:SourceDocuments/n:SalesInvoices/n:Invoice/n:Hash", xmlnsManager).InnerText;
				var invoiceDate = xmlDocument.SelectSingleNode("/n:AuditFile/n:SourceDocuments/n:SalesInvoices/n:Invoice/n:InvoiceDate", xmlnsManager).InnerText;
				var invoiceCreationLogTime = xmlDocument.SelectSingleNode("/n:AuditFile/n:SourceDocuments/n:SalesInvoices/n:Invoice/n:SystemEntryDate", xmlnsManager).InnerText;
				var invoiceNo = xmlDocument.SelectSingleNode("/n:AuditFile/n:SourceDocuments/n:SalesInvoices/n:Invoice/n:InvoiceNo", xmlnsManager).InnerText;
				var invoiceAmount = xmlDocument.SelectSingleNode("/n:AuditFile/n:SourceDocuments/n:SalesInvoices/n:Invoice/n:DocumentTotals/n:GrossTotal", xmlnsManager).InnerText;

				AssertNotNullOrEmpty(invoiceSignature);
				AssertNotNullOrEmpty(invoiceDate);
				AssertNotNullOrEmpty(invoiceCreationLogTime);
				AssertNotNullOrEmpty(invoiceNo);
				AssertNotNullOrEmpty(invoiceAmount);

				var signatureGeneratedFromXmlValues = string.Format("{0};{1};{2};{3};", invoiceDate, invoiceCreationLogTime, invoiceNo, invoiceAmount);
				Assert("generated signature using xml values should be equals to xml signature",
					new RSASecurityProvider().SignatureVerifier.VerifyHash(signatureGeneratedFromXmlValues, invoiceSignature));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestWriteSingleReportXmlToStream_WithEmptyCustomCode()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CreateCustomsCodes(Creator.ABIGAS, ZString.Empty, ZString.Empty, "1234");
				var report = CreateWriteSAFTReportData(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
				AssertWriteSAFTReport(report);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 6, 14)]
		public void TestWriteSAFTSelfBilledReport()
		{
			AssertWriteSAFTSelfBilledReport(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 6, 14)]
		public void TestWriteSAFTSelfBilledReport_SAFTOnlyTransactions()
		{
			AssertWriteSAFTSelfBilledReport(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertWriteSAFTSelfBilledReport(string reportType, string tablePrefix, string lineGrouping)
		{
			var softwareCertificateNumber = "0000";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, softwareCertificateNumber))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = reportType;

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(ZDateTime.Today, report.ACR_GC_Company);
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVA6"));

				var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
				var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

				var openingPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today.AddMonths(-2));
				var openingBalanceDR = Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				var openingBalanceCR = Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Factory.Save();

				var shipment = Creator.CreateShipment("S001001", true);
				var job = Creator.CreateJob(shipment);
				var charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 110m, creditor: Creator.Creditor1, osSellAmt: 160m, debtor: Creator.Debtor1);
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "*JC*ACR*ACRCtrl*-");
				Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "**JC*ACR**");
				Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "*JC*WIP*WIPCtrl*-");
				Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "**JC*WIP**");

				Creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "012345678");
				var address = report.Company.OrgProxy.Addresses.GetAddressWithMainAddressFallback(report.Company.OrgProxy.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";

				Creator.ABIGAS.CompanyData.OB_IsCreditor = true;
				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "123123123");

				Creator.AALSHI.CompanyData.OB_IsCreditor = true;
				Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "231231231");
				address = Creator.AALSHI.Addresses.GetAddressWithMainAddressFallback(Creator.AALSHI.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";

				var abigasSelfBillingInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0000", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
				abigasSelfBillingInvoice.AH_OH = Creator.ABIGAS.PK;
				abigasSelfBillingInvoice.AH_ComplianceSubType = "TXI";
				abigasSelfBillingInvoice.AH_TransactionReference = "Ref1";
				abigasSelfBillingInvoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
				Assert("Has Lines", abigasSelfBillingInvoice.Lines.Count > 0);
				var line1 = abigasSelfBillingInvoice.Lines[0];
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 100m, creditor: Creator.ABIGAS, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				var aalshiSelfBillingInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				Creator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
				aalshiSelfBillingInvoice.AH_OH = Creator.AALSHI.PK;
				aalshiSelfBillingInvoice.AH_ComplianceSubType = "TXI";
				aalshiSelfBillingInvoice.AH_InvoiceDate = aalshiSelfBillingInvoice.AH_PostDate.AddDays(1);
				aalshiSelfBillingInvoice.AH_TransactionReference = "Ref1";
				aalshiSelfBillingInvoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;

				Assert("Has Lines", aalshiSelfBillingInvoice.Lines.Count > 0);
				var line2_1 = aalshiSelfBillingInvoice.Lines[0];
				line2_1.AL_AC = Creator.CC10.PK;
				line2_1.AL_AT = taxRate.PK;
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 200m, creditor: Creator.AALSHI, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line2_1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				var line2_2 = Creator.CreateInvoiceLine(aalshiSelfBillingInvoice, Creator.CC1.PK, 100m, Creator.USD);
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, costCurrency: Creator.USD, osCostAmt: 100m, creditor: Creator.AALSHI, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line2_2.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				Creator.GLHeader1.AG_AccountNum = "8888.10.00";
				Creator.GLHeader1.AG_DebitCredit = Constants.DebitCredit.Debit;
				Creator.GLHeader1.AG_Description = "Test GL Account";

				var receipt = Creator.CreateARReceipt(1m, 150m, aalshiSelfBillingInvoice.AH_PostDate, aalshiSelfBillingInvoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
				Factory.Save();

				if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line1, 3);
					Creator.CreateComplianceReportTransactionPivot(report, line2_1, 4);
					Creator.CreateComplianceReportTransactionPivot(report, line2_2, 5);
				}
				else
				{
					Creator.CreateComplianceReportTransactionPivot(report, abigasSelfBillingInvoice, 3, "*AP*INV*APCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AP*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AP*INV*APSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AP*INV*APSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, aalshiSelfBillingInvoice, 5, "*AP*INV*APCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AP*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AP*INV*APSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AP*INV*APSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AP*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AP*INV*APSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AP*INV*APSusp*-");

					Creator.CreateComplianceReportTransactionPivot(report, receipt, 8, "*AR*REC*Bank*-");
					Creator.CreateComplianceReportTransactionPivot(report, receipt, 8, "*AR*REC*ARCtrl*");

					AssertEquals("GLOpeningBalanceDR is set for test", 123m, report.GLOpeningBalanceDR);
					AssertEquals("GLOpeningBalanceCR is set for test", 123m, report.GLOpeningBalanceCR);
				}

				var creditor2SelfBillingInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0002", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				creditor2SelfBillingInvoice.AH_OH = Creator.Creditor2.PK;
				creditor2SelfBillingInvoice.AH_ComplianceSubType = "TXM";
				creditor2SelfBillingInvoice.AH_InvoiceDate = creditor2SelfBillingInvoice.AH_PostDate.AddDays(2);
				creditor2SelfBillingInvoice.AH_TransactionReference = "ref3";
				creditor2SelfBillingInvoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;

				Assert("Has Lines", creditor2SelfBillingInvoice.Lines.Count > 0);
				var line3_1 = creditor2SelfBillingInvoice.Lines[0];
				line3_1.AL_AC = Creator.CC10.PK;
				line3_1.AL_AT = Creator.GST1.PK;
				line3_1.AL_AG = Creator.GLHeader1.PK;
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 200m, creditor: Creator.Creditor2, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line3_1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				var line3_2 = Creator.CreateInvoiceLine(creditor2SelfBillingInvoice, Creator.CC1.PK, 100m, Creator.USD);
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, costCurrency: Creator.USD, osCostAmt: 100m, creditor: Creator.Creditor2, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line3_2.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
				Factory.Save();

				var aalshiStandardInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0003", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				aalshiStandardInvoice.AH_OH = Creator.AALSHI.PK;
				aalshiStandardInvoice.AH_ComplianceSubType = "TXI";
				aalshiStandardInvoice.AH_InvoiceDate = aalshiStandardInvoice.AH_PostDate.AddDays(2);
				aalshiStandardInvoice.AH_TransactionNum = "I0003";
				aalshiStandardInvoice.AH_TransactionReference = "ref4";
				aalshiStandardInvoice.AH_TransactionCategory = TransactionCategory.Codes.Standard;

				Assert("Has Lines", aalshiStandardInvoice.Lines.Count > 0);
				var line4_1 = aalshiStandardInvoice.Lines[0];
				line4_1.AL_AC = Creator.CC11.PK;
				line4_1.AL_AT = Creator.GST1.PK;
				line4_1.AL_AG = Creator.GLHeader1.PK;
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC11, costCurrency: Creator.USD, osCostAmt: 200m, creditor: Creator.AALSHI, osSellAmt: 0m, debtor: Creator.Creditor2);
				charge.JR_AL_APLine = line4_1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
				Factory.Save();

				if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line3_1, 6);
					Creator.CreateComplianceReportTransactionPivot(report, line3_2, 7);
					Creator.CreateComplianceReportTransactionPivot(report, line4_1, 8);
				}
				else
				{
					Creator.CreateComplianceReportTransactionPivot(report, creditor2SelfBillingInvoice, 9, "*AP*INV*APCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AP*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AP*INV*APSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AP*INV*APSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AP*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AP*INV*APSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AP*INV*APSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, aalshiStandardInvoice, 12, "*AP*INV*APCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AP*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AP*INV*APSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AP*INV*APSusp*-");
				}

				var replaceTags = new Dictionary<string, string>
				{
					{ "<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>" },
					{ "<SoftwareCertificateNumber>0000</SoftwareCertificateNumber>", $"<SoftwareCertificateNumber>{softwareCertificateNumber}</SoftwareCertificateNumber>" },
				};
				var selector = new ReportModeAndCreditorSelector(Factory);
				selector.GenerateCreditorInvoices = true;
				selector.CreditorPK = Creator.AALSHI.PK;
				AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_SelfBilled.xml", replaceTags);

				report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);

				AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_SelfBilled.xml", replaceTags);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 6, 17)]
		public void TestWriteSAFTSelfBilledAnnualReport()
		{
			AssertWriteSAFTSelfBilledAnnualReport(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 6, 17)]
		public void TestWriteSAFTSelfBilledAnnualReport_SAFTOnlyTransactions()
		{
			AssertWriteSAFTSelfBilledAnnualReport(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertWriteSAFTSelfBilledAnnualReport(string reportType, string tablePrefix, string lineGrouping)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "1234"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var startOfFinancialYear = new ZDateTime(2019, 1, 1);
				Creator.CreateTestPeriods(startOfFinancialYear); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var periodCalculator = new AccountingPeriodCalculator(Factory);

				var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
				var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

				Creator.GLHeader1.AG_AccountNum = "8888.10.00";
				Creator.GLHeader1.AG_DebitCredit = Constants.DebitCredit.Debit;
				Creator.GLHeader1.AG_Description = "Test GL Account1";

				Creator.GLHeader2.AG_AccountNum = "8888.20.00";
				Creator.GLHeader2.AG_DebitCredit = Constants.DebitCredit.Debit;
				Creator.GLHeader2.AG_Description = "Test GL Account2";

				var openingPeriod = periodCalculator.GetPeriodFromDate(startOfFinancialYear);
				var openingBalanceDR = Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				var openingBalanceCR = Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				Factory.Save();

				var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
				report1.ACR_ReportType = reportType;
				report1.ACR_DateFrom = new ZDate(2019, 5, 1);
				report1.ACR_DateTo = new ZDate(2019, 5, 31);
				Creator.CreateConfigurationForComplianceReport(report1, tablePrefix, lineGrouping);

				var reportPeriod = periodCalculator.GetPeriodManagementFromDate(report1.ACR_DateFrom, report1.ACR_GC_Company);
				report1.AccountingPeriod = reportPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report1.ACR_DateFrom);
				AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report1.ACR_DateTo);

				Creator.CreateCustomsCodes(report1.Company.OrgProxy, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "012345678");
				var address = report1.Company.OrgProxy.Addresses.GetAddressWithMainAddressFallback(report1.Company.OrgProxy.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";

				Creator.ABIGAS.CompanyData.OB_IsCreditor = true;
				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "123123123");

				Creator.AALSHI.CompanyData.OB_IsCreditor = true;
				Creator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
				Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "231231231");
				address = Creator.AALSHI.Addresses.GetAddressWithMainAddressFallback(Creator.AALSHI.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";
				Factory.Save();

				CreateSelfBilledTestData(report1, Creator.AALSHI, Creator.CC10, Creator.GLHeader1);

				if (report1.ACR_ReportType != AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					AssertEquals("GLOpeningBalanceDR is set for test", 123m, report1.GLOpeningBalanceDR);
					AssertEquals("GLOpeningBalanceCR is set for test", 123m, report1.GLOpeningBalanceCR);
				}

				// Simulate GL TakeUp for the report1
				Creator.CreateAccGLAggregate(250m, reportPeriod.AM_Period, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-210m, reportPeriod.AM_Period, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(510m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6210.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-60m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8310.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-600m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8888.10.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(150m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "ZUSDHeader").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
				report2.ACR_ReportType = report1.ACR_ReportType;
				report2.ACR_DateFrom = new ZDate(2019, 6, 1);
				report2.ACR_DateTo = new ZDate(2019, 6, 30);
				reportPeriod = periodCalculator.GetPeriodManagementFromDate(report2.ACR_DateFrom, report2.ACR_GC_Company);
				report2.AccountingPeriod = reportPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report2.ACR_DateFrom);
				AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report2.ACR_DateTo);
				Factory.Save();

				CreateSelfBilledTestData(report2, Creator.AALSHI, Creator.CC6, Creator.GLHeader2);

				if (report2.ACR_ReportType != AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					AssertEquals("GLOpeningBalanceDR", 1033m, report2.GLOpeningBalanceDR);
					AssertEquals("GLOpeningBalanceCR", 1033m, report2.GLOpeningBalanceCR);
				}

				var replaceTags = new Dictionary<string, string>
				{
					{ "<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>" },
					{ "<SoftwareCertificateNumber>0000</SoftwareCertificateNumber>", $"<SoftwareCertificateNumber>1234</SoftwareCertificateNumber>" },
				};

				var selector = new ReportModeAndCreditorSelector(Factory);
				selector.GenerateCreditorInvoices = true;
				selector.CreditorPK = Creator.AALSHI.PK;
				AssertSAFTXml(GetWriter(null, new AccComplianceReport[] { report1, report2 }, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_SelfBilled Annual.xml", replaceTags, true);
			}
		}

		[TestDate(2019, 8, 11)]
		public void TestBuildTaxTableXml()
		{
			AssertBuildTaxTableXml(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[TestDate(2019, 8, 11)]
		public void TestBuildTaxTableXml_SAFTOnlyTransactions()
		{
			AssertBuildTaxTableXml(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertBuildTaxTableXml(string reportType, string tablePrefix, string lineGrouping)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var startOfFinancialYear = new ZDateTime(2019, 1, 1);
				Creator.CreateTestPeriods(startOfFinancialYear); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var periodCalculator = new AccountingPeriodCalculator(Factory);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = reportType;
				report.ACR_DateFrom = new ZDate(2019, 8, 1);
				report.ACR_DateTo = new ZDate(2019, 8, 31);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);
				Factory.Save();

				var taxIDs = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.Portugal).AddToFilter(AccTaxRateSchema.AT_IsActive, ZBool.True));

				var collector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT);
				Assert("Additional data collector loads all tax, including currently inactive", collector.TaxIDs.Count >= taxIDs.Length);
				AssertEquals("We do not have any lines and tere should be no Invoiced Tax IDs", 0, collector.GetInvoicedTaxIDs().Count);
				AssertNull("TaxTable Xml", new TaxTable().BuildXml(report, null, collector));

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m, Creator.ABIGAS);
				invoice.AH_ComplianceSubType = "TXI";
				var lines = new List<InvoicingLineBase>();
				var taxCodes = new ZStringBuilder();

				foreach (var taxId in taxIDs)
				{
					var line = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.AUD);
					line.AL_AT = taxId.PK;
					lines.Add(line);
					taxCodes.Append(taxId.AT_Code);
				}
				Factory.Save();

				var reportLineNumber = 0;
				if (report.ACR_ReportType != AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, invoice, ++reportLineNumber, "*AR*INV*ARCtrl*Total");
				}
				foreach (var line in lines)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line, ++reportLineNumber, "*AR*INV**Rev-");
				}
				report.ClearReportLines_ForTestOnly();

				collector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT);
				Assert("Additional data collector loads all tax, including currently inactive", collector.TaxIDs.Count >= taxIDs.Length);
				AssertEquals("We have created new line for every active Tax ID", taxIDs.Length, collector.GetInvoicedTaxIDs().Count);

				var expected = @"<TaxTable>
  <TaxTableEntry>
    <TaxType>IVA</TaxType>
    <TaxCountryRegion>PT</TaxCountryRegion>
    <TaxCode>NOR</TaxCode>
    <Description>Normal</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
  <TaxTableEntry>
    <TaxType>NS</TaxType>
    <TaxCountryRegion>PT</TaxCountryRegion>
    <TaxCode>NS</TaxCode>
    <Description>Nao sujeicao</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
  <TaxTableEntry>
    <TaxType>IVA</TaxType>
    <TaxCountryRegion>PT</TaxCountryRegion>
    <TaxCode>ISE</TaxCode>
    <Description>Isenta</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
  <TaxTableEntry>
    <TaxType>IVA</TaxType>
    <TaxCountryRegion>PT</TaxCountryRegion>
    <TaxCode>INT</TaxCode>
    <Description>Intermédia</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
  <TaxTableEntry>
    <TaxType>IVA</TaxType>
    <TaxCountryRegion>PT-AC</TaxCountryRegion>
    <TaxCode>NOR</TaxCode>
    <Description>Normal</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
  <TaxTableEntry>
    <TaxType>IVA</TaxType>
    <TaxCountryRegion>PT-MA</TaxCountryRegion>
    <TaxCode>NOR</TaxCode>
    <Description>Normal</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
  <TaxTableEntry>
    <TaxType>IVA</TaxType>
    <TaxCountryRegion>PT</TaxCountryRegion>
    <TaxCode>RED</TaxCode>
    <Description>Reduzida</Description>
    <TaxPercentage>0</TaxPercentage>
  </TaxTableEntry>
</TaxTable>";

				var actual = new TaxTable().BuildXml(report, null, collector);

				AssertEquals("Tax Codes used for generating TaxTable Xml :" + taxCodes.ToStringWithDelimiterBetweenAppends(", "), expected, actual.ToString());
			}
		}

		void CreateSelfBilledTestData(AccComplianceReport report, OrgHeader testOrg, AccChargeCode testCode, AccGLHeader testHeader)
		{
			var numberSuffix = report.AccountingPeriod.ToString().Substring(4, 2);
			var shipment = Creator.CreateShipment("S00100" + numberSuffix, true);
			var job = Creator.CreateJob(shipment);
			var charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 110m, creditor: Creator.Creditor1, osSellAmt: 160m, debtor: Creator.Debtor1);
			Factory.Save();

			var postDate = new ZDateTime(report.ACR_DateFrom.Year, report.ACR_DateFrom.Month, ZDateTime.Today.Day);
			charge.WIP.AL_PostDate = charge.Accrual.AL_PostDate = postDate;
			Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "*JC*ACR*ACRCtrl*-");
			Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "**JC*ACR**");
			Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "*JC*WIP*WIPCtrl*-");
			Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "**JC*WIP**");

			var invoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0001" + numberSuffix, Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice.AH_OH = Creator.ABIGAS.PK;
			invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(1);
			invoice.AH_ComplianceSubType = "TXI";
			invoice.AH_TransactionReference = "ref1" + numberSuffix;
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AC = testCode.PK;
			line.AL_AT = Creator.GST1.PK;
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 100m, creditor: Creator.ABIGAS, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.USD);
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 100m, creditor: Creator.ABIGAS, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line2.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();

			var receipt = Creator.CreateARReceipt(1m, 150m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
			Factory.Save();

			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				Creator.CreateComplianceReportTransactionPivot(report, line, 3);
				Creator.CreateComplianceReportTransactionPivot(report, line2, 4);
			}
			else
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice, 3, "*AP*INV*APCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AP*INV*APSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AP*INV*APSusp*-");

				Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*Bank*-");
				Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*ARCtrl*");
			}

			var invoice2 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0002" + numberSuffix, Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice2.AH_OH = testOrg.PK;
			invoice2.AH_InvoiceDate = invoice.AH_PostDate.AddDays(2);
			invoice2.AH_ComplianceSubType = "TXI";
			invoice2.AH_TransactionReference = "ref2" + numberSuffix;
			invoice2.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;

			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line2_1 = invoice2.Lines[0];
			line2_1.AL_AC = testCode.PK;
			line2_1.AL_AT = Creator.GST1.PK;
			line2_1.AL_AG = testHeader.PK;
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 200m, creditor: testOrg, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line2_1.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			var line2_2 = Creator.CreateInvoiceLine(invoice2, Creator.CC10.PK, 100m, Creator.USD);
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 100m, creditor: testOrg, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line2_2.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			Factory.Save();

			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 5);
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 6);
			}
			else
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice2, 7, "*AP*INV*APCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AP*INV*APSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AP*INV*APSusp*-");
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTXMLDescriptionLengthForSalesInvoice_SAFT()
		{
			AssertSAFTXMLDescriptionLengthForSalesInvoice(ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestSAFTXMLDescriptionLengthForSalesInvoice_SAFTOnlyTransactions()
		{
			AssertSAFTXMLDescriptionLengthForSalesInvoice(ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
		}

		void AssertSAFTXMLDescriptionLengthForSalesInvoice(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = reportType;
				report.ACR_DateFrom = ZDate.Today.AddDays(-7);
				report.ACR_DateTo = ZDate.Today;
				report.Company.GC_Phone = "012345678";
				report.Company.GC_Email = "email@company.com";

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVA6"));
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				taxMessage.A9_TaxGroupCode = "GRP";

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_ComplianceSubType = "TXI";
				Assert("Has Lines", invoice.Lines.Count > 0);
				var line = invoice.Lines[0];
				line.AL_AC = Creator.CC10.PK;
				line.AL_AT = taxRate.PK;
				line.AL_A9_VATClass = taxMessage.PK;
				line.AL_Desc = "This is for test the character count generated in the XML. This is for test the character count generated in the XML. This is to test char count generated in the XML. This description should end here.And Not print the rest of the line in the XML.";

				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT123123123");
				Factory.Save();

				if (reportType == ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line, 1);
					AssertEquals(1, report.ReportLines.Count);
				}
				else
				{
					Creator.CreateComplianceReportTransactionPivot(report, invoice, 1, "*AR*INV*ARCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*GSTOut*-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 3, "*AR*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 3, "*AR*INV*ARSusp*Rev");
					AssertEquals(5, report.ReportLines.Count);
				}

				Factory.Save();

				var replaceTags = new Dictionary<string, string>()
				{
					{ "<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>" },
					{ "<Description>tee he he</Description>", "<Description>This is for test the character count generated in the XML. This is for test the character count generated in the XML. This is to test char count generated in the XML. This description should end here.</Description>" }
				};

				var selector = new ReportModeAndCreditorSelector(Factory);
				AssertSAFTXml(GetWriter(report, null, selector), BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT-PT_SingleLine.xml", replaceTags);
			}
		}
	}
}
