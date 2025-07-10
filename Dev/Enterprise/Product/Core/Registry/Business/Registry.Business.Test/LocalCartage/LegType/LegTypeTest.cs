using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LegType))]
	sealed class LegTypeTest : RegistryBusinessObjectTestCaseBase
	{
		#region MaxLength

		public void TestPickupFromOrgMaxLength()
		{
			AssertEquals("PickupFromOrgInfo.MaxLength", 3, BizObj.PickupFromOrgInfo.MaxLength);
		}

		public void TestWaitPointOrgMaxLength()
		{
			AssertEquals("WaitPointOrgInfo.MaxLength", 3, BizObj.WaitPointOrgInfo.MaxLength);
		}

		public void TestDeliverToOrgMaxLength()
		{
			AssertEquals("DeliverToOrgInfo.MaxLength", 3, BizObj.DeliverToOrgInfo.MaxLength);
		}

		public void TestMovementTypeMaxLength()
		{
			AssertEquals("MovementTypeInfo.MaxLength", 3, BizObj.MovementTypeInfo.MaxLength);
		}

		public void TestContainerisedGroupMaxLength()
		{
			AssertEquals("ContainerisedInfo.MaxLength", 3, BizObj.ContainerisedInfo.MaxLength);
		}

		public void TestEquipmentGroupMaxLength()
		{
			AssertEquals("EquipmentGroupInfo.MaxLength", 3, BizObj.EquipmentGroupInfo.MaxLength);
		}

		#endregion

		#region Validation

		public void TestValidateDescription()
		{
			Assert("Precondition: Description should have no errors", !BizObj.DescriptionInfo.HasErrors());

			BizObj.Description = (NoResString)"";
			Assert("Description should have an error if it is empty", BizObj.DescriptionInfo.HasError("Please enter a Description."));

			BizObj.Description = (NoResString)"abc";
			Assert("Description should not have an error if it is not empty", !BizObj.DescriptionInfo.HasErrors());
		}

		public void TestValidatePickupFromOrg()
		{
			TestListValidation(BizObj.PickupFromOrgInfo, BizObj.OrgType_List, true);
		}

		public void TestValidateWaitPointOrg()
		{
			//TestListValidation(BizObj.WaitPointOrgInfo, BizObj.OrgType_List, false);

			BizObj.EquipmentGroup = Constants.FCLEquipmentNeeded.WaitForUnpack;
			TestListValidation(BizObj.WaitPointOrgInfo, BizObj.OrgType_List, true);
		}

		public void TestValidateDeliverToOrg()
		{
			TestListValidation(BizObj.DeliverToOrgInfo, BizObj.OrgType_List, true);
		}

		public void TestValidateMovementType()
		{
			TestListValidation(BizObj.MovementTypeInfo, BizObj.MovementType_List, true);
		}

		public void TestValidateContainerised()
		{
			TestListValidation(BizObj.ContainerisedInfo, BizObj.Containerised_List, true);
		}

		public void TestValidateEquipmentGroup()
		{
			BizObj.WaitPointOrg = "ABC";
			TestListValidation(BizObj.EquipmentGroupInfo, BizObj.EquipmentGroup_List, false);

			BizObj.WaitPointOrg = "";
			BizObj.EquipmentGroup = Constants.FCLEquipmentNeeded.WaitForUnpack;
			Assert("EquipmentGroupInfo should have an error if is Wait and no WaitPointOrg entered", BizObj.EquipmentGroupInfo.HasError("A Wait Point Organization is needed for this Equipment Group."));

			BizObj.WaitPointOrg = "ABC";
			BizObj.EquipmentGroup = Constants.FCLEquipmentNeeded.WaitForUnpack;
			Assert("EquipmentGroupInfo should have no error", !BizObj.EquipmentGroupInfo.HasErrors());

			BizObj.WaitPointOrg = "";
			BizObj.EquipmentGroup = Constants.FCLEquipmentNeeded.WaitForUnpack;
			Assert("EquipmentGroupInfo should have an error if is Wait and no WaitPointOrg entered", BizObj.EquipmentGroupInfo.HasError("A Wait Point Organization is needed for this Equipment Group."));

			BizObj.EquipmentGroup = Constants.FCLEquipmentNeeded.SideLoader;
			Assert("EquipmentGroupInfo should have no error", !BizObj.EquipmentGroupInfo.HasErrors());
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.PickupFromOrg = "!@#";
			BizObj.WaitPointOrg = "!@#";
			BizObj.DeliverToOrg = "!@#";
			BizObj.MovementType = "!@#";
			BizObj.Containerised = "!@#";
			BizObj.EquipmentGroup = "!@#";
			BizObj.ClearAllNotifications();

			Assert("Precondition: BizObj should not have errors", !BizObj.HasErrors);

			BizObj.RunPreSaveValidation();
			Assert("Code should have errors", BizObj.CodeInfo.HasErrors());
			Assert("Description should have errors", BizObj.DescriptionInfo.HasErrors());
			Assert("PickupFromOrg should have errors", BizObj.PickupFromOrgInfo.HasErrors());
			Assert("WaitPointOrg should have errors", BizObj.WaitPointOrgInfo.HasErrors());
			Assert("DeliverToOrg should have errors", BizObj.DeliverToOrgInfo.HasErrors());
			Assert("MovementType should have errors", BizObj.MovementTypeInfo.HasErrors());
			Assert("Containerised should have errors", BizObj.ContainerisedInfo.HasErrors());
			Assert("EquipmentGroup should have errors", BizObj.EquipmentGroupInfo.HasErrors());
		}

		#region Test Validation methods

		void TestListValidation(ZPropertyInfo propertyInfo, CodeDescriptionPairList list, bool isMandatory)
		{
			AssertEquals(propertyInfo.Name + ".MaxLength", list.MaxCodeLength, propertyInfo.MaxLength);
			Assert("Precondition: " + propertyInfo.Name + " should have no errors", !propertyInfo.HasErrors());
			AssertNotNullOrEmpty("Precondition: List[0].Code should not be empty", list[0].Code);

			propertyInfo.Value = (ZString)"%$%";
			Assert(propertyInfo.Name + " should have an error if it is invalid", propertyInfo.HasError("Enter a valid selection."));

			propertyInfo.Value = (ZString)list[0].Code;
			Assert(propertyInfo.Name + " should not have an error if it is valid", !propertyInfo.HasErrors());

			propertyInfo.Value = (ZString)"";
			AssertEquals(propertyInfo.Name + " should not have an error if not mandatory", isMandatory, propertyInfo.HasErrors());
		}

		#endregion

		#endregion

		#region Test ReadOnly

		public void TestReadOnly()
		{
			BizObj.IsSystemDefined = false;
			AssertEquals("Code should be editable", false, BizObj.CodeInfo.ReadOnly);
			AssertEquals("Description should be editable", false, BizObj.DescriptionInfo.ReadOnly);
			AssertEquals("PickupFromOrg should be editable", false, BizObj.PickupFromOrgInfo.ReadOnly);
			AssertEquals("WaitPointOrg should be editable", false, BizObj.WaitPointOrgInfo.ReadOnly);
			AssertEquals("DeliverToOrg should be editable", false, BizObj.DeliverToOrgInfo.ReadOnly);
			AssertEquals("MovementType should be editable", false, BizObj.MovementTypeInfo.ReadOnly);
			AssertEquals("Containerised should be editable", false, BizObj.ContainerisedInfo.ReadOnly);
			AssertEquals("EquipmentGroup should be editable", false, BizObj.EquipmentGroupInfo.ReadOnly);
			AssertEquals("IsSystemDefined should be ReadOnly always", true, BizObj.IsSystemDefinedInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals("Code should be ReadOnly", true, BizObj.CodeInfo.ReadOnly);
			AssertEquals("Description should be ReadOnly", true, BizObj.DescriptionInfo.ReadOnly);
			AssertEquals("PickupFromOrg should be ReadOnly", true, BizObj.PickupFromOrgInfo.ReadOnly);
			AssertEquals("WaitPointOrg should be ReadOnly", true, BizObj.WaitPointOrgInfo.ReadOnly);
			AssertEquals("DeliverToOrg should be ReadOnly", true, BizObj.DeliverToOrgInfo.ReadOnly);
			AssertEquals("MovementType should be ReadOnly", true, BizObj.MovementTypeInfo.ReadOnly);
			AssertEquals("Containerised should be ReadOnly", true, BizObj.ContainerisedInfo.ReadOnly);
			AssertEquals("EquipmentGroup should be editable", false, BizObj.EquipmentGroupInfo.ReadOnly);
			AssertEquals("IsSystemDefined should be ReadOnly always", true, BizObj.IsSystemDefinedInfo.ReadOnly);

			BizObj.IsSystemDefined = false;
			AssertEquals("Code should be editable", false, BizObj.CodeInfo.ReadOnly);
			AssertEquals("Description should be editable", false, BizObj.DescriptionInfo.ReadOnly);
			AssertEquals("PickupFromOrg should be editable", false, BizObj.PickupFromOrgInfo.ReadOnly);
			AssertEquals("WaitPointOrg should be editable", false, BizObj.WaitPointOrgInfo.ReadOnly);
			AssertEquals("DeliverToOrg should be editable", false, BizObj.DeliverToOrgInfo.ReadOnly);
			AssertEquals("MovementType should be editable", false, BizObj.MovementTypeInfo.ReadOnly);
			AssertEquals("Containerised should be editable", false, BizObj.ContainerisedInfo.ReadOnly);
			AssertEquals("EquipmentGroup should be editable", false, BizObj.EquipmentGroupInfo.ReadOnly);
			AssertEquals("IsSystemDefined should be ReadOnly always", true, BizObj.IsSystemDefinedInfo.ReadOnly);
		}

		#endregion

		#region Test Lists

		public void TestEquipmentGroup_List()
		{
			AssertEquals("EquipmentGroup_List.Count", 4, BizObj.EquipmentGroup_List.Count);
			AssertEquals("GetDescriptionFromCode - LiftOffOn", FCLEquipmentNeededList.Descriptions.LiftOffOn, BizObj.EquipmentGroup_List.GetDescriptionFromCode(Constants.FCLEquipmentNeeded.LiftOffOn));
			AssertEquals("GetDescriptionFromCode - Side Loaded", FCLEquipmentNeededList.Descriptions.SideLoader, BizObj.EquipmentGroup_List.GetDescriptionFromCode(Constants.FCLEquipmentNeeded.SideLoader));
			AssertEquals("GetDescriptionFromCode - Trailer", FCLEquipmentNeededList.Descriptions.Trailer, BizObj.EquipmentGroup_List.GetDescriptionFromCode(Constants.FCLEquipmentNeeded.Trailer));
			AssertEquals("GetDescriptionFromCode - Wait", FCLEquipmentNeededList.Descriptions.WaitForUnpack, BizObj.EquipmentGroup_List.GetDescriptionFromCode(Constants.FCLEquipmentNeeded.WaitForUnpack));
		}

		#endregion

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.PickupFromOrgInfo, "PickupFromOrg");
			TestZPropertyInfo(BizObj.WaitPointOrgInfo, "WaitPointOrg");
			TestZPropertyInfo(BizObj.DeliverToOrgInfo, "DeliverToOrg");
			TestZPropertyInfo(BizObj.MovementTypeInfo, "MovementType");
			TestZPropertyInfo(BizObj.ContainerisedInfo, "Containerised");
			TestZPropertyInfo(BizObj.EquipmentGroupInfo, "EquipmentGroup");
			TestZPropertyInfo(BizObj.IsSystemDefinedInfo, "IsSystemDefined");
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.IsSystemDefined = true;
			BizObj.Code = "ABC";
			BizObj.Description = (NoResString)"Desc";
			BizObj.PickupFromOrg = "CTO";
			BizObj.WaitPointOrg = "CFS";
			BizObj.DeliverToOrg = "CNE";
			BizObj.MovementType = Constants.CartageDirection.Destination;
			BizObj.Containerised = "FCL";
			BizObj.EquipmentGroup = "TM1";

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new LegType BizObj
		{
			get { return (LegType)base.BizObj; }
		}

		#endregion
	}
}
