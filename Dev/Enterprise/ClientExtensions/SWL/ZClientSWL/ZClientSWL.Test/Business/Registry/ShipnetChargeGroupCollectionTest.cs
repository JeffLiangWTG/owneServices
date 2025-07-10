using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	[TestedType(typeof(ShipnetChargeGroupCollection))]
	public class ShipnetChargeGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ShipnetChargeGroupCollection>
	{
		public void TestIndexedEntryType()
		{
			Entries.AddNew();
			AssertEquals("Type of indexed entry", typeof(ShipnetChargeGroup), Entries[0].GetType());
		}

		public void TestNewEntryType()
		{
			ShipnetChargeGroup entry = Entries.AddNew();
			AssertEquals("Type of new entry", typeof(ShipnetChargeGroup), entry.GetType());
		}

		public void TestIsDuplicateEntry()
		{
			ShipnetChargeGroup entry1 = Entries.AddNew();
			entry1.ChargeGroupCode = "Code";
			AssertEquals("Should not be duplicate", false, Entries.IsDuplicateEntry(entry1));
			ShipnetChargeGroup entry2 = Entries.AddNew();
			entry2.ChargeGroupCode = entry1.ChargeGroupCode;
			AssertEquals("Should be duplicate", true, Entries.IsDuplicateEntry(entry1));
			entry2.ChargeGroupCode = "Code2";
			AssertEquals("Should not be duplicate", false, Entries.IsDuplicateEntry(entry1));
		}

		#region Implementation
		#region Overrides
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipnetChargeGroup(SetupBusinessObject, Factory);
		}

		protected override ShipnetChargeGroupCollection GetCollectionToTest()
		{
			return new ShipnetChargeGroupCollection(SetupBusinessObject, Factory);
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupBusinessObject = new ShipnetSetupBusinessObject(Factory);
			SetupBusinessObject.IsShipnetCarrier = true;
			Entries = new ShipnetChargeGroupCollection(SetupBusinessObject, Factory);
		}

		#endregion
		ShipnetSetupBusinessObject SetupBusinessObject;
		ShipnetChargeGroupCollection Entries;
		#endregion
	}
}
