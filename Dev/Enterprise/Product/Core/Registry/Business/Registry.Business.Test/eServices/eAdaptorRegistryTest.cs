using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.Registry.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.eServices.Testing
{
	[TestedType(typeof(eAdaptorRegistry))]
	sealed class eAdaptorRegistryTest : RegistryItemSetTestCaseWithFactory<eAdaptorRegistry>
	{
		public void TestUseDate2012_11NamespaceChangesWhenDateIsSet()
		{
			AssertEquals("Precondition: Date2012_11NamespaceAndFormatKicksIn", DateTime.MinValue, eAdaptorRegistry.Instance.Date2012_11NamespaceAndFormatKicksIn.Value);
			AssertEquals("UseDate2012_11NamespaceAndFormat with no date set (default)", false, eAdaptorRegistry.Instance.UseDate2012_11NamespaceAndFormat);

			eAdaptorRegistry.Instance.Date2012_11NamespaceAndFormatKicksIn.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-2));
			AssertEquals("UseDate2012_11NamespaceAndFormat today", true, eAdaptorRegistry.Instance.UseDate2012_11NamespaceAndFormat);

			eAdaptorRegistry.Instance.Date2012_11NamespaceAndFormatKicksIn.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today.AddDays(1));
			AssertEquals("UseDate2012_11NamespaceAndFormat today", false, eAdaptorRegistry.Instance.UseDate2012_11NamespaceAndFormat);
		}

		public void TestUniversalXMLAlwaysIncludeJobCostingInUniversalShipment()
		{
			AssertEquals("Default value should be false.", false, eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.Value);
		}

		public void TestAllowParallelUMI()
		{
			AssertEquals("In order to avoid regressions by introducing this feature, default value should be false.", false, eAdaptorRegistry.Instance.AllowParallelUMI.Value);
			AssertEquals(RegistryOptions.Default, eAdaptorRegistry.Instance.AllowParallelUMI.Options);
		}

		public void TestMessagesPerBatch()
		{
			AssertEquals(50, eAdaptorRegistry.Instance.MessagesPerBatch.Value);
		}

		public void TestMessagesPerExecution()
		{
			AssertEquals(5000, eAdaptorRegistry.Instance.MessagesPerExecution.Value);
		}

		public void TestUMIMessagesPerBatch()
		{
			AssertEquals(2000, eAdaptorRegistry.Instance.UMIMessagesPerBatch.Value);
		}

		public void TestUMIMessagesPerExecution()
		{
			AssertEquals(50000, eAdaptorRegistry.Instance.UMIMessagesPerExecution.Value);
		}

		public void TestMessageQueueCapacity()
		{
			AssertEquals(20000, eAdaptorRegistry.Instance.MessageQueueCapacity.Value);
		}

		public void TestUniversalXMLEnableVerboseLogging()
		{
			AssertEquals("Default value should be false.", false, eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.Value);
		}

		public void TestExtendedUniversalLogging()
		{
			AssertEquals(9, eAdaptorRegistry.Instance.ExtendedUniversalLogging.Value.Count);
		}

		public void TestTransactionProtectionEnabled()
		{
			AssertEquals("Extended transaction protection is on by default", true, eAdaptorRegistry.Instance.UniversalXMLExtendedTransactionProtectionEnabled.Value);
		}

		public void TestFileNameAttachToNotesEnabled()
		{
			TestRegistryItem(ItemSet.FileNameAttachToNotesEnabled,
				"FileNameAttachToNotesEnabled",
				eAdaptorRegistry.Categories.eServices_UniversalXML,
				"Enable File Name Attach To Notes",
				"When enabled, file name element in request will create a note entry in EDI message.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestUseBrokerageDataFirstWhenExportUniversalXML()
		{
			TestRegistryItem(ItemSet.UseBrokerageDataFirstWhenExportUniversalXML,
				"UseBrokerageDataFirstWhenExportUniversalXML",
				eAdaptorRegistry.Categories.eServices_UniversalXML,
				"Use Brokerage Data First",
				"This registry governs which data is considered to be the most accurate and therefore which data gets placed in the exported Universal Shipment XML. If this registry item is set to 'yes' then most data from the brokerage tab (customs data) will take precedence. If it is set to 'no' then most shipment data will take precedence. This does not apply to packages.",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestUseDefaultingOfDataWhenImportingUniversalXML()
		{
			TestRegistryItem(ItemSet.UseDefaultingOfDataWhenImportingUniversalXML,
				"UseDefaultingOfDataWhenImportingUniversalXML",
				eAdaptorRegistry.Categories.eServices_UniversalXML,
				"Enable Data Defaulting",
				"This registry governs whether the system will default data when importing an Universal Shipment XML like manual keying of the data (Customs Only).",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestUseCombinedReferenceAndPartyIDMatchInUniversalXML()
		{
			AssertEquals("Default value should be false.", false, eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.Value);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(".GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)", true, eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(".Value", true, eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.Value);
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		public void TesteAdaptorRegistry_CanSendCargoImpMessagesThroughEAdaptor()
		{
			AssertEquals("Default value should be false.", false, eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value);
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value);
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		public void TesteUniversalXMLUpdateConsolDuringAutomaticImport()
		{
			AssertEquals("Default value should be true.", true, eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TesteUniversalXMLUpdateConsolShipmentDuringAutomaticImport()
		{
			AssertEquals("Default value should be true.", true, eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolShipmentDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TesteAdaptorFailedEDIInterchangeNotificationFrequency_Defualts()
		{
			AssertEquals(
				"Default settings should be periodic",
				HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationFrequency.Value.Settings);
			AssertEquals(
				"Default settings should be periodic",
				15,
				eAdaptorRegistry.Instance.eAdaptorFailedEDIInterchangeNotificationFrequency.Value.TimeInterval);
		}

		public void TesteUniversalXMLUpdateConsolContainersDuringAutomaticImport()
		{
			AssertEquals("Default value should be true.", true, eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolContainersDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TesteUniversalXMLUpdateConsolRoutingDuringAutomaticImport()
		{
			AssertEquals("Default value should be true.", true, eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TesteUniversalXMLUpdateShipmentDuringAutomaticImport()
		{
			AssertEquals("Default value should be true.", true, eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.Value);
			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TesteAdaptorRegistryDefaultValuesForNewRegistryOptionsForGEOShouldBeTrue()
		{
			var rego = new Mock<IProductRegistration>();
			var regoKey = new Mock<IProductRegistrationKey>();
			rego.Setup(x => x.Key).Returns(regoKey.Object);
			regoKey.Setup(x => x.EnterpriseCode).Returns("GEO");
			ObjectFactory.Substitute(rego.Object);
			Assert(eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value);
		}

		public void TestSupportDocDataInUniversalShipmentXML()
		{
			AssertEquals("Default value should be false.", false, eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.Value);
			AssertEquals("Should be only for support, and do not preserv test value.", RegistryOptions.IsOnlyForSupport, eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.Options);
		}

		public void TestMessageUserContextTracingEnabled()
		{
			AssertEquals("Default value should be true.", true, eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.DefaultValue);
			eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert(!eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.Value);
			eAdaptorRegistry.Instance.MessageUserContextTracingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Testing")]
		public void TesteAdaptorOutboundPendingItemsBatchSize()
		{
			TestRegistryItem(ItemSet.eAdaptorOutboundPendingItemsBatchSize,
				"eAdapterOutboundPendingItemsBatchSize",
				eAdaptorRegistry.Categories.eServices_eAdaptor_Outbound,
				"Outbound Pending Items Batch Size",
				"The number of pending items that are retrieved from the database at a time per company and recipient to be sent.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: 1000, expectedMinValue: 1, expectedMaxValue: int.MaxValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Testing")]
		public void TesteAdaptorOutboundPendingItemsSearchLimit()
		{
			TestRegistryItem(ItemSet.eAdaptorOutboundPendingItemsSearchLimit,
				"eAdapterOutboundPendingItemsSearchLimit",
				eAdaptorRegistry.Categories.eServices_eAdaptor_Outbound,
				"Outbound Pending Items Search Limit",
				"The maximum number of pending items that we will search through in order to establish the list of branch-recipient pair groups that we need to send interchanges for in this run.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: 100000, expectedMinValue: 1, expectedMaxValue: int.MaxValue);
		}

		public void TestOutboundCommunicationsProtocol()
		{
			AssertEquals("Default value", "SOAP", ItemSet.OutboundCommunicationsProtocol.Value);

			Assert("Registry Item OutboundCommunicationsProtocol should have RegistryOptions.IsOnlyForSupport",
				eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.HasOption(RegistryOptions.IsOnlyForSupport));

			Assert("Registry Item OutboundCommunicationsProtocol should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TestIgnoreUnknownSSLCertificate()
		{
			Assert("Registry Item IgnoreUnknownSSLCertificate should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.IgnoreUnknownSSLCertificate.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_eAdaptorInboundAuthentications()
		{
			eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "username|password");
			AssertEquals("username|password", eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert("Registry Item eAdaptorInboundAuthentications should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_eAdaptorInboundOAuthAuthorityUrls()
		{
			Assert("Registry Item eAdaptorInboundOAuthAuthorityUrls should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.eAdaptorInboundOAuthAuthorityUrls.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_eAdaptorInboundOAuthClientIDs()
		{
			Assert("Registry Item eAdaptorInboundOAuthClientIDs should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.eAdaptorInboundOAuthClientIDs.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_InboundAdaptorServiceUrl()
		{
			eAdaptorRegistry.Instance.InboundAdaptorServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "www.abc.com");
			AssertEquals("www.abc.com", eAdaptorRegistry.Instance.InboundAdaptorServiceUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert("Registry Item InboundAdapterServiceUrl should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.InboundAdaptorServiceUrl.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_OutboundAdapterServiceUrl()
		{
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "www.abc.com");
			AssertEquals("www.abc.com", eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert("Registry Item OutboundAdapterServiceUrl should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_eAdaptorOutboundPassword()
		{
			eAdaptorRegistry.Instance.eAdaptorOutboundPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
			AssertEquals("password", eAdaptorRegistry.Instance.eAdaptorOutboundPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			Assert("Registry Item eAdaptorOutboundPassword should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.eAdaptorOutboundPassword.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_eAdaptorNextOutbound_Default()
		{
			eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eAdaptorNextOutboundConfig.DefaultValue);

			var config = eAdaptorRegistry.Instance.eAdaptorNextOutbound.Value;

			AssertEquals(eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials, config.AuthorizationGrantTypeCode);
			AssertEquals("", config.AuthorizationURL);
			AssertEquals(false, config.IsOAuth2Enabled);
			AssertEquals("", config.ClientID);
			AssertEquals("", config.ClientSecret);
			AssertEquals("", config.Username);
			AssertEquals("", config.Password);

			Assert("Registry Item eAdaptorNextOutbound should have RegistryOptions.PreserveTestValue",
				eAdaptorRegistry.Instance.eAdaptorNextOutbound.HasOption(RegistryOptions.PreserveTestValue));
		}

		public void TesteServiceRegistry_eAdaptorNextOutbound_ClientCredentials()
		{
			eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new eAdaptorNextOutboundConfig
			{
				IsOAuth2Enabled = ZBool.True,
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				AuthorizationURL = "https://www.example.com/oauth/token",
				ClientID = "0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c",
				ClientSecret = "0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c",
				Username = "ABC",
				Password = "123"
			});

			var config = eAdaptorRegistry.Instance.eAdaptorNextOutbound.Value;

			AssertEquals(eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials, config.AuthorizationGrantTypeCode);
			AssertEquals("https://www.example.com/oauth/token", config.AuthorizationURL);
			AssertEquals(true, config.IsOAuth2Enabled);
			AssertEquals("0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c", config.ClientID);
			AssertEquals("0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c", config.ClientSecret);
			AssertEquals("ABC", config.Username);
			AssertEquals("123", config.Password);
		}

		public void TesteServiceRegistry_eAdaptorNextOutbound_Password()
		{
			eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new eAdaptorNextOutboundConfig
			{
				IsOAuth2Enabled = ZBool.True,
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.Password,
				AuthorizationURL = "https://www.example.com/oauth/token",
				ClientID = "0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c",
				ClientSecret = "0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c",
				Username = "ABC",
				Password = "123"
			});

			var config = eAdaptorRegistry.Instance.eAdaptorNextOutbound.Value;

			AssertEquals(eAdaptorNextOutboundGrantTypesList.Codes.Password, config.AuthorizationGrantTypeCode);
			AssertEquals("https://www.example.com/oauth/token", config.AuthorizationURL);
			AssertEquals(true, config.IsOAuth2Enabled);
			AssertEquals("0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c", config.ClientID);
			AssertEquals("0c1b8dfa-36b9-4a4c-b74c-d987f3f1f65c", config.ClientSecret);
			AssertEquals("ABC", config.Username);
			AssertEquals("123", config.Password);
		}
	}
}
