using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var container = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			Factory.Save();

			pack.Pivot.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (container.IsDeleted)
				{
					Assert("This is wrong Pivot should not be deleted after the Container", false);
				}
			};

			container.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaContainer)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink)));
		}

		public void TestHeaderAndClusterKey()
		{
			var container = Factory.New<AsycudaContainer>();
			AssertNull(container.Header);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			container.ACN_AMA_Manifest = manifestHeader.PK;
			AssertEquals(0 , container.ACN_ClusterKey);
			AssertEquals(manifestHeader.PK, container.Header.PK);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving.", 1, container.ACN_ClusterKey);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			return container;
		}
	}
}
