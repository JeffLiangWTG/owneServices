using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ColumnConfigurationField))]
	sealed class ColumnConfigurationFieldTest : FilterFieldTest
	{
		public void TestConfigManagerListeningCurrentConfigRefreshed()
		{
			AssertEquals("Default template should be current", Manager.CurrentColumnConfigurationManager, ListeningField.Value);

			Manager.CompanyDefaultConfigurationManager.Load();
			AssertEquals("Default template should be current", Manager.CompanyDefaultConfigurationManager, ListeningField.Value);
		}

		public void TestConfigManagerListeningConfigSavedAndDeleted()
		{
			AssertEquals("There are two configs", 2, Manager.ConfigurationManagersForAllSavedConfigurations.Count);

			ColumnConfigurationManager newConfigManager = new CombinedConfigurationManager(Manager, "blahblah");
			newConfigManager.Save();
			AssertEquals("There are currently 3 configurations", 3, ListeningField.SavedConfigurations.Count);

			newConfigManager.Load();
			AssertEquals("new saved setting shoudl be current", newConfigManager, ListeningField.Value);

			newConfigManager.Delete();
			AssertEquals("There are now 2 configurations", 2, ListeningField.SavedConfigurations.Count);
			AssertEquals("new saved setting shoudl be current", Manager.DefaultTemplateConfigurationManager, ListeningField.Value);
		}

		public void TestConfigurationFieldIsOpenedWithoutException()
		{
			var serializedInOldWayColumnConfigurationField =
				@"
				{
				  ""LinkPK"": ""741eed77-74bc-4a9f-8b82-bd807b89b446"",
				  ""ReportID"": ""c5c3984c-54dd-4b8e-bbf8-6c287d77230e"",
				  ""Scheduled"": false,
				  ""Description"": ""TestClient (as Client) - TestDescription"",
				  ""LinkCode"": ""TestClient"",
				  ""UniqueDescription"": ""TestDescription"",
				  ""ManagerSaveToFilterField"": ""Client"",
				  ""Name"": """",
				  ""DisplayName"": null,
				  ""FieldName"": null
				}";

			var deserialisedColumnConfigurationField = JsonConverterHelper.Deserialize<ColumnConfigurationField>(serializedInOldWayColumnConfigurationField);
			var deserialisedManager = deserialisedColumnConfigurationField.Value;

			var clientPK = new ZGuid("741eed77-74bc-4a9f-8b82-bd807b89b446");
			AssertEquals("Description must be 'TestClient (as Client) - TestDescription'", "TestClient (as Client) - TestDescription", deserialisedManager.Description);
			AssertEquals("UniqueDescription must be 'TestDescription'", "TestDescription", deserialisedManager.UniqueDescription);
			AssertEquals("LinkPK can't be empty", clientPK, deserialisedManager.LinkPK);
			AssertEquals("LinkCode must be 'TestClient'", "TestClient", deserialisedManager.LinkCode);
		}

		public void TestJsonConverter()
		{
			var originalColumnConfigurationField = new ColumnConfigurationField(Factory);
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "TestClient";
			Factory.Save();
			Manager.SaveToFilterField = "Client";
			originalColumnConfigurationField.Value = new CombinedConfigurationManager(Manager, client.PK, client.OH_Code, "", "TestDescription");
			originalColumnConfigurationField.DisplayName = "TestDisplayName";

			var result = JsonConverterHelper.Serialize(originalColumnConfigurationField);
			var deserialisedColumnConfigurationField = JsonConverterHelper.Deserialize<ColumnConfigurationField>(result);
			var deserialisedManager = deserialisedColumnConfigurationField.Value;

			AssertEquals("Description must be 'TestClient (as Client) - TestDescription'", "TestClient (as Client) - TestDescription", deserialisedManager.Description);
			AssertEquals("UniqueDescription must be 'TestDescription'", "TestDescription", deserialisedManager.UniqueDescription);
			AssertEquals("LinkPK can't be empty", client.PK, deserialisedManager.LinkPK);
			AssertEquals("LinkCode must be 'TestClient'", "TestClient", deserialisedManager.LinkCode);
			AssertEquals("DisplayName must be 'TestDisplayName'", "TestDisplayName", deserialisedColumnConfigurationField.DisplayName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizobj = (ColumnConfigurationField)base.GetNewBusinessObject();
			var manager = new ColumnConfigurationsManager(ZGuid.Empty, false);
			((IColumnHeadingManagerListener)bizobj).SetManager(manager);
			return bizobj;
		}

		ColumnConfigurationField ListeningField
		{
			get
			{
				if (fListeningField == null)
				{
					fListeningField = new ColumnConfigurationField(Factory);
					((IColumnHeadingManagerListener)fListeningField).SetManager(Manager);
				}
				return fListeningField;
			}
		}
		ColumnConfigurationField fListeningField;

		ColumnConfigurationsManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new ColumnConfigurationsManager(ZGuid.NewZGuid(), false);
				}
				return fManager;
			}
		}
		ColumnConfigurationsManager fManager;
	}
}
