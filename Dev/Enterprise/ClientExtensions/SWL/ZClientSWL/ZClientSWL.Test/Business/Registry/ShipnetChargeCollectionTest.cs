using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	[TestedType(typeof(ShipnetChargeCollection))]
	public class ShipnetChargeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ShipnetChargeCollection>
	{
		public override void TestAddNew()
		{
			base.TestAddNew();
			ShipnetCharge charge = Charges.AddNew();
			AssertEquals("Default PK", ZGuid.Empty, charge.ChargePK);
			ZGuid newPK = ZGuid.NewZGuid();
			charge = Charges.AddNew(newPK);
			AssertEquals("Default PK", newPK, charge.ChargePK);
		}

		public void TestIndexedEntryType()
		{
			Charges.AddNew();
			AssertEquals("Type of indexed entry", typeof(ShipnetCharge), Charges[0].GetType());
		}

		public void TestNewEntryType()
		{
			ShipnetCharge charge = Charges.AddNew();
			AssertEquals("Type of new entry", typeof(ShipnetCharge), charge.GetType());
		}

		public void TestIsDuplicateCharge()
		{
			ShipnetCharge charge1 = Charges.AddNew();
			charge1.ChargePK = ZGuid.NewZGuid();
			AssertEquals("Should not be duplicate", false, Charges.IsDuplicateCharge(charge1));
			ShipnetCharge charge2 = Charges.AddNew();
			charge2.ChargePK = charge1.ChargePK;
			AssertEquals("Should be duplicate", true, Charges.IsDuplicateCharge(charge1));
			charge2.ChargePK = ZGuid.NewZGuid();
			AssertEquals("Should not be duplicate", false, Charges.IsDuplicateCharge(charge1));
		}

		public void TestContainsChargePK()
		{
			ShipnetCharge charge = Charges.AddNew();
			ZGuid chargePK = ZGuid.NewZGuid();
			AssertEquals("Should not contain ChargePK", false, Charges.ContainsChargePK(chargePK));
			charge.ChargePK = chargePK;
			AssertEquals("Should contain ChargePK", true, Charges.ContainsChargePK(chargePK));
			Charges.RemoveAll();
			AssertEquals("Should not contain ChargePK", false, Charges.ContainsChargePK(chargePK));
		}

		#region Implementation
		#region Overrides
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipnetCharge(SetupBusinessObject, Factory);
		}

		protected override ShipnetChargeCollection GetCollectionToTest()
		{
			return new ShipnetChargeCollection(SetupBusinessObject, Factory);
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
			Charges = new ShipnetChargeCollection(SetupBusinessObject, Factory);
		}

		#endregion
		ShipnetSetupBusinessObject SetupBusinessObject;
		ShipnetChargeCollection Charges;
		#endregion
	}
}
