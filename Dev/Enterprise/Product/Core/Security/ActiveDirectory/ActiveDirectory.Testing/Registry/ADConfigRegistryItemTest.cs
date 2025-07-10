using System;
using System.Collections.Generic;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test.Registry
{
	class ADConfigRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataType()
		{
			var type = new ADConfigRegistryEditorInfo().BaseDataTypeToBeEdited;
			AssertEquals(typeof(ADConfigRegistryDataType), type);
		}

		public void TestEditorClassAndAssembly()
		{
			var expectedType = Type.GetType("Enterprise.Security.ActiveDirectory.GUI.Registry.ADRegistryControlItemEditor, Enterprise.Security.ActiveDirectory.GUI");
			AssertNotNull("ADRegistryControlItemEditor should be a valid type", expectedType);

			var attribute = typeof(ADConfigRegistryEditorInfo).GetCustomAttributes(typeof(RegistryEditorAttribute), inherit: false)[0] as RegistryEditorAttribute;
			AssertNotNull("RegistryEditorAttribute should be present", attribute);

			string expectedName = string.Format("{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
			AssertEquals("Attribute should specify fully-qualified type with assembly", expectedName, attribute.TypeName);
		}
	}

	class ADConfigRegistryItemTest : TestCaseWithFactoryAndMocks
	{
		[ExpectNoExceptions]
		public void TestDeleteValue_ShouldValidateDefaultValueToEnsureDeactivationRoutineIsRun()
		{
			var item = new ADConfigRegistryItem("TestADConfigItem", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, ADConfig.DefaultValue);
			var dataType = new Mock<IRegistryDataType>();
			item.DataType = dataType.Object;
			dataType.Setup(d => d.Code).Returns("BIN");
			((IRegistryItemInternals)item).DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			dataType.Verify(d => d.Validate(item, ADConfig.DefaultValue, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[ExpectNoExceptions]
		public void TestCanCallToString_ForTestingPurposes()
		{
			new ADConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, ADConfig.DefaultValue).ToString();
		}
	}

	[TestedType(typeof(ADConfigRegistryItem))]
	class ADConfigStronglyTypedRegistryItemTest : StronglyTypedRegistryItemTestCase<ADConfig>
	{
		protected override StronglyTypedRegistryItem<ADConfig, ADConfig> GetNewRegistryItem()
		{
			return new ADConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", Integration.RegistryStorageFlags.System, Integration.RegistryOptions.IsOnlyEditableBySupportIfHosted, ADConfig.DefaultValue);
		}
	}

	[TestedType(typeof(ADConfigRegistryDataType))]
	class ADConfigRegistryDataTypeNonPersistentObjectRegistryDataTypeTestCase : NonPersistentBusinessObjectRegistryDataTypeTestCase<ADConfigRegistryDataType>
	{
		protected override ADConfigRegistryDataType GetNewDataType()
		{
			return new ADConfigRegistryDataType(ADConfig.DefaultValue);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var config = ADConfig.DefaultValue;
			var xml = @"<ADConfig><IsADIntegrationEnabled>N</IsADIntegrationEnabled><EntitiesToSyncCode>ALL</EntitiesToSyncCode><SyncModeCode>AD</SyncModeCode><IsSingleSignOn>N</IsSingleSignOn></ADConfig>";
			var bytes = System.Text.Encoding.Unicode.GetBytes(xml);

			return new[] { new ValidSampleAndBinaryValueInDB(config, bytes) };
		}

		protected override string ExpectedEditorName
		{
			get { return "ADRegistryControlItemEditor"; }
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return true; }
		}
	}

	[TestedType(typeof(ADConfigRegistryDataType))]
	class ADConfigRegistryDataTypeTest : RegistryDataTypeTestCase<ADConfigRegistryDataType>
	{
		public void TestSerialiseAndDeserialise()
		{
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var bytes = dataType.Serialise(ADConfig.DefaultValue);
			var deserialisedConfig = dataType.Deserialise(bytes);
			AssertEquals(ADConfig.DefaultValue, deserialisedConfig);
		}

		public void TestDataTypeCode()
		{
			AssertEquals(RegistryDataTypes.Codes.Binary, new ADConfigRegistryDataType(ADConfig.DefaultValue).Code);
		}

		public void TestEditorClassAndAssembly()
		{
			var expectedType = Type.GetType("Enterprise.Security.ActiveDirectory.GUI.Registry.ADRegistryControlItemEditor, Enterprise.Security.ActiveDirectory.GUI");
			AssertNotNull("ADRegistryControlItemEditor should be a valid type", expectedType);

			var attribute = typeof(ADConfigRegistryDataType).GetCustomAttributes(typeof(RegistryEditorAttribute), inherit: false)[0] as RegistryEditorAttribute;
			AssertNotNull("RegistryEditorAttribute should be present", attribute);

			string expectedName = string.Format("{0}, {1}", expectedType.FullName, expectedType.Assembly.GetName().Name);
			AssertEquals("Attribute should specify fully-qualified type with assembly", expectedName, attribute.TypeName);
		}

		protected override ADConfigRegistryDataType GetNewDataType()
		{
			return new ADConfigRegistryDataType(ADConfig.DefaultValue);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var config = ADConfig.DefaultValue;
			var xml = @"<ADConfig><IsADIntegrationEnabled>N</IsADIntegrationEnabled><EntitiesToSyncCode>ALL</EntitiesToSyncCode><SyncModeCode>AD</SyncModeCode><IsSingleSignOn>N</IsSingleSignOn></ADConfig>";
			var bytes = System.Text.Encoding.Unicode.GetBytes(xml);

			return new[] { new ValidSampleAndBinaryValueInDB(config, bytes) };
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue
		{
			get { return true; }
		}
	}

	class ADConfigRegistryDataTypeTest2 : TestCaseWithFactoryAndMocks
	{
		[ExpectNoExceptions]
		public void TestValidate_ActivatingIntegration_ShouldCallActivationManager()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(ADConfig.DefaultValue);
			director.Setup(d => d.HasChanges).Returns(false);

			dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = true }, Guid.Empty, Guid.Empty, Guid.Empty);

			director.Verify(d => d.EnableIntegration(EntitiesToSync.UsersAndGroups));
		}

		[ExpectNoExceptions]
		[GuiTest]
		public void TestValidate_ActivatingIntegration_ShouldUseProposedSyncModeValues()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;

#pragma warning disable 0618
			var item = ActiveDirectoryRegistry.Instance.ActiveDirectoryConfig;
#pragma warning restore 0618
			var dataType = (ADConfigRegistryDataType)item.DataType;
			var directorySearcher = new Mock<IDirectorySearcher>();

			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;
			try
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_FullName = "sauron";
				staff.GS_SystemLastEditTimeUtc = ZDateTime.Now.AddDays(-1);
				staff.GS_IsController = true;

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny");
				directorySearcher.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);

				dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.EnterpriseIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty);

				AssertEquals("Should have set the FullName on the DirectoryEntry", "sauron", directoryEntry[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(Enterprise.ZArchitecture.Schema.GlbStaffSchema.GS_FullName)]);
			}
			finally
			{
				DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			}
		}

		[ExpectNoExceptions]
		public void TestValidate_SameActivationValue_ShouldNotCallActivationManager()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(ADConfig.DefaultValue);
			director.Setup(d => d.HasChanges).Returns(false);

			dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = false, EntitiesToSync = EntitiesToSync.UsersAndGroups }, Guid.Empty, Guid.Empty, Guid.Empty);

			director.Verify(d => d.EnableIntegration(It.IsAny<EntitiesToSync>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestValidate_DeactivatingIntegration_ShouldCallActivationManager()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(new ADConfig { IsADIntegrationEnabled = true });
			director.Setup(d => d.HasChanges).Returns(false);

			dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = false }, Guid.Empty, Guid.Empty, Guid.Empty);

			director.Verify(d => d.DisableIntegration(false));
		}

		public void TestValidate_WhenCancellingActivationManager_ShouldThrowValidationException()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(ADConfig.DefaultValue);
			director.Setup(d => d.HasChanges).Returns(false);
			director.Setup(d => d.EnableIntegration(EntitiesToSync.UsersAndGroups)).Throws(new DirectoryServicesException("Canceled!"));
			var ex = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = true }, Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Canceled!", ex.Message);

			director.Verify(d => d.EnableIntegration(EntitiesToSync.UsersAndGroups));
		}

		[ExpectNoExceptions]
		public void TestValidate_WhenHasChanges_ShouldSaveChanges()
		{
			var item = new ADConfigRegistryItem("TestADConfigItem", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, ADConfig.DefaultValue);
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<ADActivationDirectorForSaveChangesTest>();
			dataType.ActivationDirector = director.Object;
			item.DataType = dataType;

			var newValue = ADConfig.DefaultValue;
			var registryItemInternals = (IRegistryItemInternals)item;

			var config = new ADConfig { IsADIntegrationEnabled = true };
			dataType.ValidateBeforeRegistryFormSave(item, config, Guid.Empty, Guid.Empty, Guid.Empty);
			dataType.Validate(item, config, Guid.Empty, Guid.Empty, Guid.Empty);
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			item.OnUpdate(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			director.Verify(d => d.SaveChanges());
		}

		[ExpectNoExceptions]
		public void TestValidate_WhenNoChanges_ShouldNotSaveChanges()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<ADActivationDirectorForSaveChangesTest>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(ADConfig.DefaultValue);
			var config = new ADConfig { IsADIntegrationEnabled = true };
			dataType.Validate(item.Object, config, Guid.Empty, Guid.Empty, Guid.Empty);
			director.Verify(d => d.SaveChanges(), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestValidate_BeforeStandardValidation_ShouldNotSaveChanges()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<ADActivationDirectorForSaveChangesTest>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(ADConfig.DefaultValue);

			var config = new ADConfig { IsADIntegrationEnabled = true };
			dataType.ValidateBeforeRegistryFormSave(item.Object, config, Guid.Empty, Guid.Empty, Guid.Empty);
			director.Verify(d => d.SaveChanges(), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestValidate_ChangingSyncEntity_UserToAll()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersOnly });
			director.Setup(d => d.HasChanges).Returns(false);

			dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersAndGroups }, Guid.Empty, Guid.Empty, Guid.Empty);

			director.Verify(d => d.EnableIntegration(EntitiesToSync.UsersAndGroups));
		}

		[ExpectNoExceptions]
		public void TestValidate_ChangingSyncEntity_AllToUser()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersAndGroups });
			director.Setup(d => d.HasChanges).Returns(false);

			dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersOnly }, Guid.Empty, Guid.Empty, Guid.Empty);

			director.Verify(d => d.DisableIntegration(true));
		}

		[ExpectNoExceptions]
		public void TestValidate_WhenADAlreadyDisabled_DoesNotCallDisableIntegration()
		{
			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(new ADConfig { IsADIntegrationEnabled = false, EntitiesToSync = EntitiesToSync.UsersAndGroups });
			director.Setup(d => d.HasChanges).Returns(false);

			dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = false, EntitiesToSync = EntitiesToSync.UsersOnly }, Guid.Empty, Guid.Empty, Guid.Empty);

			director.Verify(d => d.DisableIntegration(It.IsAny<bool>()), Times.Never);
		}

		public void TestValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired()
		{
			AssertValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired(false, SyncMode.ADIsMaster, SyncDirection.OneWay, SyncDirection.OneWay);
			AssertValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired(true, SyncMode.ADIsMaster, SyncDirection.TwoWay, SyncDirection.TwoWay);
			AssertValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired(true, SyncMode.EnterpriseIsMaster, SyncDirection.TwoWay, SyncDirection.TwoWay);
			AssertValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired(true, SyncMode.EnterpriseIsMaster, SyncDirection.OneWay, SyncDirection.OneWay);
			AssertValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired(true, SyncMode.ADIsMaster, SyncDirection.OneWay, SyncDirection.TwoWay);
		}

		void AssertValidate_WhenChangingIntergration_ShouldThrowValidationExceptionWhenRequired(bool expectException, SyncMode syncMode, SyncDirection syncDirection, SyncDirection syncDirectionGroup = SyncDirection.OneWay)
		{
			//Default search for unit test is using ADTestAdminAccount. We don't want to use it for this test
			ObjectFactory.DisposeSubstitutions();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			var domainCredentials = ADTestHelper.CreateDomainCredentials();
			domainCredentials.DomainUserName = TestConstants.ADTestUserAccountNoOURight.NameWithDomain;
			domainCredentials.DomainUserPassword = TestConstants.ADTestUserAccountNoOURight.Password;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials));

			var item = new Mock<IRegistryItem>();
			var dataType = new ADConfigRegistryDataType(ADConfig.DefaultValue);
			var director = new Mock<IADActivationDirector>();
			dataType.ActivationDirector = director.Object;

			item.Setup(i => i.Value).Returns(new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersAndGroups, SyncMode = SyncMode.ADIsMaster, SyncDirection = SyncDirection.OneWay, SyncDirectionGroup = SyncDirection.OneWay });
			director.Setup(d => d.HasChanges).Returns(false);

			if (expectException)
			{
				var ex = AssertExceptionThrown<RegistryValidationException>(() => dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersAndGroups, SyncMode = syncMode, SyncDirection = syncDirection, SyncDirectionGroup = syncDirectionGroup }, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals(@"Integration cannot be modified.
Please cancel your changes, correct the issues below and save your changes before retrying to modify the integration:
- The user ADTest_User_No_OU@sand.wtg.zone defined in the registry setting 'System -> Staff -> Active Directory -> Domain Credentials Collection' does not have write privileges to the Organizational Units for domain sand.wtg.zone. Active Directory Synchronization will not work.", ex.Message);
			}
			else
			{
				AssertNoExceptionThrown(() => dataType.ValidateBeforeRegistryFormSave(item.Object, new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersAndGroups, SyncMode = syncMode, SyncDirection = syncDirection, SyncDirectionGroup = syncDirectionGroup }, Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}
	}

	public abstract class ADActivationDirectorForSaveChangesTest : IADActivationDirector
	{
		public void EnableIntegration(EntitiesToSync entitiesToSync) => HasChanges = true;

		public bool ConfirmChanges(IEnumerable<EntitySynchronisedEventArgs> e) => true;

		public void DisableIntegration(bool disableGroupOnly) => HasChanges = true;

		public void ProgressUpdated(SyncProgressEventArgs e)
		{
		}

		public bool HasChanges { get; private set; }

		public abstract void SaveChanges();
	}
}
