using Enterprise.Customs.Module.Testing;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(EntryHeaderOperationalActionSupporter))]
	sealed class EntryHeaderOperationalActionSupporterTest : EntryHeaderOperationalActionSupporterAbstractTest<EntryHeaderOperationalActionSupporter>
	{
		public void TestPopulateMethods()
		{
			var supporter = new EntryHeaderOperationalActionSupporter();
			var allIds = supporter.Methods.GetAllIds();
			AssertCollectionContains(ActionMethodProviderIDs.CNCusEntry, allIds);
		}
	}
}
