using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Accounting.Business.WIPAccrual.Testing
{
	public class BaseWIPAccrualReverseValidationTest : TransactionLineEmptyValidation_InnerTest
	{
		public void AssertCheckAL_ReverseDate_WithReverseWipAccrualBusinessContext(bool isAccrual)
		{
			ErrorReporter.Clear();
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var shipment = testObjectCreator.CreateShipment("S001001", false);
			var job = testObjectCreator.CreateJob(shipment, false, false);
			job.JH_GC = Env.CurrentCompanyPK;
			job.JH_GB = Env.CurrentBranchPK;
			var wipAccrual = isAccrual ? testObjectCreator.CreateAccrual(job) : (BaseWIPAccrual)testObjectCreator.CreateWIP(job);
			wipAccrual.Reverse();
			Factory.Save();
			Assert("Valid Reverse date", wipAccrual.AL_ReverseDate.IsValid);
			AssertNoErrors("No Validation error", wipAccrual.AL_ReverseDateInfo);
			AssertEquals("No Exception error", 0, ExceptionReporterTestListener.Instance.Count);
			Assert("PreCondition: Wip or Accrual is not Reversing", !wipAccrual.IsReversing);
			wipAccrual.AL_ReverseDate = ZDateTime.Today.AddDays(-1);
			wipAccrual.AL_Desc += "line needs to have changes to save and report";
			Factory.Save();
			Assert("Has exception error message", ErrorReporter.LastMessageReported.Contains("has Reverse Date"));
			ErrorReporter.Clear();
			wipAccrual.IsReversing = true;
			Assert("PreCondition: Wip or Accrual is Reversing", wipAccrual.IsReversing);
			wipAccrual.AL_ReverseDate = ZDateTime.Today.AddDays(-3);
			wipAccrual.Validation.ValidateAL_ReverseDate();
			AssertEquals("No more Developer exception if the BusinessContext is set.", 0, ErrorReporter.TotalErrorCount);
			AssertHasError(wipAccrual.AL_ReverseDateInfo, "This transaction has already been reversed.");
		}

		public void TestCheckWIPAL_ReverseDate_WithReverseWipAccrualBusinessContext()
		{
			AssertCheckAL_ReverseDate_WithReverseWipAccrualBusinessContext(false);
		}

		public void TestCheckAccrualAL_ReverseDate_WithReverseWipAccrualBusinessContext()
		{
			AssertCheckAL_ReverseDate_WithReverseWipAccrualBusinessContext(true);
		}

		public void TestCheckAL_ReverseDate_WhenReverseDateIsEmpty()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S001001", false);
			var job = testObjectCreator.CreateJob(shipment, false, false);
			var wip = testObjectCreator.CreateWIP(job);
			Factory.Save();
			wip.SetModeToReversing();
			AssertNoError(wip.AL_ReverseDateInfo, "Please enter a Reverse Date.");
			wip.AL_ReverseDate = ZDateTime.Empty;
			AssertHasError(wip.AL_ReverseDateInfo, "Please enter a Reverse Date.");
		}
	}
}
