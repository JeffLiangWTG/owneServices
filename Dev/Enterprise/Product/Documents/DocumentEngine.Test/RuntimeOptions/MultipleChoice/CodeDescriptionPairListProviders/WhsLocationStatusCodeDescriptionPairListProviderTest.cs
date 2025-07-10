using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class WhsLocationStatusCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new WhsLocationStatusCodeDescriptionPairListProvider();
		}

		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var whsLocationStatusPairList = (CodeDescriptionPairList)ObjectFactory.Get<ILocationStatus>();
			whsLocationStatusPairList.RemoveCode("VOI");
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), whsLocationStatusPairList);
		}

		#endregion

		#region TestVoidLocationStatusNotReturnedToTheList

		public void TestVoidLocationStatusNotReturnedToTheList()
		{
			var locationStatusList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			AssertEquals(false, locationStatusList.ContainsCode("VOI"));
		}

		#endregion
	}
}
