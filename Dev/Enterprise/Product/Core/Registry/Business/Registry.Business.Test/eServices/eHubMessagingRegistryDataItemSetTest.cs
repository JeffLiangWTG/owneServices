using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(eHubMessagingRegistry))]
	sealed class eHubMessagingRegistryDataItemSetTest : RegistryItemSetTestCaseWithFactory<eHubMessagingRegistry>
	{
		//Tested in project Enterprise.eHubMessaging.Tests
		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			//Registry items based on EDIMessageSubTypeList, which is multilingual list, and shares MessageTypeObj with non-multiligual list SystemMessageList
			//Therefore we need to override this test and not to run.
			Assert(true);
		}

		public void TestApplicationCodesAreClonedNotShared()
		{
			BusinessObject company = (BusinessObject)Factory.LoadTop1<IGlbCompany>(new ZQuery());
			var purgeSettings = eHubMessagingRegistry.GetDefaultPurgeSettings();
			var clone = (PurgeSettings)purgeSettings.Clone(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);

			AssertEquals(purgeSettings.ApplicationCodes[0].ApplicationCode, clone.ApplicationCodes[0].ApplicationCode);
			purgeSettings.ApplicationCodes[0].ApplicationCode = "XXX";
			AssertNotEquals(purgeSettings.ApplicationCodes[0].ApplicationCode, clone.ApplicationCodes[0].ApplicationCode);
		}

		public void TestEHIOutageStartTime()
		{
			TestGenericRegistryItem(ItemSet.EHIOutageStartTime,
				"EHIOutageStartTime",
				eServicesRegistry.Categories.eServices_eHub,
				"EHI Service Task Outage Start Time",
				"The time the current outage for the EHI service task started",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);
		}

		public void TestEHOOutageStartTime()
		{
			TestGenericRegistryItem(ItemSet.EHOOutageStartTime,
				"EHOOutageStartTime",
				eServicesRegistry.Categories.eServices_eHub,
				"EHO Service Task Outage Start Time",
				"The time the current outage for the EHO service task started",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				DateTime.MinValue);
		}

		public void TestOutageExpiryTimeInMinutes()
		{
			TestGenericRegistryItem(ItemSet.eHubOutageExpiryTimeInMinutes,
				"eHubOutageExpiryTimeInMinutes",
				eServicesRegistry.Categories.eServices_eHub,
				"eHub Outage Expiry Time in Minutes",
				"The number of minutes until we will start reporting errors if the connection to the eHub Gateway remains offline",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				30);
		}

		public void TesteHubGatewayServerAddressList()
		{
			TestGenericRegistryItem(ItemSet.eHubGatewayServerAddressList,
				"EHubGatewayServerAddress",
				eServicesRegistry.Categories.eServices_eHub,
				"eHub Gateway Server Address",
				"This is the address of the gateway server. If this value is changed, all eHub passwords will automatically be reset which will cause the eHub service tasks to re-register with eHub.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				"ehubgateway.wisegrid.net");
		}

		public void TesteHubGatewayServerCannotBeSetToTesteHub()
		{
			AssertExceptionThrown<RegistryValidationException>(
				"Cannot set production gateway address to test eHub",
				() => ItemSet.eHubGatewayServerAddressList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eHubMessagingRegistry.eHubGateway.TestServerName));
		}

		public void TesteHubTestGatewayServerAddressList()
		{
			TestGenericRegistryItem(ItemSet.eHubTestGatewayServerAddressList,
				"EHubTestGatewayServerAddress",
				eServicesRegistry.Categories.eServices_eHub,
				"eHub Test Gateway Server Address",
				"This is the address of the gateway server for performing system testing using non-production endpoints.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
				"ehub-ausyd-test.wisegrid.net");
		}

		public void TesteHubSendInterchangesToTestGateway()
		{
			using (ObjectFactory.Substitute(GetProductRegistrationMock(isDeveloperSystem: false, isUATSystem: false)))
			{
				AsserteHubSendInterchangesToTestGateway(expectedDefault: false);
			}

			using (ObjectFactory.Substitute(GetProductRegistrationMock(isDeveloperSystem: true, isUATSystem: false)))
			{
				AsserteHubSendInterchangesToTestGateway(expectedDefault: true);
			}

			using (ObjectFactory.Substitute(GetProductRegistrationMock(isDeveloperSystem: false, isUATSystem: true)))
			{
				AsserteHubSendInterchangesToTestGateway(expectedDefault: true);
			}
		}

		void AsserteHubSendInterchangesToTestGateway(bool expectedDefault)
		{
			ItemSet.RemoveItemFromCacheIfOlderThan("EHubSendInterchangesToTestGateway", TimeSpan.FromMilliseconds(-1));
			TestGenericRegistryItem(ItemSet.eHubSendInterchangesToTestGateway,
				"EHubSendInterchangesToTestGateway",
				eServicesRegistry.Categories.eServices_eHub,
				"Send Interchanges To The eHub Test Gateway",
				"On non-production systems, a value of true for this setting will result in interchanges being sent to the eHub test gateway instead of the production gateway.",
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				expectedDefault);
		}

		IProductRegistration GetProductRegistrationMock(bool isDeveloperSystem, bool isUATSystem)
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalDeveloperSystem()).Returns(isDeveloperSystem);
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalUATSystem()).Returns(isUATSystem);
			return productRegistrationMock.Object;
		}

		public void TesteHubEnableReceiveFromProductionGateway()
		{
			using (ObjectFactory.Substitute(GetProductRegistrationMock(isDeveloperSystem: false, isUATSystem: false)))
			{
				AsserteHubEnableReceiveFromProductionGateway(expectedDefault: true);
			}

			using (ObjectFactory.Substitute(GetProductRegistrationMock(isDeveloperSystem: true, isUATSystem: false)))
			{
				AsserteHubEnableReceiveFromProductionGateway(expectedDefault: false);
			}

			using (ObjectFactory.Substitute(GetProductRegistrationMock(isDeveloperSystem: false, isUATSystem: true)))
			{
				AsserteHubEnableReceiveFromProductionGateway(expectedDefault: false);
			}
		}

		void AsserteHubEnableReceiveFromProductionGateway(bool expectedDefault)
		{
			ItemSet.RemoveItemFromCacheIfOlderThan("EHubEnableReceiveFromProductionGateway", TimeSpan.FromMilliseconds(-1));
			TestGenericRegistryItem(ItemSet.eHubEnableReceiveFromProductionGateway,
				"EHubEnableReceiveFromProductionGateway",
				eServicesRegistry.Categories.eServices_eHub,
				"Enable Receive From The eHub Production Gateway",
				"A value of true for this setting will enable the EHI service task, which is responsible for retrieving messages from the eHub production gateway.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefault);
		}

		public void TesteHubOutboundPendingItemsBatchSize()
		{
			TestRegistryItem(ItemSet.eHubOutboundPendingItemsBatchSize,
				"eHubOutboundPendingItemsBatchSize",
				eServicesRegistry.Categories.eServices_eHub,
				"Outbound Pending Items Batch Size",
				"The number of pending items that are retrieved from the database at a time per company and recipient to be sent.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: 1000, expectedMinValue: 1, expectedMaxValue: int.MaxValue);
		}

		public void TesteHubOutboundPendingItemsSearchLimit()
		{
			TestRegistryItem(ItemSet.eHubOutboundPendingItemsSearchLimit,
				"eHubOutboundPendingItemsSearchLimit",
				eServicesRegistry.Categories.eServices_eHub,
				"Outbound Pending Items Search Limit",
				"The maximum number of pending items that we will search through in order to establish the list of branch-recipient pair groups that we need to send interchanges for in this run.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				expectedDefaultValue: 100000, expectedMinValue: 1, expectedMaxValue: int.MaxValue);
		}

		public void TestPurgeSettingsContainsApplicationCodes()
		{
			var purrgeSettings = new PurgeSettings();
			var appCodes = new ApplicationCodeObjCollection();
			appCodes.AddRange(new Dummy1ApplicationCodeSetting().GetPurgeSettings());
			purrgeSettings.SetApplicationCodes(appCodes);
			using (ItemSet.PurgeSettingsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, purrgeSettings))
			{
				var mockConfigList = new List<PurgeSettingsConfig> { new Dummy2ApplicationCodeSetting() };
				using (ObjectFactory.Substitute("MessagePurgeSettingsConfigList", mockConfigList))
				{
					var codes = ItemSet.PurgeSettingsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ApplicationCodes;
					Assert("Registry should contain previously saved Application Code", codes.Any(a => ((ApplicationCodeObj)a).ApplicationCode == "DM1"));
					Assert("Registry should contain the Application Code added to ObjectFactory", codes.Any(a => ((ApplicationCodeObj)a).ApplicationCode == "DM2"));
				}
			}
		}

		class Dummy1ApplicationCodeSetting : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodePurgeType("DM1", new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, 2, TimeUnit.Year);
			}
		}
		class Dummy2ApplicationCodeSetting : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodePurgeType("DM2", new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, 2, TimeUnit.Year);
			}
		}
	}
}
