using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(DummyActionMethodWithGui))]
	sealed class DummyActionMethodWithGuiTest : OperationalActionMethodTest<DummyActionMethodWithGui>
	{
		#region Implementation

		protected override DummyActionMethodWithGui NewMethod()
		{
			return new DummyActionMethodWithGui();
		}

		#endregion
	}
}
