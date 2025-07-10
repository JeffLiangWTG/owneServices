using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security.ActiveDirectory.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(AttributeMapRegistryItem))]
	class AttributeMapRegistryItemTest : StronglyTypedRegistryItemTestCase<AttributeMap>
	{
		protected override StronglyTypedRegistryItem<AttributeMap, AttributeMap> GetNewRegistryItem()
		{
			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("ColumnName1", "PropertyName1", true));
			map.MapItems.Add(new AttributeMapItem("ColumnName2", "PropertyName2", true));
			map.MapItems.Add(new AttributeMapItem("ColumnName3", "PropertyName3", false));

			return new AttributeMapRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
		}
	}

	[TestedType(typeof(AttributeMapRegistryDataType))]
	class AttributeMapRegistryDataTypeTest : RegistryDataTypeTestCase<AttributeMapRegistryDataType>
	{
		public void TestValidation_IntegrationEnabled_UserResponseAD()
		{
			AssertValidation_IntegrationEnabled(SyncModeList.Codes.ActiveDirectory);
		}

		public void TestValidation_IntegrationEnabled_UserResponseCW1()
		{
			AssertValidation_IntegrationEnabled(SyncModeList.Codes.CargoWise);
		}

		void AssertValidation_IntegrationEnabled(string oneOffSyncModeCode)
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_HomePhone", "homePhone", true));
			map.MapItems.Add(new AttributeMapItem("GS_City", "l", false));

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);
			map.MapItems.Add(new AttributeMapItem("GS_UserAddress2", "description", true)); // this test needs a mapping that is not from the default but has to be valid

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			AssertEquals("Pre-requisite", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals("Pre-requisite", false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			UnitTestUserNotification.Instance.AddUserResponse(oneOffSyncModeCode);
			UnitTestUserNotification.Instance.AddOKAnswer();
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertEquals(AttributeMapRegistryDataType.ModifyWarningQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(oneOffSyncModeCode, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals(true, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);
		}

		public void TestValidation_IntegrationEnabled_UserCancel()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("ColumnName1", "PropertyName1", true));
			map.MapItems.Add(new AttributeMapItem("ColumnName2", "PropertyName2", true));
			map.MapItems.Add(new AttributeMapItem("ColumnName3", "PropertyName3", false));

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);
			map.MapItems.Add(new AttributeMapItem("ColumnName4", "PropertyName4", true));

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			AssertEquals("Pre-requisite", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals("Pre-requisite", false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			AssertEquals(false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);
			AssertExceptionThrown<RegistryValidationException>("Modifying Attribute Mapping canceled", () => item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidation_IntegrationEnabled_ShowQuestionOnlyIfNewlySyncedProperty()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_HomePhone", "homePhone", true));
			map.MapItems.Add(new AttributeMapItem("GS_City", "l", false));

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			AssertEquals("Pre-requisite", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals("Pre-requisite", false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			//No new item, we should not show the question form
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals(false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			//One new unsynced item, we should not show the question form
			map.MapItems.Add(new AttributeMapItem("GS_UserAddress2", "description", false)); // this test needs a mapping that is not from the default but has to be valid
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals(false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			//Unsyncing an existing item, we should not show the question form
			map.MapItems[0].IsSynced = false;
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals(false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			//Newly syncing an existing item, we should show the question form
			map.MapItems[3].IsSynced = true;
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertEquals(AttributeMapRegistryDataType.ModifyWarningQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);
		}

		public void TestValidation_IntegrationEnabled_ShowQuestionOnlyIfSyncDirectionIsTwoWay()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_HomePhone", "homePhone", true));
			map.MapItems.Add(new AttributeMapItem("GS_City", "l", false));

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			AssertEquals("Pre-requisite", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals("Pre-requisite", false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			map.MapItems.Add(new AttributeMapItem("GS_UserAddress2", "description", false)); // this test needs a mapping that is not from the default but has to be valid
			map.MapItems[3].IsSynced = true;
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();

			//In TwoWay sync, when setting an existing item to sync, it should show the question form and nudge
			AssertEquals("Pre-requisite", SyncDirection.TwoWay, ActiveDirectoryRegistry.Instance.SyncDirection);
			AssertEquals(AttributeMapRegistryDataType.ModifyWarningQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			ServiceTaskNudger_ForTest.GetTestInstance().HasNudged = false;

			//In OneWay sync, it should not show the question form and should not nudge
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.OneWay;
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			ServiceTaskNudger_ForTest.GetTestInstance().HasNudged = false;

			//but group only can be TwoWay and it will show up
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertEquals(AttributeMapRegistryDataType.ModifyWarningQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			ServiceTaskNudger_ForTest.GetTestInstance().HasNudged = false;
		}

		public void TestValidation_IntegrationEnabled_ChangeAlreadyPending()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_HomePhone", "homePhone", true));
			map.MapItems.Add(new AttributeMapItem("GS_City", "l", false));

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);
			map.MapItems[ZArchitecture.Schema.GlbStaffSchema.GS_City].IsSynced = true;

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			AssertEquals("Pre-requisite", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals("Pre-requisite", false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddUserResponse(SyncModeList.Codes.CargoWise);
			UnitTestUserNotification.Instance.AddOKAnswer();
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertEquals(AttributeMapRegistryDataType.ModifyWarningQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(SyncModeList.Codes.CargoWise, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals(true, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			map.MapItems[2].IsSynced = true;
			AssertExceptionThrown<RegistryValidationException>(@"Please wait for the Active Directory Synchronization service task to run before enabling synchronization on another attribute as the Attribute Mapping has recently been modified.", () => item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidation_IntegrationDisabled()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_HomePhone", "homePhone", true));
			map.MapItems.Add(new AttributeMapItem("GS_City", "l", false));

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			AssertEquals("Pre-requisite", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals("Pre-requisite", false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);

			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			AssertEquals(false, ServiceTaskNudger_ForTest.GetTestInstance().HasNudged);
		}

		public void TestSerialiseAndDeserialise()
		{
			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("ColumnName1", "PropertyName1", true));
			map.MapItems.Add(new AttributeMapItem("ColumnName2", "PropertyName2", true));
			map.MapItems.Add(new AttributeMapItem("ColumnName3", "PropertyName3", false));

			var mapAsBytes = RegistryItemDataType.Serialise(map);
			var deserialisedMap = RegistryItemDataType.Deserialise(mapAsBytes);

			AssertEquals(deserialisedMap.MapItems.Count, map.MapItems.Count + AttributeMap.DefaultMap.MapItems.Count);
			AssertEquals(deserialisedMap.MapItems[0].EnterpriseColumnName, map.MapItems[0].EnterpriseColumnName);
			AssertEquals(deserialisedMap.MapItems[0].ActiveDirectoryAttributeName, map.MapItems[0].ActiveDirectoryAttributeName);
			AssertEquals(deserialisedMap.MapItems[0].IsSynced, map.MapItems[0].IsSynced);

			AssertEquals(deserialisedMap.MapItems[1].EnterpriseColumnName, map.MapItems[1].EnterpriseColumnName);
			AssertEquals(deserialisedMap.MapItems[1].ActiveDirectoryAttributeName, map.MapItems[1].ActiveDirectoryAttributeName);
			AssertEquals(deserialisedMap.MapItems[1].IsSynced, map.MapItems[1].IsSynced);

			AssertEquals(deserialisedMap.MapItems[2].EnterpriseColumnName, map.MapItems[2].EnterpriseColumnName);
			AssertEquals(deserialisedMap.MapItems[2].ActiveDirectoryAttributeName, map.MapItems[2].ActiveDirectoryAttributeName);
			AssertEquals(deserialisedMap.MapItems[2].IsSynced, map.MapItems[2].IsSynced);
		}

		public void TestDataTypeCode()
		{
			AssertEquals(RegistryDataTypes.Codes.Binary, RegistryItemDataType.Code);
		}

		public void TestValidation_WarningMessage()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "name", true));
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			UnitTestUserNotification.Instance.AddUserResponse("CONFIRM");
			UnitTestUserNotification.Instance.AddOKAnswer();
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();

			var expectedMessage = @$"As there are undesired risks in both the Domain and in {Core.Constants.ProductName} with incorrect mappings, please verify the following Attribute Mappings and confirm the changes.
GS_FullName -> name

Please also confirm the following warning(s):
Active Directory Attribute: Mapping GS_FullName to 'name' attribute could affect the AD user login name including 'userPrincipalName' and 'sAMAccountName' attributes, potentially affecting the functionality of user logins and single-sign-on.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestValidation_ChangeMapping()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_City", "manager", true));
			map.MapItems.Add(new AttributeMapItem("GS_MobilePhone", "middleName", true));
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_State", "st", false));
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);
			var tag = new RegistryItemTag(item);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.HasValue = true;

			UnitTestUserNotification.Instance.AddUserResponse("CONFIRM");
			UnitTestUserNotification.Instance.AddOKAnswer();
			item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty);
			tag.NewValue = map;
			tag.IsChanged = true;
			tag.SaveAllValues();

			var expectedMessage = @$"As there are undesired risks in both the Domain and in {Core.Constants.ProductName} with incorrect mappings, please verify the following Attribute Mappings and confirm the changes.
GS_City -> manager
GS_MobilePhone -> middleName";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestValidation_DuplicatedMapping()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_City", "st", true));
			map.MapItems.Add(new AttributeMapItem("GS_MobilePhone", "middleName", true));
			map.MapItems.Add(new AttributeMapItem("GS_FullName", "displayName", true));
			map.MapItems.Add(new AttributeMapItem("GS_State", "st", false));
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var item = new AttributeMapRegistryItem("TestValidation_IntegrationEnabled", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyEditableBySupportIfHosted, map);

			AssertExceptionThrown<RegistryValidationException>("Duplicated Mapping Validation",
				"Active Directory Attribute: The 'st' attribute has been mapped to more than one column in GlbStaff table.",
				() => item.DataType.ValidateBeforeRegistryFormSave(item, map, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region Implementation

		protected override void SetUp()
		{
			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("Column1", "Property1", true));
			map.MapItems.Add(new AttributeMapItem("Column2", "Property2", true));
			map.MapItems.Add(new AttributeMapItem("Column3", "Property3", false));

			RegistryItemDataType = new AttributeMapRegistryDataType(map);

			if (!ObjectFactory.HasBeenSubstituted(nameof(IServiceTaskNudger)))
			{
				ObjectFactory.Substitute<IServiceTaskNudger>(new ServiceTaskNudger_ForTest());
			}

			var adAttributesForTest = ADAttributeList.Instance.GetPredefinedAttributes().ToList();
			adAttributesForTest.Add("manager");
			adAttributesForTest.Add("middleName");
			ADTestHelper.MockSearcherWithDomainAttributes(adAttributesForTest);

			base.SetUp();
		}

		AttributeMapRegistryDataType RegistryItemDataType { get; set; }

		protected override AttributeMapRegistryDataType GetNewDataType()
		{
			return new AttributeMapRegistryDataType(AttributeMap.DefaultMap);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var map = AttributeMap.DefaultMap;
			var bytes = Encoding.Unicode.GetBytes(map.ToStringForSerialisation());

			return new[] { new ValidSampleAndBinaryValueInDB(map, bytes) };
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		#endregion
	}

	class AttributeMapRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataType()
		{
			AssertEquals(typeof(AttributeMapRegistryDataType), new AttributeMapRegistryEditorInfo().BaseDataTypeToBeEdited);
		}

		public void TestEditorClassAndAssembly()
		{
			var expectedType = Type.GetType("Enterprise.Security.ActiveDirectory.GUI.Registry.AttributeMapRegistryEditor, Enterprise.Security.ActiveDirectory.GUI");
			AssertNotNull("AttributeMapRegistryEditor should be a valid type", expectedType);

			var attribute = typeof(AttributeMapRegistryEditorInfo).GetCustomAttributes(typeof(RegistryEditorAttribute), inherit: false)[0] as RegistryEditorAttribute;
			AssertNotNull("RegistryEditorAttribute should be present", attribute);

			string expectedName = string.Format("{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
			AssertEquals("Attribute should specify fully-qualified type with assembly", expectedName, attribute.TypeName);
		}
	}

	class ServiceTaskNudger_ForTest : IServiceTaskNudger
	{
		public void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null)
		{
			HasNudged = true;
		}

		public bool HasNudged { get; set; }

		public static ServiceTaskNudger_ForTest GetTestInstance()
		{
			return ObjectFactory.Get<IServiceTaskNudger>() as ServiceTaskNudger_ForTest;
		}
	}
}
