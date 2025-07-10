using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	sealed class GeneralLedgerEntries1_10Test : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 9, 20)]
		public void TestGeneralLederEntries1_10()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var glHeaderPAndL = Creator.CreateAccGLHeader("A400.00.33", "TS", "P&L", "P&L", Core.Constants.DebitCredit.Debit);
				Creator.CreateTestPeriods(new ZDateTime(2022, 9, 20));

				Creator.GLHeader1.AG_AccountNum = "TESDCEARBN";
				Creator.GLHeader1.AG_Description = "AG Desc1";
				Creator.GLHeader2.AG_AccountNum = "TESEWXMVRD";
				Creator.GLHeader2.AG_Description = "AG Desc2";

				var arInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", Creator.AUD, 1.0m, 100m, 0m, 100m, 0m);
				var arLine = arInvoice.Lines[0];
				arLine.AL_AT = Creator.GST1.PK;
				arLine.AL_A9_VATClass = Creator.TaxMsg1.PK;
				arLine.AL_AG = glHeaderPAndL.PK;
				Creator.TaxMsg1.A9_TaxGroupCode = "TTG";

				var apInvoice = Creator.CreateAPInvoice<APInvoice>("ACE1", Creator.AUD, 1m, 200m, 0.0m, 0.0m, 200m, 0.0m, 0.0m);
				apInvoice.AH_TransactionReference = "APINV0001";
				var apLine = apInvoice.Lines[0];

				var shipment = Creator.CreateShipment("S01234", false);
				var job = Creator.CreateJob(shipment, false);
				var wip = Creator.CreateWIP(job, Creator.CC1, 1, "a new wip", 10, debtor: Creator.ABIGAS);
				wip.AL_AG = Creator.GLHeader1.PK;
				var accrual = Creator.CreateAccrual(job, Creator.CC2, 1, "a new accrual", 20, creditor: Creator.AALSHI);
				accrual.AL_AG = Creator.GLHeader2.PK;

				var arContra = Factory.NewWithValidTestData<ARContraRow>();
				var apContra = Factory.NewWithValidTestData<APContraRow>();
				arContra.AH_TransactionNum = "00010001";
				arContra.AH_OH = Creator.Debtor.PK;
				apContra.AH_TransactionNum = "00010001";
				apContra.AH_OH = Creator.Creditor1.PK;

				var arTransfer = Factory.NewWithValidTestData<ARTransferFromRow>();
				var apTransfer = Factory.NewWithValidTestData<APTransferToRow>();
				arTransfer.AH_TransactionNum = "00010001";
				apTransfer.AH_TransactionNum = "00010001";

				var autoJournal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
				var noteJournal = Creator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today);

				Factory.Save();

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
				report.ACR_GC_Company = GlbCompany.CurrentCompany.PK;
				report.ACR_DateFrom = new ZDate(2022, 9, 20);
				report.ACR_DateTo = new ZDate(2022, 9, 21);
				report.ACR_SystemCreateTimeUtc = new ZDateTime(2022, 9, 21);

				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, arInvoice, 1);
				Creator.CreateComplianceReportTransactionPivot(report, arLine, 2, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, arLine, 2, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, apInvoice, 3);
				Creator.CreateComplianceReportTransactionPivot(report, apLine, 4, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, wip, 5, "*JC*WIP*");
				Creator.CreateComplianceReportTransactionPivot(report, accrual, 5, "*JC*ACR*");
				Creator.CreateComplianceReportTransactionPivot(report, arTransfer, 6, "AR*TRF*ARCtrl*");
				Creator.CreateComplianceReportTransactionPivot(report, apTransfer, 7, "*AP*TRF*APCtrl*");
				Creator.CreateComplianceReportTransactionPivot(report, apContra, 8, "*AP*CTR*APCtrl*");
				Creator.CreateComplianceReportTransactionPivot(report, arContra, 9, "*AR*CTR*ARCtrl*");
				Creator.CreateComplianceReportTransactionPivot(report, autoJournal, 10);
				Creator.CreateComplianceReportTransactionPivot(report, noteJournal, 11);

				Factory.Save();

				var additionalDataCollector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_10, Creator.ABIGAS.PK, Creator.ABIGAS.OH_Code);

				var generalLedgerEntries = new GeneralLedgerEntries1_10();
				var result = generalLedgerEntries.BuildXml(report, additionalData: additionalDataCollector).ToString();

				var expectedXml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT1_10\SAFT-NO GeneralLederEntries.xml");
				this.AssertXMLEqualsByDiff(expectedXml, result, XmlDiffEquals.XmlCompareOptions.IgnoreXmlDecl);
			}
		}

		[TestDate(2022, 9, 20)]
		public void TestGeneralLederEntries1_10_TransactionDate_SystemEntryDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var glHeaderPAndL = Creator.CreateAccGLHeader("A400.00.33", "TS", "P&L", "P&L", Core.Constants.DebitCredit.Debit);
				Creator.CreateTestPeriods(new ZDateTime(2022, 9, 20));

				var arInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", Creator.AUD, 1.0m, 100m, 0m, 100m, 0m);
				arInvoice.AH_InvoiceDate = new ZDate(2022, 9, 19);
				arInvoice.AH_PostDate = new ZDate(2022, 9, 20);
				var arLine = arInvoice.Lines[0];
				arLine.AL_AT = Creator.GST1.PK;
				arLine.AL_A9_VATClass = Creator.TaxMsg1.PK;
				arLine.AL_AG = glHeaderPAndL.PK;
				Creator.TaxMsg1.A9_TaxGroupCode = "TTG";

				Factory.Save();

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
				report.ACR_GC_Company = GlbCompany.CurrentCompany.PK;
				report.ACR_DateFrom = new ZDate(2022, 9, 20);
				report.ACR_DateTo = new ZDate(2022, 9, 21);
				report.ACR_SystemCreateTimeUtc = new ZDateTime(2022, 9, 21);

				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, arInvoice, 1);
				Creator.CreateComplianceReportTransactionPivot(report, arLine, 2, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, arLine, 2, "*AR*INV*GSTOut*-");

				Factory.Save();

				var additionalDataCollector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_10, Creator.ABIGAS.PK, Creator.ABIGAS.OH_Code);

				var generalLedgerEntries = new GeneralLedgerEntries1_10();
				var result = generalLedgerEntries.BuildXml(report, additionalData: additionalDataCollector).ToString();

				AssertContains("<TransactionDate>2022-09-20</TransactionDate>", result);
				AssertContains("<SystemEntryDate>2022-09-20</SystemEntryDate>", result);
				AssertContains("<GLPostingDate>2022-09-20</GLPostingDate>", result);
			}
		}

		TestObjectCreator Creator
		{
			get { return fcreator ?? (fcreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fcreator;
	}
}
