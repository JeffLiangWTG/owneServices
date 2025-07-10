using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TrainingZoneRateCollection))]
	public class TrainingZoneRateCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TrainingZoneRateCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TrainingZoneRateCollection GetCollectionToTest()
		{
			return new TrainingZoneRateCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			TrainingZoneRate result = new TrainingZoneRate();
			result.DummyPKToEnsureUniquenessForTest = ZGuid.NewZGuid();
			return result;
		}

		#endregion

		#region AddNew

		public new void TestAddNew()
		{
			TrainingZoneRateCollection collection = new TrainingZoneRateCollection();
			TrainingZoneRate rate = collection.AddNew();

			AssertEquals(1, collection.Count);
			AssertEquals(rate, collection[0]);
		}

		#endregion

		#region SetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			TrainingZoneRateCollection collection = new TrainingZoneRateCollection();
			TrainingZoneRate rate = collection.AddNew();
			AssertEquals(collection, rate.ParentCollection);

			TrainingZoneRateCollection collection2 = new TrainingZoneRateCollection();
			collection.Equals(collection2);
		}

		#endregion

		#region TestFindByZone

		public void TestFindByZone()
		{
			EDIRefZoneHeader zone1 = Factory.New<EDIRefZoneHeader>();
			EDIRefZoneHeader zone2 = Factory.New<EDIRefZoneHeader>();

			TrainingZoneRateCollection collection = new TrainingZoneRateCollection();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			AssertEquals(null, collection.FindByZone(zone1));

			TrainingZoneRate rate = collection.AddNew();
			rate.ZonePK = zone1.PK;

			AssertEquals(rate, collection.FindByZone(zone1));
			AssertEquals(null, collection.FindByZone(zone2));
		}

		#endregion
	}
}
