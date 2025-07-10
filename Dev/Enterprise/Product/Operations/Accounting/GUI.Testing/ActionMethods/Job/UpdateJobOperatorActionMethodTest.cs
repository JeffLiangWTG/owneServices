using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobOperatorActionMethod))]
	internal sealed class UpdateJobOperatorActionMethodTest : OperationalActionMethodTest<UpdateJobOperatorActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Assign Job To Another Operator", Method.Name);
			AssertEquals("Assign Job To Another Operator", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateJobOperatorActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		protected override UpdateJobOperatorActionMethod NewMethod()
		{
			return new UpdateJobOperatorActionMethod();
		}
	}
}
