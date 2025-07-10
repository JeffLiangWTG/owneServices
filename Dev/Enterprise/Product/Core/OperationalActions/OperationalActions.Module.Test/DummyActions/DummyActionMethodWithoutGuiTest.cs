using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(DummyActionMethodWithoutGui))]
	sealed class DummyActionMethodWithoutGuiTest : OperationalActionMethodTest<DummyActionMethodWithoutGui>
	{
		#region Implementation

		protected override DummyActionMethodWithoutGui NewMethod()
		{
			return new DummyActionMethodWithoutGui();
		}

		#endregion
	}
}
