using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LandedCostingGroupCollection))]
	sealed class LandedCostingGroupCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<LandedCostingGroupCollection>
	{
		public void TestGetLandedCostingGroupNameFromID()
		{
			AssertEquals("Group Name with ID", "", Collection.GetLandedCostingGroupNameFromID(1));
			LandedCostingGroup lCGroup = Collection.AddNew();
			AssertEquals("Group Name with ID", "", Collection.GetLandedCostingGroupNameFromID(1));
			lCGroup.GroupID = 1;
			lCGroup.GroupName = "TEST";
			AssertEquals("Group Name with ID", "TEST", Collection.GetLandedCostingGroupNameFromID(1));
		}

		public void TestGetLandedCostingDistributionCodeFromID()
		{
			AssertEquals("Group Distribution Code", "", Collection.GetLandedCostingGroupDistributionCodeFromID(1));
			LandedCostingGroup lCGroup = Collection.AddNew();
			AssertEquals("Group Distribution Code", "", Collection.GetLandedCostingGroupDistributionCodeFromID(1));
			lCGroup.GroupID = 1;
			lCGroup.CostDistributionCode = "AWV";
			AssertEquals("Group Distribution Code", "AWV", Collection.GetLandedCostingGroupDistributionCodeFromID(1));
		}

		public void TestIndexerAndAddNew()
		{
			LandedCostingGroup landedCostingGroup1 = Collection.AddNew();
			LandedCostingGroup landedCostingGroup2 = Collection.AddNew();

			AssertEquals("Collection[0]", landedCostingGroup1, Collection[0]);
			AssertEquals("Collection[1]", landedCostingGroup2, Collection[1]);
		}

		public void TestGetLandedCostingGroupFromChargeCode()
		{
			BusinessObject chargeCode = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));
			chargeCode[AccChargeCodeSchema.Constants.AC_ChargeGroup] = new ZString("FRT");

			LandedCostingGroup landedCostingGroup1 = Collection.AddNew();
			LandedCostingGroup landedCostingGroup2 = Collection.AddNew();

			ChargeGroupAndChargeCode charge1 = landedCostingGroup1.Charges.AddNew();
			ChargeGroupAndChargeCode charge2 = landedCostingGroup2.Charges.AddNew();

			charge1.ChargeGroupCode = "FRT";
			charge2.ChargeGroupCode = "ORG";

			AssertEquals("GetLandedCostingGroupFromChargeCode() should have returned LandedCostingGroup1.", landedCostingGroup1, Collection.GetLandedCostingGroupFromChargeCode(chargeCode, landedCostingGroup2));

			charge1.ChargeGroupCode = "XYZ";
			AssertEquals("GetLandedCostingGroupFromChargeCode() should have returned null.", null, Collection.GetLandedCostingGroupFromChargeCode(chargeCode, landedCostingGroup2));

			charge2.ChargeGroupCode = "FRT";
			AssertEquals("GetLandedCostingGroupFromChargeCode() should have returned LandedCostingGroup2.", landedCostingGroup2, Collection.GetLandedCostingGroupFromChargeCode(chargeCode, landedCostingGroup1));
			AssertEquals("GetLandedCostingGroupFromChargeCode() should have returned null.", null, Collection.GetLandedCostingGroupFromChargeCode(chargeCode, landedCostingGroup2));

			charge2.ChargeCodePK = chargeCode.PK;
			charge2.IsExcluded = true;
			AssertEquals("GetLandedCostingGroupFromChargeCode() should have returned null.", null, Collection.GetLandedCostingGroupFromChargeCode(chargeCode, landedCostingGroup1));

			charge2.ChargeGroupCode = "ORG";
			charge2.IsExcluded = false;
			AssertEquals("GetLandedCostingGroupFromChargeCode() should have returned LandedCostingGroup2.", landedCostingGroup2, Collection.GetLandedCostingGroupFromChargeCode(chargeCode, landedCostingGroup1));
		}

		public void TestGetLandedCostingGroupFromChargeGroup()
		{
			LandedCostingGroup landedCostingGroup1 = Collection.AddNew();
			LandedCostingGroup landedCostingGroup2 = Collection.AddNew();

			ChargeGroupAndChargeCode charge1 = landedCostingGroup1.Charges.AddNew();
			ChargeGroupAndChargeCode charge2 = landedCostingGroup2.Charges.AddNew();

			charge1.ChargeGroupCode = "ABC";
			charge2.ChargeGroupCode = "ABC";

			AssertEquals("GetLandedCostingGroupFromChargeGroup() should have returned LandedCostingGroup1.", landedCostingGroup1, Collection.GetLandedCostingGroupFromChargeGroup("ABC", landedCostingGroup2));
			AssertEquals("GetLandedCostingGroupFromChargeGroup() should have returned LandedCostingGroup2.", landedCostingGroup2, Collection.GetLandedCostingGroupFromChargeGroup("ABC", landedCostingGroup1));
			AssertEquals("GetLandedCostingGroupFromChargeGroup() should have returned null.", null, Collection.GetLandedCostingGroupFromChargeGroup("XYZ", landedCostingGroup1));
		}

		#region Implementation

		protected override LandedCostingGroupCollection GetCollectionToTest()
		{
			return new LandedCostingGroupCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LandedCostingGroup(null, Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
