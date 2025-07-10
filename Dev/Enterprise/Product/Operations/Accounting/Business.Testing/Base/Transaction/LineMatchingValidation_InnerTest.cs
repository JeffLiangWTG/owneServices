using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class LineMatchingValidation_InnerTest : TestCaseWithFactory
	{
		public void TestCheckPaidAmount()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			new IMatchingCollection(Factory) { invoice };
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			ILineMatching lineAsILineMatching = line;
			AssertNoErrors("Prerequisite", lineAsILineMatching.PaidAmountInfo);

			IMatching thisAsIMatching = invoice;
			ISupportMatchingOfMyLines thisAsISupportMatchingOfMyLines = invoice;
			invoice.AH_LocalOutstandingAmount = 50m;
			((ILineMatching)invoice.Lines[0]).SetDefaultValues();
			((ILineMatching)invoice.Lines[0]).PaidAmount = 5m;

			thisAsIMatching.PartiallyPay();
			thisAsIMatching.GenerateMatchLinks();
			thisAsISupportMatchingOfMyLines.GenerateTransLinePayRecords(ZGuid.NewZGuid());

			line.AL_OSAmount = 10m;
			invoice.AH_OutstandingAmount = 5m;
			lineAsILineMatching.SetDefaultValues();

			lineAsILineMatching.PaidAmount = 20m;
			AssertHasError("Case 1", lineAsILineMatching.PaidAmountInfo, "This value cannot be greater than the value of the transaction line");

			lineAsILineMatching.PaidAmount = -1;
			AssertHasError("Case 2", lineAsILineMatching.PaidAmountInfo, "This value cannot be greater than the value of the transaction line");

			lineAsILineMatching.PaidAmount = 10m;
			AssertHasError("Case 3", lineAsILineMatching.PaidAmountInfo, "This value cannot be greater than the outstanding amount of the transaction line");

			lineAsILineMatching.PaidAmount = 5m;
			AssertNoErrors("Case 4", lineAsILineMatching.PaidAmountInfo);
		}

		public void TestCheckAL_GB()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "Test001";
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			line.AL_GB = branch.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			new IMatchingCollection(Factory) { invoice };
			AssertNoErrors("Prerequisite", line.AL_GBInfo);

			branch.GB_IsActive = false;
			line.Validation.ValidateAll();
			AssertNoErrors("Validation must not validate Branch", line.AL_GBInfo);
		}

		public void TestBranchDepartmentCombinationValidation_LineMatchingValidation()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "Test001";
			new IMatchingCollection(Factory) { invoice };
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, line,
				() => { line.Validation.ValidateAll(); }, line.AL_GEInfo);
		}
	}
}
