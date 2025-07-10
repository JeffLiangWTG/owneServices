using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoDepotMultiMessageManagerTest : CMRSeaCargoDepotTestCase
	{
		public void TestUnderbondParentWithNoHeader()
		{
			var consol = CreateDepotJobOBL250805001();
			var containerWrapper = CFSContainerWrapper.Load(consol.Containers[0]);
			containerWrapper.Underbonds.AddNew();

			var manager = new SeaCargoDepotMultiMessageManagerForTest(containerWrapper);
			AssertNotNull(manager.ExposedGetAllMessageManager());
		}

		sealed class SeaCargoDepotMultiMessageManagerForTest : SeaCargoDepotMultiMessageManager
		{
			public SeaCargoDepotMultiMessageManagerForTest(ICusUnderbondUnionCollectionParent underbondParent) : base(underbondParent)
			{
			}

			internal Customs.Business.SingleMessageManager[] ExposedGetAllMessageManager() => GetAllMessageManagers();
		}
	}
}
