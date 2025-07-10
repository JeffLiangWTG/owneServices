using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(CLMessagingConstants))]

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		#region Sea Mode

		[TestDate(2021, 03, 10, 12, 00, 00)]
		public void TestSendSeaManifest()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = "SEA";

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);

			var bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(1, menu.MenuItems.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (CLBillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.CHB);
		}

		[TestDate(2020, 10, 02, 18, 00, 00)]
		public void TestAskBeforeSendManifest()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			var bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(1, menu.MenuItems.Count);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();

					AssertMultilineASCIIEquals("Popup message", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

ARV: You have not entered an Estimated Time of Arrival at Border.
Manifest UQ: You have not entered a Manifest UQ.
Goods Location: A Warehouse is required
Voyage/Flight: You have not entered a Voyage.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (CLBillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCancelSeaManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(2, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "Reason";
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
		}

		public void TestCancelSeaManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(3, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectAll();
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "Reason";
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);
		}

		public void TestAmendSeaManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(2, menu.MenuItems.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (CLBillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						var chooser = dialog.BusinessEntity;
						chooser.Reason = "Reason";
						chooser.AmendType = "M";
						chooser.AmendReason = "01";
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Amend Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
		}

		public void TestAmendSeaManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(3, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectAll();
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "Reason";
					chooser.AmendType = "M";
					chooser.AmendReason = "01";
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);
		}

		public void TestGetMessageSendingNotification()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = "SEA";

			CreateAndPopulateHouseBill(true);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("To create a message for Chile you must be logged-in under a Chilean company.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(0, header.Bills[0].Messages.Count);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Chile;
			GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Your staff profile requires an email address as this is needed for messaging.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(0, header.Bills[0].Messages.Count);

			GlbStaff.CurrentUser.GS_EmailAddress = "test@gmail.com";

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (CLBillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
		}

		#endregion

		#region Air Mode

		[TestDate(2022, 10, 18, 12, 00, 00)]
		public void TestSendAirManifest()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = "AIR";

			CreateAndPopulateHouseBill(true);

			var bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(1, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("Send Manifest").PerformClick();
				AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertMessage(header.Bills[0], MessageTypes.Codes.CHE);
		}

		[TestDate(2022, 10, 18, 12, 00, 00)]
		public void TestSendAirManifestSecondPart()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = "AIR";

			CreateAndPopulateHouseBill(true);

			var bill = header.Bills[0];
			CreateAndPopulatePack(bill);

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(1, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("Send Manifest").PerformClick();
				AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.CHE);
		}

		public void TestCancelAirManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Bills[0].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(3, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "Reason";
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
		}

		public void TestCancelAirManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(3, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectAll();
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "Reason";
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);
		}

		public void TestAmendAirManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
					AssertEquals(3, menu.MenuItems.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (CLBillsSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						var chooser = dialog.BusinessEntity;
						chooser.Reason = "Reason";
						chooser.AmendType = "M";
						chooser.AmendReason = "01";
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Amend Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
		}

		public void TestAmendAirManifestTwoBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = new ZDate(2021, 02, 01);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateHouseBill(true);
			CreateAndPopulateMasterBill();

			header.Bills[0].ABL_BillStatus = "ACP";
			header.Bills[1].ABL_BillStatus = "ACP";

			var bill = header.Bills[0];

			CreateAndPopulatePack(bill);

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			using (var menu = new AsycudaMenuForTest(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(3, menu.MenuItems.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (CLBillsSelectionDialog)obj;
					dialog.SelectAll();
					var chooser = dialog.BusinessEntity;
					chooser.Reason = "Reason";
					chooser.AmendType = "M";
					chooser.AmendReason = "01";
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);
		}

		#endregion

		AsycudaManifestHeader header;

		protected override void SetUp()
		{
			base.SetUp();
			GlbStaff.CurrentUser.GS_EmailAddress = "test@gmail.com";
		}

		void AssertMessage(AsycudaBill bill, ZString messageType)
		{
			var message = bill.Messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.CLCustoms);
				AssertEquals(message.EM_ApplicationReference, header.Bills[0].ABL_BillNumber);
				AssertEquals(message.EM_LinkUniqueID, bill.PK);
				AssertEquals(message.EM_MessageType, messageType);
				AssertEquals(message.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				AssertEquals(message.EM_Status, EDIMessage.Status.Queued);
				AssertEquals(message.EM_IsTestMessage, CLCustomsDataRegistry.Instance.CLTestingSystem.Value);
			});
		}

		void CreateAndPopulateHouseBill(ZBool roro)
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = roro;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "1X20 DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400";
			pack.APA_LineNo = 1;
			pack.APA_MarksAndNumbers = "S/I";
			pack.APA_PackQty = 1;
			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.APA_Volume = 0.76;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicMetres;
			pack.APA_Weight = 170;
			pack.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			Factory.Save();
		}

		void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;

			bill.ABL_BillNumber = "04507816955";
			bill.ABL_RL_NKPortOfLoading = "USMIA";
			bill.ABL_RL_NKPortOfDischarge = "CLSPE";
		}
	}
}
