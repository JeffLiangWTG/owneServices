using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq.Protected;
using NUnit.Framework;
using EUInterfaces = Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TemporaryStorageMessagesMenu))]
public class TemporaryStorageMessagesMenuTest : TestCaseWithFactory
{
	public void TestMenuItems()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
		using (var form = new TemporaryStorageForm(tempStorage))
		{
			var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
			AssertNotNull(sendMessageMenuItem);
			sendMessageMenuItem.PerformClick();
			AssertType<MessageSendingFormWithValidationDetails>(ZFormModaliser.LastFormShownDialogForTest);
		}

		tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.Latvia;
		using (var form = new TemporaryStorageForm(tempStorage))
		{
			var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
			AssertNotNull(sendMessageMenuItem);
			sendMessageMenuItem.PerformClick();
			AssertEquals("PNTS messages are not supported in your country.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestCreateMenuItems()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";

		using (var form = new TemporaryStorageForm(tempStorage))
		{
			var messagesMenu = form.Menu.MenuItems.FindByText("Messages", true);

			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
				"-",
				"TS Register Management",
			}, messagesMenu.MenuItems.ToList<ZMenuItem>().Select(x => x.Text));

			var tsRegisterManagementMenuItem = messagesMenu.MenuItems[3];
			AssertEquals("TS Register Management", tsRegisterManagementMenuItem.Text);
			AssertEquals("TS Register Management sub menu item", "Select Inventory", tsRegisterManagementMenuItem.MenuItems[0].Text);
		}
	}

	#region RefreshMenu

	public void TestRefreshMenu_MessageStatusSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageStatus = "SNT";
		using (var form = new TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"Set Entry as Failed From Transmission",
				"&Send To Customs",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	public void TestRefreshMenu_MessageStatusNotSNT()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		using (var form = new TemporaryStorageForm(tempStorage))
		{
			AssertContainsExactElementsInExactOrder(new[]
			{
				"&Send To Customs",
			}, form.Menu.MenuItems.FindByText("Messages", true).MenuItems.ToList<ZMenuItem>().Where(x => x.Visible).Select(x => x.Text));
		}
	}

	#endregion

	[RequiresSTA]
	public void TestSetAsFailedFromTransmission()
	{
		var confirmationMessageText = "Are you sure you want to set this Entry as Failed from Transmission?";

		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_JobReference = "JobReference";
		tempStorage.AMA_MessageStatus = "AAA";
		tempStorage.Factory.Save();

		ZFormModaliser.ShowDialogsInTest = false;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		using (var form = new TemporaryStorageForm(tempStorage))
		{
			form.Show();

			var unitTestUserNotificationInstance = UnitTestUserNotification.Instance;
			var setEntryAsFailedMenuItem = (ZMenuItem)form.FindMenuItem_ForTest("Set Entry as Failed From Transmission");

			CombineAssertions(() =>
			{
				setEntryAsFailedMenuItem.PerformClick();
				AssertEquals("Should not have message asking to confirm the action when report selected has message status not SNT", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));
				AssertEquals("Temporary Storage Header has the same message status as before because it was not SNT", "AAA", tempStorage.AMA_MessageStatus);

				tempStorage.AMA_MessageStatus = "SNT";
				Factory.Save();
				unitTestUserNotificationInstance.ClearMessagesAndAnswers();
				unitTestUserNotificationInstance.AddAnswer(ZDialogResult.Cancel);
				setEntryAsFailedMenuItem.PerformClick();
				AssertEquals("Should have message asking to confirm the action (when cancel)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));
				AssertEquals("Temporary Storage Header has the same message status as before", "SNT", tempStorage.AMA_MessageStatus);

				unitTestUserNotificationInstance.ClearMessagesAndAnswers();
				unitTestUserNotificationInstance.AddOKAnswer();
				setEntryAsFailedMenuItem.PerformClick();
				AssertEquals("Should have message asking to confirm the action (when ok)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));
				AssertEquals("Temporary Storage Header has message status SNT and the confirmation was accepted so the message status is changed", "Entry was set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Temporary Storage Header has new message status", "FAL", tempStorage.AMA_MessageStatus);

				var mainFactoryChangeSet = Factory.GetChanges();
				var mainFactoryHasChanges = mainFactoryChangeSet.GetChangedObjects().Any() || mainFactoryChangeSet.GetAddedObjects().Any();
				AssertEquals("After pressing OK in the pop up the change will not be saved. Does Main Factory have changes?", true, mainFactoryHasChanges);
			});
		}
	}

	public void TestMessagesFromDatabaseAreDifferentFromMessagesInMemory()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
		var messagingProvider = tempStorage.MessagingProvider;
		var sendingObject = new TemporaryStorageMessageSendingObject(tempStorage);
		sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
		messagingProvider.SendMessage(sendingObject, new SendsMessagesToCustomsGUI(), Globals.Message);
		Factory.Save();

		var message = Factory.New<EDIMessage>();
		message.EM_MessageNum = "1234";
		message.EM_MessageSubType = TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
		message.EM_LinkTable = tempStorage.TableName;
		message.EM_LinkUniqueID = tempStorage.PK;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_MessageText = "ABC";

		using (var form = new TemporaryStorageForm(tempStorage))
		{
			var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			sendMessageMenuItem.PerformClick();
			AssertEquals("A new message has been attached to this temporary storage, please reopen the form before sending a message.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestLastOutgoingMessageIsWaitingForResponse()
	{
		var tempStorage = Factory.New<TemporaryStorageHeader>();
		tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
		var messagingProvider = tempStorage.MessagingProvider;
		var sendingObject = new TemporaryStorageMessageSendingObject(tempStorage);
		sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
		messagingProvider.SendMessage(sendingObject, new SendsMessagesToCustomsGUI(), Globals.Message);

		using (var form = new TemporaryStorageForm(tempStorage))
		{
			var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			sendMessageMenuItem.PerformClick();
			AssertEquals("The last outgoing 015 message is waiting for response, you can only resend another 015 message, do you want to resend this message?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("The resend message is of the same type as the last outgoing message", TemporaryStorageMessageTypeList.Codes.PreLodgedTSD, tempStorage.Messages[1].EM_MessageSubType);
		}
	}

	public void TestTSRegisterManagementMenuVisibility() => CombineAssertions(() =>
	{
		var orgAddress = GetOrgAddress();
		CreateCusTempStorageRegPremises(CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, "Managed ADT Location", "P01");
		Factory.Save();

		var headerMock = Factory.NewMoq<TemporaryStorageHeader>();
		var header = headerMock.Object;
		header.GoodsLocation.Address.AuthorisationNumber = "Managed ADT Location";

		var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabled;
		using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			AssertTSRegisterManagementMenuVisibility(true);
			AssertTSRegisterManagementMenuVisibility(false);
		}

		void AssertTSRegisterManagementMenuVisibility(bool isTSRegisterManagementApplicable, [CallerLineNumber] int line = 0)
		{
			headerMock.Protected().Setup<bool>("IsMessageTypeMatchingToTSRegisterManagementSelectInventoryCore").Returns(isTSRegisterManagementApplicable);

			using (var form = new TemporaryStorageForm(header))
			{
				form.Show();

				var tsRegisterManagementMenuItem = form.Menu.MenuItems.FindByText("TS Register Management", true);
				AssertEquals($"[{line}]: IsTSRegisterManagementApplicable = '{isTSRegisterManagementApplicable}'.", isTSRegisterManagementApplicable, tsRegisterManagementMenuItem.Visible);
			}
		}

		void CreateCusTempStorageRegPremises(string type, string customsLocation, string code)
		{
			var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises.SRP_Type = type;
			premises.SRP_CustomsLocation = customsLocation;
			premises.SRP_Code = code;
			premises.SRP_Description = $"{code} - Description";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
		}

		OrgAddress GetOrgAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TST";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.Address1 = "Test Address";
			return orgAddress;
		}
	});

	public void TestSelectInventoryMenuItem_ClickShowsError()
	{
		using var form = new TemporaryStorageForm(Factory.New<TemporaryStorageHeader>());
		form.Show();

		var selectInventoryMenuItem = form.Menu.MenuItems.FindByText("Select Inventory", true);
		selectInventoryMenuItem.PerformClick();
		AssertEquals("Is going to be implemented in 'Coding - Reqs 3 & 4' WF from 'WI00749380 - EU - TS - Trigger selection of Goods from G5 V1 Expedition'.", UnitTestUserNotification.Instance.LastMessage.Text);
	}
}
