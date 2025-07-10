using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSMenuTest : TestCaseWithFactory
	{
		public void TestVisibility_ForConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var plugInSupport = new RNSPlugInSupportConsolWrapper(consol);
			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Send House Bill Release Status Query(s)");
				AssertNotNull("Menu 'Send House Bill Release Status Query(s)' should be added", menu);

				menu = FindMenuItem(rnsMenu.MenuItems, "House Bill Arrival Certification Message(s)");
				AssertNotNull("Menu 'House Bill Arrival Certification Message(s)' should be added", menu);

				menu = FindMenuItem(rnsMenu.MenuItems, "Send Release Status Query");
				AssertNotNull("Menu 'Send Release Status Query' should be added", menu);

				menu = FindMenuItem(rnsMenu.MenuItems, "Arrival Certification Message");
				AssertNull("Menu 'Arrival Certification Message' should not be added", menu);

				menu = FindMenuItem(rnsMenu.MenuItems, "Enter Manual Release");
				AssertNull("Menu 'Enter Manual Release' should not be added", menu);
			}
		}

		public void TestQueryHouseBilReleaseStatus_Click()
		{
			var ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			var plugInSupport = new RNSPlugInSupportConsolWrapper(consol);
			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Send House Bill Release Status Query(s)");
				AssertNotNull("Menu 'Send House Bill Release Status Query(s)' should be added", menu);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object dialog) =>
				{
					if (dialog is MessagesChooserDialog)
					{
						((MessagesChooserDialog)dialog).SendPressed = true;
						var chooser = (MessageChooserNonPersistent)((MessagesChooserDialog)dialog).DataSource;
						chooser.SelectAll();
					}
				});
				menu.PerformClick();
				Assert("RNSRequestForm should not be showed", ZFormModaliser.LastFormShownDialogForTest is MessagesChooserDialog);
				AssertEquals("shipment1 should have created 1 SingleMessageManager", 1, shipment1.Messages.Count);
				AssertEquals("shipment2 should have created 1 SingleMessageManager", 1, shipment2.Messages.Count);
				AssertEquals("Should have sent 2 messages", ediMessageCount + 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		public void TestHouseBillArrivalCertification_Click()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0495", "0495", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var subLocation1 = CACSubLocationTest.CreateSubLocation(Factory, "1212", port: "0495");
			var subLocation2 = CACSubLocationTest.CreateSubLocation(Factory, "3252", port: "0495");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var entryNumber1 = shipment1.Numbers.AddNew();
			entryNumber1.CE_EntryNum = "111";
			entryNumber1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			var shipment2 = consol.Shipments.AddNew();
			var entryNumber2 = shipment2.Numbers.AddNew();
			entryNumber2.CE_EntryNum = "222";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			Factory.Save();
			var plugInSupport = new RNSPlugInSupportConsolWrapper(consol);
			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "House Bill Arrival Certification Message(s)");
				AssertNotNull("Menu 'House Bill Arrival Certification Message(s)' should be added", menu);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object dialog) =>
				{
					if (dialog is HouseBillArrivalCertificationSelectionDialog houseBillArrivalCertificationSelectionDialog)
					{
						var rnsRequestBos = (RNSRequestBOCollection)(houseBillArrivalCertificationSelectionDialog).DataSource;
						rnsRequestBos[0].Selected = true;
						rnsRequestBos[1].Selected = true;
					}
				});
				menu.PerformClick();
				Assert("A HouseBillArrivalCertificationSelectionDialog should be showed", ZFormModaliser.LastFormShownDialogForTest is HouseBillArrivalCertificationSelectionDialog);
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertContains("Do you wish to continue despite the following?", lastMessage.Text);
				AssertContains("RNS Status Query for Shipment S00001000", lastMessage.Text);
				AssertContains("RNS Status Query for Shipment S00001001", lastMessage.Text);
				AssertContains("Message Error - OfficeCode: You have not entered an Office Code.", lastMessage.Text);
				AssertContains("Message Error - SubLocationCode: You have not entered a value.", lastMessage.Text);
				AssertEquals("shipment1 should have created 0 SingleMessageManager", 0, shipment1.Messages.Count);
				AssertEquals("shipment2 should have created 0 SingleMessageManager", 0, shipment2.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((object dialog) =>
				{
					if (dialog is HouseBillArrivalCertificationSelectionDialog houseBillArrivalCertificationSelectionDialog)
					{
						var rnsRequestBos = (RNSRequestBOCollection)(houseBillArrivalCertificationSelectionDialog).DataSource;
						var requestBO1 = rnsRequestBos[0];
						requestBO1.Selected = true;
						var requestBO2 = rnsRequestBos[1];
						requestBO2.Selected = true;
						requestBO1.OfficeCode = "0495";
						requestBO1.SubLocationCode = "1212";
						requestBO2.OfficeCode = "0495";
						requestBO2.SubLocationCode = "3252";
					}
				});
				UnitTestUserNotification.Instance.AddYesAnswer();
				menu.PerformClick();
				AssertEquals("2 original messages have been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("shipment1 should have created 1 SingleMessageManager", 1, shipment1.Messages.Count);
				AssertEquals("shipment2 should have created 1 SingleMessageManager", 1, shipment2.Messages.Count);
			}
		}

		public void TestArrivalMessageMenu()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			IRNSPlugInSupport plugInSupport = new RNSPlugInSupportShipmentWrapper(shipment1);
			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Arrival Certification Message");
				AssertNotNull("Menu 'Arrival Certification Message' should be added", menu);

				Assert("Precondition:", plugInSupport.CargoControlNumber.IsEmpty);
				menu.PerformClick();

				AssertEquals("The CCN is required for sending an arrival certification message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipment2 = Factory.New<CFSShipment>();
			plugInSupport = new RNSPlugInSupportShipmentWrapper(shipment2);
			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Arrival Certification Message");
				AssertNotNull("Menu 'Arrival Certification Message' should be added", menu);

				Assert("Precondition:", plugInSupport.CargoControlNumber.IsEmpty);
				menu.PerformClick();

				AssertEquals("The CCN is required for sending an arrival certification message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipment3 = Factory.New<CFSShipment>();

			var num1 = shipment3.Numbers.AddNew();
			num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			num1.CE_EntryNum = "1";

			Factory.Save();

			IRNSPlugInSupport plugInSupport3 = new RNSPlugInSupportShipmentWrapper(shipment3);

			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport3)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Arrival Certification Message");
				AssertNotNull("Menu 'Arrival Certification Message' should be added", menu);

				Assert("Precondition:", !plugInSupport3.CargoControlNumber.IsEmpty);
				menu.PerformClick();

				Assert("A RNSRequestForm should be showed", ZFormModaliser.LastFormShownDialogForTest is RNSRequestForm);
			}
		}

		public void TestMenualReleaseMenu()
		{
			var shipment = Factory.New<CFSShipment>();
			IRNSPlugInSupport plugInSupport = new RNSPlugInSupportShipmentWrapper(shipment);

			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Enter Manual Release");
				AssertNotNull("Menu 'Enter Manual Release' should be added", menu);
				menu.PerformClick();
				Assert("A ManualReleaseForm should be showed", ZFormModaliser.LastFormShownDialogForTest is ManualReleaseForm);
			}

			var shipment1 = Factory.New<ForwardingShipment>();
			plugInSupport = new RNSPlugInSupportShipmentWrapper(shipment);

			using (var rnsMenu = new RNSMenu(new RNSMessagingBO(plugInSupport)))
			{
				var menu = FindMenuItem(rnsMenu.MenuItems, "Enter Manual Release");
				AssertNotNull("Menu 'Enter Manual Release' should be added", menu);
				menu.PerformClick();
				Assert("A ManualReleaseForm should be showed", ZFormModaliser.LastFormShownDialogForTest is ManualReleaseForm);
			}
		}

		ZMenuItem FindMenuItem(Menu.MenuItemCollection menuItems, string caption)
		{
			foreach (ZMenuItem menu in menuItems)
			{
				if (menu.Caption == caption)
				{
					return menu;
				}
			}

			return null;
		}
	}
}
