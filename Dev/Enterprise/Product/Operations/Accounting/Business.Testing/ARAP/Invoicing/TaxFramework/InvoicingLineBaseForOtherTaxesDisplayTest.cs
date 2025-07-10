using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	[TestedType(typeof(InvoicingLineBaseForOtherTaxesDisplay))]
	public class InvoicingLineBaseForOtherTaxesDisplayTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTaxRecordParentPassedAsParameter()
		{
			var collection = objForTest.OtherTaxesLinkedToTransactionLinesCollection;
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			ITaxRecordParent taxRecordParentAsParam = null;
			taxProcessorMock.Setup(t => t.DeleteTaxRecordNotInDB(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction>())).Callback<ITaxRecordParent, AccTaxTransaction>((t1, t2) => taxRecordParentAsParam = t1);
			collection.Delete(null);
			AssertEquals(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice), taxRecordParentAsParam);
		}

		public void TestOtherTaxesLinkedToTransactionLinesCollection()
		{
			var transactionLine = objForTest as TransactionLineForOtherTaxesDisplay;
			var collection = transactionLine.OtherTaxesLinkedToTransactionLinesCollection;
			AssertType<OtherTaxesLinkedToTransactionLinesCollection>(collection);
			AssertEquals(line, collection.Relationship.Master);
		}

		public void TestTransactionLineForOtherTaxesDisplayValues()
		{
			line.AL_Desc = ZString.Empty;
			line.AL_AC = creator.CC1.PK;
			line.AL_GB = creator.NonCurrentCompanyBranch.PK;
			line.AL_GE = creator.FEADepartment.PK;
			line.AL_GovtChargeCode = "G002";

			line.AL_RX_NKTransactionCurrency = creator.TWD.Code;
			line.AL_ExchangeRate = 2m;
			line.AL_OSExTaxAmount = 90;
			line.AL_OSTaxAmount = 10m;
			line.AL_OverseasTotal = 100m;
			line.AL_LocalExTaxAmount = 45m;
			line.AL_LocalTaxAmount = 5m;

			var transactionLineForOtherTaxesDisplay = objForTest as TransactionLineForOtherTaxesDisplay;
			AssertEquals(creator.CC1.AC_Code, transactionLineForOtherTaxesDisplay.ChargeCode);
			AssertEquals(creator.CC1.AC_Desc, transactionLineForOtherTaxesDisplay.ChargeCodeDescription);
			AssertEquals(creator.NonCurrentCompanyBranch.GB_Code, transactionLineForOtherTaxesDisplay.Branch);
			AssertEquals(creator.FEADepartment.GE_Code, transactionLineForOtherTaxesDisplay.Department);
			AssertEquals(ZString.Empty, transactionLineForOtherTaxesDisplay.Job);
			AssertEquals("G002", transactionLineForOtherTaxesDisplay.GovtChargeCode);

			AssertEquals("TWD", transactionLineForOtherTaxesDisplay.Currency);

			AssertEquals(90m, transactionLineForOtherTaxesDisplay.OSExTaxAmount);
			AssertEquals(10m, transactionLineForOtherTaxesDisplay.OSTaxAmount);
			AssertEquals(100m, transactionLineForOtherTaxesDisplay.OSTotalAmount);
			AssertEquals(0, transactionLineForOtherTaxesDisplay.OSCurrencyDecimals);

			AssertEquals(45m, transactionLineForOtherTaxesDisplay.LocalExTaxAmount);
			AssertEquals(5m, transactionLineForOtherTaxesDisplay.LocalTaxAmount);
			AssertEquals(50m, transactionLineForOtherTaxesDisplay.LocalTotalAmount);
			AssertEquals(2, transactionLineForOtherTaxesDisplay.LocalCurrencyDecimals);

			var shipment = creator.CreateShipment("S001", "NZAKL", "AUSYD");
			var job = creator.CreateJob(shipment, false);
			job.JH_JobNum = "J000001";
			line.AL_JH = job.PK;

			AssertEquals("J000001", transactionLineForOtherTaxesDisplay.Job);
		}

		public void TestSupplyTypeValue()
		{
			line.AL_SupplyType = "SPT";
			var transactionLineForOtherTaxesDisplay = objForTest as TransactionLineForOtherTaxesDisplay;
			AssertEquals("SPT", objForTest.SupplyType);
		}

		public void TestTaxBranchValue()
		{
			AssertEquals("TaxBranch property on invoice line is not set", ZString.Empty, objForTest.TaxBranch);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "ABC";
			line.AL_GB_TaxBranch = branch.PK;
			AssertEquals("TaxBranch property on invoice line is set", "ABC", objForTest.TaxBranch);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			line = creator.CreateInvoiceLine(invoice, 100M);
			objForTest = TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line);
			return objForTest;
		}

		protected override void SetUp()
		{
			creator = new TestObjectCreator(Factory);
			invoice = creator.CreateInvoice(typeof(APInvoice));
			line = creator.CreateInvoiceLine(invoice, 100M);
			objForTest = TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(line);
		}

		InvoicingLineBaseForOtherTaxesDisplay objForTest;
		InvoicingBase invoice;
		InvoicingLineBase line;
		TestObjectCreator creator;
	}
}
