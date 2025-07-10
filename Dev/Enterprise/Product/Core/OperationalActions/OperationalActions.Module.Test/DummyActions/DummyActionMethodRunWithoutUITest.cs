using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	[TestedType(typeof(DummyActionMethodRunWithoutUI))]
	sealed class DummyActionMethodRunWithoutUITest : OperationalActionMethodTest<DummyActionMethodRunWithoutUI>
	{
		#region Implementation

		protected override DummyActionMethodRunWithoutUI NewMethod()
		{
			return new DummyActionMethodRunWithoutUI();
		}

		#endregion
	}
}
