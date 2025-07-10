using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Business.Testing
{
	[TestedType(typeof(ShipnetSetupBusinessObject))]
	public class ShipnetSetupBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidation()
		{
			ShipnetSetupBusinessObject bizObj = new ShipnetSetupBusinessObject(Factory);
			bizObj.RunPreSaveValidation();
			AssertEquals("HasError", false, bizObj.HasErrors);
			bizObj.IsShipnetCarrier = true;
			bizObj.RunPreSaveValidation();
			AssertHasErrorContaining(bizObj.DebtorControlCodeInfo, "Please enter a value.");
			AssertHasErrorContaining(bizObj.CreditorControlCodeInfo, "Please enter a value.");
			AssertHasRowError(bizObj, "A Charge Group must be assigned.");
			bizObj.IsShipnetCarrier = false;
			bizObj.RunPreSaveValidation();
			AssertEquals("HasError", false, bizObj.HasErrors);
			bizObj.IsShipnetCarrier = true;
			bizObj.DebtorControlCode = "DebtorCode";
			bizObj.CreditorControlCode = "CreditorCode";
			ShipnetChargeGroup group = bizObj.ChargeGroups.AddNew();
			bizObj.RunPreSaveValidation();
			AssertEquals("DebtorControlCodeInfo", false, bizObj.DebtorControlCodeInfo.HasErrors());
			AssertEquals("CreditorControlCodeInfo", false, bizObj.CreditorControlCodeInfo.HasErrors());
			AssertEquals("HasRowErrors", false, bizObj.HasRowErrors);
			AssertEquals("Group.HasErrors", true, group.HasErrors);
			AssertEquals("HasErrors should be true as child has error", true, bizObj.HasErrors);
			group.ChargeGroupCode = "Code";
			group.ChargeGroupDescription = "Code Desc";
			ShipnetCharge charge = group.Charges.AddNew();
			charge.ChargePK = ZGuid.NewZGuid();
			bizObj.RunPreSaveValidation();
			AssertEquals("Charge.HasErrors", false, charge.HasErrors);
			AssertEquals("Group.HasErrors", false, group.HasErrors);
			AssertEquals("HasErrors should be true as child has error", false, bizObj.HasErrors);
			ShipnetChargeGroup group2 = bizObj.ChargeGroups.AddNew();
			group2.ChargeGroupCode = "Code2";
			group2.ChargeGroupDescription = "Code2 Desc";
			ShipnetCharge charge2 = group2.Charges.AddNew();
			charge2.ChargePK = charge.ChargePK;
			bizObj.RunPreSaveValidation();
			AssertEquals("Charge.HasErrors", false, charge.HasErrors);
			AssertEquals("Group.HasErrors", false, group.HasErrors);
			AssertEquals("Charge2.HasErrors", false, charge2.HasErrors);
			AssertEquals("Group2.HasErrors", false, group2.HasErrors);
			AssertHasRowError(bizObj, "A charge code is assigned to more than one Charge Group.");
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.ChargePK = chargeCode.PK;
			charge2.ChargePK = chargeCode.PK;
			bizObj.RunPreSaveValidation();
			AssertEquals("Charge.HasErrors", false, charge.HasErrors);
			AssertEquals("Charge2.HasErrors", false, charge2.HasErrors);
			AssertHasRowError(bizObj, string.Format("Charge code '{0}' is assigned to more than one Charge Group.", chargeCode.AC_Code));
		}

		public void TestHasChanges()
		{
			ShipnetSetupBusinessObject bizObj = new ShipnetSetupBusinessObject(Factory);
			AssertEquals("Default HasChanges", false, bizObj.HasChanges);
			ShipnetChargeGroup group = bizObj.ChargeGroups.AddNew();
			group.ChargeGroupCode = "NewCode";
			AssertEquals("HasChanges should be true as child has changes", true, bizObj.HasChanges);
			group.HasChanges = false;
			AssertEquals("HasChanges", false, bizObj.HasChanges);
			group.HasChanges = true;
			AssertEquals("HasChanges should be true as child has changes", true, bizObj.HasChanges);
			bizObj.HasChanges = false;
			AssertEquals("HasChanges should be false as child should have been set to false", false, bizObj.HasChanges);
		}

		public void TestDataIsClearAndDeletedWhenIsShipnetCarrierSetToFalse()
		{
			ShipnetSetupBusinessObject bizObj = new ShipnetSetupBusinessObject(Factory);
			bizObj.CompanyDataPK = GlbCompany.CurrentCompany.OrgProxy.CompanyData.PK;
			bizObj.IsShipnetCarrier = true;
			EDICommunicationsMode mode = bizObj.CommunicationMode;
			bizObj.DebtorControlCode = "DEBT";
			bizObj.CreditorControlCode = "CRED";
			ShipnetChargeGroup group = bizObj.ChargeGroups.AddNew();
			group.ChargeGroupCode = "Group Code";
			group.ChargeGroupDescription = "Group Description";
			ShipnetCharge charge = group.Charges.AddNew();
			charge.ChargePK = ZGuid.NewZGuid();
			bizObj.IsShipnetCarrier = false;
			AssertEquals("DebtorControlCode", "", bizObj.DebtorControlCode);
			AssertEquals("CreditorControlCode", "", bizObj.CreditorControlCode);
			AssertEquals("ChargeGroups.Count", 0, bizObj.ChargeGroups.Count);
			AssertNotEquals("CommunicationMode.PK", mode.PK, bizObj.CommunicationMode.PK);
			AssertEquals("mode should have been deleted", true, mode.IsDeleted);
		}

		public void TestCommunicationModeParentDataIsSet()
		{
			ShipnetSetupBusinessObject bizObj = new ShipnetSetupBusinessObject(Factory);
			OrgCompanyData companyData = GlbCompany.CurrentCompany.OrgProxy.CompanyData;
			bizObj.CompanyDataPK = companyData.PK;
			EDICommunicationsMode mode = bizObj.CommunicationMode;
			AssertEquals("CommunicationMode.EK_ParentID", companyData.PK, mode.EK_ParentID);
			AssertEquals("CommunicationMode.EK_ParentTableCode", "OB", mode.EK_ParentTableCode);
		}

		#region Implementation
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ShipnetSetupBusinessObject result = new ShipnetSetupBusinessObject(Factory);
			result.fCommunicationPK = ZGuid.NewZGuid();
			ShipnetChargeGroup group = result.ChargeGroups.AddNew();
			group.ChargeGroupCode = "Group Code";
			group.ChargeGroupDescription = "Group Description";
			ShipnetCharge charge = group.Charges.AddNew();
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
				ShipnetSetupBusinessObject originalBizO = (ShipnetSetupBusinessObject)originalBusinessObject;
				ShipnetSetupBusinessObject newBizO = (ShipnetSetupBusinessObject)newBusinessObject;
				AssertEquals("Number of charge groups should be the same", originalBizO.ChargeGroups.Count, newBizO.ChargeGroups.Count);
				for (int i = 0; i < originalBizO.ChargeGroups.Count; i++)
				{
					ShipnetChargeGroup originalGroup = originalBizO.ChargeGroups[i];
					ShipnetChargeGroup newGroup = newBizO.ChargeGroups[i];
					AssertEquals("Groups's Code should be the same", originalGroup.ChargeGroupCode, newGroup.ChargeGroupCode);
					AssertEquals("Groups's Description should be the same", originalGroup.ChargeGroupDescription, newGroup.ChargeGroupDescription);
					AssertEquals("Number of charges should be the same", originalGroup.Charges.Count, newGroup.Charges.Count);
					for (int j = 0; j < originalGroup.Charges.Count; j++)
					{
						ShipnetCharge originalCharge = originalGroup.Charges[j];
						ShipnetCharge newCharge = newGroup.Charges[j];
						AssertEquals("ChargePK's should be the same", originalCharge.ChargePK, newCharge.ChargePK);
					}
				}
			}
		}
		#endregion
	}
}
