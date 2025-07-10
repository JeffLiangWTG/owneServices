using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	[TestedType(typeof(ShipnetChargeGroup))]
	public class ShipnetChargeGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateCode()
		{
			ShipnetChargeGroupCollection entries = new ShipnetChargeGroupCollection(SetupBusinessObject, Factory);
			ShipnetChargeGroup entry = entries.AddNew();
			entries.RunPreSaveValidation();
			AssertHasErrorContaining(entry.ChargeGroupCodeInfo, "Please enter a value.");
			entry.ChargeGroupCode = "Code";
			entries.RunPreSaveValidation();
			AssertHasErrorContaining(entry.ChargeGroupCodeInfo, "No charge code is entered for this Group 'Code'.");
			entry.Charges.AddNew();
			entries.RunPreSaveValidation();
			AssertEquals("HasErrors", false, entry.ChargeGroupCodeInfo.HasErrors());
			ShipnetChargeGroup entry2 = entries.AddNew();
			entry2.ChargeGroupCode = "Code";
			entry2.Charges.AddNew();
			entries.RunPreSaveValidation();
			AssertHasErrorContaining(entry2.ChargeGroupCodeInfo, "Duplicate Codes are entered.");
			entry2.ChargeGroupCode = "Code2";
			entries.RunPreSaveValidation();
			AssertEquals("HasErrors", false, entry2.ChargeGroupCodeInfo.HasErrors());
		}

		public void TestCheckChargeCodes()
		{
			ShipnetChargeGroup entry = new ShipnetChargeGroup(SetupBusinessObject, Factory);
			entry.ChargeGroupCode = "Code";
			entry.RunPreSaveValidation();
			AssertHasErrorContaining(entry.ChargeGroupCodeInfo, "No charge code is entered for this Group 'Code'.");
			ShipnetCharge charge = entry.Charges.AddNew();
			charge.ChargePK = ZGuid.NewZGuid();
			entry.RunPreSaveValidation();
			AssertEquals("Should not have error", false, entry.ChargeGroupCodeInfo.HasErrors());
		}

		public void TestValidateDescription()
		{
			ShipnetChargeGroupCollection entries = new ShipnetChargeGroupCollection(SetupBusinessObject, Factory);
			ShipnetChargeGroup entry = entries.AddNew();
			entry.ChargeGroupDescription = "Description";
			AssertEquals("HasErrors", false, entry.ChargeGroupDescriptionInfo.HasErrors());
			entry.ChargeGroupDescription = "";
			AssertEquals("Should have error", true, entry.ChargeGroupDescriptionInfo.HasErrors());
			AssertEquals("Should contain error message", true, entry.ChargeGroupDescriptionInfo.GetErrors().Contains("Please enter a value."));
			entry.ChargeGroupDescription = "Description2";
			AssertEquals("HasErrors", false, entry.ChargeGroupDescriptionInfo.HasErrors());
		}

		public void TestHasChanges()
		{
			ShipnetChargeGroup bizObj = new ShipnetChargeGroup(SetupBusinessObject, Factory);
			AssertEquals("Default HasChanges", false, bizObj.HasChanges);
			ShipnetCharge charge = bizObj.Charges.AddNew();
			charge.ChargePK = ZGuid.NewZGuid();
			AssertEquals("HasChanges should be true as child has changes", true, bizObj.HasChanges);
			charge.HasChanges = false;
			AssertEquals("HasChanges", false, bizObj.HasChanges);
			charge.HasChanges = true;
			AssertEquals("HasChanges should be true as child has changes", true, bizObj.HasChanges);
			bizObj.HasChanges = false;
			AssertEquals("HasChanges should be false as child should have been set to false", false, bizObj.HasChanges);
		}

		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ShipnetChargeGroup result = new ShipnetChargeGroup(SetupBusinessObject, Factory);
			result.ChargeGroupCode = "123";
			result.ChargeGroupDescription = "Desc 1234";
			ShipnetCharge charge = result.Charges.AddNew();
			charge.ChargePK = ZGuid.NewZGuid();
			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
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

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			if (!isClone)
			{
				ShipnetChargeGroup originalEntry = (ShipnetChargeGroup)originalBusinessObject;
				ShipnetChargeGroup newEntry = (ShipnetChargeGroup)newBusinessObject;
				AssertEquals("Number of charges should be the same", originalEntry.Charges.Count, newEntry.Charges.Count);
				for (int i = 0; i < originalEntry.Charges.Count; i++)
				{
					ShipnetCharge originalCharge = originalEntry.Charges[i];
					ShipnetCharge newCharge = newEntry.Charges[i];
					AssertEquals("ChargePK's should be the same", originalCharge.ChargePK, newCharge.ChargePK);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupBusinessObject = new ShipnetSetupBusinessObject(Factory);
			SetupBusinessObject.IsShipnetCarrier = true;
		}

		ShipnetSetupBusinessObject SetupBusinessObject;
		#endregion
	}
}
