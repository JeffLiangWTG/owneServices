using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class NctsPluginTest : TestCaseWithFactory
	{
		public void TestPreventDuplicateNctsHeaderCreation_Shipment()
		{
			const string notification = "Departure Movement has already been created and linked to this Shipment.";

			var shipment = Factory.New<ForwardingShipment>();
			using var plugIn = new NctsPluginForTest(shipment);

			// NctsHeader was created in another factory before CreateInBond was called.
			var factory = new BusinessObjectFactory();
			var headerCreatedOutsidePlugin = factory.New<NctsHeader>();
			headerCreatedOutsidePlugin.BH_ParentID = shipment.PK;
			headerCreatedOutsidePlugin.SetMovementType(NctsMovementType.Codes.Departure);
			headerCreatedOutsidePlugin.BH_ParentTableCode = shipment.TablePrefix;
			factory.Save();

			var result = plugIn.CreateInBond();

			var headerLinkedToShipment = Factory.Load<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_ParentID,
				shipment.PK)).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals("PK", headerCreatedOutsidePlugin.PK, headerLinkedToShipment.PK);
				AssertEquals(notification, plugIn.PlugInNotDisplayedMessage);
				AssertEquals(notification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
			});
		}

		public void TestPreventDuplicateNctsHeaderCreation_Consul()
		{
			const string notification = "Arrival Movement has already been created and linked to this Consol.";

			var consol = Factory.New<ForwardingConsol>();
			using var plugIn = new NctsPluginForTest(consol);

			// NctsHeader was created in another factory before CreateInBond was called.
			var factory = new BusinessObjectFactory();
			var headerCreatedOutsidePlugin = factory.New<NctsHeader>();
			headerCreatedOutsidePlugin.BH_ParentID = consol.PK;
			headerCreatedOutsidePlugin.SetMovementType(NctsMovementType.Codes.Arrival);
			headerCreatedOutsidePlugin.BH_ParentTableCode = consol.TablePrefix;
			factory.Save();

			var result = plugIn.CreateInBond();

			var headerLinkedToShipment = Factory.Load<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_ParentID,
				consol.PK)).Single();

			CombineAssertions(() =>
			{
				AssertEquals(false, result);
				AssertEquals("PK", headerCreatedOutsidePlugin.PK, headerLinkedToShipment.PK);
				AssertEquals(notification, plugIn.PlugInNotDisplayedMessage);
				AssertEquals(notification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
			});
		}

		public void TestPluginAlwaysEnabledForGB()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euctp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, Core.Constants.CountryCodes.UnitedKingdom);

			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var plugIn = new NctsPluginForTest(consol))
			{
				consol.JK_RL_NKLoadPort = "GBAAC";
				AssertEquals("NCTS should be enabled for GBACC", true, plugIn.Enabled);
			}
		}

		public void TestPlugEnabledForConsol()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euctp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, GlbCompany.CurrentCompany.Country.Code);

			var consol = Factory.New<ForwardingConsol>();
			using (var plugIn = new NctsPluginForTest(consol))
			{
				CombineAssertions(() =>
				{
					consol.JK_RL_NKLoadPort = "USATL";
					consol.JK_RL_NKDischargePort = "ZADUR";
					AssertEquals("Not Eu Ports", false, plugIn.Enabled);

					consol.JK_RL_NKLoadPort = "FRPAR";
					AssertEquals("EU Load Port", true, plugIn.Enabled);

					consol.JK_RL_NKLoadPort = "USATL";
					var transport = consol.Transports[0];
					transport.JW_RL_NKLoadPort = "USNYC";
					transport.JW_RL_NKDiscPort = "GBLON";
					AssertEquals("Transport has EU Port", true, plugIn.Enabled);
				});
			}
		}

		public void TestPlugEnabledForShipment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euctp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, GlbCompany.CurrentCompany.Country.Code);

			var shipment = Factory.New<ForwardingShipment>();
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					shipment.JS_RL_NKOrigin = "USLAX";
					shipment.JS_RL_NKDestination = "AUSYD";
					AssertEquals("No EU Ports", false, plugIn.Enabled);
					shipment.JS_RL_NKDestination = "GBLHR";
					AssertEquals("Has an EU desitnation", true, plugIn.Enabled);

					shipment.JS_RL_NKDestination = "AUSYD";
					var consol = shipment.Consols.AddNew();
					consol.JK_RL_NKLoadPort = "USLAX";
					consol.JK_RL_NKDischargePort = "AUSYD";
					AssertEquals("Consol and Shipment have no EU ports", false, plugIn.Enabled);
					consol.JK_RL_NKLoadPort = "GBLHR";
					AssertEquals("Consol has an EU port", true, plugIn.Enabled);
				});
			}
		}

		public void TestPluginNotVisibleBasedOnRegistry()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var consol = Factory.New<ForwardingConsol>();
				using (var plugIn = new NctsPluginForTest(consol))
				{
					consol.JK_RL_NKLoadPort = "LVCEN";
					AssertEquals(false, plugIn.Enabled);
				}
			}
		}

		public void TestPluginRegistryHasNoAffectOnES()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			using (var plugIn = new NctsPluginForTest(consol))
			{
				consol.JK_RL_NKLoadPort = "ESAAQ";
				AssertEquals(true, plugIn.Enabled);
			}
		}

		public void TestPluginRegistryHasNoAffectOnFR()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (var plugIn = new NctsPluginForTest(consol))
			{
				consol.JK_RL_NKLoadPort = "FREMU";
				AssertEquals(true, plugIn.Enabled);
			}
		}

		public void TestMenuItemWhenSecurityRightsAllowed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euctp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, GlbCompany.CurrentCompany.Country.Code);

			var consol = Factory.New<ForwardingConsol>();
			Env.Security.EuNctsMessaging.IsAllowed = true;
			using (var plugin = new NctsPluginForTest(consol))
			{
				AssertEquals("No Messaging options for EU", 0, plugin.TopLevelMenu.MenuItems.Count);
			}
		}

		public void TestMenuItemWhenSecurityRightsDenied()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euctp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, GlbCompany.CurrentCompany.Country.Code);

			var consol = Factory.New<ForwardingConsol>();
			Env.Security.EuNctsMessaging.IsAllowed = false;
			using (var plugin = new NctsPluginForTest(consol))
			{
				AssertEquals("Security denied is the only menu", "The menu is disabled because you don't have the appropriate security rights: Operate -> NCTS -> NCTS Transit Movements -> NCTS Messaging", plugin.TopLevelMenu.MenuItems[0].Text);
			}
		}

		public void TestMenu_Phase4()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var shipment = Factory.New<ForwardingShipment>();
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPlugin(shipment))
			{
				var menuItem = plugIn.TopLevelMenu;

				CombineAssertions(() =>
				{
					AssertNotNull(menuItem);
					AssertType(typeof(NctsMessagingMenu), menuItem);
				});
			}
		}

		public void TestMenu_ExistingNctsHeaderIsPhase4()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;

			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPlugin(shipment))
			{
				var menuItem = plugIn.TopLevelMenu;

				CombineAssertions(() =>
				{
					AssertEquals(true, plugIn.Enabled);
					AssertNotNull(menuItem);
					AssertType(typeof(NctsMessagingMenu), menuItem);
				});
			}
		}

		public void TestMenu_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);

			var shipment = Factory.New<ForwardingShipment>();
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPlugin(shipment))
			{
				var menuItem = plugIn.TopLevelMenu;

				CombineAssertions(() =>
				{
					AssertNotNull(menuItem);
					AssertType(typeof(Phase5NctsMessagingMenuItem), menuItem);
				});
			}
		}

		public void TestPhase4ContainerSynchronisation()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBNRW";
			consol.JK_RL_NKDischargePort = "FRPAR";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "009008";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "GBNRW";
			shipment.JS_RL_NKDestination = "FRPAR";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON102";
			container.JC_SealNum = "SEAL102";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var header = (NctsHeader)Factory.New<Integration.Customs.FR.ICusInBondHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header.BH_ParentID = shipment.PK;
				header.BH_ParentTableCode = shipment.TablePrefix;
				using (ObjectFactory.Substitute(mockSettings.Object))
				using (var plugIn = new NctsPlugin(shipment))
				{
					AssertSame(header, plugIn.InBond);
					header.DepartureHeaderContainers.Load();
					AssertEquals("header.DepartureHeaderContainers.Count", 1, header.DepartureHeaderContainers.Count);
					var departureHeaderContainer = header.DepartureHeaderContainers[0];
					AssertEquals("departureHeaderContainer.BC_ContainerNum", "CON102", departureHeaderContainer.BC_ContainerNum);
				}
			}
		}

		public void TestApplicationCode_Phase4()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var shipment = Factory.New<ForwardingShipment>();
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				AssertEquals(CusInBondApplicationCodeList.Codes.NCTS4, plugIn.ApplicationCodeExposed);
			}
		}

		public void TestApplicationCode_Phase5()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);

			var shipment = Factory.New<ForwardingShipment>();
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				AssertEquals(CusInBondApplicationCodeList.Codes.NCTS5, plugIn.ApplicationCodeExposed);
			}
		}

		public void TestSynchronisationIsFired_Departure()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "LVRIX";
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				plugIn.Enabled = true;
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertEquals("User said yes -> Departure", true, plugIn.InBond.IsDepartureMovement);
					AssertEquals("Synching has occurred", "LVRIX", plugIn.InBond.MovementHeader.BM_PlaceOfUnloading);
				});
			}
		}

		public void TestSynchronisationIsFired_Arrival()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "LVRIX";
				using (var plugIn = new NctsPluginForTest(shipment))
				{
					plugIn.Enabled = true;
					plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

					CombineAssertions(() =>
					{
						AssertEquals("User said no -> Arrival", true, plugIn.InBond.IsArrivalMovement);
						AssertEquals("Synching has occurred", "LV", plugIn.InBond.ArrivalMovementHeader.BM_RL_NKDestinationPort);
					});
				}
			}
		}

		public void TestSynchronisationIsFired_Cancel()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "LVRIX";
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				plugIn.Enabled = true;
				var createdOK = plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertEquals("User said cancel -> nothing", false, createdOK);
					AssertEquals("User said cancel -> nothing", null, plugIn.InBond);
				});
			}
		}

		[TestDate(1998, 08, 09)]
		public void TestNctsLicenceIsLoggedForDeparturePlugin()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var query = GetActivityLogQuery();
			var shipment = GetShipmentForTest();

			using (var plugIn = new NctsPluginForTest(shipment))
			{
				Env.Licence.NCTS.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals("NCTS log entry expected for Departure", 1, Factory.Load<StmActivityLog>(query).Length);
			}
		}

		[TestDate(1998, 08, 09)]
		public void TestNctsLicenceIsLoggedForArrivalPlugin()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var query = GetActivityLogQuery();
			var shipment = GetShipmentForTest();

			using (var plugIn = new NctsPluginForTest(shipment))
			{
				Env.Licence.NCTS.SetLastConsumptionLogCreatedForTest(ZDateTime.Empty);
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
				AssertEquals("NCTS log entry expected for Arrival", 1, Factory.Load<StmActivityLog>(query).Length);
			}
		}

		public void TestShipmentPlugin_EuNctsMovementNew_IsAllowed()
		{
			Env.Security.EuNctsMovementNew.IsAllowed = true;
			Env.Security.USInBondNew.IsAllowed = false;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			shipment.JS_RL_NKOrigin = "GBNRW";
			shipment.JS_RL_NKDestination = "NLROT";
			using (var plugin = new NctsPluginForTest(shipment))
			{
				plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertNotEquals("Expect no Staff or Group Security Rights to allow access to ... message", Env.Security.USInBondNew.ErrorMessageForNotAllowed, plugin.PlugInNotDisplayedMessage);
					AssertNotNull("Expect an NctsHeader", plugin.InBond);
				});
			}
		}

		public void TestConsolPlugin_EuNctsMovementNew_IsAllowed()
		{
			Env.Security.EuNctsMovementNew.IsAllowed = true;
			Env.Security.USInBondNew.IsAllowed = false;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBNRW";
			consol.JK_RL_NKDischargePort = "NLROT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "009008";
			using (var plugin = new NctsPluginForTest(consol))
			{
				plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertNotEquals("Expect no Staff or Group Security Rights to allow access to ... message", Env.Security.USInBondNew.ErrorMessageForNotAllowed, plugin.PlugInNotDisplayedMessage);
					AssertNotNull("Expect an NctsHeader", plugin.InBond);
				});
			}
		}

		public void TestShipmentPlugin_Phase5_Departure()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new NctsPluginForTest(shipment))
			{
				plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertNotNull("Expect an NctsHeader", plugin.InBond);
					AssertNotNull("Expect an MovementHeader", plugin.InBond.MovementHeader);
					AssertEquals("Expect an NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugin.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase5_Arrival()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new NctsPluginForTest(shipment))
			{
				plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertNotNull("Expect an NctsHeader", plugin.InBond);
					AssertNotNull("Expect an ArrivalMovementHeader", plugin.InBond.ArrivalMovementHeader);
					AssertEquals("Expect an NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugin.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase5App_ExistingPhase5Departure()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect a MovementHeader", plugIn.InBond.MovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase5App_ExistingPhase4Departure()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect a MovementHeader", plugIn.InBond.MovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase4App_ExistingPhase5Departure()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect a MovementHeader", plugIn.InBond.MovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase4App_ExistingPhase4Departure()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect a MovementHeader", plugIn.InBond.MovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase5App_ExistingPhase5Arrival()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect an ArrivalMovementHeader", plugIn.InBond.ArrivalMovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase5App_ExistingPhase4Arrival()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect an ArrivalMovementHeader", plugIn.InBond.ArrivalMovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase4App_ExistingPhase5Arrival()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect an ArrivalMovementHeader", plugIn.InBond.ArrivalMovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS5", CusInBondApplicationCodeList.Codes.NCTS5, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		public void TestShipmentPlugin_Phase4App_ExistingPhase4Arrival()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			var shipment = GetShipmentForTest();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ParentID = shipment.PK;
			nctsHeader.BH_ParentTableCode = shipment.TablePrefix;
			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var plugIn = new NctsPluginForTest(shipment))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expect ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.ApplicationCodeExposed);
					AssertEquals("Expect same nctsHeader already associated", nctsHeader, plugIn.InBond);
					AssertNotNull("Expect an ArrivalMovementHeader", plugIn.InBond.ArrivalMovementHeader);
					AssertEquals("Expect InBond.BH_ApplicationCode NCTS4", CusInBondApplicationCodeList.Codes.NCTS4, plugIn.InBond.BH_ApplicationCode);
				});
			}
		}

		ZQuery GetActivityLogQuery()
		{
			var query = new ZQuery(StmActivityLogSchema.S7_FormCaption, SQLComparisonOperator.Like, "NCT%");
			query.AddToFilter(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.Equal, Convert.ToDateTime("1998-08-09 00:00:00"));
			AssertEquals("No NCTS log entries expected", 0, Factory.Load<StmActivityLog>(query).Length);
			return query;
		}

		public void TestConsolPlugin_Phase5_Departure_MultipleShipments()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
			{
				var shipment1 = GetShipmentForTest();
				var shipment2 = GetShipmentForTest();
				var consol = Factory.New<ForwardingConsol>();
				consol.Shipments.Add(shipment1);
				consol.Shipments.Add(shipment2);

				using var plugin = new NctsPluginForTest(consol);
				plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

				CombineAssertions(() =>
				{
					AssertNotNull("InBond NctsHeader", plugin.InBond);
					AssertType<ZMessageBox>(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Last Message",
						"No NCTS Movement exists. What type of NCTS Movement would you like to create?",
						UnitTestUserNotification.Instance.LastMessage?.Text ?? string.Empty);
				});
			}
		}

		ForwardingShipment GetShipmentForTest()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			shipment.JS_RL_NKOrigin = "GBLHR";
			shipment.JS_RL_NKDestination = "LVRIX";
			return shipment;
		}

		class NctsPluginForTest : NctsPlugin
		{
			public NctsPluginForTest(ICusInBondParent hostBusinessEntity)
			: base(hostBusinessEntity)
			{
			}

			public string ApplicationCodeExposed => ApplicationCode;

			public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
