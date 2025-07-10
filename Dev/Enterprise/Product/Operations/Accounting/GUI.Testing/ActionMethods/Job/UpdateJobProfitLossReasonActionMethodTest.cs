using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobProfitLossReasonActionMethod))]
	internal sealed class UpdateJobProfitLossReasonActionMethodTest : OperationalActionMethodTest<UpdateJobProfitLossReasonActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Updates Job Profit / Loss Reason Code", Method.Name);
			AssertEquals("Updates Job Profit / Loss Reason Code", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateJobProfitLossReasonActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		protected override UpdateJobProfitLossReasonActionMethod NewMethod()
		{
			return new UpdateJobProfitLossReasonActionMethod();
		}
	}
}
