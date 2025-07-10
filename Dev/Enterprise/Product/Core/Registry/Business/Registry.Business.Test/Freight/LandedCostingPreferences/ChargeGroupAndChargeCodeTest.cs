using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeGroupAndChargeCode))]
	sealed class ChargeGroupAndChargeCodeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestChargeCodePK()
		{
			Charge.CurrentFallbackLevel = null;
			Charge.ChargeCodePK = ZGuid.Empty;
			Charge.ChargeCodeCodeForDefaultValue = "PS";

			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompany.PK);
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, "PS");
			ZGuid chargeCodePK = ((BusinessObject)Factory.LoadTop1<MasterFiles.Integration.IAccChargeCode>(filter)).PK;

			AssertEquals("ChargeCodePK", ZGuid.Empty, Charge.ChargeCodePK);

			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("ChargeCodePK", chargeCodePK, Charge.ChargeCodePK);

			ZGuid newGuid = ZGuid.NewZGuid();

			Charge.ChargeCodePK = newGuid;
			AssertEquals("ChargeCodePK", newGuid, Charge.ChargeCodePK);
			AssertEquals("ChargeCodeCodeForDefaultValue", "", Charge.ChargeCodeCodeForDefaultValue);

			Charge.ChargeCodeCodeForDefaultValue = "!@#";
			AssertEquals("ChargeCodePK", ZGuid.Empty, Charge.ChargeCodePK);
		}

		public void TestChargeGroupDescription()
		{
			Charge.ChargeGroupList.AddPair("!@#", "!@#$%");
			Charge.ChargeGroupCode = "!@#";
			AssertEquals("ChargeGroupDescription", "!@#$%", Charge.ChargeGroupDescription);
		}

		public void TestChargeCodeDescription()
		{
			BusinessObject chargeCode = AddAndReturnNewAccChargeCode("A New Description", "", ZGuid.Empty);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertEquals("ChargeCodeDescription", "A New Description", Charge.ChargeCodeDescription);
		}

		public void TestChargeGroupList()
		{
			CodeDescriptionPairList chargeGroupList = Charge.ChargeGroupList;
			AssertEquals("ChargeGroupList.Count", 9, chargeGroupList.Count);
			AssertEquals("ChargeGroupList should contain \"BRK\".", true, chargeGroupList.ContainsCode("BRK"));
		}

		public void TestChargeCodeList()
		{
			AssertEquals("Enterprise.MasterFiles.Business.AccChargeCodeCollection", Charge.ChargeCodeList.GetType().FullName);
			AssertNotNull("ChargeCodeList.Factory should not be null.", Charge.ChargeCodeList.Factory);
		}

		#region Validation Tests

		public void TestValidateChargeGroupCode_ValueIsValid()
		{
			AssertNoErrors("Precondition: ChargeGroupCode should not have errors.", Charge.ChargeGroupCodeInfo);

			Charge.ChargeGroupCode = "!@#";
			AssertHasError(Charge.ChargeGroupCodeInfo, "Enter a valid selection.");

			Charge.ChargeGroupCode = "";
			AssertNoErrors(Charge.ChargeGroupCodeInfo);
		}

		public void TestValidateChargeCodePK_ValueIsInList()
		{
			AssertNoErrors("Precondition: ChargeCodePK should not have errors.", Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertHasError(Charge.ChargeCodePKInfo, "Enter a valid selection.");

			BusinessObject chargeCode = AddAndReturnNewAccChargeCode("", "", ZGuid.Empty);
			Charge.ChargeCodePK = chargeCode.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.Empty;
			AssertNoErrors(Charge.ChargeCodePKInfo);
		}

		public void TestValidateChargeCodePK_ValueCanOrCannotBeExcluded()
		{
			BusinessObject chargeCode1 = AddAndReturnNewAccChargeCode("", "FRT", ZGuid.Empty);
			BusinessObject chargeCode2 = AddAndReturnNewAccChargeCode("", "ORG", ZGuid.Empty);

			AssertNoErrors("Precondition: ChargeCodePK should not have errors.", Charge.ChargeCodePKInfo);

			Charge.IsExcluded = true;
			Charge.ChargeGroupCode = "FRT";
			Charge.ChargeCodePK = chargeCode1.PK;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = chargeCode2.PK;
			AssertHasError(Charge.ChargeCodePKInfo, "You cannot exclude this Charge Code as it is not part of the Charge Group specified.");

			Charge.IsExcluded = false;
			AssertNoErrors(Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = chargeCode1.PK;
			AssertHasError(Charge.ChargeCodePKInfo, "You cannot include this Charge Code as it is already part of the Charge Group specified, and therefore already included.");
		}

		public void TestValidatedIsExcluded()
		{
			AssertEquals("Precondition: ChargeGroupCode should be empty.", "", Charge.ChargeGroupCode);
			AssertEquals("Precondition: ChargeCodePK should be empty.", ZGuid.Empty, Charge.ChargeCodePK);
			AssertEquals("Precondition: IsExcluded should be false.", false, Charge.IsExcluded);
			AssertNoErrors("Precondition: IsExcluded should not have errors.", Charge.IsExcludedInfo);

			Charge.IsExcluded = true;
			AssertHasError(Charge.IsExcludedInfo, "You cannot exclude a Charge unless you specify a Charge Group and a Charge Code.");

			Charge.ChargeGroupCode = "ABC";
			Charge.ChargeCodePK = ZGuid.NewZGuid();
			AssertNoErrors(Charge.IsExcludedInfo);

			Charge.ChargeGroupCode = "";
			AssertHasError(Charge.IsExcludedInfo, "You cannot exclude a Charge unless you specify a Charge Group and a Charge Code.");

			Charge.ChargeGroupCode = "ABC";
			Charge.ChargeCodePK = ZGuid.Empty;
			AssertHasError(Charge.IsExcludedInfo, "You cannot exclude a Charge unless you specify a Charge Group and a Charge Code.");

			Charge.IsExcluded = false;
			AssertNoErrors(Charge.IsExcludedInfo);
		}

		public void TestValidateChargeExistsInOneLandedCostingGroupOnly()
		{
			BusinessObject chargeCode = AddAndReturnNewAccChargeCode("", "FRT", ZGuid.Empty);

			LandedCostingGroupCollection landedCostingGroups = new LandedCostingGroupCollection();

			LandedCostingGroup landedCostingGroup1 = landedCostingGroups.AddNew();
			LandedCostingGroup landedCostingGroup2 = landedCostingGroups.AddNew();

			landedCostingGroup1.GroupID = 1;
			landedCostingGroup2.GroupID = 2;

			landedCostingGroup1.GroupName = "Group 1";
			landedCostingGroup2.GroupName = "Group 2";

			ChargeGroupAndChargeCode charge1 = landedCostingGroup1.Charges.AddNew();
			ChargeGroupAndChargeCode charge2 = landedCostingGroup2.Charges.AddNew();

			charge1.ChargeGroupCode = "FRT";
			charge2.ChargeGroupCode = "ORG";

			charge1.IsExcluded = false;
			charge2.IsExcluded = false;
			charge1.ChargeCodePK = chargeCode.PK;
			charge2.ChargeCodePK = chargeCode.PK;

			AssertEquals("There should be no row errors on Charge1.", false, charge1.HasRowErrors);
			AssertEquals("There should be a row error on Charge2 because ChargeCodePK already exists in Charge1.", true, charge2.HasRowErrors);
			AssertEquals("Charge2.Notifications.Errors[0]", "Error - record: This Charge Group/Code is already included in Landed Costing Group 1 (Group 1).", charge2.Notifications.GetErrors().GetFirstMessage());

			charge1.IsExcluded = true;
			charge1.ChargeCodePK = chargeCode.PK;

			charge2.ValidateChargeCodePK();
			AssertEquals("There should be no row errors on Charge1.", false, charge1.HasRowErrors);
			AssertEquals("There should be no row errors on Charge2.", false, charge2.HasRowErrors);
		}

		public void TestRunPreSaveValidation()
		{
			Charge.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			Charge.ChargeGroupCode = "!@#";
			Charge.ChargeCodePK = ZGuid.Invalid;
			Charge.IsExcluded = true;

			Charge.ClearAllNotifications();

			AssertNoErrors("Precondition: Charge should not have errors.", Charge);

			Charge.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated ChargeGroupCode.", Charge.ChargeGroupCodeInfo);
			AssertHasErrors("RunPreSaveValidation() should have validated ChargeCodePK.", Charge.ChargeCodePKInfo);

			Charge.ChargeCodePK = ZGuid.Empty;
			Charge.ClearAllNotifications();
			Charge.RunPreSaveValidation();
			AssertHasErrors("RunPreSaveValidation() should have validated IsExcluded.", Charge.IsExcludedInfo);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ParentLandedCostingGroupCollection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = ParentLandedCostingGroupCollection.AddNew();
			Charge = landedCostingGroup.Charges.AddNew();
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("NewBusinessObject.ChargeCodeCodeForDefaultValue", ((ChargeGroupAndChargeCode)originalBusinessObject).ChargeCodeCodeForDefaultValue, ((ChargeGroupAndChargeCode)newBusinessObject).ChargeCodeCodeForDefaultValue);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			Charge.ChargeGroupCode = "ABC";
			Charge.ChargeCodePK = ZGuid.NewZGuid();
			Charge.IsExcluded = true;

			return Charge;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		BusinessObject AddAndReturnNewAccChargeCode(string aC_Desc, string aC_ChargeGroup, ZGuid aC_GC)
		{
			BusinessObject result = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(MasterFiles.Integration.IAccChargeCode)));

			if (!string.IsNullOrEmpty(aC_Desc))
			{
				result[AccChargeCodeSchema.Constants.AC_Desc] = new ZString(aC_Desc);
			}

			if (!string.IsNullOrEmpty(aC_ChargeGroup))
			{
				result[AccChargeCodeSchema.Constants.AC_ChargeGroup] = new ZString(aC_ChargeGroup);
			}

			if (aC_GC != ZGuid.Empty)
			{
				result[AccChargeCodeSchema.Constants.AC_GC] = aC_GC;
			}

			Factory.Save();

			return result;
		}

		LandedCostingGroupCollection ParentLandedCostingGroupCollection;
		ChargeGroupAndChargeCode Charge;

		#endregion
	}
}
