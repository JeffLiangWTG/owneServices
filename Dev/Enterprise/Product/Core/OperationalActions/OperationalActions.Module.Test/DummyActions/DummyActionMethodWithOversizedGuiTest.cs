using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(DummyActionMethodWithOversizedGui))]
	sealed class DummyActionMethodWithOversizedGuiTest : OperationalActionMethodTest<DummyActionMethodWithOversizedGui>
	{
		#region Implementation

		protected override DummyActionMethodWithOversizedGui NewMethod()
		{
			return new DummyActionMethodWithOversizedGui();
		}

		#endregion
	}
}
