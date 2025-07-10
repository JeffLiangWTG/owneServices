using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LandedCostingGroup))]
	sealed class LandedCostingGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestILandedCostPreference()
		{
			BizObj.GroupID = 5;
			BizObj.GroupName = "TEST";
			BizObj.CostDistributionCode = "WGT";
			AssertEquals("GroupID", (ZByte)5, ((ILandedCostPreference)BizObj).LCGroupID);
			AssertEquals("LCGroupName", BizObj.GroupName, ((ILandedCostPreference)BizObj).LCGroupName);
			AssertEquals("DistributionBy", BizObj.CostDistributionCode, ((ILandedCostPreference)BizObj).DistributionBy);
		}

		public void TestCharges()
		{
			ChargeGroupAndChargeCode charge = BizObj.Charges.AddNew();

			AssertEquals("Charges.ParentLandedCostingGroup", BizObj, BizObj.Charges.ParentLandedCostingGroup);
			AssertEquals("Charges.Factory", BizObj.Factory, BizObj.Charges.Factory);
			AssertEquals("Charges.CurrentFallbackLevel", BizObj.CurrentFallbackLevel, charge.CurrentFallbackLevel);
		}

		public void TestCostDistributionDescription()
		{
			BizObj.CostDistributionList.AddPair("=_=", "=_=;");
			BizObj.CostDistributionCode = "=_=";
			AssertEquals("CostDistributionDescription", "=_=;", BizObj.CostDistributionDescription);
		}

		public void TestCostDistributionList()
		{
			AssertEquals("CostDistributionList.Count", 5, BizObj.CostDistributionList.Count);
			AssertEquals("CostDistributionList should contain \"AWV\".", true, BizObj.CostDistributionList.ContainsCode("AWV"));
		}

		#region Validation Tests

		public void TestValidateGroupID_ValueIsGreaterThanZero()
		{
			AssertNoErrors("Precondition: GroupID should not have errors.", BizObj.GroupIDInfo);

			BizObj.GroupID = 0;
			AssertHasError(BizObj.GroupIDInfo, string.Format("Please enter a '{0}' greater than 0.", BizObj.GroupIDInfo.HumanReadableName));

			BizObj.GroupID = 1;
			AssertNoErrors(BizObj.GroupIDInfo);
		}

		public void TestValidateGroupID_ValueIsUniqueInCollection()
		{
			LandedCostingGroup landedCostingGroup1 = new LandedCostingGroup(null, Factory);
			LandedCostingGroup landedCostingGroup2 = new LandedCostingGroup(null, Factory);
			LandedCostingGroup landedCostingGroup3 = new LandedCostingGroup(null, Factory);

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			collection.Add(landedCostingGroup1);
			collection.Add(landedCostingGroup2);
			collection.Add(landedCostingGroup3);

			AssertNoErrors("Precondition: LandedCostingGroup1.GroupID should not have errors.", landedCostingGroup1.GroupIDInfo);
			AssertNoErrors("Precondition: LandedCostingGroup2.GroupID should not have errors.", landedCostingGroup2.GroupIDInfo);
			AssertNoErrors("Precondition: LandedCostingGroup3.GroupID should not have errors.", landedCostingGroup3.GroupIDInfo);

			landedCostingGroup1.GroupID = 1;
			landedCostingGroup2.GroupID = 1;
			landedCostingGroup3.GroupID = 2;

			AssertNoErrors(landedCostingGroup1.GroupIDInfo);
			AssertHasError(landedCostingGroup2.GroupIDInfo, string.Format("The {0} has been duplicated and must be unique.", landedCostingGroup2.GroupIDInfo.HumanReadableName));
			AssertNoErrors(landedCostingGroup3.GroupIDInfo);

			landedCostingGroup2.GroupID = 3;
			AssertNoErrors(landedCostingGroup2.GroupIDInfo);
		}

		public void TestValidateGroupName()
		{
			AssertNoErrors("Precondition: GroupName should not have errors.", BizObj.GroupNameInfo);

			BizObj.GroupName = "";
			AssertHasError(BizObj.GroupNameInfo, "Please enter a value.");

			BizObj.GroupName = "Group Name";
			AssertNoErrors(BizObj.GroupNameInfo);
		}

		public void TestValidateCostDistributionCode()
		{
			AssertNoErrors("Precondition: CostDistributionCode should not have errors.", BizObj.CostDistributionDescriptionInfo);

			BizObj.CostDistributionCode = "";
			AssertHasError(BizObj.CostDistributionCodeInfo, "Please enter a value.");

			BizObj.CostDistributionCode = "!@#";
			AssertHasError(BizObj.CostDistributionCodeInfo, "Enter a valid selection.");

			BizObj.CostDistributionList.AddPair("=_=", "");
			BizObj.CostDistributionCode = "=_=";
			AssertNoErrors(BizObj.CostDistributionCodeInfo);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.GroupID = 0;
			BizObj.GroupName = "";
			BizObj.CostDistributionCode = "";

			BizObj.ClearAllNotifications();

			AssertNoErrors("Precondition: BizObj should not have errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated GroupID.", BizObj.GroupIDInfo);
			AssertHasErrors("RunPreSaveValidation() should have validated GroupName.", BizObj.GroupNameInfo);
			AssertHasErrors("RunPreSaveValidation() should have validated CostDistributionCode.", BizObj.CostDistributionCodeInfo);
		}

		#endregion

		#region Implementation

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			LandedCostingGroup originalGroup = (LandedCostingGroup)originalBusinessObject;
			LandedCostingGroup newGroup = (LandedCostingGroup)newBusinessObject;

			AssertEquals("NewBusinessObject.Charges.Count", originalGroup.Charges.Count, newGroup.Charges.Count);
			AssertEquals("NewBusinessObject.Charges.ParentLandedCostingGroup", newGroup, newGroup.Charges.ParentLandedCostingGroup);

			if (!isClone)
			{
				AssertEquals("NewBusinessObject.CurrentFallbackLevel", originalGroup.CurrentFallbackLevel, newGroup.CurrentFallbackLevel);
			}

			for (int i = 0; i < originalGroup.Charges.Count; ++i)
			{
				CheckAllPropertiesInZPropertyInfoHashAreEqual(originalGroup.Charges[i], newGroup.Charges[i]);
			}
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.GroupID = 1;
			BizObj.GroupName = "Group 1";
			BizObj.CostDistributionCode = "ABC";

			ChargeGroupAndChargeCode charge1 = BizObj.Charges.AddNew();
			ChargeGroupAndChargeCode charge2 = BizObj.Charges.AddNew();

			charge1.ChargeGroupCode = "ABC";
			charge2.ChargeGroupCode = "XYZ";

			charge1.ChargeCodePK = ZGuid.NewZGuid();
			charge2.ChargeCodePK = ZGuid.NewZGuid();

			charge1.IsExcluded = true;
			charge2.IsExcluded = false;

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new LandedCostingGroup BizObj
		{
			get { return (LandedCostingGroup)base.BizObj; }
		}

		#endregion
	}
}
