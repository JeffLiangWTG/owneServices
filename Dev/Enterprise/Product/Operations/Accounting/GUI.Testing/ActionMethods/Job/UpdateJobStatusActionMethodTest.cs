using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobStatusActionMethod))]
	internal sealed class UpdateJobStatusActionMethodTest : OperationalActionMethodTest<UpdateJobStatusActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update Job Status", Method.Name);
			AssertEquals("Update Job Status", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateJobStatusActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		protected override UpdateJobStatusActionMethod NewMethod()
		{
			return new UpdateJobStatusActionMethod();
		}
	}
}
