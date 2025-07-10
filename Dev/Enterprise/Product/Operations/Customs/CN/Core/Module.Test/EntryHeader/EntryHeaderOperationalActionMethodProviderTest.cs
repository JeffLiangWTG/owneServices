using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(EntryHeaderOperationalActionMethodProvider))]
	class EntryHeaderOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.CNCusEntry;
	}
}
