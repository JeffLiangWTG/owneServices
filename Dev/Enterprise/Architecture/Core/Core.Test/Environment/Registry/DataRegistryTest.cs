using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	class DataRegistryTest : TransactionedTestCase
	{
		public void TestScimAudienceId()
		{
			AssertEquals("8adf8e6e-67b2-4cf2-a259-e3dc5476c621", Registry.ScimAudienceId);
			RawDataRegistry.Instance.ScimAudienceId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "piu piu");
			AssertEquals("piu piu", Registry.ScimAudienceId);
		}

		public void TestScimKnownEndpointPath()
		{
			AssertEquals("/.well-known/openid-configuration", Registry.ScimKnownEndpointPath);
			RawDataRegistry.Instance.ScimKnownEndpointPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123 123");
			AssertEquals("123 123", Registry.ScimKnownEndpointPath);
		}

		public void TestScimIssuer()
		{
			AssertEquals("", Registry.ScimIssuer);
			RawDataRegistry.Instance.ScimIssuer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "zoom");
			AssertEquals("zoom", Registry.ScimIssuer);
		}

		public void TestScimLoggingKafkaBrokers()
		{
			AssertEquals("106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,108-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,109-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,110-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044"
				, Registry.ScimLoggingKafkaBrokers);
			RawDataRegistry.Instance.ScimLoggingKafkaBrokers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "broker1");
			AssertEquals("broker1", Registry.ScimLoggingKafkaBrokers);
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
			AssertEquals("Should have default value", expectedTopic, Registry.ScimLoggingKafkaTopic);

			Registry.ScimLoggingKafkaTopic = "modified_topic_value";
			AssertEquals("Value should be changed to ", "modified_topic_value", Registry.ScimLoggingKafkaTopic);
		}

		public void TestScimLoggingKafkaTopicUsername_ProductionSystem() => ScimLoggingKafkaTopicUsername(true);
		public void TestScimLoggingKafkaTopicUsername_NonProductionSystem() => ScimLoggingKafkaTopicUsername(false);

		public void ScimLoggingKafkaTopicUsername(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);
			AssertEquals(isProductionSystem, EnvProxy.Instance.IsProductionSystem);

			string expectedUsername = isProductionSystem
							? "topic-au2-prod-scim-service-logs-prod"
							: "topic-au1-test-scim-service-logs-test";
			AssertEquals("Should have default value", expectedUsername, Registry.ScimLoggingKafkaTopicUsername);

			Registry.ScimLoggingKafkaTopicUsername = "modified_topic_username_value";
			AssertEquals("Value should be changed to ", "modified_topic_username_value", Registry.ScimLoggingKafkaTopicUsername);
		}

		public void TestScimLoggingKafkaTopicPassword()
		{
			AssertEquals("", Registry.ScimLoggingKafkaTopicPassword);
			RawDataRegistry.Instance.ScimLoggingKafkaTopicPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pass1");
			AssertEquals("pass1", Registry.ScimLoggingKafkaTopicPassword);
		}

		public void TestProductivityWiseModeEnabled()
		{
			AssertEquals(false, Registry.ProductivityWiseModeEnabled);

			RawDataRegistry.Instance.ProductivityWiseModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(true, Registry.ProductivityWiseModeEnabled);
		}

		[ExpectNoExceptions]
		public void TestGetQuotationDocumentLogo()
		{
			Registry.GetQuotationDocumentLogo(Guid.NewGuid(), Guid.NewGuid());
		}

		public void TestEnableEDIMessageInterpreter()
		{
			Registry.RawRegistry.EnableEDIMessageInterpreter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.EnableEDIMessageInterpreter);
			Registry.RawRegistry.EnableEDIMessageInterpreter.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.EnableEDIMessageInterpreter);
			Registry.EnableEDIMessageInterpreter = true;
			AssertEquals(true, Registry.RawRegistry.EnableEDIMessageInterpreter.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOperationalActionsRecordBatchSize()
		{
			AssertEquals(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION, Registry.RawRegistry.OperationalActionsRecordBatchSize.DefaultValue);
			Assert(Registry.RawRegistry.OperationalActionsRecordBatchSize.HasOption(RegistryOptions.IsOnlyForController));
		}

		public void TestOpenInMicrosoftOffice365FileTypeListValidation()
		{
			AssertExceptionThrown<RegistryValidationException>(
				"Should throw RegistryValidationException if MicrosoftOffice365ApplicationIdForDragDrop is not set",
				"In order to open eDocs in Microsoft Office 365, please set [Microsoft Office 365 Application ID For Drag and Drop] and [Microsoft Office 365 Tenant ID For Drag and Drop] first.",
				() => Registry.OpenInMicrosoftOffice365FileTypeList = new string[] { "txt" });
			Registry.MicrosoftOffice365ApplicationIdForDragDrop = "41EE13BB-D8C9-4596-B256-9E734C1A8E43";
			AssertNoExceptionThrown(() => Registry.OpenInMicrosoftOffice365FileTypeList = new string[] { "txt" });
			AssertEquals("txt", string.Join("", Registry.OpenInMicrosoftOffice365FileTypeList));
		}

		public void TestMicrosoftOffice365AttachmentEmailFetchLimit()
		{
			//Act
			Registry.RawRegistry.MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 700);

			//Assert
			AssertEquals("Value should be changed to ", 700, Registry.MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop);
		}

		public void TestRemoteAppEnableDragDropLite()
		{
			AssertEquals("DisableDragDropLiteMessage", false, Registry.RemoteAppEnableDragDropLite);
			Registry.RemoteAppEnableDragDropLite = true;
			AssertEquals("EnableDragDropLiteMessage", true, Registry.RemoteAppEnableDragDropLite);
		}

		public void TestRemoteAppAlwaysShowSelectionFormOnFileDrop()
		{
			AssertEquals("DoNotShowSelectionFormOnEveryFileDrop", false, Registry.RemoteAppAlwaysShowSelectionFormOnFileDrop);
			Registry.RemoteAppAlwaysShowSelectionFormOnFileDrop = true;
			AssertEquals("AlwaysShowSelectionFormOnFileDrop", true, Registry.RemoteAppAlwaysShowSelectionFormOnFileDrop);
		}

		public void TestRemoteAppSendTestingMessageOnInitialize()
		{
			AssertEquals("SendTestingMessageOnInitialize", true, Registry.RemoteAppSendTestingMessageOnInitialize);
			Registry.RemoteAppSendTestingMessageOnInitialize = false;
			AssertEquals("DoNotSendTestingMessageOnInitialize", false, Registry.RemoteAppSendTestingMessageOnInitialize);
		}

		public void TestRemoteAppShowFormViaMenuDelayMilliseconds()
		{
			AssertEquals("Should have default value", 600, Registry.RemoteAppShowFormViaMenuDelayMilliseconds);

			Registry.RemoteAppShowFormViaMenuDelayMilliseconds = 700;
			AssertEquals("Value should be changed to ", 700, Registry.RemoteAppShowFormViaMenuDelayMilliseconds);
		}

		public void TestRemoteAppWaitingForReconnectionTimeoutInSeconds()
		{
			AssertEquals("Should have default value", 30, Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds);

			Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = 5;
			AssertEquals("Value should be changed to ", 5, Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds);

			AssertExceptionThrown<RegistryValidationException>(() => Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds = 1);
		}

		public void TestRemoteAppCheckDriveMappingTimeoutInSeconds()
		{
			AssertEquals(
				"Should have default value",
				TimeSpan.FromSeconds(3d),
				Registry.RemoteAppCheckDriveMappingTimeoutInSeconds);
		}

		public void TestRemoteAppAllowEDocAccessWithoutConnectorMode_HostedWithCW()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.HostedLocationForTest = "SYD";

			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ConnectorOrServer);
			AssertEquals(RemoteConnectingModes.ConnectorOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ConnectorOnly);
			AssertEquals(RemoteConnectingModes.ConnectorOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ServerOnly);
			AssertEquals(RemoteConnectingModes.ConnectorOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
		}

		public void TestRemoteAppAllowEDocAccessWithoutConnectorMode_NotHostedWithCW()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.HostedLocationForTest = "NCW";

			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ConnectorOrServer);
			AssertEquals(RemoteConnectingModes.ConnectorOrServer, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ConnectorOnly);
			AssertEquals(RemoteConnectingModes.ConnectorOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RemoteConnectingModes.ServerOnly);
			AssertEquals(RemoteConnectingModes.ServerOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
		}

		public void TestRemoteAppAllowEDocAccessWithoutConnectorModeCompatibleWithLegacy()
		{
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENB");
			AssertEquals(RemoteConnectingModes.ConnectorOrServer, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DISB");
			AssertEquals(RemoteConnectingModes.ConnectorOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
			Registry.RawRegistry.RemoteAppAllowEDocAccessWithoutConnectorMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SKIP");
			AssertEquals(RemoteConnectingModes.ServerOnly, Registry.RemoteAppAllowEDocAccessWithoutConnectorMode);
		}

		public void TestEnglishSpellingForCompany()
		{
			var factory = new BusinessObjectFactory();
			var company = (ICompany)factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			Registry.RawRegistry.EnglishSpelling.SetValue(company.PK, Guid.Empty, Guid.Empty, Enterprise.Core.SharedConstants.Languages.EnglishBritish);

			AssertEquals(Enterprise.Core.SharedConstants.Languages.EnglishBritish, Registry.EnglishSpellingForCompany(company));

			Registry.RawRegistry.EnglishSpelling.SetValue(company.PK, Guid.Empty, Guid.Empty, Enterprise.Core.SharedConstants.Languages.EnglishAmerican);

			AssertEquals(Enterprise.Core.SharedConstants.Languages.EnglishAmerican, Registry.EnglishSpellingForCompany(company));
		}

		public void TestShowSystemGeneratedContactsOnOrgDocuments()
		{
			Registry.RawRegistry.ShowSystemGeneratedContactsOnOrgDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.ShowSystemGeneratedContactsOnOrgDocuments);
			Registry.RawRegistry.ShowSystemGeneratedContactsOnOrgDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.ShowSystemGeneratedContactsOnOrgDocuments);

			Registry.ShowSystemGeneratedContactsOnOrgDocuments = true;
			AssertEquals(true, Registry.RawRegistry.ShowSystemGeneratedContactsOnOrgDocuments.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Registry.ShowSystemGeneratedContactsOnOrgDocuments = false;
			AssertEquals(false, Registry.RawRegistry.ShowSystemGeneratedContactsOnOrgDocuments.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestTrainingModeEnabled()
		{
			Registry.TraningModeEnabled = true;
			AssertEquals(true, Registry.TraningModeEnabled);
			Registry.TraningModeEnabled = false;
			AssertEquals(false, Registry.TraningModeEnabled);
		}

		public void TestDisplayUtcOffset()
		{
			AssertEquals("Default value should be true.", true, Registry.DisplayUtcOffset);
			Registry.DisplayUtcOffset = false;
			AssertEquals(false, Registry.DisplayUtcOffset);
			Registry.DisplayUtcOffset = true;
			AssertEquals(true, Registry.DisplayUtcOffset);
		}

		public void TestInternalApplicationActivityTrackingInterval()
		{
			var item = Registry.RawRegistry.InternalApplicationActivityTrackingInterval;

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 450);
			AssertEquals(450, Registry.InternalApplicationActivityTrackingInterval);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 600);
			AssertEquals(600, Registry.InternalApplicationActivityTrackingInterval);

			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 450);
			AssertEquals(450, Registry.InternalApplicationActivityTrackingInterval);

			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 600);
			AssertEquals(600, Registry.InternalApplicationActivityTrackingInterval);
		}

		public void TestDisplayGMTOffsetOnF5UserInfo()
		{
			Assert(Registry.DisplayGMTOffsetOnF5UserInfo);
			Registry.DisplayGMTOffsetOnF5UserInfo = false;
			Assert(!Registry.DisplayGMTOffsetOnF5UserInfo);
			Registry.DisplayGMTOffsetOnF5UserInfo = true;
			Assert(Registry.DisplayGMTOffsetOnF5UserInfo);
		}

		public void TestShowDialogForReopeningFormsLeftBeforeRestart()
		{
			Assert(!Registry.RememberOpenedFormsAfterUpgrade);
			Registry.RememberOpenedFormsAfterUpgrade = true;
			Assert(Registry.RememberOpenedFormsAfterUpgrade);
			Registry.RememberOpenedFormsAfterUpgrade = false;
			Assert(!Registry.RememberOpenedFormsAfterUpgrade);
		}

		public void TestShowSaveProgressBox()
		{
			AssertEquals(true, Registry.ShowSaveProgressBox);
			Registry.ShowSaveProgressBox = false;
			AssertEquals(false, Registry.ShowSaveProgressBox);
			Registry.ShowSaveProgressBox = true;
			AssertEquals(true, Registry.ShowSaveProgressBox);
		}

		public void TestShowCodeAtCompanyAndBranchName()
		{
			AssertEquals(false, Registry.ShowCodeAtCompanyAndBranchName);
			Registry.ShowCodeAtCompanyAndBranchName = true;
			AssertEquals(true, Registry.ShowCodeAtCompanyAndBranchName);
			Registry.ShowCodeAtCompanyAndBranchName = false;
			AssertEquals(false, Registry.ShowCodeAtCompanyAndBranchName);
		}

		public void TestLightValidationEnabled()
		{
			Registry.LightValidationEnabled = false;
			AssertEquals(false, Registry.LightValidationEnabled);
			Registry.LightValidationEnabled = true;
			AssertEquals(true, Registry.LightValidationEnabled);
		}

		public void TestReferenceFiles()
		{
			AssertNotNull(Registry.ReferenceFiles);
			AssertEquals(typeof(ReferenceFilesRegistry), Registry.ReferenceFiles.GetType());
		}

		public void TestNativeXMLSupportTillDate()
		{
			AssertEquals("Should be Minimum date by default to disable Native XML", DateTime.MinValue, Registry.NativeXMLSupportTillDate);
			AssertEquals(false, Registry.IsNativeXMLSupported);
			var dateValue = ZDateTime.UtcNow.Date.AddMonths(2).ToDateTime();
			Registry.RawRegistry.NativeXMLSupportTillDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue);
			AssertEquals(dateValue, Registry.NativeXMLSupportTillDate);
			AssertEquals(true, Registry.IsNativeXMLSupported);
			AssertExceptionThrown(typeof(RegistryValidationException), "Date cannot be greater than 3 months in the future", () => Registry.RawRegistry.NativeXMLSupportTillDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.Date.AddMonths(4).ToDateTime()));
			Registry.NativeXMLSupportTillDate = dateValue = ZDateTime.UtcNow.Date.AddMonths(-2).ToDateTime();
			AssertEquals(false, Registry.IsNativeXMLSupported);
		}

		public void TestStaffDetailsUpdateFrequency()
		{
			Registry.RawRegistry.StaffDetailsUpdateFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			AssertEquals("Staff Details Update Frequency", 1, Registry.StaffDetailsUpdateFrequency);
		}

		public void TestUserContextErrorEmailTimestampsWithEmptyValue()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => Registry.UserContextErrorEmailTimestamps = null);
			Registry.UserContextErrorEmailTimestamps = new Dictionary<string, ZDateTime>(StringComparer.OrdinalIgnoreCase);
			AssertEquals(0, Registry.UserContextErrorEmailTimestamps.Count);
		}

		public void TestUserContextErrorEmailTimestampsWithSingleValue()
		{
			Registry.UserContextErrorEmailTimestamps = new Dictionary<string, ZDateTime>(StringComparer.OrdinalIgnoreCase) { { "abc", new ZDateTime(2000, 1, 1) } };
			AssertEquals(1, Registry.UserContextErrorEmailTimestamps.Count);
			Assert(Registry.UserContextErrorEmailTimestamps.ContainsKey("abc"));
			AssertEquals(2000, Registry.UserContextErrorEmailTimestamps["abc"].Year);
			AssertEquals(1, Registry.UserContextErrorEmailTimestamps["abc"].Month);
			AssertEquals(1, Registry.UserContextErrorEmailTimestamps["abc"].Day);
		}

		public void TestUserContextErrorEmailTimestampsWithMultipleValues()
		{
			Registry.UserContextErrorEmailTimestamps = new Dictionary<string, ZDateTime>(StringComparer.OrdinalIgnoreCase) { { "user1", new ZDateTime(2000, 1, 1) }, { "user2", new ZDateTime(2012, 12, 12) } };
			var userContextErrorEmailTimestamps = Registry.UserContextErrorEmailTimestamps;
			AssertEquals(2, userContextErrorEmailTimestamps.Count);
			Assert(userContextErrorEmailTimestamps.ContainsKey("user1"));
			Assert(userContextErrorEmailTimestamps.ContainsKey("user2"));
			AssertEquals(2000, userContextErrorEmailTimestamps["user1"].Year);
			AssertEquals(1, userContextErrorEmailTimestamps["user1"].Month);
			AssertEquals(1, userContextErrorEmailTimestamps["user1"].Day);
			AssertEquals(2012, userContextErrorEmailTimestamps["user2"].Year);
			AssertEquals(12, userContextErrorEmailTimestamps["user2"].Month);
			AssertEquals(12, userContextErrorEmailTimestamps["user2"].Day);
		}

		public void TestUserContextErrorEmailTimestampsByAddingValueWithTheSameKeyButInDifferentCases()
		{
			Registry.UserContextErrorEmailTimestamps = new Dictionary<string, ZDateTime>(StringComparer.OrdinalIgnoreCase) { { "user1", new ZDateTime(2000, 1, 1) } };
			var userContextErrorEmailTimestamps = Registry.UserContextErrorEmailTimestamps;
			userContextErrorEmailTimestamps["USER1"] = new ZDateTime(2012, 12, 12);
			userContextErrorEmailTimestamps["User2"] = new ZDateTime(2013, 1, 15);
			Registry.UserContextErrorEmailTimestamps = userContextErrorEmailTimestamps;
			AssertEquals(2, userContextErrorEmailTimestamps.Count);
			Assert(userContextErrorEmailTimestamps.ContainsKey("user1"));
			AssertEquals(2012, userContextErrorEmailTimestamps["user1"].Year);
			AssertEquals(12, userContextErrorEmailTimestamps["user1"].Month);
			AssertEquals(12, userContextErrorEmailTimestamps["user1"].Day);
			Assert(userContextErrorEmailTimestamps.ContainsKey("user2"));
			AssertEquals(2013, userContextErrorEmailTimestamps["user2"].Year);
			AssertEquals(1, userContextErrorEmailTimestamps["user2"].Month);
			AssertEquals(15, userContextErrorEmailTimestamps["user2"].Day);
		}

		public void TestUserContextErrorEmailInterval()
		{
			AssertEquals(24, Registry.UserContextErrorEmailInterval);
			Registry.UserContextErrorEmailInterval++;
			AssertEquals(25, Registry.UserContextErrorEmailInterval);
		}

		public void TestServiceTaskHeartbeatDurationSeconds()
		{
			AssertEquals(900, Registry.ServiceTaskHeartbeatDurationSeconds);
			Registry.ServiceTaskHeartbeatDurationSeconds = 60;
			AssertEquals(60, Registry.ServiceTaskHeartbeatDurationSeconds);
		}

		public void TestDeliverReportsInBackground()
		{
			Registry.RawRegistry.DeliverReportsInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("DeliverReportsInBackground", false, Registry.DeliverReportsInBackground);
		}

		public void TestPhysicalServerID()
		{
			AssertEquals("PhysicalServerID", "DAT", Registry.PhysicalServerID);
		}

		public void TestLegacyEncryptedSystemRegistrationKey()
		{
			AssertNotNull("LegacyEncryptedSystemRegistrationKey", Registry.LegacyEncryptedSystemRegistrationKey);
		}

		public void TestAllowHostedClientAccessToEmailSettingsHasValue()
		{
			AssertNotNull("AllowHostedClientAccessToEmailSettings", Registry.AllowHostedClientAccessToEmailSettings);
		}

		public void TestSMTPServer()
		{
			AssertNotNull("SMTPServer", Registry.SMTPServer);
		}

		public void TestSMTPPort()
		{
			Assert("SMTPPort", Registry.SMTPPort >= 0);
		}

		public void TestMailServer()
		{
			Assert("MailServer", Registry.MailServer.Length >= 0);
		}

		public void TestMailServerPort()
		{
			Assert("MaiServerPort", Registry.MailServerPort >= 0);
		}

		public void TestShipmentScreenLayout()
		{
			AssertNotNullOrEmpty("ShipmentScreenLayout", Registry.ShipmentScreenLayout);
		}

		public void TestServiceLevel()
		{
			Assert("ServiceLevel", Registry.ServiceLevel != Guid.Empty);
		}

		public void TestFreightChargeCode()
		{
			Assert("FreightChargeCode", Registry.FreightChargeCode != Guid.Empty);
		}

		public void TestCommodityCode()
		{
			Assert("CommodityCode", Registry.CommodityCode != Guid.Empty);
		}

		public void TestMailboxUserName()
		{
			Assert("MailboxUserName", Registry.MailboxUserName.Length >= 0);
		}

		public void TestMailboxDisplayName()
		{
			AssertEquals("MailboxDisplayName", EnvProxy.Instance.CurrentCompany.Name, EnvProxy.Instance.Registry.MailboxDisplayName);
		}

		public void TestMailboxPassword()
		{
			Assert("MailboxPassword", Registry.MailboxPassword.Length >= 0);
		}

		public void TestAUCCompanyCertificateData()
		{
			Registry.AUCCompanyCertificateData = new byte[4] { 0, 1, 2, 3 };
			AssertEquals(new byte[4] { 0, 1, 2, 3 }, Registry.AUCCompanyCertificateData);
		}

		[TestDate]
		public void TestAUCCompanyCertificatePassword()
		{
			var certificatesHelper = ObjectFactory.New<Customs.AU.ICertificateManagerHelper>(new BusinessObjectFactory());
			certificatesHelper.SetupValidCompanyCertificatesForTest();

			AssertEquals("AUCCompanyCertificatePassword", certificatesHelper.AUCCompanyCertificatePasswordForTest, Registry.AUCCompanyCertificatePassword);
		}

		[ExpectNoExceptions]
		public void TestCommercialInvoiceLineMergeMethod()
		{
			string x = Registry.CommercialInvoiceLineMergeMethod;
		}

		public void TestLandedCostingFallbackExRatesToJobInvoicing()
		{
			Registry.RawRegistry.LandedCostingFallbackExRatesToJobInvoicing.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.LandedCostingFallbackExRatesToJobInvoicing);
			Registry.RawRegistry.LandedCostingFallbackExRatesToJobInvoicing.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.LandedCostingFallbackExRatesToJobInvoicing);
		}

		public void TestAutoPopulateHAWBsOnConsol()
		{
			Registry.RawRegistry.AutoPopulateHAWBsOnConsol.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.AutoPopulateHAWBsOnConsol);
			Registry.RawRegistry.AutoPopulateHAWBsOnConsol.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.AutoPopulateHAWBsOnConsol);
		}

		[ExpectNoExceptions]
		public void TestediTariffInstallationDirectory()
		{
			string x = Registry.ediTariffInstallationDirectory;
		}

		public void TestExternalBorderComplianceTool()
		{
			AssertEquals(ExternalBorderComplianceToolList.Codes.BorderWiseWeb, Registry.ExternalBorderComplianceTool);

			RawDataRegistry.Instance.ExternalBorderComplianceTool.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ExternalBorderComplianceToolList.Codes.BorderWiseWeb);
			AssertEquals(ExternalBorderComplianceToolList.Codes.BorderWiseWeb, Registry.ExternalBorderComplianceTool);

			RawDataRegistry.Instance.ExternalBorderComplianceTool.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExternalBorderComplianceToolList.Codes.BorderWiseWeb);
			AssertEquals(ExternalBorderComplianceToolList.Codes.BorderWiseWeb, Registry.ExternalBorderComplianceTool);
		}

		public void TestDatabaseSchemaVersion()
		{
			Assert("DatabaseMajorSchemaVersion", Registry.DatabaseMajorSchemaVersion >= 0);
			Assert("DatabaseMinorSchemaVersion", Registry.DatabaseMinorSchemaVersion >= 0);
		}

		public void TestDatabaseScriptVersion()
		{
			Assert("DatabaseMajorScriptVersion", Registry.DatabaseMajorScriptVersion >= 0);
			Assert("DatabaseMinorScriptVersion", Registry.DatabaseMinorScriptVersion >= 0);
		}

		public void TestTransformationVersion()
		{
			Assert("MajorTransformationVersion", Registry.DatabaseMajorTransformationVersion >= 0);
			Assert("MinorTransformationVersion", Registry.DatabaseMinorTransformationVersion >= 0);
		}

		public void TestDatabaseSystemDataVersion()
		{
			Registry.DatabaseSystemDataVersionMajor = 12;
			AssertEquals("DatabaseSystemDataVersionMajor", 12, Registry.DatabaseSystemDataVersionMajor);
			Registry.DatabaseSystemDataVersionMinor = 888;
			AssertEquals("DatabaseSystemDataVersionMinor", 888, Registry.DatabaseSystemDataVersionMinor);
		}

		public void TestDatabaseClrAssembliesVersion()
		{
			Registry.DatabaseMajorClrAssembliesVersion = 12;
			AssertEquals("DatabaseMajorClrAssembliesVersion", 12, Registry.DatabaseMajorClrAssembliesVersion);
			Registry.DatabaseMinorClrAssembliesVersion = 888;
			AssertEquals("DatabaseMinorClrAssembliesVersion", 888, Registry.DatabaseMinorClrAssembliesVersion);
		}

		public void TestOnlineTransformationStatus()
		{
			Registry.OnlineTransformationStatus = "Xyz";
			AssertEquals("testOnlineTransformationStatus", "Xyz", Registry.OnlineTransformationStatus);
		}

		public void TestDotNetCheckWhitelist()
		{
			EnvProxy.Instance.Registry.DotNetPreUpgradeCheckWhitelist = "PC1,PC2";
			AssertEquals("PC1,PC2", EnvProxy.Instance.Registry.DotNetPreUpgradeCheckWhitelist);
			EnvProxy.Instance.Registry.DotNetPreUpgradeCheckWhitelist = null;
			AssertNull(EnvProxy.Instance.Registry.DotNetPreUpgradeCheckWhitelist);
		}

		public void TestEnableDotNetPreUpgradeCheck()
		{
			AssertEquals("EnableDotNetPreUpgradeCheck", true, Registry.EnableDotNetPreUpgradeCheck);
			Registry.EnableDotNetPreUpgradeCheck = false;
			AssertEquals("EnableDotNetPreUpgradeCheck", false, Registry.EnableDotNetPreUpgradeCheck);
		}

		public void TestExpectedClientDLLVersion()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientFoo";
			AssertEquals("ZClientFoo", EnvProxy.Instance.Registry.ExpectedClientDLL);
			EnvProxy.Instance.Registry.ExpectedClientDLL = null;
			AssertNull(EnvProxy.Instance.Registry.ExpectedClientDLL);
		}

		public void TestShowExactRowCountOnExcessResult()
		{
			AssertEquals("ShowExactRowCountOnExcessResult", false, Registry.ShowExactRowCountOnExcessResult);
			Registry.ShowExactRowCountOnExcessResult = true;
			AssertEquals("ShowExactRowCountOnExcessResult", true, Registry.ShowExactRowCountOnExcessResult);
		}

		public void TestNotificationGroup()
		{
			AssertNotNull("NotificationGroup", Registry.NotificationGroup(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
		}

		public void TestOrgAllowMixedCase()
		{
			AssertEquals("OrgAllowMixedCase", false, Registry.OrgAllowMixedCase);
			Registry.SetOrgAllowMixedCase(true);
			AssertEquals("OrgAllowMixedCase", true, Registry.OrgAllowMixedCase);
			Registry.SetOrgAllowMixedCase(false);
			AssertEquals("OrgAllowMixedCase", false, Registry.OrgAllowMixedCase);
		}

		public void TestOrgUsePhoneNumberFormatting()
		{
			AssertEquals("OrgUsePhoneNumberFormatting", false, Registry.OrgUsePhoneNumberFormatting);
			Registry.SetOrgUsePhoneNumberFormatting(true);
			AssertEquals("OrgUsePhoneNumberFormatting", true, Registry.OrgUsePhoneNumberFormatting);
			Registry.SetOrgUsePhoneNumberFormatting(false);
			AssertEquals("OrgUsePhoneNumberFormatting", false, Registry.OrgUsePhoneNumberFormatting);
		}

		public void TestAllowInvalidPhoneNumbers()
		{
			AssertEquals("AllowInvalidPhoneNumbers", false, Registry.DowngradeInvalidPhoneNumbersToAWarning);
			Registry.SetDowngradeInvalidPhoneNumbersToAWarning(true);
			AssertEquals("AllowInvalidPhoneNumbers", true, Registry.DowngradeInvalidPhoneNumbersToAWarning);
			Registry.SetDowngradeInvalidPhoneNumbersToAWarning(false);
			AssertEquals("AllowInvalidPhoneNumbers", false, Registry.DowngradeInvalidPhoneNumbersToAWarning);
		}

		public void TestNumericValuesOnlyForPhoneNumberFields()
		{
			AssertEquals("The default value of registry item NumericValuesOnlyForPhoneNumberFields should be false", false, Registry.NumericValuesOnlyForPhoneNumberFields);
			Registry.SetNumericValuesOnlyForPhoneNumberFields(true);
			AssertEquals("Registry item NumericValuesOnlyForPhoneNumberFields's value should be true", true, Registry.NumericValuesOnlyForPhoneNumberFields);
			Registry.SetNumericValuesOnlyForPhoneNumberFields(false);
			AssertEquals("Registry item NumericValuesOnlyForPhoneNumberFields's value should be false", false, Registry.NumericValuesOnlyForPhoneNumberFields);
		}

		public void TestAllowDefaultNoteContextFromCurrentlyLoggedInDepartment()
		{
			AssertEquals("AllowDefaultNoteContextFromCurrentlyLoggedInDepartment", false, Registry.DefaultNoteContextFromCurrentlyLoggedInDepartment);
			Registry.SetDefaultNoteContextFromCurrentlyLoggedInDepartment(true);
			AssertEquals("AllowDefaultNoteContextFromCurrentlyLoggedInDepartment", true, Registry.DefaultNoteContextFromCurrentlyLoggedInDepartment);
			Registry.SetDefaultNoteContextFromCurrentlyLoggedInDepartment(false);
			AssertEquals("AllowDefaultNoteContextFromCurrentlyLoggedInDepartment", false, Registry.DefaultNoteContextFromCurrentlyLoggedInDepartment);
		}

		public void TestAllowDefaultNoteCompanyFromCurrentlyLoggedInCompany()
		{
			AssertEquals("AllowDefaultNoteCompanyFromCurrentlyLoggedInCompany", false, Registry.DefaultNoteCompanyFromCurrentlyLoggedInCompany);
			Registry.SetDefaultNoteCompanyFromCurrentlyLoggedInCompany(true);
			AssertEquals("AllowDefaultNoteCompanyFromCurrentlyLoggedInCompany", true, Registry.DefaultNoteCompanyFromCurrentlyLoggedInCompany);
			Registry.SetDefaultNoteCompanyFromCurrentlyLoggedInCompany(false);
			AssertEquals("AllowDefaultNoteCompanyFromCurrentlyLoggedInCompany", false, Registry.DefaultNoteCompanyFromCurrentlyLoggedInCompany);
		}

		public void TestCalendarIntegration()
		{
			Registry.RawRegistry.CalendarIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.RawRegistry.CalendarIntegration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("CalendarIntegration", false, Registry.CalendarIntegration);

			Registry.RawRegistry.CalendarIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CalendarIntegration", false, Registry.CalendarIntegration);

			Registry.RawRegistry.CalendarIntegration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.RawRegistry.CalendarIntegration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("CalendarIntegration", true, Registry.CalendarIntegration);

			Registry.RawRegistry.CalendarIntegration.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, false);
			AssertEquals("CalendarIntegration", false, Registry.CalendarIntegration);
		}

		public void TestSystemFont()
		{
			var defaultFont = "Tahoma";
			var companyFont = "Times New Roman";
			var thirdFont = "Arial";

			Registry.RawRegistry.SystemFontRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFont);
			AssertEquals("SystemFontRegItem", defaultFont, Registry.SystemFontRegItem);
			AssertEquals("SystemFontRegItem", defaultFont, EnvProxy.Instance.Registry.SystemFontRegItem);

			Registry.RawRegistry.SystemFontRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultFont);
			Registry.RawRegistry.SystemFontRegItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, companyFont);
			AssertEquals("SystemFontRegItem", companyFont, Registry.SystemFontRegItem);

			Registry.RawRegistry.SystemFontRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thirdFont);
			AssertEquals("SystemFontRegItem", companyFont, Registry.SystemFontRegItem);
		}

		public void TestGraphicRenderingEngineRegItem()
		{
			var defaultRenderingEngine = nameof(TextRendererType.GDI);
			var testRenderingEngine = nameof(TextRendererType.GDIPlus);
			Registry.RawRegistry.GraphicRenderingEngineRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultRenderingEngine);
			AssertEquals("Default Rendering Engine", defaultRenderingEngine, Registry.GraphicRenderingEngineRegItem);
			Registry.RawRegistry.GraphicRenderingEngineRegItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testRenderingEngine);
			AssertEquals("Set to GDI+ Rendering Engine", testRenderingEngine, Registry.GraphicRenderingEngineRegItem);
		}

		public void TestForceDialogRenderingOverTS()
		{
			Registry.RawRegistry.ForceDialogRenderingOverTS.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("From system level", true, Registry.ForceDialogRenderingOverTS);

			Registry.RawRegistry.ForceDialogRenderingOverTS.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("From system level", false, Registry.ForceDialogRenderingOverTS);

			Registry.RawRegistry.ForceDialogRenderingOverTS.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("From company level", true, Registry.ForceDialogRenderingOverTS);

			Registry.RawRegistry.ForceDialogRenderingOverTS.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("From company level", false, Registry.ForceDialogRenderingOverTS);
		}

		public void TestShouldForceDialogRendering()
		{
			Registry.RawRegistry.ForceDialogRenderingOverTS.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition - option is enabled", true, Registry.ForceDialogRenderingOverTS);

			Registry.SetIsTerminalServiceModeForTest(false);
			AssertEquals("Disabled in not TS mode", false, Registry.ShouldForceDialogRendering);

			Registry.SetIsTerminalServiceModeForTest(true);
			AssertEquals("Enabled in TS mode", true, Registry.ShouldForceDialogRendering);

			Registry.RawRegistry.ForceDialogRenderingOverTS.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - option is disabled", false, Registry.ForceDialogRenderingOverTS);

			Registry.SetIsTerminalServiceModeForTest(false);
			AssertEquals("Disabled when disabled regardles of TS mode", false, Registry.ShouldForceDialogRendering);

			Registry.SetIsTerminalServiceModeForTest(true);
			AssertEquals("Disabled when disabled regardles of TS mode", false, Registry.ShouldForceDialogRendering);
		}

		public void TestCalendarInvitationSolutionForOrganisers()
		{
			Registry.RawRegistry.CalendarInvitationSolutionForOrganisers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CalendarInvitationSolutionForOrganisers", false, Registry.CalendarInvitationSolutionForOrganisers);

			Registry.RawRegistry.CalendarInvitationSolutionForOrganisers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CalendarInvitationSolutionForOrganisers", true, Registry.CalendarInvitationSolutionForOrganisers);
		}

		public void TestUseGraphApiForIncoming()
		{
			Registry.RawRegistry.UseGraphApiForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("UseGraphApiForIncoming", false, Registry.UseGraphApiForIncoming);

			Registry.RawRegistry.UseGraphApiForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("UseGraphApiForIncoming", true, Registry.UseGraphApiForIncoming);
		}

		public void TestUseGraphApiForOutgoing()
		{
			Registry.RawRegistry.UseGraphApiForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("UseGraphApiForOutgoing", false, Registry.UseGraphApiForOutgoing);

			Registry.RawRegistry.UseGraphApiForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("UseGraphApiForOutgoing", true, Registry.UseGraphApiForOutgoing);
		}

		public void TestOrgShowConsigneeConsignorTab()
		{
			Registry.RawRegistry.OrgShowConsigneeConsignorTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.RawRegistry.OrgShowConsigneeConsignorTab.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("OrgShowConsigneeConsignorTab", false, Registry.OrgShowConsigneeConsignorTab);

			Registry.RawRegistry.OrgShowConsigneeConsignorTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("OrgShowConsigneeConsignorTab", false, Registry.OrgShowConsigneeConsignorTab);

			Registry.RawRegistry.OrgShowConsigneeConsignorTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.RawRegistry.OrgShowConsigneeConsignorTab.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("OrgShowConsigneeConsignorTab", true, Registry.OrgShowConsigneeConsignorTab);

			Registry.RawRegistry.OrgShowConsigneeConsignorTab.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("OrgShowConsigneeConsignorTab", false, Registry.OrgShowConsigneeConsignorTab);
		}

		public void TestOrgShowARTab()
		{
			Registry.RawRegistry.OrgShowARTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.RawRegistry.OrgShowARTab.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("OrgShowARTab", false, Registry.OrgShowARTab);

			Registry.RawRegistry.OrgShowARTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("OrgShowARTab", false, Registry.OrgShowARTab);

			Registry.RawRegistry.OrgShowARTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.RawRegistry.OrgShowARTab.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("OrgShowARTab", true, Registry.OrgShowARTab);

			Registry.RawRegistry.OrgShowARTab.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("OrgShowARTab", false, Registry.OrgShowARTab);
		}

		public void TestOrgUserFlagCount()
		{
			AssertEquals(Registry.RawRegistry.OrgUserFlagCount, Registry.OrgUserFlagCount);
		}

		public void TestOrgUserFlagLabels()
		{
			for (var i = 1; i <= Registry.OrgUserFlagCount; i++)
			{
				var userFlagLabelPropertyName = "OrgUserFlag" + i + "Label";
				var userFlagLabelProperty = Registry.GetType().GetProperty(userFlagLabelPropertyName, BindingFlags.Public | BindingFlags.Instance);
				AssertNotNull("PropertyInfo: " + userFlagLabelPropertyName, userFlagLabelProperty);
				AssertEquals("PropertyType: " + userFlagLabelPropertyName, typeof(string), userFlagLabelProperty.PropertyType);

				var userFlagLabel = (string)userFlagLabelProperty.GetValue(Registry);
				var userFlagLabelRegistryItem = Registry.RawRegistry.FindByName("OrgUserFlag" + i + "Label");
				AssertEquals(userFlagLabelRegistryItem.Value, userFlagLabel);
			}
		}

		public void TestNavBarState()
		{
			Guid userPK = Guid.NewGuid();
			Registry.SetNavBarState(userPK, NavBarStyles.TreeView);
			AssertEquals("NavBarState", NavBarStyles.TreeView, Registry.GetNavBarState(userPK));
		}

		public void TestTimeLineView()
		{
			Guid userPK = Guid.NewGuid();
			AssertEquals("Should return Default value \"TimeLine\"", "TimeLine", Registry.GetTimeLineView(userPK));
			Registry.SetTimeLineView(userPK, "Regular");
			AssertEquals("TimeLineViewState", "Regular", Registry.GetTimeLineView(userPK));
		}

		public void TestShowUnshippedOrders()
		{
			Guid userPK = Guid.NewGuid();
			AssertEquals("Should return Default value \"DoNotShow\"", "DoNotShow", Registry.GetShowUnshippedOrdersMode(userPK));
			Registry.SetShowUnshippedOrdersMode(userPK, "Show");
			AssertEquals("ShowUnshippedOrdersMode", "Show", Registry.GetShowUnshippedOrdersMode(userPK));
		}

		public void TestWebUserDefaultSettingsForDocAddress()
		{
			Guid userPK = Guid.NewGuid();
			Assert("Default value should be empty", Registry.GetWebUserDefaultSettingsForDocAddress(userPK).IsEmpty);

			WebUserDefaultSettingsForDocAddress expectedSettings = new WebUserDefaultSettingsForDocAddress(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "DevileryAddress");
			Registry.SetWebUserDefaultSettingsForDocAddress(userPK, expectedSettings);

			AssertEquals("WebUserDefaultSettingsForDocAddress", expectedSettings.ToString(), Registry.GetWebUserDefaultSettingsForDocAddress(userPK).ToString());
		}

		public void TestWebVersionLaunchUrl()
		{
			const string expectedValue = "http://localhost:8200";
			Assert("Default value should be empty", Registry.WebVersionLaunchUrl.IsNullOrEmpty());

			Registry.LastAccessedModule = "abc";
			Registry.WebVersionLaunchUrl = expectedValue;

			AssertEquals("Set value is same as read value", expectedValue, Registry.WebVersionLaunchUrl);
		}

		public void TestLastAccessedModule()
		{
			Registry.LastAccessedModule = "abc";
			AssertEquals("LastAccessedModule", "abc", Registry.LastAccessedModule);
		}

		public void TestDoNotDisplayReleaseNotesVersion()
		{
			Registry.DoNotDisplayReleaseNotesVersion = "abc";
			AssertEquals("DoNotDisplayReleaseNotesVersion", "abc", Registry.DoNotDisplayReleaseNotesVersion);
		}

		public void TestGlobalFormTopCaption()
		{
			Registry.GlobalFormTopCaption = "abc";
			AssertEquals("GlobalFormTopCaption", "abc", Registry.GlobalFormTopCaption);
		}

		public void TestShowDatabaseName()
		{
			Registry.ShowDatabaseName = true;
			AssertEquals("ShowDatabaseName", true, Registry.ShowDatabaseName);
		}

		public void TestShowBranchName()
		{
			Registry.ShowBranchName = true;
			AssertEquals("ShowBranchName", true, Registry.ShowBranchName);
		}

		public void TestShowCompanyName()
		{
			Registry.ShowCompanyName = true;
			AssertEquals("ShowCompanyName", true, Registry.ShowCompanyName);
		}

		public void TestShowDepartmentName()
		{
			Registry.ShowDepartmentName = true;
			AssertEquals("ShowDepartmentName", true, Registry.ShowDepartmentName);
		}

		public void TestShowUserName()
		{
			Registry.ShowUserName = true;
			AssertEquals("ShowUserName", true, Registry.ShowUserName);
		}

		public void TestInterchangeMaxSends()
		{
			Registry.InterchangeMaxSends = 2;
			AssertEquals("InterchangeMaxSends", 2, Registry.InterchangeMaxSends);
		}

		public void TestAUCustomsExportMessagingMode()
		{
			Registry.AUCustomsImportsMessagingMode = Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("InterchangeResendDelay", Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, Registry.AUCustomsImportsMessagingMode);
		}

		public void TestInterchangeResendDelay()
		{
			Registry.InterchangeResendDelay = 10;
			AssertEquals("InterchangeResendDelay", 10, Registry.InterchangeResendDelay);
		}

		public void TestCanUserEditOrganisationCode()
		{
			Registry.CanUserEditOrganisationCode = true;
			AssertEquals("CanUserEditOrganisationCode", true, Registry.CanUserEditOrganisationCode);
		}

		public void TestPasswordControl()
		{
			AssertEquals("PasswordHashingIterationsCount default value", 200_000, Registry.PasswordHashingIterationsCount);

			EnvProxy.Instance.Registry.RawRegistry.PasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			EnvProxy.Instance.Registry.RawRegistry.PromptPasswordChangeBeforeExpireDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 14);
			EnvProxy.Instance.Registry.RawRegistry.PasswordHashingIterationsCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			AssertEquals("PasswordHashingIterationsCount type", typeof(int), Registry.PasswordHashingIterationsCount.GetType());
			AssertEquals("PasswordHashingIterationsCount value", 3, Registry.PasswordHashingIterationsCount);

			AssertEquals("PasswordHistoryCount type", typeof(int), Registry.PasswordHistoryCount.GetType());
			AssertEquals("PasswordHistoryCount value", 3, Registry.PasswordHistoryCount);

			AssertEquals("PromptPasswordChangeDays type", typeof(int), Registry.PromptPasswordChangeBeforeExpireDays.GetType());
			AssertEquals("PromptPasswordChangeDays value", 14, Registry.PromptPasswordChangeBeforeExpireDays);
		}

		public void TestQuotationDocumentLogo()
		{
			AssertNull("QuotationDocumentLogo", Registry.QuotationDocumentLogo);
		}

		public void TestHouseBillOfLadingLogo()
		{
			AssertNull("HouseBillOfLadingLogo", Registry.HouseBillOfLadingLogo);
		}

		public void TestBackupFilePath()
		{
			AssertEquals("BackupFilePath", "", Registry.BackupDirectoryPath);
		}

		public void TestBackupSystemDatabases()
		{
			AssertEquals("BackupSystemDatabase", false, Registry.BackupSystemDatabases);

			Registry.BackupSystemDatabases = true;
			AssertEquals("BackupSystemDatabase", true, Registry.BackupSystemDatabases);

			string originalHostedPlace = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("BackupSystemDatabase", false, Registry.BackupSystemDatabases);
			EnvProxy.SetHostedLocationForTest(originalHostedPlace);
		}

		public void TestPurgeDataTimeoutLimit()
		{
			AssertEquals("PurgeDataTimeoutLimit", 30, Registry.PurgeDataTimeoutLimit);
		}

		public void TestPurgeDataRunStatusFlag()
		{
			AssertEquals("PurgeDataRunStatusFlag", "CMP", Registry.PurgeDataRunStatusFlag);
		}

		public void TestGhostRecordCleanerLockTimeoutSeconds()
		{
			AssertEquals("GhostRecordCleanerLockTimeoutSeconds", 10, Registry.GhostRecordCleanerLockTimeoutSeconds);
		}

		public void TestGhostRecordProcessingThreadInterval()
		{
			AssertEquals("GhostRecordProcessingThreadInterval", 5, Registry.GhostRecordProcessingThreadInterval);
		}

		public void TestGhostRecordMaxProcessingThreadCount()
		{
			AssertEquals("GhostRecordMaxProcessingThreadCount", 10, Registry.GhostRecordMaxProcessingThreadCount);
		}

		public void TestGhostRecordCleanupCommandTimeOutThreshold()
		{
			AssertEquals("GhostRecordCleanupCommandTimeOutThreshold", 0, Registry.GhostRecordCleanupCommandTimeOutThreshold);
		}

		public void TestGhostRecordCleanupKillBlockersThreshold()
		{
			AssertEquals("GhostRecordCleanupKillBlockersThreshold", -1, Registry.GhostRecordCleanupKillBlockersThreshold);
		}

		public void TestGhostRecordCleanupOnlineRebuild()
		{
			AssertEquals("GhostRecordCleanupOnlineRebuild", true, Registry.GhostRecordCleanupOnlineRebuild);
		}

		public void TestLockTimeout()
		{
			AssertEquals("ConnectionLockTimeout", DbConnection.LockTimeout.Default, Registry.LockTimeout);

			Registry.LockTimeout = 100;
			AssertEquals("ConnectionLockTimeout", 100, Registry.LockTimeout);
		}

		public void TestDbccInitialTimeout()
		{
			AssertEquals("Default", 7200000, Registry.DbccInitialTimeout);

			Registry.DbccInitialTimeout = 400000;
			AssertEquals("New value", 400000, Registry.DbccInitialTimeout);
		}

		public void TestDbccUserWaitThreshold()
		{
			AssertEquals("Default", 5000, Registry.DbccUserWaitThreshold);

			Registry.DbccUserWaitThreshold = 400000;
			AssertEquals("New value", 400000, Registry.DbccUserWaitThreshold);
		}

		public void TestDbccServiceTaskWaitThreshold()
		{
			AssertEquals("Default", 240000, Registry.DbccServiceTaskWaitThreshold);

			Registry.DbccServiceTaskWaitThreshold = 400000;
			AssertEquals("New value", 400000, Registry.DbccServiceTaskWaitThreshold);
		}

		public void TestDbccPollingInterval()
		{
			AssertEquals("Default", 3500, Registry.DbccPollingInterval);

			Registry.DbccPollingInterval = 10000;
			AssertEquals("New value", 10000, Registry.DbccPollingInterval);
		}

		public void TestDbccRunCheckdbWithPhysicalOnly()
		{
			AssertEquals("Default", false, Registry.DbccRunCheckdbWithPhysicalOnly);

			Registry.DbccRunCheckdbWithPhysicalOnly = true;
			AssertEquals("New value", true, Registry.DbccRunCheckdbWithPhysicalOnly);
		}

		public void TestDbccRunSecondariesInParallel()
		{
			AssertEquals("Default", false, Registry.DbccRunSecondariesInParallel);

			Registry.DbccRunSecondariesInParallel = true;
			AssertEquals("New value", true, Registry.DbccRunSecondariesInParallel);
		}

		public void TestDbccRunSecondariesByParts()
		{
			AssertEquals("Default", false, Registry.DbccRunSecondariesByParts);

			Registry.DbccRunSecondariesByParts = true;
			AssertEquals("New value", true, Registry.DbccRunSecondariesByParts);
		}

		public void TestISU_Rebuild_ThresholdPercentage()
		{
			AssertEquals("Default", 40, Registry.ISU_Rebuild_ThresholdPercentage);

			Registry.ISU_Rebuild_ThresholdPercentage = 100;
			AssertEquals("New value", 100, Registry.ISU_Rebuild_ThresholdPercentage);
		}

		public void TestISU_Rebuild_MaxDopPercentage()
		{
			AssertEquals("Default", 25, Registry.ISU_Rebuild_MaxDopPercentage);

			Registry.ISU_Rebuild_MaxDopPercentage = 16;
			AssertEquals("New value", 16, Registry.ISU_Rebuild_MaxDopPercentage);
		}

		public void TestISU_Rebuild_Online()
		{
			AssertEquals("Default", true, Registry.ISU_Rebuild_Online);

			Registry.ISU_Rebuild_Online = false;
			AssertEquals("New value", false, Registry.ISU_Rebuild_Online);
		}

		public void TestISU_Rebuild_UseObserver()
		{
			AssertEquals("Default", false, Registry.ISU_Rebuild_UseObserver);

			Registry.ISU_Rebuild_UseObserver = true;
			AssertEquals("New value", true, Registry.ISU_Rebuild_UseObserver);
		}

		public void TestISU_Rebuild_MaxWaitInMinutes()
		{
			AssertEquals("Default", 1, Registry.ISU_Rebuild_MaxWaitInMinutes);

			Registry.ISU_Rebuild_MaxWaitInMinutes = 100;
			AssertEquals("New value", 100, Registry.ISU_Rebuild_MaxWaitInMinutes);
		}

		public void TestISU_Rebuild_AbortAfterWait()
		{
			AssertEquals("Default", OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers, Registry.ISU_Rebuild_AbortAfterWait);

			Registry.ISU_Rebuild_AbortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self;
			AssertEquals("New value", OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Self, Registry.ISU_Rebuild_AbortAfterWait);
		}

		public void TestISU_Reorganize_Threshold()
		{
			AssertEquals("Default", 10, Registry.ISU_Reorganize_Threshold);

			Registry.ISU_Reorganize_Threshold = 100;
			AssertEquals("New value", 100, Registry.ISU_Reorganize_Threshold);
		}

		public void TestISU_Reorganize_MaxConcurrentProcessesPercentage()
		{
			AssertEquals("Default", 25, Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage);

			Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 10;
			AssertEquals("New value", 10, Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage);
		}

		public void TestISU_Reorganize_PeriodInDays()
		{
			AssertEquals("Default", 7, Registry.ISU_Reorganize_PeriodInDays);

			Registry.ISU_Reorganize_PeriodInDays = 10000;
			AssertEquals("New value", 10000, Registry.ISU_Reorganize_PeriodInDays);
		}

		public void TestISU_MinimumIndexPageCount()
		{
			AssertEquals("Default", 50, Registry.ISU_MinimumIndexPageCount);

			Registry.ISU_MinimumIndexPageCount = 10000;
			AssertEquals("New value", 10000, Registry.ISU_MinimumIndexPageCount);
		}

		public void TestISU_MaxBacklogWaitTime_InMinutes()
		{
			AssertEquals("Default", 60, Registry.ISU_MaxBacklogWaitTime_InMinutes);

			Registry.ISU_MaxBacklogWaitTime_InMinutes = 100;
			AssertEquals("New value", 100, Registry.ISU_MaxBacklogWaitTime_InMinutes);
		}

		public void TestISU_DisableAutoStatisticsDuringUpgrade()
		{
			AssertEquals("Default", true, Registry.ISU_DisableAutoStatisticsDuringUpgrade);

			Registry.ISU_DisableAutoStatisticsDuringUpgrade = false;
			AssertEquals("New value", false, Registry.ISU_DisableAutoStatisticsDuringUpgrade);
		}

		public void TestShowUnimplementedModule()
		{
			Registry.ShowUnimplementedModule = false;
			AssertEquals("ShowUnimplementedModule", false, Registry.ShowUnimplementedModule);
		}

		public void TestEagleMailPrivateKey()
		{
			AssertNull(Registry.EagleMailPrivateKey);
		}

		public void TestEagleMailPrivateKeyPassword()
		{
			AssertNotNull("EagleMailPrivateKeyPassword", Registry.EagleMailPrivateKeyPassword);
		}

		public void TestEagleMailPublicCertificate()
		{
			AssertNull("EagleMailPublicCertificate", Registry.EagleMailPublicCertificate);
		}

		public void TestResourceStringUsageTrustedDomains()
		{
			AssertEquals("ResourceStringUsageTrustedDomains default value should be empty.", 0, Registry.ResourceStringUsageTrustedDomains.Length);
			Registry.ResourceStringUsageTrustedDomains = new string[] { "https://localhost:8080" };
			AssertArrayEqualsByElements("ResourceStringUsageTrustedDomains", new string[] { "https://localhost:8080" }, Registry.ResourceStringUsageTrustedDomains);
		}

		public void TestMailboxEmailAddress()
		{
			Registry.EnterpriseMailboxEmailAddress = "henry@test.com";
			AssertEquals("MailboxEmailAddress", "henry@test.com", Registry.EnterpriseMailboxEmailAddress);
		}

		public void TestSelectNDRPath()
		{
			Registry.SelectNDRPath = Constants.SelectNDRPath.Codes.CP;
			AssertEquals(Constants.SelectNDRPath.Codes.CP, Registry.SelectNDRPath);
		}

		public void TestMs365AppSecretForIncoming()
		{
			AssertMs365OAuth2AppTokenForIncomingHasValue();
			Registry.Ms365AppSecretForIncoming = "123";
			AssertEquals("Ms365AppSecretForIncoming", "123", Registry.Ms365AppSecretForIncoming);
			AssertEquals("Test OnUpdateAction: AppToken will be deleted", true, Registry.Ms365OAuth2AppTokenForIncoming.IsNullOrEmpty());
		}

		public void TestMs365OAuth2AppTokenForIncoming()
		{
			AssertMs365OAuth2AppTokenForIncomingHasValue();
		}

		public void TestMs365AppSecretForOutgoing()
		{
			AssertMs365OAuth2AppTokenForOutgoingHasValue();
			Registry.Ms365AppSecretForOutgoing = "123";
			AssertEquals("Ms365AppSecretForOutgoing", "123", Registry.Ms365AppSecretForOutgoing);
			AssertEquals("Test OnUpdateAction: AppToken will be deleted", true, Registry.Ms365OAuth2AppTokenForOutgoing.IsNullOrEmpty());
		}

		public void TestMs365OAuth2AppTokenForOutgoing()
		{
			AssertMs365OAuth2AppTokenForOutgoingHasValue();
		}

		internal void AssertMs365OAuth2AppTokenForIncomingHasValue()
		{
			var token = new byte[] { 1, 2 };
			Registry.Ms365OAuth2AppTokenForIncoming = token;
			AssertEquals("Ms365OAuth2AppTokenForIncoming", token, Registry.Ms365OAuth2AppTokenForIncoming);
		}

		internal void AssertMs365OAuth2AppTokenForOutgoingHasValue()
		{
			var token = new byte[] { 1, 2 };
			Registry.Ms365OAuth2AppTokenForOutgoing = token;
			AssertEquals("Ms365OAuth2AppTokenForOutgoing", token, Registry.Ms365OAuth2AppTokenForOutgoing);
		}

		public void TestSMTPEhloDomain()
		{
			Registry.SMTPEhloDomain = "test.com";
			AssertEquals("test.com", Registry.SMTPEhloDomain);
		}

		public void TestFreightVolumeUnit()
		{
			AssertEquals("FreightVolumeUnit", "M3", Registry.FreightVolumeUnit);
		}

		public void TestFreightWeightUnit()
		{
			AssertEquals("FreightWeightUnit", "KG", Registry.FreightWeightUnit);
		}

		public void TestOrderLineContainersVisible()
		{
			Guid buyerPK = Guid.NewGuid();
			Registry.SetOrderLineContainersVisible(buyerPK, true);

			AssertEquals("GetOrderLineContainersVisible", true, Registry.GetOrderLineContainersVisible(buyerPK));
			AssertEquals("GetOrderLineContainersVisible from wrong buyer", false, Registry.GetOrderLineContainersVisible(Guid.NewGuid()));

			Registry.SetOrderLineContainersVisible(buyerPK, false);
			AssertEquals("GetOrderLineContainersVisible", false, Registry.GetOrderLineContainersVisible(buyerPK));
		}

		public void TestIsExpress()
		{
			Registry.IsExpress = false;
			Assert(!Registry.IsExpress);
			Registry.IsExpress = true;
			Assert(Registry.IsExpress);
		}

		public void TestFilterCriteria()
		{
			Registry.SetFilterCriteria("Testform", "TestCriteria");
			AssertEquals("FilterCriteria", "TestCriteria", Registry.GetFilterCriteria("Testform"));
		}

		public void TestFilterCriteria_WithLoginPKAsParameter()
		{
			Guid parentGuid1 = Guid.NewGuid();
			Guid parentGuid2 = Guid.NewGuid();

			Registry.SetFilterCriteria(parentGuid1, "TestForm", "Guid1Criteria");
			AssertEquals("Assigned to ParentGuid1", "", Registry.GetFilterCriteria(parentGuid2, "TestForm"));
			AssertEquals("Assigned to ParentGuid1", "Guid1Criteria", Registry.GetFilterCriteria(parentGuid1, "TestForm"));

			Registry.SetFilterCriteria(parentGuid2, "TestForm", "Guid2Criteria");
			AssertEquals("Assigned to ParentGuid2", "Guid2Criteria", Registry.GetFilterCriteria(parentGuid2, "TestForm"));
			AssertEquals("Assigned to ParentGuid1", "Guid1Criteria", Registry.GetFilterCriteria(parentGuid1, "TestForm"));
		}

		public void TestClearCacheWorksForFilterCriteria()
		{
			EnvProxy.Instance.Registry.SetFilterCriteria("Testform", "TestCriteria");
			Db.Connection.RollbackTransaction();// rolling back 15543
			Db.Connection.BeginTransaction();// rolling back 15543
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals("Cache Cleared", "", EnvProxy.Instance.Registry.GetFilterCriteria("Testform"));
		}

		public void AssertOrgRequiredFields(OrgRequiredFields fields, bool requireAddress2, bool requireBranch, bool requireCity, bool requirePhoneNumber, bool requireBusinessNumber, bool requirePhoneOrBusinessNumber, bool requireFaxNumber, bool requireEmailAddress, bool requireWebAddress, bool requireFaxEmailOrWeb, bool requireARContact)
		{
			AssertEquals("RequireAddress2", requireAddress2, fields.RequireAddress2);
			AssertEquals("RequireUNLOCO", requireBranch, fields.RequireBranch);
			AssertEquals("RequireCity", requireCity, fields.RequireCity);
			AssertEquals("RequirePhoneNumber", requirePhoneNumber, fields.RequirePhoneNumber);
			AssertEquals("RequireBusinessNumber", requireBusinessNumber, fields.RequireBusinessNumber);
			AssertEquals("RequirePhoneOrBusinessNumber", requirePhoneOrBusinessNumber, fields.RequirePhoneOrBusinessNumber);
			AssertEquals("RequireFaxNumber", requireFaxNumber, fields.RequireFaxNumber);
			AssertEquals("RequireEmailAddress", requireEmailAddress, fields.RequireEmailAddress);
			AssertEquals("RequireWebAddress", requireWebAddress, fields.RequireWebAddress);
			AssertEquals("RequireFaxEmailOrWeb", requireFaxEmailOrWeb, fields.RequireFaxEmailOrWeb);
			AssertEquals("RequireARContact", requireARContact, fields.RequireARContact);
		}

		public void TestGetTempOrgSalesRequiredFields()
		{
			OrgRequiredFields fields = Registry.GetTempOrgSalesRequiredFields();

			AssertOrgRequiredFields(fields, false, false, true, false, false, false, false, false, false, false, false);

			OrgRequiredFields newFields = new OrgRequiredFields(true, true, false, true, true, true, true, true, true, true, true);

			Registry.RawRegistry.TempOrgSalesRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newFields.GetRegistryValue());

			fields = Registry.GetTempOrgSalesRequiredFields();

			AssertOrgRequiredFields(fields, true, true, false, true, true, true, true, true, true, true, true);
		}

		public void TestSetTempOrgSalesRequiredFields()
		{
			OrgRequiredFields fields = new OrgRequiredFields(true, true, false, true, true, true, true, true, true, true, true);
			Registry.SetTempOrgSalesRequiredFields(fields);

			OrgRequiredFields newFields = Registry.GetTempOrgSalesRequiredFields();

			AssertOrgRequiredFields(newFields, true, true, false, true, true, true, true, true, true, true, true);
		}

		public void TestGetTempOrgDebtorRequiredFields()
		{
			OrgRequiredFields fields = Registry.GetTempOrgDebtorRequiredFields();

			AssertOrgRequiredFields(fields, false, false, true, false, false, false, false, false, false, false, false);

			OrgRequiredFields newFields = new OrgRequiredFields(true, true, false, true, true, true, true, true, true, true, true);

			Registry.RawRegistry.TempOrgDebtorRequiredFields.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newFields.GetRegistryValue());

			fields = Registry.GetTempOrgDebtorRequiredFields();

			AssertOrgRequiredFields(fields, true, true, false, true, true, true, true, true, true, true, true);
		}

		public void TestSetTempOrgDebtorRequiredFields()
		{
			OrgRequiredFields fields = new OrgRequiredFields(true, true, false, true, true, true, true, true, true, true, true);
			Registry.SetTempOrgDebtorRequiredFields(fields);

			OrgRequiredFields newFields = Registry.GetTempOrgDebtorRequiredFields();

			AssertOrgRequiredFields(newFields, true, true, false, true, true, true, true, true, true, true, true);
		}

		public void TestGetTempOrgDebtorRequiredFieldsForSpecificCompany()
		{
			OrgRequiredFields fieldsForSystem = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false);
			Registry.SetTempOrgDebtorRequiredFields(fieldsForSystem);

			OrgRequiredFields fieldsForCompany = new OrgRequiredFields(true, true, false, true, true, true, true, true, true, true, true);
			Registry.RawRegistry.TempOrgDebtorRequiredFields.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, fieldsForCompany.GetRegistryValue());

			OrgRequiredFields newFields = Registry.GetTempOrgDebtorRequiredFields();

			AssertOrgRequiredFields(newFields, true, true, false, true, true, true, true, true, true, true, true);
		}

		public void TestGetTempOrgCreditorRequiredFieldsForSpecificCompany()
		{
			OrgRequiredFields fieldsForSystem = new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false);
			Registry.SetTempOrgCreditorRequiredFields(fieldsForSystem);

			OrgRequiredFields fieldsForCompany = new OrgRequiredFields(true, true, false, true, true, true, true, true, true, true, true);
			Registry.RawRegistry.TempOrgCreditorRequiredFields.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, fieldsForCompany.GetRegistryValue());

			OrgRequiredFields newFields = Registry.GetTempOrgCreditorRequiredFields();

			AssertOrgRequiredFields(newFields, true, true, false, true, true, true, true, true, true, true, true);
		}

		public void TestBrokerageID()
		{
			Registry.BrokerageID = "321";
			AssertEquals("BrokerageID", "321", Registry.BrokerageID);
		}

		public void TestRequestOfficialCustomsPaymentReceipt()
		{
			AssertEquals("RequestOfficialCustomsPaymentReceipt should be true by default", true, Registry.RequestOfficialCustomsPaymentReceipt);
			Registry.RequestOfficialCustomsPaymentReceipt = false;
			AssertEquals("RequestOfficialCustomsPaymentReceipt", false, Registry.RequestOfficialCustomsPaymentReceipt);
		}

		public void TestUpdateCMRReferenceFilesWhenModifiedByCustoms()
		{
			AssertEquals("UpdateCMRReferenceFilesWhenModifiedByCustoms should be true by default", true, Registry.UpdateCMRReferenceFilesWhenModifiedByCustoms);
			Registry.UpdateCMRReferenceFilesWhenModifiedByCustoms = false;
			AssertEquals("UpdateCMRReferenceFilesWhenModifiedByCustoms", false, Registry.UpdateCMRReferenceFilesWhenModifiedByCustoms);
		}

		public void TestCartageAdviceText()
		{
			Registry.RawRegistry.CartageAdviceExportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Export Opening Text");
			Registry.RawRegistry.CartageAdviceExportClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Export Closing Text");
			Registry.RawRegistry.CartageAdviceImportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Import Opening Text");
			Registry.RawRegistry.CartageAdviceImportClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Import Closing Text");

			AssertEquals("CartageAdviceExport.OpeningText", "Export Opening Text", Registry.CartageAdviceExport.OpeningText);
			AssertEquals("CartageAdviceExport.ClosingText", "Export Closing Text", Registry.CartageAdviceExport.ClosingText);
			AssertEquals("CartageAdviceImport.OpeningText", "Import Opening Text", Registry.CartageAdviceImport.OpeningText);
			AssertEquals("CartageAdviceImport.ClosingText", "Import Closing Text", Registry.CartageAdviceImport.ClosingText);
		}

		public void TestConsignmentRequestForServiceAndAuthorization()
		{
			Registry.RawRegistry.ConsignmentRequestForServiceOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "RequestForService Opening Text");
			Registry.RawRegistry.ConsignmentRequestForServiceClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "RequestForService Closing Text");
			Registry.RawRegistry.ConsignmentAuthorizationForServiceOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "AuthorizationForService Opening Text");
			Registry.RawRegistry.ConsignmentAuthorizationForServiceClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "AuthorizationForService Closing Text");

			AssertEquals("ConsignmentRequestForService.OpeningText", "RequestForService Opening Text", Registry.ConsignmentRequestForService.OpeningText);
			AssertEquals("ConsignmentRequestForService.ClosingText", "RequestForService Closing Text", Registry.ConsignmentRequestForService.ClosingText);
			AssertEquals("ConsignmentAuthorizationForService.OpeningText", "AuthorizationForService Opening Text", Registry.ConsignmentAuthorizationForService.OpeningText);
			AssertEquals("ConsignmentAuthorizationForService.ClosingText", "AuthorizationForService Closing Text", Registry.ConsignmentAuthorizationForService.ClosingText);
		}

		public void TestCartageAdviceTimeSlotRequestOpeningText()
		{
			Registry.RawRegistry.CartageAdviceTimeSlotRequestExportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Export Opening Text");
			Registry.RawRegistry.CartageAdviceTimeSlotRequestImportOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, "Import Opening Text");

			AssertEquals("CartageAdviceTimeSlotRequestExportOpeningText.Value", "Export Opening Text", Registry.CartageAdviceTimeSlotRequestExportOpeningText);
			AssertEquals("CartageAdviceTimeSlotRequestImportOpeningText.Value", "Import Opening Text", Registry.CartageAdviceTimeSlotRequestImportOpeningText);
		}

		public void TestCartageAdviceTimeSlotConfirmationOpeningText()
		{
			Registry.RawRegistry.CartageAdviceTimeSlotConfirmationOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Opening Text");
			AssertEquals("CartageAdviceTimeSlotConfirmationOpeningText.Value", "Opening Text", Registry.CartageAdviceTimeSlotConfirmationOpeningText);
		}

		public void TestCartageAdviceTimeSlotConfirmationClosingText()
		{
			Registry.RawRegistry.CartageAdviceTimeSlotConfirmationClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Closing Text");
			AssertEquals("CartageAdviceTimeSlotConfirmationClosingText.Value", "Closing Text", Registry.CartageAdviceTimeSlotConfirmationClosingText);
		}

		public void TestExportCertificationOpeningText()
		{
			Registry.RawRegistry.ExportCertificationOpeningText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Opening Text");
			AssertEquals("ExportCertificationOpeningText.Value", "Opening Text", Registry.ExportCertificationOpeningText);
		}

		public void TestExportCertificationClosingText()
		{
			Registry.RawRegistry.ExportCertificationClosingText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Closing Text");
			AssertEquals("ExportCertificationClosingText.Value", "Closing Text", Registry.ExportCertificationClosingText);
		}

		public void TestCMRTestMode()
		{
			Registry.CMRTestMode = true;
			AssertEquals("CMRTestMode", true, Registry.CMRTestMode);
			AssertEquals("GetCMRTestMode", true, Registry.GetCMRTestMode(EnvProxy.Instance.CurrentCompany.PK));
		}

		public void TestCompetitorActivityList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.CompetitorActivityList;
			Assert("List.Count > 0", list.Count > 0);
		}

		public void TestAllowEmailsToBeSentFromUsersAddress()
		{
			Registry.AllowEmailsToBeSentFromUsersAddress = true;
			AssertEquals("AllowEmailsToBeSentFromUsersAddress", true, Registry.AllowEmailsToBeSentFromUsersAddress);
		}

		public void TestPayablesCreditAgreedPaymentMethodsList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.PayablesCreditAgreedPaymentMethodsList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestAddressAccessPointList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.AddressAccessPointList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestAddressCommunicationRequiredList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.AddressCommunicationRequiredList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestAddressContainerHandlingList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.AddressContainerHandlingList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestAddressDockHeightList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.AddressDockHeightList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestAddressLabourRequiredList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.AddressLabourRequiredList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestDeliveryRoutes()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.DeliveryRoutesList;

			CodeDescriptionPairList newList = new CodeDescriptionPairList();
			newList.AddPair("ABC", "ABC Desc");

			Registry.DeliveryRoutesList = newList;
			AssertEquals("DeliveryRoutesList.Count", 1, Registry.DeliveryRoutesList.Count);
			AssertEquals("DeliveryRoutesList.GetDescriptionFromCode(\"ABC\")", "ABC Desc", Registry.DeliveryRoutesList.GetDescriptionFromCode("ABC"));
			Registry.DeliveryRoutesList = list;
		}

		public void TestDeliveryRoutesListSorted()
		{
			var list = Registry.DeliveryRoutesList;

			var newList = new CodeDescriptionPairList();
			newList.AddPair("CCC", "CCC Desc");
			newList.AddPair("AAA", "AAA Desc");
			newList.AddPair("BBB", "BBB Desc");

			Registry.DeliveryRoutesList = newList;
			AssertEquals("DeliveryRoutesList.Count", 3, Registry.DeliveryRoutesListSorted.Count);

			AssertEquals("AAA", Registry.DeliveryRoutesListSorted[0].Code);
			AssertEquals("BBB", Registry.DeliveryRoutesListSorted[1].Code);
			AssertEquals("CCC", Registry.DeliveryRoutesListSorted[2].Code);
			AssertEquals("AAA Desc", Registry.DeliveryRoutesListSorted[0].Description);
			AssertEquals("BBB Desc", Registry.DeliveryRoutesListSorted[1].Description);
			AssertEquals("CCC Desc", Registry.DeliveryRoutesListSorted[2].Description);
			Registry.DeliveryRoutesList = list;
		}

		public void TestAgentCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.AgentCategoryList;
			Assert("List.Count", list.Count > 0);

			CodeDescriptionPairList newList = new CodeDescriptionPairList();
			newList.AddPair("ABC", "ABC Desc");

			Registry.AgentCategoryList = newList;
			AssertEquals("AgentCategoryList.Count", 1, Registry.AgentCategoryList.Count);
			AssertEquals("AgentCategoryList.GetDescriptionFromCode(\"ABC\")", "ABC Desc", Registry.AgentCategoryList.GetDescriptionFromCode("ABC"));
		}

		public void TestARCreditRatingList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.ARCreditRatingList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestCompetitorCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.CompetitorCategoryList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestExporterCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.ExporterCategoryList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestImporterCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.ImporterCategoryList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestSalesCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.SalesCategoryList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestPayablesCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.PayablesCategoryList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestReceivablesCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.ReceivablesCategoryList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestSalesGrowthOutlookList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.SalesGrowthOutlookList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestSalesEffectOnCostList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.SalesEffectOnCostList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestSalesStyleList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.SalesStyleList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestOrgListOfInterests()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.OrgListOfInterests;
			AssertEquals("List.Count", Registry.RawRegistry.OrgListOfInterests.Value.Count, list.Count);
		}

		public void TestOrgListOfContactAllocations()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.OrgListOfContactAllocations;
			AssertEquals("List.Count", Registry.RawRegistry.OrgListOfContactAllocations.Value.Count, list.Count);
			Assert("List.Count", list.Count > 0);
		}

		public void TestOrgStaffMemberAssignmentRoles()
		{
			AssertEquals("List", Registry.OrgStaffMemberAssignmentRoles.ElementsAsString, new StaffAssignmentRoles().ElementsAsString);
		}

		public void TestOrderHeaderStatusList()
		{
			Registry.RawRegistry.OrderHeaderStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList(OLookUpEditType.Gender));
			ReadOnlyCodeDescriptionPairList list = Registry.OrderHeaderStatusList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestOrderLineStatusList()
		{
			Registry.RawRegistry.OrderLineStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList(OLookUpEditType.Gender));
			ReadOnlyCodeDescriptionPairList list = Registry.OrderLineStatusList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestFeatureTestModeEnabled()
		{
			Registry.FeatureTestModeEnabled = true;
			AssertEquals("FeatureTestModeEnabled Set To True", true, Registry.FeatureTestModeEnabled);
			Registry.FeatureTestModeEnabled = false;
			AssertEquals("FeatureTestModeEnabled Default Value", false, Registry.FeatureTestModeEnabled);
		}

		public void TestWebVersionSiteOfflineMessage()
		{
			Registry.WebVersionSiteOfflineMessage = "Site Offline Message";
			AssertEquals("WebVersionSiteOfflineMessage", "Site Offline Message", Registry.WebVersionSiteOfflineMessage);
		}

		public void TestAUCustomsAirCargoTestMode()
		{
			Registry.AUCustomsAirCargoTestMode = true;
			AssertEquals("AUCustomsAirCargoTestMode", true, Registry.AUCustomsAirCargoTestMode);
			AssertEquals("GetAUCustomsAirCargoTestMode", true, Registry.GetAUCustomsAirCargoTestMode(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestAUCustomsSeaCargoTestMode()
		{
			Registry.AUCustomsSeaCargoTestMode = true;
			AssertEquals("AUCustomsSeaCargoTestMode", true, Registry.AUCustomsSeaCargoTestMode);
			AssertEquals("GetAUCustomsSeaCargoTestMode", true, Registry.GetAUCustomsSeaCargoTestMode(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestAQISMessagingTestMode()
		{
			Registry.AQISMessagingTestMode = true;
			AssertEquals("AQISMessagingTestMode", true, Registry.AQISMessagingTestMode);
			AssertEquals("GetAQISMessagingTestMode", true, Registry.GetAQISMessagingTestMode(EnvProxy.Instance.CurrentCompany.PK));
		}

		public void TestSMTPUsername()
		{
			AssertEquals("SMTPUsername", "", Registry.SMTPUsername);
		}

		public void TestSMTPPassword()
		{
			AssertEquals("SMTPPassword", "", Registry.SMTPPassword);
		}

		public void TestSMTPDefaultReturnEmailAddress()
		{
			AssertEquals("SMTPDefaultReturnEmailAddress", Registry.MailboxEmailAddress, Registry.SMTPDefaultReturnEmailAddress);
		}

		public void TestSMTPDefaultDoNotReplyEmailAddress()
		{
			AssertEquals("SMTPDefaultDoNotReplyEmailAddress", "PleaseDoNotReply@wisetechglobal.com", Registry.SMTPDefaultDoNotReplyEmailAddress);

			Registry.SMTPDefaultDoNotReplyEmailAddress = "test@test.com";
			AssertEquals("SMTPDefaultDoNotReplyEmailAddress", "test@test.com", Registry.SMTPDefaultDoNotReplyEmailAddress);
		}

		public void TestMaximumNumberOfMailItemsToSendInABatch()
		{
			AssertEquals("MaximumNumberOfMailItemsToSendInABatch", 1000, Registry.MaximumNumberOfMailItemsToSendInABatch);
		}

		public void TestSmtpServerTimeout()
		{
			AssertEquals("Default value", 30, Registry.SMTPServerTimeout);

			Registry.RawRegistry.SMTPServerTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			AssertEquals(100, Registry.SMTPServerTimeout);

			Registry.RawRegistry.SMTPServerTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0, Registry.SMTPServerTimeout);
		}

		public void TestMaxRecommendedNumberOfRecordsToShowInDisplayGrids()
		{
			AssertEquals("MaxRecommendedNumberOfRecordsToShowInDisplayGrids", 100, Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids);

			try
			{
				Registry.RawRegistry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1001);
				Fail("Exception expected");
			}
			catch (RegistryValidationException)
			{
				Assert(true);
			}
		}

		public void TestRunSearchOnEnteringAModule()
		{
			AssertEquals("RunSearchOnEnteringAModule", false, Registry.RunSearchOnEnteringAModule);
			Registry.RawRegistry.RunSearchOnEnteringAModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("RunSearchOnEnteringAModule", true, Registry.RunSearchOnEnteringAModule);
		}

		public void TestAutoRunSearchFromFindBox()
		{
			AssertEquals("AutoRunSearchFromFindBox", true, Registry.AutoRunSearchFromFindBox);
			Registry.RawRegistry.AutoRunSearchFromFindBox.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("AutoRunSearchFromFindBox", false, Registry.AutoRunSearchFromFindBox);
		}

		public void TestCarrierCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.CarrierCategoryList;
			Assert("List.Count > 0", list.Count > 0);
		}

		public void TestServicesCategoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.ServicesCategoryList;
			Assert("List.Count > 0", list.Count > 0);
		}

		public void TestSalesTerritoryList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.SalesTerritoryList;
			Assert("List.Count > 0", list.Count > 0);
		}

		public void TestLanguageSkillLevelList()
		{
			ReadOnlyCodeDescriptionPairList list = Registry.LanguageSkillLevelList;
			Assert("List.Count", list.Count > 0);
		}

		public void TestStandardWorkingHoursDuration()
		{
			AssertEquals(new TimeSpan(40, 0, 0), Registry.StandardWorkingHoursDuration);
			Registry.StandardWorkingHoursDuration = new TimeSpan(38, 59, 0);
			AssertEquals(new TimeSpan(38, 59, 0), Registry.StandardWorkingHoursDuration);
			AssertExceptionThrown(typeof(RegistryValidationException), () => Registry.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "blabla"));
		}

		public void TestStandardWorkingHoursDurationForBranch()
		{
			Registry.RawRegistry.StandardWorkingHours.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "38:59");
			AssertEquals("StandardWorkingHoursDurationForBranch", new TimeSpan(38, 59, 0), Registry.StandardWorkingHoursDurationForBranch(EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestManifestClientID()
		{
			try
			{
				Registry.RawRegistry.ManifestClientID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "1234");
				Fail("Exception expected");
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("Error Message", "The Manifest Client ID must have a length of 10.", e.Message);
			}

			try
			{
				Registry.RawRegistry.ManifestClientID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "1234567890");
				Fail("Exception expected");
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("Error Message", "The Manifest Client ID must start with a character.", e.Message);
			}

			try
			{
				Registry.RawRegistry.ManifestClientID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "C23456789C");
				Fail("Exception expected");
			}
			catch (RegistryValidationException e)
			{
				AssertEquals("Error Message", "The last 9 characters must be a valid ACN.", e.Message);
			}

			Registry.RawRegistry.ManifestClientID.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "C234567890");
			AssertEquals("ManifestClientID", "C234567890", Registry.ManifestClientID);
		}

		public void TestOrgBuyerSupplierSubCategory()
		{
			AssertEquals("Use Buyer Supplier Relationships - default.", true, Registry.UseBuyerSupplierRelationships);
			AssertEquals("Prompt to save Buyer Supplier - default.", true, Registry.PromptToSaveBuyerSupplier);
		}

		public void TestNumberGroupSeparator()
		{
			Registry.RawRegistry.NumberGroupSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("NumberGroupSeparator", "*", Registry.NumberGroupSeparator);
		}

		public void TestNumberDecimalSeparator()
		{
			Registry.RawRegistry.NumberDecimalSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("NumberDecimalSeparator", "*", Registry.NumberDecimalSeparator);
		}

		public void TestNumberGroupSizes()
		{
			Registry.RawRegistry.NumberGroupSizes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "4");
			AssertEquals("NumberGroupSizes", "4", Registry.NumberGroupSizes);

			Registry.RawRegistry.NumberGroupSizes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "3,4");
			AssertEquals("NumberGroupSizes", "3,4", Registry.NumberGroupSizes);
		}

		public void TestCurrencyGroupSeparator()
		{
			Registry.RawRegistry.CurrencyGroupSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("CurrencyGroupSeparator", "*", Registry.CurrencyGroupSeparator);
		}

		public void TestCurrencyDecimalSeparator()
		{
			Registry.RawRegistry.CurrencyDecimalSeparator.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("CurrencyDecimalSeparator", "*", Registry.CurrencyDecimalSeparator);
		}

		public void TestCurrencyGroupSizes()
		{
			Registry.RawRegistry.CurrencyGroupSizes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "4");
			AssertEquals("CurrencyGroupSizes", "4", Registry.CurrencyGroupSizes);

			Registry.RawRegistry.CurrencyGroupSizes.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "3,4");
			AssertEquals("CurrencyGroupSizes", "3,4", Registry.CurrencyGroupSizes);
		}

		public void TestOuterPacklinesMeasurementDefaultUnit()
		{
			AssertEquals("OuterPacklinesMeasurementDefaultUnit", Constants.Length.Metres, Registry.OuterPacklinesMeasurementDefaultUnit);
		}

		public void TestInnerPacklinesMeasurementDefaultUnit()
		{
			AssertEquals("InnerPacklinesMeasurementDefaultUnit", Constants.Length.Centimetres, Registry.InnerPacklinesMeasurementDefaultUnit);
		}

		public void TestDisbursementNoteTitle()
		{
			AssertEquals("DisbursementNoteTitle", "Note", Registry.DisbursementNoteTitle);
		}

		public void TestCurrentCheckedOutClientName()
		{
			Registry.CurrentCheckedOutClientName = "TestClient";
			AssertEquals("CurrentCheckedOutClientName", "TestClient", Registry.CurrentCheckedOutClientName);
		}

		public void TestClientDocumentVersion()
		{
			Registry.ClientDocumentVersion = 10;
			AssertEquals("ClientDocumentVersion", 10, Registry.ClientDocumentVersion);
		}

		public void TestClientDocumentName()
		{
			Registry.ClientDocumentName = "Woolworths";
			AssertEquals("ClientDocumentName", "Woolworths", Registry.ClientDocumentName);
		}

		public void TestNotChargedText()
		{
			AssertEquals("NotChargedText", "Not Charged", Registry.NotChargedText);
		}

		public void TestGlobalTariffDefault()
		{
			Registry.RawRegistry.GlobalTariffDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals("GlobalTariffDefault", (byte)10, Registry.GlobalTariffDefault);

			Registry.RawRegistry.GlobalTariffDefault.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 20);
			AssertEquals("GlobalTariffDefault", (byte)20, Registry.GlobalTariffDefault);
		}

		public void TestCustomsRequestForMissingDocumentsClause()
		{
			Registry.RawRegistry.CustomsRequestForMissingDocumentsClause.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Clause");
			AssertEquals("Clause", "Clause", Registry.CustomsRequestForMissingDocumentsClause);
		}

		public void TestExcelPassword()
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>().Key;
			var strValue = productRegistration.EnterpriseCode + productRegistration.ServerCode;
			var defaultPassword =  strValue.GetHashCode().ToString().Substring(1, 6);

			AssertEquals("ExcelPassword Length", 6, Registry.ExcelPasswordForModifying.Length);
			AssertEquals("ExcelPassword Length", 6, Registry.ExcelPasswordForOpening.Length);

			AssertEquals("ExcelPassword Length", defaultPassword, Registry.ExcelPasswordForModifying);
			AssertEquals("ExcelPassword Length", defaultPassword, Registry.ExcelPasswordForOpening);
		}

		public void TestShowChargesOnBookingsBookingConfirmation()
		{
			AssertEquals("ShowChargesOnBookingsBookingConfirmation", false, Registry.ShowChargesOnBookingsBookingConfirmation);
			Registry.ShowChargesOnBookingsBookingConfirmation = true;
			AssertEquals("ShowChargesOnBookingsBookingConfirmation", true, Registry.ShowChargesOnBookingsBookingConfirmation);
		}

		public void TestShowChargesOnForwardingBookingConfirmation()
		{
			AssertEquals("ShowChargesOnForwardingBookingConfirmation", false, Registry.ShowChargesOnForwardingBookingConfirmation);
			Registry.ShowChargesOnForwardingBookingConfirmation = true;
			AssertEquals("ShowChargesOnForwardingBookingConfirmation", true, Registry.ShowChargesOnForwardingBookingConfirmation);
		}

		public void TestShowMarksAndNumbersForFCL()
		{
			AssertEquals("ShowMarksAndNumbersForFCL", true, Registry.ShowMarksAndNumbersForFCL);
			Registry.ShowMarksAndNumbersForFCL = false;
			AssertEquals("ShowMarksAndNumbersForFCL", false, Registry.ShowMarksAndNumbersForFCL);
		}

		public void TestShowMarksAndNumbersForNonFCL()
		{
			AssertEquals("ShowMarksAndNumbersForNonFCL", true, Registry.ShowMarksAndNumbersForNonFCL);
			Registry.ShowMarksAndNumbersForNonFCL = false;
			AssertEquals("ShowMarksAndNumbersForNonFCL", false, Registry.ShowMarksAndNumbersForNonFCL);
		}

		public void TestShowMarksAndNumbersForLSE()
		{
			AssertEquals("ShowMarksAndNumbersForLSE", true, Registry.ShowMarksAndNumbersForLSE);
			Registry.ShowMarksAndNumbersForLSE = false;
			AssertEquals("ShowMarksAndNumbersForLSE", false, Registry.ShowMarksAndNumbersForLSE);
		}

		public void TestShowMarksAndNumbersForNonLSE()
		{
			AssertEquals("ShowMarksAndNumbersForNonLSE", true, Registry.ShowMarksAndNumbersForNonLSE);
			Registry.ShowMarksAndNumbersForNonLSE = false;
			AssertEquals("ShowMarksAndNumbersForNonLSE", false, Registry.ShowMarksAndNumbersForNonLSE);
		}

		public void TestMarksAndNumbersHeadingForFCL()
		{
			AssertEquals("MarksAndNumbersHeadingForFCL", "MARKS AND NUMBERS", Registry.MarksAndNumbersHeadingForFCL);
			Registry.MarksAndNumbersHeadingForFCL = "SOMETHING ELSE";
			AssertEquals("MarksAndNumbersHeadingForFCL", "SOMETHING ELSE", Registry.MarksAndNumbersHeadingForFCL);
		}

		public void TestMarksAndNumbersHeadingForNonFCL()
		{
			AssertEquals("MarksAndNumbersHeadingForNonFCL", "MARKS AND NUMBERS", Registry.MarksAndNumbersHeadingForNonFCL);
			Registry.MarksAndNumbersHeadingForNonFCL = "SOMETHING ELSE";
			AssertEquals("MarksAndNumbersHeadingForNonFCL", "SOMETHING ELSE", Registry.MarksAndNumbersHeadingForNonFCL);
		}

		public void TestMarksAndNumbersHeadingForLSE()
		{
			AssertEquals("MarksAndNumbersHeadingForLSE", "MARKS AND NUMBERS", Registry.MarksAndNumbersHeadingForLSE);
			Registry.MarksAndNumbersHeadingForLSE = "SOMETHING ELSE";
			AssertEquals("MarksAndNumbersHeadingForLSE", "SOMETHING ELSE", Registry.MarksAndNumbersHeadingForLSE);
		}

		public void TestMarksAndNumbersHeadingForNonLSE()
		{
			AssertEquals("MarksAndNumbersHeadingForNonLSE", "MARKS AND NUMBERS", Registry.MarksAndNumbersHeadingForNonLSE);
			Registry.MarksAndNumbersHeadingForNonLSE = "SOMETHING ELSE";
			AssertEquals("MarksAndNumbersHeadingForNonLSE", "SOMETHING ELSE", Registry.MarksAndNumbersHeadingForNonLSE);
		}

		public void TestPDFTIFResolution()
		{
			AssertEquals("PDFTIFResolution", 200, Registry.PDFTIFResolution);
		}

		public void TestUserHelpMode()
		{
			// set this as the initial state
			Registry.UserHelpMode = HelpMode.Standard;
			AssertEquals("UserHelpMode", HelpMode.Standard, Registry.UserHelpMode);

			Registry.UserHelpMode = HelpMode.Expert;
			AssertEquals("UserHelpMode", HelpMode.Expert, Registry.UserHelpMode);

			Registry.UserHelpMode = HelpMode.Training;
			AssertEquals("UserHelpMode", HelpMode.Training, Registry.UserHelpMode);

			Registry.UserHelpMode = HelpMode.Standard;
			AssertEquals("UserHelpMode", HelpMode.Standard, Registry.UserHelpMode);
		}

		public void TestPDFTIFColourDepth()
		{
			Registry.RawRegistry.PDFTIFColourDepth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ColourDepth.BlackAndWhite);
			AssertEquals("PDFTIFColourDepth", ColourDepth.BlackAndWhite, Registry.PDFTIFColourDepth);

			Registry.RawRegistry.PDFTIFColourDepth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ColourDepth.TrueColour);
			AssertEquals("PDFTIFColourDepth", ColourDepth.TrueColour, Registry.PDFTIFColourDepth);
		}

		public void TestUserRunningUpgrade()
		{
			Registry.UserRunningUpgrade = EnvProxy.Instance.CurrentUser.PK;
			AssertEquals("UserRunningUpgrade", EnvProxy.Instance.CurrentUser.PK, Registry.UserRunningUpgrade);
		}

		public void TestUserEventTrackingEnterprise()
		{
			var item = Registry.RawRegistry.UserEventTrackingEnterprise;
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("fallback to enterprise level", false, Registry.UserEventTrackingEnterprise);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("fallback to enterprise level", true, Registry.UserEventTrackingEnterprise);

			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("follows company override", false, Registry.UserEventTrackingEnterprise);

			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("follows company override", true, Registry.UserEventTrackingEnterprise);
		}

		public void TestUserEventTrackingExternal()
		{
			var item = Registry.RawRegistry.UserEventTrackingExternal;
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("fallback to enterprise level", false, Registry.UserEventTrackingExternal);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("fallsback to enterprise level", true, Registry.UserEventTrackingExternal);

			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("follows company override", false, Registry.UserEventTrackingExternal);

			item.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("follows company override", true, Registry.UserEventTrackingExternal);
		}

		public void TestEnterpriseCDDate()
		{
			AssertEquals("EnterpriseCDDate", DateTime.MinValue, Registry.EnterpriseCDDate);

			DateTime testDate = new DateTime(2005, 10, 1);
			Registry.RawRegistry.EnterpriseCDDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDate);
			AssertEquals("EnterpriseCDDate", testDate, Registry.EnterpriseCDDate);
		}

		[ExpectNoExceptions()]
		public void TestEnterpriseCDDateThrowsNoException()
		{
			string sql = @"
			insert into dbo.StmData
			(sd_pk, sd_name, sd_binaryvalue)
			values (newid(),'EnterpriseCDDate', convert(varbinary(8000), N'10*07*1984 03:34:48'))";

			using (DbCommand cmd = Db.Connection.Command(sql))// This is the only way to put invalid date in
			{
				cmd.ExecuteNonQuery();
			}
			AssertEquals("EnterpriseCDDate", CargoWise.Types.ZDateTime.UtcNow.AddMonths(-3).Date.ToString("dd/MM/yyyy HH:mm:ss"), Registry.EnterpriseCDDate.Date.ToString("dd/MM/yyyy HH:mm:ss"));
		}

		public void TestRelatedNoteReadDelay()
		{
			AssertEquals("Registry.RelatedNoteReadDelay default value should be 2.", 2, Registry.RelatedNoteReadDelay);

			Registry.RawRegistry.RelatedNoteReadDelay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 0);
			AssertEquals("Registry.RelatedNoteReadDelay should be set to 0.", 0, Registry.RelatedNoteReadDelay);

			Registry.RawRegistry.RelatedNoteReadDelay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 10);
			AssertEquals("Registry.RelatedNoteReadDelay should be set to 10.", 10, Registry.RelatedNoteReadDelay);

			try
			{
				Registry.RawRegistry.RelatedNoteReadDelay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, -1);
				Fail("Registry.RelatedNoteReadDelay should throw an exception when set to -1.");
			}
			catch (RegistryValidationException) { }

			try
			{
				Registry.RawRegistry.RelatedNoteReadDelay.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 11);
				Fail("Registry.RelatedNoteReadDelay should throw an exception when set to 11.");
			}
			catch (RegistryValidationException) { }
		}

		public void TestDateTimeStaffFormWasLastShown()
		{
			DateTime dateTime = new DateTime(2005, 1, 1);
			Registry.RawRegistry.DateTimeStaffFormWasLastShown.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, dateTime);
			AssertEquals("DateTimeStaffFormWasLastShown", dateTime, Registry.DateTimeStaffFormWasLastShown);

			dateTime = new DateTime(2005, 2, 2);
			Registry.DateTimeStaffFormWasLastShown = dateTime;
			AssertEquals("DateTimeStaffFormWasLastShown", dateTime, Registry.DateTimeStaffFormWasLastShown);
			AssertEquals("Registry Item value", dateTime, Registry.RawRegistry.DateTimeStaffFormWasLastShown.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestMessagesPerInterchange()
		{
			AssertEquals("DefaultValue", 50, Registry.RawRegistry.MessagesPerInterchange.DefaultValue);
			Registry.RawRegistry.MessagesPerInterchange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AssertEquals("MessagesPerInterchange", 2, Registry.MessagesPerInterchange);
		}

		public void TestInterchangesPerRun()
		{
			AssertEquals("DefaultValue", 10, Registry.RawRegistry.InterchangesPerRun.DefaultValue);
			Registry.RawRegistry.InterchangesPerRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AssertEquals("InterchangesPerRun", 2, Registry.InterchangesPerRun);
		}

		public void TestDebugBusinessObjectType()
		{
			AssertEquals("DebugBusinessObjectType", "", Registry.DebugBusinessObjectType);
			Registry.DebugBusinessObjectType = "This a debug string";
			AssertEquals("DebugBusinessObjectType", "This a debug string", Registry.DebugBusinessObjectType);
		}

		public void TestExportGridLayoutDetails()
		{
			AssertEquals("ExportGridLayoutDetails", false, Registry.ExportGridLayoutDetails);
			Registry.ExportGridLayoutDetails = true;
			AssertEquals("ExportGridLayoutDetails", true, Registry.ExportGridLayoutDetails);
		}

		#region Document Opening/Closing Text

		public void TestAirAgentsInstruction()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.AirAgentsInstructionOpeningText, Registry.RawRegistry.AirAgentsInstructionClosingText, "AirAgentsInstruction");
		}

		public void TestAirWeightsAndMeasurements()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.AirWeightsAndMeasurementsOpeningText, Registry.RawRegistry.AirWeightsAndMeasurementsClosingText, "AirWeightsAndMeasurements");
		}

		public void TestBookingCartageAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.BookingCartageAdviceOpeningText, Registry.RawRegistry.BookingCartageAdviceClosingText, "BookingCartageAdvice");
		}

		public void TestLocalCartageCartageAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.LocalCartageCartageAdviceOpeningText, Registry.RawRegistry.LocalCartageCartageAdviceClosingText, "LocalCartageCartageAdvice");
		}

		public void TestCartageAdviceExport()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.CartageAdviceExportOpeningText, Registry.RawRegistry.CartageAdviceExportClosingText, "CartageAdviceExport");
		}

		public void TestCartageAdviceImport()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.CartageAdviceImportOpeningText, Registry.RawRegistry.CartageAdviceImportClosingText, "CartageAdviceImport");
		}

		public void TestCFSCartageAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.CFSCartageAdviceOpeningText, Registry.RawRegistry.CFSCartageAdviceClosingText, "CFSCartageAdvice");
		}

		public void TestCustomsDeclarationCartageAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.CustomsDeclarationCartageAdviceOpeningText, Registry.RawRegistry.CustomsDeclarationCartageAdviceClosingText, "CustomsDeclarationCartageAdvice");
		}

		public void TestCustomsRequestForMissingDocuments()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.CustomsRequestForMissingDocumentsOpeningText, Registry.RawRegistry.CustomsRequestForMissingDocumentsClosingText, "CustomsRequestForMissingDocuments");
		}

		public void TestSeaAgentsInstruction()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.SeaAgentsInstructionOpeningText, Registry.RawRegistry.SeaAgentsInstructionClosingText, "SeaAgentsInstruction");
		}

		public void TestSeaWeightsAndMeasurements()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.SeaWeightsAndMeasurementsOpeningText, Registry.RawRegistry.SeaWeightsAndMeasurementsClosingText, "SeaWeightsAndMeasurements");
		}

		public void TestAWBSecurityDeclaration()
		{
			Registry.RawRegistry.AWBSecurityDeclarationOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test");
			AssertEquals("Value", "Test", Registry.AWBSecurityDeclaration.OpeningText);

			Registry.AWBSecurityDeclaration = new DocumentOpenCloseText("Opening Test", string.Empty);
			AssertEquals("Value", "Opening Test", Registry.AWBSecurityDeclaration.OpeningText);
		}

		#region Export Air

		public void TestExportAirBookingConfirmation()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportAirBookingConfirmationOpeningText, Registry.RawRegistry.ExportAirBookingConfirmationClosingText, "ExportAirBookingConfirmation");
		}

		public void TestExportAirFreightAgentDepartureNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportAirFreightAgentDepartureNoticeOpeningText, Registry.RawRegistry.ExportAirFreightAgentDepartureNoticeClosingText, "ExportAirFreightAgentDepartureNotice");
		}

		public void TestExportAirFreightShipperDepartureNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportAirFreightShipperDepartureNoticeOpeningText, Registry.RawRegistry.ExportAirFreightShipperDepartureNoticeClosingText, "ExportAirFreightShipperDepartureNotice");
		}

		public void TestExportAirLetterToOverseasAgent()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportAirLetterToOverseasAgentOpeningText, Registry.RawRegistry.ExportAirLetterToOverseasAgentClosingText, "ExportAirLetterToOverseasAgent");
		}

		#endregion

		#region Export Order

		public void TestExportOrderAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportOrderAdviceOpeningText, Registry.RawRegistry.ExportOrderAdviceClosingText, "ExportOrderAdvice");
		}

		public void TestExportOrderNotification()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportOrderNotificationOpeningText, Registry.RawRegistry.ExportOrderNotificationClosingText, "ExportOrderNotification");
		}

		public void TestExportOrderStatus()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportOrderStatusOpeningText, Registry.RawRegistry.ExportOrderStatusClosingText, "ExportOrderStatus");
		}

		#endregion

		#region Export Sea

		public void TestExportSeaBookingConfirmation()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportSeaBookingConfirmationOpeningText, Registry.RawRegistry.ExportSeaBookingConfirmationClosingText, "ExportSeaBookingConfirmation");
		}

		public void TestExportSeaFreightAgentDepartureNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportSeaFreightAgentDepartureNoticeOpeningText, Registry.RawRegistry.ExportSeaFreightAgentDepartureNoticeClosingText, "ExportSeaFreightAgentDepartureNotice");
		}

		public void TestExportSeaFreightShipperDepartureNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportSeaFreightShipperDepartureNoticeOpeningText, Registry.RawRegistry.ExportSeaFreightShipperDepartureNoticeClosingText, "ExportSeaFreightShipperDepartureNotice");
		}

		public void TestExportSeaLetterToOverseasAgent()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ExportSeaLetterToOverseasAgentOpeningText, Registry.RawRegistry.ExportSeaLetterToOverseasAgentClosingText, "ExportSeaLetterToOverseasAgent");
		}

		#endregion

		#region Import Air

		public void TestImportAirArrivalNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportAirArrivalNoticeOpeningText, Registry.RawRegistry.ImportAirArrivalNoticeClosingText, "ImportAirArrivalNotice");
		}

		public void TestImportAirPreAlert()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportAirPreAlertOpeningText, Registry.RawRegistry.ImportAirPreAlertClosingText, "ImportAirPreAlert");
		}

		public void TestImportAirOutturnReport()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportAirOutturnReportOpeningText, Registry.RawRegistry.ImportAirOutturnReportClosingText, "ImportAirOutturnReport");
		}

		public void TestImportAirShippingAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportAirShippingAdviceOpeningText, Registry.RawRegistry.ImportAirShippingAdviceClosingText, "ImportAirShippingAdvice");
		}

		public void TestImportAirUltimateConsigneeArrivalNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportAirUltimateConsigneeArrivalNoticeOpeningText, Registry.RawRegistry.ImportAirUltimateConsigneeArrivalNoticeClosingText, "ImportAirUltimateConsigneeArrivalNotice");
		}

		public void TestImportAirUltimateConsigneePreAlert()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportAirUltimateConsigneePreAlertOpeningText, Registry.RawRegistry.ImportAirUltimateConsigneePreAlertClosingText, "ImportAirUltimateConsigneePreAlert");
		}

		#endregion

		#region Import Order

		public void TestImportOrderAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportOrderAdviceOpeningText, Registry.RawRegistry.ImportOrderAdviceClosingText, "ImportOrderAdvice");
		}

		public void TestImportOrderNotification()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportOrderNotificationOpeningText, Registry.RawRegistry.ImportOrderNotificationClosingText, "ImportOrderNotification");
		}

		public void TestImportOrderStatus()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportOrderStatusOpeningText, Registry.RawRegistry.ImportOrderStatusClosingText, "ImportOrderStatus");
		}

		#endregion

		#region Import Sea

		public void TestImportSeaFreightArrivalNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaFreightArrivalNoticeOpeningText, Registry.RawRegistry.ImportSeaFreightArrivalNoticeClosingText, "ImportSeaFreightArrivalNotice");
		}

		public void TestImportSeaFreightDeliveryOrder()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaFreightDeliveryOrderOpeningText, Registry.RawRegistry.ImportSeaFreightDeliveryOrderClosingText, "ImportSeaFreightDeliveryOrder");
		}

		public void TestImportSeaFreightPreAlert()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaFreightPreAlertOpeningText, Registry.RawRegistry.ImportSeaFreightPreAlertClosingText, "ImportSeaFreightPreAlert");
		}

		public void TestImportSeaOutturnReport()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaOutturnReportOpeningText, Registry.RawRegistry.ImportSeaOutturnReportClosingText, "ImportSeaOutturnReport");
		}

		public void TestImportSeaShippingAdvice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaShippingAdviceOpeningText, Registry.RawRegistry.ImportSeaShippingAdviceClosingText, "ImportSeaShippingAdvice");
		}

		public void TestImportSeaUltimateConsigneeArrivalNotice()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaUltimateConsigneeArrivalNoticeOpeningText, Registry.RawRegistry.ImportSeaUltimateConsigneeArrivalNoticeClosingText, "ImportSeaUltimateConsigneeArrivalNotice");
		}

		public void TestImportSeaUltimateConsigneePreAlert()
		{
			TestDocumentOpeningAndClosingText(Registry.RawRegistry.ImportSeaUltimateConsigneePreAlertOpeningText, Registry.RawRegistry.ImportSeaUltimateConsigneePreAlertClosingText, "ImportSeaUltimateConsigneePreAlert");
		}

		#endregion

		void TestDocumentOpeningAndClosingText(IRegistryItem openingTextRegistryItem, IRegistryItem closingTextRegistryItem, string openCloseTextPropertyName)
		{
			PropertyInfo propertyInfo = typeof(DataRegistry).GetProperty(openCloseTextPropertyName);
			DocumentOpenCloseText openCloseText = (DocumentOpenCloseText)propertyInfo.GetValue(Registry, null);

			AssertEquals("OpeningText", string.Empty, openCloseText.OpeningText);
			AssertEquals("ClosingText", string.Empty, openCloseText.ClosingText);

			openingTextRegistryItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Opening");
			closingTextRegistryItem.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Closing");

			openCloseText = (DocumentOpenCloseText)propertyInfo.GetValue(Registry, null);

			AssertEquals("OpeningText", "Opening", openCloseText.OpeningText);
			AssertEquals("ClosingText", "Closing", openCloseText.ClosingText);
		}

		#endregion

		#region Order Documents

		public void TestOrderAdvice()
		{
			Registry.RawRegistry.ImportOrderAdviceOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Open");
			AssertEquals("OpeningText", "Open", Registry.ImportOrderAdvice.OpeningText);

			Registry.RawRegistry.ImportOrderAdviceClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Close");
			AssertEquals("ClosingText", "Close", Registry.ImportOrderAdvice.ClosingText);

			Registry.RawRegistry.ExportOrderAdviceOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Open");
			AssertEquals("OpeningText", "Open", Registry.ExportOrderAdvice.OpeningText);

			Registry.RawRegistry.ExportOrderAdviceClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Close");
			AssertEquals("ClosingText", "Close", Registry.ExportOrderAdvice.ClosingText);
		}

		public void TestOrderNotification()
		{
			Registry.RawRegistry.ImportOrderNotificationOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Open");
			AssertEquals("OpeningText", "Open", Registry.ImportOrderNotification.OpeningText);

			Registry.RawRegistry.ImportOrderNotificationClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Close");
			AssertEquals("ClosingText", "Close", Registry.ImportOrderNotification.ClosingText);

			Registry.RawRegistry.ExportOrderNotificationOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Open");
			AssertEquals("OpeningText", "Open", Registry.ExportOrderNotification.OpeningText);

			Registry.RawRegistry.ExportOrderNotificationClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Close");
			AssertEquals("ClosingText", "Close", Registry.ExportOrderNotification.ClosingText);
		}

		public void TestOrderStatus()
		{
			Registry.RawRegistry.ImportOrderStatusOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Open");
			AssertEquals("OpeningText", "Open", Registry.ImportOrderStatus.OpeningText);

			Registry.RawRegistry.ImportOrderStatusClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Close");
			AssertEquals("ClosingText", "Close", Registry.ImportOrderStatus.ClosingText);

			Registry.RawRegistry.ExportOrderStatusOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Open");
			AssertEquals("OpeningText", "Open", Registry.ExportOrderStatus.OpeningText);

			Registry.RawRegistry.ExportOrderStatusClosingText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Close");
			AssertEquals("ClosingText", "Close", Registry.ExportOrderStatus.ClosingText);
		}

		#endregion

		#region DocumentScanningTests
		public void TestBurnCDSettingsStruct()
		{
			BurnCDSettingsStruct testSettings = new BurnCDSettingsStruct();
			testSettings.DeleteSourceDir = true;
			testSettings.BurnDriveLetter = 'd';
			testSettings.WriteSpeedTimes = 4;

			BurnCDSettingsStruct anotherSettings = new BurnCDSettingsStruct(testSettings.GetValue());
			AssertEquals("DeleteSourceDir", testSettings.DeleteSourceDir, anotherSettings.DeleteSourceDir);
			AssertEquals("BurnDriveLetter", testSettings.BurnDriveLetter, anotherSettings.BurnDriveLetter);
			AssertEquals("WriteSpeedTimes", testSettings.WriteSpeedTimes, anotherSettings.WriteSpeedTimes);

			Registry.BurnCDSettings = testSettings;
			BurnCDSettingsStruct retrievedSettings = Registry.BurnCDSettings;
			AssertEquals("BurnDriveLetter", testSettings.BurnDriveLetter, retrievedSettings.BurnDriveLetter);
		}

		public void TestDMMagnifyingGlassSettingsStruct()
		{
			DMMagnifyingGlassSettingsStruct testSettings = new DMMagnifyingGlassSettingsStruct();
			testSettings.MagnificationPercentage = 25;

			DMMagnifyingGlassSettingsStruct anotherSettings = new DMMagnifyingGlassSettingsStruct(testSettings.GetValue());
			AssertEquals("Magnification Percentage", testSettings.MagnificationPercentage, anotherSettings.MagnificationPercentage);

			Registry.DMMagnifyingGlassSettings = testSettings;
			DMMagnifyingGlassSettingsStruct retrievedSettings = Registry.DMMagnifyingGlassSettings;
			AssertEquals("Magnification Percentage", testSettings.MagnificationPercentage, retrievedSettings.MagnificationPercentage);
		}

		public void TestDMThumbnailSettingsStruct()
		{
			DMThumbnailSettingsStruct testSettings = new DMThumbnailSettingsStruct();
			testSettings.NumberOfThumbnails = 5;
			testSettings.ThumbNailViewActive = true;

			DMThumbnailSettingsStruct anotherSettings = new DMThumbnailSettingsStruct(testSettings.GetValue());
			AssertEquals("Number Of Thumbnails", testSettings.NumberOfThumbnails, anotherSettings.NumberOfThumbnails);
			AssertEquals("Thumb Nail View Active", testSettings.ThumbNailViewActive, anotherSettings.ThumbNailViewActive);

			Registry.DMThumbnailSettings = testSettings;
			DMThumbnailSettingsStruct retrievedSettings = Registry.DMThumbnailSettings;
			AssertEquals("Number Of Thumbnails", testSettings.NumberOfThumbnails, retrievedSettings.NumberOfThumbnails);
			AssertEquals("Thumb Nail View Active", testSettings.ThumbNailViewActive, retrievedSettings.ThumbNailViewActive);
		}

		public void TestDMImportConfigurationSettingsStruct()
		{
			DMImportConfigurationSettingsStruct testSettings = new DMImportConfigurationSettingsStruct();
			testSettings.DefaultDirectory = @"c:\hello\world"; // This is just for testing
			testSettings.PDFColourOption = "Black & White";

			DMImportConfigurationSettingsStruct anotherSettings = new DMImportConfigurationSettingsStruct(testSettings.GetValue());
			AssertEquals("Default Directory", testSettings.DefaultDirectory, anotherSettings.DefaultDirectory);
			AssertEquals("PDFColour ", testSettings.PDFColourOption, anotherSettings.PDFColourOption);

			Registry.DMImportConfigurationSettings = testSettings;
			DMImportConfigurationSettingsStruct retrievedSettings = Registry.DMImportConfigurationSettings;
			AssertEquals("Default Directory", testSettings.DefaultDirectory, retrievedSettings.DefaultDirectory);
		}

		public void TestGetDMImportConfigurationSettingsStruct()
		{
			Guid staffPK = Guid.NewGuid();
			DMImportConfigurationSettingsStruct newUserSettings = new DMImportConfigurationSettingsStruct(null);
			newUserSettings.OutputOption = "AutomaticSingle";
			newUserSettings.DefaultDirectory = @"C:\Test"; // This is just for testing
			Registry.RawRegistry.DMImportConfigurationSettings.SetValue(staffPK, Guid.Empty, Guid.Empty, newUserSettings.GetValue());

			DMImportConfigurationSettingsStruct retrievedSettings = EnvProxy.Instance.Registry.GetDMImportConfigurationSettings(staffPK);
			AssertEquals("Test directory should match when retrieved using GetDMImport..", @"C:\Test", retrievedSettings.DefaultDirectory); // This is just for testing
			AssertEquals("Output Option should match when retrieved using GetDMImport..", "AutomaticSingle", retrievedSettings.OutputOption);

			DMImportConfigurationSettingsStruct currentUserSettings = EnvProxy.Instance.Registry.GetDMImportConfigurationSettings(EnvProxy.Instance.CurrentUser.PK);
			AssertEquals("check retrieve of currently logged in user works - all options retrieved should match", currentUserSettings.AutoAllocate, EnvProxy.Instance.Registry.DMImportConfigurationSettings.AutoAllocate);
			AssertEquals("check retrieve of currently logged in user works - all options retrieved should match", currentUserSettings.DefaultDirectory, EnvProxy.Instance.Registry.DMImportConfigurationSettings.DefaultDirectory);
			AssertEquals("check retrieve of currently logged in user works - all options retrieved should match", currentUserSettings.DeleteSourceFilesAfterImport, EnvProxy.Instance.Registry.DMImportConfigurationSettings.DeleteSourceFilesAfterImport);
			AssertEquals("check retrieve of currently logged in user works - all options retrieved should match", currentUserSettings.IncludeSubdirectories, EnvProxy.Instance.Registry.DMImportConfigurationSettings.IncludeSubdirectories);
			AssertEquals("check retrieve of currently logged in user works - all options retrieved should match", currentUserSettings.OutputOption, EnvProxy.Instance.Registry.DMImportConfigurationSettings.OutputOption);
			AssertEquals("check retrieve of currently logged in user works - all options retrieved should match", currentUserSettings.PDFColourOption, EnvProxy.Instance.Registry.DMImportConfigurationSettings.PDFColourOption);
		}

		public void TestDMImportConfigurationSettingsStructDefaultValues()
		{
			DMImportConfigurationSettingsStruct @default = new DMImportConfigurationSettingsStruct(null);
			Assert(@default.AutoAllocate);
			AssertEquals(@"C:\", @default.DefaultDirectory); // This is just for testing
			Assert(!@default.DeleteSourceFilesAfterImport);
			Assert(@default.IncludeSubdirectories);
			AssertEquals("Automatic", @default.OutputOption);
			AssertEquals("Black & White", @default.PDFColourOption);

			byte[] value = @default.GetValue();

			DMImportConfigurationSettingsStruct newValue = new DMImportConfigurationSettingsStruct(value);
			AssertEquals(@default.AutoAllocate, newValue.AutoAllocate);
			AssertEquals(@default.DefaultDirectory, newValue.DefaultDirectory);
			AssertEquals(@default.DeleteSourceFilesAfterImport, newValue.DeleteSourceFilesAfterImport);
			AssertEquals(@default.IncludeSubdirectories, newValue.IncludeSubdirectories);
			AssertEquals(@default.OutputOption, newValue.OutputOption);
			AssertEquals(@default.PDFColourOption, newValue.PDFColourOption);

			Registry.DMImportConfigurationSettings = @default;
			DMImportConfigurationSettingsStruct retrievedSettings = Registry.DMImportConfigurationSettings;
			AssertEquals(@default.AutoAllocate, retrievedSettings.AutoAllocate);
			AssertEquals(@default.DefaultDirectory, retrievedSettings.DefaultDirectory);
			AssertEquals(@default.DeleteSourceFilesAfterImport, retrievedSettings.DeleteSourceFilesAfterImport);
			AssertEquals(@default.IncludeSubdirectories, retrievedSettings.IncludeSubdirectories);
			AssertEquals(@default.OutputOption, retrievedSettings.OutputOption);
			AssertEquals(@default.PDFColourOption, retrievedSettings.PDFColourOption);
		}

		public void TestDMScanningFileOutputOptionSettingsStruct()
		{
			DMScanningFileOutputOptionSettingsStruct testSettings = new DMScanningFileOutputOptionSettingsStruct();
			testSettings.OutputOption = "SomeOption";
			testSettings.AutoAllocate = false;

			DMScanningFileOutputOptionSettingsStruct anotherSettings = new DMScanningFileOutputOptionSettingsStruct(testSettings.GetValue());
			AssertEquals("Output Option", testSettings.OutputOption, anotherSettings.OutputOption);
			AssertEquals("Auto Allocate", testSettings.AutoAllocate, anotherSettings.AutoAllocate);

			Registry.DMScanningFileOutputOptionSettings = testSettings;
			DMScanningFileOutputOptionSettingsStruct retrievedSettings = Registry.DMScanningFileOutputOptionSettings;
			AssertEquals("Output Option", testSettings.OutputOption, retrievedSettings.OutputOption);
		}

		public void TestDMScanningFileOutputOptionSettingsStructDefaultValues()
		{
			DMScanningFileOutputOptionSettingsStruct @default = new DMScanningFileOutputOptionSettingsStruct(null);
			Assert(@default.AutoAllocate);
			AssertEquals("Automatic", @default.OutputOption);

			byte[] value = @default.GetValue();

			DMScanningFileOutputOptionSettingsStruct newValue = new DMScanningFileOutputOptionSettingsStruct(value);
			AssertEquals(@default.AutoAllocate, newValue.AutoAllocate);
			AssertEquals(@default.OutputOption, newValue.OutputOption);

			Registry.DMScanningFileOutputOptionSettings = @default;
			DMScanningFileOutputOptionSettingsStruct retrievedSettings = Registry.DMScanningFileOutputOptionSettings;
			AssertEquals(@default.AutoAllocate, retrievedSettings.AutoAllocate);
			AssertEquals(@default.OutputOption, retrievedSettings.OutputOption);
		}

		public void TestAUCustomsToleranceAmount()
		{
			Registry.AUCustomsToleranceAmount = 100M;
			AssertEquals("AUCustomsToleranceAmount", 100M, Registry.AUCustomsToleranceAmount);
		}

		public void TestAUCustomsRefundToleranceAmount()
		{
			AssertEquals("Default Value", 2M, Registry.RawRegistry.AUCustomsRefundToleranceAmount.DefaultValue);
			Registry.AUCustomsRefundToleranceAmount = 100M;
			AssertEquals("AUCustomsRefundToleranceAmount", 100M, Registry.AUCustomsRefundToleranceAmount);
		}

		public void TestAUCustomsTolerancePercentage()
		{
			Registry.AUCustomsTolerancePercentage = 10;
			AssertEquals("AUCustomsTolerancePercentage", 10, Registry.AUCustomsTolerancePercentage);
		}

		public void TestAUCustomsSenderID()
		{
			Registry.AUCustomsSenderID = "123";
			AssertEquals("AUCustomsSenderID", "123", Registry.AUCustomsSenderID);
			AssertEquals("AUCustomsSenderID", "123", Registry.GetAUCustomsSenderID(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestAUCustomsEdificeSenderID()
		{
			Registry.AUCustomsEdificeSenderID = "123";
			AssertEquals("AUCustomsEdificeSenderID", "123", Registry.AUCustomsEdificeSenderID);
			AssertEquals("AUCustomsEdificeSenderID", "123", Registry.GetAUCustomsEdificeSenderID(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestAUCustomsCompileSiteID()
		{
			Guid branchPK = Guid.NewGuid();
			Registry.SetAUCustomsCompileSiteID(branchPK, "site12");
			AssertEquals("AUCustomsEdificeSenderID", "site12", Registry.AUCustomsCompileSiteID(branchPK));
		}

		public void TestCustomsPaymentBankAccount()
		{
			Guid bankAccountPK = Guid.NewGuid();
			Registry.SetCustomsPaymentBankAccountForCurrentCompany(bankAccountPK);
			AssertEquals("CustomsPaymentBankAccount", bankAccountPK, Registry.CustomsPaymentBankAccount);
		}

		public void TestCustomsSecondPaymentBankAccount()
		{
			Guid bankAccountPK = Guid.NewGuid();
			Registry.SetCustomsSecondPaymentBankAccountForCurrentCompany(bankAccountPK);
			AssertEquals("CustomsSecondPaymentBankAccount", bankAccountPK, Registry.CustomsSecondPaymentBankAccount);
		}

		public void TestPreAlertWeightAndVolumeDisplay()
		{
			AssertEquals("PreAlertWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.PreAlertWeightAndVolumeDisplay);
			Registry.RawRegistry.PreAlertWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("PreAlertWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.PreAlertWeightAndVolumeDisplay);

			using (Registry.RawRegistry.PreAlertWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.PreAlertWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("PreAlertWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.PreAlertWeightAndVolumeDisplay);
		}

		public void TestArrivalNoticeWeightAndVolumeDisplay()
		{
			AssertEquals("ArrivalNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ArrivalNoticeWeightAndVolumeDisplay);
			Registry.RawRegistry.ArrivalNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ArrivalNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ArrivalNoticeWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ArrivalNoticeWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ArrivalNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ArrivalNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ArrivalNoticeWeightAndVolumeDisplay);
		}

		public void TestShippingAdviceWeightAndVolumeDisplay()
		{
			AssertEquals("ShippingAdviceWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ShippingAdviceWeightAndVolumeDisplay);
			Registry.RawRegistry.ShippingAdviceWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ShippingAdviceWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ShippingAdviceWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ShippingAdviceWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ShippingAdviceWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ShippingAdviceWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ShippingAdviceWeightAndVolumeDisplay);
		}

		public void TestDeliveryOrderWeightAndVolumeDisplay()
		{
			AssertEquals("DeliveryOrderWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.DeliveryOrderWeightAndVolumeDisplay);
			Registry.RawRegistry.DeliveryOrderWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("DeliveryOrderWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.DeliveryOrderWeightAndVolumeDisplay);

			using (Registry.RawRegistry.DeliveryOrderWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.DeliveryOrderWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("DeliveryOrderWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.DeliveryOrderWeightAndVolumeDisplay);
		}

		public void TestOutturnReportWeightAndVolumeDisplay()
		{
			AssertEquals("OutturnReportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.OutturnReportWeightAndVolumeDisplay);
			Registry.RawRegistry.OutturnReportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("OutturnReportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.OutturnReportWeightAndVolumeDisplay);

			using (Registry.RawRegistry.OutturnReportWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.OutturnReportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("OutturnReportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.OutturnReportWeightAndVolumeDisplay);
		}

		public void TestBookingConfirmationWeightAndVolumeDisplay()
		{
			AssertEquals("BookingConfirmationWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.BookingConfirmationWeightAndVolumeDisplay);
			Registry.RawRegistry.BookingConfirmationWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("BookingConfirmationWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.BookingConfirmationWeightAndVolumeDisplay);

			using (Registry.RawRegistry.BookingConfirmationWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.BookingConfirmationWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("BookingConfirmationWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.BookingConfirmationWeightAndVolumeDisplay);
		}

		public void TestShipperDepartureNoticeWeightAndVolumeDisplay()
		{
			AssertEquals("ShipperDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ShipperDepartureNoticeWeightAndVolumeDisplay);
			Registry.RawRegistry.ShipperDepartureNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ShipperDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ShipperDepartureNoticeWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ShipperDepartureNoticeWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ShipperDepartureNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ShipperDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ShipperDepartureNoticeWeightAndVolumeDisplay);
		}

		public void TestAgentsInstructionNoticeWeightAndVolumeDisplay()
		{
			AssertEquals("AgentsInstructionNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.AgentsInstructionNoticeWeightAndVolumeDisplay);
			Registry.RawRegistry.AgentsInstructionNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("AgentsInstructionNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.AgentsInstructionNoticeWeightAndVolumeDisplay);

			using (Registry.RawRegistry.AgentsInstructionNoticeWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.AgentsInstructionNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("AgentsInstructionNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.AgentsInstructionNoticeWeightAndVolumeDisplay);
		}

		public void TestCartageAdviceImportWeightAndVolumeDisplay()
		{
			AssertEquals("CartageAdviceImportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.CartageAdviceImportWeightAndVolumeDisplay);
			Registry.RawRegistry.CartageAdviceImportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("CartageAdviceImportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.CartageAdviceImportWeightAndVolumeDisplay);

			using (Registry.RawRegistry.CartageAdviceImportWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.CartageAdviceImportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("CartageAdviceImportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.CartageAdviceImportWeightAndVolumeDisplay);
		}

		public void TestCartageAdviceExportWeightAndVolumeDisplay()
		{
			AssertEquals("CartageAdviceExportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.CartageAdviceExportWeightAndVolumeDisplay);
			Registry.RawRegistry.CartageAdviceExportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("CartageAdviceExportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.CartageAdviceExportWeightAndVolumeDisplay);

			using (Registry.RawRegistry.CartageAdviceExportWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.CartageAdviceExportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("CartageAdviceExportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.CartageAdviceExportWeightAndVolumeDisplay);
		}

		public void TestBillOfLadingWeightAndVolumeDisplay()
		{
			AssertEquals("BillOfLadingWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.BillOfLadingWeightAndVolumeDisplay);
			Registry.RawRegistry.BillOfLadingWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("BillOfLadingWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.BillOfLadingWeightAndVolumeDisplay);

			using (Registry.RawRegistry.BillOfLadingWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.BillOfLadingWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("BillOfLadingWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.BillOfLadingWeightAndVolumeDisplay);
		}

		public void TestCoLoadMasterManifestWeightAndVolumeDisplay()
		{
			AssertEquals("CoLoadMasterManifestWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.CoLoadMasterManifestWeightAndVolumeDisplay);
			Registry.RawRegistry.CoLoadMasterManifestWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("CoLoadMasterManifestWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.CoLoadMasterManifestWeightAndVolumeDisplay);

			using (Registry.RawRegistry.CoLoadMasterManifestWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.CoLoadMasterManifestWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("CoLoadMasterManifestWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.CoLoadMasterManifestWeightAndVolumeDisplay);
		}

		public void TestCoLoadMasterManifestDisplayVolumeWhenAir()
		{
			AssertEquals("CoLoadMasterManifestDisplayVolumeWhenAir", true, Registry.CoLoadMasterManifestDisplayVolumeWhenAir);
			Registry.RawRegistry.CoLoadMasterManifestDisplayVolumeWhenAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CoLoadMasterManifestDisplayVolumeWhenAir", false, Registry.CoLoadMasterManifestDisplayVolumeWhenAir);
		}

		public void TestCoLoadMasterManifestDisplayChargeableWhenAir()
		{
			AssertEquals("CoLoadMasterManifestDisplayChargeableWhenAir", true, Registry.CoLoadMasterManifestDisplayChargeableWhenAir);
			Registry.RawRegistry.CoLoadMasterManifestDisplayChargeableWhenAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CoLoadMasterManifestDisplayChargeableWhenAir", false, Registry.CoLoadMasterManifestDisplayChargeableWhenAir);
		}

		public void TestCoLoadMasterManifestDisplayChargeableWhenSea()
		{
			AssertEquals("CoLoadMasterManifestDisplayChargeableWhenSea", true, Registry.CoLoadMasterManifestDisplayChargeableWhenSea);
			Registry.RawRegistry.CoLoadMasterManifestDisplayChargeableWhenSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CoLoadMasterManifestDisplayChargeableWhenSea", false, Registry.CoLoadMasterManifestDisplayChargeableWhenSea);
		}

		public void TestShipmentRequestForMissingDocuments()
		{
			Registry.RawRegistry.ShipmentRequestForMissingDocumentsOpeningText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Opening Text");
			AssertEquals("OpeningText", "Opening Text", Registry.ShipmentRequestForMissingDocuments.OpeningText);

			Registry.RawRegistry.ShipmentRequestForMissingDocumentsClosingText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Closing Text");
			AssertEquals("ClosingText", "Closing Text", Registry.ShipmentRequestForMissingDocuments.ClosingText);

			Registry.RawRegistry.ShipmentRequestForMissingDocumentsClause.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Clause");
			AssertEquals("Clause", "Clause", Registry.ShipmentRequestForMissingDocumentsClause);
		}

		public void TestAuthorisationReleaseClause()
		{
			Registry.RawRegistry.AuthorisationReleaseClause.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("AuthorisationReleaseClause", "abc", Registry.AuthorisationReleaseClause);

			Registry.RawRegistry.AuthorisationReleaseClause.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "xyz");
			AssertEquals("AuthorisationReleaseClause", "xyz", Registry.AuthorisationReleaseClause);
		}

		public void TestConsolManifestConsolImportWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolManifestConsolImportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolManifestConsolImportWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolManifestConsolImportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolManifestConsolImportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolManifestConsolImportWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolManifestConsolImportWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolManifestConsolImportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolManifestConsolImportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolManifestConsolImportWeightAndVolumeDisplay);
		}

		public void TestConsolManifestConsolExportWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolManifestConsolExportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolManifestConsolExportWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolManifestConsolExportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolManifestConsolExportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolManifestConsolExportWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolManifestConsolExportWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolManifestConsolExportWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolManifestConsolExportWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolManifestConsolExportWeightAndVolumeDisplay);
		}

		public void TestConsolManifestConsolExportDisplayVolumewhenAir()
		{
			AssertEquals("ConsolManifestConsolExportDisplayVolumewhenAir", true, Registry.ConsolManifestConsolExportDisplayVolumewhenAir);
			Registry.RawRegistry.ConsolManifestConsolExportDisplayVolumewhenAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ConsolManifestConsolExportDisplayVolumewhenAir", false, Registry.ConsolManifestConsolExportDisplayVolumewhenAir);
		}

		public void TestConsolManifestConsolExportDisplayChargeableWhenAir()
		{
			AssertEquals("ConsolManifestConsolExportDisplayChargeableWhenAir", true, Registry.ConsolManifestConsolExportDisplayChargeableWhenAir);
			Registry.RawRegistry.ConsolManifestConsolExportDisplayChargeableWhenAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ConsolManifestConsolExportDisplayChargeableWhenAir", false, Registry.ConsolManifestConsolExportDisplayChargeableWhenAir);
		}

		public void TestConsolManifestConsolExportDisplayChargeableWhenSea()
		{
			AssertEquals("ConsolManifestConsolExportDisplayChargeableWhenSea", true, Registry.ConsolManifestConsolExportDisplayChargeableWhenSea);
			Registry.RawRegistry.ConsolManifestConsolExportDisplayChargeableWhenSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ConsolManifestConsolExportDisplayChargeableWhenSea", false, Registry.ConsolManifestConsolExportDisplayChargeableWhenSea);
		}

		public void TestConsolForwardingInstructionWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolForwardingInstructionWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolForwardingInstructionWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolForwardingInstructionWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolForwardingInstructionWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolForwardingInstructionWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolForwardingInstructionWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolForwardingInstructionWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolForwardingInstructionWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolForwardingInstructionWeightAndVolumeDisplay);
		}

		public void TestConsolShipperDepartureNoticeWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolShipperDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolShipperDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolShipperDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolShipperDepartureNoticeWeightAndVolumeDisplay);
		}

		public void TestConsolAgentDepartureNoticeWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolAgentDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolAgentDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolAgentDepartureNoticeWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolAgentDepartureNoticeWeightAndVolumeDisplay);
		}

		public void TestConsolCargoLoadListWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolCargoLoadListWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolCargoLoadListWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolCargoLoadListWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolCargoLoadListWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolCargoLoadListWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolCargoLoadListWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolCargoLoadListWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolCargoLoadListWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolCargoLoadListWeightAndVolumeDisplay);
		}

		public void TestConsolLetterToOverseasAgentWeightAndVolumeDisplay()
		{
			AssertEquals("ConsolLetterToOverseasAgentWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay);
			Registry.RawRegistry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("ConsolLetterToOverseasAgentWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Client, Registry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay);

			using (Registry.RawRegistry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay.DataType.SuspendValidation())
			{
				Registry.RawRegistry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			AssertEquals("ConsolLetterToOverseasAgentWeightAndVolumeDisplay", WeightAndVolumeDisplayTypes.Codes.Actual, Registry.ConsolLetterToOverseasAgentWeightAndVolumeDisplay);
		}

		public void TestNotifyPartyDefaultText()
		{
			Registry.NotifyPartyDefaultText = "Test";
			AssertEquals("NotifyPartyDefaultText", "Test", Registry.NotifyPartyDefaultText);
		}

		public void TestWeightMinimumDecimalPlacesToDisplay()
		{
			Registry.RawRegistry.WeightMinimumDecimalPlacesToDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AssertEquals("WeightMinimumDecimalPlacesToDisplay", 2, Registry.WeightMinimumDecimalPlacesToDisplay);
		}

		public void TestVolumeMinimumDecimalPlacesToDisplay()
		{
			Registry.RawRegistry.VolumeMinimumDecimalPlacesToDisplay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			AssertEquals("VolumeMinimumDecimalPlacesToDisplay", 2, Registry.VolumeMinimumDecimalPlacesToDisplay);
		}

		public void TestDisplayTareAndGrossWeightOnHBOL()
		{
			Registry.RawRegistry.DisplayTareAndGrossWeightOnHBOL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("DisplayTareAndGrossWeightOnHBOL", true, Registry.DisplayTareAndGrossWeightOnHBOL);
		}

		public void TestExcelPrinterInternationalFormats()
		{
			Registry.RawRegistry.ExcelPrinterInternationalFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "some \u5656 value");
			AssertEquals("Get stored value", "some \u5656 value", Registry.ExcelPrinterInternationalFormats);
		}

		public void TestCoverPageText()
		{
			Registry.RawRegistry.CoverPageText.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Cover Page Text");
			AssertEquals("Value", "Cover Page Text", Registry.CoverPageText);
		}

		public void TestCoverPageTextOneOff()
		{
			Registry.RawRegistry.CoverPageTextOneOff.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Cover Page Text One Off");
			AssertEquals("Value", "Cover Page Text One Off", Registry.CoverPageTextOneOff);
		}

		public void TestCoverPageTextNew()
		{
			Registry.RawRegistry.CoverPageTextNew.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Cover Page Text New");
			AssertEquals("Value", "Cover Page Text New", Registry.CoverPageTextNew);
		}

		public void TestCoverPageTextOneOffNew()
		{
			Registry.RawRegistry.CoverPageTextNew.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Cover Page Text One Off New");
			AssertEquals("Value", "Cover Page Text One Off New", Registry.CoverPageTextNew);
		}

		#endregion

		public void TestPackageVolumeUnit()
		{
			AssertEquals("PackageVolumeUnit", "M3", Registry.PackageVolumeUnit);
		}

		public void TestPackageWeightUnit()
		{
			AssertEquals("PackageWeightUnit", "KG", Registry.PackageWeightUnit);
		}

		public void TestUnitConversionPackTypesValidation()
		{
			Registry.RawRegistry.UnitConversionPackTypesValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("UnitConversionPackTypesValidation", false, Registry.UnitConversionPackTypesValidation);
			Registry.RawRegistry.UnitConversionPackTypesValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("UnitConversionPackTypesValidation", true, Registry.UnitConversionPackTypesValidation);
		}

		public void TestFaxDestinationOverride()
		{
			AssertNotNull("FaxDestinationOverride", Registry.FaxDestinationOverride);
		}

		public void TestEmailDestinationOverride()
		{
			AssertNotNull("EmailDestinationOverride", Registry.EmailDestinationOverride);
		}

		public void TestSystemEmailDestinationOverride()
		{
			Registry.RawRegistry.SystemEmailDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.SystemEmailDestinationOverride);
			Registry.RawRegistry.SystemEmailDestinationOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.SystemEmailDestinationOverride);
		}

		public void TestDefaultDepotForBuildHVLV()
		{
			AssertEquals(Guid.Empty, Registry.DefaultDepotForBuildHVLV);
			var depotGuid = Guid.NewGuid();
			Registry.DefaultDepotForBuildHVLV = depotGuid;
			AssertEquals(depotGuid, Registry.DefaultDepotForBuildHVLV);
		}

		public void TestEHubTesting()
		{
			Assert(!Registry.EHubTesting);
			Registry.RawRegistry.EHubTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(Registry.EHubTesting);
		}

		public void TestShowAvailableBranchesOnly()
		{
			Registry.RawRegistry.ShowAvailableBranchesOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.ShowAvailableBranchesOnly);
			Registry.RawRegistry.ShowAvailableBranchesOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.ShowAvailableBranchesOnly);
		}

		public void TestShowAvailableDepartmentsOnly()
		{
			Registry.RawRegistry.ShowAvailableDepartmentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("ShowAvailableDepartmentsOnly should be able to be set to true.", true, Registry.ShowAvailableDepartmentsOnly);
			Registry.RawRegistry.ShowAvailableDepartmentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("ShowAvailableDepartmentsOnly should be able to be set to false.", false, Registry.ShowAvailableDepartmentsOnly);
		}

		[ExpectNoExceptions]
		public void TestNewStaffWithCustomStaffMembershipTypesList()
		{
			Registry.RawRegistry.StaffMembershipTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Registry.RawRegistry.StaffMembershipTypeList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			var userContext = EnvProxy.Instance.CurrentUserContext;
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				factory.New(ObjectFactory.GetType("IGlbStaff"));
			}
		}

		public void TestSMTPDefaultDoNotReplyEmailAddressWithNoCurrentCompany()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown("Should not be throwing NullReferenceException.", () => { var x = Registry.SMTPDefaultDoNotReplyEmailAddress; });
			}
		}

		public void TestEnableExternalValidationService()
		{
			Registry.RawRegistry.EnableExternalValidationService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.EnableExternalValidationService);
			Registry.RawRegistry.EnableExternalValidationService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.EnableExternalValidationService);
		}

		public void TestExternalValidationServiceUrl()
		{
			const string url = "http://www.google.com.au/";
			Registry.RawRegistry.ExternalValidationServiceUrl.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, url);
			AssertEquals(url, Registry.ExternalValidationServiceUrl);
		}

		public void TestExternalValidationServiceTimeout()
		{
			AssertEquals("Incorrect default timeout value.", 60, Registry.ExternalValidationServiceTimeout);
			Registry.RawRegistry.ExternalValidationServiceTimeout.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 120);
			AssertEquals(120, Registry.ExternalValidationServiceTimeout);
		}

		#region Address Validation

		public void TestAddressValidationWebServiceTimeout()
		{
			const int timeout = 10;
			Registry.RawRegistry.AddressValidationWebServiceTimeout.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, timeout);
			AssertEquals(timeout, Registry.AddressValidationWebServiceTimeout);
		}

		public void TestEnableAddressValidationWebService()
		{
			Registry.RawRegistry.EnableAddressValidationWebService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, Registry.EnableAddressValidationWebService);
			Registry.RawRegistry.EnableAddressValidationWebService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, Registry.EnableAddressValidationWebService);
		}

		#endregion

		public void TestUpgradeObserverMaxWaitInSeconds()
		{
			AssertEquals("Default", 60, Registry.UpgradeObserverMaxWaitInSeconds);

			Registry.UpgradeObserverMaxWaitInSeconds = 100;
			AssertEquals("New value", 100, Registry.UpgradeObserverMaxWaitInSeconds);
		}

		public void TestBorderWiseUserManagementPortalRegistryItems_ShouldBeVisible()
		{
			RegistryItemDictionary.Instance.PurgeAll();

			AssertEquals(RegistryOptions.IsOnlyForDevelopers, Registry.RawRegistry.BorderWiseUmpApiBaseAddress.Options);

			RegistryItemDictionary.Instance.PurgeAll();
			Registry.RawRegistry.ProductivityWiseModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(RegistryOptions.IsOnlyForDevelopers, Registry.RawRegistry.BorderWiseUmpApiBaseAddress.Options);
		}

		public void TestBorderWiseEnableMultilineTariffClassificationRegistryItems_ShouldBeVisible()
		{
			RegistryItemDictionary.Instance.PurgeAll();

			AssertEquals(RegistryOptions.PreserveTestValue, Registry.RawRegistry.BorderWiseEnableMultilineTariffClassification.Options);

			RegistryItemDictionary.Instance.PurgeAll();
			Registry.RawRegistry.ProductivityWiseModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(RegistryOptions.IsHidden, Registry.RawRegistry.BorderWiseEnableMultilineTariffClassification.Options);
		}

		public void TestUseAlwaysOnReplicaCache()
		{
			AssertEquals("UseAlwaysOnReplicaCache", true, Registry.RawRegistry.UseAlwaysOnReplicaCache.DefaultValue);
			Registry.UseAlwaysOnReplicaCache = false;
			AssertEquals("UseAlwaysOnReplicaCache", false, Registry.UseAlwaysOnReplicaCache);
		}

		public void TestAlwaysOnReplicaCachedInfos()
		{
			var expected = Registry.AlwaysOnReplicaCachedInfos = new[] {
				new AlwaysOnReplicaInfo() { ReplicaServerName = "testValue1", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo() { ReplicaServerName = "ABC.sand.wtg.zone\\MSSQLSERVER2022", AvailabilityMode = 1 } };
			AssertArrayEqualsByElements("AlwaysOnReplicaCachedInfos", expected, Registry.AlwaysOnReplicaCachedInfos);
		}

		public void TestUseModernSqlSecuritySystemDefaultValueIsTrueForUnitTests()
		{
			Globals.IsTest_ForTest.Value = true;
			AssertEquals("UseModernSqlSecuritySystem", true, Registry.UseModernSqlSecuritySystem);
		}

		public void TestUseModernSqlSecuritySystemDefaultValueIsFalseForProduction()
		{
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				AssertEquals("UseModernSqlSecuritySystem", false, Registry.UseModernSqlSecuritySystem);
			}
		}

		public void TestUseModernSqlSecuritySystem()
		{
			Registry.UseModernSqlSecuritySystem = false;
			AssertEquals("UseModernSqlSecuritySystem", false, Registry.UseModernSqlSecuritySystem);

			Registry.UseModernSqlSecuritySystem = true;
			AssertEquals("UseModernSqlSecuritySystem", true, Registry.UseModernSqlSecuritySystem);
		}

		readonly ReleaseInfo alpRelease = ReleaseInfo.CreateNewInstanceForTesting("23.1.1.1", DateTime.Today, ReleaseRings.Codes.ALP);
		readonly ReleaseInfo dprRelease = ReleaseInfo.CreateNewInstanceForTesting("23.1.1.1", DateTime.Today, ReleaseRings.Codes.DPR);
		public void TestEnableDotNetVersionSwitchMenuCanSetValue()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(alpRelease))
			{
				Registry.EnableDotNetVersionSwitchMenu = false;
				AssertEquals("Default value is false", false, Registry.EnableDotNetVersionSwitchMenu);

				Registry.EnableDotNetVersionSwitchMenu = true;
				AssertEquals("Value is changed", true, Registry.EnableDotNetVersionSwitchMenu);
			}
		}

		public void TestEnableDotNetVersionSwitchMenuUseDefaultValueIfNotALP()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(alpRelease))
			{
				Registry.EnableDotNetVersionSwitchMenu = false;
				AssertEquals("Default value is false", false, Registry.EnableDotNetVersionSwitchMenu);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(dprRelease))
			{
				Registry.EnableDotNetVersionSwitchMenu = true;
				AssertEquals("Should use default value even value is changed", false, Registry.EnableDotNetVersionSwitchMenu);
			}
		}

		public void TestDefaultDotNetVersionOnLaunchCanSetValue()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(alpRelease))
			{
				Registry.DefaultDotNetVersionOnLaunch = DotNetBuildVersionTargetTypeList.Codes.NetFramework48;
				AssertEquals("Default value is Framework48", DotNetBuildVersionTargetTypeList.Codes.NetFramework48, Registry.DefaultDotNetVersionOnLaunch);

				Registry.DefaultDotNetVersionOnLaunch = DotNetBuildVersionTargetTypeList.Codes.NetCore8;
				AssertEquals("Value is changed", DotNetBuildVersionTargetTypeList.Codes.NetCore8, Registry.DefaultDotNetVersionOnLaunch);
			}
		}

		public void TestDefaultDotNetVersionOnLaunchUseDefaultValueIfNotALP()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(alpRelease))
			{
				Registry.DefaultDotNetVersionOnLaunch = DotNetBuildVersionTargetTypeList.Codes.NetFramework48;
				AssertEquals("Default value is Framework48", DotNetBuildVersionTargetTypeList.Codes.NetFramework48, Registry.DefaultDotNetVersionOnLaunch);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(dprRelease))
			{
				Registry.DefaultDotNetVersionOnLaunch = DotNetBuildVersionTargetTypeList.Codes.NetCore8;
				AssertEquals("Should use default value even value is changed", DotNetBuildVersionTargetTypeList.Codes.NetFramework48, Registry.DefaultDotNetVersionOnLaunch);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion
	}
}
