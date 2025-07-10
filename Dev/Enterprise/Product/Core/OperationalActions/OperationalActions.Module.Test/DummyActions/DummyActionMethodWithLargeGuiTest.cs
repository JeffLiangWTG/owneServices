using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(DummyActionMethodWithLargeGui))]
	sealed class DummyActionMethodWithLargeGuiTest : OperationalActionMethodTest<DummyActionMethodWithLargeGui>
	{
		#region Implementation

		protected override DummyActionMethodWithLargeGui NewMethod()
		{
			return new DummyActionMethodWithLargeGui();
		}

		#endregion
	}
}
