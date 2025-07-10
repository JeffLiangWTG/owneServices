using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil.Testing
{
	class EInvoicingDataValidatorForBrazilTest : BaseEInvoicingDataValidatorTest
	{
		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForBrazil(GlbCompany.CurrentCompany);
		}

		public void TestCheck_ComplianceNumberIsRequired()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			invoice.AH_TransactionReference = string.Empty;

			var aRInvoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.VND, 1m, "desc", 100m);
			aRInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched);

			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Should have error message 'Compliance number is missing.'", "Compliance number is missing.", pivot.AIP_ErrorDescription);

			pivot.AIP_ErrorDescription = string.Empty;
			AssertEquals("ErrorDescription is empty", ZString.Empty, pivot.AIP_ErrorDescription);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = invoice.PK;
			taxTransaction.ATT_TaxSystemCode = "ISS";
			taxTransaction.TaxConfiguration.ETC_Code = "TTT";
			taxTransaction.TaxConfiguration.ETC_TaxAuthorityCode = "IMCPQ";

			batch = TestObjectCreator.CreateEInvoicingBatch(101, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
		}
	}
}
