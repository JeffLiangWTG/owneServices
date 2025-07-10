using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsOperationalActionMethodProvider))]
	sealed class B2AdjustmentsOperationalActionMethodProviderTest : Enterprise.Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.B2Adjustments;
	}
}
