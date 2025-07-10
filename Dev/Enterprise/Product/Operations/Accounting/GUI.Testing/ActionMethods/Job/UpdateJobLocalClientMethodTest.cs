using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobLocalClientActionMethod))]
	internal sealed class UpdateJobLocalClientActionMethodTest : OperationalActionMethodTest<UpdateJobLocalClientActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update Local Client", Method.Name);
			AssertEquals("Update Local Client", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateJobLocalClientActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		protected override UpdateJobLocalClientActionMethod NewMethod()
		{
			return new UpdateJobLocalClientActionMethod();
		}
	}
}
