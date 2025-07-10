using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;
using MessageStatusList = Enterprise.Customs.CA.Business.MessageStatusList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ShipmentCargoReportPlugInTest : ZPlugInGenericTest
	{
		[ExpectNoExceptions]
		public void TestChooseNotToCreateACIWhenClickMenuItem()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugin = new ShipmentCargoReportPlugIn(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var menu = plugin.TopLevelMenu;
				var sendMessageMenuItem = menu.MenuItems[0];
				sendMessageMenuItem.PerformClick();
			}
		}

		public void TestVisibilityChangesWithMode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			var shipment = consol.Shipments.AddNew();
			using (var plugIn = new ShipmentCargoReportPlugIn(shipment))
			{
				AssertEquals(true, plugIn.Enabled);
				shipment.JS_TransportMode = Constants.TransportModes.Road;
				AssertEquals(false, plugIn.Enabled);
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals(true, plugIn.Enabled);
				shipment.JS_TransportMode = Constants.TransportModes.Rail;
				AssertEquals(false, plugIn.Enabled);
				consol.Transports[0].JW_RL_NKDiscPort = "USNYK";
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				AssertEquals(false, plugIn.Enabled);
			}
		}

		public void TestCusSCAHouse_OnApplicationTypeChanged()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			CusSCAHouse houseBill1;
			using (var plugIn = new ShipmentCargoReportPlugIn(shipment1))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				houseBill1 = (CusSCAHouse)plugIn.BusinessEntity;
				houseBill1.CA_FROBTransitImportCode = InTransitCodeList.Codes.FROB;
			}

			CusSCAHouse houseBill2;
			using (var plugIn = new ShipmentCargoReportPlugIn(shipment2))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				houseBill2 = (CusSCAHouse)plugIn.BusinessEntity;
				AssertEquals("CA_FROBTransitImportCode should be defaulted from first house bill", InTransitCodeList.Codes.FROB, houseBill2.CA_FROBTransitImportCode);
			}
		}

		public void TestPlugIn_UsingUserNotification()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";
			var shipment0 = consol.Shipments.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00000011";
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_GoodsDescription = "XXXXX";
			var packLine = shipment1.OuterPackLines.AddNew();
			container.PackLines.Add(packLine);
			using (var form = new ShipmentForm(shipment1))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport);
				plugIn.SetNewNotification(new Customs.GUI.UserNotification());
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				var bizo = (CusSCAHouse)plugIn.BusinessEntity;
				AssertNotNull(bizo);
				Assert("Ocean bill mutex", plugIn.Mutex.RecordIdentifier.Contains("_Only1ACIperCAShipment"));
				Assert("Mutex is locked", plugIn.Mutex.HasLock);
				var menu = plugIn.TopLevelMenu;
				AssertEquals("ACI", menu.Text);
				AssertEquals(5, menu.MenuItems.Count);
				var sendMessageMenuItem = menu.MenuItems[0];
				AssertEquals("sendMessageMenuItem.Text", "Send ACI Supplementary (House) Cargo Message", sendMessageMenuItem.Text);
				sendMessageMenuItem.PerformClick();
				AssertEquals("LastMessage.Text", "System cannot send an ACI Supplementary Cargo Report for S00000011 message as Job not yet saved, Please save before sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message sent", 0, bizo.Messages.Count);
				Factory.Save();
				sendMessageMenuItem.PerformClick();
				AssertEquals("LastMessage.Text", @"System cannot send an ACI Supplementary Cargo Report for S00000011 message as The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "2222");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMessageMenuItem.PerformClick();
				AssertEquals("original sent, 1 message on job", 1, bizo.Messages.Count);
				AssertEquals("Sent with message errors", ZBool.True, bizo.Messages[0].EM_SendWithMessageErrors);
				AssertEquals("waiting status", MessageStatusList.Codes.AwaitingOriginal, bizo.CA_MessageStatus);
				var сommencedEvent = bizo.Shipment.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "ACI");
				AssertNotNull("Customs Commenced event should exist on shipment", сommencedEvent);
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMessageMenuItem.PerformClick();
				AssertEquals("LastMessage.Text", "This job is waiting for a CBSA response.\r\nAre you sure that you want to resend to the CBSA?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No message sent", 1, bizo.Messages.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMessageMenuItem.PerformClick();
				AssertEquals("change sent", 2, bizo.Messages.Count);
				bizo.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Validated;
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMessageMenuItem.PerformClick();
				AssertEquals("change sent", 3, bizo.Messages.Count);

				var forcedMessagesSubMenu = menu.MenuItems[2];
				AssertEquals("Forced Messages", forcedMessagesSubMenu.Text);
				var sendOriginal = forcedMessagesSubMenu.MenuItems[0];
				AssertEquals("Send ACI Supplementary (House) Original Cargo Message", sendOriginal.Text);
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendOriginal.PerformClick();
				AssertEquals(4, bizo.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, bizo.CA_MessageStatus);
				Factory.Save();

				var sendAmendment = forcedMessagesSubMenu.MenuItems[1];
				AssertEquals("Send ACI Supplementary (House) Amendment Cargo Message", sendAmendment.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendAmendment.PerformClick();
				AssertEquals(5, bizo.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingChange, bizo.CA_MessageStatus);
			}

			using (var form = new ShipmentForm(shipment1))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				var bizo = (CusSCAHouse)plugIn.BusinessEntity;
				AssertNotNull(bizo);
				Assert("No Mutex is locked", !plugIn.Mutex.IsLocked);
			}

			using (var form = new ShipmentForm(shipment0))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				var bizo = (CusSCAHouse)plugIn.BusinessEntity;
				AssertNotNull(bizo);
				Assert("House mutex", plugIn.Mutex.RecordIdentifier.Contains("_Only1ACIperCAShipment"));
				Assert("Mutex is locked", plugIn.Mutex.HasLock);
			}
		}

		public void TestPlugIn_UsingMessageInstructionNotification()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";
			var shipment0 = consol.Shipments.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00000011";
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_GoodsDescription = "XXXXX";
			var packLine = shipment1.OuterPackLines.AddNew();
			container.PackLines.Add(packLine);
			using (var form = new ShipmentForm(shipment1))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport);
				var userNotification = new Business.MessageManagers.Testing.TestMessageInstructionUserNotification();
				plugIn.SetNewNotification(userNotification);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				var bizo = (CusSCAHouse)plugIn.BusinessEntity;
				AssertNotNull(bizo);
				Assert("Ocean bill mutex", plugIn.Mutex.RecordIdentifier.Contains("_Only1ACIperCAShipment"));
				Assert("Mutex is locked", plugIn.Mutex.HasLock);

				var menu = plugIn.TopLevelMenu;
				AssertEquals("ACI", menu.Text);
				AssertEquals(5, menu.MenuItems.Count);

				var sendMessageMenuItem = menu.MenuItems[0];
				AssertEquals("sendMessageMenuItem.Text", "Send ACI Supplementary (House) Cargo Message", sendMessageMenuItem.Text);
				sendMessageMenuItem.PerformClick();
				Assert("messageInstructionUserNotificationNOTshown", !userNotification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("LastMessage.Text", "System cannot send an ACI Supplementary Cargo Report for S00000011 message as Job not yet saved, Please save before sending.", userNotification.LastMessage);
				AssertEquals("No message sent", 0, bizo.Messages.Count);

				Factory.Save();
				userNotification.Reset();
				sendMessageMenuItem.PerformClick();
				Assert("messageInstructionUserNotificationNOTshown", !userNotification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("LastMessage.Text", @"System cannot send an ACI Supplementary Cargo Report for S00000011 message as The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", userNotification.LastMessage);

				userNotification.Reset();
				CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "2222");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMessageMenuItem.PerformClick();
				Assert("messageInstructionUserNotificationshown", userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("messageInstructionUserNotification.IsWaitingForResponse", !userNotification.IsWaitingForResponse);
				Assert("messageInstructionUserNotification.ContainsValidationErrors", userNotification.ContainsValidationErrors);
				Assert("messageInstructionUserNotification.ContainsAdditionalWarnings", !userNotification.ContainsAdditionalWarnings);
				AssertEquals("original sent, 1 message on job", 1, bizo.Messages.Count);
				AssertEquals("Sent with message errors", ZBool.True, bizo.Messages[0].EM_SendWithMessageErrors);
				AssertEquals("waiting status", MessageStatusList.Codes.AwaitingOriginal, bizo.CA_MessageStatus);

				userNotification.Reset();
				var сommencedEvent = bizo.Shipment.Logs.MostRecentLogByEventTime(Events.CustomsCommenced, "ACI");
				AssertNotNull("Customs Commenced event should exist on shipment", сommencedEvent);
				Factory.Save();
				userNotification.NextAnswer = false;
				sendMessageMenuItem.PerformClick();
				Assert("messageInstructionUserNotificationshown", userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("messageInstructionUserNotification.IsWaitingForResponse", userNotification.IsWaitingForResponse);
				Assert("messageInstructionUserNotification.ContainsValidationErrors", userNotification.ContainsValidationErrors);
				Assert("messageInstructionUserNotification.ContainsAdditionalWarnings", !userNotification.ContainsAdditionalWarnings);
				AssertEquals("No message sent", 1, bizo.Messages.Count);

				userNotification.Reset();
				userNotification.NextAnswer = true;
				sendMessageMenuItem.PerformClick();
				Assert("messageInstructionUserNotificationshown", userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("messageInstructionUserNotification.IsWaitingForResponse", userNotification.IsWaitingForResponse);
				Assert("messageInstructionUserNotification.ContainsValidationErrors", userNotification.ContainsValidationErrors);
				Assert("messageInstructionUserNotification.ContainsAdditionalWarnings", !userNotification.ContainsAdditionalWarnings);
				AssertEquals("change sent", 2, bizo.Messages.Count);

				userNotification.Reset();
				bizo.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Validated;
				Factory.Save();
				userNotification.NextAnswer = true;
				sendMessageMenuItem.PerformClick();
				Assert("messageInstructionUserNotificationshown", userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("messageInstructionUserNotification.IsWaitingForResponse", userNotification.IsWaitingForResponse);
				Assert("messageInstructionUserNotification.ContainsValidationErrors", userNotification.ContainsValidationErrors);
				Assert("messageInstructionUserNotification.ContainsAdditionalWarnings", !userNotification.ContainsAdditionalWarnings);
				AssertEquals("change sent", 3, bizo.Messages.Count);

				userNotification.Reset();
				var forcedMessagesSubMenu = menu.MenuItems[2];
				AssertEquals("Forced Messages", forcedMessagesSubMenu.Text);
				var sendOriginal = forcedMessagesSubMenu.MenuItems[0];
				AssertEquals("Send ACI Supplementary (House) Original Cargo Message", sendOriginal.Text);
				Factory.Save();
				userNotification.NextAnswer = true;
				sendOriginal.PerformClick();
				Assert("messageInstructionUserNotificationshown", userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("messageInstructionUserNotification.IsWaitingForResponse", userNotification.IsWaitingForResponse);
				Assert("messageInstructionUserNotification.ContainsValidationErrors", userNotification.ContainsValidationErrors);
				Assert("messageInstructionUserNotification.ContainsAdditionalWarnings", !userNotification.ContainsAdditionalWarnings);
				AssertEquals(4, bizo.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, bizo.CA_MessageStatus);

				Factory.Save();
				userNotification.Reset();
				var sendAmendment = forcedMessagesSubMenu.MenuItems[1];
				AssertEquals("Send ACI Supplementary (House) Amendment Cargo Message", sendAmendment.Text);
				userNotification.NextAnswer = true;
				sendAmendment.PerformClick();
				Assert("messageInstructionUserNotificationshown", userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("messageInstructionUserNotification.IsWaitingForResponse", userNotification.IsWaitingForResponse);
				Assert("messageInstructionUserNotification.ContainsValidationErrors", userNotification.ContainsValidationErrors);
				Assert("messageInstructionUserNotification.ContainsAdditionalWarnings", !userNotification.ContainsAdditionalWarnings);
				AssertEquals(5, bizo.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingChange, bizo.CA_MessageStatus);
			}

			using (var form = new ShipmentForm(shipment1))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport);
				var userNotification = new Business.MessageManagers.Testing.TestMessageInstructionUserNotification();
				plugIn.SetNewNotification(userNotification);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				var bizo = (CusSCAHouse)plugIn.BusinessEntity;
				AssertNotNull(bizo);
				Assert("messageInstrucationUserNotificationNOTShown", !userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("No Mutex is locked", !plugIn.Mutex.IsLocked);
			}

			using (var form = new ShipmentForm(shipment0))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport);
				var userNotification = new Business.MessageManagers.Testing.TestMessageInstructionUserNotification();
				plugIn.SetNewNotification(userNotification);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.InitialiseCusSCAHouse();
				var bizo = (CusSCAHouse)plugIn.BusinessEntity;
				AssertNotNull(bizo);
				Assert("messageInstrucationUserNotificationNOTShown", !userNotification.HasShowMessageInstructionFormBeenCalled);
				Assert("House mutex", plugIn.Mutex.RecordIdentifier.Contains("_Only1ACIperCAShipment"));
				Assert("Mutex is locked", plugIn.Mutex.HasLock);
			}
		}

		public void TestWhenConsolAlreadyLockedHasNoError()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment0 = consol.Shipments.AddNew();
			var shipment1 = consol.Shipments.AddNew();

			var consolMutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol.PK + ShipmentCargoReportPlugIn.ConsolMutexString);
			var shipment0Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment0.PK + ShipmentCargoReportPlugIn.HouseMutexString);
			var shipment1Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment1.PK + ShipmentCargoReportPlugIn.HouseMutexString);

			AssertEquals("shipment0Mutex is unlocked", false, shipment0Mutex.IsLocked);
			AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
			AssertEquals("consolMutex is unlocked", false, consolMutex.IsLocked);
			using (var form0 = new ShipmentForm(shipment0))
			using (var form1 = new ShipmentForm(shipment1))
			{
				form0.Show();
				form1.Show();
				UserIdleWorker.Flush();
				using (var plugIn = (ShipmentCargoReportPlugIn)form0.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport))
				{
					var userNotification = new Business.MessageManagers.Testing.TestMessageInstructionUserNotification();
					plugIn.SetNewNotification(userNotification);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn.InitialiseCusSCAHouse();
					AssertEquals("shipment0Mutex is locked", true, shipment0Mutex.IsLocked);
					AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
					AssertEquals("consolMutex is locked", true, consolMutex.IsLocked);

					using (var plugIn1 = (ShipmentCargoReportPlugIn)form1.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport))
					{
						var userNotification1 = new Business.MessageManagers.Testing.TestMessageInstructionUserNotification();
						plugIn1.SetNewNotification(userNotification1);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						plugIn1.InitialiseCusSCAHouse();
						AssertEquals("shipment0Mutex is locked", true, shipment0Mutex.IsLocked);
						AssertEquals("shipment1Mutex is locked", true, shipment1Mutex.IsLocked);
						AssertEquals("consolMutex is locked", true, consolMutex.IsLocked);
					}
					AssertEquals("shipment0Mutex is locked", true, shipment0Mutex.IsLocked);
					AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
					AssertEquals("consolMutex is locked", true, consolMutex.IsLocked);
				}
			}
			AssertEquals("shipment0Mutex is unlocked", false, shipment0Mutex.IsLocked);
			AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
			AssertEquals("consolMutex is unlocked", false, consolMutex.IsLocked);
		}

		public void TestSpecificMutexesGotLockedandUnlockedCorrectly()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment0 = consol.Shipments.AddNew();
			var shipment1 = consol.Shipments.AddNew();

			var consol0 = shipment0.Consols.AddNew();
			consol0.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol0.JK_TransportMode = Constants.TransportModes.Air;
			var consol1 = shipment0.Consols.AddNew();
			consol1.Transports[0].JW_RL_NKDiscPort = "USTOR";
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			var consol2 = shipment1.Consols.AddNew();
			consol2.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol2.JK_TransportMode = Constants.TransportModes.Air;

			var consolMutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol.PK + ShipmentCargoReportPlugIn.ConsolMutexString);
			var shipment0Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment0.PK + ShipmentCargoReportPlugIn.HouseMutexString);
			var shipment1Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment1.PK + ShipmentCargoReportPlugIn.HouseMutexString);
			var consol0Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol0.PK + ShipmentCargoReportPlugIn.ConsolMutexString);
			var consol1Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol1.PK + ShipmentCargoReportPlugIn.ConsolMutexString);
			var consol2Mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol2.PK + ShipmentCargoReportPlugIn.ConsolMutexString);

			AssertEquals("shipment0Mutex is unlocked", false, shipment0Mutex.IsLocked);
			AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
			AssertEquals("consolMutex is unlocked", false, consolMutex.IsLocked);
			AssertEquals("consol0Mutex is unlocked", false, consol0Mutex.IsLocked);
			AssertEquals("consol1Mutex is unlocked", false, consol1Mutex.IsLocked);
			AssertEquals("consol2Mutex is unlocked", false, consol2Mutex.IsLocked);
			using (var form = new ShipmentForm(shipment0))
			{
				form.Show();
				UserIdleWorker.Flush();
				using (var plugIn = (ShipmentCargoReportPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAShipmentCargoReport))
				{
					var userNotification = new Business.MessageManagers.Testing.TestMessageInstructionUserNotification();
					plugIn.SetNewNotification(userNotification);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn.InitialiseCusSCAHouse();

					AssertEquals("shipment0Mutex is unlocked", true, shipment0Mutex.IsLocked);
					AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
					AssertEquals("consolMutex is unlocked", !consol0Mutex.IsLocked, consolMutex.IsLocked);
					AssertEquals("consol0Mutex is unlocked", !consolMutex.IsLocked, consol0Mutex.IsLocked);
					AssertEquals("consol1Mutex is unlocked", false, consol1Mutex.IsLocked);
					AssertEquals("consol2Mutex is unlocked", false, consol2Mutex.IsLocked);
				}
			}
			AssertEquals("shipment0Mutex is unlocked", false, shipment0Mutex.IsLocked);
			AssertEquals("shipment1Mutex is unlocked", false, shipment1Mutex.IsLocked);
			AssertEquals("consolMutex is unlocked", false, consolMutex.IsLocked);
			AssertEquals("consol0Mutex is unlocked", false, consol0Mutex.IsLocked);
			AssertEquals("consol1Mutex is unlocked", false, consol1Mutex.IsLocked);
			AssertEquals("consol2Mutex is unlocked", false, consol2Mutex.IsLocked);
		}

		public void TestPlugInNotDisplayedMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();
			using (var plugIn = new ShipmentCargoReportPlugIn(shipment, true))
			{
				AssertEquals(plugIn.CoveringLabelText, plugIn.PlugInNotDisplayedMessage);
			}
		}

		public void TestConsolForCanada()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "USLGB";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "SEA";
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = "DEHAM";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "USLGB";

			var shipment = consol.Shipments.AddNew();

			using (var plugIn = new ShipmentCargoReportPlugIn(shipment))
			{
				plugIn.ChangeTheVisibility();
				Assert(!plugIn.Enabled);
				AssertNull(plugIn.ConsolForCanada);

				consol.JK_RL_NKDischargePort = "CATOR";
				plugIn.ChangeTheVisibility();
				Assert(plugIn.Enabled);
				AssertNotNull(plugIn.ConsolForCanada);
				AssertEquals(consol.PK, plugIn.ConsolForCanada.PK);

				consol.JK_RL_NKDischargePort = "";
				consol.JK_RL_NKFirstForeignPort = "CATOR";
				plugIn.ChangeTheVisibility();
				Assert(plugIn.Enabled);
				AssertNotNull(plugIn.ConsolForCanada);
				AssertEquals(consol.PK, plugIn.ConsolForCanada.PK);

				consol.JK_RL_NKFirstForeignPort = "";
				consol.JK_RL_NKLastForeignPort = "CATOR";
				plugIn.ChangeTheVisibility();
				Assert(plugIn.Enabled);
				AssertNotNull(plugIn.ConsolForCanada);
				AssertEquals(consol.PK, plugIn.ConsolForCanada.PK);
			}
		}

		public void TestCAConsolACIPlugInIsNotDisplayedDueToTransportMode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Constants.TransportModes.Road;
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKDischargePort = "CATOR";
			using (var plugIn = new ShipmentCargoReportPlugIn(shipment, true))
			{
				Assert(!plugIn.Enabled);

				consol1.JK_TransportMode = Constants.TransportModes.Sea;
				plugIn.ChangeTheVisibility();
				Assert(plugIn.Enabled);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			var shipment = consol.Shipments.AddNew();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_ActualVolume = 10;
			line.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			line.JL_ActualWeight = 10;
			line.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			line.JL_PackageCount = 10;
			var undg = line.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var plugIn = new ShipmentCargoReportPlugIn(shipment);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			plugIn.InitialiseCusSCAHouse();
			return plugIn;
		}

		BusinessObjectFactory newFactory;
		protected override void SetUp()
		{
			base.SetUp();
			newFactory = new BusinessObjectFactory();
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(newFactory, "8080");
		}

		protected override void TearDown()
		{
			ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(newFactory);
			base.TearDown();
		}

		sealed class ShipmentForm : ZTemplateForm
		{
			public ShipmentForm(ForwardingShipment shipment)
				: base(shipment)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.CAShipmentCargoReport);
			}
		}
	}
}
