using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class AFRMainMenuItemTest : TestCaseWithFactory
	{
		public void TestBLLFunctionMenuItem()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var registerSplitMenuItem = afr.MenuItems.FindByText("Register Split Bill", true);
					var registerSwitchMenuItem = afr.MenuItems.FindByText("Register Switch Bill", true);
					var registerMergeMenuItem = afr.MenuItems.FindByText("Register Merge Bill", true);
					var cancelSplitMenuItem = afr.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = afr.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = afr.MenuItems.FindByText("Cancel Merge Bill", true);
					CombineAssertions(() =>
					{
						AssertNull(registerSplitMenuItem);
						AssertNull(registerSwitchMenuItem);
						AssertNull(registerMergeMenuItem);
						AssertNull(cancelSplitMenuItem);
						AssertNull(cancelSwitchMenuItem);
						AssertNull(cancelMergeMenuItem);
					});
				}
			}

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var registerSplitMenuItem = afr.MenuItems.FindByText("Register Split Bill", true);
					var registerSwitchMenuItem = afr.MenuItems.FindByText("Register Switch Bill", true);
					var registerMergeMenuItem = afr.MenuItems.FindByText("Register Merge Bill", true);
					var cancelSplitMenuItem = afr.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = afr.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = afr.MenuItems.FindByText("Cancel Merge Bill", true);
					CombineAssertions(() =>
					{
						AssertNotNull(registerSplitMenuItem);
						AssertNotNull(registerSwitchMenuItem);
						AssertNotNull(registerMergeMenuItem);
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
					});
				}
			}
		}

		public void TestRegisterationMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var registerSplitMenuItem = afr.MenuItems.FindByText("Register Split Bill", true);
					var registerSwitchMenuItem = afr.MenuItems.FindByText("Register Switch Bill", true);
					var registerMergeMenuItem = afr.MenuItems.FindByText("Register Merge Bill", true);
					var bLLFuncitonMenuItem = afr.MenuItems.FindByText("BLL Function", true);
					bLLFuncitonMenuItem.PerformSelect();
					CombineAssertions(() =>
					{
						AssertNotNull(registerSplitMenuItem);
						AssertNotNull(registerSwitchMenuItem);
						AssertNotNull(registerMergeMenuItem);
						Assert(registerSplitMenuItem.Visible);
						Assert(registerSwitchMenuItem.Visible);
						Assert(registerMergeMenuItem.Visible);
					});
				}
			}
		}

		public void TestCancelSplitMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill1.PK;
			bllFunctionInfo.B7_ParentTableCode = bill1.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_FunctionCode = (int)BLLFunctionCode.RegisterSplit;
			Factory.Save();

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var cancelSplitMenuItem = afr.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = afr.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = afr.MenuItems.FindByText("Cancel Merge Bill", true);
					var bLLFunctionMenuItem = afr.MenuItems.FindByText("BLL Function", true);
					bLLFunctionMenuItem.PerformSelect();
					CombineAssertions(() =>
					{
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
						Assert(cancelSplitMenuItem.Visible);
						Assert(!cancelSwitchMenuItem.Visible);
						Assert(!cancelSwitchMenuItem.Visible);
					});
				}
			}
		}

		public void TestCancelSwitchMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill1.PK;
			bllFunctionInfo.B7_ParentTableCode = bill1.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_FunctionCode = (int)BLLFunctionCode.RegisterSwitch;
			Factory.Save();

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var cancelSplitMenuItem = afr.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = afr.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = afr.MenuItems.FindByText("Cancel Merge Bill", true);
					var bLLFunctionMenuItem = afr.MenuItems.FindByText("BLL Function", true);
					bLLFunctionMenuItem.PerformSelect();
					CombineAssertions(() =>
					{
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
						Assert(!cancelSplitMenuItem.Visible);
						Assert(cancelSwitchMenuItem.Visible);
						Assert(!cancelMergeMenuItem.Visible);
					});
				}
			}
		}

		public void TestCancelMergeMenuItem_Visibility()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill1.PK;
			bllFunctionInfo.B7_ParentTableCode = bill1.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_FunctionCode = (int)BLLFunctionCode.RegisterMerge;
			Factory.Save();

			using (JPAFRRegistry.Instance.AFRShowBLLFunctions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var cancelSplitMenuItem = afr.MenuItems.FindByText("Cancel Split Bill", true);
					var cancelSwitchMenuItem = afr.MenuItems.FindByText("Cancel Switch Bill", true);
					var cancelMergeMenuItem = afr.MenuItems.FindByText("Cancel Merge Bill", true);
					var bLLFunctionMenuItem = afr.MenuItems.FindByText("BLL Function", true);
					bLLFunctionMenuItem.PerformSelect();
					CombineAssertions(() =>
					{
						AssertNotNull(cancelSplitMenuItem);
						AssertNotNull(cancelSwitchMenuItem);
						AssertNotNull(cancelMergeMenuItem);
						Assert(!cancelSplitMenuItem.Visible);
						Assert(!cancelSwitchMenuItem.Visible);
						Assert(cancelMergeMenuItem.Visible);
					});
				}
			}
		}

		[TestDate(2017, 5, 3)]
		public void TestSendBlanketVesselChangeMenuItem()
		{
			TestSendBlanketVesselChangeMenuItem_Visibilty(true);
			TestSendBlanketVesselChangeMenuItem_Visibilty(false);
		}

		void TestSendBlanketVesselChangeMenuItem_Visibilty(bool isShippingLineEntry)
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = isShippingLineEntry;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			using (JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 1, 1)))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var sendBlanketVesselChangeMenuItem = afr.MenuItems.FindByText("Send Blanket Vessel Change");
					AssertNull(sendBlanketVesselChangeMenuItem);
				}
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var sendBlanketVesselChangeMenuItem = afr.MenuItems.FindByText("Send Blanket Vessel Change");
					AssertNotNull(sendBlanketVesselChangeMenuItem);
				}
			}

			using (JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 10, 01)))
			{
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var sendBlanketVesselChangeMenuItem = afr.MenuItems.FindByText("Send Blanket Vessel Change");
					AssertNull(sendBlanketVesselChangeMenuItem);
				}
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				using (var afr = new AFRMainMenuItem(header))
				using (var form = new ZForm(header))
				{
					form.Menu.MenuItems.Add(afr);
					var sendBlanketVesselChangeMenuItem = afr.MenuItems.FindByText("Send Blanket Vessel Change");
					AssertNull(sendBlanketVesselChangeMenuItem);
				}
			}
		}

		public void TestMenuItemsWithInActiveHeader()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_IsActive = false;
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNull(registerMenuItem);

				registerMenuItem = afr.MenuItems.FindByText("Register Manifest Completion");
				AssertNull(registerMenuItem);

				registerMenuItem = afr.MenuItems.FindByText("Amendment Manifest");
				AssertNull(registerMenuItem);

				registerMenuItem = afr.MenuItems.FindByText("AFR Header deactivated, to reactivate go to Actions->Make Active");
				AssertNotNull(registerMenuItem);
			}
		}

		public void TestMenuItemsList()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = false;
			using (var afrMenuItems = new AFRMainMenuItem(header))
			{
				var menuItems = afrMenuItems.MenuItems;
				AssertEquals(3, menuItems.Count);
				AssertNotNull(menuItems.FindByText("Register Manifest"));
				AssertNotNull(menuItems.FindByText("Register Manifest Completion"));
				AssertNull(menuItems.FindByText("Register Departure Time"));
				AssertNotNull(menuItems.FindByText("Amendment Manifest"));
			}
			header.JPH_IsShippingLineEntry = true;
			using (var afrMenuItems = new AFRMainMenuItem(header))
			{
				var menuItems = afrMenuItems.MenuItems;
				AssertEquals(3, menuItems.Count);
				AssertNotNull(menuItems.FindByText("Register Manifest"));
				AssertNull(menuItems.FindByText("Register Manifest Completion"));
				AssertNotNull(menuItems.FindByText("Register Departure Time"));
				AssertNotNull(menuItems.FindByText("Amendment Manifest"));
			}
		}

		public void TestMenuItemsSendingWithInActiveHeader()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				header.JPH_IsActive = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Messages can not be sent when AFR Header is deactivated, to reactivate go to Actions->Make Active", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				registerMenuItem = afr.MenuItems.FindByText("Register Manifest Completion");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				registerMenuItem.PerformClick();
				AssertEquals("Messages can not be sent when AFR Header is deactivated, to reactivate go to Actions->Make Active", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				registerMenuItem = afr.MenuItems.FindByText("Amendment Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				registerMenuItem.PerformClick();
				AssertEquals("Messages can not be sent when AFR Header is deactivated, to reactivate go to Actions->Make Active", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);
			}
		}

		public void TestMenuItemsWithNoBOL_NVOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_IsShippingLineEntry = false;
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoUnRegisteredBillsNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);
			}
		}

		public void TestMenuItemsWithNoBOL_VOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_IsShippingLineEntry = true;
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoUnRegisteredBillsNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);
			}
		}

		public void TestMenuItemsWithBOL_NVOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_IsShippingLineEntry = false;
			var bill = Factory.New<JPAFRBills>();
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Please fix these errors before sending any messages:\r\n\r\nBill Of Lading: Bill Of Lading is missing\nMaster Bill Of Lading: Master Bill Of Lading is missing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				header.JPH_MasterBillNumber = "SPQAVICTM002";
				bill.JPB_BillNumber = "J07JVICTH002001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);

				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, message.EM_MessageOwner);
				var interchange = message.Interchange;
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			}
		}

		public void TestMenuItemWhenHeaderDeleted()
		{
			var header = Factory.New<JPAFRHeader>();
			header.LogBillRegistrationCompletion();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_CarrierCode = "SPQA";
			header.JPH_IsActive = false;

			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var normalMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNull(normalMenuItem);
				normalMenuItem = afr.MenuItems.FindByText("Register Manifest Completion");
				AssertNull(normalMenuItem);
				normalMenuItem = afr.MenuItems.FindByText("Amendment Manifest");
				AssertNull(normalMenuItem);
				var deactiveMenuItem = afr.MenuItems.FindByText("AFR Header deactivated, to reactivate go to Actions->Make Active");
				AssertNotNull(deactiveMenuItem);
			}
		}

		public void TestMenuItemsWithBOL_VOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_IsShippingLineEntry = true;
			var bill = Factory.New<JPAFRBills>();
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Please fix these errors before sending any messages:\r\n\r\nBill Of Lading: Bill Of Lading is missing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				bill.JPB_BillNumber = "J07JVICTH002001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);

				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster, message.EM_MessageOwner);
				var interchange = message.Interchange;
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			}
		}

		public void TestMenuItemWithBillRegistrationCompletionNotification_NVOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.LogBillRegistrationCompletion();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_CarrierCode = "SPQA";
			var bill = Factory.New<JPAFRBills>();
			bill.JPB_BillNumber = "J07JVICTH002001";
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.BillRegistrationCompletionNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				var log = header.Logs.MostRecentLogByEventTime(Events.TaskCompleted);
				log.Cancel();
				header.LogDepartureTimeRegistration();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, header.Bills[0].JPB_MessageStatus);
			}
		}

		public void TestMenuItemWithBillRegistrationCompletionNotification_VOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.LogDepartureTimeRegistration();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_CarrierCode = "SPQA";
			var bill = Factory.New<JPAFRBills>();
			bill.JPB_BillNumber = "J07JVICTH002001";
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DepartureTimeRegistrationCompletionNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				var log = header.Logs.MostRecentLogByEventTime(Events.TaskCompleted);
				log.Cancel();
				header.LogBillRegistrationCompletion();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingMasterBillRegistration, header.Bills[0].JPB_MessageStatus);
			}
		}

		public void TestMenuItemWithBOLAlreadyRegisteredOrMessageInProcess()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			var bill = Factory.New<JPAFRBills>();
			bill.JPB_BillNumber = "B324";
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, header.Bills[0].JPB_MessageStatus);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				var waitingForResponse = Business.ValidationConstants.MessageSending.BillsToSendThatAreWaitingForResponseMessage("B324");
				AssertEquals(waitingForResponse, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);

				header.Messages.RemoveAndDeleteAllFromTest();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, header.Bills[0].JPB_MessageStatus);

				header.Bills[0].JPB_MessageStatus = ZString.Empty;
				header.Bills[0].JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoUnRegisteredBillsNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(ZString.Empty, header.Bills[0].JPB_MessageStatus);

				var bill2 = header.Bills.AddNew();
				bill2.JPB_BillNumber = "J07JVICTH002002";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, header.Messages.Count);
				AssertEquals(MessageStatusList.Codes.AwaitingHouseBillRegistration, bill2.JPB_MessageStatus);
			}
		}

		public void TestSendingBillRegistrationCompletionMessage()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest Completion");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoBillsNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				var bill1 = header.Bills.AddNew();
				bill1.JPB_BillNumber = "HB1";
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				header.LogBillRegistrationCompletion();
				AssertEquals(true, header.IsBillRegistrationCompleted);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.BillRegistrationCompletionAlreadyDoneNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				bill1.JPB_ReleaseStatus = "";
				var log = header.Logs.MostRecentLogByEventTime(Events.TaskCompleted);
				AssertEquals(User.ServiceUserCode, log.SL_GS_NKUser);
				AssertEquals("Bill Registration", log.SL_Reference);
				log.SL_GS_NKUser = "ZZ";
				AssertEquals(false, header.IsBillRegistrationCompleted);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NotAllBillsHaveAlreadyBeenRegistered, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				var bill2 = header.Bills.AddNew();
				bill2.JPB_BillNumber = "HB2";
				bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NotAllBillsHaveAlreadyBeenRegistered, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);
			}
		}

		public void TestSendingATDRegistrationMessage_WithNoBill()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.JPH_CarrierCode = "SPQA";
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Departure Time");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoBillsNotification + " " + AFRMainMenuItem.Constants.Message.DepartureTimeSolelyRegistrationNotification,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				header.LogDepartureTimeRegistration();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Case: Already registered", "Departure Time Registration has already been lodged. Please use the 'Amendment Manifest' if you wish to correct the departure time registration.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Case: Already registered", 0, header.Messages.Count);
			}
		}

		public void TestSendingATDRegistrationMessage_WithBill()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.JPH_CarrierCode = "SPQA";
			var bill = Factory.New<JPAFRBills>();
			bill.JPB_BillNumber = "J07JVICTH002001";
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Departure Time");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertNotEquals("Case: Not Sending When Error Prompts", AFRMainMenuItem.Constants.Message.NoBillsNotification + " " + AFRMainMenuItem.Constants.Message.DepartureTimeSolelyRegistrationNotification,
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Case: Not Sending When Error Prompts", AFRMainMenuItem.Constants.Message.NotAllBillsHaveAlreadyBeenRegistered, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Case: Not Sending When Error Prompts", 0, header.Messages.Count);

				header.LogDepartureTimeRegistration();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals("Case: Already registered", "Departure Time Registration has already been lodged. Please use the 'Amendment Manifest' if you wish to correct the departure time registration.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Case: Already registered", 0, header.Messages.Count);

				header.LogDepartureTimeRegistration();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Case: Already registered 2", "Departure Time Registration has already been lodged. Please use the 'Amendment Manifest' if you wish to correct the departure time registration.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Case: Already registered 2", 0, header.Messages.Count);
			}
		}

		public void TestSendWithMessageErrorSecurityCheckPoint()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			var bill = Factory.New<JPAFRBills>();
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(true, header.IsInDatabase);
				AssertEquals(0, header.Messages.Count);

				Env.Security.JPAFRReportingSendWithMessageErrors.IsAllowed = false;
				header.JPH_MasterBillNumber = "SPQAVICTM002";
				bill.JPB_BillNumber = "J07JVICTH002001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertContains("There are message errors on this job and you don't have security rights to send with message errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				Env.Security.JPAFRReportingSendWithMessageErrors.IsAllowed = true;
				header.JPH_MasterBillNumber = "SPQAVICTM002";
				bill.JPB_BillNumber = "J07JVICTH002002";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
			}
		}

		public void TestSendWithConsolNoDischargeinJapan()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "SPQA";
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill = Factory.New<JPAFRBills>();
			header.Bills.Add(bill);
			using (var afr = new AFRMainMenuItem(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(afr);
				var registerMenuItem = afr.MenuItems.FindByText("Register Manifest");
				AssertNotNull(registerMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(Business.ValidationConstants.MessageSending.ConsolDoesNotDischargeInJapan, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				registerMenuItem.PerformClick();
				AssertEquals(Business.ValidationConstants.MessageSending.ConsolDoesNotDischargeInJapan, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);

				header.JPH_MasterBillNumber = "SPQAVICTM002";
				bill.JPB_BillNumber = "J07JVICTH002001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(1, header.Messages.Count);

				var testLeg = consol.Transports.AddNew();
				testLeg.JW_RL_NKLoadPort = "AUSYD";
				testLeg.JW_RL_NKDiscPort = "NZAKL";
				bill.JPB_GoodsDescription = "GD1";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(Business.ValidationConstants.MessageSending.ConsolDoesNotDischargeInJapan, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);

				testLeg.JW_RL_NKLoadPort = "JPAAM";
				testLeg.JW_RL_NKDiscPort = "JPTKY";
				bill.JPB_GoodsDescription = "GD2";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals(Business.ValidationConstants.MessageSending.ConsolDoesNotDischargeInJapan, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);

				testLeg.JW_RL_NKLoadPort = "AUSYD";
				testLeg.JW_RL_NKDiscPort = "JPTKY";
				bill.JPB_GoodsDescription = "GD3";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				registerMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, header.Messages.Count);
			}
		}

		public void TestSendMessageForAmendmentForm()
		{
			CombineAssertions("Case: Send Partial Bill Update", () =>
			{
				var header = Factory.New<JPAFRHeader>();
				header.JPH_CarrierCode = "SPQA";
				header.JPH_MasterBillNumber = "MBOL";
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				bill1.JPB_BillNumber = "HBOL1";
				bill2.JPB_BillNumber = "HBOL2";
				Factory.Save();
				AssertEquals("PRE: Prepare for Normal CHR Messaging with Partial Sending", 0, header.Messages.Count);

				var testMenuItem = new AFRMainMenuItem(header);
				var testSendingAction = new MessageSendingAction(header, ActionCode.AmendingAdd);
				testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "HBOL1").JPM_Send = false;

				testMenuItem.SendMessageForAmendmentForm(testSendingAction);
				AssertEquals("Should have 1 message Created", 1, header.Messages.Count);
				AssertEquals("OutGoing MessageType", MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, header.Messages[0].EM_MessageOwner);
				AssertEquals("Notification Message", "Sent one Manifest Amendment Message containing details of 1 Bill.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MessageStatus for Bill1", string.Empty, bill1.JPB_MessageStatus);
				AssertEquals("MessageStatus for Bill2", MessageStatusList.Codes.AwaitingHouseBillRegistration, bill2.JPB_MessageStatus);
			});
			CombineAssertions("Case: Send Bill Update", () =>
			{
				var header = Factory.New<JPAFRHeader>();
				header.JPH_CarrierCode = "SPQA";
				header.JPH_MasterBillNumber = "MBOL";
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				bill1.JPB_BillNumber = "HBOL1";
				bill1.JPB_BillNumber = "HBOL2";
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				Factory.Save();
				AssertEquals("PRE: Prepare for Normal CHR Messaging", 0, header.Messages.Count);

				var testMenuItem = new AFRMainMenuItem(header);
				var testSendingAction = new MessageSendingAction(header, ActionCode.AmendingAdd);

				testMenuItem.SendMessageForAmendmentForm(testSendingAction);
				AssertEquals("Should have 1 message Created", 1, header.Messages.Count);
				AssertEquals("OutGoing MessageType", MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, header.Messages[0].EM_MessageOwner);
				AssertEquals("Notification Message", "Sent one Manifest Amendment Message containing details of 2 Bills.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MessageStatus for Bill1", MessageStatusList.Codes.AwaitingHouseBillUpdate, bill1.JPB_MessageStatus);
				AssertEquals("MessageStatus for Bill2", MessageStatusList.Codes.AwaitingHouseBillUpdate, bill2.JPB_MessageStatus);
			});
			CombineAssertions("Case: Send Bill Update with ATD Update", () =>
			{
				var header = Factory.New<JPAFRHeader>();
				header.JPH_IsShippingLineEntry = true;
				header.JPH_CarrierCode = "SPQA";
				header.LogDepartureTimeRegistration();
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				var bill3 = header.Bills.AddNew();
				bill1.JPB_BillNumber = "MBOL1";
				bill2.JPB_BillNumber = "MBOL2";
				bill3.JPB_BillNumber = "MBOL3";
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				Factory.Save();
				AssertEquals("PRE: Prepare for Change DepartureTime", 0, header.Messages.Count);

				var testMenuItem = new AFRMainMenuItem(header);
				var testSendingAction = new MessageSendingAction(header, ActionCode.AmendingAdd);
				testSendingAction.UpdateMessageSendingAction(ActionCode.ChangeDepartureTimeAfterATD);
				testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL1").JPM_Send = false;
				Assert(testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL2").JPM_Send);
				Assert(testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL3").JPM_Send);

				testMenuItem.SendMessageForAmendmentForm(testSendingAction);
				var messages = header.Messages.ToArray<EDIMessage>().OrderBy(msg => msg.EM_MessageNum).ToArray();
				AssertEquals("Should have 2 message Created", 2, header.Messages.Count);
				AssertEquals("1st OutGoing MessageType", MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, messages[0].EM_MessageOwner);
				AssertEquals("2nd OutGoing MessageType", MessagingTypeList.Codes.DepartureTimeRegistration, messages[1].EM_MessageOwner);
				AssertEquals("Notification Message", "Sent one Manifest Amendment Message containing details of 2 Bills.\r\nSent one Departure Time Correction Message\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MessageStatus for header", MessageStatusList.Codes.AwaitingDepartureTimeRegistration, header.JPH_MessageStatus);
				AssertEquals("RegistrationStatus for header", "Completed", header.JPH_BillRegistrationStatus);
			});
			CombineAssertions("Case: Send ATD Update Only", () =>
			{
				var header = Factory.New<JPAFRHeader>();
				header.JPH_IsShippingLineEntry = true;
				header.JPH_CarrierCode = "SPQA";
				header.LogDepartureTimeRegistration();
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				var bill3 = header.Bills.AddNew();
				bill1.JPB_BillNumber = "MBOL1";
				bill2.JPB_BillNumber = "MBOL2";
				bill3.JPB_BillNumber = "MBOL3";
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				Factory.Save();
				AssertEquals("PRE: Prepare for Change DepartureTime", 0, header.Messages.Count);

				var testMenuItem = new AFRMainMenuItem(header);
				var testSendingAction = new MessageSendingAction(header, ActionCode.AmendingAdd);
				testSendingAction.UpdateMessageSendingAction(ActionCode.ChangeDepartureTimeAfterATD);
				testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL1").JPM_Send = false;
				testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL2").JPM_Send = false;
				testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL3").JPM_Send = false;

				testMenuItem.SendMessageForAmendmentForm(testSendingAction);
				var messages = header.Messages.ToArray<EDIMessage>().OrderBy(msg => msg.EM_MessageNum).ToArray();
				AssertEquals("Should have 1 message Created", 1, header.Messages.Count);
				AssertEquals("1nd OutGoing MessageType", MessagingTypeList.Codes.DepartureTimeRegistration, messages[0].EM_MessageOwner);
				AssertEquals("Notification Message", "Sent one Manifest Amendment Message containing details of 2 Bills.\r\nSent one Departure Time Correction Message\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MessageStatus for header", MessageStatusList.Codes.AwaitingDepartureTimeRegistration, header.JPH_MessageStatus);
				AssertEquals("RegistrationStatus for header", "Completed", header.JPH_BillRegistrationStatus);
			});
			CombineAssertions("Case: Update VesselInformation", () =>
			{
				var header = Factory.New<JPAFRHeader>();
				header.JPH_IsShippingLineEntry = true;
				header.JPH_CarrierCode = "SPQA";
				header.LogDepartureTimeRegistration();
				var bill1 = header.Bills.AddNew();
				var bill2 = header.Bills.AddNew();
				var bill3 = header.Bills.AddNew();
				bill1.JPB_BillNumber = "MBOL1";
				bill2.JPB_BillNumber = "MBOL2";
				bill3.JPB_BillNumber = "MBOL3";
				bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
				Factory.Save();
				AssertEquals("PRE: Prepare for Change DepartureTime", 0, header.Messages.Count);

				var testMenuItem = new AFRMainMenuItem(header);
				var testSendingAction = new MessageSendingAction(header, ActionCode.AmendingAdd);
				testSendingAction.UpdateMessageSendingAction(ActionCode.ReRegisterMasterAfterATD);
				testSendingAction.MessageSendingObjects.ToArray().OfType<MessageSendingObject>().FirstOrDefault(obj => obj.Bill.JPB_BillNumber == "MBOL1").JPM_Send = false;

				testMenuItem.SendMessageForAmendmentForm(testSendingAction);
				var messages = header.Messages.ToArray<EDIMessage>().OrderBy(msg => msg.EM_MessageNum);
				AssertEquals("Should have 1 message Created", 1, header.Messages.Count);
				AssertEquals("1st OutGoing MessageType", MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, header.Messages[0].EM_MessageOwner);
				AssertEquals("Notification Message", "Sent one Manifest Amendment Message containing details of 2 Bills.\r\nDeparture Time Registration Log has been Canceled\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MessageStatus for header", string.Empty, header.JPH_MessageStatus);
				AssertEquals("RegistrationStatus for header", string.Empty, header.JPH_BillRegistrationStatus);
			});
		}

		public void TestSpelling()
		{
			Assert(string.CompareOrdinal(MessageStatusList.Descriptions.AwaitingBLLRegistration, "Awaiting BLL Registration") == 0);
		}
	}
}
