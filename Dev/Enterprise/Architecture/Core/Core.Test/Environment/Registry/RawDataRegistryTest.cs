using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.ServiceManager;
using Enterprise.MailManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.DevTools.Definitions;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.ZArchitecture.Environment.RawDataRegistry;
using Constants = Enterprise.Core.Constants;

[assembly: UsesConstants(typeof(CargoWise.Licensing.Constants))]
namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(RawDataRegistry))]
	sealed class RawDataRegistryTest : RegistryItemSetTestCase<RawDataRegistry>
	{
		public void TestUseModernSqlSecuritySystemHostedInCWUnitTesting()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest("SYD");

			// Act & Assert
			AssertEquals("Category", "System/Database", ItemSet.UseModernSqlSecuritySystem.Category);
			AssertEquals("StorageFlags", RegistryStorageFlags.System, ItemSet.UseModernSqlSecuritySystem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, ItemSet.UseModernSqlSecuritySystem.Options);
			AssertEquals("DefaultValue", true, ItemSet.UseModernSqlSecuritySystem.DefaultValue);
		}

		public void TestUseModernSqlSecuritySystemHostedInCWProduction()
		{
			// Arrange
			var productRegistrationMock = new Mock<IProductRegistration>();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();

			productRegistrationMock.Setup(productRetistration => productRetistration.Key).Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock.Setup(productRetistrationKey => productRetistrationKey.HostedLocation).Returns("SYD");

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				// Act & Assert
				AssertEquals("Category", "System/Database", ItemSet.UseModernSqlSecuritySystem.Category);
				AssertEquals("StorageFlags", RegistryStorageFlags.System, ItemSet.UseModernSqlSecuritySystem.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, ItemSet.UseModernSqlSecuritySystem.Options);
				AssertEquals("DefaultValue", false, ItemSet.UseModernSqlSecuritySystem.DefaultValue);
			}
		}

		[TestDate(2025, 2, 4, 14, 0, 0)]
		public void TestMailServerOnUpdateAction()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var schedule = factory.New<IStmScheduleTask>();
			schedule.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			schedule.S5_ScheduleType = "IMS";
			factory.Save();

			var serviceManagerGovernorMock = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(serviceManagerGovernorMock.Object))
			{
				// Act
				AssertNoExceptionThrown(() =>
				{
					((RegistryItemImpl)ItemSet.MailServer).OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, "xxx@xxx.com");
				});

				// Assert
				var expectedNextRunDateOffset = ZDateTime.Truncate(ZDateTime.UtcNow.AddSeconds(RegistryRefresh.FrequencyInSeconds.Value), TimeSpan.TicksPerSecond).UtcToDateTimeOffset().ToDateTimeOffsetSafe();
				serviceManagerGovernorMock.Verify(s => s.SetServiceTaskNextRuntime("IMS", expectedNextRunDateOffset), Times.Once);
			}
		}

		[TestDate(2025, 2, 4, 14, 0, 0)]
		public void TestSmtpServerOnUpdateAction()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var schedule = factory.New<IStmScheduleTask>();
			schedule.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			schedule.S5_ScheduleType = "OMS";
			factory.Save();

			var serviceManagerGovernorMock = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(serviceManagerGovernorMock.Object))
			{
				// Act
				AssertNoExceptionThrown(() =>
				{
					((RegistryItemImpl)ItemSet.SMTPServer).OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, "xxx@xxx.com");
				});

				// Assert
				var expectedNextRunDateOffset = ZDateTime.Truncate(ZDateTime.UtcNow.AddSeconds(RegistryRefresh.FrequencyInSeconds.Value), TimeSpan.TicksPerSecond).UtcToDateTimeOffset().ToDateTimeOffsetSafe();
				serviceManagerGovernorMock.Verify(s => s.SetServiceTaskNextRuntime("OMS", expectedNextRunDateOffset));
			}
		}

		public void TestUseModernSqlSecuritySystemSelfHostedUnitTesting()
		{
			// Arrange
			EnvProxy.SetHostedLocationForTest(CargoWise.Licensing.Constants.NotHostedWithCargoWise);

			// Act & Assert
			AssertEquals("Category", "System/Database", ItemSet.UseModernSqlSecuritySystem.Category);
			AssertEquals("StorageFlags", RegistryStorageFlags.System, ItemSet.UseModernSqlSecuritySystem.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, ItemSet.UseModernSqlSecuritySystem.Options);
			AssertEquals("DefaultValue", true, ItemSet.UseModernSqlSecuritySystem.DefaultValue);
		}

		public void TestUseModernSqlSecuritySystemSelfHostedProduction()
		{
			// Arrange
			var productRegistrationMock = new Mock<IProductRegistration>();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();

			productRegistrationMock.Setup(productRetistration => productRetistration.Key).Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock.Setup(productRetistrationKey => productRetistrationKey.HostedLocation).Returns(CargoWise.Licensing.Constants.NotHostedWithCargoWise);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				// Act & Assert
				AssertEquals("Category", "System/Database", ItemSet.UseModernSqlSecuritySystem.Category);
				AssertEquals("StorageFlags", RegistryStorageFlags.System, ItemSet.UseModernSqlSecuritySystem.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, ItemSet.UseModernSqlSecuritySystem.Options);
				AssertEquals("DefaultValue", false, ItemSet.UseModernSqlSecuritySystem.DefaultValue);
			}
		}

		public void TestCWSupportLoginTokenCertificate()
		{
			AssertEquals(IdentityRegistry.CWSupportLoginTokenCertificateRegistryKey, ItemSet.CWSupportLoginTokenCertificate.Name);
			AssertEquals("System/Staff/Support User Login", ItemSet.CWSupportLoginTokenCertificate.Category);
			AssertEquals("CWSupport Login Token Certificate", ItemSet.CWSupportLoginTokenCertificate.Caption);
			AssertEquals("This certificate is used to validate CWSupport account login token.", ItemSet.CWSupportLoginTokenCertificate.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.CWSupportLoginTokenCertificate.Storage);
			AssertEquals(RegistryOptions.IsHidden | RegistryOptions.IsReadOnly, ItemSet.CWSupportLoginTokenCertificate.Options);
			AssertEquals(IdentityRegistry.GetCWSupportLoginTokenCertificateDefaultValue(), ItemSet.CWSupportLoginTokenCertificate.DefaultValue);
		}

		public void TestScimAudienceId()
		{
			AssertEquals("Name", "ScimAudienceId", ItemSet.ScimAudienceId.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimAudienceId.Categories.Length);
			AssertEquals("Category", "System/SCIM", ItemSet.ScimAudienceId.Category);
			AssertEquals("Caption", "Audience Id", ItemSet.ScimAudienceId.Caption);
			AssertEquals("Hint", @"Enter the Azure audience (or Application ID) used for token authentication.", ItemSet.ScimAudienceId.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimAudienceId.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ScimAudienceId.Options);
			AssertEquals("DefaultValue", "8adf8e6e-67b2-4cf2-a259-e3dc5476c621", ItemSet.ScimAudienceId.DefaultValue);
		}

		public void TestScimKnownEndpointPath()
		{
			AssertEquals("Name", "ScimKnownEndpointPath", ItemSet.ScimKnownEndpointPath.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimKnownEndpointPath.Categories.Length);
			AssertEquals("Category", "System/SCIM", ItemSet.ScimKnownEndpointPath.Category);
			AssertEquals("Caption", "Known Endpoint Path", ItemSet.ScimKnownEndpointPath.Caption);
			AssertEquals("Hint", @"Enter the Known Endpoint Path for the SCIM service. The default option points to the Azure AD well-known endpoint.", ItemSet.ScimKnownEndpointPath.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimKnownEndpointPath.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ScimKnownEndpointPath.Options);
			AssertEquals("DefaultValue", "/.well-known/openid-configuration", ItemSet.ScimKnownEndpointPath.DefaultValue);
		}

		public void TestScimIssuer()
		{
			AssertEquals("Name", "ScimIssuer", ItemSet.ScimIssuer.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimIssuer.Categories.Length);
			AssertEquals("Category", "System/SCIM", ItemSet.ScimIssuer.Category);
			AssertEquals("Caption", "Issuer", ItemSet.ScimIssuer.Caption);
			AssertEquals("Hint", @"Used to identify the issuer, or ""authorization server"" that constructs and returns the token.", ItemSet.ScimIssuer.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimIssuer.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ScimIssuer.Options);
			AssertEquals("DefaultValue", "", ItemSet.ScimIssuer.DefaultValue);
		}

		public void TestScimLoggingKafkaBrokers()
		{
			AssertEquals("Name", "ScimLoggingKafkaBrokers", ItemSet.ScimLoggingKafkaBrokers.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimLoggingKafkaBrokers.Categories.Length);
			AssertEquals("Category", "System/SCIM/Logging", ItemSet.ScimLoggingKafkaBrokers.Category);
			AssertEquals("Caption", "Kafka Target Brokers", ItemSet.ScimLoggingKafkaBrokers.Caption);
			AssertEquals("Hint", @"Specifies the Kafka brokers as NLog Kafka target config 'brokers' attribute for SCIM.", ItemSet.ScimLoggingKafkaBrokers.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimLoggingKafkaBrokers.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ScimLoggingKafkaBrokers.Options);
			AssertEquals("DefaultValue", "106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,108-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,109-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,110-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044", ItemSet.ScimLoggingKafkaBrokers.DefaultValue);
		}

		public void TestScimCanLogin()
		{
			AssertEquals("Name", "ScimCanLogin", ItemSet.ScimCanLogin.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimCanLogin.Categories.Length);
			AssertEquals("Category", "System/SCIM", ItemSet.ScimCanLogin.Category);
			AssertEquals("Caption", "Can Login Default", ItemSet.ScimCanLogin.Caption);
			AssertEquals("Hint", @"Staff Can Login default value upon provision.", ItemSet.ScimCanLogin.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimCanLogin.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ScimCanLogin.Options);
			AssertEquals("DefaultValue", true, ItemSet.ScimCanLogin.DefaultValue);
		}

		public void TestScimLoggingKafkaTopic_ProductionSystem() => ScimLoggingKafkaTopic(true);
		public void TestScimLoggingKafkaTopic_NonProductionSystem() => ScimLoggingKafkaTopic(false);

		public void ScimLoggingKafkaTopic(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, EnvProxy.Instance.IsProductionSystem);

			string expectedTopic = isProductionSystem
							? "topic-au2-prod-scim-service-logs-prod"
							: "topic-au1-test-scim-service-logs-test";

			AssertEquals("Name", "ScimLoggingKafkaTopic", ItemSet.ScimLoggingKafkaTopic.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimLoggingKafkaTopic.Categories.Length);
			AssertEquals("Category", "System/SCIM/Logging", ItemSet.ScimLoggingKafkaTopic.Category);
			AssertEquals("Caption", "Kafka Target Topic", ItemSet.ScimLoggingKafkaTopic.Caption);
			AssertEquals("Hint", @"Specifies the Kafka topic as NLog Kafka target config 'topic' attribute for SCIM.", ItemSet.ScimLoggingKafkaTopic.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimLoggingKafkaTopic.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ScimLoggingKafkaTopic.Options);
			AssertEquals("DefaultValue", expectedTopic, ItemSet.ScimLoggingKafkaTopic.DefaultValue);
		}

		public void TestScimLoggingKafkaTopicUsername_ProductionSystem() => ScimLoggingKafkaTopicUsername(true);
		public void TestScimLoggingKafkaTopicUsername_NonProductionSystem() => ScimLoggingKafkaTopicUsername(false);

		public void ScimLoggingKafkaTopicUsername(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, EnvProxy.Instance.IsProductionSystem);

			string expectedTopicUsername = isProductionSystem
							? "topic-au2-prod-scim-service-logs-prod"
							: "topic-au1-test-scim-service-logs-test";

			AssertEquals("Name", "ScimLoggingKafkaTopicUsername", ItemSet.ScimLoggingKafkaTopicUsername.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimLoggingKafkaTopicUsername.Categories.Length);
			AssertEquals("Category", "System/SCIM/Logging", ItemSet.ScimLoggingKafkaTopicUsername.Category);
			AssertEquals("Caption", "Kafka Target Topic Username", ItemSet.ScimLoggingKafkaTopicUsername.Caption);
			AssertEquals("Hint", @"Specifies the Kafka topic username for SCIM.", ItemSet.ScimLoggingKafkaTopicUsername.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimLoggingKafkaTopicUsername.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ScimLoggingKafkaTopicUsername.Options);
			AssertEquals("DefaultValue", expectedTopicUsername, ItemSet.ScimLoggingKafkaTopicUsername.DefaultValue);
		}

		public void TestScimLoggingKafkaTopicPassword()
		{
			AssertEquals("Name", "ScimLoggingKafkaTopicPassword", ItemSet.ScimLoggingKafkaTopicPassword.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimLoggingKafkaTopicPassword.Categories.Length);
			AssertEquals("Category", "System/SCIM/Logging", ItemSet.ScimLoggingKafkaTopicPassword.Category);
			AssertEquals("Caption", "Kafka Target Topic Password", ItemSet.ScimLoggingKafkaTopicPassword.Caption);
			AssertEquals("Hint", @"Specifies the Kafka topic password for SCIM.", ItemSet.ScimLoggingKafkaTopicPassword.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimLoggingKafkaTopicPassword.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.ScimLoggingKafkaTopicPassword.Options);
			AssertEquals("DefaultValue", "", ItemSet.ScimLoggingKafkaTopicPassword.DefaultValue);
			AssertEquals(true, (ItemSet.ScimLoggingKafkaTopicPassword.DataType as StringRegistryDataType).IsEncrypted);
		}

		public void TestScimCodeGenerationCharacters()
		{
			AssertEquals("Name", "ScimCodeGenerationCharacters", ItemSet.ScimCodeGenerationCharacters.Name);
			AssertEquals("Categories.Length", 1, ItemSet.ScimCodeGenerationCharacters.Categories.Length);
			AssertEquals("Category", "System/SCIM", ItemSet.ScimCodeGenerationCharacters.Category);
			AssertEquals("Caption", "Additional characters for code generation", ItemSet.ScimCodeGenerationCharacters.Caption);
			AssertEquals("Hint", "This registry determines a set of additional characters that are used in conjunction with the letters for the Staff codes during SCIM imports. Letters and numbers only provide 46656 combinations. Default full set of characters increases this number to 287496 combinations. Changing this registry might take up to 1 hour to be applied, please wait before starting provisioning.", ItemSet.ScimCodeGenerationCharacters.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ScimCodeGenerationCharacters.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.ScimCodeGenerationCharacters.Options);
			AssertEquals("DefaultValue", "0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~", ItemSet.ScimCodeGenerationCharacters.DefaultValue);
		}

		public void TestScimCodeGenerationCharacters_Validation()
		{
			var lettersError = "Letters are used by default and are not allowed in this registry.";
			var charsError = "Eligible characters are for codes between 33 and 126 except letters, single quote (') and question mark (?): 0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~";
			var duplicateError = "Duplicate character: {0}";
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", lettersError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "abc"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", lettersError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "#$a"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", lettersError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "a"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", lettersError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "z!@"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", lettersError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "!#$x^!"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", charsError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "@$%^?"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", charsError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "*'"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", string.Format(duplicateError, "!"), delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "!!!&*("); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", charsError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "☻%$"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", charsError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Ω"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", charsError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Ω%!"); });
			AssertExceptionThrown<RegistryValidationException>("Should show correct error", charsError, delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "(@Ω"); });
			AssertNoExceptionThrown(delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123"); });
			AssertNoExceptionThrown(delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123$%&*("); });
			AssertNoExceptionThrown(delegate { ItemSet.ScimCodeGenerationCharacters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~"); });
		}

		public void TestEnableComplianceRiskWhenNoImplementationOfOnUpdateAction()
		{
			AssertExceptionThrown<NullReferenceException>("Registry EnableComplianceRisk no implementation of OnUpdateAction", () => { ItemSet.EnableComplianceRisk.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, false); });
		}

		public void TestEnableComplianceRiskDescriptionsAndDefaultValue()
		{
			AssertEquals("Name", "EnableComplianceRisk", ItemSet.EnableComplianceRisk.Name);
			AssertEquals("Categories.Length", 1, ItemSet.EnableComplianceRisk.Categories.Length);
			AssertEquals("Category", "Compliance", ItemSet.EnableComplianceRisk.Category);
			AssertEquals("Caption", "Enable ComplianceWise", ItemSet.EnableComplianceRisk.Caption);
			AssertEquals("Hint", @"This registry should only be updated by the Master Data Product team.
When enabled, ComplianceWise will be available on this system. Customers will also need to enable ComplianceWise on specific modules.
When disabled, ComplianceWise will not be available on this system.", ItemSet.EnableComplianceRisk.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableComplianceRisk.Storage);
			AssertEquals("DefaultValue", true, ItemSet.EnableComplianceRisk.DefaultValue);
		}

		public void TestEnableComplianceRiskWhenCurrentUserIsSupportUserExpectedRegistryVisible()
		{
			AssertEquals("Registry options IsOnlyForSupport", RegistryOptions.IsOnlyForSupport, ItemSet.EnableComplianceRisk.Options);
			AssertEquals("Registry is visible", true, ItemSet.EnableComplianceRisk.IsVisible(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestEnableComplianceRiskWhenCurrentUserIsNotSupportUserExpectedRegistryHidden()
		{
			using (CurrentUserChanger.SwitchToNewUserTemporarily(User.WebUserName))
			{
				AssertEquals("Registry options IsHidden", RegistryOptions.IsHidden, ItemSet.EnableComplianceRisk.Options);
				AssertEquals("Registry is hidden", false, ItemSet.EnableComplianceRisk.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			}
		}

		public void TestEnableComplianceRiskWhenCurrentUserIsNullShouldNotThrowTargetInvocationException()
		{
			var environmentMock = new Mock<IEnvironment>();
			environmentMock.Setup(x => x.CurrentUser).Returns<IUser>(null);

			var envMock = new Mock<IEnv>();
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					AssertEquals(RegistryOptions.IsHidden, ItemSet.EnableComplianceRisk.Options);
				});
			}
		}

		public void TestOrganisationRequiredFieldsDescriptions()
		{
			AssertHint(ItemSet.OrgBrokerRequiredFields, "Broker");
			AssertHint(ItemSet.OrgCarrierRequiredFields, "Carrier");
			AssertHint(ItemSet.OrgCompetitorRequiredFields, "Competitor");
			AssertHint(ItemSet.OrgConsigneeRequiredFields, "Consignee");
			AssertHint(ItemSet.OrgConsignorRequiredFields, "Consignor");
			AssertHint(ItemSet.OrgContainerYardRequiredFields, "Container Yard");
			AssertHint(ItemSet.OrgCreditorRequiredFields, "Creditor");
			AssertHint(ItemSet.OrgCTORequiredFields, "CTO");
			AssertHint(ItemSet.OrgDebtorRequiredFields, "Debtor");
			AssertHint(ItemSet.OrgForwarderRequiredFields, "Forwarder");
			AssertHint(ItemSet.OrgPackDepotRequiredFields, "Pack Depot");
			AssertHint(ItemSet.OrgTransportClientRequiredFields, "Transport Client");
			AssertHint(ItemSet.OrgWarehouseRequiredFields, "Warehouse");

			void AssertHint(IRegistryItem item, string type)
			{
				AssertEquals($"Required fields for {type} organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service.", item.Hint);
			}
		}

		public void TestProductivityWiseModeEnabled_ShouldBeHiddenAndDisabledByDefault()
		{
			AssertEquals(BrandingFactory.Instance is ProductivityWiseBranding, ItemSet.ProductivityWiseModeEnabled.DefaultValue);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.ProductivityWiseModeEnabled.Options);
		}

		public void TestEnableEDIMessageInterpreter()
		{
			AssertEquals("Name", "EnableEDIMessageInterpreter", ItemSet.EnableEDIMessageInterpreter.Name);
			AssertEquals("Categories.Length", 1, ItemSet.EnableEDIMessageInterpreter.Categories.Length);
			AssertEquals("Category", "System/Messaging", ItemSet.EnableEDIMessageInterpreter.Category);
			AssertEquals("Caption", "Enable EDIMessage Interpreter", ItemSet.EnableEDIMessageInterpreter.Caption);
			AssertEquals("Hint", "This registry item let you enable or disable completely the EDIMessage Interpreter module.", ItemSet.EnableEDIMessageInterpreter.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.EnableEDIMessageInterpreter.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.EnableEDIMessageInterpreter.Options);
			AssertEquals("DefaultValue", false, ItemSet.EnableEDIMessageInterpreter.DefaultValue);
		}

		public void TestMakeSalesRepMandatoryForTempOrganization()
		{
			AssertEquals(false, ItemSet.MakeSalesRepMandatoryForTempOrganization.DefaultValue);
			AssertEquals("MakeSalesRepMandatoryForTempOrganization", ItemSet.MakeSalesRepMandatoryForTempOrganization.Name);
			AssertEquals("Master Data/Organizations/Temp. Organization Required Fields", ItemSet.MakeSalesRepMandatoryForTempOrganization.Category);
			AssertEquals("Make Sales Rep Mandatory", ItemSet.MakeSalesRepMandatoryForTempOrganization.Caption);
			AssertEquals("If registry is turned on, when saving changes on temporary organizations (new or edits), if it does not have a sales rep record in staff assignment, it will auto add in a Sales rep record but leaving Initials field blank with an error, so that it cannot be saved until it is entered.", ItemSet.MakeSalesRepMandatoryForTempOrganization.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.MakeSalesRepMandatoryForTempOrganization.Storage);
		}

		public void TestRunSelectTopNAsRowNumberQuery()
		{
			AssertEquals(false, ItemSet.RunSelectTopNAsRowNumberQuery.DefaultValue);
			ItemSet.RunSelectTopNAsRowNumberQuery.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.RunSelectTopNAsRowNumberQuery.Value);
		}

		public void TestShowSystemGeneratedContactsOnOrgDocuments()
		{
			AssertEquals(false, ItemSet.ShowSystemGeneratedContactsOnOrgDocuments.DefaultValue);
			ItemSet.ShowSystemGeneratedContactsOnOrgDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.ShowSystemGeneratedContactsOnOrgDocuments.Value);
		}

		public void TestPackageWeightUnit()
		{
			AssertEquals("KG", ItemSet.PackageWeightUnit.DefaultValue);
			ItemSet.PackageWeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "T");
			AssertEquals("Value set", "T", ItemSet.PackageWeightUnit.Value);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch, ItemSet.PackageWeightUnit.Storage);
		}

		public void TestPackageVolumeUnit()
		{
			AssertEquals("M3", ItemSet.PackageVolumeUnit.DefaultValue);
			ItemSet.PackageVolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "L");
			AssertEquals("Value set", "L", ItemSet.PackageVolumeUnit.Value);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch, ItemSet.PackageWeightUnit.Storage);
		}

		public void TestDefaultStockUnit()
		{
			AssertEquals("UNT", ItemSet.DefaultStockUnits.DefaultValue);
			ItemSet.DefaultStockUnits.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NO");
			AssertEquals("Default Stock Unit set", "NO", ItemSet.DefaultStockUnits.Value);
			AssertContainsExactElementsInAnyOrder(new string[] { "Customs/Products", "Warehouse/Products" }, ItemSet.DefaultStockUnits.Categories);
		}

		public void TestUnitConversionPackTypesValidation()
		{
			var expectedDefaultValue = false;
			var expectedOptions = RegistryOptions.Default;
			var item = ItemSet.UnitConversionPackTypesValidation;

			AssertEquals("Name", "UnitConversionPackTypesValidation", item.Name);
			AssertEquals("Caption", "Unit Conversion Pack Types Validation", item.Caption);
			AssertEquals("Hint", @"Enabling this setting will display an error if the entered pack type or parent pack type of a unit conversion is not defined in the list of valid pack types.

Enabling this setting will also reject products with invalid pack types during importing CSV or Native XML files.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", expectedOptions, item.Options);
			AssertEquals("DefaultValue", expectedDefaultValue, item.DefaultValue);
			AssertContainsExactElementsInAnyOrder(new string[] { "Customs/Products", "Warehouse/Products" }, item.Categories);

			RegistryTester.SetValue(item, !expectedDefaultValue);
			AssertEquals("Value", !expectedDefaultValue, item.Value);
		}

		public void TestDefaultPurgeReportStatisticsLogs()
		{
			AssertEquals("Purge Report Statistics Logs older than n days registry setting defaults to 60", 60, ItemSet.PurgeReportStatisticsLogs.DefaultValue);
		}

		public void TestInternalApplicationActivityTrackingInterval()
		{
			var item = ItemSet.InternalApplicationActivityTrackingInterval;

			AssertEquals("InternalApplicationActivityTrackingInterval", item.Name);
			AssertEquals("System/Staff/Activity Logging", item.Category);
			AssertEquals("Internal application Activity Tracking Interval", item.Caption);
			AssertEquals("Customizes the interval of when the Activity Logs are updated.  The minimum value is 10 seconds and the maximum value is 600 seconds.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(300, item.DefaultValue);
			AssertEquals(10, (int)((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals(600, (int)((IntRegistryDataType)item.DataType).UpperBound);
		}

		public void TestDefaulDepotForHVLV()
		{
			AssertEquals(Guid.Empty, ItemSet.DefaultDepotForBuildHVLV.DefaultValue);
			var guid = Guid.NewGuid();
			ItemSet.DefaultDepotForBuildHVLV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, guid);
			AssertEquals("Default Depot For HVLV", guid, ItemSet.DefaultDepotForBuildHVLV.Value);
		}

		public void TestRunSearchOnEnteringAModuleDefaultIsFalse()
		{
			AssertEquals(false, ItemSet.RunSearchOnEnteringAModule.DefaultValue);
		}

		public void TestAutoRunSearchFromFindBoxDefaultIsFalse()
		{
			AssertEquals(true, ItemSet.AutoRunSearchFromFindBox.DefaultValue);
		}

		public void TestResidencyStatus_DefaultValues()
		{
			var defaultValue = ItemSet.ResidencyStatus.DefaultValue;
			AssertEquals("DefaultValue.Count", 4, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"CTZ\")", "Citizen", defaultValue.GetDescriptionFromCode("CTZ"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"RES\")", "Permanent Resident", defaultValue.GetDescriptionFromCode("RES"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"TMP\")", "Temporary Visa Holder", defaultValue.GetDescriptionFromCode("TMP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"STD\")", "Student Visa Holder", defaultValue.GetDescriptionFromCode("STD"));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("TSV", "Test Visa");

			ItemSet.ResidencyStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, ItemSet.ResidencyStatus.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"TSV\")", "Test Visa", ItemSet.ResidencyStatus.Value.GetDescriptionFromCode("TSV"));

			AssertEquals("ResidencyStatus", ItemSet.ResidencyStatus.Name);
			AssertEquals("System/Staff/HRMS", ItemSet.ResidencyStatus.Category);
		}

		public void TestStandardWorkingHours_DefaultValues()
		{
			var expectedHint = @"Describes the standard weekly working hours for staff employed full-time at the provided branch or company. Working hours must be specified in HH:mm format.";
			AssertEquals("Category", "System/Staff/HRMS", ItemSet.StandardWorkingHours.Category);
			AssertEquals("Name", "StandardWorkingHours", ItemSet.StandardWorkingHours.Name);
			AssertEquals("Hint", expectedHint, ItemSet.StandardWorkingHours.Hint);
			AssertEquals("DefaultValue", "40:00", ItemSet.StandardWorkingHours.DefaultValue);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, ItemSet.StandardWorkingHours.Storage);

			var companyDefVal = ItemSet.StandardWorkingHours.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("Company default value", "40:00", companyDefVal);

			var branchDefVal = ItemSet.StandardWorkingHours.GetValueWithoutFallback(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty);
			AssertEquals("Branch default value", "40:00", branchDefVal);
		}

		public void TestDepartureReason_DefaultValues()
		{
			var defaultValue = ItemSet.DepartureReason.DefaultValue;

			AssertEquals("DefaultValue.Count", 7, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"RSN\")", "Resignation", defaultValue.GetDescriptionFromCode("RSN"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"RTA\")", "Retirement - Age", defaultValue.GetDescriptionFromCode("RTA"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"RTH\")", "Retirement - Health", defaultValue.GetDescriptionFromCode("RTH"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"DML\")", "Dismissal", defaultValue.GetDescriptionFromCode("DML"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"RDY\")", "Redundancy", defaultValue.GetDescriptionFromCode("RDY"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"DIS\")", "Death in Service", defaultValue.GetDescriptionFromCode("DIS"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"OTH\")", "Other - See Comments", defaultValue.GetDescriptionFromCode("OTH"));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("MIA", "Missing in action");

			ItemSet.DepartureReason.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, ItemSet.DepartureReason.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"MIA\")", "Missing in action", ItemSet.DepartureReason.Value.GetDescriptionFromCode("MIA"));

			AssertEquals("DepartureReason", ItemSet.DepartureReason.Name);
			AssertEquals("System/Staff/HRMS", ItemSet.DepartureReason.Category);
		}

		public void TestLanguageSkillLevel_DefaultValues()
		{
			var defaultValue = ItemSet.LanguageSkillLevel.DefaultValue;
			AssertEquals("DefaultValue.Count", 4, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"NAT\")", "Natural Language", defaultValue.GetDescriptionFromCode("NAT"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"POR\")", "Poor", defaultValue.GetDescriptionFromCode("POR"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"AVG\")", "Average", defaultValue.GetDescriptionFromCode("AVG"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"EXC\")", "Excellent", defaultValue.GetDescriptionFromCode("EXC"));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("TE1", "Test1");

			ItemSet.LanguageSkillLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, ItemSet.LanguageSkillLevel.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"TE1\")", "Test1", ItemSet.LanguageSkillLevel.Value.GetDescriptionFromCode("TE1"));

			AssertEquals("LanguageSkillLevel", ItemSet.LanguageSkillLevel.Name);
			AssertEquals("System/Staff/HRMS", ItemSet.LanguageSkillLevel.Category);
		}

		public void TestUserEventTrackingEnterprise()
		{
			AssertEquals(false, ItemSet.UserEventTrackingEnterprise.DefaultValue);
			AssertEquals("UserEventTrackingEnterprise", ItemSet.UserEventTrackingEnterprise.Name);
			AssertEquals("System/Staff/Activity Logging", ItemSet.UserEventTrackingEnterprise.Category);
			AssertEquals("Internal application Activity Tracking", ItemSet.UserEventTrackingEnterprise.Caption);
			AssertEquals(@"If turned on all user activity, including Window Titles, activity duration and activity statistics, within the application is tracked.

Note: Ensure you have obtained consent of individuals to track their application usage prior to setting this up in order to comply with GDPR Regulations that applies to all EU residents.", ItemSet.UserEventTrackingEnterprise.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UserEventTrackingEnterprise.Storage);
		}

		public void TestUserEventTrackingExternal()
		{
			AssertEquals(false, ItemSet.UserEventTrackingExternal.DefaultValue);
			AssertEquals("UserEventTrackingExternal", ItemSet.UserEventTrackingExternal.Name);
			AssertEquals("System/Staff/Activity Logging", ItemSet.UserEventTrackingExternal.Category);
			AssertEquals("Activity Tracking - External", ItemSet.UserEventTrackingExternal.Caption);
			AssertEquals(@"If turned on, all user activity is tracked - even that that occurs outside the application. Window Titles and activity duration is logged against the staff member.", ItemSet.UserEventTrackingExternal.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.UserEventTrackingExternal.Storage);
		}

		public void TestUserEventTrackingExternal_ForHostedSystem()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				Assert("Registry item should not be visible for hosted production system", !itemSet.UserEventTrackingExternal.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			}

			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestUserEventTrackingExternal_ForSelfHostedSystem()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest(string.Empty);
				Assert("Registry item should be visible for self-hosted production system", itemSet.UserEventTrackingExternal.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			}

			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestUserEventTrackingExternal_ForCWSupport()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest(string.Empty);
				Assert("Registry item should be visible for self-hosted production system with CWSupport user", itemSet.UserEventTrackingExternal.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				Assert("Registry item should be visible for hosted production system with CWSupport user", itemSet.UserEventTrackingExternal.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			}

			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestLegacyEncryptedSystemRegistrationKey()
		{
			string defaultValueInRegistry = ItemSet.LegacyEncryptedSystemRegistrationKey.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("", defaultValueInRegistry);
		}

		public void TestDisableEdocsFileTypeValidation()
		{
			AssertEquals("DefaultValue", false, ItemSet.DisableEdocsFileTypeValidation.DefaultValue);

			ItemSet.DisableEdocsFileTypeValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisableEdocsFileTypeValidation.Value);
		}

		public void TestPayablesCreditAgreedPaymentMethodDefaultValue()
		{
			AssertPayablesCreditAgreedPaymentMethodDefaultValues(true);
			AssertPayablesCreditAgreedPaymentMethodDefaultValues(false);
		}

		void AssertPayablesCreditAgreedPaymentMethodDefaultValues(bool isOFXEPaymentEnabled)
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, isOFXEPaymentEnabled))
			{
				var systemLevelDefaultValue = ItemSet.PayablesCreditAgreedPaymentMethodsList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("DefaultValue.Count", 5, systemLevelDefaultValue.Count);
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CHK\")", "Business Check", systemLevelDefaultValue.GetDescriptionFromCode("CHK"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CCD\")", "Credit Card", systemLevelDefaultValue.GetDescriptionFromCode("CCD"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"TRF\")", "Bank Transfer", systemLevelDefaultValue.GetDescriptionFromCode("TRF"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CBC\")", "Cash and/or Bank Check", systemLevelDefaultValue.GetDescriptionFromCode("CBC"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"DBC\")", "Debit Card", systemLevelDefaultValue.GetDescriptionFromCode("DBC"));

				var companyLevelDefaultValue = ItemSet.PayablesCreditAgreedPaymentMethodsList.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				var expectedPaymentMethodCount = isOFXEPaymentEnabled ? 6 : 5;
				AssertEquals("DefaultValue.Count", expectedPaymentMethodCount, companyLevelDefaultValue.Count);
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CHK\")", "Business Check", companyLevelDefaultValue.GetDescriptionFromCode("CHK"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CCD\")", "Credit Card", companyLevelDefaultValue.GetDescriptionFromCode("CCD"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"TRF\")", "Bank Transfer", companyLevelDefaultValue.GetDescriptionFromCode("TRF"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"CBC\")", "Cash and/or Bank Check", companyLevelDefaultValue.GetDescriptionFromCode("CBC"));
				AssertEquals("DefaultValue.GetDescriptionFromCode(\"DBC\")", "Debit Card", companyLevelDefaultValue.GetDescriptionFromCode("DBC"));
				if (isOFXEPaymentEnabled)
				{
					AssertEquals("DefaultValue.GetDescriptionFromCode(\"EPA\")", "E-Payment", companyLevelDefaultValue.GetDescriptionFromCode("EPA"));
				}
				else
				{
					AssertNull(companyLevelDefaultValue.GetDescriptionFromCode("EPA"));
				}
			}
		}

		public void TestFindDeletedItemsForCodeDescriptionPairList()
		{
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			list1.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, OrgDescriptions.CreditAgreedPaymentMethods.BusinessCheck);
			list1.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, OrgDescriptions.CreditAgreedPaymentMethods.CreditCard);
			list1.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, OrgDescriptions.CreditAgreedPaymentMethods.BankTransfer);
			list1.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, OrgDescriptions.CreditAgreedPaymentMethods.CashAndBankCheck);
			list1.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, OrgDescriptions.CreditAgreedPaymentMethods.DebitCard);

			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			list2.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, OrgDescriptions.CreditAgreedPaymentMethods.BusinessCheck);
			list2.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, OrgDescriptions.CreditAgreedPaymentMethods.BankTransfer);

			AssertEquals("Deleted: [ CCD - Credit Card] [ CBC - Cash and/or Bank Check] [ DBC - Debit Card] ", RawDataRegistry.FindDeletedItemsForCodeDescriptionPairList(list1, list2));
		}

		public void TestDeliveryRoutesList()
		{
			AssertNotNull(ItemSet.DeliveryRoutesList);
		}

		public void TestTrainingModeEnabled()
		{
			AssertEquals("DefaultValue", false, ItemSet.TrainingModeEnabled.DefaultValue);
			ItemSet.TrainingModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.TrainingModeEnabled.Value);
		}

		public void TestDisplayGMTOffsetOnF5UserInfo()
		{
			AssertEquals(true, ItemSet.DisplayGMTOffsetOnF5UserInfo.DefaultValue);
			ItemSet.DisplayGMTOffsetOnF5UserInfo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.DisplayGMTOffsetOnF5UserInfo.Value);
			ItemSet.DisplayGMTOffsetOnF5UserInfo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.DisplayGMTOffsetOnF5UserInfo.Value);
		}

		public void TestRememberOpenedFormsAfterUpgrade()
		{
			AssertEquals("default is false", false, ItemSet.RememberOpenedFormsAfterUpgrade.DefaultValue);
			ItemSet.RememberOpenedFormsAfterUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(ItemSet.RememberOpenedFormsAfterUpgrade.Value);
			ItemSet.RememberOpenedFormsAfterUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!ItemSet.RememberOpenedFormsAfterUpgrade.Value);
		}

		public void TestShowSaveProgressBox()
		{
			AssertEquals("DefaultValue", true, ItemSet.ShowSaveProgressBox.DefaultValue);
			ItemSet.ShowSaveProgressBox.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.ShowSaveProgressBox.Value);
			ItemSet.ShowSaveProgressBox.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ShowSaveProgressBox.Value);
		}

		public void TestShowCodeAtCompanyAndBranchName()
		{
			AssertEquals("Category", "System/UI", ItemSet.ShowCodeAtCompanyAndBranchName.Category);
			AssertEquals("Name", "ShowCodeAtCompanyAndBranchName", ItemSet.ShowCodeAtCompanyAndBranchName.Name);
			AssertEquals("Hint", "When enabled, Company and Branch codes will display on the Registry fallback levels.", ItemSet.ShowCodeAtCompanyAndBranchName.Hint);
			AssertEquals("DefaultValue", false, ItemSet.ShowCodeAtCompanyAndBranchName.DefaultValue);

			ItemSet.ShowCodeAtCompanyAndBranchName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ShowCodeAtCompanyAndBranchName.Value);
			ItemSet.ShowCodeAtCompanyAndBranchName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.ShowCodeAtCompanyAndBranchName.Value);
		}

		public void TestApplyOptionRecompile()
		{
			AssertEquals("DefaultValue", true, ItemSet.ApplyOptionRecompile.DefaultValue);
			ItemSet.ApplyOptionRecompile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.ApplyOptionRecompile.Value);
			ItemSet.ApplyOptionRecompile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.ApplyOptionRecompile.Value);
		}

		public void TestSMTPEhloDomain()
		{
			ItemSet.SMTPEhloDomain.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test.com");
			AssertEquals("Value", "test.com", ItemSet.SMTPEhloDomain.Value);
		}

		public void TestSMTPEhloDomainDefaultValueWhenHostedWithWiseGlobal()
		{
			var originalHostedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("DefaultValue", RawDataRegistry.EntityFrameworkRegistryDefaults.SMTPEhloDomainForWiseGlobal, ItemSet.SMTPEhloDomain.DefaultValue);
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestSMTPEhloDomainDefaultValueWhenNotHostedWithWiseGlobal()
		{
			var originalHostedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals("DefaultValue", "", ItemSet.SMTPEhloDomain.DefaultValue);
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestLightValidationEnabled()
		{
			AssertEquals("DefaultValue", true, ItemSet.LightValidationEnabled.DefaultValue);
			ItemSet.LightValidationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.LightValidationEnabled.Value);
			ItemSet.LightValidationEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.LightValidationEnabled.Value);
		}

		public void TestShipmentRequestForMissingDocuments()
		{
			ItemSet.ShipmentRequestForMissingDocumentsOpeningText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Opening Text");
			AssertEquals("ShipmentRequestForMissingDocumentsOpeningText.Value", "Opening Text", ItemSet.ShipmentRequestForMissingDocumentsOpeningText.Value);

			ItemSet.ShipmentRequestForMissingDocumentsClosingText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Closing Text");
			AssertEquals("ShipmentRequestForMissingDocumentsClosingText.Value", "Closing Text", ItemSet.ShipmentRequestForMissingDocumentsClosingText.Value);

			AssertEquals("Clause Default Value", "We have not yet received documents for the shipment referenced herein. Please send the documents requested below, by E-mail attachment or Fax. If you have any problem that might delay the matter further, please contact the writer urgently. Otherwise, we look forward to receiving these documents as soon as possible. Without them, the completion of Customs formalities may not proceed and delivery will be delayed.", ItemSet.ShipmentRequestForMissingDocumentsClause.DefaultValue);
			ItemSet.ShipmentRequestForMissingDocumentsClause.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Clause");
			AssertEquals("ShipmentRequestForMissingDocumentsClause.Value", "Clause", ItemSet.ShipmentRequestForMissingDocumentsClause.Value);
		}

		public void TestCustomsRequestForMissingDocuments()
		{
			ItemSet.CustomsRequestForMissingDocumentsOpeningText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Opening Text");
			AssertEquals("CustomsRequestForMissingDocumentsOpeningText.Value", "Opening Text", ItemSet.CustomsRequestForMissingDocumentsOpeningText.Value);

			ItemSet.CustomsRequestForMissingDocumentsClosingText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Closing Text");
			AssertEquals("CustomsRequestForMissingDocumentsClosingText.Value", "Closing Text", ItemSet.CustomsRequestForMissingDocumentsClosingText.Value);

			AssertEquals("Clause Default Value", "We have not yet received documents for the shipment referenced herein. Please send the documents requested below, by E-mail attachment or Fax. If you have any problem that might delay the matter further, please contact the writer urgently. Otherwise, we look forward to receiving these documents as soon as possible. Without them, the completion of Customs formalities may not proceed and delivery will be delayed.", ItemSet.CustomsRequestForMissingDocumentsClause.DefaultValue);
			ItemSet.CustomsRequestForMissingDocumentsClause.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Clause");
			AssertEquals("CustomsRequestForMissingDocumentsClause.Value", "Clause", ItemSet.CustomsRequestForMissingDocumentsClause.Value);
		}

		public void TestDateTimeStaffFormWasLastShown()
		{
			AssertEquals("Registry Item should be hidden.", RegistryOptions.IsHidden, ItemSet.DateTimeStaffFormWasLastShown.Options);

			DateTime dateTime = new DateTime(2005, 1, 1);
			ItemSet.DateTimeStaffFormWasLastShown.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, dateTime);
			AssertEquals("GetValue()", dateTime, ItemSet.DateTimeStaffFormWasLastShown.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestCartageAdviceTimeSlotRequestOpeningText()
		{
			AssertEquals("CartageAdviceTimeSlotRequestExportOpeningText.DefaultValue", "Please book time slot(s) for the container(s) listed below.", ItemSet.CartageAdviceTimeSlotRequestExportOpeningText.DefaultValue);
			AssertEquals("CartageAdviceTimeSlotRequestImportOpeningText.DefaultValue", "Please book time slot(s) for the container(s) listed below.", ItemSet.CartageAdviceTimeSlotRequestImportOpeningText.DefaultValue);

			ItemSet.CartageAdviceTimeSlotRequestExportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Export Opening Text");
			ItemSet.CartageAdviceTimeSlotRequestImportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Import Opening Text");

			AssertEquals("CartageAdviceTimeSlotRequestExportOpeningText.Value", "Export Opening Text", ItemSet.CartageAdviceTimeSlotRequestExportOpeningText.Value);
			AssertEquals("CartageAdviceTimeSlotRequestImportOpeningText.Value", "Import Opening Text", ItemSet.CartageAdviceTimeSlotRequestImportOpeningText.Value);
		}

		public void TestCartageAdviceTimeSlotConfirmationOpeningText()
		{
			AssertEquals("CartageAdviceTimeSlotConfirmationOpeningText.DefaultValue", "", ItemSet.CartageAdviceTimeSlotConfirmationOpeningText.DefaultValue);
			ItemSet.CartageAdviceTimeSlotConfirmationOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Opening Text");
			AssertEquals("CartageAdviceTimeSlotConfirmationOpeningText.Value", "Opening Text", ItemSet.CartageAdviceTimeSlotConfirmationOpeningText.Value);
		}

		public void TestCartageAdviceTimeSlotConfirmationClosingText()
		{
			AssertEquals("CartageAdviceTimeSlotConfirmationClosingText.DefaultValue", "", ItemSet.CartageAdviceTimeSlotConfirmationClosingText.DefaultValue);
			ItemSet.CartageAdviceTimeSlotConfirmationClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Closing Text");
			AssertEquals("CartageAdviceTimeSlotConfirmationClosingText.Value", "Closing Text", ItemSet.CartageAdviceTimeSlotConfirmationClosingText.Value);
		}

		public void TestConsignmentRequestForServiceOpeningText()
		{
			AssertEquals("ConsignmentRequestForServiceOpeningText.DefaultValue", "", ItemSet.ConsignmentRequestForServiceOpeningText.DefaultValue);
			ItemSet.ConsignmentRequestForServiceOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Opening Text");
			AssertEquals("ConsignmentRequestForServiceOpeningText.Value", "Opening Text", ItemSet.ConsignmentRequestForServiceOpeningText.Value);
		}

		public void TestConsignmentRequestForServiceClosingText()
		{
			AssertEquals("ConsignmentRequestForServiceClosingText.DefaultValue", "", ItemSet.ConsignmentRequestForServiceClosingText.DefaultValue);
			ItemSet.ConsignmentRequestForServiceClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Closing Text");
			AssertEquals("ConsignmentRequestForServiceClosingText.Value", "Closing Text", ItemSet.ConsignmentRequestForServiceClosingText.Value);
		}

		public void TestConsignmentAuthorizationForServiceOpeningText()
		{
			AssertEquals("ConsignmentAuthorizationForServiceOpeningText.DefaultValue", "", ItemSet.ConsignmentAuthorizationForServiceOpeningText.DefaultValue);
			ItemSet.ConsignmentAuthorizationForServiceOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Opening Text");
			AssertEquals("ConsignmentAuthorizationForServiceOpeningText.Value", "Opening Text", ItemSet.ConsignmentAuthorizationForServiceOpeningText.Value);
		}

		public void TestConsignmentAuthorizationForServiceClosingText()
		{
			AssertEquals("ConsignmentAuthorizationForServiceClosingText.DefaultValue", "", ItemSet.ConsignmentAuthorizationForServiceClosingText.DefaultValue);
			ItemSet.ConsignmentAuthorizationForServiceClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Closing Text");
			AssertEquals("ConsignmentAuthorizationForServiceClosingText.Value", "Closing Text", ItemSet.ConsignmentAuthorizationForServiceClosingText.Value);
		}

		public void TestExportCertificationOpeningText()
		{
			AssertEquals("ExportCertificationOpeningText.DefaultValue", "", ItemSet.ExportCertificationOpeningText.DefaultValue);
			ItemSet.ExportCertificationOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Opening Text");
			AssertEquals("ExportCertificationOpeningText.Value", "Opening Text", ItemSet.ExportCertificationOpeningText.Value);
		}

		public void TestExportCertificationClosingText()
		{
			AssertEquals("ExportCertificationClosingText.DefaultValue", "", ItemSet.ExportCertificationClosingText.DefaultValue);
			ItemSet.ExportCertificationClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Closing Text");
			AssertEquals("ExportCertificationClosingText.Value", "Closing Text", ItemSet.ExportCertificationClosingText.Value);
		}

		public void TestCartageAdviceOpeningAndClosingText()
		{
			ItemSet.CartageAdviceExportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Export Opening Text");
			AssertEquals("CartageAdviceExportOpeningText.Value", "Export Opening Text", ItemSet.CartageAdviceExportOpeningText.Value);

			ItemSet.CartageAdviceExportClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Export Closing Text");
			AssertEquals("CartageAdviceExportClosingText.Value", "Export Closing Text", ItemSet.CartageAdviceExportClosingText.Value);

			ItemSet.CartageAdviceImportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Import Opening Text");
			AssertEquals("CartageAdviceImportOpeningText.Value", "Import Opening Text", ItemSet.CartageAdviceImportOpeningText.Value);

			ItemSet.CartageAdviceImportClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Import Closing Text");
			AssertEquals("CartageAdviceImportClosingText.Value", "Import Closing Text", ItemSet.CartageAdviceImportClosingText.Value);
		}

		public void TestAUCCompanyCertificateData()
		{
			FileUpLoaderX509CertificateRegistryEditorInfo editorInfo = (FileUpLoaderX509CertificateRegistryEditorInfo)ItemSet.AUCCompanyCertificateData.EditorInfo;
			IRegistryItem privateKeyRegistryItem = (IRegistryItem)typeof(FileUpLoaderX509CertificateRegistryEditorInfo).GetField("PrivateKeyRegistryItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorInfo);
			AssertEquals("EditorInfo.PrivateKeyRegistryItem", ItemSet.AUCCompanyCertificatePassword, privateKeyRegistryItem);
		}

		public void TestEnableConsolidatedEntries()
		{
			var item = ItemSet.EnableConsolidatedEntries;
			AssertEquals("Name", "EnableConsolidatedEntries", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Customs_ConsolidatedEntries, item.Category);
			AssertContains("Caption", "Enable Consolidated Entries", item.Caption);
			AssertContains("Hint", "Set to Yes to enable Consolidated Entries for the system.", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Registry Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default Value", false, item.DefaultValue);
		}

		public void TestFTPConnectionTimeout()
		{
			AssertEquals("Default value is:", 6, ItemSet.FTPConnectionTimeout.Value);
			AssertEquals("System/FTP Service", ItemSet.FTPConnectionTimeout.Category);
			ItemSet.FTPConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals("Changed value should be:", 0, ItemSet.FTPConnectionTimeout.Value);
		}

		public void TestFTPReadWriteTimeout()
		{
			AssertEquals("Default value is:", 5, ItemSet.FTPReadTimeout.Value);
			AssertEquals("System/FTP Service", ItemSet.FTPReadTimeout.Category);
			ItemSet.FTPReadTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals("Changed value should be:", 0, ItemSet.FTPReadTimeout.Value);
		}

		public void TestHAWBDimensionsDefault()
		{
			AssertEquals(Constants.AWB.Dimensions.DEF, ItemSet.HAWBDimensionsDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.HAWBDimensionsDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.Dimensions.PKS);
			AssertEquals("Freight/AWB/HAWB", ItemSet.HAWBDimensionsDefault.Category);
			AssertEquals(Constants.AWB.Dimensions.PKS, ItemSet.HAWBDimensionsDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestMAWBDimensionsDefault()
		{
			AssertEquals(Constants.AWB.Dimensions.DEF, ItemSet.MAWBDimensionsDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.MAWBDimensionsDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.Dimensions.PKS);
			AssertEquals("Freight/AWB/MAWB", ItemSet.MAWBDimensionsDefault.Category);
			AssertEquals(Constants.AWB.Dimensions.PKS, ItemSet.MAWBDimensionsDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShipperAddressDefaultsTo()
		{
			AssertEquals(OrgConstants.AddressType.Documentary, ItemSet.ShipperAddressDefaultsTo.Value);
			ItemSet.ShipperAddressDefaultsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgConstants.AddressType.Office);
			AssertEquals("Freight/AWB", ItemSet.ShipperAddressDefaultsTo.Category);
			AssertEquals(OrgConstants.AddressType.Office, ItemSet.ShipperAddressDefaultsTo.Value);
		}

		public void TestConsigneeAddressDefaultsTo()
		{
			AssertEquals(OrgConstants.AddressType.Documentary, ItemSet.ConsigneeAddressDefaultsTo.Value);
			ItemSet.ConsigneeAddressDefaultsTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgConstants.AddressType.Office);
			AssertEquals("Freight/AWB", ItemSet.ConsigneeAddressDefaultsTo.Category);
			AssertEquals(OrgConstants.AddressType.Office, ItemSet.ConsigneeAddressDefaultsTo.Value);
		}

		public void TestAllowAutoCalculationOfTax()
		{
			AssertEquals("Default", true, ItemSet.AllowAutoCalculationOfTax.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Allow Auto Calculation of Tax for domestic AWBs", ItemSet.AllowAutoCalculationOfTax.Caption);
			ItemSet.AllowAutoCalculationOfTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Freight/AWB", ItemSet.AllowAutoCalculationOfTax.Category);
			AssertEquals(false, ItemSet.AllowAutoCalculationOfTax.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowManualShipmentNumberEntry()
		{
			AssertEquals("Default", false, ItemSet.AllowManualShipmentNumberEntry.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AllowManualShipmentNumberEntry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Enabled", true, ItemSet.AllowManualShipmentNumberEntry.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AllowManualShipmentNumberEntry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Disabled", false, ItemSet.AllowManualShipmentNumberEntry.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestHAWBPaperType()
		{
			AssertEquals("Use paper type default", Constants.AWB.PaperTypes.Iata, ItemSet.HAWBPaperType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.HAWBPaperType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.PaperTypes.Letter);
			AssertEquals("Freight/AWB/HAWB/Dot Matrix", ItemSet.HAWBPaperType.Category);
			AssertEquals("Paper type", Constants.AWB.PaperTypes.Letter, ItemSet.HAWBPaperType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestMAWBPaperType()
		{
			AssertEquals("Use paper type default", Constants.AWB.PaperTypes.Iata, ItemSet.MAWBPaperType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.MAWBPaperType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.PaperTypes.Traxon);
			AssertEquals("Freight/AWB/MAWB/Dot Matrix", ItemSet.MAWBPaperType.Category);
			AssertEquals("Paper type", Constants.AWB.PaperTypes.Traxon, ItemSet.MAWBPaperType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestHAWBLogo()
		{
			AssertNull("Default Logo", ItemSet.HAWBLogo.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Freight/AWB/HAWB/Laser", ItemSet.HAWBLogo.Category);
			AssertNotNull("Some Image is returned", ItemSet.HAWBLogo);
		}

		public void TestPrintAsAgreedOnFirstSetHAWB()
		{
			AssertEquals("Default", Constants.AWB.AsAgreedTypes.Codes.None, ItemSet.PrintAsAgreedOnFirstSetHAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.PrintAsAgreedOnFirstSetHAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("Freight/AWB/HAWB/As Agreed", ItemSet.PrintAsAgreedOnFirstSetHAWB.Category);
			AssertEquals(Constants.AWB.AsAgreedTypes.Codes.All, ItemSet.PrintAsAgreedOnFirstSetHAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPrintAsAgreedOnSecondSetHAWB()
		{
			AssertEquals("Default", Constants.AWB.AsAgreedTypes.Codes.None, ItemSet.PrintAsAgreedOnSecondSetHAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.PrintAsAgreedOnSecondSetHAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("Freight/AWB/HAWB/As Agreed", ItemSet.PrintAsAgreedOnSecondSetHAWB.Category);
			AssertEquals(Constants.AWB.AsAgreedTypes.Codes.All, ItemSet.PrintAsAgreedOnSecondSetHAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPrintAsAgreedOnFirstSetMAWB()
		{
			AssertEquals("Default", Constants.AWB.AsAgreedTypes.Codes.None, ItemSet.PrintAsAgreedOnFirstSetMAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.PrintAsAgreedOnFirstSetMAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("Freight/AWB/MAWB/As Agreed", ItemSet.PrintAsAgreedOnFirstSetMAWB.Category);
			AssertEquals(Constants.AWB.AsAgreedTypes.Codes.All, ItemSet.PrintAsAgreedOnFirstSetMAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPrintAsAgreedOnSecondSetMAWB()
		{
			AssertEquals("Default", Constants.AWB.AsAgreedTypes.Codes.None, ItemSet.PrintAsAgreedOnSecondSetMAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.PrintAsAgreedOnSecondSetMAWB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AWB.AsAgreedTypes.Codes.All);
			AssertEquals("Freight/AWB/MAWB/As Agreed", ItemSet.PrintAsAgreedOnFirstSetMAWB.Category);
			AssertEquals(Constants.AWB.AsAgreedTypes.Codes.All, ItemSet.PrintAsAgreedOnSecondSetMAWB.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAirWaybillHAWBWeightAndVolumeDisplay()
		{
			AssertEquals("Default", WeightAndVolumeDisplayTypes.Codes.Actual, ItemSet.AirWaybillHAWBWeightAndVolumeDisplay.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AirWaybillHAWBWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Freight/AWB/HAWB", ItemSet.AirWaybillHAWBWeightAndVolumeDisplay.Category);
			AssertEquals(WeightAndVolumeDisplayTypes.Codes.Client, ItemSet.AirWaybillHAWBWeightAndVolumeDisplay.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestHAWBDefaultShipperText()
		{
			AssertEquals("Default", "", ItemSet.HAWBDefaultShipperText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.HAWBDefaultShipperText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/HAWB", ItemSet.HAWBDefaultShipperText.Category);
			AssertEquals("Test", ItemSet.HAWBDefaultShipperText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestHAWBDefaultCarrierText()
		{
			AssertEquals("Default", "", ItemSet.HAWBDefaultCarrierText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.HAWBDefaultCarrierText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/HAWB", ItemSet.HAWBDefaultCarrierText.Category);
			AssertEquals("Test", ItemSet.HAWBDefaultCarrierText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestIssuingCarrierAgentAccountNumber()
		{
			AssertEquals("Default", "", ItemSet.IssuingCarrierAgentAccountNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.IssuingCarrierAgentAccountNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/MAWB/Issuing Carrier Agent", ItemSet.IssuingCarrierAgentAccountNumber.Category);
			AssertEquals("Test", ItemSet.IssuingCarrierAgentAccountNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAWBSecurityDeclaration()
		{
			AssertEquals("Default", string.Empty, ItemSet.AWBSecurityDeclarationOpeningText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AWBSecurityDeclarationOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Documents/Forwarding/Shipment/AWB Security Declaration", ItemSet.AWBSecurityDeclarationOpeningText.Category);
			AssertEquals("Test", ItemSet.AWBSecurityDeclarationOpeningText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestIssuingCarrierAgentCity()
		{
			AssertEquals("Default", "", ItemSet.IssuingCarrierAgentCity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.IssuingCarrierAgentCity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/MAWB/Issuing Carrier Agent", ItemSet.IssuingCarrierAgentCity.Category);
			AssertEquals("Test", ItemSet.IssuingCarrierAgentCity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestIssuingCarrierIATACode()
		{
			AssertEquals("Default", "", ItemSet.IssuingCarrierAgentIATACode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.IssuingCarrierAgentIATACode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/MAWB/Issuing Carrier Agent", ItemSet.IssuingCarrierAgentIATACode.Category);
			AssertEquals("Test", ItemSet.IssuingCarrierAgentIATACode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestIssuingCarrierAgentName()
		{
			AssertEquals("Default", "", ItemSet.IssuingCarrierAgentName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.IssuingCarrierAgentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/MAWB/Issuing Carrier Agent", ItemSet.IssuingCarrierAgentName.Category);
			AssertEquals("Test", ItemSet.IssuingCarrierAgentName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowAsAgreed()
		{
			AssertEquals("Default", true, ItemSet.AllowAsAgreed.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AllowAsAgreed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Freight/AWB/MAWB/As Agreed", ItemSet.AllowAsAgreed.Category);
			AssertEquals(false, ItemSet.AllowAsAgreed.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestSelectFHLByDefault()
		{
			AssertEquals("Default", false, ItemSet.SelectFHLByDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.SelectFHLByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Freight/AWB/MAWB", ItemSet.SelectFHLByDefault.Category);
			AssertEquals(true, ItemSet.SelectFHLByDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("SelectFHLByDefault Hint",
				"Set this Registry to 'Yes' for the 'Send FHL' option to be checked by default when the 'Send FWB' option is checked, regardless of origin/destination for Air Consols that are not direct.\r\n\r\nIf the Registry is not overridden, the 'Send FHL' option will be checked by default when the 'Send FWB' option is checked, for Air Consols that are not direct, originating/destined to/transhipping countries/regions in the prescribed list.\r\n\r\nRefer to the warning message visible on the Send FWB message for countries/regions in the prescribed list.",
				ItemSet.SelectFHLByDefault.Hint);
		}

		public void TestAirWaybillMAWBWeightAndVolumeDisplay()
		{
			AssertEquals("Default", WeightAndVolumeDisplayTypes.Codes.Actual, ItemSet.AirWaybillMAWBWeightAndVolumeDisplay.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.AirWaybillMAWBWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Freight/AWB/MAWB", ItemSet.AirWaybillMAWBWeightAndVolumeDisplay.Category);
			AssertEquals(WeightAndVolumeDisplayTypes.Codes.Client, ItemSet.AirWaybillMAWBWeightAndVolumeDisplay.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestMAWBDefaultShipperText()
		{
			AssertEquals("Default", "", ItemSet.MAWBDefaultShipperText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.MAWBDefaultShipperText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/MAWB", ItemSet.MAWBDefaultShipperText.Category);
			AssertEquals("Test", ItemSet.MAWBDefaultShipperText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestMAWBDefaultCarrierText()
		{
			AssertEquals("Default", "", ItemSet.MAWBDefaultCarrierText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.MAWBDefaultCarrierText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Freight/AWB/MAWB", ItemSet.MAWBDefaultCarrierText.Category);
			AssertEquals("Test", ItemSet.MAWBDefaultCarrierText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestHAWBDocumentTitlesData()
		{
			AssertEquals("Category", "Freight/AWB/HAWB/Laser", ItemSet.HAWBDocumentTitles.Category);
			AssertEquals("TypeOf Editory", typeof(HAWBDocumentPivotRegistryEditorInfo), ItemSet.HAWBDocumentTitles.EditorInfo.GetType());
			AssertEquals("Type of Registry Item", typeof(BinaryRegistryItem), ItemSet.HAWBDocumentTitles.GetType());
		}

		public void TestMAWBDocumentTitlesData()
		{
			AssertEquals("Category", "Freight/AWB/MAWB", ItemSet.MAWBDocumentTitles.Category);
			AssertEquals("TypeOf Editory", typeof(MAWBDocumentPivotRegistryEditorInfo), ItemSet.MAWBDocumentTitles.EditorInfo.GetType());
			AssertEquals("Type of Registry Item", typeof(BinaryRegistryItem), ItemSet.MAWBDocumentTitles.GetType());
		}

		public void TestUseJSEngineForDocumentMacroEvaluation()
		{
			AssertEquals("Default value", false, ItemSet.UseJSEngineForDocumentMacroEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.UseJSEngineForDocumentMacroEvaluation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Updated value", true, ItemSet.UseJSEngineForDocumentMacroEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestUseUseExpressionCachingForFormBuilderMacroEngine()
		{
			AssertEquals("Default value", true, ItemSet.UseExpressionCachingForFormBuilderMacroEngine.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.UseExpressionCachingForFormBuilderMacroEngine.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Updated value", false, ItemSet.UseExpressionCachingForFormBuilderMacroEngine.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestUseJSEngineForTriggerConditionsEvaluation()
		{
			AssertEquals("Default value", true, ItemSet.UseJSEngineForTriggerConditionsEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.UseJSEngineForTriggerConditionsEvaluation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Updated value", false, ItemSet.UseJSEngineForTriggerConditionsEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestUseJSEngineForAutoRatingConditionsEvaluation()
		{
			AssertEquals("Default value", true, ItemSet.UseJSEngineForAutoRatingConditionsEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.UseJSEngineForAutoRatingConditionsEvaluation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Updated value", false, ItemSet.UseJSEngineForAutoRatingConditionsEvaluation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestExcelPrinterInternationalFormats()
		{
			AssertEquals("Default value", "", ItemSet.ExcelPrinterInternationalFormats.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.ExcelPrinterInternationalFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "some \u5656 value");
			AssertEquals("Set value", "some \u5656 value", ItemSet.ExcelPrinterInternationalFormats.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCommodityCode()
		{
			Guid testGuid1 = Guid.NewGuid();
			ItemSet.CommodityCode.SetValue(CompanyPK, Guid.Empty, DepartmentPK, testGuid1);
			AssertEquals("CommodityCode", testGuid1, ItemSet.CommodityCode.GetValueWithoutFallback(CompanyPK, Guid.Empty, DepartmentPK));
			AssertEquals("Options", RegistryOptions.IsValueOptional, ItemSet.CommodityCode.Options);
		}

		public void TestFreightChargeCode()
		{
			AssertEquals("DefaultChargeCode", "FRT", ItemSet.FreightChargeCode.DefaultChargeCode);
			Guid testChargeCode1 = Guid.NewGuid();
			ItemSet.FreightChargeCode.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, testChargeCode1);
			AssertEquals("FreightChargeCode", testChargeCode1, ItemSet.FreightChargeCode.Value);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.FreightChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals("FreightChargeCodeStorage", RegistryStorageFlags.Company, ItemSet.FreightChargeCode.Storage);
		}

		public void TestConsolPaymentTerm()
		{
			AssertEquals("ConsolPaymentTerm default.", "", ItemSet.ConsolPaymentTerm.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			ItemSet.ConsolPaymentTerm.SetValue(CompanyPK, Guid.Empty, Guid.Empty, Constants.PaymentType.Collect);
			AssertEquals("ConsolPaymentTerm Set and Get", Constants.PaymentType.Collect, ItemSet.ConsolPaymentTerm.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestShipmentScreenLayout()
		{
			AssertEquals("ShipmentScreenLayout default", Constants.ShipmentScreenOptions.Consignor, ItemSet.ShipmentScreenLayout.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));

			ItemSet.ShipmentScreenLayout.SetValue(CompanyPK, Guid.Empty, Guid.Empty, Constants.ShipmentScreenOptions.Consignee);
			AssertEquals("Set ShipmentScreenLayout", Constants.ShipmentScreenOptions.Consignee, ItemSet.ShipmentScreenLayout.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestServiceLevel()
		{
			Guid testServicePK = Guid.NewGuid();

			ItemSet.ServiceLevel.SetValue(CompanyPK, Guid.Empty, Guid.Empty, testServicePK);
			AssertEquals("GetServiceLevel", testServicePK, ItemSet.ServiceLevel.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestShipmentOSMGSecurityLevel()
		{
			var item = ItemSet.ShipmentOSMGSecurityLevel;
			AssertEquals("Category", RawDataRegistry.Categories.Freight_Shipment, item.Category);
			AssertEquals("Caption", "Org. Security Groups - Security Level", item.Caption);
			AssertEquals("Hint", $@"Configure the level of security to apply to Organization Security Groups.
This is used for users that have the security right '{Constants.CRMSecurityCaptions.IgnoreOSMG}' denied.
When the default value '{Constants.OSMGSecurityLevels.Standard}' is selected, these users will need to be part of the security group of at least one primary organization involved in the shipment.
When the value is set to '{Constants.OSMGSecurityLevels.Enhanced}', these users will need to be part of the security group of all primary organizations on the shipment. Task and staff assignments will be ignored.", item.Hint);
			AssertEquals("LookUpEditType", OLookUpEditType.OSMGSecurityLevel, ((CodePairRegistryDataType)item.DataType).LookUpEditType);
			AssertEquals("DataType AllowBlank", false, ((CodePairRegistryDataType)item.DataType).AllowBlank);
			AssertEquals("DataType ValidateCode", true, ((CodePairRegistryDataType)item.DataType).ValidateCode);
			AssertEquals("RegistryStorageFlags", RegistryStorageFlags.System, item.Storage);

			var editorInfo = (ComboBoxRegistryEditorInfo)item.EditorInfo;
			AssertEquals("LookupList should have 2 elements", 2, editorInfo.LookUpList.Count);
			AssertEquals($"LookupList should contain '{Constants.OSMGSecurityLevels.Standard}'", true, editorInfo.LookUpList.ContainsCode(Constants.OSMGSecurityLevels.Standard));
			AssertEquals($"LookupList should contain '{Constants.OSMGSecurityLevels.Enhanced}'", true, editorInfo.LookUpList.ContainsCode(Constants.OSMGSecurityLevels.Enhanced));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.OSMGSecurityLevels.Enhanced);
			AssertEquals(Constants.OSMGSecurityLevels.Enhanced, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPhysicalServerID()
		{
			ItemSet.PhysicalServerID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			AssertEquals("PhysicalServerID", "123", ItemSet.PhysicalServerID.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("IsReadOnly", true, ItemSet.PhysicalServerID.IsReadOnly);
		}

		public void TestAllowHostedClientAccessToEmailSettings()
		{
			RawDataRegistry rawReg = new RawDataRegistry();
			EnvProxy.SetHostedLocationForTest("SYD");
			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should not be able to edit this, it's only for support", rawReg.AllowHostedClientAccessToEmailSettings.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !rawReg.AllowHostedClientAccessToEmailSettings.IsReadOnly);
			}
		}

		public void TestUseOAuth2ForIncoming()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(string.Empty, ItemSet.UseOAuth2ForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.UseOAuth2ForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365);
			AssertEquals(OAuth2TypeList.Codes.Ms365, ItemSet.UseOAuth2ForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.UseOAuth2ForIncoming.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.UseOAuth2ForIncoming.IsReadOnly);
			}
		}

		public void TestUseOAuth2ForOutgoing()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(string.Empty, ItemSet.UseOAuth2ForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.UseOAuth2ForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365);
			AssertEquals(OAuth2TypeList.Codes.Ms365, ItemSet.UseOAuth2ForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.UseOAuth2ForOutgoing.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.UseOAuth2ForOutgoing.IsReadOnly);
			}
		}

		public void TestMs365ApplicationIdForIncoming()
		{
			var item = ItemSet.Ms365ApplicationIdForIncoming;
			AssertEquals("Name", "Ms365ApplicationIdForIncoming", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365In, item.Category);
			AssertEquals("Caption", "Application ID", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
		}

		public void TestMs365ApplicationIdForOutgoing()
		{
			var item = ItemSet.Ms365ApplicationIdForOutgoing;
			AssertEquals("Name", "Ms365ApplicationIdForOutgoing", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365Out, item.Category);
			AssertEquals("Caption", "Application ID", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
		}

		public void TestUseGraphApiForIncoming()
		{
			var item = ItemSet.UseGraphApiForIncoming;
			AssertEquals("Name", "UseGraphApiForIncoming", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365In, item.Category);
			AssertEquals("Caption", "Use Graph API", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, item.Options);
		}

		public void TestUseGraphApiForOutgoing()
		{
			var item = ItemSet.UseGraphApiForOutgoing;
			AssertEquals("Name", "UseGraphApiForOutgoing", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365Out, item.Category);
			AssertEquals("Caption", "Use Graph API", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, item.Options);
		}

		public void TestOAuth2TokenForIncoming()
		{
			var item = ItemSet.Ms365OAuth2TokenForIncoming;
			AssertEquals("Name", "Ms365OAuth2TokenForIncoming", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365In, item.Category);
			AssertContains("Caption", "Incoming", item.Caption);
			AssertContains("Hint", "Inbound", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);

			AssertEquals("EmailType", EmailType.Incoming, item.EmailType);
			AssertEquals("Ms365OAuth2TenantId", ItemSet.Ms365OAuth2TenantId, item.Ms365OAuth2TenantId);
			AssertEquals("Ms365ApplicationIdForIncoming", ItemSet.Ms365ApplicationIdForIncoming, item.Ms365ApplicationIdForIncoming);
			AssertEquals("UseGraphApiForIncoming", ItemSet.UseGraphApiForIncoming, item.UseGraphApiForIncoming);

			var editorInfo = item.EditorInfo as Ms365OAuth2TokenRegistryEditorInfo;
			AssertNotNull(editorInfo);
		}

		public void TestMs365AppSecretForIncoming()
		{
			var item = ItemSet.Ms365AppSecretForIncoming;
			AssertEquals("Name", "Ms365AppSecretForIncoming", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365In, item.Category);
			AssertContains("Caption", "Client Secret", item.Caption);
			AssertContains("Hint", @"This is the Client Secret that has been registered in the Azure platform.  The Client Secret is the value in the Value column of Certificates & secrets settings in Azure.  For added security, the Client Secret will be encrypted when the Registry is saved.

Important: Please consider the expiry period of the Client Secret in the Certificates & secrets page of your App registration in Azure as an expired Client Secret can lead to authentication issues.  Please refer to the Registering App in Microsoft Azure Technical Guide at https://myaccount.cargowise.com/Home/CargoWise/TechnicalGuides.aspx for further details on registering an App in Azure.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
			AssertEquals("DefaultValue", string.Empty, item.DefaultValue);

			var token = new byte[] { 1, 2 };
			ItemSet.Ms365OAuth2AppTokenForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, token);
			AssertEquals("Pre-Condition", token, ItemSet.Ms365OAuth2AppTokenForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, "secret");
			AssertEquals("Test OnUpdateAction: AppToken will be deleted", true, ItemSet.Ms365OAuth2AppTokenForIncoming.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).IsNullOrEmpty());
		}

		public void TestOAuth2AppTokenForIncoming()
		{
			var item = ItemSet.Ms365OAuth2AppTokenForIncoming;
			AssertEquals("Name", "Ms365OAuth2AppTokenForIncoming", item.Name);
			AssertEquals("Category", "", item.Category);
			AssertContains("Caption", null, item.Caption);
			AssertContains("Hint", null, item.Hint);

			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, item.Options);
		}

		public void TestMsOAuth2TokenForOutgoing()
		{
			var item = ItemSet.Ms365OAuth2TokenForOutgoing;
			AssertEquals("Name", "Ms365OAuth2TokenForOutgoing", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365Out, item.Category);
			AssertContains("Caption", "Outgoing", item.Caption);
			AssertContains("Hint", "Outbound", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);

			AssertEquals("EmailType", EmailType.Outgoing, item.EmailType);
			AssertEquals("Ms365OAuth2TenantId", ItemSet.Ms365OAuth2TenantId, item.Ms365OAuth2TenantId);
			AssertEquals("Ms365ApplicationIdForIncoming", null, item.Ms365ApplicationIdForIncoming);
			AssertEquals("UseGraphApiForIncoming", null, item.UseGraphApiForIncoming);

			var editorInfo = item.EditorInfo as Ms365OAuth2TokenRegistryEditorInfo;
			AssertNotNull(editorInfo);
		}

		public void TestMs365AppSecretForOutgoing()
		{
			var item = ItemSet.Ms365AppSecretForOutgoing;
			AssertEquals("Name", "Ms365AppSecretForOutgoing", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365Out, item.Category);
			AssertContains("Caption", "Client Secret", item.Caption);
			AssertContains("Hint", @"This is the Client Secret that has been registered in the Azure platform.  The Client Secret is the value in the Value column of Certificates & secrets settings in Azure.  For added security, the Client Secret will be encrypted when the Registry is saved.

Important: Please consider the expiry period of the Client Secret in the Certificates & secrets page of your App registration in Azure as an expired Client Secret can lead to authentication issues.  Please refer to the Registering App in Microsoft Azure Technical Guide at https://myaccount.cargowise.com/Home/CargoWise/TechnicalGuides.aspx for further details on registering an App in Azure.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
			AssertEquals("DefaultValue", string.Empty, item.DefaultValue);

			var token = new byte[] { 1, 2 };
			ItemSet.Ms365OAuth2AppTokenForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, token);
			AssertEquals("Pre-Condition", token, ItemSet.Ms365OAuth2AppTokenForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.OnUpdateAction(Guid.Empty, Guid.Empty, Guid.Empty, "secret");
			AssertEquals("Test OnUpdateAction: AppToken will be deleted", true, ItemSet.Ms365OAuth2AppTokenForOutgoing.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).IsNullOrEmpty());
		}

		public void TestOAuth2AppTokenForOutgoing()
		{
			var item = ItemSet.Ms365OAuth2AppTokenForOutgoing;
			AssertEquals("Name", "Ms365OAuth2AppTokenForOutgoing", item.Name);
			AssertEquals("Category", "", item.Category);
			AssertContains("Caption", null, item.Caption);
			AssertContains("Hint", null, item.Hint);

			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden, item.Options);
		}

		public void TestMsOAuth2TenantId()
		{
			var item = ItemSet.Ms365OAuth2TenantId;
			AssertEquals("Name", "Ms365OAuth2TenantId", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_M365, item.Category);
			AssertEquals("Caption", "Tenant ID", item.Caption);
			AssertEquals("Hint", "The Tenant ID is used to identify the organization when authenticating the user. If your account type is Single tenant, enter in the Tenant ID. When left blank, the common authority will be used.", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
		}

		public void TestMicrosoftOffice365AttachmentEmailFetchLimitForDragDrop()
		{
			var item = ItemSet.MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop;
			AssertEquals("Name", "MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop", item.Name);
			AssertEquals("Category", Categories.System_RemoteApp, item.Category);
			AssertEquals("Type", typeof(IntRegistryItem), item.GetType());
			AssertEquals(100, (int)((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals(1000, (int)((IntRegistryDataType)item.DataType).UpperBound);
			AssertEquals(1000, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, item.Options);
		}

		#region Google Gmail OAuth2.0

		public void TestGmailDelegatedMailForIncoming()
		{
			var item = ItemSet.GmailDelegatedMailForIncoming;
			AssertEquals("Name", "GmailDelegatedMailForIncoming", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_GmailIn, item.Category);
			AssertEquals("Caption", "User Email", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
		}

		public void TestGmailServiceAccountKeyForIncoming()
		{
			var item = ItemSet.GmailServiceAccountKeyForIncoming;
			AssertEquals("Name", "GmailServiceAccountKeyForIncoming", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_GmailIn, item.Category);
			AssertEquals("Caption", "Service Account Key", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, item.Options);
		}

		public void TestGmailDelegatedMailForOutgoing()
		{
			var item = ItemSet.GmailDelegatedMailForOutgoing;
			AssertEquals("Name", "GmailDelegatedMailForOutgoing", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_GmailOut, item.Category);
			AssertEquals("Caption", "User Email", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, item.Options);
		}

		public void TestGmailServiceAccountKeyForOutgoing()
		{
			var item = ItemSet.GmailServiceAccountKeyForOutgoing;
			AssertEquals("Name", "GmailServiceAccountKeyForOutgoing", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_Mail_OAuth2_GmailOut, item.Category);
			AssertEquals("Caption", "Service Account Key", item.Caption);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, item.Options);
		}

		#endregion

		public void TestUseVerboseProtocolLogging()
		{
			var item = ItemSet.UseVerboseProtocolLogging;
			AssertEquals("Name", "UseVerboseProtocolLogging", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_SMTP, item.Category);
			AssertEquals("Caption", "Enable verbose protocol logging", item.Caption);
			AssertEquals("Hint", "When enabled, protocol logging will be available in the Log File of the Outbound Mail Service task.\r\nWarning:  This will generate a large volume of entries into the Log Files.  It is advised to enable this for debugging purposes only.\r\nNote:  The protocol logs are generated at the Debug level, which requires the System > Process Controller > Logging > Enable verbose logging to be enabled. In WiseCloud hosted systems, this can only be enabled by WiseCloud support.", item.Hint);

			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(!(bool)ItemSet.UseVerboseProtocolLogging.DefaultValue);

			ItemSet.UseVerboseProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert((bool)ItemSet.UseVerboseProtocolLogging.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.UseVerboseProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!(bool)ItemSet.UseVerboseProtocolLogging.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.UseVerboseProtocolLogging.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.UseVerboseProtocolLogging.IsReadOnly);
			}
		}

		public void TestUseVerboseProtocolLogging_MailKitEnabled()
		{
			ItemSet.UseVerboseProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				Assert("Protcol logging should not be hidden when MailKit is enabled", !ItemSet.UseVerboseProtocolLogging.HasOption(RegistryOptions.IsHidden));
			});
		}

		public void TestSMTPServer()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.SMTPServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestID");
			AssertEquals("SMTPServer", "TestID", ItemSet.SMTPServer.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.SMTPServer.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPServer.IsReadOnly);
			}
		}

		public void TestSMTPServerReadOnly()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should not be able to edit this", ItemSet.SMTPServer.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPServer.IsReadOnly);
			}
		}

		public void TestSMTPServerNonHosted()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.SMTPServer.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPServer.IsReadOnly);
			}
		}

		public void TestSMTPServerTimeout()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("Default value", 30, ItemSet.SMTPServerTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.SMTPServerTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, ItemSet.SMTPServerTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.SMTPServerTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0, ItemSet.SMTPServerTimeout.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.SMTPServerTimeout.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPServerTimeout.IsReadOnly);
			}
		}

		public void TestAttachmentEncodingFormat()
		{
			var item = ItemSet.AttachmentEncodingFormat;

			AssertEquals("RFC2047", item.DefaultValue);
			AssertEquals("AttachmentEncodingFormat", item.Name);
			AssertEquals(RawDataRegistry.Categories.PhysicalServer_MailOut, item.Category);
			AssertEquals("Caption", "Attachment Encoding Format", item.Caption);
			AssertEquals("Hint", "This option specifies the encoding format used for attachments", item.Hint);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, item.Storage);
		}

		public void TestAttachmentEncodingFormatReadOnly()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should not be able to edit this", ItemSet.AttachmentEncodingFormat.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.AttachmentEncodingFormat.IsReadOnly);
			}
		}

		public void TestAttachmentEncodingFormatNonHosted()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.AttachmentEncodingFormat.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.AttachmentEncodingFormat.IsReadOnly);
			}
		}

		public void TestRemoveNonAsciiCharactersInEmailMessageHeader()
		{
			AssertEquals("[PRE-CONDITION] RemoveNonAsciiCharactersInEmailMessageHeader", false, ItemSet.RemoveNonAsciiCharactersInEmailMessageHeader.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.RemoveNonAsciiCharactersInEmailMessageHeader.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("RemoveNonAsciiCharactersInEmailMessageHeader", true, ItemSet.RemoveNonAsciiCharactersInEmailMessageHeader.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDatabaseVersion()
		{
			AssertNotNull("DatabaseMajorSchemaVersion", ItemSet.DatabaseMajorSchemaVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMinorSchemaVersion", ItemSet.DatabaseMinorSchemaVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMajorScriptVersion", ItemSet.DatabaseMajorScriptVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMinorScriptVersion", ItemSet.DatabaseMinorScriptVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMajorTransformationVersion", ItemSet.DatabaseMajorTransformationVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMinorTransformationVersion", ItemSet.DatabaseMinorTransformationVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseSystemDataVersionMajor", ItemSet.DatabaseSystemDataVersionMajor.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseSystemDataVersionMinor", ItemSet.DatabaseSystemDataVersionMinor.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseSystemGlowDataVersionMajor", ItemSet.DatabaseSystemDataVersionMinor.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMajorClrAssembliesVersion", ItemSet.DatabaseMajorClrAssembliesVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNotNull("DatabaseMinorClrAssembliesVersion", ItemSet.DatabaseMinorClrAssembliesVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestComplianceVersionNumber()
		{
			AssertEquals(string.Empty, ItemSet.ComplianceVersionNumber.Value);

			ItemSet.ComplianceVersionNumber.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12.12.12");
			AssertEquals("12.12.12", ItemSet.ComplianceVersionNumber.Value);

			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			company[GlbCompanySchema.GC_RX_NKLocalCurrency] = "EUR";
			company[GlbCompanySchema.GC_RN_NKCountryCode] = "PT";
			factory.Save();

			var versionNumber = string.Join(".", ReleaseInfo.Instance.VersionNumber.ToString().Split('.').Take(3)); // deliberately creating the version number a different way to the production code
			AssertEquals(versionNumber, DataRegistry.Instance.RawRegistry.ComplianceVersionNumber.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		public void TestParameterizeInsertAndUpdateStatements()
		{
			AssertEquals("[PRE-CONDITION] ParameterizeInsertAndUpdateStatements", true, ItemSet.ParameterizeInsertAndUpdateStatements.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.ParameterizeInsertAndUpdateStatements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ParameterizeInsertAndUpdateStatements", false, ItemSet.ParameterizeInsertAndUpdateStatements.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestFieldsToLiteralize()
		{
			AssertEquals("[PRE-CONDITION] FieldsToLiteralize", "AH_TransactionType, VX_ParentTableCode, VJ_TableName", ItemSet.FieldsToLiteralize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.FieldsToLiteralize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SL_PK");
			AssertEquals("FieldsToLiteralize", "SL_PK", ItemSet.FieldsToLiteralize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestTVPRule()
		{
			AssertEquals("[PRE-CONDITION] TVPRule", "5,10,20,50,100", ItemSet.TVPRule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.TVPRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "10,50,100,500");
			AssertEquals("TVPRule", "10,50,100,500", ItemSet.TVPRule.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestConcatenateMultipleFetchHintTypes()
		{
			AssertEquals("[PRE-CONDITION] ConcatenateMultipleFetchHintTypes", false, ItemSet.ConcatenateMultipleFetchHintTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.ConcatenateMultipleFetchHintTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ConcatenateMultipleFetchHintTypes", true, ItemSet.ConcatenateMultipleFetchHintTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestNotificationGroup()
		{
			Guid testGroup = Guid.NewGuid();
			ItemSet.NotificationGroup.SetValue(Guid.Empty, BranchPK, DepartmentPK, testGroup);
			AssertEquals("NotificationGroup", testGroup, ItemSet.NotificationGroup.GetValueWithoutFallback(Guid.Empty, BranchPK, DepartmentPK));
			AssertEquals("Company Notification Group Category", RawDataRegistry.Categories.Notification, ItemSet.NotificationGroup.Category);
		}

		public void TestOrgShowConsigneeConsignorTab()
		{
			ItemSet.OrgShowConsigneeConsignorTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("OrgShowConsigneeConsignorTab", true, ItemSet.OrgShowConsigneeConsignorTab.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCalendarIntegration()
		{
			ItemSet.CalendarIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ItemSet.CalendarIntegration.SetValue(CompanyPK, Guid.Empty, Guid.Empty, false);
			ItemSet.CalendarIntegration.SetValue(Guid.Empty, BranchPK, Guid.Empty, true);
			AssertEquals("CalendarIntegration", true, ItemSet.CalendarIntegration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("CalendarIntegration", false, ItemSet.CalendarIntegration.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals("CalendarIntegration", true, ItemSet.CalendarIntegration.GetValueWithoutFallback(Guid.Empty, BranchPK, Guid.Empty));
		}

		public void TestDisplayUtcOffset()
		{
			ItemSet.DisplayUtcOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("DisplayUtcOffset should be true", true, ItemSet.DisplayUtcOffset.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.DisplayUtcOffset.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("DisplayUtcOffset should be false", false, ItemSet.DisplayUtcOffset.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AssertEquals("Display GMT Offset for DateTimeOffset fields", ItemSet.DisplayUtcOffset.Caption);
			AssertEquals("Specifies whether the GMT Offset is displayed for DateTimeOffset fields.", ItemSet.DisplayUtcOffset.Hint);
		}

		public void TestFontListIsLazyLoaded()
		{
			Assert("Precondition: Font list should not be created before it is accessed", !ItemSet.fontListIsCreated);

			var fonts = ItemSet.FontList.CodeDescriptionPairList;
			Assert("Font list should now be created", ItemSet.fontListIsCreated);
		}

		public void TestSystemFont()
		{
			var defaultFont = "Tahoma";
			var companyFont = "Times New Roman";
			ItemSet.SystemFontRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFont);
			ItemSet.SystemFontRegItem.SetValue(CompanyPK, Guid.Empty, Guid.Empty, companyFont);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.SystemFontRegItem.Options);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.SystemFontRegItem.Category);
			AssertEquals("GlobalFont", defaultFont, ItemSet.SystemFontRegItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GlobalFont", companyFont, ItemSet.SystemFontRegItem.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestNavigationBarFont()
		{
			var defaultFont = "Tahoma";
			var companyFont = "Times New Roman";
			ItemSet.NavigationMenuFontRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFont);
			ItemSet.NavigationMenuFontRegItem.SetValue(CompanyPK, Guid.Empty, Guid.Empty, companyFont);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.NavigationMenuFontRegItem.Options);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.NavigationMenuFontRegItem.Category);
			AssertEquals("GlobalFont", defaultFont, ItemSet.NavigationMenuFontRegItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GlobalFont", companyFont, ItemSet.NavigationMenuFontRegItem.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestNewsPanelFont()
		{
			var defaultFont = "Tahoma";
			var companyFont = "Times New Roman";
			ItemSet.NewsAnnouncementFontRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFont);
			ItemSet.NewsAnnouncementFontRegItem.SetValue(CompanyPK, Guid.Empty, Guid.Empty, companyFont);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.NewsAnnouncementFontRegItem.Options);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.NewsAnnouncementFontRegItem.Category);
			AssertEquals("GlobalFont", defaultFont, ItemSet.NewsAnnouncementFontRegItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("GlobalFont", companyFont, ItemSet.NewsAnnouncementFontRegItem.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestGraphicRenderingEngine()
		{
			var defaultRenderingEngine = nameof(TextRendererType.GDI);
			var testRenderingEngine = nameof(TextRendererType.GDIPlus);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.GraphicRenderingEngineRegItem.Options);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.GraphicRenderingEngineRegItem.Category);
			ItemSet.GraphicRenderingEngineRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultRenderingEngine);
			AssertEquals("Default Rendering Engine", defaultRenderingEngine, ItemSet.GraphicRenderingEngineRegItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.GraphicRenderingEngineRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testRenderingEngine);
			AssertEquals("Set to GDI+ Rendering Engine", testRenderingEngine, ItemSet.GraphicRenderingEngineRegItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestTextOnlyNoteFontSize()
		{
			AssertEquals("Default TextOnlyNoteFontSize", 10.25m, ItemSet.TextOnlyNoteFontSize.DefaultValue);
			ItemSet.TextOnlyNoteFontSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15.25m);
			AssertEquals("TextOnlyNoteFontSize", 15.25m, ItemSet.TextOnlyNoteFontSize.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestForceDialogRenderingOverTS()
		{
			AssertEquals("Default value", true, ItemSet.ForceDialogRenderingOverTS.DefaultValue);
			AssertEquals("Should be Company-level", RegistryStorageFlags.Company, ItemSet.ForceDialogRenderingOverTS.Storage);
		}

		public void TestCalendarInvitationSolutionForOrganisers()
		{
			AssertEquals("CalendarInvitationSolutionForOrganisers.DefaultValue", false, ItemSet.CalendarInvitationSolutionForOrganisers.DefaultValue);
			ItemSet.CalendarInvitationSolutionForOrganisers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CalendarInvitationSolutionForOrganisers", true, ItemSet.CalendarInvitationSolutionForOrganisers.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOrgShowARTab()
		{
			ItemSet.OrgShowARTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("OrgShowARTab", false, ItemSet.OrgShowARTab.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOrgUserFlagCount()
		{
			var userFlagLabelRegex = new Regex("^OrgUserFlag.+Label$");
			var count = ItemSet.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Count(property => userFlagLabelRegex.IsMatch(property.Name));
			AssertEquals("OrgUserFlagCount", count, ItemSet.OrgUserFlagCount);
		}

		public void TestOrgUserFlagLabels()
		{
			for (var i = 1; i <= ItemSet.OrgUserFlagCount; i++)
			{
				var userFlagLabelRegistryItemPropertyName = "OrgUserFlag" + i + "Label";
				var userFlagLabelRegistryItemProperty = ItemSet.GetType().GetProperty(userFlagLabelRegistryItemPropertyName, BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNotNull("PropertyInfo: " + userFlagLabelRegistryItemPropertyName, userFlagLabelRegistryItemProperty);
				AssertEquals("PropertyType: " + userFlagLabelRegistryItemPropertyName, typeof(MultilingualStringRegistryItem), userFlagLabelRegistryItemProperty.PropertyType);

				var userFlagLabelRegistryItem = (MultilingualStringRegistryItem)userFlagLabelRegistryItemProperty.GetValue(ItemSet);
				CombineAssertions("Properties for " + userFlagLabelRegistryItemPropertyName, () =>
				{
					AssertEquals("Name", userFlagLabelRegistryItemPropertyName, userFlagLabelRegistryItem.Name);
					AssertEquals("Category", RawDataRegistry.Categories.SalesMarketing_ClientIntelligence_MarketingFlags, userFlagLabelRegistryItem.Category);
					AssertEquals("Caption", "User Flag " + i.ToString().PadLeft(2, '0') + " Label", userFlagLabelRegistryItem.Caption);
					AssertEquals("Hint", "This will be displayed in the label on the Sales tab page.", userFlagLabelRegistryItem.Hint);
					AssertEquals("DefaultValue", "User Flag " + i.ToString().PadLeft(2, '0'), userFlagLabelRegistryItem.DefaultValue);
				});
			}
		}

		public void TestMailServer()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.MailServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			AssertEquals("MailServer", "ABC", ItemSet.MailServer.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.MailServer.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.MailServer.IsReadOnly);
			}
		}

		public void TestMailPort()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.MailServerPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals("MailServerPort", 100, ItemSet.MailServerPort.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.MailServerPort.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.MailServerPort.IsReadOnly);
			}
		}

		public void TestSMTPPort()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.SMTPPort.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 300);
			AssertEquals("SMTPPort", 300, ItemSet.SMTPPort.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.SMTPPort.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPPort.IsReadOnly);
			}
		}

		public void TestMailBoxUserName()
		{
			AssertEquals("Name", "MailboxUserName", ItemSet.MailboxUserName.Name);
			AssertEquals("Category", "Physical Server/Mail/Incoming", ItemSet.MailboxUserName.Category);
			AssertEquals("Caption", "Mailbox User Name", ItemSet.MailboxUserName.Caption);
			AssertEquals("Hint", "When OAuth 2.0 and Graph API are enabled, this account will be used to download email messages from the mail server. When OAuth 2.0 and Graph API are not enabled, this value of this setting will not be used. \r\n\r\nE.g. user@example.com", ItemSet.MailboxUserName.Hint);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.MailboxUserName.Options);
			AssertEquals("DefaultValue", "", ItemSet.MailboxUserName.DefaultValue);

			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.MailboxUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("MailboxUserName", "test", ItemSet.MailboxUserName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.MailboxUserName.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.MailboxUserName.IsReadOnly);
			}
		}

		public void TestMailboxEmailAddress()
		{
			AssertEquals("Name", "MailboxEmailAddress", ItemSet.MailboxEmailAddress.Name);
			AssertEquals("Category", "Physical Server/Mail/Incoming", ItemSet.MailboxEmailAddress.Category);
			AssertEquals("Caption", "Mailbox Email Address", ItemSet.MailboxEmailAddress.Caption);
			AssertEquals("Hint", "This email address will be used to authenticate to the POP3/IMAP server. Additionally, this email address will show as the From sender when emails are sent from the system email address. \r\n\r\nE.g. pop3@domain_name.com", ItemSet.MailboxEmailAddress.Hint);
			AssertEquals("Options", RegistryOptions.PreserveTestValue, ItemSet.MailboxEmailAddress.Options);
			AssertEquals("DefaultValue", "", ItemSet.MailboxEmailAddress.DefaultValue);

			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.MailboxEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pop3@edi.com.au");
			AssertEquals("pop3@edi.com.au", ItemSet.SMTPDefaultReturnEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.MailboxEmailAddress.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.MailboxEmailAddress.IsReadOnly);
			}
		}

		public void TestSMTPDefaultReturnEmailAddress()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.MailboxEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pop3@edi.com.au");
			AssertEquals("pop3@edi.com.au", ItemSet.SMTPDefaultReturnEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.SMTPDefaultReturnEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "default@edi.com.au");
			AssertEquals("default@edi.com.au", ItemSet.SMTPDefaultReturnEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.SMTPDefaultReturnEmailAddress.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPDefaultReturnEmailAddress.IsReadOnly);
			}
		}

		public void TestSMTPDefaultDoNotReplyEmailAddress()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("PleaseDoNotReply@wisetechglobal.com", ItemSet.SMTPDefaultDoNotReplyEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.SMTPDefaultDoNotReplyEmailAddress.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "company@test.com");
			AssertEquals("company@test.com", ItemSet.SMTPDefaultDoNotReplyEmailAddress.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));

			ItemSet.SMTPDefaultDoNotReplyEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com");
			AssertEquals("test@test.com", ItemSet.SMTPDefaultDoNotReplyEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.SMTPDefaultDoNotReplyEmailAddress.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.SMTPDefaultDoNotReplyEmailAddress.IsReadOnly);
			}
		}

		public void TestCustomsMailboxPassword()
		{
			ItemSet.AllowHostedClientAccessToEmailSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ItemSet.MailboxPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pass");
			AssertEquals("MailboxPassword", "pass", ItemSet.MailboxPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			EnvProxy.SetHostedLocationForTest("SYD");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				Assert("webuser should be able to edit this", !ItemSet.MailboxPassword.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				Assert("support should be able to edit this", !ItemSet.MailboxPassword.IsReadOnly);
			}
		}

		public void TestResourceStringUsageTrustedDomains()
		{
			AssertEquals("Name", "ResourceStringUsageTrustedDomains", ItemSet.ResourceStringUsageTrustedDomains.Name);
			AssertEquals("Category", RawDataRegistry.Categories.WebAndVisibility, ItemSet.ResourceStringUsageTrustedDomains.Category);
			AssertEquals("Caption", "Resource String Usage Trusted Domains", ItemSet.ResourceStringUsageTrustedDomains.Caption);
			AssertEquals("Hint", "Resource String Usage Trusted Domains.", ItemSet.ResourceStringUsageTrustedDomains.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ResourceStringUsageTrustedDomains.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, ItemSet.ResourceStringUsageTrustedDomains.Options);
			AssertEquals("MaximumLength", 256, ItemSet.ResourceStringUsageTrustedDomains.DataType.MaximumLength);
		}

		public void TestUserHelpMode()
		{
			// set this as the initial state
			ItemSet.UserHelpMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nameof(HelpMode.Standard));
			AssertEquals("UserHelpMode", nameof(HelpMode.Standard), ItemSet.UserHelpMode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.UserHelpMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nameof(HelpMode.Expert));
			AssertEquals("UserHelpMode", nameof(HelpMode.Expert), ItemSet.UserHelpMode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.UserHelpMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nameof(HelpMode.Training));
			AssertEquals("UserHelpMode", nameof(HelpMode.Training), ItemSet.UserHelpMode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.UserHelpMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nameof(HelpMode.Standard));
			AssertEquals("UserHelpMode", nameof(HelpMode.Standard), ItemSet.UserHelpMode.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestNavBarState()
		{
			ItemSet.NavBarState.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("NavBarState", "abc", ItemSet.NavBarState.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestTimeLineView()
		{
			AssertEquals("Default value should be \"TimeLine\"", "TimeLine", ItemSet.TimeLineView.GetValueWithoutFallback(Guid.Empty, Guid.Empty, BranchPK));
			ItemSet.TimeLineView.SetValue(Guid.Empty, Guid.Empty, BranchPK, "abc");
			AssertEquals("TimeLineViewState", "abc", ItemSet.TimeLineView.GetValueWithoutFallback(Guid.Empty, Guid.Empty, BranchPK));
		}

		public void TestShowUnshippedOrders()
		{
			AssertEquals("Default value should be \"DoNotShow\"", "DoNotShow", ItemSet.ShowUnshippedOrders.GetValueWithoutFallback(Guid.Empty, Guid.Empty, BranchPK));
			ItemSet.ShowUnshippedOrders.SetValue(Guid.Empty, Guid.Empty, BranchPK, "abcd");
			AssertEquals("ShowUnshippedOrdersMode", "abcd", ItemSet.ShowUnshippedOrders.GetValueWithoutFallback(Guid.Empty, Guid.Empty, BranchPK));
		}

		public void TestWebUserDefaultSettingsForDocAddress()
		{
			AssertEquals("Default value should be empty", "", ItemSet.WebUserDefaultSettingsForDocAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, BranchPK));

			WebUserDefaultSettingsForDocAddress expectedSettings = new WebUserDefaultSettingsForDocAddress(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "DevileryAddress");
			ItemSet.WebUserDefaultSettingsForDocAddress.SetValue(Guid.Empty, Guid.Empty, BranchPK, expectedSettings.ToString());

			AssertEquals("WebUserDefaultSettingsForDocAddress", expectedSettings.ToString(), ItemSet.WebUserDefaultSettingsForDocAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, BranchPK).ToString());
		}

		public void TestLastAccessedModule()
		{
			ItemSet.LastAccessedModule.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("LastAccessedModule", "abc", ItemSet.LastAccessedModule.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestDoNotDisplayReleaseNotesVersion()
		{
			ItemSet.DoNotDisplayReleaseNotesVersion.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("DoNotDisplayReleaseNotesVersion", "abc", ItemSet.DoNotDisplayReleaseNotesVersion.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestGlobalFormTopCaption()
		{
			ItemSet.GlobalFormTopCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("GlobalFormTopCaption", "abc", ItemSet.GlobalFormTopCaption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShowDatabaseName()
		{
			AssertEquals("ShowDatabaseName DefaultValue", false, ItemSet.ShowDatabaseName.DefaultValue);
			ItemSet.ShowDatabaseName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ShowDatabaseName", true, ItemSet.ShowDatabaseName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShowBranchName()
		{
			AssertEquals("ShowBranchName DefaultValue", true, ItemSet.ShowBranchName.DefaultValue);
			ItemSet.ShowBranchName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ShowBranchName", false, ItemSet.ShowBranchName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShowCompanyName()
		{
			AssertEquals("ShowCompanyName DefaultValue", true, ItemSet.ShowCompanyName.DefaultValue);
			ItemSet.ShowCompanyName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ShowCompanyName", false, ItemSet.ShowCompanyName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShowDepartmentName()
		{
			AssertEquals("ShowDepartmentName DefaultValue", true, ItemSet.ShowDepartmentName.DefaultValue);
			ItemSet.ShowDepartmentName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ShowDepartmentName", false, ItemSet.ShowDepartmentName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestShowUserName()
		{
			AssertEquals("ShowUserName DefaultValue", false, ItemSet.ShowUserName.DefaultValue);
			ItemSet.ShowUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ShowUserName", true, ItemSet.ShowUserName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPasswordControl()
		{
			string sqlText = @"	DELETE	FROM dbo.StmData
								WHERE	SD_Name = @name";
			DbCommand cmd = Db.Connection.Command(sqlText);// rolling back 15543
			cmd.AddParameterBasedOnDbColumn("@name", ItemSet.PromptPasswordChangeBeforeExpireDays.Name, StmDataSchema.SD_Name);
			cmd.ExecuteNonQuery();

			AssertEquals("Prompt password default value", 7, ItemSet.PromptPasswordChangeBeforeExpireDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.PromptPasswordChangeBeforeExpireDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			AssertEquals("Prompt password changed value", 3, ItemSet.PromptPasswordChangeBeforeExpireDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPasswordControl_ADEnabled()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;
			AssertEquals("LoginAttempts should be hidden when AD enabled", true, ItemSet.LoginAttempts.HasOption(RegistryOptions.IsHidden));
			AssertEquals("LoginLockoutMinutes should be hidden when AD enabled", true, ItemSet.LoginLockoutMinutes.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinLength should be hidden when AD enabled", true, ItemSet.PasswordMinLength.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinUpperAlphas should be hidden when AD enabled", true, ItemSet.PasswordMinUpperAlphas.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinLowerAlphas should be hidden when AD enabled", true, ItemSet.PasswordMinLowerAlphas.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinNumeric should be hidden when AD enabled", true, ItemSet.PasswordMinNumeric.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinNonAlphNums should be hidden when AD enabled", true, ItemSet.PasswordMinNonAlphNums.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordHashingIterationsCount should not be hidden despite AD enabled", false, ItemSet.PasswordHashingIterationsCount.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PromptPasswordChangeBeforeExpireDays should not be hidden despite AD enabled", false, ItemSet.PromptPasswordChangeBeforeExpireDays.HasOption(RegistryOptions.IsHidden));
		}

		public void TestPasswordControl_ADDisabled()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = false;
			AssertEquals("LoginAttempts should not be hidden when AD disabled", false, ItemSet.LoginAttempts.HasOption(RegistryOptions.IsHidden));
			AssertEquals("LoginLockoutMinutes should not be hidden when AD disabled", false, ItemSet.LoginLockoutMinutes.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinLength should not be hidden when AD disabled", false, ItemSet.PasswordMinLength.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinUpperAlphas should not be hidden when AD disabled", false, ItemSet.PasswordMinUpperAlphas.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinLowerAlphas should not be hidden when AD disabled", false, ItemSet.PasswordMinLowerAlphas.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinNumeric should not be hidden when AD disabled", false, ItemSet.PasswordMinNumeric.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordMinNonAlphNums should not be hidden when AD disabled", false, ItemSet.PasswordMinNonAlphNums.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PromptPasswordChangeBeforeExpireDays should not be hidden when AD disabled", false, ItemSet.PromptPasswordChangeBeforeExpireDays.HasOption(RegistryOptions.IsHidden));
			AssertEquals("PasswordHashingIterationsCount should not be hidden when AD disabled", false, ItemSet.PasswordHashingIterationsCount.HasOption(RegistryOptions.IsHidden));
		}

		public void TestPasswordChangeDaysMaximum()
		{
			AssertEquals("Password change days default value", 0, ItemSet.PasswordChangeDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			ItemSet.PasswordChangeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 365);
			AssertEquals("Password change days new value", 365, ItemSet.PasswordChangeDays.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown(typeof(RegistryValidationException), delegate
			{
				ItemSet.PasswordChangeDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 366);
			});
		}

		public void TestBorderWiseAPIKey()
		{
			AssertEquals("NjAyMDRmN2MtMjA1OC00YjY4LWI1ZTEtZmQ0NDY5ZGE3ZTA2", DataRegistry.BorderWiseAPIKey);
		}

		public void TestBorderWiseUmpApiBaseAddress()
		{
			AssertEquals("https://ump.borderwise.com/api", ItemSet.BorderWiseUmpApiBaseAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestBorderWiseWebAddress()
		{
			AssertEquals("https://app.borderwise.com", ItemSet.BorderWiseWebAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPasswordHashingIterationsCount()
		{
			var item = ItemSet.PasswordHashingIterationsCount;

			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(200_000, item.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAUImportsessagingMode()
		{
			ItemSet.AUImportsMessagingMode.SetValue(CompanyPK, Guid.Empty, Guid.Empty, Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
			AssertEquals("AUImportsMessagingMode", Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, ItemSet.AUImportsMessagingMode.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestInterchangeResendDelay()
		{
			ItemSet.InterchangeResendDelay.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 15);
			AssertEquals("InterchangeResendDelay", 15, ItemSet.InterchangeResendDelay.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestInterchangeMaxSends()
		{
			ItemSet.InterchangeMaxSends.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 2);
			AssertEquals("InterchangeMaxSends", 2, ItemSet.InterchangeMaxSends.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals("Min value", 1, (int)(ItemSet.InterchangeMaxSends.DataType as IntRegistryDataType).LowerBound);
			AssertEquals("Max value", 3, (int)(ItemSet.InterchangeMaxSends.DataType as IntRegistryDataType).UpperBound);
		}

		public void TestDocumentOpeningText()
		{
			AssertNotNull("DefaultValue should not be null.", ItemSet.DocumentOpeningText.DefaultValue);
			object value = ItemSet.DocumentOpeningText.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("DocumentOpeningText", value is MultilingualString);
		}

		public void TestDocumentClosingText()
		{
			AssertNotNull("DefaultValue should not be null.", ItemSet.DocumentClosingText.DefaultValue);
			object value = ItemSet.DocumentClosingText.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK);
			Assert("DocumentClosingText", value is MultilingualString);
		}

		public void TestQuotationDocumentLogo()
		{
			Image testImage = SystemIcons.Information.ToBitmap();
			ItemSet.QuotationDocumentLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testImage);
			AssertNotNull((Image)ItemSet.QuotationDocumentLogo.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestBackupDirectoryLocation()
		{
			ItemSet.BackupDirectoryPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("BackupFilePath", "test", ItemSet.BackupDirectoryPath.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestBackupSystemDatabases()
		{
			AssertEquals("BackupSystemDatabases DefaultValue", false, ItemSet.BackupSystemDatabases.DefaultValue);
			ItemSet.BackupSystemDatabases.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Backup System Databases", true, ItemSet.BackupSystemDatabases.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestBackupReferenceDatabases()
		{
			AssertEquals("BackupReferenceDatabases DefaultValue", true, ItemSet.BackupReferenceDatabases.DefaultValue);
			ItemSet.BackupReferenceDatabases.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Backup Reference Databases", false, ItemSet.BackupReferenceDatabases.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestUseDbBackupCompression()
		{
			AssertEquals("UseDbBackupCompression DefaultValue", true, ItemSet.UseDbBackupCompression.DefaultValue);
			ItemSet.UseDbBackupCompression.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Use Database Backup Compression", false, ItemSet.UseDbBackupCompression.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDaysOfLogBackupToKeep()
		{
			AssertEquals("DaysOfLogBackupToKeep should have a default value of 10.", 10, ItemSet.DaysOfLogBackupToKeep.DefaultValue);
			ItemSet.DaysOfLogBackupToKeep.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			AssertEquals("DaysOfLogBackupToKeep should have been set to 3.", 3, DataRegistry.Instance.DaysOfLogBackupToKeep);
		}

		public void TestSkipLogShrinkingAndBackupInUpgrade()
		{
			AssertEquals("SkipLogShrinkingAndBackupInUpgrade DefaultValue", false, ItemSet.SkipLogShrinkingAndBackupInUpgrade.DefaultValue);
			ItemSet.SkipLogShrinkingAndBackupInUpgrade.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Skip Log Shrinking And Backup In Upgrade", true, ItemSet.SkipLogShrinkingAndBackupInUpgrade.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestLogDeltaSizeInKbToBackupDirectly()
		{
			AssertEquals("LogDeltaSizeInKbToBackupDirectly DefaultValue", 200, ItemSet.LogDeltaSizeInKbToBackupDirectly.DefaultValue);
			ItemSet.LogDeltaSizeInKbToBackupDirectly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 150);
			AssertEquals("LogDeltaSizeInKbToBackupDirectly modified", 150, ItemSet.LogDeltaSizeInKbToBackupDirectly.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOnlySupportsSqlServerEnterpriseEdition()
		{
			AssertEquals(
				"OnlySupportsSqlServerEnterpriseEdition should be true by default",
				true, ItemSet.OnlySupportsSqlServerEnterpriseEdition.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.OnlySupportsSqlServerEnterpriseEdition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("OnlySupportsSqlServerEnterpriseEdition",
				false, ItemSet.OnlySupportsSqlServerEnterpriseEdition.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestZSqlSaverRowsToPostPerSqlStatement()
		{
			AssertEquals(500, ItemSet.ZSqlSaverRowsToPostPerSqlStatement.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)ItemSet.ZSqlSaverRowsToPostPerSqlStatement.DataType).LowerBound);
			AssertEquals((double)1000, ((IntRegistryDataType)ItemSet.ZSqlSaverRowsToPostPerSqlStatement.DataType).UpperBound);

			ItemSet.ZSqlSaverRowsToPostPerSqlStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, ItemSet.ZSqlSaverRowsToPostPerSqlStatement.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPurgeDataTimeoutLimit()
		{
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.PurgeDataTimeoutLimit.Options);
			AssertEquals(30, ItemSet.PurgeDataTimeoutLimit.DefaultValue);
			AssertEquals((double)5, ((IntRegistryDataType)ItemSet.PurgeDataTimeoutLimit.DataType).LowerBound);
			AssertEquals((double)1440, ((IntRegistryDataType)ItemSet.PurgeDataTimeoutLimit.DataType).UpperBound);

			ItemSet.PurgeDataTimeoutLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, ItemSet.PurgeDataTimeoutLimit.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestPurgeDataRunStatusFlag()
		{
			AssertEquals(RegistryOptions.IsHidden, ItemSet.PurgeDataRunStatusFlag.Options);
			AssertEquals("CMP", ItemSet.PurgeDataRunStatusFlag.DefaultValue);

			ItemSet.PurgeDataRunStatusFlag.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "RUN");
			AssertEquals("RUN", ItemSet.PurgeDataRunStatusFlag.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanerLockTimeoutSeconds()
		{
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.GhostRecordCleanerLockTimeoutSeconds.Options);
			AssertEquals(10, ItemSet.GhostRecordCleanerLockTimeoutSeconds.DefaultValue);

			ItemSet.GhostRecordCleanerLockTimeoutSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			AssertEquals(20, ItemSet.GhostRecordCleanerLockTimeoutSeconds.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanerLockTimeoutSeconds_Hosted()
		{
			var originalHostedPlace = EnvProxy.HostedLocation;
			try
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(RegistryOptions.IsOnlyEditableBySupportIfHosted, ItemSet.GhostRecordCleanerLockTimeoutSeconds.Options);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedPlace);
			}
		}

		public void TestGhostRecordCleanUpThreshold()
		{
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.GhostRecordCleanUpThreshold.Options);
			AssertEquals(5000, ItemSet.GhostRecordCleanUpThreshold.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)ItemSet.GhostRecordCleanUpThreshold.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)ItemSet.GhostRecordCleanUpThreshold.DataType).UpperBound);

			ItemSet.GhostRecordCleanUpThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, ItemSet.GhostRecordCleanUpThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanUpThreshold_Hosted()
		{
			var originalHostedPlace = EnvProxy.HostedLocation;
			try
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(RegistryOptions.IsOnlyEditableBySupportIfHosted, ItemSet.GhostRecordCleanUpThreshold.Options);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedPlace);
			}
		}

		public void TestGhostRecordCleanUpRebuildWaitMinutes()
		{
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.GhostRecordCleanUpRebuildWaitMinutes.Options);
			AssertEquals(1, ItemSet.GhostRecordCleanUpRebuildWaitMinutes.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)ItemSet.GhostRecordCleanUpRebuildWaitMinutes.DataType).LowerBound);
			AssertEquals((double)60, ((IntRegistryDataType)ItemSet.GhostRecordCleanUpRebuildWaitMinutes.DataType).UpperBound);

			ItemSet.GhostRecordCleanUpRebuildWaitMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, ItemSet.GhostRecordCleanUpRebuildWaitMinutes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanUpRebuildWaitMinutes_Hosted()
		{
			var originalHostedPlace = EnvProxy.HostedLocation;
			try
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals(RegistryOptions.IsOnlyEditableBySupportIfHosted, ItemSet.GhostRecordCleanUpRebuildWaitMinutes.Options);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedPlace);
			}
		}

		public void TestGhostRecordProcessingThreadInterval()
		{
			AssertEquals(RegistryOptions.IsHidden, ItemSet.GhostRecordProcessingThreadInterval.Options);
			AssertEquals(5, ItemSet.GhostRecordProcessingThreadInterval.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)ItemSet.GhostRecordProcessingThreadInterval.DataType).LowerBound);
			AssertEquals((double)60, ((IntRegistryDataType)ItemSet.GhostRecordProcessingThreadInterval.DataType).UpperBound);

			ItemSet.GhostRecordProcessingThreadInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, ItemSet.GhostRecordProcessingThreadInterval.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordMaxProcessingThreadCount()
		{
			AssertEquals(RegistryOptions.IsHidden, ItemSet.GhostRecordMaxProcessingThreadCount.Options);
			AssertEquals(10, ItemSet.GhostRecordMaxProcessingThreadCount.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)ItemSet.GhostRecordMaxProcessingThreadCount.DataType).LowerBound);
			AssertEquals((double)20, ((IntRegistryDataType)ItemSet.GhostRecordMaxProcessingThreadCount.DataType).UpperBound);

			ItemSet.GhostRecordMaxProcessingThreadCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
			AssertEquals(7, ItemSet.GhostRecordMaxProcessingThreadCount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanupOnlineRebuild()
		{
			AssertEquals(true, ItemSet.GhostRecordCleanupOnlineRebuild.DefaultValue);

			ItemSet.GhostRecordCleanupOnlineRebuild.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.GhostRecordCleanupOnlineRebuild.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanupKillBlockersThreshold()
		{
			AssertEquals(-1, ItemSet.GhostRecordCleanupKillBlockersThreshold.DefaultValue);
			AssertEquals(-1D, ((IntRegistryDataType)ItemSet.GhostRecordCleanupKillBlockersThreshold.DataType).LowerBound);
			AssertEquals(100D, ((IntRegistryDataType)ItemSet.GhostRecordCleanupKillBlockersThreshold.DataType).UpperBound);

			ItemSet.GhostRecordCleanupKillBlockersThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
			AssertEquals(7, ItemSet.GhostRecordCleanupKillBlockersThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGhostRecordCleanupCommandTimeOutThreshold()
		{
			AssertEquals(0, ItemSet.GhostRecordCleanupCommandTimeOutThreshold.DefaultValue);
			AssertEquals(0D, ((IntRegistryDataType)ItemSet.GhostRecordCleanupCommandTimeOutThreshold.DataType).LowerBound);
			AssertEquals(1000D, ((IntRegistryDataType)ItemSet.GhostRecordCleanupCommandTimeOutThreshold.DataType).UpperBound);

			ItemSet.GhostRecordCleanupCommandTimeOutThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 200);
			AssertEquals(200, ItemSet.GhostRecordCleanupCommandTimeOutThreshold.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestWebVersionUserMonitoringEnabled()
		{
			AssertEquals(RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport, ItemSet.WebVersionUserMonitoringEnabled.Options);
			AssertEquals(false, ItemSet.WebVersionUserMonitoringEnabled.DefaultValue);

			ItemSet.WebVersionUserMonitoringEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.WebVersionUserMonitoringEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestWebVersionUserMonitoringUrl()
		{
			AssertEquals(RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport, ItemSet.WebVersionUserMonitoringUrl.Options);
			AssertEquals(string.Empty, ItemSet.WebVersionUserMonitoringUrl.DefaultValue);
			ItemSet.WebVersionUserMonitoringUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost:8200");
			AssertEquals("http://localhost:8200", ItemSet.WebVersionUserMonitoringUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestWebVersionLaunchUrl()
		{
			AssertEquals(RegistryOptions.IsOnlyForCargoWise | RegistryOptions.IsOnlyForSupport, ItemSet.WebVersionLaunchUrl.Options);
			AssertEquals(string.Empty, ItemSet.WebVersionLaunchUrl.DefaultValue);
			ItemSet.WebVersionLaunchUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost:8200");
			AssertEquals("http://localhost:8200", ItemSet.WebVersionLaunchUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestSuppressDbFilesHealthCheckNotificationsForHostedSystems()
		{
			AssertEquals("DefaultValue", true, ItemSet.SuppressDbFilesHealthCheckNotificationsForHostedSystems.DefaultValue);
			ItemSet.SuppressDbFilesHealthCheckNotificationsForHostedSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.SuppressDbFilesHealthCheckNotificationsForHostedSystems.Value);
			ItemSet.SuppressDbFilesHealthCheckNotificationsForHostedSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.SuppressDbFilesHealthCheckNotificationsForHostedSystems.Value);
		}

		public void TestHeartbeatDurationSeconds()
		{
			var item = ItemSet.HeartbeatDurationSeconds;
			AssertEquals("HeartbeatDuration", item.Name);
			AssertEquals(Categories.System_Database, item.Category);
			AssertEquals("Heartbeat duration", item.Caption);
			AssertEquals("The number of seconds that a heartbeat will survive without being refreshed. Refresh is performed automatically at 40% of this value.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(RegistryOptions.IsOnlyForCargoWise, item.Options);
			AssertEquals(2700, item.DefaultValue);
		}

		public void TestLockTimeout()
		{
			var item = ItemSet.LockTimeout;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(DbConnection.LockTimeout.Default, item.DefaultValue);
			AssertEquals((double)-1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDowngradeInvalidPhoneNumbersToAWarning()
		{
			var item = ItemSet.DowngradeInvalidPhoneNumbersToAWarning;

			AssertEquals(RegistryOptions.Default, item.Options);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		public void TestDbccInitialTimeout()
		{
			var item = ItemSet.DbccInitialTimeout;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(7200000, item.DefaultValue);
			AssertEquals((double)300000, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 400000);
			AssertEquals(400000, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbccUserWaitThreshold()
		{
			var item = ItemSet.DbccUserWaitThreshold;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(5000, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbccServiceTaskWaitThreshold()
		{
			var item = ItemSet.DbccServiceTaskWaitThreshold;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(240000, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbccPollingInterval()
		{
			var item = ItemSet.DbccPollingInterval;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(3500, item.DefaultValue);
			AssertEquals((double)500, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)300000, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5000);
			AssertEquals(5000, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbccRunCheckdbWithPhysicalOnly()
		{
			var item = ItemSet.DbccRunCheckdbWithPhysicalOnly;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(false, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbccRunSecondariesInParallel()
		{
			var item = ItemSet.DbccRunSecondariesInParallel;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(false, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbccRunSecondariesByParts()
		{
			var item = ItemSet.DbccRunSecondariesByParts;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(false, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Rebuild_Threshold()
		{
			var item = ItemSet.ISU_Rebuild_ThresholdPercentage;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(40, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)100, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Rebuild_MaxDop()
		{
			var item = ItemSet.ISU_Rebuild_MaxDopPercentage;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(25, item.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)100, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Rebuild_Online()
		{
			var item = ItemSet.ISU_Rebuild_Online;

			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(true, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Rebuild_UseObserver()
		{
			var item = ItemSet.ISU_Rebuild_UseObserver;

			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(false, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Rebuild_MaxWaitInMinutes()
		{
			var item = ItemSet.ISU_Rebuild_MaxWaitInMinutes;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(1, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)120, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Rebuild_AbortAfterWait()
		{
			var item = ItemSet.ISU_Rebuild_AbortAfterWait;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self);
			AssertEquals(OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Reorganize_Threshold()
		{
			var item = ItemSet.ISU_Reorganize_ThresholdPercentage;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(10, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)100, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Reorganize_MaxConcurrentProcessesPercentage()
		{
			var item = ItemSet.ISU_Reorganize_MaxConcurrentProcessesPercentage;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(25, item.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)100, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_Reorganize_PeriodInDays()
		{
			var item = ItemSet.ISU_Reorganize_PeriodInDays;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(7, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5000);
			AssertEquals(5000, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_MinimumIndexPageCount()
		{
			var item = ItemSet.ISU_MinimumIndexPageCount;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(50, item.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5000);
			AssertEquals(5000, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_MaxBacklogWaitTime_InMinutes()
		{
			var item = ItemSet.ISU_MaxBacklogWaitTime_InMinutes;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(60, item.DefaultValue);
			AssertEquals(1, (int)((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals(600, (int)((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
			AssertEquals(120, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_EmptyStatisticsRowsThreshold()
		{
			var item = ItemSet.ISU_EmptyStatisticsRowsThreshold;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(1000, item.DefaultValue);
			AssertEquals(1, (int)((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals(1000000, (int)((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 352847);
			AssertEquals(352847, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestISU_DisableAutoStatisticsDuringUpgrade()
		{
			var item = ItemSet.ISU_DisableAutoStatisticsDuringUpgrade;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(true, item.DefaultValue);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region AUCustoms

		public void TestAUCustomsEdificeSenderID()
		{
			ItemSet.AUCustomsEdificeSenderID.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("AUCustomsEdificeSenderID", "Test", ItemSet.AUCustomsEdificeSenderID.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, ItemSet.AUCustomsEdificeSenderID.Options);
		}

		public void TestAUCustomsSenderID()
		{
			ItemSet.AUCustomsSenderID.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("AUCustomsSenderID", "Test", ItemSet.AUCustomsSenderID.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, ItemSet.AUCustomsSenderID.Options);
		}

		public void TestAUCustomsSeaCargoDepotMailbox()
		{
			ItemSet.AUCustomsSeaCargoDepotMailbox.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("AUCustomsSeaCargoDepotMailbox", "Test", ItemSet.AUCustomsSeaCargoDepotMailbox.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, ItemSet.AUCustomsSeaCargoDepotMailbox.Options);
		}

		public void TestAUCustomsCompileSiteID()
		{
			ItemSet.AUCustomsCompileSiteID.SetValue(Guid.Empty, BranchPK, Guid.Empty, "aaa111");
			AssertEquals("AUCustomsCompileSiteID", "aaa111", ItemSet.AUCustomsCompileSiteID.GetValueWithoutFallback(Guid.Empty, BranchPK, Guid.Empty));
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, ItemSet.AUCustomsCompileSiteID.Options);

			try
			{
				ItemSet.AUCustomsCompileSiteID.SetValue(Guid.Empty, BranchPK, Guid.Empty, "%jjjjj");
				Fail("Exception expected");
			}
			catch (RegistryValidationException)
			{
				Assert(true);
			}
		}

		public void TestCMRTestMode()
		{
			AssertEquals(RegistryOptions.Default, ItemSet.CMRTestMode.Options);
		}

		public void TestAUCustomsAirCargoTestMode()
		{
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, ItemSet.AUCustomsAirCargoTestMode.Options);
		}

		public void TestAUCustomsSeaCargoTestMode()
		{
			AssertEquals(RegistryOptions.IsOnlyForDevelopers, ItemSet.AUCustomsSeaCargoTestMode.Options);
		}

		public void TestAQISMessagingTestMode()
		{
			AssertEquals(RegistryOptions.Default, ItemSet.AQISMessagingTestMode.Options);
		}

		public void TestAUCHVLVSpecialReporterNumber()
		{
			ItemSet.AUCHVLVSpecialReporterNumber.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "123456");
			AssertEquals("AUCHVLVSpecialReporterNumber", "123456", ItemSet.AUCHVLVSpecialReporterNumber.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestAUCRemailSpecialReporterNumber()
		{
			ItemSet.AUCRemailSpecialReporterNumber.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "654321");
			AssertEquals("AUCRemailSpecialReporterNumber", "654321", ItemSet.AUCRemailSpecialReporterNumber.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestNEXDOCSDisableQRPView()
		{
			var item = ItemSet.NEXDOCSDisableQRPView;
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(true, item.DefaultValue);
		}

		#endregion

		#region MYCustoms

		public void TestMYCustomsUseRankAlphaForK4K5()
		{
			ItemSet.MYCustomsUseRankAlphaForK4K5.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("MYCustomsUseRankAlphaForK4K5", true, ItemSet.MYCustomsUseRankAlphaForK4K5.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
			AssertEquals(RegistryOptions.PreserveTestValue, ItemSet.MYCustomsUseRankAlphaForK4K5.Options);
		}

		public void TestMYMessageOutputDirectory()
		{
			ItemSet.MYMessageOutputDirectory.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "splaty");
			AssertEquals("MYMessageOutputDirectory", "splaty", ItemSet.MYMessageOutputDirectory.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestMYCustomsSenderID()
		{
			ItemSet.MYCustomsSenderID.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("MYCustomsSenderID", "Test", ItemSet.MYCustomsSenderID.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(RegistryOptions.PreserveTestValue, ItemSet.MYCustomsSenderID.Options);
		}

		#endregion

		public void TestOrderLineContainersVisible()
		{
			ItemSet.OrderLineContainersVisible.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("OrderLineContainersVisible", false, ItemSet.OrderLineContainersVisible.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
			ItemSet.OrderLineContainersVisible.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("OrderLineContainersVisible", true, ItemSet.OrderLineContainersVisible.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestCustomsPaymentBankAccount()
		{
			Guid accountPK = Guid.NewGuid();
			ItemSet.CustomsPaymentBankAccount.SetValue(CompanyPK, Guid.Empty, Guid.Empty, accountPK);
			AssertEquals("CustomsPaymentBankAccount", accountPK, (Guid)ItemSet.CustomsPaymentBankAccount.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestCustomsSecondPaymentBankAccount()
		{
			Guid accountPK = Guid.NewGuid();
			ItemSet.CustomsSecondPaymentBankAccount.SetValue(CompanyPK, Guid.Empty, Guid.Empty, accountPK);
			AssertEquals("CustomsSecondPaymentBankAccount", accountPK, (Guid)ItemSet.CustomsSecondPaymentBankAccount.GetFallBackValueAtAllLevels(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestMYIsTestMode()
		{
			ItemSet.MYIsTestMode.SetValue(CompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals("MYIsTestMode", true, ItemSet.MYIsTestMode.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals("MY Customs 'IsTestMode' flag should only be available to developers", true, ItemSet.MYIsTestMode.HasOption(RegistryOptions.IsOnlyForDevelopers));
		}

		public void TestLastDateCertificatesChecked()
		{
			ItemSet.LastDateCertificatesChecked.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.Time.CurrentLocalDate);
			AssertEquals("LastDateCertificatesChecked", EnvProxy.Instance.Time.CurrentLocalDate, ItemSet.LastDateCertificatesChecked.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestAECourierID()
		{
			AssertEquals("Default Value", "", ItemSet.AECourierID.DefaultValue);

			ItemSet.AECourierID.SetValue(CompanyPK, Guid.Empty, Guid.Empty, "HASH");
			AssertEquals("AE Courier ID is set", "HASH", ItemSet.AECourierID.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		#region Air Cargo

		public void TestAirCargoShipmentType()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.AirCargoShipmentType.DefaultValue;
			AssertEquals("DefaultValue Count", 3, defaultValue.Count);
			AssertEquals("GetDescriptionFromCode(\"STD\")", "Standard Shipment", defaultValue.GetDescriptionFromCode("STD"));
			AssertEquals("GetDescriptionFromCode(\"EXP\")", "Express Shipment", defaultValue.GetDescriptionFromCode("EXP"));
			AssertEquals("GetDescriptionFromCode(\"DOC\")", "Documents Only", defaultValue.GetDescriptionFromCode("DOC"));

			CodeDescriptionPairList newValue = new CodeDescriptionPairList();
			newValue.Add(new CodeDescriptionPair("XYZ", "Test Shipment"));
			ItemSet.AirCargoShipmentType.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			ReadOnlyCodeDescriptionPairList obtainedValue = ItemSet.AirCargoShipmentType.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("ObtainedValue Count", 1, obtainedValue.Count);
			AssertEquals("GetDescriptionFromCode(\"XYZ\")", "Test Shipment", obtainedValue.GetDescriptionFromCode("XYZ"));
		}

		public void TestAirCargoCommercialStatus()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.AirCargoCommercialStatus.DefaultValue;
			AssertEquals("DefaultValue Count", 1, defaultValue.Count);
			AssertEquals("Undefined - You can modify this in the System Registry, under Customs / Australia /Air Cargo", defaultValue.GetDescriptionFromCode("UDF"));

			CodeDescriptionPairList newValue = new CodeDescriptionPairList();
			newValue.Add(new CodeDescriptionPair("XYZ", "Test Shipment"));
			ItemSet.AirCargoCommercialStatus.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			ReadOnlyCodeDescriptionPairList obtainedValue = ItemSet.AirCargoCommercialStatus.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("ObtainedValue Count", 1, obtainedValue.Count);
			AssertEquals("GetDescriptionFromCode(\"XYZ\")", "Test Shipment", obtainedValue.GetDescriptionFromCode("XYZ"));
		}

		public void TestSeaCargoCommercialStatus()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.SeaCargoCommercialStatus.DefaultValue;
			AssertEquals("DefaultValue Count", 1, defaultValue.Count);
			AssertEquals("Undefined - You can modify this in the System Registry, under Customs / Australia /Sea Cargo", defaultValue.GetDescriptionFromCode("UDF"));

			CodeDescriptionPairList newValue = new CodeDescriptionPairList();
			newValue.Add(new CodeDescriptionPair("XYZ", "Test Shipment"));
			ItemSet.SeaCargoCommercialStatus.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);

			ReadOnlyCodeDescriptionPairList obtainedValue = ItemSet.SeaCargoCommercialStatus.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("ObtainedValue Count", 1, obtainedValue.Count);
			AssertEquals("GetDescriptionFromCode(\"XYZ\")", "Test Shipment", obtainedValue.GetDescriptionFromCode("XYZ"));
		}

		public void TestHVLVAirCargoSendErrors()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, ItemSet.HVLVAirCargoSendErrors.DefaultValue);
			ItemSet.HVLVAirCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Set and Get value", Constants.EmailTo.StaffMember, ItemSet.HVLVAirCargoSendErrors.Value);
		}

		public void TestHVLVAirCargoSendAcknowledgements()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, ItemSet.HVLVAirCargoSendAcknowledgements.DefaultValue);
			ItemSet.HVLVAirCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Set and Get value", Constants.EmailTo.StaffMember, ItemSet.HVLVAirCargoSendAcknowledgements.Value);
		}

		public void TestHVLVAirCargoSendImpediments()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, ItemSet.HVLVAirCargoSendImpediments.DefaultValue);
			ItemSet.HVLVAirCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Set and Get value", Constants.EmailTo.StaffMember, ItemSet.HVLVAirCargoSendImpediments.Value);
		}

		#endregion

		#region Region Number Format

		public void TestNumberGroupSeparator()
		{
			var registry = ItemSet.NumberGroupSeparator;

			AssertEquals("Name", "NumberGroupSeparator", registry.Name);
			AssertEquals("Category", Categories.System_UI_RegionNumberFormat, registry.Category);
			AssertEquals("Caption", "Number Group Separator", registry.Caption);
			AssertEquals("Hint", "The string that separates groups of digits to the left of the decimal in numeric values.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, registry.Options);
		}

		public void TestNumberDecimalSeparator()
		{
			var registry = ItemSet.NumberDecimalSeparator;

			AssertEquals("Name", "NumberDecimalSeparator", registry.Name);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI_RegionNumberFormat, registry.Category);
			AssertEquals("Caption", "Number Decimal Separator", registry.Caption);
			AssertEquals("Hint", "The string to use as the decimal separator in numeric values.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, registry.Options);
		}

		public void TestNumberGroupSizes()
		{
			var registry = ItemSet.NumberGroupSizes;

			AssertEquals("Name", "NumberGroupSizes", registry.Name);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI_RegionNumberFormat, registry.Category);
			AssertEquals("Caption", "Number Group Sizes", registry.Caption);
			AssertEquals("Hint", "The number of digits in each group to the left of the decimal in numeric values, multiple values separated by commas.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.Default, registry.Options);
		}

		public void TestDefaultValueOfNumberGroupSeparator()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-ZA")))
			{
				AssertEquals("DefaultValueOfNumberGroupSeparator", " ", ItemSet.NumberGroupSeparator.DefaultValue);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-US")))
			{
				AssertEquals("DefaultValueOfNumberGroupSeparator", ",", ItemSet.NumberGroupSeparator.DefaultValue);
			}
		}

		public void TestDefaultValueOfNumberDecimalSeparator()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-ZA")))
			{
				AssertEquals("DefaultValueOfNumberDecimalSeparator", ",", ItemSet.NumberDecimalSeparator.DefaultValue);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-US")))
			{
				AssertEquals("DefaultValueOfNumberDecimalSeparator", ".", ItemSet.NumberDecimalSeparator.DefaultValue);
			}
		}

		public void TestDefaultValueOfNumberGroupSizes()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-ZA")))
			{
				AssertEquals("DefaultValueOfNumberGroupSizes", "3", ItemSet.NumberGroupSizes.DefaultValue);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-US")))
			{
				AssertEquals("DefaultValueOfNumberGroupSizes", "3", ItemSet.NumberGroupSizes.DefaultValue);
			}
		}

		public void TestCurrencyGroupSeparator()
		{
			var registry = ItemSet.CurrencyGroupSeparator;

			AssertEquals("Name", "CurrencyGroupSeparator", registry.Name);
			AssertEquals("Category", Categories.System_UI_RegionNumberFormat, registry.Category);
			AssertEquals("Caption", "Currency Group Separator", registry.Caption);
			AssertEquals("Hint", "The string that separates groups of digits to the left of the decimal in currency values.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, registry.Options);
		}

		public void TestCurrencyDecimalSeparator()
		{
			var registry = ItemSet.CurrencyDecimalSeparator;

			AssertEquals("Name", "CurrencyDecimalSeparator", registry.Name);
			AssertEquals("Category", Categories.System_UI_RegionNumberFormat, registry.Category);
			AssertEquals("Caption", "Currency Decimal Separator", registry.Caption);
			AssertEquals("Hint", "The string to use as the decimal separator in currency values.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.IsValueMandatory, registry.Options);
		}

		public void TestCurrencyGroupSizes()
		{
			var registry = ItemSet.CurrencyGroupSizes;

			AssertEquals("Name", "CurrencyGroupSizes", registry.Name);
			AssertEquals("Category", Categories.System_UI_RegionNumberFormat, registry.Category);
			AssertEquals("Caption", "Currency Group Sizes", registry.Caption);
			AssertEquals("Hint", "The number of digits in each group to the left of the decimal in currency values, multiple values separated by commas.", registry.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, registry.Storage);
			AssertEquals("Options", RegistryOptions.Default, registry.Options);
		}

		public void TestDefaultValueOfCurrencyGroupSeparator()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-ZA")))
			{
				AssertEquals("DefaultValueOfCurrencyGroupSeparator", " ", ItemSet.CurrencyGroupSeparator.DefaultValue);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-US")))
			{
				AssertEquals("DefaultValueOfCurrencyGroupSeparator", ",", ItemSet.CurrencyGroupSeparator.DefaultValue);
			}
		}

		public void TestDefaultValueOfCurrencyDecimalSeparator()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-ZA")))
			{
				AssertEquals("DefaultValueOfCurrencyDecimalSeparator", ",", ItemSet.CurrencyDecimalSeparator.DefaultValue);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-US")))
			{
				AssertEquals("DefaultValueOfCurrencyDecimalSeparator", ".", ItemSet.CurrencyDecimalSeparator.DefaultValue);
			}
		}

		public void TestDefaultValueOfCurrencyGroupSizes()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("hi-IN")))
			{
				AssertEquals("DefaultValueOfCurrencyGroupSizes", "3,2", ItemSet.CurrencyGroupSizes.DefaultValue);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(new CultureInfo("en-US")))
			{
				AssertEquals("DefaultValueOfCurrencyGroupSizes", "3", ItemSet.CurrencyGroupSizes.DefaultValue);
			}
		}

		#endregion

		public void TestOrgListOfInterests()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.OrgListOfInterests.DefaultValue;
			AssertEquals("DefaultValue.Count", 49, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "AQS", defaultValue[0].Code);

			var newInterestList = new CodeDescriptionPairList();
			newInterestList.AddPair("EAT", "Eating");
			ItemSet.OrgListOfInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newInterestList);

			ReadOnlyCodeDescriptionPairList getValue = ItemSet.OrgListOfInterests.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("GetValue.Count", 1, getValue.Count);
			AssertEquals("GetValue[0].Code", "EAT", getValue[0].Code);
			AssertEquals("GetValue[0].Description", "Eating", getValue[0].Description);

			newInterestList.AddPair("EAT", "Eating");
			string error = ItemSet.OrgListOfInterests.GetValidationErrorMessage(newInterestList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has duplicate error", "The code 'EAT' has been duplicated. Please enter a unique code.", error);

			var emptyValueList = new CodeDescriptionPairList();
			emptyValueList.AddPair("", "Eating");
			error = ItemSet.OrgListOfInterests.GetValidationErrorMessage(emptyValueList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has error", "You cannot enter an item with no Code.", error);
		}

		public void TestOrgListOfAllocations()
		{
			var defaultValue = ItemSet.OrgListOfContactAllocations.DefaultValue;
			var dataInExpectedOrder = new (string code, string description)[]
			{
					("CAP", "CA PGA"),
					("CIV", "Commercial Invoice Party (Exporter)"),
					("CNC", "China Customs/CIQ"),
					("CUS", "Customs"),
					("HAZ", "D/G - Dangerous Goods"),
					("KRC", "Korea Company Representative"),
					("KRS", "Korea Secondary Company Representative"),
					("KRV", "KR Customs Valuation Authority Contact"),
					("NFO", "BR Foreign Operator Responsible Contact"),
					("NZB", "MPI Biosecurity"),
					("NZC", "New Zealand Customs Service"),
					("USF", "US FDA Foreign Supplier Verification Program Contact"),
					("USP", "US PGA Contact"),
					("VAT", "Customs VAT Reporting"),
			};

			CombineAssertions(() =>
			{
				AssertEquals("DefaultValue.Count", dataInExpectedOrder.Length, defaultValue.Count);
				for (var i = 0; i < dataInExpectedOrder.Length; i++)
				{
					var id = i.ToString();
					var expectedData = dataInExpectedOrder[i];
					var actualData = defaultValue[i];
					AssertEquals(id + " - Code", expectedData.code, actualData.Code);
					AssertEquals(id + " - Description", expectedData.description, actualData.Description);
				}
			});

			var newInterestList = (CodeDescriptionPairList)ItemSet.OrgListOfContactAllocations.DefaultValue;
			newInterestList.AddPair("AUC", "Australian Customs");
			ItemSet.OrgListOfContactAllocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newInterestList);

			var getValue = ItemSet.OrgListOfContactAllocations.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("GetValue.Count", dataInExpectedOrder.Length + 1, getValue.Count);

			CombineAssertions(() =>
			{
				var index = dataInExpectedOrder.Length;
				var actualData = getValue[index];
				AssertEquals($"GetValue[{index}].Code", "AUC", actualData.Code);
				AssertEquals($"GetValue[{index}].Description", "Australian Customs", actualData.Description);
			});

			newInterestList.AddPair("AUC", "Australian Customs");
			string error = ItemSet.OrgListOfInterests.GetValidationErrorMessage(newInterestList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has duplicate error", "The code 'AUC' has been duplicated. Please enter a unique code.", error);

			var emptyValueList = new CodeDescriptionPairList();
			emptyValueList.AddPair("", "Australian Customs");
			error = ItemSet.OrgListOfInterests.GetValidationErrorMessage(emptyValueList, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("List has error", "You cannot enter an item with no Code.", error);

			var registryStorageFlags = ItemSet.OrgListOfContactAllocations.Storage;
			AssertEquals("Registry Storage Flags error", RegistryStorageFlags.System, registryStorageFlags);
		}

		public void TestOrgStaffMemberAssignmentRoles()
		{
			var dataType = (CodeDescriptionPairListRegistryDataType)ItemSet.OrgStaffMemberAssignmentRoles.DataType;
			Assert("Org Staff Member Assignment Roles registry item should not allow duplicate codes", !dataType.AllowDuplicateCodes);
		}

		public void TestShowChargeCodeForOtherChargesInHAWBScreen()
		{
			AssertEquals("ShowChargeCodeForOtherChargesInHAWBScreen", ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.Name);
			AssertEquals("Show IATA code field in other charges grid on the HAWB form", ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.Caption);
			AssertEquals("When set up to 'Yes' the IATA code column will be available to select an IATA code in the other charges grid on the HAWB form and only the IATA code description will print on the HAWB.\r\n\r\nWhen set up to 'No' the IATA code column will not be available to select an IATA code in the other charges grid on the HAWB form, the IATA code description will print on the HAWB.", ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.Hint);
			AssertEquals(false, ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.DefaultValue);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.Storage);

			ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.ShowChargeCodeForOtherChargesInHAWBScreen.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowShortGoodsDescriptionOverrideforFHL()
		{
			AssertEquals("AllowShortGoodsDescriptionOverrideforFHL", ItemSet.AllowShortGoodsDescriptionOverrideforFHL.Name);
			AssertEquals("Short Goods Description Override for FHL purposes", ItemSet.AllowShortGoodsDescriptionOverrideforFHL.Caption);
			AssertEquals("Change the default value of this registry if you want to include first 15 characters of short goods description (with fallback to the long description) to the AWB form for FHL/AMS messaging purposes (this field will not be printed on HAWB document).", ItemSet.AllowShortGoodsDescriptionOverrideforFHL.Hint);
			AssertEquals(false, ItemSet.AllowShortGoodsDescriptionOverrideforFHL.DefaultValue);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.AllowShortGoodsDescriptionOverrideforFHL.Storage);

			ItemSet.AllowShortGoodsDescriptionOverrideforFHL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.AllowShortGoodsDescriptionOverrideforFHL.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.AllowShortGoodsDescriptionOverrideforFHL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.AllowShortGoodsDescriptionOverrideforFHL.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCoverPageText()
		{
			ItemSet.CoverPageText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cover Page Text");
			AssertEquals("New Value", "Cover Page Text", ItemSet.CoverPageText.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCoverPageTextOneOff()
		{
			ItemSet.CoverPageTextOneOff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cover Page Text One Off");
			AssertEquals("New Value", "Cover Page Text One Off", ItemSet.CoverPageTextOneOff.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCoverPageTextNew()
		{
			ItemSet.CoverPageTextNew.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cover Page Text New");
			AssertEquals("New Value", "Cover Page Text New", ItemSet.CoverPageTextNew.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCoverPageTextOneOffNew()
		{
			ItemSet.CoverPageTextOneOffNew.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Cover Page Text One Off New");
			AssertEquals("New Value", "Cover Page Text One Off New", ItemSet.CoverPageTextOneOffNew.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestLocalCustomsBranchIdentifier()
		{
			AssertEquals("LocalCustomsBranchIdentifier.DefaultValue", "", ItemSet.LocalCustomsBranchIdentifier.DefaultValue);
			AssertEquals("LocalCustomsBranchIdentifier.DataType.MaxLength", 6, ((StringRegistryDataType)ItemSet.LocalCustomsBranchIdentifier.DataType).MaxLength);

			ItemSet.LocalCustomsBranchIdentifier.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "123456");
			AssertEquals("LocalCustomsBranchIdentifier.Value", "123456", ItemSet.LocalCustomsBranchIdentifier.Value);
		}

		public void TestShowChargesOnBookingsBookingConfirmation()
		{
			AssertEquals(ItemSet.ShowChargesOnBookingsBookingConfirmation.Caption, "Show Charges (Legacy Documents)");
			AssertEquals(ItemSet.ShowChargesOnBookingsBookingConfirmation.Category, "Documents/Booking/Booking Confirmation");
			AssertEquals(ItemSet.ShowChargesOnBookingsBookingConfirmation.DefaultValue, false);

			ItemSet.ShowChargesOnBookingsBookingConfirmation.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, true);
			AssertEquals(ItemSet.ShowChargesOnBookingsBookingConfirmation.Value, true);
		}

		public void TestTACTRateImportPartitionSize()
		{
			AssertEquals(ItemSet.TACTRateImportPartitionSize.Caption, "Partition size for TACT rates import");
			AssertEquals(ItemSet.TACTRateImportPartitionSize.Hint, "Specifies how many lines included in one partition when importing from full TACT rate plain text file. Set smaller partition size to reduce memory usage, but it will take longer time to import.");
			AssertEquals(ItemSet.TACTRateImportPartitionSize.DefaultValue, 50000);

			ItemSet.TACTRateImportPartitionSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40000);
			AssertEquals(ItemSet.TACTRateImportPartitionSize.Value, 40000);
		}

		[ExpectNoExceptions]
		public void TestRawRegistryCanBeCreatedWithoutEnvironmentSetup()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				RawDataRegistry rawRegistry = new RawDataRegistry();
			}
		}

		public void TestDebugBusinessObjectType()
		{
			AssertEquals("DebugBusinessObjectType DefaultValue", "", ItemSet.DebugBusinessObjectType.DefaultValue);
			AssertEquals("DebugBusinessObjectType Options", RegistryOptions.IsOnlyForDevelopers, ItemSet.DebugBusinessObjectType.Options);
			ItemSet.DebugBusinessObjectType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Debug Test String");
			AssertEquals("DebugBusinessObjectType", "Debug Test String", ItemSet.DebugBusinessObjectType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestMailboxDisplayName()
		{
			AssertEquals("Value", EnvProxy.Instance.CurrentCompany.Name, ItemSet.MailboxDisplayName.Value);

			var type = ObjectFactory.GetType("IGlbCompany");
			var factory = new BusinessObjectFactory();
			ICompany company = (ICompany)factory.NewWithValidTestData(type);
			factory.Save();
			AssertEquals("Company", company.Name);
			AssertEquals("Value", "Company", DataRegistry.Instance.RawRegistry.MailboxDisplayName.GetFallBackValueAtAllLevels(company.PK, Guid.Empty, Guid.Empty));

			ItemSet.MailboxDisplayName.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "new value");
			AssertEquals("Value", "new value", ItemSet.MailboxDisplayName.Value);
		}

		public void TestFaxDestinationOverride()
		{
			AssertEquals("System/Testing", ItemSet.FaxDestinationOverride.Category);
			AssertEquals("FaxDestinationOverride", ItemSet.FaxDestinationOverride.Name);
			AssertEquals("Enter a fax number here to force all outbound faxes to be sent to this number. This feature is useful for test systems where administrators want to prevent test faxes being sent out to real customers.", ItemSet.FaxDestinationOverride.Hint);
		}

		public void TestFaxDestinationOverrideForHostedSystem_ProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			Assert("This is visible", itemSet.FaxDestinationOverride.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Support should be able to edit for hosted production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Support should be able to edit for self-hosted production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Non-support user should not not be able access for hosted production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Non-support user should be able to edit for self-hosted production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
			}
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestFaxDestinationOverrideForHostedSystem_NonProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var itemSet = GetNewItemSet();
			Assert("This is visible", itemSet.FaxDestinationOverride.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Support should be able to edit for hosted non-production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Support should be able to edit for self-hosted non-production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Non-support user should be able to edit for hosted non-production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Non-support user should be able to edit for self-hosted non-production system", false, itemSet.FaxDestinationOverride.IsReadOnly);
			}
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestHostedNotificationsEmailOverride()
		{
			var itemSet = GetNewItemSet();
			AssertEquals("Default value for HostedNotificationsEmailOverride", "Hosting.Notifications@wisetechglobal.com", itemSet.HostedNotificationsEmailOverride.DefaultValue);
			Assert("HostedNotificationsEmailOverride should be hidden", !itemSet.HostedNotificationsEmailOverride.IsVisible(CompanyPK, BranchPK, DepartmentPK));
		}

		public void TestEmailDestinationOverride()
		{
			var itemSet = GetNewItemSet();
			AssertEquals("System/Testing", itemSet.EmailDestinationOverride.Category);
			AssertEquals("EmailDestinationOverride", itemSet.EmailDestinationOverride.Name);
			AssertEquals("Enter an email address here to force all emails to be sent to this email address (Fax emails will still be sent out to CargoWise fax server).", itemSet.EmailDestinationOverride.Hint);
		}

		public void TestEnforceChangePasswordAtNextLogin()
		{
			var itemSet = GetNewItemSet();
			AssertEquals("Password Control", itemSet.EnforceChangePasswordAtNextLogin.Category);
			AssertEquals("Enforce 'Change Password At Next Login'", itemSet.EnforceChangePasswordAtNextLogin.Caption);
			AssertEquals("If set to 'Yes', the 'Change Password at Next Login' checkbox can not be unticked until the user changes their password.", itemSet.EnforceChangePasswordAtNextLogin.Hint);
			Assert(!itemSet.EnforceChangePasswordAtNextLogin.DefaultValue);
			AssertEquals(RegistryStorageFlags.System, ItemSet.EnforceChangePasswordAtNextLogin.Storage);
		}

		public void TestEmailDestinationOverrideForHostedSystem_ProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			Assert("This is visible", itemSet.EmailDestinationOverride.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Support should be able to edit for hosted production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Support should be able to edit for self-hosted production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Non-support user should not not be able access for hosted production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Non-support user should be able to edit for self-hosted production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
			}
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestEmailDestinationOverrideForHostedSystem_NonProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var itemSet = GetNewItemSet();
			Assert("This is visible", itemSet.EmailDestinationOverride.IsVisible(CompanyPK, BranchPK, DepartmentPK));
			var originalHostedLocation = EnvProxy.HostedLocation;

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Support should be able to edit for hosted non-production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Support should be able to edit for self-hosted non-production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.PostMasterUserName, Guid.Empty, Guid.Empty))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				AssertEquals("Non-support user should be able to edit for hosted non-production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
				EnvProxy.SetHostedLocationForTest("");
				AssertEquals("Non-support user should be able to edit for self-hosted non-production system", false, itemSet.EmailDestinationOverride.IsReadOnly);
			}
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		#region TestEmailDestinationOverrideMandatoriness

		#region DEBUG build

		public void TestEmailDestinationOverrideMandatoriness_DebugBuild_ProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();

			Assert(itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.IsValueMandatory));
			Assert(itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.MustOverrideDefaultValue));
		}

		public void TestEmailDestinationOverrideMandatoriness_DebugBuild_NonProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var itemSet = GetNewItemSet();

			Assert(itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.IsValueMandatory));
			Assert(itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.MustOverrideDefaultValue));
		}

		#endregion

		#region RELEASE build

		public void TestEmailDestinationOverrideMandatoriness_ReleaseBuild_ProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			var itemSet = GetNewItemSet();
			itemSet.IgnoreIsDebugCheckForTest = true;

			Assert(!itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.IsValueMandatory));
			Assert(!itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.MustOverrideDefaultValue));
		}

		public void TestEmailDestinationOverrideMandatoriness_ReleaseBuild_NonProductionDB()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			var itemSet = GetNewItemSet();
			itemSet.IgnoreIsDebugCheckForTest = true;

			Assert(itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.IsValueMandatory));
			Assert(itemSet.EmailDestinationOverride.Options.HasFlag(RegistryOptions.MustOverrideDefaultValue));
		}

		#endregion

		public void TestSystemEmailDestinationOverride()
		{
			AssertEquals("The default value of SystemEmailDestinationOverride should be true.", true, ItemSet.SystemEmailDestinationOverride.Value);
			ItemSet.SystemEmailDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("The value of SystemEmailDestinationOverride should be able to be set to false.", false, ItemSet.SystemEmailDestinationOverride.Value);
		}

		public void TestIgnoreIsDebugCheckForTestDefaultValue()
		{
			var itemSet = GetNewItemSet();
			Assert(!itemSet.IgnoreIsDebugCheckForTest);
		}

		#endregion

		public void TestEHubTesting()
		{
			AssertEquals("System/Testing", ItemSet.EHubTesting.Category);
			AssertEquals("EHubTesting", ItemSet.EHubTesting.Name);
			AssertEquals("eHub Testing", ItemSet.EHubTesting.Caption);
			AssertEquals("Set this option to 'Yes' to enable eHub Testing.", ItemSet.EHubTesting.Hint);
		}

		public void TestObtainDynamicSTLCollectorDefinitionsFromTheCode_InternalTestSystem()
		{
			AssertObtainDynamicSTLCollectorDefinitionsFromTheCode(true, DatabaseTypes.Codes.Test, true);
		}

		public void TestObtainDynamicSTLCollectorDefinitionsFromTheCode_InternalProdSystem()
		{
			AssertObtainDynamicSTLCollectorDefinitionsFromTheCode(false, DatabaseTypes.Codes.Production, true);
		}

		public void TestObtainDynamicSTLCollectorDefinitionsFromTheCode_NonInternalTestSystem()
		{
			AssertObtainDynamicSTLCollectorDefinitionsFromTheCode(false, DatabaseTypes.Codes.Test, false);
		}

		void AssertObtainDynamicSTLCollectorDefinitionsFromTheCode(bool expectedDefaultValue, string databaseTypeCode, bool isInternalSystem)
		{
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationKeyMock.Setup(_ => _.DatabaseType).Returns(databaseTypeCode);

			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(_ => _.Key).Returns(productRegistrationKeyMock.Object);
			productRegistrationMock.Setup(_ => _.IsWiseTechGlobalInternalSystem()).Returns(isInternalSystem);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				AssertNotNull("RegistryUserUpdateVersion", ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode);
				AssertEquals("Name", "ObtainDynamicSTLCollectorDefinitionsFromTheCode", ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Name);
				AssertEquals("Category", RawDataRegistry.Categories.System_Testing, ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Category);
				AssertEquals("Caption", "Obtain Dynamic STL Collector Definitions From The Code", ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Caption);
				AssertEquals("Hint", "Set this option to 'Yes' on non-production systems to ask the STL service task to obtain the Dynamic STL collector scripts from the code rather than from the reference database.  This should be used when performing UAT on a collector that a developer is in the process of adding to the system.", ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Hint);
				AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Storage);
				AssertEquals("Options", RegistryOptions.IsOnlyForCargoWise, ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.Options);
				AssertEquals("DefaultValue", expectedDefaultValue, ItemSet.ObtainDynamicSTLCollectorDefinitionsFromTheCode.DefaultValue);
			}
		}

		public void TestRegistryUserUpdateVersion()
		{
			AssertNotNull("RegistryUserUpdateVersion", ItemSet.RegistryUserUpdateVersion);
			AssertEquals("Name", RawDataRegistry.RegistryUserUpdateVersionItemName, ItemSet.RegistryUserUpdateVersion.Name);
			AssertEquals("Category", RawDataRegistry.Categories.System_Miscellaneous, ItemSet.RegistryUserUpdateVersion.Category);
			AssertEquals("Caption", "", ItemSet.RegistryUserUpdateVersion.Caption);
			AssertEquals("Hint", "", ItemSet.RegistryUserUpdateVersion.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.RegistryUserUpdateVersion.Storage);
			AssertEquals("Options", RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.NotLogged, ItemSet.RegistryUserUpdateVersion.Options);
			AssertEquals("DafaultValue", 0, ItemSet.RegistryUserUpdateVersion.DefaultValue);
		}

		public void TestExpectedClientDetailRegistryItemIsOnlyForSupport()
		{
			AssertEquals("EXPECTED_CLIENT_DLL should only be for support staff", RegistryOptions.IsOnlyForSupport, ItemSet.ExpectedClientDLL.Options);
		}

		public void TestExpectedClientDetailPreventsModifyingNonEmptyValue()
		{
			using (ItemSet.ExpectedClientDLL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "123"))
			{
				AssertExceptionThrown<RegistryValidationException>("Changing a non-empty value of EXPECTED_CLIENT_DLL from registry form should be prevented",
					"For safety reasons, changing the value of 'Expected Client-Specific DLL' is not allowed if its current value is not empty",
					() => ItemSet.ExpectedClientDLL.DataType.ValidateBeforeRegistryFormSave(ItemSet.ExpectedClientDLL, "456", Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		public void TestPhysicalPOPHostedOption()
		{
			AssertEquals("Default value is", true, ItemSet.AllowHostedClientAccessToEmailSettings.Value);
			AssertEquals("Default tree location", "Physical Server", ItemSet.AllowHostedClientAccessToEmailSettings.Category);
		}

		public void TestDefaultCTOPostCodeAir()
		{
			AssertEquals("Default value is empty", "", ItemSet.DefaultCTOPostCodeAir.Value);
			ItemSet.DefaultCTOPostCodeAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", ItemSet.DefaultCTOPostCodeAir.Value);
		}

		public void TestDefaultCTOPostCodeSea()
		{
			AssertEquals("Default value is empty", "", ItemSet.DefaultCTOPostCodeSea.Value);
			ItemSet.DefaultCTOPostCodeSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", ItemSet.DefaultCTOPostCodeSea.Value);
		}

		public void TestDefaultCTOPostCodeRail()
		{
			AssertEquals("Default value is empty", "", ItemSet.DefaultCTOPostCodeRail.Value);
			ItemSet.DefaultCTOPostCodeRail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", ItemSet.DefaultCTOPostCodeRail.Value);
		}

		public void TestDefaultCFSPostCodeRoad()
		{
			AssertEquals("Default value is empty", "", ItemSet.DefaultCFSPostCodeRoad.Value);
			ItemSet.DefaultCFSPostCodeRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", ItemSet.DefaultCFSPostCodeRoad.Value);
		}

		public void TestUseDistanceCalculcationService()
		{
			AssertEquals("Default value is False", false, ItemSet.UseDistanceCalculcationService.Value);
			ItemSet.UseDistanceCalculcationService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value set to True", true, ItemSet.UseDistanceCalculcationService.Value);
		}

		public void TestShowAvailableBranchesOnly()
		{
			AssertEquals(false, ItemSet.ShowAvailableBranchesOnly.DefaultValue);
			ItemSet.ShowAvailableBranchesOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.ShowAvailableBranchesOnly.Value);
		}

		public void TestShowAvailableDepartmentsOnly()
		{
			AssertEquals("The default value of ShowAvailableDepartmentsOnly should be false.", false, ItemSet.ShowAvailableDepartmentsOnly.Value);
			ItemSet.ShowAvailableDepartmentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("The value of ShowAvailableDepartmentsOnly should be able to be set to true.", true, ItemSet.ShowAvailableDepartmentsOnly.Value);
		}

		public void TestSignOffTextIsMultilingual()
		{
			ItemSet.SignOffText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Later");
			AssertEquals("Later", ItemSet.SignOffText.Value);
			using (Res.TemporarilySwitchLanguage("FRN"))
			using (var mockFrn = Res.UseMockData())
			{
				string key = ((ResourceString)ItemSet.SignOffText.Value).ResourceKey;
				mockFrn.Put(key, new ResourceStringData(key, "A Bientot"));

				AssertEquals("A Bientot", ItemSet.SignOffText.Value);
			}
		}

		public void TestKeepOnlyLatestVersion()
		{
			AssertEquals("DefaultValue", true, ItemSet.KeepOnlyLatestVersion.DefaultValue);
			ItemSet.KeepOnlyLatestVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.KeepOnlyLatestVersion.Value);
			ItemSet.KeepOnlyLatestVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.KeepOnlyLatestVersion.Value);
		}

		public void TestRegistryItemsHaveLockedDownOption()
		{
			Assert(ItemSet.RunSelectTopNAsRowNumberQuery.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));

			Assert(ItemSet.MaxRecommendedNumberOfRecordsToShowInDisplayGrids.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(ItemSet.RunSearchOnEnteringAModule.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(ItemSet.ShowExactRowCountOnExcessResult.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));

			Assert(!ItemSet.MailboxDisplayName.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.MailboxEmailAddress.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.MailboxPassword.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.MailboxUserName.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.MailServerPort.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.MailServer.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.POP3SecureConnectionType.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.IMAPSecureConnectionType.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));

			Assert(!ItemSet.AllowEmailsToBeSentFromUsersAddress.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPDefaultReturnEmailAddress.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPDefaultDoNotReplyEmailAddress.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.MaximumNumberOfMailItemsToSendInABatch.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPPort.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPServer.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPSecureConnection.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPServerTimeout.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPPassword.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
			Assert(!ItemSet.SMTPUsername.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted));
		}

		public void TestRegistryItemsHavePreserveTestValueOption()
		{
			Assert(ItemSet.MailboxEmailAddress.HasOption(RegistryOptions.PreserveTestValue));
			Assert(ItemSet.POP3SecureConnectionType.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TestRegistryItemsHaveIsPasswordVisibleForControllerUserOptionAndKeepSupportOption()
		{
			var supportOption = new RawDataRegistry().GetSupportOnlyOrClientEditableOptionForHostedSystems();
			Assert(ItemSet.SMTPPassword.HasOption(RegistryOptions.IsPasswordVisibleForControllerUser));
			Assert(ItemSet.MailboxPassword.HasOption(RegistryOptions.IsPasswordVisibleForControllerUser));
			Assert(ItemSet.ExcelPasswordForOpening.HasOption(RegistryOptions.IsPasswordVisibleForControllerUser));
			Assert(ItemSet.ExcelPasswordForModifying.HasOption(RegistryOptions.IsPasswordVisibleForControllerUser));

			Assert(ItemSet.SMTPPassword.HasOption(supportOption | RegistryOptions.PreserveTestValue));
			Assert(ItemSet.MailboxPassword.HasOption(supportOption | RegistryOptions.PreserveTestValue));
		}
		public void TestMaximumNumberOfMailItemsToSendInABatch()
		{
			AssertEquals("Default Value", 1000, ItemSet.MaximumNumberOfMailItemsToSendInABatch.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.PhysicalServer_SMTP, ItemSet.MaximumNumberOfMailItemsToSendInABatch.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.MaximumNumberOfMailItemsToSendInABatch.Storage);
			ItemSet.MaximumNumberOfMailItemsToSendInABatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals("Set value", 10, ItemSet.MaximumNumberOfMailItemsToSendInABatch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestMaximumNumberOfRecentItems()
		{
			AssertEquals("Default Value", 30, ItemSet.MaximumNumberOfRecentItems.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.MaximumNumberOfRecentItems.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.MaximumNumberOfRecentItems.Storage);
			ItemSet.MaximumNumberOfRecentItems.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 20);
			AssertEquals("Set value", 20, ItemSet.MaximumNumberOfRecentItems.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestMaximumNumberOfFavorites()
		{
			AssertEquals("Default Value", 12, ItemSet.MaximumNumberOfFavorites.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.MaximumNumberOfFavorites.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.MaximumNumberOfFavorites.Storage);
			ItemSet.MaximumNumberOfFavorites.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 20);
			AssertEquals("Set value", 20, ItemSet.MaximumNumberOfFavorites.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestMaximumNumberOfMyTasks()
		{
			AssertEquals("Default Value", 50, ItemSet.MaximumNumberOfMyTasks.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.MaximumNumberOfMyTasks.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.MaximumNumberOfMyTasks.Storage);
			ItemSet.MaximumNumberOfMyTasks.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 20);
			AssertEquals("Set value", 20, ItemSet.MaximumNumberOfMyTasks.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestOpenModuleInANewWindow()
		{
			Assert("Default Value", !ItemSet.OpenModuleInANewWindow.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.OpenModuleInANewWindow.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.OpenModuleInANewWindow.Storage);
			ItemSet.OpenModuleInANewWindow.SetValue(CompanyPK, Guid.Empty, Guid.Empty, true);
			Assert("Set value", ItemSet.OpenModuleInANewWindow.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestMaximumNumberOfRecentModules()
		{
			AssertEquals("Default Value", 5, ItemSet.MaximumNumberOfRecentModules.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.MaximumNumberOfRecentModules.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.MaximumNumberOfRecentModules.Storage);
			ItemSet.MaximumNumberOfRecentModules.SetValue(CompanyPK, Guid.Empty, Guid.Empty, 20);
			AssertEquals("Set value", 20, ItemSet.MaximumNumberOfRecentModules.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty));
		}

		public void TestFavoriteModules()
		{
			AssertEquals("Empty by default", 0, ItemSet.FavoriteModules.DefaultValue.Length);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.FavoriteModules.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.CompanyDepartment, ItemSet.FavoriteModules.Storage);
			Assert("Options - not cached", ItemSet.FavoriteModules.HasOption(RegistryOptions.NotCached));
			Assert("Options - is hidden", ItemSet.FavoriteModules.HasOption(RegistryOptions.IsHidden));
			AssertEquals("Data type max length", 1000, ItemSet.FavoriteModules.DataType.MaximumLength);
		}

		public void TestRecentModules()
		{
			AssertEquals("Empty by default", 0, ItemSet.RecentModules.DefaultValue.Length);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.RecentModules.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.CompanyDepartment, ItemSet.RecentModules.Storage);
			Assert("Options - not cached", ItemSet.RecentModules.HasOption(RegistryOptions.NotCached));
			Assert("Options - is hidden", ItemSet.RecentModules.HasOption(RegistryOptions.IsHidden));
			AssertEquals("Data type max length", 1000, ItemSet.RecentModules.DataType.MaximumLength);
		}

		public void TestRecentItems()
		{
			AssertEquals("Empty by default", 0, ItemSet.RecentItems.DefaultValue.Length);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, ItemSet.RecentItems.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.CompanyDepartment, ItemSet.RecentItems.Storage);
			Assert("Options - not cached", ItemSet.RecentItems.HasOption(RegistryOptions.NotCached));
			Assert("Options - is hidden", ItemSet.RecentItems.HasOption(RegistryOptions.IsHidden));
			AssertEquals("Data type max length", 1000, ItemSet.RecentItems.DataType.MaximumLength);
		}

		public void TestRecentItemsByModule()
		{
			const string moduleKey = "BlahModule";
			var registryItem = ItemSet.RecentItemsByModule(moduleKey);

			AssertEquals("Empty by default", 0, registryItem.DefaultValue.Length);
			AssertEquals("Category", RawDataRegistry.Categories.System_UI, registryItem.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.CompanyDepartment, registryItem.Storage);
			Assert("Options - not cached", registryItem.HasOption(RegistryOptions.NotCached));
			Assert("Options - is hidden", registryItem.HasOption(RegistryOptions.IsHidden));
			AssertEquals("Data type max length", 1000, registryItem.DataType.MaximumLength);
		}

		public void TestSystemUpgradeWarningPeriod()
		{
			AssertEquals("Default Value", 0, ItemSet.SystemUpgradeWarningPeriod.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.System_Upgrade, ItemSet.SystemUpgradeWarningPeriod.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.SystemUpgradeWarningPeriod.Storage);
			ItemSet.SystemUpgradeWarningPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 500);
			AssertEquals("Set value", 500, ItemSet.SystemUpgradeWarningPeriod.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Min value cannot be less than heartbeat pulse period if value is greater than zero",
				string.Format(CultureInfo.InvariantCulture, "In order to keep the function working, System upgrade warning period cannot be less than current system heartbeat pulse duration: {0}.", ItemSet.HeartbeatpulseDuration),
				ItemSet.SystemUpgradeWarningPeriod.GetValidationErrorMessage((int)ItemSet.HeartbeatpulseDuration - 1, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowCityPostcodeValidation()
		{
			AssertEquals("Default Value", false, ItemSet.AllowCityPostcodeValidation.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Freight_Shipment, ItemSet.AllowCityPostcodeValidation.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.AllowCityPostcodeValidation.Storage);
			AssertEquals("Allow City/Postcode Validation", ItemSet.AllowCityPostcodeValidation.Caption);
			AssertEquals("Set this to \"yes\" to turn on City / Postcode validation when editing / saving an eTail / eManifest address.", ItemSet.AllowCityPostcodeValidation.Hint);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, ItemSet.AllowCityPostcodeValidation.Options);
			Assert("CountryFilterPK", ItemSet.AllowCityPostcodeValidation.CountryFilterPKs.Contains(Constants.CountryGuids.Australia));
		}

		public void TestJobAddressValidation_CityMandatory()
		{
			AssertEquals("Default Value", true, ItemSet.JobAddressValidation_CityMandatory.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Operations_JobAddress, ItemSet.JobAddressValidation_CityMandatory.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.JobAddressValidation_CityMandatory.Storage);
			AssertEquals("City Mandatory", ItemSet.JobAddressValidation_CityMandatory.Caption);
			AssertEquals("Specifies whether the City on an job address is mandatory.", ItemSet.JobAddressValidation_CityMandatory.Hint);
		}

		public void TestJobAddressValidation_CountryMandatory()
		{
			AssertEquals("Default Value", false, ItemSet.JobAddressValidation_CountryMandatory.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Operations_JobAddress, ItemSet.JobAddressValidation_CountryMandatory.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.JobAddressValidation_CountryMandatory.Storage);
			AssertEquals("Country/Region Mandatory", ItemSet.JobAddressValidation_CountryMandatory.Caption);
			AssertEquals("Specifies whether the Country/Region on an job address is mandatory.", ItemSet.JobAddressValidation_CountryMandatory.Hint);
		}

		public void TestJobAddressValidation_UsePostcodeRules()
		{
			AssertEquals("Default Value", true, ItemSet.JobAddressValidation_UsePostcodeRules.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Operations_JobAddress, ItemSet.JobAddressValidation_UsePostcodeRules.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.JobAddressValidation_UsePostcodeRules.Storage);
			AssertEquals("Postcode Validation Rule affects Job Addresses", ItemSet.JobAddressValidation_UsePostcodeRules.Caption);
			AssertEquals("If enabled, postcode validation on job addresses will follow the rule defined on the country/region. If disabled, postcodes will not be mandatory on job addresses.", ItemSet.JobAddressValidation_UsePostcodeRules.Hint);
		}

		public void TestJobAddressValidation_UseStateRules()
		{
			AssertEquals("Default Value", true, ItemSet.JobAddressValidation_UseStateRules.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Operations_JobAddress, ItemSet.JobAddressValidation_UseStateRules.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.JobAddressValidation_UseStateRules.Storage);
			AssertEquals("State/Province Validation Rule affects Job Addresses", ItemSet.JobAddressValidation_UseStateRules.Caption);
			AssertEquals("If enabled, state validation on job addresses will follow the rule defined on the country/region. If disabled, states will not be mandatory on job addresses.", ItemSet.JobAddressValidation_UseStateRules.Hint);
		}

		public void TestJobAddress_ShowUNLOCO()
		{
			AssertEquals("Default Value", false, ItemSet.JobAddress_ShowUNLOCO.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Operations_JobAddress, ItemSet.JobAddress_ShowUNLOCO.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.System, ItemSet.JobAddress_ShowUNLOCO.Storage);
			AssertEquals("Registry Options", RegistryOptions.IsOnlyForSupport, ItemSet.JobAddress_ShowUNLOCO.Options);
			AssertEquals("Show UNLOCO on Job Addresses", ItemSet.JobAddress_ShowUNLOCO.Caption);
			AssertEquals("If enabled, show UNLOCO on job addresses. If disabled, will hide UNLOCO on job addresses.", ItemSet.JobAddress_ShowUNLOCO.Hint);
		}

		public void TestEnableDynamics365Feature()
		{
			AssertEquals("Default Value", false, ItemSet.EnableDynamics365Feature.DefaultValue);
			AssertEquals("Category", RawDataRegistry.Categories.Organizations_DataExchange_Dynamics365, ItemSet.EnableDynamics365Feature.Category);
			AssertEquals("Storage Flags", RegistryStorageFlags.Company, ItemSet.EnableDynamics365Feature.Storage);
			AssertEquals((NoResString)"Enable Dynamics365 Feature", ItemSet.EnableDynamics365Feature.Caption);
			AssertEquals((NoResString)"This registry setting controls the organizations and the relevant country specific information that will be exported to Dynamics 365, based on the login companies that are enabled", ItemSet.EnableDynamics365Feature.Hint);
		}

		public void TestCreditorRequiredFieldsStorage()
		{
			AssertEquals("Creditor Required Fields Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OrgCreditorRequiredFields.Storage);
		}

		public void TestDebtorRequiredFieldsStorage()
		{
			AssertEquals("Debtor Required Fields Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.OrgDebtorRequiredFields.Storage);
		}

		public void TestTempCreditorRequiredFieldsStorage()
		{
			AssertEquals("Temp Creditor Required Fields Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TempOrgCreditorRequiredFields.Storage);
		}

		public void TestTempDebtorRequiredFieldsStorage()
		{
			AssertEquals("Temp Debtor Required Fields Storage Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, ItemSet.TempOrgDebtorRequiredFields.Storage);
		}

		public void TestWebBranchHasADefault()
		{
			AssertNotEquals(Guid.Empty, ItemSet.WebBranch.Value);
		}

		public void TestWebBranchRegistryOptionsIsSetToDefault()
		{
			AssertEquals(RegistryOptions.Default, ItemSet.WebBranch.Options);
		}

		public void TestWebBranchValidation()
		{
			var factory = new BusinessObjectFactory();
			var branchPk = (Guid)ItemSet.WebBranch.Value;
			var branch = factory.Load(ObjectFactory.GetType("IGlbBranch"), branchPk);
			branch[GlbBranchSchema.GB_WebAddress] = string.Empty;
			factory.Save();

			AssertEquals("The branch's company's web address can not be empty. Please press F3, go to the Branch Details tab and enter a value for Web Address",
			ItemSet.WebBranch.GetValidationErrorMessage(branchPk, Guid.Empty, Guid.Empty, Guid.Empty));

			branch[GlbBranchSchema.GB_WebAddress] = "http://a.com";
			factory.Save();

			AssertEquals(string.Empty, ItemSet.WebBranch.GetValidationErrorMessage(branchPk, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestClientDocumentsName()
		{
			Assert(ItemSet.ClientDocumentName.HasOption(RegistryOptions.IsOnlyForSupport));
		}

		public void TestUberFactoryTimeoutPeriod()
		{
			AssertEquals("Default Value should be 10 mins, and yet...", 600, ItemSet.UberFactoryTimeoutPeriod.DefaultValue);
		}

		public void TestDbConnectionTimeoutPeriod()
		{
			var item = ItemSet.DbConnectionTimeoutPeriod;

			AssertEquals(RegistryOptions.IsOnlyForDevelopers, item.Options);
			AssertEquals((double)0, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 400000);
			AssertEquals(400000, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDbConnectionTimeoutPeriod_HeartbeatSet()
		{
			AssertEquals("Default value should be heartbeat/upgrade check time (120s) + 60s if heartbeat is set", 180, ItemSet.DbConnectionTimeoutPeriod.DefaultValue);
		}

		public void TestDbConnectionTimeoutPeriod_HeartbeatNotSet()
		{
			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();
			environmentMock.Setup(x => x.CurrentUser).Returns<IUser>(null);
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			{
				AssertEquals("Default value should be DefaultHeartbeatPulseInMs (120s) + 60s if environment/heartbeat is not set", RawDataRegistry.EntityFrameworkRegistryDefaults.DefaultHeartbeatPulseInMs / 1000 + 60, ItemSet.DbConnectionTimeoutPeriod.DefaultValue);
			}
		}

		public void TestUpgradeObserverMaxWaitInSeconds()
		{
			var item = ItemSet.UpgradeObserverMaxWaitInSeconds;

			AssertEquals(RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals(60, item.DefaultValue);
			AssertEquals((double)-1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)3600, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region TestDatabaseRecoveryModel

		public void TestDatabaseRecoveryModelIsConsistentWithDbRegistry()
		{
			ItemSet.DatabaseRecoveryModel.Inner.DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(ItemSet.DatabaseRecoveryModel.DefaultValue, DbRegistry.DatabaseRecoveryModel.LoadValue(TestConnection));

			ItemSet.DatabaseRecoveryModel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "FULL");
			AssertEquals("FULL", DbRegistry.DatabaseRecoveryModel.LoadValue(TestConnection));

			ItemSet.DatabaseRecoveryModel.Inner.DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);

			DbRegistry.DatabaseRecoveryModel.SaveValue("FULL", TestConnection);
			AssertEquals("FULL", ItemSet.DatabaseRecoveryModel.Value);
		}

		public void TestDatabaseRecoveryModel()
		{
			var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			var savedDbType = keyForTest.DatabaseTypeForTest;
			var savedIsWiseTechGlobalDatabaseServerForTest = DataUtils.IsWiseTechGlobalDatabaseServerForTest;

			const string production = DatabaseTypes.Codes.Production;
			const string nonProduction = DatabaseTypes.Codes.Test;

			try
			{
				AssertEquals("Default Value", "SIMPLE", ItemSet.DatabaseRecoveryModel.DefaultValue);

				// test hidden value
				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: true);
				Assert("The option is hidden for non-production database when the server is hosted by WiseTech", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is cached", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));

				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: production, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is hidden for production database", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is cached", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));

				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: true, isWiseTechHosted: false);
				Assert("The option is hidden when AlwaysOn is enabled", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is cached", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));

				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is not hidden when it is non-production database, AlwaysOn is not enabled and the server is not hosted by WiseTech", !ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));
				SetTestParameters(databaseType: production, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is cached", !ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.IsHidden));

				// test preserved value
				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: true);
				Assert("The option is preserved for tests", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.PreserveTestValue));

				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: production, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is preserved for tests", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.PreserveTestValue));

				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: true, isWiseTechHosted: false);
				Assert("The option is preserved for tests", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.PreserveTestValue));

				ClearDatabaseRecoveryModelFromCache();
				SetTestParameters(databaseType: nonProduction, isAlwaysOn: false, isWiseTechHosted: false);
				Assert("The option is preserved for tests", ItemSet.DatabaseRecoveryModel.HasOption(RegistryOptions.PreserveTestValue));
			}
			finally
			{
				keyForTest.DatabaseTypeForTest = savedDbType;
				Db.Connection.IsAlwaysOnEnabledForTest = null;
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = savedIsWiseTechGlobalDatabaseServerForTest;
			}

			void ClearDatabaseRecoveryModelFromCache()
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("DatabaseRecoveryModel", TimeSpan.Zero);
			}

			void SetTestParameters(string databaseType, bool isAlwaysOn, bool isWiseTechHosted)
			{
				keyForTest.DatabaseTypeForTest = databaseType;
				Db.Connection.IsAlwaysOnEnabledForTest = isAlwaysOn;
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = isWiseTechHosted;
			}
		}

		public void TestTableRowCountDictionary()
		{
			var dict = new Dictionary<string, long>();
			dict.Add("abc", 12);
			dict.Add("def", 13);
			var utcNow = DateTime.UtcNow;

			var data = new TableRowCountDataType();
			data.UtcTime = utcNow;
			data.Dict = dict;

			ItemSet.TableRowCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, data);

			AssertEquals(utcNow.ToString(TableRowCountRegistryDataType.TimeFormat), ItemSet.TableRowCount.Value.UtcTime.ToString(TableRowCountRegistryDataType.TimeFormat));
			AssertEquals(2, ItemSet.TableRowCount.Value.Dict.Count);
			AssertEquals(12, ItemSet.TableRowCount.Value.Dict["abc"]);
			AssertEquals(13, ItemSet.TableRowCount.Value.Dict["def"]);
		}

		#endregion

		public void TestPDFTIFResolution()
		{
			var item = ItemSet.PDFTIFResolution;
			AssertEquals(200, item.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)int.MaxValue, ((IntRegistryDataType)item.DataType).UpperBound);
		}

		readonly ReleaseInfo alpRelease = ReleaseInfo.CreateNewInstanceForTesting("23.1.1.1", DateTime.Today, ReleaseRings.Codes.ALP);
		public void TestDefaultDotNetVersionOnLaunch()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(alpRelease))
			{
				var item = ItemSet.DefaultDotNetVersionOnLaunch;
				AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
				AssertEquals(RegistryStorageFlags.System, item.Storage);
				AssertEquals(DotNetBuildVersionTargetTypeList.Codes.NetFramework48, item.DefaultValue);
				AssertEquals(DotNetBuildVersionTargetTypeList.Codes.NetFramework48, ItemSet.DefaultDotNetVersionOnLaunch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

				ItemSet.DefaultDotNetVersionOnLaunch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DotNetBuildVersionTargetTypeList.Codes.NetCore8);
				AssertEquals(DotNetBuildVersionTargetTypeList.Codes.NetCore8, ItemSet.DefaultDotNetVersionOnLaunch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			}
		}

		public void TestDefaultDotNetVersionOnLaunchShouldHiddenIfNotALP()
		{
			var nonALPList = ReleaseRings.List()
				.Where(x => x.Code != ReleaseRings.Codes.ALP)
				.Select(x => x.Code)
				.ToList();
			foreach (var releaseRing in nonALPList)
			{
				using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.1.1.1", DateTime.Today, releaseRing)))
				{
					var item = ItemSet.DefaultDotNetVersionOnLaunch;
					AssertEquals(RegistryOptions.IsHidden, item.Options);
				}
			}
		}

		public void TestEnableDotNetVersionSwitchMenu()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(alpRelease))
			{
				var item = ItemSet.EnableDotNetVersionSwitchMenu;
				AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
				AssertEquals(RegistryStorageFlags.System, item.Storage);
				AssertEquals(false, item.DefaultValue);
			}
		}

		public void TestEnableDotNetVersionSwitchMenuShouldHiddenIfNotALP()
		{
			var nonALPList = ReleaseRings.List()
				.Select(x => x.Code)
				.Where(x => x != ReleaseRings.Codes.ALP)
				.ToList();
			foreach (var releaseRing in nonALPList)
			{
				using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.1.1.1", DateTime.Today, releaseRing)))
				{
					var item = ItemSet.EnableDotNetVersionSwitchMenu;
					AssertEquals(RegistryOptions.IsHidden, item.Options);
				}
			}
		}

		public void TestRemoteAppCheckDriveMappingTimeoutInSeconds()
		{
			AssertEquals("DefaultValue", 3, ItemSet.RemoteAppCheckDriveMappingTimeoutInSeconds.DefaultValue);
			AssertEquals("Category", "System/RemoteApp", ItemSet.RemoteAppCheckDriveMappingTimeoutInSeconds.Category);
			AssertEquals("StorageFlags", RegistryStorageFlags.System, ItemSet.RemoteAppCheckDriveMappingTimeoutInSeconds.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForController, ItemSet.RemoteAppCheckDriveMappingTimeoutInSeconds.Options);

			var dataType = (IntRegistryDataType)ItemSet.RemoteAppCheckDriveMappingTimeoutInSeconds.DataType;
			AssertEquals("LowerBound", 1, (int)dataType.LowerBound);
			AssertEquals("UpperBound", 20, (int)dataType.UpperBound);
		}
		public void TestSelectNDRPath()
		{
			using (ItemSet.AllowEmailsToBeSentFromUsersAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var item = ItemSet.SelectNDRPath;
				AssertEquals(Constants.SelectNDRPath.Codes.MB, item.DefaultValue);
				AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.SelectNDRPath.Codes.CP);
				AssertEquals(Constants.SelectNDRPath.Codes.CP, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals("SelectNDRPath", item.Name);
				AssertEquals(Categories.PhysicalServer_SMTP, item.Category);
				AssertEquals(RegistryOptions.Default, item.Options);
				AssertEquals("Caption", "Select NDR Path", item.Caption);
				AssertEquals("Hint", "This setting will set the Return-Path for NDR (Non-Delivery Receipt) notifications.\r\n\r\nNote:  This setting can only be configured when the Allow Emails To Be Sent From User's Address Registry setting is enabled.", item.Hint);
			}
			using (ItemSet.AllowEmailsToBeSentFromUsersAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsReadOnly, ItemSet.SelectNDRPath.Options);
			}
		}

		public void TestAddDatabaseInfoToEdientUrls()
		{
			var item = ItemSet.AddDatabaseInfoToEdientUrls;
			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(false, item.DefaultValue);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, item.Value);
		}

		readonly Guid CompanyPK = Guid.NewGuid();
		readonly Guid BranchPK = Guid.NewGuid();
		readonly Guid DepartmentPK = Guid.NewGuid();

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "PrintAsAgreedOnFirstSetMAWB";
				yield return "PrintAsAgreedOnSecondSetMAWB";
				yield return "EnableConsolidatedEntries";
				yield return nameof(RawDataRegistry.NotifyPartyDefaultText);

				foreach (var registryItem in base.ConditionallyVisibleRegistryItems)
				{
					yield return registryItem;
				}
			}
		}

		RegistryOptions GetSupportOnlyOrClientEditableOptionForHostedSystems()
		{
			var registry = new RawDataRegistry();

			if ((bool)registry.AllowHostedClientAccessToEmailSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				return RegistryOptions.Default;
			}
			else
			{
				return RegistryOptions.IsOnlyEditableBySupportIfHosted;
			}
		}

		public void TestNotifyPartyDefaultText()
		{
			using (Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Instance.ProductivityWiseModeEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsHidden, Instance.NotifyPartyDefaultText.Options);
			}

			RegistryItemDictionary.Instance.PurgeAll();

			using (Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Instance.ProductivityWiseModeEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, Instance.NotifyPartyDefaultText.Options);
			}

			RegistryItemDictionary.Instance.PurgeAll();

			using (Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Instance.ProductivityWiseModeEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.Default, Instance.NotifyPartyDefaultText.Options);
			}

			RegistryItemDictionary.Instance.PurgeAll();

			using (Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Instance.ProductivityWiseModeEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsHidden, Instance.NotifyPartyDefaultText.Options);
			}
		}
	}
}
