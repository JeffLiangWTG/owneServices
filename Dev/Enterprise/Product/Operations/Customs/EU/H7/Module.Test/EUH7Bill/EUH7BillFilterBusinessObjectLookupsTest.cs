using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7BillFilterBusinessObjectLookups))]
	public class EUH7BillFilterBusinessObjectLookupsTest : TestCaseWithFactory
	{
		public void TestCustomsStatusList()
		{
			var parent = new EUH7BillFilterBusinessObject();
			var lookups = new EUH7BillFilterBusinessObjectLookups(parent);

			AssertType<AISEntryStatusList>(lookups.CustomsStatusList);
		}
	}
}
