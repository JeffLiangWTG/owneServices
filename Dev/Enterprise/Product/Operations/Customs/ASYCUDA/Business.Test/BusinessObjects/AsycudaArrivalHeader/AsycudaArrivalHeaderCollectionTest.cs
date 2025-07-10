using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ASYCUDA.Business.Testing.AsycudaManifestHeaderBaseOnlyTest;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalHeaderCollection<>))]
	sealed class AsycudaArrivalHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>>
	{
		public void TestNewArrivalHeaderIsPersisted()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			var arrivalHeader = manifest.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_VoyageFlightNo = "FL001";
			arrivalHeader.ATH_Reference = "ABC";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var manifestReloaded = newFactory.Load<AsycudaManifestHeaderForTest>(manifest.PK);
			var arrivalHeaderReloaded = manifestReloaded.ArrivalHeaders[0];
			AssertType<AsycudaArrivalHeader>(arrivalHeaderReloaded);
			AssertEquals("FL001", arrivalHeaderReloaded.ATH_VoyageFlightNo);
			AssertEquals("ABC", arrivalHeaderReloaded.ATH_Reference);
		}

		public void TestLoadWithClusterKey()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			for (var i = 0; i < 500; i++)
			{
				_ = manifest.ArrivalHeaders.AddNew();
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var manifestReloaded = newFactory.Load<AsycudaManifestHeader>(manifest.PK);
			_ = newFactory.Load<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_ClusterKey, manifestReloaded.AMA_ClusterKey));
			AssertEquals(1, newFactory.GetTableHitCount(AsycudaArrivalHeader.Schema.TableName));
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>);

		protected override AsycudaArrivalHeaderCollection<AsycudaArrivalHeader> GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
			return new AsycudaArrivalHeaderCollection<AsycudaArrivalHeader>(header);
		}
	}
}
