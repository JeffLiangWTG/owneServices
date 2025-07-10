using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UpdateJobOverseasAgentActionMethod))]
	internal sealed class UpdateJobOverseasAgentActionMethodTest : OperationalActionMethodTest<UpdateJobOverseasAgentActionMethod>
	{
		public void TestTestNameAndDescription()
		{
			AssertEquals("Update Overseas Agent", Method.Name);
			AssertEquals("Update Overseas Agent", Method.Description);
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(typeof(UpdateJobOverseasAgentActionMethodApplicator), Method.NewApplicator(Factory, null).GetType());
		}

		protected override UpdateJobOverseasAgentActionMethod NewMethod()
		{
			return new UpdateJobOverseasAgentActionMethod();
		}
	}
}
