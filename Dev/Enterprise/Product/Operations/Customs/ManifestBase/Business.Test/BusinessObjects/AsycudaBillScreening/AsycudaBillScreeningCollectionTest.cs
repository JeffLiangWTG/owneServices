using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBillWithIAsycudaBillScreeningTypeSupporter>))]
	class AsycudaBillScreeningCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadWithClusterKey()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();
			var clusterKey = header.AMA_ClusterKey;
			for (var i = 0; i < 10; i++)
			{
				var bill = header.Bills.AddNew();
				var billScreening = Factory.New<AsycudaBillScreening>();
				billScreening.ASR_ABL = bill.PK;
				billScreening.ASR_ClusterKey = clusterKey;
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			var bills = newFactory.Load<AsycudaBillWithIAsycudaBillScreeningTypeSupporter>(new ZQuery(AsycudaBillSchema.ABL_ClusterKey, clusterKey));
			newFactory.Load<AsycudaBillScreening>(new ZQuery(AsycudaBillScreeningSchema.ASR_ClusterKey, clusterKey));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ AsycudaBillSchema.Constants.TableName, 1 },
				{ AsycudaBillScreeningSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				foreach (var bill in bills)
				{
					new AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBillWithIAsycudaBillScreeningTypeSupporter>(bill).Load();
				}
			}
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBillWithIAsycudaBillScreeningTypeSupporter>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBillWithIAsycudaBillScreeningTypeSupporter>();
			header.Bills.Add(bill);
			return new AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBillWithIAsycudaBillScreeningTypeSupporter>(bill);
		}
	}

	class AsycudaBillWithIAsycudaBillScreeningTypeSupporter : AsycudaBill
		, IAsycudaBillScreeningTypeSupporter
	{
		public AsycudaBillWithIAsycudaBillScreeningTypeSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Type GetAsycudaBillScreeningType() => typeof(AsycudaBillScreening);
	}
}
