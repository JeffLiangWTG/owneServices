using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ARCreditNoteApprovalTest : ScriptTest
	{
		public void TestSimpleRun()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Invoice01", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var approval1 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval1.XP_ParentTableCode = "AH";
			approval1.XP_ParentID = invoice.PK;
			string xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><MaxAmountToApprove>200.12</MaxAmountToApprove><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			approval1.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(xml);

			var consol = TestObjectCreator.CreateConsol("CNSHA", "ITROM", "Consol01");
			var approval2 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval2.XP_ParentTableCode = "VK";
			approval2.XP_ParentID = consol.PK;
			xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><MaxAmountToApprove>200.12</MaxAmountToApprove><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>AUSYD</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			approval2.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(xml);

			var job = TestObjectCreator.CreateJob("Job0000001", TestObjectCreator.LocalClient, 1M, TestObjectCreator.Agent2, 1M);
			var approval3 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval3.XP_ParentTableCode = "JH";
			approval3.XP_ParentID = job.PK;
			xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><MaxAmountToApprove>200.12</MaxAmountToApprove><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>AUD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			approval3.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(xml);

			Factory.Save();

			var resultTable = RunScript();

			var headers = new[] { "ReferenceNumber", "BranchCode", "ApprovalStatus", "ApprovalRequestDescription", "CreatingUser", "ApprovingUser", "ApprovingUser2", "ChargeCodes" };

			var lines = new[]
			{
				new object[] { "00001000", "BNE", "REQ", "", "E", "", "", "ABIGAS USD FIN : 300.22" },
				new object[] { "Consol01", "BNE", "REQ", "", "E", "", "", "ABIGAS USD FIN : 100.10, AUSYD USD FIN : 200.12" },
				new object[] { "Job0000001", "BNE", "REQ", "", "E", "", "", "ABIGAS AUD FIN : 200.12, ABIGAS USD FIN : 100.10" }
			};

			AssertDataTableAllRowsByKeyColumns("TransactionsExportBatching", resultTable, headers, lines);
		}

		public void TestInvalidCharactersInXMLApprovalDataDoesNotThrowException()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Invoice01", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var approval1 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval1.XP_ParentTableCode = "AH";
			approval1.XP_ParentID = invoice.PK;
			string xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><MaxAmountToApprove>200.12</MaxAmountToApprove><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			approval1.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(xml);

			var consol = TestObjectCreator.CreateConsol("CNSHA", "ITROM", "Consol01");
			var approval2 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval2.XP_ParentTableCode = "VK";
			approval2.XP_ParentID = consol.PK;
			xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><MaxAmountToApprove>200.12</MaxAmountToApprove><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>AUSYD</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			approval2.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(xml);

			var job = TestObjectCreator.CreateJob("Job0000001", TestObjectCreator.LocalClient, 1M, TestObjectCreator.Agent2, 1M);
			var approval3 = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval3.XP_ParentTableCode = "JH";
			approval3.XP_ParentID = job.PK;
			xml = "\uFEFF<?xml version=\"1.0\" encoding=\"utf-16\"?><PostingRequest><PostingOption>PostLocalClientCharges</PostingOption><MaxAmountToApprove><><>></MaxAmountToApprove><ArrayOfChargeDetails xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>FRT</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>USD</SellCurrency><OSSellAmount>100.1</OSSellAmount><LocalSellAmount>100.1</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails><ChargeDetails><JobNumber>S00001234</JobNumber><ChargeCode>BAF</ChargeCode><Branch>SYD</Branch><Department>FES</Department><SellAccount>ABIGAS</SellAccount><SellCurrency>AUD</SellCurrency><OSSellAmount>200.12</OSSellAmount><LocalSellAmount>200.12</LocalSellAmount><InvoiceType>FIN</InvoiceType></ChargeDetails></ArrayOfChargeDetails></PostingRequest>";
			approval3.XP_ApprovalRequestData = Encoding.Unicode.GetBytes(xml);

			Factory.Save();

			var resultTable = RunScript();

			var headers = new[] { "ReferenceNumber", "BranchCode", "ApprovalStatus", "ApprovalRequestDescription", "CreatingUser", "ApprovingUser", "ApprovingUser2", "ChargeCodes" };

			var lines = new[]
			{
				new object[] { "00001000", "BNE", "REQ", "", "E", "", "", "ABIGAS USD FIN : 300.22" },
				new object[] { "Consol01", "BNE", "REQ", "", "E", "", "", "ABIGAS USD FIN : 100.10, AUSYD USD FIN : 200.12" },
				new object[] { "Job0000001", "BNE", "REQ", "", "E", "", "", "" } //No Charge Details could be extracted from the xml approval data, as it contains illegal xml character.
			};

			AssertDataTableAllRowsByKeyColumns("TransactionsExportBatching", resultTable, headers, lines);
		}

		DataTable RunScript()
		{
			var sql = string.Format(@"
SELECT * FROM Report_ARCreditNoteApproval(
	'{0}'
)",
							GlbCompany.CurrentCompany.PK           //@CompanyPK
	);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
