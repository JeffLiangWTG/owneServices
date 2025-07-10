using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.Business
{
	[TestedType(typeof(eHubMessagingRegistry))]
	public class eHubMessagingRegistryTest : RegistryItemSetTestCaseWithFactory<eHubMessagingRegistry>
	{
		public void TestPurgeSettingsItem()
		{
			TestGenericRegistryItem(ItemSet.PurgeSettingsItem,
				"PurgeSettings",
				eHubMessagingRegistry.Categories.eServices,
				"Message Purge Settings",
				"These settings specify what the EPG - Message Purge Service Task will do when it runs. For each application code and its Message Sub Types, you can select them to be purged by the service task and also specify how many months to keep the messages before they are purged.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue);

			var defaultValue = ItemSet.PurgeSettingsItem.DefaultValue;
			AssertEquals(56, defaultValue.ApplicationCodes.Count);

			var applicationCodeMessageTypeCountList = new List<(string, int)>
			{
				(ApplicationCodeList.Codes.XMS, 22),
				(ApplicationCodeList.Codes.UniversalDataMessaging, 13),
				(ApplicationCodeList.Codes.UniversalDataQuery, 13),
				(ApplicationCodeList.Codes.NativeDataMessaging, 18),
				(ApplicationCodeList.Codes.NativeDataQuery, 29),
				(ApplicationCodeList.Codes.SYS, 15),
				(ApplicationCodeList.Codes.Telematics, 2),
				(ApplicationCodeList.Codes.UsageData, 1),
				(ApplicationCodeList.Codes.UsageDataToSummarise, 1),
				(ApplicationCodeList.Codes.eNett, 9),
				(ApplicationCodeList.Codes.GlobalElectronicInvoice, 30),
				(ApplicationCodeList.Codes.GlobalElectronicPayment, 4),
				(ApplicationCodeList.Codes.CPWRequestMessage, 1),
				(ApplicationCodeList.Codes.DPSRequestMessage, 1),
				(ApplicationCodeList.Codes.JPCustoms, 0),
				(ApplicationCodeList.Codes.TWCustoms, 0),
				(ApplicationCodeList.Codes.ContainerManagement, 3),
				(ApplicationCodeList.Codes.ComTrac, 2),
				(ApplicationCodeList.Codes.EIDO, 3),
				(ApplicationCodeList.Codes.ForwardAir, 1),
				(ApplicationCodeList.Codes.PortAuthority, 4),
				(ApplicationCodeList.Codes.ILCustoms, 1),
				(ApplicationCodeList.Codes.TRCustoms, 0),
				(ApplicationCodeList.Codes.NOCustoms, 11),
				(ApplicationCodeList.Codes.NOCustomsEmma, 3),
				(ApplicationCodeList.Codes.NOCustomsNcts, 2),
				(ApplicationCodeList.Codes.NOCustomsDMO, 4),
				(ApplicationCodeList.Codes.BECustoms, 3),
				(ApplicationCodeList.Codes.NLCustoms, 3),
				(ApplicationCodeList.Codes.UAECustoms, 0),
				(ApplicationCodeList.Codes.ESCustomsMessage, 0),
				(ApplicationCodeList.Codes.ShipamaxIntegration, 0),
				(ApplicationCodeList.Codes.ZATransactionOrders, 0),
				(ApplicationCodeList.Codes.ZACustoms, 0),
			};

			foreach (var tuple in applicationCodeMessageTypeCountList)
			{
				var appCode = defaultValue.ApplicationCodes.GetApplicationCodeObj(tuple.Item1);
				AssertNotNull($"Application Code '{tuple.Item1}'", appCode);
				AssertEquals($"Application Code '{tuple.Item1}' has incorrect number of message type", tuple.Item2, appCode.MessageTypes.Count);
				Array.ForEach(appCode.MessageTypes.OfType<MessageTypeObj>().ToArray(), m => Assert(m.Selected));
			}
		}

		public void TestPurgeMessageBatchSize()
		{
			var settings = eHubMessagingRegistry.GetDefaultPurgeSettings();
			var expectedDefaultBatchsize = 100;
			var defaultBatchSize = settings.BatchSize;
			AssertEquals(expectedDefaultBatchsize, defaultBatchSize);

			settings.BatchSize = 10001;
			var notifications = settings.BatchSizeInfo.Notifications.ToList();
			Assert("Contains error message size > 10000", notifications.Count == 1);
			AssertEquals("Please enter a 'Message Purge Batch Size' within the range 100 to 10000.", notifications[0].Message);

			settings.BatchSize = 99;
			notifications = settings.BatchSizeInfo.Notifications.ToList();
			Assert("Contains error message size < 100", notifications.Count == 1);
			AssertEquals("Please enter a 'Message Purge Batch Size' within the range 100 to 10000.", notifications[0].Message);

			settings.BatchSize = 150;
			notifications = settings.BatchSizeInfo.Notifications.ToList();
			Assert("Contains no error message in range", notifications.Count == 0);
		}

		public void TestAUSuppressResends()
		{
			TestGenericRegistryItem(ItemSet.AUSuppressResends, "AUSuppressResends", eHubMessagingRegistry.Categories.eServices_CustomsServiceBureau_AU, "Suppress Resends", "Select 'No' if you want the normal re-sending of interchanges to be reinstated when AU Customs Messaging via eHub is enabled", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, true);
		}

		public void TestAUSuppressCONTRLAcknowledgements()
		{
			TestGenericRegistryItem(ItemSet.AUSuppressCONTRLAcknowledgements, "AUSuppressCONTRLAcknowledgements", eHubMessagingRegistry.Categories.eServices_CustomsServiceBureau_AU, "Suppress sending CONTRL Acknowledgements", "Select 'Yes' if you want to suppress the sending of the CONTRL Acknowledgement message in response to a Customs Messaging received via eHub.\r\nNote that the use of this feature requires that Customs set not to expect CONTRL messages for your user site.\r\nDo not set this to 'Yes' if Customs have not made the necessary adjustment to your user site.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue, false);
		}

		public void TestSendHKISACEHub()
		{
			TestGenericRegistryItem(ItemSet.SendHKISACEViaEHub, "SendHKISACEViaEHub", eHubMessagingRegistry.Categories.eServices_CustomsServiceBureau_HK, "Send ISAC Via eHub", "Select 'Yes' if you want to enable HK Customs Messaging via eHub", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, true);
		}

		public void TestEHubGatewayServerAddressList()
		{
			AssertEquals(eHubMessagingRegistry.eHubGateway.ServerName, ItemSet.eHubGatewayServerAddressList.DefaultValue);
			ItemSet.eHubGatewayServerAddressList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "URI.com");
			AssertEquals("URI.com", ItemSet.eHubGatewayServerAddressList.Value);
		}

		public void TestUpdateToEHubGatewayServerAddressList()
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.EHubRegistryUpdate));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_From, ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_To, "eHub"));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued));
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc";

			RegistryItemTag item = new RegistryItemTag(ItemSet.eHubGatewayServerAddressList);
			item.NewValue = "test.ehub.url";
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			IEDIInterchange interchange = Factory.LoadTop1<IEDIInterchange>(query);
			AssertNotNull("interchange must have been created", interchange);
			AssertEquals(string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", EDIInterchangeTypeList.Descriptions.EHubRegistryUpdate, ItemSet.EHINudgeURL.Value), interchange.EI_BodyText);
		}

		public void TestUpdateToEHINudgeURL()
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.eHub);
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, EDIInterchangeTypeList.Codes.EHubRegistryUpdate));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_From, ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_To, "eHub"));
			query.AddToFilter(new ZQuery(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.eHubQueued));
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " desc";

			RegistryItemTag item = new RegistryItemTag(ItemSet.EHINudgeURL);
			item.NewValue = "http://ehi.nudge.url";
			item.IsChanged = true;
			item.HasValue = true;
			item.SaveAllValues();
			IEDIInterchange interchange = Factory.LoadTop1<IEDIInterchange>(query);
			AssertNotNull("interchange must have been created", interchange);
			AssertEquals(string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", EDIInterchangeTypeList.Descriptions.EHubRegistryUpdate, ItemSet.EHINudgeURL.Value), interchange.EI_BodyText);

			item.NewValue = string.Empty;
			item.IsChanged = true;
			item.HasValue = false;
			item.SaveAllValues();
			interchange = new BusinessObjectFactory().LoadTop1<IEDIInterchange>(query);
			AssertNotNull("interchange must have been created", interchange);
			AssertEquals(string.Format(@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", EDIInterchangeTypeList.Descriptions.EHubRegistryUpdate, ItemSet.EHINudgeURL.Value), interchange.EI_BodyText);
		}

		public void TestEhiNudgeUrlIsVisibleToUsers()
		{
			var ehiNudgeUrlItem = AllItems.First(x => x.Name == "EHINudgeURL");
			Assert(string.Format("Visibility for registry item {0} should be Default.", ehiNudgeUrlItem.Name), ehiNudgeUrlItem.Options == RegistryOptions.Default);
		}

		public void TesteServiceRegistry_ScavengingTaskSettings()
		{
			var collection = new ScavengingSettingCollection();
			var setting1 = collection.AddNew();
			setting1.TaskName = "Task1";
			var setting2 = collection.AddNew();
			setting2.TaskName = "Task2";

			eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var output = eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(2, output.Count);
			AssertEquals("Task1", output[0].TaskName);
			AssertEquals("Task2", output[1].TaskName);
		}

		public void TestEHubOutboundSendLimit()
		{
			AssertEquals(5, (int)ItemSet.eHubOutboundSendLimitsRule.Value.SendCountLimit);
			AssertEquals(1024, (int)ItemSet.eHubOutboundSendLimitsRule.Value.SendSizeLimit);
			OutboundSendLimitsRule value = new OutboundSendLimitsRule()
			{
				SendCountLimit = 999,
				SendSizeLimit = 999999
			};
			ItemSet.eHubOutboundSendLimitsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(999, (int)ItemSet.eHubOutboundSendLimitsRule.Value.SendCountLimit);
			AssertEquals(999999, (int)ItemSet.eHubOutboundSendLimitsRule.Value.SendSizeLimit);
		}

		[TestDate(2016, 2, 8, 13, 14, 15)]
		public void TestInterfaceConnectorTemporarilyEnabledUntil()
		{
			AssertEquals(ZDateTime.Empty, ItemSet.InterfaceConnectorTemporarilyEnabledUntilItem.Value.EnabledUntil);

			var value = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.AddMonths(6) };
			ItemSet.InterfaceConnectorTemporarilyEnabledUntilItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(ZDateTime.Now.AddMonths(6), ItemSet.InterfaceConnectorTemporarilyEnabledUntilItem.Value.EnabledUntil);
		}

		[TestDate(2016, 2, 8, 13, 14, 15)]
		public void TestHasInterfaceConnector_InterfaceConnectorTemporarilyEnabledUntilIsDefault_ReturnFalse()
		{
			AssertEquals(false, ItemSet.HasInterfaceConnector);
		}

		[TestDate(2016, 2, 8, 13, 14, 15)]
		[TestDateIncremental(1)]
		public void TestHasInterfaceConnector_InterfaceConnectorTemporarilyEnabledUntilIsPast_ReturnFalse()
		{
			var dayThatWouldBePast = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.AddDays(2) };
			ItemSet.InterfaceConnectorTemporarilyEnabledUntilItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dayThatWouldBePast);
			AssertEquals(false, ItemSet.HasInterfaceConnector);
		}

		[TestDate(2016, 2, 8, 13, 14, 15)]
		public void TestHasInterfaceConnector_InterfaceConnectorTemporarilyEnabledUntilIsFuture_ReturnTrue()
		{
			var today = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date };
			ItemSet.InterfaceConnectorTemporarilyEnabledUntilItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, today);
			AssertEquals(true, ItemSet.HasInterfaceConnector);

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			ItemSet.InterfaceConnectorTemporarilyEnabledUntilItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			AssertEquals(true, ItemSet.HasInterfaceConnector);
		}

		public void TestPurgeSettings_TelematicsXmlMessageSubTypes()
		{
			var telematicsApplicationCode = ItemSet.PurgeSettingsItem.DefaultValue.ApplicationCodes.GetApplicationCodeObj(ApplicationCodeList.Codes.Telematics);

			AssertEquals(2, telematicsApplicationCode.MessageTypes.Count);
			AssertCollectionContains(TelematicsMessageList.Codes.TelematicsXmlData, telematicsApplicationCode.MessageTypes.Select(m => ((MessageTypeObj)m).MessageSubType));
			AssertCollectionContains(TelematicsMessageList.Codes.ProtobufData, telematicsApplicationCode.MessageTypes.Select(m => ((MessageTypeObj)m).MessageSubType));
		}

		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			//Registry items based on EDIMessageSubTypeList, which is multilingual list, and shares MessageTypeObj with non-multiligual list SystemMessageList
			//Therefore we need to override this test and not to run.
			Assert(true);
		}
	}
}
