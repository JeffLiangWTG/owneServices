using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaPackCollection<AsycudaPack, AsycudaBill>))]
	sealed class AsycudaPackCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCorrectTypeIsUsedInAddNew()
		{
			var businessObject = Factory.New<AsycudaBill>();
			var billMock = new Mock<AsycudaBill>(Factory, ((INeedRow)businessObject).Row);
			billMock.CallBase = true;

			var pack = Factory.New<AsycudaPack>();
			var packType = pack.GetType();

			billMock.Protected().Setup<Type>("GetPackTypeCore").Returns(packType);
			var bill = billMock.Object;
			AssertEquals(packType, bill.Packs.TypeOfElements);
			AssertType(packType, bill.Packs.AddNew());
		}

		public void TestPackedItemCreatedWhenAddingAPack()
		{
			using (ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.FillWithValidTestData();
				header.AMA_JobReference = "HKD3232432";
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				header = newFactory.Load<AsycudaManifestHeader>(header.PK);
				var bill = header.Bills.AddNew();
				var pack = bill.Packs.AddNew();
				var packCountries = pack.LoadChildren<AsycudaPackPackedItemPivot>(AsycudaPackPackedItemPivotSchema.APP_APA_Pack);
				AssertEquals(1, packCountries.Length);
			}
		}

		public void TestLoadWithClusterKey()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			for (var i = 0; i < 500; i++)
			{
				var bill = header.Bills.AddNew();
				_ = bill.Packs.AddNew();
				_ = bill.Packs.AddNew();
			}
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			var clusterKey = headerReloaded.AMA_ClusterKey;
			_ = newFactory.Load<AsycudaBill>(new ZQuery(AsycudaBillSchema.ABL_ClusterKey, clusterKey));
			_ = newFactory.Load<AsycudaPack>(new ZQuery(AsycudaPackSchema.APA_ClusterKey, clusterKey));
			foreach (AsycudaBill bill in headerReloaded.Bills)
			{
				var packCollection = new AsycudaPackCollection<AsycudaPack, AsycudaBill>(bill);
				packCollection.Load();
			}
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaBill.Schema.TableName));
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaPack.Schema.TableName));
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaPackCollection<AsycudaPack, AsycudaBill>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return (AsycudaPackCollection<AsycudaPack, AsycudaBill>)bill.Packs;
		}
	}
}
