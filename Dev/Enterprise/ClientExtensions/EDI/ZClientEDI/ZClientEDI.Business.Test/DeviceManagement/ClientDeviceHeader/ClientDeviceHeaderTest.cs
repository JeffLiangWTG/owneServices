using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientDeviceHeader))]
	internal class ClientDeviceHeaderTest : PersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<ClientDeviceHeader>();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Factory.NewWithValidTestData<ClientDeviceHeader>();
		}

		public void TestLicenceOrgPK()
		{
			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OOO";
			licenceEnterprise.LE_OH = orgHeader.PK;
			licenceEnterprise.LE_EnterpriseCode = "BBB";

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = licenceEnterprise.PK;
			licenceDatabase.LD_ServerCode = "AAA";
			Factory.Save();

			var device = Factory.New<ClientDeviceHeader>();
			AssertEquals(ZString.Empty, device.CDH_ServerCode);
			AssertEquals(ZString.Empty, device.CDH_EnterpriseCode);

			device.CDH_EnterpriseCode = licenceDatabase.LicEnterprise.LE_EnterpriseCode;
			device.CDH_ServerCode = licenceDatabase.LD_ServerCode;

			AssertEquals(orgHeader.PK, device.LicenceOrgPK);

			device.CDH_EnterpriseCode = ZString.Empty;
			device.CDH_ServerCode = ZString.Empty;
			AssertEquals(ZGuid.Empty, device.LicenceOrgPK);
		}

		public void TestSettingEnterpriseCodeClearsServerCode()
		{
			var device = Factory.New<ClientDeviceHeader>();

			device.CDH_EnterpriseCode = "AAA";
			device.CDH_ServerCode = "BBB";

			AssertEquals("AAA", device.CDH_EnterpriseCode);
			AssertEquals("BBB", device.CDH_ServerCode);

			device.CDH_EnterpriseCode = ZString.Empty;

			AssertEquals(ZString.Empty, device.CDH_EnterpriseCode);
			AssertEquals(ZString.Empty, device.CDH_ServerCode);
		}

		public void TestDefaultValues()
		{
			var device = Factory.New<ClientDeviceHeader>();
			AssertEquals(ClientDeviceHeaderLookups.Statuses.Active, device.CDH_Status);
		}

		public void TestCode()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_Identifier = "ID";
			device.CDH_ModelID = "MODEL";

			AssertEquals("ID", device.Code);
			device.CDH_IsTemplate = true;
			AssertEquals("MODEL", device.Code);
		}

		public void TestSave_GeneratesNumberForDevice()
		{
			var device = Factory.New<ClientDeviceHeader>();
			Factory.Save();
			var generatedId = device.CDH_Identifier;
			AssertNotEquals(ZString.Empty, generatedId);
			Factory.Save();
			AssertEquals(generatedId, device.CDH_Identifier);
		}

		public void TestSave_KeepsExistingNumber()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_Identifier = "ALEEEEE";
			Factory.Save();
			AssertEquals("ALEEEEE", device.CDH_Identifier);
		}

		public void TestSave_GeneratesNoNumberForTemplates()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_IsTemplate = true;
			Factory.Save();
			AssertEquals(ZString.Empty, device.CDH_Identifier);
		}

		public void TestReadOnlyProperties_Device()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_IsTemplate = false;
			AssertEquals(true, device.CDH_ModelIDInfo.ReadOnly);
			AssertEquals(true, device.CDH_DescriptionInfo.ReadOnly);
			AssertEquals(false, device.CDH_StatusInfo.ReadOnly);
			AssertEquals(false, device.CDH_EnterpriseCodeInfo.ReadOnly);
			AssertEquals(true, device.CDH_ServerCodeInfo.ReadOnly);
			AssertEquals(true, device.CDH_IsTemplateInfo.ReadOnly);
			AssertEquals(false, device.CDH_IdentifierInfo.ReadOnly);
			AssertEquals(true, device.CDH_ClientParentIDInfo.ReadOnly);
			AssertEquals(true, device.CDH_ClientParentTypeInfo.ReadOnly);

			Factory.Save();
			AssertEquals(true, device.CDH_IdentifierInfo.ReadOnly);
		}

		public void TestReadOnlyProperties_Template()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_IsTemplate = true;
			AssertEquals(false, device.CDH_ModelIDInfo.ReadOnly);
			AssertEquals(false, device.CDH_DescriptionInfo.ReadOnly);
			AssertEquals(false, device.CDH_StatusInfo.ReadOnly);
			AssertEquals(true, device.CDH_EnterpriseCodeInfo.ReadOnly);
			AssertEquals(true, device.CDH_ServerCodeInfo.ReadOnly);
			AssertEquals(true, device.CDH_IsTemplateInfo.ReadOnly);
			AssertEquals(true, device.CDH_IdentifierInfo.ReadOnly);
			AssertEquals(true, device.CDH_ClientParentIDInfo.ReadOnly);
			AssertEquals(true, device.CDH_ClientParentTypeInfo.ReadOnly);
		}

		public void TestComponentTypeDescription()
		{
			var component = Factory.New<ClientDeviceComponent>();
			component.CDC_ComponentType = component.Lookups.ComponentTypeList[0].Code;
			AssertEquals(component.Lookups.ComponentTypeList[0].Description, component.ComponentTypeDescription);
		}

		public void TestDoesNotLoadAbstractTypeForDmgDeviceHeaderTablePrefix()
		{
			var header = Factory.New<ClientDeviceHeader>();
			Factory.Save();
			AssertNoExceptionThrown("Exception thrown loading ClientDeviceHeader ensure that it is not trying to instantiate an abstract class and that the type loaded is of the expected type", () => Factory.Load<ClientDeviceHeader>(DmgDeviceHeaderSchema.Constants.Prefix, header.PK));
		}

		public void TestDelete()
		{
			var header = Factory.New<ClientDeviceHeader>();
			var component = header.Components.AddNew();
			var id = component.Identifiers.AddNew();
			header.Delete();
			AssertEquals(true, header.IsDeleted);
			AssertEquals(true, component.IsDeleted);
			AssertEquals(true, id.IsDeleted);
		}

		public void TestPopulateDeviceFromModel()
		{
			var header = Factory.New<ClientDeviceHeader>();

			header.CDH_IsTemplate = true;
			header.CDH_ModelID = "RAKHSH";
			var component = header.Components.AddNew();

			component.CDC_ModelIdentifier = "ALEE";
			var id1 = component.Identifiers.AddNew();
			id1.CDD_IdentificationType = "LOL";
			id1.CDD_Identifier = "RY";
			var id2 = component.Identifiers.AddNew();
			id2.CDD_IdentificationType = "RAK";
			id2.CDD_Identifier = "ZAY";

			var result = Factory.New<ClientDeviceHeader>();
			header.PopulateDeviceFromModel(result);
			AssertEquals("RAKHSH", result.CDH_ModelID);
			AssertEquals(false, result.CDH_IsTemplate);
			AssertEquals(1, result.Components.Count);
			AssertEquals("ALEE", result.Components[0].CDC_ModelIdentifier);
			AssertEquals(2, result.Components[0].Identifiers.Count);
			AssertEquals("RY", result.Components[0].Identifiers.Single(x => x.CDD_IdentificationType == "LOL").CDD_Identifier);
			AssertEquals("ZAY", result.Components[0].Identifiers.Single(x => x.CDD_IdentificationType == "RAK").CDD_Identifier);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "You can only clone devices that are device models/templates.")]
		public void TestPopulateDeviceFromModel_NonTemplate()
		{
			var header = Factory.New<ClientDeviceHeader>();
			header.CDH_ModelID = "FAIL";

			var result = Factory.New<ClientDeviceHeader>();
			header.PopulateDeviceFromModel(result);
		}

		public void TestNoAuditLogs()
		{
			var header = Factory.New<ClientDeviceHeader>();
			Factory.Save();

			header.CDH_EnterpriseCode = "TST";
			Factory.Save();

			header.Delete();
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("An ADD audit log was created.", !header.Logs.HasLogWith(l => l.SL_SE_NKEvent == "ADD"));
				Assert("An EDT audit log was created.", !header.Logs.HasLogWith(l => l.SL_SE_NKEvent == "EDT"));
				Assert("A DEL audit log was created.", !header.Logs.HasLogWith(l => l.SL_SE_NKEvent == "DEL"));
			});
		}
	}

	class ClientDeviceHeaderNotificationTestBase : TestCaseWithFactory
	{
		protected ClientDeviceHeader DeviceHeader { get; private set; }

		protected LicenceDatabase LicenceDatabase { get; private set; }

		protected OrgHeader OrgHeader { get; private set; }

		protected override void SetUp()
		{
			base.SetUp();

			OrgHeader = Factory.New<OrgHeader>();
			OrgHeader.OH_Code = "TEST000";
			OrgHeader.OH_FullName = "Test Organization";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "A12";
			licenceEnterprise.LE_OH = OrgHeader.PK;

			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyCode = "B34";
			licenceCompany.LC_LE = licenceEnterprise.PK;
			licenceCompany.LC_OH = OrgHeader.PK;

			LicenceDatabase = Factory.New<LicenceDatabase>();
			LicenceDatabase.LD_ServerCode = "C56";
			LicenceDatabase.LD_LE = licenceEnterprise.PK;

			DeviceHeader = Factory.New<ClientDeviceHeader>();
			DeviceHeader.CDH_Description = "Brain Scanner";
			DeviceHeader.CDH_Identifier = "TD01234567";
			DeviceHeader.CDH_IsTemplate = false;
			DeviceHeader.CDH_EnterpriseCode = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
			DeviceHeader.CDH_ServerCode = LicenceDatabase.LD_ServerCode;
			DeviceHeader.CDH_ModelID = "BSMK1";
			DeviceHeader.CDH_Status = ClientDeviceHeaderLookups.Statuses.Active;

			DeviceHeader.CDH_DeviceIdentifier = "QWEASDZXC";

			SetUpBeforeSave();

			Factory.Save();
		}

		protected virtual void SetUpBeforeSave()
		{
		}

		protected override void TearDown()
		{
			LicenceDatabase = null;
			DeviceHeader = null;

			base.TearDown();
		}
	}

	#region Clears Cached Client Info

	abstract class ClientDeviceHeader_ClearsCachedClientInfo_TestBase : ClientDeviceHeaderNotificationTestBase
	{
		public void TestClearsCachedClientInfo()
		{
			DeviceHeader.CDH_ClientParentID = "Hank Pym's Laboratory";
			DeviceHeader.CDH_ClientParentType = "RQ";
			Factory.Save();

			MakeChange();
			Factory.Save();

			AssertNullOrEmpty(nameof(DeviceHeader.CDH_ClientParentID), DeviceHeader.CDH_ClientParentID);
			AssertNullOrEmpty(nameof(DeviceHeader.CDH_ClientParentType), DeviceHeader.CDH_ClientParentType);
		}

		protected abstract void MakeChange();
	}

	class ClientDeviceHeader_CachedClientInfoRemainsUntouched : ClientDeviceHeaderNotificationTestBase
	{
		public void TestCachedClientInfoRemainsUntouched()
		{
			DeviceHeader.CDH_ClientParentID = "Hank Pym's Laboratory";
			DeviceHeader.CDH_ClientParentType = "RQ";

			Factory.Save();

			DeviceHeader.CDH_Description = "A different description.";
			Factory.Save();

			AssertEquals(nameof(DeviceHeader.CDH_ClientParentID), "Hank Pym's Laboratory", DeviceHeader.CDH_ClientParentID);
			AssertEquals(nameof(DeviceHeader.CDH_ClientParentType), "RQ", DeviceHeader.CDH_ClientParentType);
		}

		protected override void SetUpBeforeSave() => DeviceHeader.CDH_IsTemplate = true;
	}

	class ClientDeviceHeader_ClearsCachedClientInfo_WhenDeassigned : ClientDeviceHeader_ClearsCachedClientInfo_TestBase
	{
		protected override void MakeChange()
		{
			DeviceHeader.CDH_EnterpriseCode = ZString.Empty;
			DeviceHeader.CDH_ServerCode = ZString.Empty;
		}
	}

	class ClientDeviceHeader_ClearsCachedClientInfo_WhenReassigned : ClientDeviceHeader_ClearsCachedClientInfo_TestBase
	{
		protected override void MakeChange()
		{
			// "If you wish to make apple pie from scratch, you must first create the universe" - Carl Sagan
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORGHDRXXX";

			var licenceEnterprise = Factory.New<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseCode = "PYM";
			licenceEnterprise.LE_OH = orgHeader.PK;

			var licenceCompany = Factory.New<LicenceCompany>();
			licenceCompany.LC_CompanyCode = "000";
			licenceCompany.LC_LE = licenceEnterprise.PK;
			licenceCompany.LC_OH = orgHeader.PK;

			var licenceDatabase = Factory.New<LicenceDatabase>();
			licenceDatabase.LD_ServerCode = "LAB";
			licenceDatabase.LD_LE = licenceEnterprise.PK;

			DeviceHeader.CDH_ServerCode = LicenceDatabase.LD_ServerCode;
			DeviceHeader.CDH_ServerCode = LicenceDatabase.LicEnterprise.LE_EnterpriseCode;
		}
	}

	#endregion
}
