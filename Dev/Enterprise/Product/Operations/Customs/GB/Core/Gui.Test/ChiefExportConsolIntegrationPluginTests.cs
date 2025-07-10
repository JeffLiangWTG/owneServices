using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirInventory;
using Enterprise.Customs.GB.GUI.Ccsuk.Testing;
using Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	internal class ChiefExportConsolIntegrationPlugInTests : ZPlugInGenericTest
	{
		public void TestTopLevelDataSourceType()
		{
			using (var control = new MawbExportAddInfoUserControl())
			{
				AssertEquals(typeof(MawbExportAddInfo), control.DataSourceType);
				AssertEquals(typeof(MawbExportAddInfo), ((ITopLevelDataSourceType)control).DataSourceType);
			}
		}

		public void TestControlCaption()
		{
			using (var control = new ChiefExportConsolIntegrationUserControl(new CustomsExportConsolIntegrationWrapper()))
			{
				var chiefDataTab = control.FindSingle<ZTabPage>("ChiefDataTab");

				AssertEquals("Customs Data", chiefDataTab.Text);
			}

			using (var control = new MawbExportAddInfoUserControl())
			{
				var chiefGroupBox = control.FindSingle<ZGroupBox>("ChiefGroupBox");
				var messagingGroupBox = control.FindSingle<ZGroupBox>("MessagingGroupBox");

				AssertEquals("Last Data From Customs/CCSUK", chiefGroupBox.Text);
				AssertEquals("Data for Customs Messaging and Documents", messagingGroupBox.Text);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			consolParent = Factory.New<ForwardingConsol>();
			consolParent.JK_RL_NKLoadPort = "GBXXX";
			consolParent.JK_RL_NKDischargePort = "AUXXX";
			consolParent.JK_MasterBillNum = "MB0001";
			consolParent.JK_TransportMode = "AIR";
			return new ChiefExportConsolIntegrationPlugIn(consolParent);
		}

		public void TestMenuAndPlugin()
		{
			MawbTestHelper.MakeDepBadge("XAA", GatewayList.Codes.CCSUKviaNTMsgGW, true);
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR");

			consolParent = Factory.New<ForwardingConsol>();
			consolParent.JK_RL_NKLoadPort = "GBLHR";
			consolParent.JK_RL_NKDischargePort = "AUXXX";
			consolParent.JK_MasterBillNum = "MB0001";
			consolParent.JK_TransportMode = "SEA";
			GBCustomsDataRegistry.Instance.ChiefExportConsolIntegrationShowForAirOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertChiefCcsukMenuAndPlugin(true, true);

			consolParent.JK_MasterBillNum = "";
			AssertChiefCcsukMenuAndPlugin(false, true);
			consolParent.JK_MasterBillNum = "MB0001";

			consolParent.JK_RL_NKLoadPort = "AEXXX";
			AssertChiefCcsukMenuAndPlugin(false, false);
			consolParent.JK_RL_NKLoadPort = "GBLHR";

			GBCustomsDataRegistry.Instance.ChiefExportConsolIntegrationShowForAirOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertChiefCcsukMenuAndPlugin(false, false);
			consolParent.JK_TransportMode = "AIR";
			consolParent.JK_MasterBillNum = "MB0001";
			AssertChiefCcsukMenuAndPlugin(true, true);
		}

		void AssertChiefCcsukMenuAndPlugin(bool expectedMenuEnabled, bool expectedTabEnabled)
		{
			using (var plugin = new ChiefExportConsolIntegrationPlugIn(consolParent))
			{
				if (expectedTabEnabled)
				{
					AssertEquals(true, plugin.Enabled);
					AssertEquals(1, plugin.UserControl.Controls.Find("ChiefDataTab", true).Length);

					GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					var rootMenuForRefresh = (EDIMenu)plugin.TopLevelMenu;
					// Agent
					plugin.ChiefExportConsolIntegrationWrapper.MawbExportHelper.ME_Profile = "ABC";
					rootMenuForRefresh.RefreshMenu();
					AssertEquals("Customs && CCS-UK", rootMenuForRefresh.Text);
					if (expectedMenuEnabled)
					{
						var chiefMenu = (EDIMenu)rootMenuForRefresh.MenuItems[0];
						var ccsukMenu = (EDIMenu)rootMenuForRefresh.MenuItems[1];
						var cdsMenu = (EDIMenu)rootMenuForRefresh.MenuItems[2];
						chiefMenu.RefreshMenu();
						ccsukMenu.RefreshMenu();
						cdsMenu.RefreshMenu();
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "CHIEF", "CCS-UK", "CDS" }, rootMenuForRefresh);
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "EAC", "DEC", "EAA", "EAL", "EDL", "Close Master" }, chiefMenu);
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "FSR", "FSR (omit shed)", "Good to Go" }, ccsukMenu);
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "Close Master", "Query Master", "Anticipate", "Arrive", "Depart" }, cdsMenu);
						AssertEquals(expectedMenuEnabled, chiefMenu.Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[0].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[1].Enabled);
						AssertEquals(false, chiefMenu.MenuItems[2].Enabled);
						AssertEquals(false, chiefMenu.MenuItems[3].Enabled);
						AssertEquals(false, chiefMenu.MenuItems[4].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[5].Enabled);
						AssertEquals(expectedMenuEnabled, ccsukMenu.Enabled);
						AssertEquals(expectedMenuEnabled, ccsukMenu.MenuItems[0].Enabled);
						AssertEquals(expectedMenuEnabled, ccsukMenu.MenuItems[1].Enabled);
						AssertEquals(false, ccsukMenu.MenuItems[2].Enabled);

						AssertEquals(expectedMenuEnabled, cdsMenu.Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[0].Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[1].Enabled);
						AssertEquals(false, cdsMenu.MenuItems[2].Enabled);
						AssertEquals(false, cdsMenu.MenuItems[3].Enabled);
						AssertEquals(false, cdsMenu.MenuItems[4].Enabled);
					}
					// IsDEP
					plugin.ChiefExportConsolIntegrationWrapper.MawbExportHelper.ME_Profile = "XAA";
					rootMenuForRefresh.RefreshMenu();
					AssertEquals("Customs && CCS-UK", rootMenuForRefresh.Text);
					if (expectedMenuEnabled)
					{
						var chiefMenu = (EDIMenu)rootMenuForRefresh.MenuItems[0];
						var ccsukMenu = (EDIMenu)rootMenuForRefresh.MenuItems[1];
						var cdsMenu = (EDIMenu)rootMenuForRefresh.MenuItems[2];
						chiefMenu.RefreshMenu();
						ccsukMenu.RefreshMenu();
						using (GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
						{
							cdsMenu.RefreshMenu();
						}
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "CHIEF", "CCS-UK", "CDS" }, rootMenuForRefresh);
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "EAC", "DEC", "EAA", "EAL", "EDL", "Close Master" }, chiefMenu);
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "FSR", "FSR (omit shed)", "Good to Go" }, ccsukMenu);
						CcsukMenuTests.AssertMenuItemsContain(new string[] { "Close Master", "Query Master", "Anticipate", "Arrive", "Depart" }, cdsMenu);
						AssertEquals(expectedMenuEnabled, chiefMenu.Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[0].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[1].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[2].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[3].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[4].Enabled);
						AssertEquals(expectedMenuEnabled, chiefMenu.MenuItems[5].Enabled);
						AssertEquals(expectedMenuEnabled, ccsukMenu.Enabled);
						AssertEquals(expectedMenuEnabled, ccsukMenu.MenuItems[0].Enabled);
						AssertEquals(expectedMenuEnabled, ccsukMenu.MenuItems[1].Enabled);
						AssertEquals(false, ccsukMenu.MenuItems[2].Enabled);

						AssertEquals(expectedMenuEnabled, cdsMenu.Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[0].Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[1].Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[2].Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[3].Enabled);
						AssertEquals(expectedMenuEnabled, cdsMenu.MenuItems[4].Enabled);

						GBCustomsDataRegistry.Instance.ChiefFallbackExports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						rootMenuForRefresh.RefreshMenu();
						ccsukMenu = (EDIMenu)rootMenuForRefresh.MenuItems[1];
						ccsukMenu.RefreshMenu();
						AssertEquals(expectedMenuEnabled, ccsukMenu.MenuItems[2].Enabled);
					}
				}
				else
				{
					AssertEquals(false, plugin.Enabled);
					AssertEquals(1, plugin.UserControl.Controls.Find("ChiefDataTab", true).Length);

					var rootMenuForRefresh = (EDIMenu)plugin.TopLevelMenu;
					plugin.ChiefExportConsolIntegrationWrapper.MawbExportHelper.ME_Profile = "ABC";
					AssertEquals("Customs && CCS-UK", rootMenuForRefresh.Text);
					rootMenuForRefresh.RefreshMenu();
					AssertEquals(0, rootMenuForRefresh.MenuItems.Count);
				}
			}
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestDirectMessaging_AutoAssociateDisassociateDucrConsol()
		{
			RunShipmentAndConsolAssociateTest(Factory,
								delegate(ForwardingConsol c, ForwardingShipment s)
								{ return new ChiefExportConsolIntegrationPlugIn(c); },
								delegate(ForwardingConsol c, ForwardingShipment s)
								{ c.Shipments.Add(s); },
								delegate(ForwardingConsol c, ForwardingShipment s)
								{ c.Shipments.Remove(s); }
								);
		}

		public delegate IDisposable GetShipmentOrConsolPluginDelgate(ForwardingConsol c, ForwardingShipment s);
		public delegate void AttachShipmentAndConsolDelegate(ForwardingConsol c, ForwardingShipment s);
		public delegate void DetachShipmentAndConsolDelegate(ForwardingConsol c, ForwardingShipment s);

		public static void RunShipmentAndConsolAssociateTest(BusinessObjectFactory factory, GetShipmentOrConsolPluginDelgate getPluginDelgate, AttachShipmentAndConsolDelegate attach, DetachShipmentAndConsolDelegate detach)
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR", GB.Registry.MucrGenerationStyles.Codes.Air);
			ForwardingConsol consolParent = factory.New<ForwardingConsol>();
			consolParent.JK_TransportMode = "AIR";
			consolParent.JK_RL_NKLoadPort = "GBLHR";
			consolParent.JK_RL_NKDischargePort = "AUSYD";
			consolParent.JK_MasterBillNum = "125-12345678";
			var sendsMessagesToCustoms = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var wrapperOnlyForPreparingConsol = new CustomsExportConsolIntegrationWrapper(consolParent, sendsMessagesToCustoms);
			wrapperOnlyForPreparingConsol.MawbExportHelper.ME_Profile = "ABC";
			var exportShipment = factory.New<ForwardingShipment>();
			exportShipment.JS_RL_NKOrigin = "GBLHR";
			exportShipment.JS_RL_NKDestination = "AUSYD";
			exportShipment.JS_TransportMode = "AIR";
			var exportDeclarationWithManyEntries = factory.New<JobDeclaration>();
			exportDeclarationWithManyEntries.JE_JS = exportShipment.PK;
			exportDeclarationWithManyEntries.JE_MessageType = "EXP";
			exportDeclarationWithManyEntries.JE_UCR = "1GB123456789000-Poop";
			var ceh1 = exportDeclarationWithManyEntries.CustomsEntryHeaders.AddNew();
			var ceh2 = exportDeclarationWithManyEntries.CustomsEntryHeaders.AddNew();
			ceh1.CH_BGMReference = "DUCR1";
			ceh2.CH_BGMReference = "DUCR2";

			var importShipment = factory.New<ForwardingShipment>();
			importShipment.JS_RL_NKOrigin = "AUSYD";
			importShipment.JS_RL_NKDestination = "GBLHR";
			importShipment.JS_TransportMode = "AIR";
			var importDeclaration = factory.New<JobDeclaration>();
			importDeclaration.JE_JS = importShipment.PK;
			importDeclaration.JE_MessageType = "IMP";
			importDeclaration.JE_UCR = "1GB123456789000-Crap";
			importDeclaration.CustomsEntryHeaders.AddNew();

			var shipmentWithExternalDucrs = factory.New<ForwardingShipment>();
			shipmentWithExternalDucrs.JS_RL_NKOrigin = "GBLHR";
			shipmentWithExternalDucrs.JS_RL_NKDestination = "AUSYD";
			shipmentWithExternalDucrs.JS_TransportMode = "AIR";
			var num1 = shipmentWithExternalDucrs.Numbers.AddNew();
			num1.CE_EntryNum = "External1";
			num1.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			var num2 = shipmentWithExternalDucrs.Numbers.AddNew();
			num2.CE_EntryNum = "External2";
			num2.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			var num3 = shipmentWithExternalDucrs.Numbers.AddNew();
			num3.CE_EntryNum = "Irrelevant";
			num3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			var cStatusExportShipment = factory.New<ForwardingShipment>();
			cStatusExportShipment.JS_RL_NKOrigin = "GBLHR";
			cStatusExportShipment.JS_RL_NKDestination = "AUSYD";
			cStatusExportShipment.JS_TransportMode = "AIR";
			cStatusExportShipment.JS_CommunityTransitStatus = "C";
			factory.Save();

			using (var plugin = getPluginDelgate(consolParent, exportShipment))
			{
				// Do nothing, but if we get the expected number of messages below then the unhooking on dispose is working properly.
			}

			using (var plugin = getPluginDelgate(consolParent, exportShipment))
			{
				attach(consolParent, exportShipment);
				AssertEquals("One ASS message for each entry header", 2, consolParent.Messages.Count);
				AssertContains("BGM+EAC:105:109'RFF+ABO:DUCR1'RFF+UCN:A?:12512345678'UNS", consolParent.Messages[0].EM_MessageText);
				AssertContains("BGM+EAC:105:109'RFF+ABO:DUCR2'RFF+UCN:A?:12512345678'UNS", consolParent.Messages[1].EM_MessageText);
				AssertEquals("A:12512345678", exportDeclarationWithManyEntries.JE_MasterUCR);
				detach(consolParent, exportShipment);
				AssertEquals("Two more messages, for DIS", 4, consolParent.Messages.Count);
				AssertContains("BGM+EAC:105:109'RFF+ABO:DUCR1'UNS", consolParent.Messages[2].EM_MessageText);
				AssertContains("BGM+EAC:105:109'RFF+ABO:DUCR2'UNS", consolParent.Messages[3].EM_MessageText);
				AssertEquals("", exportDeclarationWithManyEntries.JE_MasterUCR);

				wrapperOnlyForPreparingConsol.MawbExportHelper.ME_Profile = "";
				attach(consolParent, exportShipment);
				AssertEquals("No extra message created when profile not present", 4, consolParent.Messages.Count);

				wrapperOnlyForPreparingConsol.MawbExportHelper.ME_Profile = "ABC";
			}

			using (var plugin = getPluginDelgate(consolParent, importShipment))
			{
				attach(consolParent, importShipment);
				AssertEquals("No extra message created when import shipment added", 4, consolParent.Messages.Count);
			}

			using (var plugin = getPluginDelgate(consolParent, shipmentWithExternalDucrs))
			{
				attach(consolParent, shipmentWithExternalDucrs);
				AssertEquals("Two more messages, one ASS message for each external DUCR", 6, consolParent.Messages.Count);
				AssertContains("BGM+EAC:105:109'RFF+ABO:EXTERNAL1'RFF+UCN:A?:12512345678'UNS", consolParent.Messages[4].EM_MessageText);
				AssertContains("BGM+EAC:105:109'RFF+ABO:EXTERNAL2'RFF+UCN:A?:12512345678'UNS", consolParent.Messages[5].EM_MessageText);
			}

			sendsMessagesToCustoms = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			using (var plugin = getPluginDelgate(consolParent, cStatusExportShipment))
			{
				consolParent.JK_SystemLastEditTimeUtc = ZDateTime.Now.AddDays(-1);
				attach(consolParent, cStatusExportShipment);
				AssertEquals("No additional message for C-status shipment", 6, consolParent.Messages.Count);
				AssertEquals("Edit date recored when messaging", ZDateTime.Now, consolParent.JK_SystemLastEditTimeUtc);
			}

			var consolNonExport = factory.New<ForwardingConsol>();
			consolNonExport.JK_TransportMode = "AIR";

			exportShipment.Consols.RemoveAll();
			importShipment.Consols.RemoveAll();
			using (var plugin = getPluginDelgate(consolNonExport, exportShipment))
			{
				attach(consolNonExport, exportShipment);
				AssertEquals(0, consolNonExport.Messages.Count);
			}
			using (var plugin = getPluginDelgate(consolNonExport, importShipment))
			{
				attach(consolNonExport, importShipment);
				AssertEquals(0, consolNonExport.Messages.Count);
			}
			consolNonExport.JK_RL_NKLoadPort = "AUSYD";
			consolNonExport.JK_RL_NKDischargePort = "GBLHR";
			exportShipment.Consols.RemoveAll();
			importShipment.Consols.RemoveAll();
			using (var plugin = getPluginDelgate(consolNonExport, exportShipment))
			{
				attach(consolNonExport, exportShipment);
				AssertEquals(0, consolNonExport.Messages.Count);
			}
			using (var plugin = getPluginDelgate(consolNonExport, importShipment))
			{
				attach(consolNonExport, importShipment);
				AssertEquals(0, consolNonExport.Messages.Count);
			}
		}

		public void TestShipmentAddedToOrRemovedFromConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "USXXX";
			consol.JK_RL_NKDischargePort = "GBXXX";
			var sendsMessagesToCustomsGUI = new SendsMessagesToCustomsGUI();
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, sendsMessagesToCustomsGUI);
			var importShipment = Factory.New<ForwardingShipment>();
			importShipment.JS_RL_NKOrigin = "USXXX";
			importShipment.JS_RL_NKDestination = "GBXXX";
			importShipment.JS_TransportMode = "AIR";
			var exportShipment = Factory.New<ForwardingShipment>();
			exportShipment.JS_RL_NKOrigin = "GBXXX";
			exportShipment.JS_RL_NKDestination = "USXXX";
			exportShipment.JS_TransportMode = "AIR";

			Factory.Save();

			using (var plugin = new ChiefExportConsolIntegrationPlugIn(consol))
			{
				consol.Shipments.Add(exportShipment);
				AssertEquals("No unhandled exception when attaching a export shipment to an import consol", 0, consol.Messages.Count);
				consol.Shipments.Add(importShipment);
				AssertEquals("No unhandled exception when attaching a import shipment to an import consol", 0, consol.Messages.Count);
			}
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestDirectMessaging_AutoAssociateConsolWithNoEntryHeaders()
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR", GB.Registry.MucrGenerationStyles.Codes.Air);
			consolParent = Factory.New<ForwardingConsol>();
			consolParent.JK_TransportMode = "AIR";
			consolParent.JK_RL_NKLoadPort = "GBLHR";
			consolParent.JK_RL_NKDischargePort = "AUSYD";
			consolParent.JK_MasterBillNum = "125-12345678";
			var sendsMessagesToCustoms = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var wrapper = new CustomsExportConsolIntegrationWrapper(consolParent, sendsMessagesToCustoms);
			wrapper.MawbExportHelper.ME_Profile = "ABC";

			var exportShipment = Factory.New<ForwardingShipment>();
			exportShipment.JS_RL_NKOrigin = "GBLHR";
			exportShipment.JS_RL_NKDestination = "AUSYD";
			exportShipment.JS_TransportMode = "AIR";

			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_JS = exportShipment.PK;
			Factory.Save();

			using (var plugin = new ChiefExportConsolIntegrationPlugIn(consolParent))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consolParent.Shipments.Add(exportShipment);
				AssertContains("No EAC message sent", "An EAC message could not be sent", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var exportShipment2 = Factory.New<ForwardingShipment>();
			exportShipment2.JS_RL_NKOrigin = "GBLHR";
			exportShipment2.JS_RL_NKDestination = "AUSYD";
			exportShipment2.JS_TransportMode = "AIR";

			var exportDeclaration2 = Factory.New<JobDeclaration>();
			exportDeclaration2.JE_JS = exportShipment2.PK;
			exportDeclaration2.JE_MessageType = "EXP";
			exportDeclaration2.JE_UCR = "1GB123456789000-XXX";
			var ceh1 = exportDeclaration2.CustomsEntryHeaders.AddNew();
			ceh1.CH_BGMReference = "DUCR1";
			Factory.Save();

			using (var plugin = new ChiefExportConsolIntegrationPlugIn(consolParent))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consolParent.Shipments.Add(exportShipment2);
				AssertEquals("One EAC messages sent", 1, consolParent.Messages.Count);
				AssertContains("BGM+EAC:105:109'RFF+ABO:DUCR1'RFF+UCN:A?:12512345678'UNS+D'UNS", consolParent.Messages[0].EM_MessageText);
			}
		}

		public void TestDirectMessaging_AutoAssociateDisassociateConsolInConsol()
		{
			MawbTestHelper.MakeBadge("ABC", GatewayList.Codes.CCSUKviaNTMsgGW, "CUKFFW98000", true, "", true, false, BadgeDirectionList.Codes.EXP, "GBLHR", GB.Registry.MucrGenerationStyles.Codes.Air);
			consolParent = Factory.New<ForwardingConsol>();
			consolParent.JK_TransportMode = "AIR";
			consolParent.JK_RL_NKLoadPort = "GBLHR";
			consolParent.JK_RL_NKDischargePort = "AUSYD";
			consolParent.JK_MasterBillNum = "125-12345678";
			consolParent.JK_UniqueConsignRef = "C00001000";
			var consolChild = Factory.New<ForwardingConsol>();
			consolChild.JK_TransportMode = "AIR";
			consolChild.JK_RL_NKLoadPort = "GBLHR";
			consolChild.JK_RL_NKDischargePort = "AUSYD";
			consolChild.JK_MasterBillNum = "999-87654321";
			consolChild.JK_UniqueConsignRef = "C00001001";
			var shutUp = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var wrapperForParent = new CustomsExportConsolIntegrationWrapper(consolParent, shutUp);
			wrapperForParent.MawbExportHelper.ME_Profile = "ABC";
			wrapperForParent.MawbExportHelper.ME_ChiefConsolIsClosed = true;

			Factory.Save();

			using (var plugin = new ChiefExportConsolIntegrationPlugIn(consolParent))
			{
				wrapperForParent.RelatedConsols.Add(consolChild);
				AssertNotContains("master consol is not known to be closed", shutUp.PastYesNoCancelQuestionsAsked[0]);
				AssertContains("Do you wish to send an EAC message/s to associate Consol C00001001 (Master Bill='99987654321') to consolidation A:12512345678?", shutUp.PastYesNoCancelQuestionsAsked[0]);
				AssertEquals(1, consolParent.Messages.Count);
				AssertContains("BGM+EAC:105:109'RFF+ABO:A?:99987654321'RFF+UCN:A?:12512345678'UNS", consolParent.Messages[0].EM_MessageText);
				shutUp.PastYesNoCancelQuestionsAsked.Clear();

				wrapperForParent.RelatedConsols.RemoveFromRelationship(consolChild);
				AssertEquals(2, consolParent.Messages.Count);
				AssertContains("BGM+EAC:105:109'RFF+ABO:A?:99987654321'UNS", consolParent.Messages[1].EM_MessageText);
				//AssertContains("Do you wish to send an EAC message/s to disassociate Consol C00001001 (Master Bill='99987654321') from consolidation A:12512345678?", shutUp.PastYesNoQuestionsAsked[0]);
				AssertEquals(0, shutUp.PastYesNoCancelQuestionsAsked.Count);
				wrapperForParent.MawbExportHelper.ME_ChiefConsolIsClosed = false;
				wrapperForParent.RelatedConsols.Add(consolChild);
			}
		}

		ForwardingConsol consolParent;
	}
}
