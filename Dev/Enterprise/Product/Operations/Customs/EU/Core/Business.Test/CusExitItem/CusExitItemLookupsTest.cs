using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			var lookups = new CusExitItemLookups(exitItem);

			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.StatusList, lookups.StatusList);
				NUnit.Framework.Assert.That(lookups.StatusList.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExitItemStatusList)));
			});
		}

		[ExpectNoExceptions]
		public void TestWeightUQList()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			var exitItem = exitDetail.CusExitItems.AddNew();
			var weightUnits = exitItem.Lookups.WeightUQList;

			var list = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

			NUnit.Framework.Assert.That(weightUnits, NUnit.Framework.Is.SameAs(list));
			NUnit.Framework.Assert.That(weightUnits.ContainsCode(Core.Constants.Weight.Kilograms), NUnit.Framework.Is.True);
		}
	}
}
