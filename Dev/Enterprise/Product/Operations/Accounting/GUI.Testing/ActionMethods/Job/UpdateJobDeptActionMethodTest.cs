using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobDeptActionMethod))]
	internal sealed class UpdateJobDeptActionMethodTest : OperationalActionMethodTest<UpdateJobDeptActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update Job Department", Method.Name);
			AssertEquals("Update Job Department", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateJobDeptActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		protected override UpdateJobDeptActionMethod NewMethod()
		{
			return new UpdateJobDeptActionMethod();
		}
	}
}
