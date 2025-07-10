using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			var header = NewManifestHeaderWithDefault();
			using (var form = new ZForm(header))
			{
				AssertEquals("EU ICS2 Manifest", new MenuBuilder(header, form).MenuCaption.EnglishText);
			}
		}

		public void TestBuildMenu_AmendMenuItem_DisabledWhenF25IsSelected()
		{
			var header = NewManifestHeaderWithDefault();
			header.RegistrationDate = ZDateTime.Today;
			header.RegistrationNumber = "Test";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F25;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Amend Manifest menu item should be disabled when F25 is selected", false, menu.MenuItems.FindByText("&Amend Manifest").Enabled);
			}
		}

		[RequiresSTA]
		public void TestBuildMenu_SendAndAmendManifestVisibility()
		{
			var header = NewManifestHeaderWithDefault();
			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNotNull("Send Manifest", menu.MenuItems.FindByText("Send Manifest"));
				AssertNull("No Amend Manifest menu item", menu.MenuItems.FindByText("Amend Manifest"));
			}

			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;

			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F10);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F13);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F14);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F15);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F16);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F17);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F22);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F23);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F24);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F25);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F26);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F40);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F41);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F43);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F44);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F50);
			AssertAmendManifestVisibility(EUICS2SpecificCircumstanceList.Codes.F51);

			void AssertAmendManifestVisibility(string specificCircumstanceIndicator)
			{
				header.SpecificCircumstanceIndicator = specificCircumstanceIndicator;

				using (var form = new ZForm(header))
				using (var menu = new AsycudaMenuForTest(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertNull($"{specificCircumstanceIndicator} - No Send Manifest menu item", menu.MenuItems.FindByText("Send Manifest"));
					AssertNotNull($"{specificCircumstanceIndicator} - Amend Manifest", menu.MenuItems.FindByText("Amend Manifest"));
				}
			}
		}

		[RequiresSTA]
		public void TestBuildMenu_AmendManifestEnabled()
		{
			var header = NewManifestHeaderWithDefault();
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;

			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;

			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F10);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F13);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F14);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F15);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F16);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F17);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F22);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F23);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F24);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F26);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F40);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F41);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F50);
			AssertAmendManifestEnabled(EUICS2SpecificCircumstanceList.Codes.F51);

			header.RegistrationStatus = EUICS2CustomsStatusList.Codes.CAN;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var amendManifestMenuItem = menu.MenuItems.FindByText("Amend Manifest");
				AssertEquals(false, amendManifestMenuItem.Enabled);
			}

			void AssertAmendManifestEnabled(string specificCircumstanceIndicator)
			{
				header.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				using (var form = new ZForm(header))
				using (var menu = new AsycudaMenuForTest(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					var amendManifestMenuItem = menu.MenuItems.FindByText("Amend Manifest");
					Assert($"{specificCircumstanceIndicator} - Amend Manifest enabled", amendManifestMenuItem.Enabled);
				}
			}
		}

		[RequiresSTA]
		public void TestBuildMenu_SendManifestEnabled()
		{
			var specificCircumstanceIndicatorsToTest = new List<KeyValuePair<ZString, ZString>>()
			{
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F10, MessageTypes.Codes.F10),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F13, MessageTypes.Codes.F13),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F14, MessageTypes.Codes.F14),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F15, MessageTypes.Codes.F15),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F16, MessageTypes.Codes.F16),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F17, MessageTypes.Codes.F17),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F22, MessageTypes.Codes.F22),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F23, MessageTypes.Codes.F23),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F24, MessageTypes.Codes.F24),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F25, MessageTypes.Codes.F25),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F26, MessageTypes.Codes.F26),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F40, MessageTypes.Codes.F40),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F41, MessageTypes.Codes.F41),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F43, MessageTypes.Codes.F43),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F44, MessageTypes.Codes.F44),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F51, MessageTypes.Codes.F51),
			};

			foreach (var circumstance in specificCircumstanceIndicatorsToTest)
			{
				AssertSendManifestEnabled(circumstance.Key);
			}
		}

		void AssertSendManifestEnabled(ZString specificCircumstanceIndicator)
		{
			var header = NewManifestHeaderWithDefault();
			header.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendManifestMenuItem = menu.MenuItems.FindByText("Send Manifest");
				Assert($"{specificCircumstanceIndicator} - Send Manifest enabled", sendManifestMenuItem.Enabled);
			}
		}

		public void TestBuildMenu_SendAndAmendManifestWithSelectItems()
		{
			var header = NewManifestHeaderWithDefault();

			var requestHeader1 = header.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var requestHeader2 = header.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "B01";
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;

			AssertEquals("Should show EUICS2AmendedItemsSelectionDialog for amending.", true, PerformMenuClickExpectingItemsSelection(header, "Amend Manifest"));
			AssertContains("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestBuildMenu_HRCMScreeningResponseWithSelectItems()
		{
			var menuText = "HRCM Screening Response";

			var header = NewManifestHeaderWithDefault();
			var message = Factory.New<ICS2InboundEDIMessage>();
			message.EM_MessageType = MessageTypes.Codes.Q03;
			header.Messages.Add(message);
			var requestHeader1 = header.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;

			AssertEquals("Referral popup not shown without HRCM referral", false, PerformMenuClickExpectingItemsSelection(header, menuText));

			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;
			AssertEquals("Referral popup shown with HRCM referral", true, PerformMenuClickExpectingItemsSelection(header, menuText));

			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;
			AssertEquals("Referral popup not shown without HRCM request", false, PerformMenuClickExpectingItemsSelection(header, menuText));
		}

		public void TestBuildMenu_AdditionalInformationReplyWithSelectItems()
		{
			var menuText = "Additional Information Reply";

			var header = NewManifestHeaderWithDefault();
			var message = Factory.New<ICS2InboundEDIMessage>();
			message.EM_MessageType = MessageTypes.Codes.Q02;
			header.Messages.Add(message);
			var requestHeader1 = header.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;

			AssertEquals("Referral popup not shown without information referral", false, PerformMenuClickExpectingItemsSelection(header, menuText));

			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;
			AssertEquals("Referral popup shown with information referral", true, PerformMenuClickExpectingItemsSelection(header, menuText));

			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;
			AssertEquals("Referral popup shown with amendment", true, PerformMenuClickExpectingItemsSelection(header, menuText));
		}

		bool PerformMenuClickExpectingItemsSelection(AsycudaManifestHeader header, string menuText)
		{
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;
			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var isItemSelectionDialogShown = false;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					isItemSelectionDialogShown = isItemSelectionDialogShown || obj is EUICS2AmendedItemsSelectionDialog;
				});

				var amendManifestMenuItem = menu.MenuItems.FindByText(menuText);
				amendManifestMenuItem.PerformClick();
				return isItemSelectionDialogShown;
			}
		}

		public void TestBuildMenu_CancelManifest()
		{
			var header = NewManifestHeaderWithDefault();
			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNull("Precondition : No Cancel Manifest menu item", menu.MenuItems.FindByText("Cancel Manifest"));
			}

			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNotNull("Cancel Manifest", menu.MenuItems.FindByText("Cancel Manifest"));
			}

			header.RegistrationStatus = EUICS2CustomsStatusList.Codes.CAN;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				var canceledManifestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertEquals(false, canceledManifestMenuItem.Enabled);
			}
		}

		public void TestBuildMenu_HRCMScreeningResponse()
		{
			var header = NewManifestHeaderWithDefault();
			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNull("Precondition : No HRCM Screening Response menu item", menu.MenuItems.FindByText("HRCM Screening Response"));
			}

			header.RegistrationNumber = "RegistrationNumber";
			var q03Message = Factory.NewWithValidTestData<EDIMessage>();
			q03Message.EM_MessageType = MessageTypes.Codes.Q03;
			header.Messages.Add(q03Message);

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNotNull("HRCM Screening Response", menu.MenuItems.FindByText("HRCM Screening Response"));
			}
		}

		public void TestBuildMenu_ArrivalNotification_ForwarderManifest()
		{
			var header = NewManifestHeaderWithDefault();

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNull(menu.MenuItems.FindByText("Arrival Notification"));
			}
		}

		[RequiresSTA]
		public void TestBuildMenu_ArrivalNotification_CarrierManifest()
		{
			var header = NewManifestHeaderWithDefault();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNotNull(menu.MenuItems.FindByText("Arrival Notification"));
			}
		}

		public void TestBuildMenu_AdditionalInformationReply_WhenHasRegistration()
		{
			var header = NewManifestHeaderWithDefault();

			header.RegistrationNumber = "RegistrationNumber";

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNull("No Additional Information Reply menu item - Q02 message is requred", menu.MenuItems.FindByText("Additional Information Reply"));
			}
		}

		public void TestBuildMenu_AdditionalInformationReply_WhenHasQ02Message()
		{
			var header = NewManifestHeaderWithDefault();

			var q02Message = Factory.NewWithValidTestData<EDIMessage>();
			q02Message.EM_MessageType = MessageTypes.Codes.Q02;
			header.Messages.Add(q02Message);

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNull("No Additional Information Reply menu item - Registration is required", menu.MenuItems.FindByText("Additional Information Reply"));
			}
		}

		public void TestBuildMenu_AdditionalInformationReply_WhenHasRegistrationAndQ02Message()
		{
			var header = NewManifestHeaderWithDefault();

			header.RegistrationNumber = "RegistrationNumber";
			var q02Message = Factory.NewWithValidTestData<EDIMessage>();
			q02Message.EM_MessageType = MessageTypes.Codes.Q02;
			header.Messages.Add(q02Message);

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertNotNull("Additional Information Reply", menu.MenuItems.FindByText("Additional Information Reply"));
			}
		}

		public void TestClickMenu_SendManifest()
		{
			var specificCircumstanceIndicatorsToTest = new List<KeyValuePair<ZString, ZString>>()
			{
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F10, MessageTypes.Codes.F10),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F13, MessageTypes.Codes.F13),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F14, MessageTypes.Codes.F14),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F16, MessageTypes.Codes.F16),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F17, MessageTypes.Codes.F17),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F22, MessageTypes.Codes.F22),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F23, MessageTypes.Codes.F23),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F24, MessageTypes.Codes.F24),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F25, MessageTypes.Codes.F25),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F26, MessageTypes.Codes.F26),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F40, MessageTypes.Codes.F40),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F41, MessageTypes.Codes.F41),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F43, MessageTypes.Codes.F43),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F44, MessageTypes.Codes.F44),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F50, MessageTypes.Codes.F50),
				new KeyValuePair<ZString, ZString>(EUICS2SpecificCircumstanceList.Codes.F51, MessageTypes.Codes.F51),
			};

			foreach (var testData in specificCircumstanceIndicatorsToTest)
			{
				AssertSendManifest(testData.Key, testData.Value);
			}
		}

		public void TestClickMenu_AmendManifest()
		{
			var specificCircumstanceIndicatorsToTest = new List<KeyValuePair<ZString, ZString>>
			{
				new (EUICS2SpecificCircumstanceList.Codes.F10, MessageTypes.Codes.A10),
				new (EUICS2SpecificCircumstanceList.Codes.F13, MessageTypes.Codes.A13),
				new (EUICS2SpecificCircumstanceList.Codes.F14, MessageTypes.Codes.A14),
				new (EUICS2SpecificCircumstanceList.Codes.F15, MessageTypes.Codes.A15),
				new (EUICS2SpecificCircumstanceList.Codes.F16, MessageTypes.Codes.A16),
				new (EUICS2SpecificCircumstanceList.Codes.F17, MessageTypes.Codes.A17),
				new (EUICS2SpecificCircumstanceList.Codes.F23, MessageTypes.Codes.A23),
				new (EUICS2SpecificCircumstanceList.Codes.F24, MessageTypes.Codes.A24),
				new (EUICS2SpecificCircumstanceList.Codes.F40, MessageTypes.Codes.A40),
				new (EUICS2SpecificCircumstanceList.Codes.F41, MessageTypes.Codes.A41),
				new (EUICS2SpecificCircumstanceList.Codes.F50, MessageTypes.Codes.A50),
				new (EUICS2SpecificCircumstanceList.Codes.F51, MessageTypes.Codes.A51),
			};

			foreach (var testData in specificCircumstanceIndicatorsToTest)
			{
				AssertAmendManifest(testData.Key, testData.Value);
			}
		}

		void AssertSendManifest(ZString specificCircumstanceIndicator, ZString messageType)
		{
			var header = NewManifestHeaderWithDefault();
			header.SpecificCircumstanceIndicator = specificCircumstanceIndicator;

			AssertSendOrAmendClick("Send Manifest", header, () => AssertMessageCreatedAndSaved(header, messageType));
		}

		void AssertAmendManifest(ZString specificCircumstanceIndicator, ZString messageType)
		{
			var header = NewManifestHeaderWithDefault();
			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;
			header.SpecificCircumstanceIndicator = specificCircumstanceIndicator;

			AssertSendOrAmendClick("Amend Manifest", header, () => AssertMessageCreatedAndSaved(header, messageType));
		}

		void AssertSendOrAmendClick(string menuToClick, AsycudaManifestHeader header, Action messageCreatedAndSavedAssertion)
		{
			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var amendManifestMenuItem = menu.MenuItems.FindByText(menuToClick);
				AssertNotNull(amendManifestMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

				amendManifestMenuItem.PerformClick();

				AssertEquals("message status", MessageStatusCodeList.Codes.Awaiting, header.MessageStatus);
				messageCreatedAndSavedAssertion();
			}
		}

		public void TestClickMenu_ArrivalNotification()
		{
			var header = NewManifestHeaderWithDefault();
			header.AMA_A_ARV = ZDateTime.Now;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var amendManifestMenuItem = menu.MenuItems.FindByText("Arrival Notification");
				AssertNotNull(amendManifestMenuItem);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

				amendManifestMenuItem.PerformClick();

				AssertMessageCreatedAndSaved(header, MessageTypes.Codes.N06);
			}
		}

		public void TestClickMenu_CancelManifest_SendMessage()
		{
			var header = NewManifestHeaderWithDefault();
			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendInvalidationRequestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertNotNull(sendInvalidationRequestMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendInvalidationRequestMenuItem.PerformClick();
				AssertCollectionContains("Cancelling confirmation pop up", "Are you sure you want to cancel this Manifest?", UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text));

				AssertMessageCreatedAndSaved(header, MessageTypes.Codes.Q04);
			}
		}

		public void TestClickMenu_CancelManifest_Cancel()
		{
			var header = NewManifestHeaderWithDefault();
			header.RegistrationNumber = "RegistrationNumber";
			header.RegistrationDate = ZDateTime.Today;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendInvalidationRequestMenuItem = menu.MenuItems.FindByText("Cancel Manifest");
				AssertNotNull(sendInvalidationRequestMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				sendInvalidationRequestMenuItem.PerformClick();

				var actualConfirmationText = UnitTestUserNotification.Instance.LastMessage.Text;
				const string expectedConfirmationText = "Are you sure you want to cancel this Manifest?";
				AssertEquals("Cancelling confirmation pop up", expectedConfirmationText, actualConfirmationText);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("No message has been sent", 0, header.Messages.Count);
			}
		}

		public void TestClickMenu_HRCMScreeningResponse()
		{
			var header = NewManifestHeaderWithDefault();
			header.RegistrationNumber = "RegistrationNumber";

			var q03Message = Factory.NewWithValidTestData<EDIMessage>();
			q03Message.EM_MessageType = MessageTypes.Codes.Q03;
			header.Messages.Add(q03Message);

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var hrcmScreeningRequestMenuItem = menu.MenuItems.FindByText("HRCM Screening Response");
				AssertNotNull(hrcmScreeningRequestMenuItem);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors

				hrcmScreeningRequestMenuItem.PerformClick();

				AssertMessageCreatedAndSaved(header, MessageTypes.Codes.R03);
			}
		}

		public void TestBuildMenu_StatusRequestMenuItem()
		{
			var header = NewManifestHeaderWithDefault();

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var statusRequestMenuItem = menu.MenuItems.FindByText("Status Request");
				AssertNull(statusRequestMenuItem);
			}

			header.RegistrationNumber = "RegNo";
			header.RegistrationDate = ZDateTime.Today;

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var statusRequestMenuItem = menu.MenuItems.FindByText("Status Request");
				AssertNotNull(statusRequestMenuItem);
				statusRequestMenuItem.PerformClick();

				AssertMessageCreatedAndSaved(header, MessageTypes.Codes.Q05);
			}
		}

		void AssertMessageCreatedAndSaved(AsycudaManifestHeader header, ZString expectedMessageType)
		{
			var message = (EDIMessage)header.Messages.Single(m => ((EDIMessage)m).EM_MessageType == expectedMessageType);
			AssertNotNull("Message created", message);
			Assert("Message has been saved", message.IsInDatabase);
		}

		AsycudaManifestHeader NewManifestHeaderWithDefault()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = "IC2MS";
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "DE", "Germany", startDate, endDate);
			Factory.Save();
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.EuropeanUnion;
			manifestHeader.AddressedMemberState = Core.Constants.CountryCodes.Germany;
			manifestHeader.AMA_ManifestType = "ENS";
			Factory.Save();

			return manifestHeader;
		}
	}
}
