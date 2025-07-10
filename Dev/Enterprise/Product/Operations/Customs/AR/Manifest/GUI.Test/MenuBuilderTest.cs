using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		#region Sea Mode

		public void TestSendSeaManifest()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";

			header.Bills.AddNew();

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

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
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Original);
		}

		public void TestCancelBLManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			header.Bills.AddNew();

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Cancellation);
		}

		public void TestCancelBLManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Cancellation);
			AssertMessage(header.Bills[1], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Cancellation);
		}

		public void TestAmendSeaManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			header.Bills.AddNew();

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Change);
		}

		public void TestAmendSeaManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Change);
			AssertMessage(header.Bills[1], MessageTypes.Codes.ARA, MessageSubTypeCodes.Codes.Change);
		}

		public void TestGetExtraMessageSendingNotification()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;

			Factory.Save();

			using (var menu = new AsycudaMenuForTest(header))
			{
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("To create a message for Argentina you must be logged-in under a Argentinian company.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(0, header.Bills[0].Messages.Count);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;

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
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
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

		[TestDate(2023, 01, 01, 13, 00, 00)]
		public void TestSendAirManifest()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBillIssueDate = new ZDate(2023, 02, 02);
			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";

			header.Bills.AddNew();

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			Factory.Save();

			AssertEquals(0, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

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
						var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
						dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
						AssertEquals(1, dialog.GetSelectedItems().Length);
					});

					menu.MenuItems.FindByText("Send Manifest").PerformClick();
					AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Original);

			Assert("ID", header.Bills[0].Messages[0].EM_MessageText.Contains("<ram:ID>00000001</ram:ID>"));
			Assert("Arrival Event", header.Bills[0].Messages[0].EM_MessageText.Contains("<ram:ScheduledOccurrenceDateTime>2023-01-12T00:00:00</ram:ScheduledOccurrenceDateTime"));
			Assert("Departure Event", header.Bills[0].Messages[0].EM_MessageText.Contains("<ram:ScheduledOccurrenceDateTime>2023-01-11T00:00:00</ram:ScheduledOccurrenceDateTime>"));
			Assert("Signatory Carrier Authentication Actual Date Time", header.Bills[0].Messages[0].EM_MessageText.Contains("<ram:ActualDateTime>2023-02-02T00:00:00</ram:ActualDateTime>"));
			Assert("Issue Date Time", header.Bills[0].Messages[0].EM_MessageText.Contains("<ram:IssueDateTime>2023-01-01T13:00:00</ram:IssueDateTime>"));
		}

		public void TestCancelAWBManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBillIssueDate = new ZDate(2023, 01, 12);
			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			header.Bills.AddNew();

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Cancellation);
		}

		public void TestCancelAWBManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBillIssueDate = new ZDate(2023, 01, 12);
			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Cancel Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Cancellation);
			AssertMessage(header.Bills[1], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Cancellation);
		}

		public void TestAmendAirManifestOneBillSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBillIssueDate = new ZDate(2023, 01, 12);
			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			header.Bills.AddNew();

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == header.Bills[0].PK);
					AssertEquals(1, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Amend Manifest").PerformClick();
				AssertEquals("Message sent successfully  (H)QRHW20050055C" + "\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(0, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Change);
		}

		public void TestAmendAirManifestTwoBillsSelected()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_MasterBillIssueDate = new ZDate(2023, 01, 12);
			header.AMA_E_ARV = new ZDate(2023, 01, 12);
			header.AMA_E_DEP = new ZDate(2023, 01, 11);

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();
			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			bill = header.Bills.AddNew();

			Factory.Save();

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
					var dialog = (ASYCUDA.GUI.AsycudaItemSelectionDialog)obj;
					dialog.SelectAll();
					AssertEquals(2, dialog.GetSelectedItems().Length);
				});

				menu.MenuItems.FindByText("&Amend Manifest").PerformClick();
				AssertContains("Message sent successfully", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(1, header.Bills[0].Messages.Count);
			AssertEquals(1, header.Bills[1].Messages.Count);
			AssertEquals(0, header.Bills[2].Messages.Count);

			AssertMessage(header.Bills[0], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Change);
			AssertMessage(header.Bills[1], MessageTypes.Codes.ARD, MessageSubTypeCodes.Codes.Change);
		}

		#endregion

		void AssertMessage(AsycudaBill bill, ZString messageType, ZString messageSubType)
		{
			var message = bill.Messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(message.EM_ApplicationCode, ApplicationCodeList.Codes.ARCustoms);
				AssertEquals(message.EM_ApplicationReference, bill.ABL_BillNumber);
				AssertEquals(message.EM_IsTestMessage, ARCustomsDataRegistry.Instance.ARTestingSystem.Value);
				AssertEquals(message.EM_LinkUniqueID, bill.PK);
				AssertEquals(message.EM_LinkTable, AsycudaBillSchema.Constants.TableName);
				AssertEquals(message.EM_MessageSubType, ZString.Empty);
				AssertEquals(message.EM_MessageType, messageType);
				AssertEquals(message.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				AssertEquals(message.EM_Status, EDIMessage.Status.Queued);
			});
		}
	}
}
