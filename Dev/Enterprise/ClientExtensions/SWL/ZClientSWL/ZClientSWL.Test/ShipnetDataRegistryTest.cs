using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.SWL.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Testing
{
	[TestedType(typeof(SWLDataRegistry))]
	class ShipnetDataRegistryTest : RegistryItemSetTestCaseWithFactory<SWLDataRegistry>
	{
		public void TestShipnetDataRegistryItems()
		{
			AssertEquals("Collection count", 6, AllItems.Count);
			AssertVisible(ItemSet.IsShipnetEnable, true);
			AssertVisible(ItemSet.ShipnetBackupDirectoryItem);
			AssertVisible(ItemSet.ShipnetPurgeBackupItem);
			AssertVisible(ItemSet.ShipnetNotificationEmailGroup);
			AssertVisible(ItemSet.ShipnetHighWaterMarkItem);
			AssertNotVisible(ItemSet.ShipnetSetupRaw);
		}

		public void TestIsShipnetEnable()
		{
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.IsShipnetEnable.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, ItemSet.IsShipnetEnable.Options);
			AssertEquals("EditorInfo", typeof(BooleanRegistryEditorInfo), ItemSet.IsShipnetEnable.EditorInfo.GetType());
			AssertEquals("DefaultValue", false, ItemSet.IsShipnetEnable.DefaultValue);
			AssertEquals("IsShipnetEnableItem", false, ItemSet.IsShipnetEnable.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			ItemSet.IsShipnetEnable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("IsShipnetEnableItem", true, ItemSet.IsShipnetEnable.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		#region Shipnet Notification Email Group
		public void TestShipnetNotificationEmailGroup()
		{
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ShipnetNotificationEmailGroup.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue, ItemSet.ShipnetNotificationEmailGroup.Options);
			Guid allUsersGroup = GetAllUsersGlbGroup().PK.ToGuid();
			AssertEquals("ShipnetNotificationEmailGroup.DefaultValue", allUsersGroup, ItemSet.ShipnetNotificationEmailGroup.DefaultValue);
			Guid newGuid = Guid.NewGuid();
			ItemSet.ShipnetNotificationEmailGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("ShipnetNotificationEmailGroup.Value", newGuid, ItemSet.ShipnetNotificationEmailGroup.Value);
		}

		public void TestShipnetNotificationEmailGroupDefaultValue()
		{
			AssertEquals("DefaultValue", Core.Constants.Groups.AllPK, ItemSet.ShipnetNotificationEmailGroup.DefaultValue);
		}

		BusinessObject GetAllUsersGlbGroup()
		{
			return (BusinessObject)Factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
		}

		#endregion
		public void TestShipnetBackupDirectoryItem()
		{
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ShipnetBackupDirectoryItem.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.ShipnetBackupDirectoryItem.Options);
			AssertEquals("ShipnetNextExportRunItem", "", ItemSet.ShipnetBackupDirectoryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			ItemSet.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "HELLO");
			AssertEquals("ShipnetBackupDirectoryItem", "HELLO", ItemSet.ShipnetBackupDirectoryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestShipnetPurgeBackupItem()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ShipnetPurgeBackupItem.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.ShipnetPurgeBackupItem.Options);
			AssertEquals("ShipnetPurgeBackupItem", 30, ItemSet.ShipnetPurgeBackupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.ShipnetPurgeBackupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 90);
			AssertEquals("ShipnetPurgeBackupItem", 90, ItemSet.ShipnetPurgeBackupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShipnetHighWaterMark()
		{
			AssertEquals("Storage", RegistryStorageFlags.Company, ItemSet.ShipnetHighWaterMarkItem.Storage);
			AssertEquals("Options", RegistryOptions.NotCached | RegistryOptions.PreserveTestValue, ItemSet.ShipnetHighWaterMarkItem.Options);
			AssertEquals("EditorInfo", typeof(DateTimeRegistryEditorInfo), ItemSet.ShipnetHighWaterMarkItem.EditorInfo.GetType());
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, ((DateTimeRegistryEditorInfo)ItemSet.ShipnetHighWaterMarkItem.EditorInfo).DateTimeFormat);
			AssertEquals("ShipnetHighWaterMarkItem", ItemSet.ShipnetHighWaterMarkItem.DefaultValue, ItemSet.ShipnetHighWaterMarkItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			DateTime date = new DateTime(2006, 11, 23, 15, 23, 23);
			ItemSet.ShipnetHighWaterMarkItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, date);
			AssertEquals("ShipnetHighWaterMarkItem", date, ItemSet.ShipnetHighWaterMarkItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "BizObj type passed into SetOrDeleteShipnetSetupBusinessObject(ZGuid OrgPK, RegistryBusinessObjectTemplate BizObj) must be derived from ShipnetSetupBusinessObject. Error on type : Enterprise.Client.SWL.Testing.ShipnetDataRegistryTest+DummyRegistryBusinessObjectTemplate")]
		public void TestSetOrDeleteShipnetSetupBusinessObjectThrowException()
		{
			ZGuid orgPK = ZGuid.NewZGuid();
			DummyRegistryBusinessObjectTemplate setup = new DummyRegistryBusinessObjectTemplate();
			ItemSet.SetOrDeleteShipnetSetupBusinessObject(orgPK, setup);
		}

		public void TestGetAndSetOrDeleteShipnetSetupBusinessObject()
		{
			ZGuid orgPK = ZGuid.NewZGuid();
			RegistryBusinessObjectTemplate setup = ItemSet.GetShipnetSetupBusinessObject(orgPK);
			AssertNotNull("Setup should not be null", setup);
			AssertEquals("Setup should be an empty object", "", setup["DebtorControlCode"]);
			RegistryBusinessObjectTemplate newObj = new ShipnetSetupBusinessObject(Factory);
			((ShipnetSetupBusinessObject)newObj).IsShipnetCarrier = true;
			newObj.RunPreSaveValidation();
			AssertEquals("PreCondition: NewObj should have error", true, newObj.HasErrors);
			ItemSet.SetOrDeleteShipnetSetupBusinessObject(orgPK, newObj);
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)ItemSet.ShipnetSetupRaw).HasActualValue(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid()));
			setup = ItemSet.GetShipnetSetupBusinessObject(orgPK);
			AssertNotNull("Setup should not be null", setup);
			AssertEquals("Setup should be an empty object", "", setup["DebtorControlCode"]);
			newObj.FillWithValidTestData();
			newObj.RunPreSaveValidation();
			AssertEquals("PreCondition: NewObj should have no error", false, newObj.HasErrors);
			ItemSet.SetOrDeleteShipnetSetupBusinessObject(orgPK, newObj);
			AssertEquals("HasActualValue", true, ((IRegistryItemInternals)ItemSet.ShipnetSetupRaw).HasActualValue(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid()));
			setup = ItemSet.GetShipnetSetupBusinessObject(orgPK);
			AssertNotNull("Setup should not be null", setup);
			AssertEquals("Setup should be same as NewObj", newObj["DebtorControlCode"], setup["DebtorControlCode"]);
			setup = ItemSet.GetShipnetSetupBusinessObject(ZGuid.Invalid);
			AssertNull("Setup should be null", setup);
			((ShipnetSetupBusinessObject)newObj).IsShipnetCarrier = false;
			ItemSet.SetOrDeleteShipnetSetupBusinessObject(orgPK, newObj);
			setup = ItemSet.GetShipnetSetupBusinessObject(orgPK);
			AssertEquals("Setup should be an empty object", "", setup["DebtorControlCode"]);
			AssertEquals("HasActualValue", false, ((IRegistryItemInternals)ItemSet.ShipnetSetupRaw).HasActualValue(Env.CurrentCompany.PK, Guid.Empty, orgPK.ToGuid()));
		}

		public void TestShipnetSetupBusinessObjectCompanyPK()
		{
			ZGuid orgPK = ZGuid.NewZGuid();
			ShipnetSetupBusinessObject setup = (ShipnetSetupBusinessObject)ItemSet.GetShipnetSetupBusinessObject(orgPK);
			AssertNotNull("Setup should not be null", setup);
			AssertEquals("CompanyDataPK", orgPK, setup.CompanyDataPK);
		}

		[XmlSerializerAssembly("ZClientSWL.XmlSerializers")]
		class DummyRegistryBusinessObjectTemplate : RegistryBusinessObjectTemplate
		{
			protected override void ReadElements(XmlReaderWrapper reader)
			{
			}

			protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			{
				return null;
			}
		}
	}
}
