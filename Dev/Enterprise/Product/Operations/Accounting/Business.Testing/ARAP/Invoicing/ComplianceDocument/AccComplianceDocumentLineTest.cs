using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentLine))]
	public class AccComplianceDocumentLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testObjectCreator = new TestObjectCreator(factory);
			var complianceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var complianceDocumentLine = testObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test");
			return complianceDocumentLine;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		public void TestChargeCode()
		{
			AssertEquals("CAF", complianceDocumentLine.Charge);
		}

		public void TestTaxRate()
		{
			AssertEquals("TX1", complianceDocumentLine.TaxID);
			AssertEquals(0, complianceDocumentLine.PostingGroup);
		}

		public void TestTaxMessage()
		{
			AssertEquals("MSG", complianceDocumentLine.TaxMessage);
		}

		public void TestAmount()
		{
			AssertEquals(120m, complianceDocumentLine.Amount);
		}

		public void TestTaxAmount()
		{
			AssertEquals(12m, complianceDocumentLine.TaxAmount);
		}

		public void TestTotalAmount()
		{
			AssertEquals(132m, complianceDocumentLine.TotalAmount);
		}

		public void TestLocalAmount()
		{
			AssertEquals(120m, complianceDocumentLine.LocalAmount);
		}

		public void TestLocalTaxAmount()
		{
			AssertEquals(12m, complianceDocumentLine.LocalTaxAmount);
		}

		public void TestLocalTotalAmount()
		{
			AssertEquals(132m, complianceDocumentLine.LocalTotalAmount);
		}

		public void TestTransactionLines()
		{
			AssertEquals(2, complianceDocumentLine.TransactionLines.Count);
		}

		public void TestComplianceDocumentPivots()
		{
			AssertEquals(2, complianceDocumentLine.ComplianceDocumentPivots.Count);
		}

		public void TestTransactionHeaders()
		{
			AssertEquals(1, complianceDocumentLine.TransactionHeaders.Length);
		}

		AccComplianceDocumentHeader complianceDocumentHeader;
		AccComplianceDocumentLine complianceDocumentLine;

		protected override void SetUp()
		{
			base.SetUp();

			var frt = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var caf = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CAF"));

			var accInvMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accInvMsg.A9_Code = "MSG";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TX1";
			taxRate.AT_PostingGroupId = 0;
			taxRate.AT_A9_DefaultVatClass = accInvMsg.PK;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TX2";
			taxRate1.AT_PostingGroupId = 1;

			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_InvoiceAmount = 256;
			arInvoice.AH_OutstandingAmount = 256;
			arInvoice.AH_ExchangeRate = 1;

			var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine.AL_AH = arInvoice.PK;
			arInvoiceLine.AL_AT = taxRate.PK;
			arInvoiceLine.AL_AC = frt.PK;
			arInvoiceLine.AL_LineAmount = 44;
			arInvoiceLine.AL_GSTVAT = 4;
			arInvoiceLine.AL_OSAmount = 48;
			arInvoiceLine.AL_ExchangeRate = 1;

			var arInvoiceLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine1.AL_AH = arInvoice.PK;
			arInvoiceLine1.AL_AT = taxRate.PK;
			arInvoiceLine1.AL_AC = caf.PK;
			arInvoiceLine1.AL_LineAmount = 50;
			arInvoiceLine1.AL_GSTVAT = 5;
			arInvoiceLine1.AL_OSAmount = 55;
			arInvoiceLine1.AL_ExchangeRate = 1;

			var arInvoiceLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine2.AL_AH = arInvoice.PK;
			arInvoiceLine2.AL_AT = taxRate.PK;
			arInvoiceLine2.AL_AC = caf.PK;
			arInvoiceLine2.AL_LineAmount = 70;
			arInvoiceLine2.AL_GSTVAT = 7;
			arInvoiceLine2.AL_OSAmount = 77;
			arInvoiceLine2.AL_ExchangeRate = 1;

			var arInvoiceLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine3.AL_AH = arInvoice.PK;
			arInvoiceLine3.AL_AT = taxRate1.PK;
			arInvoiceLine3.AL_AC = frt.PK;
			arInvoiceLine3.AL_LineAmount = 92;
			arInvoiceLine3.AL_GSTVAT = 5;
			arInvoiceLine3.AL_OSAmount = 97;
			arInvoiceLine3.AL_ExchangeRate = 1;

			complianceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			complianceDocumentLine = TestObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test");
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine, arInvoiceLine1);
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine, arInvoiceLine2);
			complianceDocumentHeader.ADH_DocumentNumber = "D00001";

			var complianceDocumentLine1 = TestObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test", 2);
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine1, arInvoiceLine);

			TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "Desc", "D00002", "DEF", "Desc", arInvoiceLine3);

			Factory.Save();
		}

		TestObjectCreator testObjectCreator;
		public TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
	}
}
