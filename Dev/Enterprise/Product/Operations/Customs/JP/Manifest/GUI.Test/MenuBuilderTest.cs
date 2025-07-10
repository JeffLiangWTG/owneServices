using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(MenuBuilder))]
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			using (var form = new ZForm(header))
			{
				AssertEquals("JP Manifest", new MenuBuilder(header, form).MenuCaption.EnglishText);
			}
		}

		public void TestSendOrExportNACCSMessageMenuItems_Visibility()
		{
			using var form = new ZForm(header);
			var menu = new AsycudaMenuForTest(header);
			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var sendOrExportNACCSMessageMenuItem = menu.MenuItems.FindByText("Send/Export NACCS Message");
			var hch01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");
			var chaMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("CHA – Correction of HAWB information");
			var hdf01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HDF01 - Consolidation Registration");
			var nvc01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("NVC01 - House B/L Cargo Registration (Registration/Amendment/Cancellation)");
			var nvc01BondedLocationAmendmentMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("NVC01 – House B/L Cargo Registration (Bonded Location Amendment)");
			var hdeMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HDE - Completion Registration");

			CombineAssertions(() =>
			{
				AssertEquals(JPManifestTypeCodeList.Codes.HCH, header.AMA_ManifestType);

				Assert(hch01MenuItem.Visible);
				Assert(chaMenuItem.Visible);
				Assert(!hdf01MenuItem.Visible);
				Assert(!hdeMenuItem.Visible);
				Assert(!nvc01MenuItem.Visible);
				Assert(!nvc01BondedLocationAmendmentMenuItem.Visible);

				header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
				menu.OnPopup(EventArgs.Empty);
				sendOrExportNACCSMessageMenuItem = menu.MenuItems.FindByText("Send/Export NACCS Message");
				hch01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");
				chaMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("CHA – Correction of HAWB information");
				hdf01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HDF01 - Consolidation Registration");
				hdeMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HDE - Completion Registration");
				nvc01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("NVC01 - House B/L Cargo Registration (Registration/Amendment/Cancellation)");
				nvc01BondedLocationAmendmentMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("NVC01 – House B/L Cargo Registration (Bonded Location Amendment)");
				Assert(!chaMenuItem.Visible);
				Assert(!hch01MenuItem.Visible);
				Assert(!hdf01MenuItem.Visible);
				Assert(!hdeMenuItem.Visible);
				Assert(nvc01MenuItem.Visible);
				Assert(nvc01BondedLocationAmendmentMenuItem.Visible);

				header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
				menu.OnPopup(EventArgs.Empty);
				sendOrExportNACCSMessageMenuItem = menu.MenuItems.FindByText("Send/Export NACCS Message");
				hch01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");
				chaMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("CHA – Correction of HAWB information");
				hdf01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HDF01 - Consolidation Registration");
				hdeMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("HDE - Completion Registration");
				nvc01MenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("NVC01 - House B/L Cargo Registration (Registration/Amendment/Cancellation)");
				nvc01BondedLocationAmendmentMenuItem = sendOrExportNACCSMessageMenuItem.MenuItems.FindByText("NVC01 – House B/L Cargo Registration (Bonded Location Amendment)");
				Assert(!chaMenuItem.Visible);
				Assert(!hch01MenuItem.Visible);
				Assert(hdf01MenuItem.Visible);
				Assert(hdeMenuItem.Visible);
				Assert(!nvc01MenuItem.Visible);
				Assert(!nvc01BondedLocationAmendmentMenuItem.Visible);
			});
		}

		public void TestNVC01_BondedLocationAmendment()
		{
			using (var form = new ManifestForm(header))
			using (var dir = new TestTemporaryDirectory())
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var nvc01MenuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("NVC01 – House B/L Cargo Registration (Bonded Location Amendment)");

				ZFormModaliser.ShowDialogsInTest = true;

				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;

					var messageSendingParent = dialog.BusinessEntity;
					messageSendingParent.GetContentProviders();
					messageSendingParent.AllowSendWithError = true;
					((MessageSendingContext)messageSendingParent.Context).SendTarget = SendTarget.FlatFile;
					messageSendingParent.ExportPath = dir.Directory.FullName;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.ClearMessages();

				nvc01MenuItem.PerformClick();
				AssertEquals("Message is created.", 0, header.Messages.Count);
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessages();

				nvc01MenuItem.PerformClick();
				AssertEquals("Message is created.", 1, header.Messages.Count);
			}
		}

		public void TestClickSendOrExportNACCSMessageBeforeSavingWithEmptyBills()
		{
			using (var form = new ZForm(header))
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var hch01MenuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");

				hch01MenuItem.PerformClick();
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessages();
				hch01MenuItem.PerformClick();
				AssertEquals("There must be one bill at least. Please go to Main - Bills to add a bill first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetMessageSenders_NVC01_GroupByActionThenBatchByMaxLength()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			header.AMA_JobReference = "MAN000024";
			header.AMA_InputReference = "3456789012";

			for (var i = 0; i < 23; i++)
			{
				header.Bills.AddNew();
			}
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var sendingObjects = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ToList();

			for (var i = 0; i < 21; i++)
			{
				sendingObjects[i].Action = NVC01MessageActionList.Codes.Nine;
				sendingObjects[i].Bill.ABL_GoodsDescription = "GoodsDescription" + i;
				sendingObjects[i].ShouldSend = true;
			}

			sendingObjects[21].Action = NVC01MessageActionList.Codes.Five;
			sendingObjects[21].Bill.ABL_GoodsDescription = "GoodsDescription21";
			sendingObjects[21].ShouldSend = true;
			sendingObjects[22].Action = NVC01MessageActionList.Codes.Five;
			sendingObjects[22].Bill.ABL_GoodsDescription = "GoodsDescription22";
			sendingObjects[22].ShouldSend = true;

			using (var form = new ZForm())
			{
				var menuBuilder = new MenuBuilderForTest(header, form);
				var messageSenders = menuBuilder.GetMessageSenders(sendingObjects, JPProcedureCodeList.Codes.NVC01).ToArray();
				AssertEquals(3, messageSenders.Length);

				var bills = header.Bills;
				messageSenders[0].UpdateBillStatuses();
				AssertEquals(20, bills.Where(x => x.ABL_MessageStatus == JPMessageStatusList.Codes.Sending).Count());
				for (var i = 0; i < 20; i++)
				{
					Assert(bills[i].ABL_MessageStatus == JPMessageStatusList.Codes.Sending);
				}

				messageSenders[1].UpdateBillStatuses();
				AssertEquals(21, header.Bills.Where(x => x.ABL_MessageStatus == JPMessageStatusList.Codes.Sending).Count());
				for (var i = 0; i < 21; i++)
				{
					Assert(bills[i].ABL_MessageStatus == JPMessageStatusList.Codes.Sending);
				}

				messageSenders[2].UpdateBillStatuses();
				AssertEquals(23, header.Bills.Where(x => x.ABL_MessageStatus == JPMessageStatusList.Codes.Sending).Count());
				for (var i = 0; i < 23; i++)
				{
					Assert(bills[i].ABL_MessageStatus == JPMessageStatusList.Codes.Sending);
				}
			}
		}

		public void TestClickSendOrExportNACCSMessageBeforeSavingWithEmptyBills_NVC01()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			using (var form = new ZForm(header))
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var nvc01MenuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("NVC01 - House B/L Cargo Registration (Registration/Amendment/Cancellation)");

				nvc01MenuItem.PerformClick();
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessages();
				nvc01MenuItem.PerformClick();
				AssertNull("Should be no notification when sending NVC01", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportNACCSMessageMenuItem_Visibility()
		{
			using var form = new ZForm(header);
			var menu = new AsycudaMenuForTest(header);
			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var importNACCSMessageMenuItem = menu.MenuItems.FindByText("Import NACCS Message (.txt)");
			AssertNotNull("Import NACCS Message (.txt)", importNACCSMessageMenuItem);
			Assert("Menu item 'Import NACCS Message (.txt)' should be visible", importNACCSMessageMenuItem.Visible);
		}

		public void TestExportNACCSMessageMenu_Action()
		{
			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			using (var dir = new TestTemporaryDirectory())
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var hch01MenuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");

				ZFormModaliser.ShowDialogsInTest = true;

				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;

					var messageSendingParent = dialog.BusinessEntity;
					messageSendingParent.AllowSendWithError = true;
					((MessageSendingContext)messageSendingParent.Context).SendTarget = SendTarget.FlatFile;
					messageSendingParent.ExportPath = dir.Directory.FullName;

					var action = messageSendingParent.SendingObjectsCollection[0];
					action.ShouldSend = true;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.ClearMessages();

				hch01MenuItem.PerformClick();
				AssertEquals("Message is created.", 0, header.Messages.Count);
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessages();

				hch01MenuItem.PerformClick();
				AssertEquals("Message is created.", 1, header.Messages.Count);

				CombineAssertions(() =>
				{
					AssertEquals("EM_ApplicationReference", EDIMessage.FlatFile, header.Messages[0].EM_ApplicationReference);
					AssertEquals("EM_Status", EDIMessageStatusList.Codes.Sent, header.Messages[0].EM_Status);
					AssertEquals("Bill Message Status", JPMessageStatusList.Codes.Exported, bill.ABL_MessageStatus);
					AssertEquals("Bill Status", ZString.Empty, bill.ABL_BillStatus);
					AssertEquals("Manifest Message Status", JPMessageStatusList.Codes.Exported, header.AMA_MessageStatus);

					var files = dir.Directory.GetFiles();

					AssertEquals("Should create a flat file for saving the new message", 1, files.Length);
					Assert("Should export the message data to the file.", files[0].Length > 0);
					AssertEquals($"Export has been completed. All files can be found at {dir.Directory.FullName}.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestSubmit_Action()
		{
			AssertEquals(0, header.Messages.Count);
			var bill = header.Bills.AddNew();

			using var form = new ManifestForm(header);
			var menu = new AsycudaMenuForTest(header);
			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);
			var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
			var hch01MenuItem = sendOrExportNACCSMessageMenu.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingForm)obj;
				var messageSendingParent = dialog.BusinessEntity;
				messageSendingParent.AllowSendWithError = true;
				var action = messageSendingParent.SendingObjectsCollection[0];
				action.ShouldSend = true;
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			hch01MenuItem.PerformClick();

			AssertEquals("Message is created.", 0, header.Messages.Count);
			AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

			form.FireSaveButton();
			UnitTestUserNotification.Instance.ClearMessages();

			hch01MenuItem.PerformClick();
			AssertEquals("Message is created.", 1, header.Messages.Count);
			AssertEquals("Bill Message Status", JPMessageStatusList.Codes.Sending, bill.ABL_MessageStatus);
			AssertEquals("Manifest Message Status", JPMessageStatusList.Codes.Sending, header.AMA_MessageStatus);
			AssertEquals("Send Successful Message", "1 Message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSubmit_LogEventOnEverySingleMessage()
		{
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_MasterBill = "123456789";
			for (var i = 1; i < 51; i++)
			{
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = i.ToString().PadLeft(3, '0');
				bill.MarkLightValidationAsValidForTesting();
			}

			using (var form = new ManifestForm(header))
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var submitMenu = sendOrExportNACCSMessageMenu.MenuItems.FindByText("NVC01 - House B/L Cargo Registration (Registration/Amendment/Cancellation)");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;
					var messageSendingParent = dialog.BusinessEntity;
					messageSendingParent.AllowSendWithError = true;
					messageSendingParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault().ShouldSend = false;
				});

				CombineAssertions(() =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					submitMenu.PerformClick();

					var log = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeSent.Code)).FirstOrDefault();
					AssertNull("No message sent successfully", log);

					form.FireSaveButton();
					UnitTestUserNotification.Instance.ClearMessages();

					submitMenu.PerformClick();
					AssertEquals("Three messages were created.", 3, header.Messages.Count);
					var logs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.InterchangeSent.Code)).OrderBy(a => a.SL_PostedTimeUtc).ToList();
					AssertEquals("logs count", 3, logs.Count);
					AssertEquals("The first log", "   NVC01                                                                                                                                                                                                                   NVC01000000000100000000001        0000000001                                                                                                     2                           058101", logs[0].SL_Reference);
					AssertEquals("The second log", "   NVC01                                                                                                                                                                                                                   NVC01000000000100000000002        0000000001                                                                                                     2                           058101", logs[1].SL_Reference);
					AssertEquals("The third log", "   NVC01                                                                                                                                                                                                                   NVC01000000000100000000003        0000000001                                                                                                     2                           026267", logs[2].SL_Reference);
				});
			}
		}

		public void TestUpdateCustomsStatus_NVC01Message()
		{
			header.AMA_TransportMode = "SEA";
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			header.MessageInitiator = messageInitiator;
			header.AMA_JobReference = "MAN000024";
			header.AMA_InputReference = "3456789012";

			var bill = header.Bills.AddNew();

			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var sendingObjects = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ToList();

			sendingObjects[0].Action = NVC01MessageActionList.Codes.Nine;
			sendingObjects[0].ShouldSend = true;

			using var form = new ZForm();
			var menuBuilder = new MenuBuilderForTest(header, form);
			var messageSender = menuBuilder.GetMessageSenders(sendingObjects, JPProcedureCodeList.Codes.NVC01).ToArray()[0];

			messageSender.UpdateBillStatuses();
			AssertEquals(JPCustomsStatusList.Codes.AWR, bill.ABL_BillStatus);

			sendingObjects[0].Action = NVC01MessageActionList.Codes.Five;
			messageSender.UpdateBillStatuses();
			AssertEquals(JPCustomsStatusList.Codes.AWA, bill.ABL_BillStatus);

			sendingObjects[0].Action = NVC01MessageActionList.Codes.One;
			messageSender.UpdateBillStatuses();
			AssertEquals(JPCustomsStatusList.Codes.AWD, bill.ABL_BillStatus);
		}

		public void TestSendingWithOverriddenValuesLog()
		{
			header.AMA_MasterBill = "123456789";
			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);
				var sendOrExportNACCSMessageMenu = menu.MenuItems.FindByText("Send/Export NACCS Message");
				var hch01Menu = sendOrExportNACCSMessageMenu.MenuItems.FindByText("HCH01 - HAWB Registration (Import)");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm)obj;

					var messageSendingParent = dialog.BusinessEntity;
					messageSendingParent.AllowSendWithError = true;
					((MessageSendingContext)messageSendingParent.Context).SendTarget = SendTarget.FlatFile;

					var sendingObject = messageSendingParent.SendingObjectsCollection[0];
					sendingObject.ShouldSend = true;

					messageSendingParent.UseVisualData = true;
					var contentProviders = messageSendingParent.GetContentProviders();
					var visualObjectParent = messageSendingParent.VisualObjectParent;
					visualObjectParent.InitializeVisualObjects(contentProviders);
					var visualObject = visualObjectParent.VisualObjects.Cast<MessageVisualObject>().FirstOrDefault();
					visualObject.Header.Cast<EditableFieldBizObject>().FirstOrDefault().OverrideValue = "X";
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.ClearMessages();

				hch01Menu.PerformClick();

				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessages();
				hch01Menu.PerformClick();

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.Authorised.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, header.PK);
				var sendingWithOverriddenValuesLogs = new BusinessObjectFactory().Load<StmALog>(query).Where(log => log.SL_Reference.StartsWith("Sending with overridden values|EDIMessage Number="));
				AssertEquals(1, sendingWithOverriddenValuesLogs.Count());
			}
		}

		class MenuBuilderForTest : MenuBuilder
		{
			public MenuBuilderForTest(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
			{
			}

			public new IEnumerable<ManifestMessageSender> GetMessageSenders(IEnumerable<ManifestMessageSendingObject> sendingObjects, string procedureCode) => base.GetMessageSenders(sendingObjects, procedureCode);
		}

		AsycudaManifestHeader header;

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "JPTYO";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
		}
	}
}
