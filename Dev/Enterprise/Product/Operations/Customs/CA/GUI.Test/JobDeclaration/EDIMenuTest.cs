using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.CA.Services.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.CA.Business.CusEntryHeader;
using CustomsGUI = Enterprise.Customs.GUI;
using IUserNotification = Enterprise.Customs.Business.MessageManagers.IUserNotification;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class EDIMenuTest : CustomsGUI.Testing.TestCaseForAttachGUI
	{
		public void TestCheckDeclarationAndSendMessage_Exception()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";

			Factory.Save();

			var createTimeUtc = ZDateTime.UtcNow;
			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var builder = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Original, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			message.EM_Status = MessageStatusList.Codes.Sent;
			message.EM_SystemCreateTimeUtc = createTimeUtc;
			message.EM_MessageText = @"<s0:UniversalEvent><s0:Event><s0:EventType>MAA</s0:EventType><s0:EventReference>IID Accepted</s0:EventReference><s0:ContextCollection><s0:Context><s0:Type>InterchangeNumber</s0:Type><s0:Value>10484</s0:Value></s0:Context><s0:Context><s0:Type>MessageNumber</s0:Type><s0:Value>1</s0:Value></s0:Context><s0:Context><s0:Type>IsTest</s0:Type><s0:Value>Y</s0:Value></s0:Context><s0:Context><s0:Type>OrganizationReference</s0:Type><s0:Value>B00172152</s0:Value></s0:Context></s0:ContextCollection></s0:Event></s0:UniversalEvent>";

			var messageMAA = Factory.New<UniversalEventMessage>();
			messageMAA.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			messageMAA.EM_MessageText = message.EM_MessageText;
			messageMAA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageMAA.EM_SystemCreateTimeUtc = createTimeUtc;

			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = messageMAA.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;
			Factory.Save();

			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				var sendIIDCancelMessage = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("Send IID Cancel Message");
				AssertNotNull(sendIIDCancelMessage);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertExceptionThrown<DeveloperNotificationException>(() => sendIIDCancelMessage.PerformClick());
				AssertEquals("Failed to create IID Cancellation message. Please try to save the changes and send the IID Cancellation message again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2024, 3, 15)]
		public void TestClickForceSendB3MessageMenuWillPopAWarningBoxWhenFuncCBSABOIsActivated()
		{
			var forceSendB3MessageMenuName = "Force Send Entry Message";
			var isCARMBlackoutperiod = "CBSA is currently in the CARM Blackout period where no CAD entries may be sent. Entry cannot be sent at this time.";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CBSABO, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				CusEntryHeader entry;
				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;
					var forcedMessageMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(forceSendB3MessageMenuName);
					AssertNotNull(forcedMessageMenuItem);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					forcedMessageMenuItem.PerformClick();
					AssertEquals(ZDateTime.Empty, entry.CH_EntrySubmittedDate);
					AssertEquals(isCARMBlackoutperiod, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				entry = declaration.B3EntryHeader;
				var message = entry.Messages.AddNew();
				message.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				message.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
				message.EM_Status = EDIMessageStatusList.Codes.Queued;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_HeldUntilDate = ZDateTime.UtcToday.AddDays(10);
				Factory.Save();

				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;
					var forcedMessageMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(forceSendB3MessageMenuName);
					AssertNotNull(forcedMessageMenuItem);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					forcedMessageMenuItem.PerformClick();
					AssertEquals(true, entry.CH_EntrySubmittedDate.IsEmpty);
					AssertEquals(isCARMBlackoutperiod, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSendCADQuery()
		{
			var sendCADQuery = "Send CAD Query";
			var declaration = Factory.New<JobDeclaration>();

			AssertSendCADQueryMenuVisible(false);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				AssertSendCADQueryMenuVisible(true);

				var b3CHeader = declaration.ActiveEntryHeaders.AddNew();
				b3CHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				AssertSendCADQueryMenuVisible(true);

				b3CHeader.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
				AssertSendCADQueryMenuVisible(false);

				b3CHeader.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
				AssertSendCADQueryMenuVisible(false);

				b3CHeader.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
				AssertSendCADQueryMenuVisible(false);
			}

			void AssertSendCADQueryMenuVisible(bool visible)
			{
				using (var menu = new EDIMenu())
				{
					menu.Declaration = declaration;
					var sendCADQueryMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendCADQuery);
					AssertEquals(visible, sendCADQueryMenu.Visible);
				}
			}
		}

		public void TestSendCADQuery_Click()
		{
			var sendCADQuery = "Send CAD Query";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var sendCADQueryMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendCADQuery);
					sendCADQueryMenu.PerformClick();
					AssertEquals("Entry should have a valid CAD Submitted date.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				declaration.B3EntryHeader.CH_EntrySubmittedDate = DateTime.Now;
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var sendCADQueryMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendCADQuery);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendCADQueryMenu.PerformClick();
					AssertEquals("CARM API Key and CARM End Point need to have a valid value, please setup in Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CARM -> CARM API Key and Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CARM -> CARM End Point.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
				using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var sendCADQueryMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendCADQuery);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendCADQueryMenu.PerformClick();
					AssertEquals("CARM API Key and CARM End Point need to have a valid value, please setup in Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CARM -> CARM API Key and Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CARM -> CARM End Point.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
				using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CADTEST://adwadxv19923hhiwir1r34-CADQueryMessageTest.cadtest"))
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var sendCADQueryMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendCADQuery);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertExceptionThrown<Exception>(() =>
					{
						sendCADQueryMenu.PerformClick();
					});
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					using (var progressForm = (ProgressForm)ZApplication.GetOpenForms().FirstOrDefault(x => x.GetType() == typeof(ProgressForm)))
					{
						AssertNotNull(progressForm);
						AssertEquals("CAD Query Start", progressForm.Status);
						AssertEquals(50, progressForm.PercentComplete);
					}
				}
			}
		}

		public void TestSendAsDeclaredAndAsAdjustedCADMessage()
		{
			var sendAsDeclaredCADMessageMenuName = "Send As Declared CAD Message";
			var sendAsAdjustedCADMessageMenuName = "Send As Adjusted CAD Message";
			var previousDeclaration = Factory.New<JobDeclaration>();
			var declaration = Factory.New<JobDeclaration>();
			previousDeclaration.RelatedDeclarations.Add(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			CusEntryHeader entry;
			using (var menu = new EDIMenu())
			{
				entry = declaration.B3EntryHeader;
				menu.Declaration = declaration;
				var sendAsDeclaredCADMessageMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendAsDeclaredCADMessageMenuName);
				Assert(sendAsDeclaredCADMessageMenu.Visible);

				var sendAsAdjustedCADMessageMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendAsAdjustedCADMessageMenuName);
				Assert(sendAsAdjustedCADMessageMenu.Visible);
			}
		}

		public void TestSendAdjustedCADMessage_Click()
		{
			var sendAsAdjustedCADMessageMenuName = "Send As Adjusted CAD Message";
			var previousDeclaration = Factory.New<JobDeclaration>();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				previousDeclaration.RelatedDeclarations.Add(declaration);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				Env.Security.CAManualReleaseCancel.IsAllowed = true;

				CusEntryHeader entry;
				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;

					var sendAsAdjustedCADMessageMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendAsAdjustedCADMessageMenuName);
					sendAsAdjustedCADMessageMenu.PerformClick();
					AssertEquals("Submit a CAD As Declared to initiate the As Adjusted message.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				entry = declaration.B3EntryHeader;
				entry.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
				Factory.Save();

				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;

					var sendAsAdjustedCADMessageMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(sendAsAdjustedCADMessageMenuName);
					sendAsAdjustedCADMessageMenu.PerformClick();
					AssertEquals("Send As Adjusted CAD Message form.", typeof(CADCorrectionMessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}

		public void TestForceSendCADOriginalMessageAndForceSendCADCorOrAdjMessage()
		{
			var forceSendCADOriginalMessageMenuName = "Force Send of CAD Original Message";
			var forceSendCADCorAdjMessageMenuName = "Force Send of CAD Cor/Adj Message";
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				CusEntryHeader entry;
				using (var menu = new EDIMenuForTesting())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;
					var forceSendCADOriginalMessageMenu = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(EDIMenu.ForcedMessageText).MenuItems.FindByText(forceSendCADOriginalMessageMenuName);
					Assert(forceSendCADOriginalMessageMenu.Visible);
					AssertEquals(0, entry.Messages.Count);
					menu.ExposedNotification.NextAnswer = true;
					forceSendCADOriginalMessageMenu.PerformClick();
					AssertEquals(1, entry.Messages.Count);
					AssertEquals(ZDateTime.Empty, entry.Messages[0].EM_HeldUntilDate);

					var forceSendCADCorAdjMessage = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(EDIMenu.ForcedMessageText).MenuItems.FindByText(forceSendCADCorAdjMessageMenuName);
					Assert(forceSendCADCorAdjMessage.Visible);
				}
			}
		}

		public void TestForceSendMessageWhenNoDeferredMessage()
		{
			var forceSendCADCorAdjMessageMenuName = "Force Send of CAD Cor/Adj Message";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				CusEntryHeader entry;
				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;

					var forcedMessageMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(EDIMenu.ForcedMessageText).MenuItems.FindByText(forceSendCADCorAdjMessageMenuName);
					AssertNotNull(forcedMessageMenuItem);

					forcedMessageMenuItem.PerformClick();
					AssertEquals(ZDateTime.Empty, entry.CH_EntrySubmittedDate);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				entry = declaration.B3EntryHeader;
				var heldDate = ZDateTime.UtcToday.AddDays(10);
				var message01 = entry.Messages.AddNew();
				var message02 = entry.Messages.AddNew();
				var message03 = entry.Messages.AddNew();
				message01.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				message02.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				message03.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
				message01.EM_MessageSubType = IIDMessageSubTypeList.Codes.Original;
				message02.EM_MessageSubType = IIDMessageSubTypeList.Codes.Change;
				message03.EM_MessageSubType = IIDMessageSubTypeList.Codes.Amendment;
				message01.EM_Status = EDIMessageStatusList.Codes.Queued;
				message02.EM_Status = EDIMessageStatusList.Codes.Queued;
				message03.EM_Status = EDIMessageStatusList.Codes.Queued;
				message01.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message02.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message03.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message01.EM_HeldUntilDate = heldDate;
				message02.EM_HeldUntilDate = heldDate;
				message03.EM_HeldUntilDate = heldDate;
				Factory.Save();

				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;

					var forcedMessageMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(EDIMenu.ForcedMessageText).MenuItems.FindByText(forceSendCADCorAdjMessageMenuName);
					AssertNotNull(forcedMessageMenuItem);

					forcedMessageMenuItem.PerformClick();
					AssertEquals("Force Send of CAD Cor/Adj Message form.", typeof(CADCorrectionMessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals(true, entry.CH_EntrySubmittedDate.IsEmpty);
				}

				message01.EM_HeldUntilDate = heldDate;
				message02.EM_HeldUntilDate = heldDate;
				entry.CH_EntrySubmittedDate = ZDateTime.Empty;
				Factory.Save();

				var factory = new BusinessObjectFactory();
				var group = factory.New<GlbGroup>();
				group.GG_Code = "ABC";
				var user = factory.New<GlbStaff>();
				user.FillWithValidTestData();
				user.GS_Code = "AMA";
				user.GS_LoginName = "anton";
				user.StaffPlainTextPassword = "password";
				user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				group.Staff.Add(user);

				var se = factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.CAForceSendB3Message.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = user.PK;
				se.GU_GB = GlbBranch.CurrentBranch.PK;
				factory.Save();

				using (var menu = new EDIMenu())
				{
					entry = declaration.B3EntryHeader;
					menu.Declaration = declaration;
					var forcedMessageMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText(EDIMenu.ForcedMessageText).MenuItems.FindByText(forceSendCADCorAdjMessageMenuName);
					AssertNotNull(forcedMessageMenuItem);

					try
					{
						Env.Security.CAForceSendB3Message.IsAllowed = false;
						GlbStaff.CurrentUser.GS_IsController = false;
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
						forcedMessageMenuItem.PerformClick();
						AssertEquals(ZDateTime.Empty, entry.CH_EntrySubmittedDate);
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					}
					finally
					{
						Env.Security.CAForceSendB3Message.IsAllowed = true;
						GlbStaff.CurrentUser.GS_IsController = true;
					}
				}
			}
		}

		public void TestAIRSValidation_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = "463";
			CACustomsDataRegistry.Instance.AIRSValidationRequestURL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid AIRS Request");
			CACustomsDataRegistry.Instance.WebProxyAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Invalid WebProxyAddress");
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var menuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - All Lines");
				menuItem.PerformClick();
				AssertEquals(@"AIRS Validation Key hasn't been setup. The key is allocated by CFIA. Please set it up in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> AIRS Validation Key
An invalid URL entered in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> AIRS Validation Request URL
An invalid URL entered in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> Web Proxy Address", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestManualSubmission_ReleaseEntry_Click_MissingExpectedEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Env.Security.CAManualReleaseCancel.IsAllowed = true;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var manualSubmissionMenuItem = menu.MenuItems.FindByText("Manual Submission");
				var menuEntrySubmission = manualSubmissionMenuItem.MenuItems.FindByText("Release Entry Manual Submission");
				menuEntrySubmission.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedtMessage =
					"Entries for this job have not been generated, please run the Brokerage->Generate Entries (Merge) menu item and try again.";
				AssertEquals("Entries not created", expectedtMessage, lastMessage);
			}
		}

		public void TestManualSubmission_B3Entry_Click_MissingExpectedEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Env.Security.CAManualReleaseCancel.IsAllowed = true;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var manualSubmissionMenuItem = menu.MenuItems.FindByText("Manual Submission");
				var menuEntrySubmission = manualSubmissionMenuItem.MenuItems.FindByText("CAD Entry Manual Submission");
				menuEntrySubmission.PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedtMessage =
					"Entries for this job have not been generated, please run the Brokerage->Generate Entries (Merge) menu item and try again.";
				AssertEquals("Entries not created", expectedtMessage, lastMessage);
			}
		}

		public void TestManualReleaseWithSupervisorOverrides()
		{
			AssertSupervisorOverrides("Manual Release", typeof(ManualReleaseForm));
		}

		public void TestManualReleaseWithSupervisorOverrides201()
		{
			AssertSupervisorOverrides201("Manual Release", typeof(ManualReleaseForm));
		}

		public void TestManualCancelWithSupervisorOverrides()
		{
			AssertSupervisorOverrides("Manual Cancel", typeof(ManualCancelForm));
		}

		public void TestManualCancelWithSupervisorOverrides201()
		{
			AssertSupervisorOverrides201("Manual Cancel", typeof(ManualCancelForm));
		}

		public void TestManualSubmission_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var testB3CHeader = declaration.ActiveEntryHeaders.AddNew();
			testB3CHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var testRELHeader = declaration.ActiveEntryHeaders.AddNew();
			testRELHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			Factory.Save();

			Env.Security.CAManualReleaseCancel.IsAllowed = true;

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var manualSubmissionMenuItem = menu.MenuItems.FindByText("Manual Submission");
				var menuEntrySubmission = manualSubmissionMenuItem.MenuItems.FindByText("CAD Entry Manual Submission");
				menuEntrySubmission.PerformClick();

				var lastDialogForm = ZFormModaliser.LastFormShownDialogForTest;
				Assert("ManualSubmissionForm opened", lastDialogForm is ManualSubmissionForm);
			}
		}

		public void TestManualSubmissionCAD_Click()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				var testCADCHeader = declaration.ActiveEntryHeaders.AddNew();
				testCADCHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

				var testRELHeader = declaration.ActiveEntryHeaders.AddNew();
				testRELHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

				Factory.Save();

				Env.Security.CAManualReleaseCancel.IsAllowed = true;

				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;

					var manualSubmissionMenuItem = menu.MenuItems.FindByText("Manual Submission");
					var menuEntrySubmission = manualSubmissionMenuItem.MenuItems.FindByText("CAD Entry Manual Submission");
					menuEntrySubmission.PerformClick();

					var lastDialogForm = ZFormModaliser.LastFormShownDialogForTest;
					Assert("ManualSubmissionForm opened", lastDialogForm is ManualSubmissionForm);
				}
			}
		}

		public void TestManualCancelMenuVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				var manualCancelMenuItem = menu.MenuItems.FindByText("Manual Cancel");
				AssertNotNull("Manual Cancel Menu Item", manualCancelMenuItem);
				Assert("Manual Cancel Menu Item", manualCancelMenuItem.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				Factory.Save();
				menu.RefreshMenu();
				manualCancelMenuItem = menu.MenuItems.FindByText("Manual Cancel");
				Assert("Manual Cancel Menu Item", !manualCancelMenuItem.Visible);
			}
		}

		public void TestAIRSValidationMenuVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				var menuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - All Lines");
				AssertNotNull("AIRS Validation Menu Item", menuItem);
				Assert("AIRS Validation Menu Item", menuItem.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				Factory.Save();
				menu.RefreshMenu();
				menuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - All Lines");
				Assert("AIRS Validation Menu Item", !menuItem.Visible);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				Factory.Save();
				menu.RefreshMenu();
				menuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - All Lines");
				AssertNotNull(menuItem);
				Assert(menuItem.Visible);
			}
		}

		public void TestManualSubmissionReleaseMenuVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Env.Security.CAManualReleaseCancel.IsAllowed = true;

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var manualSubmissionMenuItem = menu.MenuItems.FindByText("Manual Submission");
				var manualReleaseMenuItem = menu.MenuItems.FindByText("Manual Release");

				AssertNotNull("Manual Submission Menu Item", manualSubmissionMenuItem);
				AssertNotNull("Manual Release Menu Item", manualReleaseMenuItem);

				Assert("Manual Submission Menu Item", manualSubmissionMenuItem.Visible);
				Assert("Manual Release Menu Item", manualReleaseMenuItem.Visible);

				Env.Security.CAManualReleaseCancel.IsAllowed = false;
				menu.RefreshMenu();

				Assert("Manual Release Menu Item", !manualReleaseMenuItem.Visible);
				AssertNotNull("Manual Submission Menu Item", !manualSubmissionMenuItem.Visible);

				Env.Security.CAManualReleaseCancel.IsAllowed = true;
				declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				Factory.Save();
				menu.RefreshMenu();

				AssertNotNull("Manual Submission Menu Item", !manualSubmissionMenuItem.Visible);
				Assert("Manual Release Menu Item", !manualReleaseMenuItem.Visible);
			}
		}

		public void TestPromptSaveJobBeforeSendingMessagesOnShipmentForm()
		{
			var i = 0;
			foreach (ZString menuItemName in new[] { "Send G7 Export Declaration", "Send Entry Message" })
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_UniqueConsignRef = $"SH{i++:D8}";
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = true;
				Factory.Save();
				shipment.JS_UniqueConsignRef = menuItemName.Left(10);
				using (var form = new ZForm(shipment))
				using (var menu = new EDIMenuForTesting())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var item = GetMenuItem(menu, menuItemName);
					menu.ExposedNotification.NextAnswer = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("Pre-condition", shipment.HasChanges);
					item.PerformClick();
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", menu.ExposedNotification.LastMessage);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.ExposedNotification.NextAnswer = true;
					item.PerformClick();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPromptSaveJobBeforeSendingMessages()
		{
			foreach (var menuItemName in new[] { "Send G7 Export Declaration", "Send Entry Message" })
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				Factory.Save();
				declaration.JE_OwnerRef = "ABC";
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenuForTesting())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var item = GetMenuItem(menu, menuItemName);
					menu.ExposedNotification.NextAnswer = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("Pre-condition", declaration.HasChanges);
					item.PerformClick();
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", menu.ExposedNotification.LastMessage);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.ExposedNotification.NextAnswer = true;
					UnitTestUserNotification.Instance.AddOKAnswer();
					item.PerformClick();
					Assert(!declaration.HasChanges);
				}
			}
		}

		public void TestPromptSaveJobBeforeSendingB3XMessages()
		{
			foreach (var menuItemName in new[] { "Send X Type message" })
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				Factory.Save();
				declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				declaration.JE_OwnerRef = "ABC";
				declaration.CA_OriginalTransactionNo = "12234567890";
				declaration.CA_AmendmentTo = "B2";
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenuForTesting())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					var item = GetMenuItem(menu, menuItemName);
					menu.ExposedNotification.NextAnswer = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Assert("Pre-condition", declaration.HasChanges);
					item.PerformClick();
					AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", menu.ExposedNotification.LastMessage);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.ExposedNotification.NextAnswer = true;
					UnitTestUserNotification.Instance.AddOKAnswer();
					item.PerformClick();
					Assert(!declaration.HasChanges);
				}
			}
		}

		public void TestG7ForcedMessages()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICEN", Core.Constants.CountryCodes.Canada);
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.JE_MessageStatus = MessageStatusList.Codes.NotSent;
			Factory.Save();
			using (var menu = new EDIMenuForTesting())
			{
				menu.Declaration = declaration;
				var forcedMessageMenuItem = menu.MenuItems.FindByText(EDIMenu.ForcedMessageText);
				AssertNotNull(forcedMessageMenuItem);
				var amendSendMenuItem = forcedMessageMenuItem.MenuItems.FindByText(EDIMenu.ForcedSendOfAmendmentMessageText("G7"));
				AssertNotNull(amendSendMenuItem);
				menu.ExposedNotification.NextAnswer = true;
				amendSendMenuItem.PerformClick();
				AssertEquals("1 message", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("Amend message despite of current status is not sent", Common.Shared.MessageSubTypeCodes.Codes.Change, declaration.ActiveEntryHeaders[0].Messages[0].EM_MessageSubType);

				declaration.JE_MessageStatus = MessageStatusList.Codes.ClearOriginal;
				declaration.ActiveEntryHeaders[0].Messages.RemoveAndDeleteAllFromTest();
				Factory.Save();
				var originalSendMenuItem = forcedMessageMenuItem.MenuItems.FindByText(EDIMenu.ForcedSendOfOriginalMessageText("G7"));
				AssertNotNull(originalSendMenuItem);
				originalSendMenuItem.PerformClick();
				AssertEquals("1 message", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("Original message despite of current status is awaiting original", Common.Shared.MessageSubTypeCodes.Codes.Original, declaration.ActiveEntryHeaders[0].Messages[0].EM_MessageSubType);
			}
		}

		public void TestLockEntryHeaderWhenSendingMessage()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "XLICEN", Core.Constants.CountryCodes.Canada);
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			using (var menu = new EDIMenuForTesting())
			{
				var entryHeader = declaration.ActiveEntryHeaders[0];
				var entryHeaderMutex = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, entryHeader.PK.ToString());
				AssertEquals(true, entryHeaderMutex.Lock());

				menu.Declaration = declaration;
				var forcedMessageMenuItem = menu.MenuItems.FindByText(EDIMenu.ForcedMessageText);
				AssertNotNull(forcedMessageMenuItem);
				var amendSendMenuItem = forcedMessageMenuItem.MenuItems.FindByText(EDIMenu.ForcedSendOfAmendmentMessageText("G7"));
				AssertNotNull(amendSendMenuItem);
				menu.ExposedNotification.NextAnswer = true;
				amendSendMenuItem.PerformClick();
				AssertEquals("0 message", 0, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertContains("This entry has been locked, as CargoWise Support", menu.ExposedNotification.LastMessage);
				AssertContains("is trying to send the same message for this entry. Please wait until the lock has been released or close and re-open the form then try again.", menu.ExposedNotification.LastMessage);

				entryHeaderMutex.Unlock();
				amendSendMenuItem.PerformClick();
				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
			}
		}

		public void TestConvertToNormalDeclaration()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsAir = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Supplementary;

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				bool formClosed = false;
				form.FormClosed += delegate
				{ formClosed = true; };
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var convertNormalMenuItem = menu.MenuItems.FindByText("Convert to Normal Declaration");
				AssertNotNull("Convert to normal declaration", convertNormalMenuItem);

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				declaration2.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
				var invoice = declaration.LVXInvoiceHeader;
				invoice.CA_CarrierCode = "4646";
				invoice.CA_OtherReference = "11\r\n22\r\n33";
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.CA_AuthorityNumber = "123456";
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice, declaration2);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				convertNormalMenuItem.PerformClick();
				AssertEquals("JE_MessageType", JobMessageTypeList.Codes.LVSForConsolidation, declaration.JE_MessageType);
				AssertEquals("JE_MessageSubType", B3EntryTypeList.Codes.Supplementary, declaration.JE_MessageSubType);
				AssertEquals("Please detach from the consolidated LVS before converting to a normal declaration.", UnitTestUserNotification.Instance.LastMessage.Text);

				LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice, declaration2);
				convertNormalMenuItem.PerformClick();
				AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				AssertEquals("JE_MessageSubType", B3EntryTypeList.Codes.Confirming, declaration.JE_MessageSubType);
				AssertEquals("JE_TransportMode", Constants.TransportModes.Air, declaration.JE_TransportMode);
				AssertEquals("JE_CarrierCode", "4646", declaration.JE_CarrierCode);
				AssertEquals("JE_OwnerRef", "11,22,33", declaration.JE_OwnerRef);
				Assert("Form should be closed", formClosed);
				var normalDeclarationForm = (JobDeclarationForm)menu.LastFormShownForTest;
				AssertNotNull("Should show a new declaration form for converted declaration", normalDeclarationForm);
				AssertType<CustomsBrokerageUserControl>("Should show a normal declaration form for converted declaration", normalDeclarationForm.CustomsBrokerageUserControl);
				AssertEquals(declaration, normalDeclarationForm.DataSource);
				normalDeclarationForm.Close();
				normalDeclarationForm.Dispose();
			}
		}

		public void TestConvertToNormalDeclaration_WithErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.LVXInvoiceHeader.JZ_RX_NKInvoice_Currency = "XXX";

			bool formClosed = false;
			using (var form = new JobDeclarationForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Show();
				form.FormClosed += delegate
				{ formClosed = true; };
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertNormalMenuItem = menu.MenuItems.FindByText("Convert to Normal Declaration");
				convertNormalMenuItem.PerformClick();

				AssertEquals("Errors Dialog should be popped up", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should not be changed to Normal Declaration", JobMessageTypeList.Codes.LVSForConsolidation, declaration.JE_MessageType);
				Assert("Declaration should not be saved", declaration.HasChanges);
				Assert("Form should not be closed", !formClosed);
				AssertNull("Should not show the new declaration form", menu.LastFormShownForTest);
			}
		}

		public void TestConvertToNormalDeclaration_TransactionNumber()
		{
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "12345678";
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("PreCondition: AccountSecurityCode", ZString.Empty, declaration.TransactionNumber.AccountSecurityCode);

			using (CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.FormClosed += delegate
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				};
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertNormalMenuItem = menu.MenuItems.FindByText("Convert to Normal Declaration");
				convertNormalMenuItem.PerformClick();

				AssertEquals("Should be changed to Normal Declaration", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				Assert("Should not ask user", UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("AccountSecurityCode", "12345", declaration.TransactionNumber.AccountSecurityCode);

				((ZForm)menu.LastFormShownForTest).Close();
				menu.LastFormShownForTest.Dispose();
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.JE_OH_Importer = importer.PK;

			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("99999"))
			using (CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "99999");

				form.FormClosed += delegate
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				};
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var convertNormalMenuItem = menu.MenuItems.FindByText("Convert to Normal Declaration");
				convertNormalMenuItem.PerformClick();

				AssertEquals("Should be changed to Normal Declaration", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				AssertEquals("Should ask user", "Question Do you want to use the Importer's Account Security Number instead of yours?", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("AccountSecurityCode", "99999", declaration.TransactionNumber.AccountSecurityCode);

				((ZForm)menu.LastFormShownForTest).Close();
				menu.LastFormShownForTest.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestMergeInvoiceLines()
		{
			var mockDeclaration = Factory.NewMoq<JobDeclaration>();
			var initiator = new SendsMessagesToCustomsShutterUpperer(false);
			mockDeclaration.Object.MessageInitiator = initiator;
			mockDeclaration.Protected().Setup<bool>("DoMergeCore", initiator).Returns(true);

			mockDeclaration.Object.Invoices.AddNew().JobComInvoiceLines.AddNew();

			using (var menu = new EDIMenu())
			{
				menu.Declaration = mockDeclaration.Object;
				menu.GenerateEntriesMenuItem.PerformClick();
				mockDeclaration.VerifyAll();
			}
		}

		[TestDate(2012, 7, 4)]
		public void TestCanSendB3Message()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = universalHelper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2010, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();
			CombineAssertions(() =>
			{
				using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
				{
					TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

					var data = (RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
					var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
					customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					var factory = JobComInvoiceLineTestHelper.PopulateDutiesAndTaxesRefFilesReturningFactory();
					CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
					ZString messageText;
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_InvoiceAmount = 1000m;
					var line = invoice.JobComInvoiceLines.AddNew();
					JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Constants.Weight.Kilograms);
					line.CA_TreatmentCode = TariffTreatmentCodes.Codes.Mexico;
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.DoMerge();
					using (declaration.SuspendMarkApportionmentDirty())
					{
						line.DutiesAndTaxes.DeleteAll();
					}
					Factory.Save();
					var entryHeader = declaration.B3EntryHeader;
					var manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
					manager.SetNextAnswer(false);
					Assert("1. CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("1. LastMessage.Text", @"This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?", manager.LastMessage);
					Factory.Save();
					manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader), true);
					manager.SetNextAnswer(false);
					Assert("2. CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("2. LastMessage.Text", @"This job needs Duty and Tax recalculated. Would you like the system to recalculate now using the most recent data?", manager.LastMessage);
					entryHeader = declaration.ReleaseEntryHeader;
					entryHeader.CH_EntryStatus = Enterprise.Customs.CA.Business.EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
					Factory.Save();
					manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(entryHeader));
					manager.SetNextAnswer(true);
					Assert("3. CanSendThisMessage", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
					AssertEquals("3. LastMessage.Text", @"Duty and Tax has been recalculated. These new values will be sent in the B3 message.", manager.LastMessage);
				}
			});
			ErrorReporter.Clear();
		}

		public void TestValidateEntryLineQuantities()
		{
			AssertValidateEntryLineQuantities(MessageTypeList.Codes.B3CUSDEC, true);
		}

		public void TestValidateEntryLineQuantitiesNotApplyForCADEntry()
		{
			AssertValidateEntryLineQuantities(MessageTypeList.Codes.CommercialAccountingDeclaration, false);
		}

		void AssertValidateEntryLineQuantities(ZString entryType, bool shouldApply)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_MergeBy = B3MergeByList.Codes.NotMerge;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = entryType;
			for (var i = 0; i <= 4000; i++)
			{
				entry.AllEntryLines.AddNew();
			}
			var validator = new CanSendDeclarationChecker(declaration);
			validator.CheckEntryLineQuantities();
			if (shouldApply)
			{
				AssertHasRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.");
				declaration.CA_MergeBy = B3MergeByList.Codes.ProductNumber;
				validator = new CanSendDeclarationChecker(declaration);
				validator.CheckEntryLineQuantities();
				AssertHasRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.");
			}
			else
			{
				AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please review the “B3 Merge By” option on the MISC tab and consider merging multiple invoice lines into one B3 line.");
				AssertNoRowMessageErrorContaining(entry, "The number of entry lines exceeds 4000, system may not be able to generate a B3 document for this job. Please reduce the number of entry lines and create an additional entry if necessary.");
			}
		}

		public void TestRequiredEntryExistOnCustomsEntryHeaderShouldNotThrowExceptionButReportError()
		{
			var declaration = Factory.New<JobDeclaration>();
			var validator = new CanSendDeclarationChecker(declaration);
			var valid = validator.RequiredEntryExist("ABC");
			AssertEquals(false, valid);
			Assert(validator.LastErrorMessage.StartsWith("Required entry"));
			if (ErrorReporter.TotalErrorCount == 1)
			{
				if (ErrorReporter.LastMessageReported.StartsWith("RequiredEntry"))
				{
					ErrorReporter.Clear();
				}
			}
		}

		[TestDate(2016, 2, 2)]
		public void TestAIRSValidation()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("99999"))
			using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "99999");

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;

				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();
				var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine1.FillWithValidTestData();
				var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine2.FillWithValidTestData();
				invoiceLine2.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, Enterprise.Customs.CA.Business.PGACodes.Codes.CFIA);
				invoiceLine2.CA_OGDStatus = AVSStatusList.Codes.Blank;
				var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				invoiceLine3.FillWithValidTestData();
				invoiceLine3.JI_Tariff = CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, Enterprise.Customs.CA.Business.PGACodes.Codes.CFIA);
				invoiceLine3.CA_OGDStatus = AVSStatusList.Codes.Blank;
				Factory.Save();

				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenuForTesting())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;
					form.Show();
					Application.DoEvents();

					var validationAllMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - All Lines");
					var validationNOTMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - Lines Not Validated");

					validationNOTMenuItem.PerformClick();
					AssertEquals("There's no invoice line for AIRS Validation", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					validationAllMenuItem.PerformClick();

					AssertEquals("invoiceLine1.CA_OGDStatus", ZString.Empty, invoiceLine1.CA_OGDStatus);
					AssertEquals("invoiceLine2.CA_OGDStatus", "OKA", invoiceLine2.CA_OGDStatus);
					AssertEquals("invoiceLine3.CA_OGDStatus", "OKA", invoiceLine3.CA_OGDStatus);
					AssertEquals("invoiceLine1.Note", 0, invoiceLine1.Notes.FindByDescription("AIRS Validation Results").Length);
					AssertEquals("invoiceLine2.Note", "2016-02-02T00:00:00|No errors reported by CFIA", invoiceLine2.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
					AssertEquals("invoiceLine3.Note", "2016-02-02T00:00:00|No errors reported by CFIA", invoiceLine3.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);

					Assert("declaration.AVSEventRequired", !declaration.AVSEventRequired);
					AssertEquals("declaration.CA_OGDStatus", "OKA", declaration.CA_OGDStatus);
					Assert("declaration should be saved", !declaration.HasChanges);

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		[TestDate(2017, 10, 30)]
		public void TestAIRSValidationForIID()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("99999"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "99999");

				using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TEST"))
				{
					CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, "CFIA", "0101210000");
					var declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

					var invoice = declaration.Invoices.AddNew();
					invoice.FillWithValidTestData();
					var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
					invoiceLine1.FillWithValidTestData();
					var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
					invoiceLine2.FillWithValidTestData();
					invoiceLine2.CA_CFIAInd = "Y";
					invoiceLine2.JI_Tariff = "0101210000";
					invoiceLine2.CA_OGDStatus = AVSStatusList.Codes.Blank;
					var invoiceLine3 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
					invoiceLine3.FillWithValidTestData();
					invoiceLine3.CA_CFIAInd = "Y";
					invoiceLine3.JI_Tariff = "0101210000";
					invoiceLine3.CA_OGDStatus = AVSStatusList.Codes.Blank;
					Factory.Save();

					using (var form = new ZForm(declaration))
					using (var menu = new EDIMenuForTesting())
					{
						form.Menu.MenuItems.Add(menu);
						menu.Declaration = declaration;
						form.Show();
						Application.DoEvents();

						var validationAllMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - All Lines");
						var validationNOTMenuItem = menu.MenuItems.FindByText("Messages").MenuItems.FindByText("AIRS Validation - Lines Not Validated");

						validationNOTMenuItem.PerformClick();
						AssertEquals("There's no invoice line for AIRS Validation", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						validationAllMenuItem.PerformClick();

						AssertEquals("invoiceLine1.CA_OGDStatus", ZString.Empty, invoiceLine1.CA_OGDStatus);
						AssertEquals("invoiceLine2.CA_OGDStatus", "OKA", invoiceLine2.CA_OGDStatus);
						AssertEquals("invoiceLine3.CA_OGDStatus", "OKA", invoiceLine3.CA_OGDStatus);
						AssertEquals("invoiceLine1.Note", 0, invoiceLine1.Notes.FindByDescription("AIRS Validation Results").Length);
						AssertEquals("invoiceLine2.Note", "2017-10-30T00:00:00|No errors reported by CFIA", invoiceLine2.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);
						AssertEquals("invoiceLine3.Note", "2017-10-30T00:00:00|No errors reported by CFIA", invoiceLine3.Notes.FindByDescription("AIRS Validation Results")[0].ST_NoteText);

						Assert("declaration.AVSEventRequired", !declaration.AVSEventRequired);
						AssertEquals("declaration.CA_OGDStatus", "OKA", declaration.CA_OGDStatus);
						Assert("declaration should be saved", !declaration.HasChanges);

						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}
				}
			}
		}

		public void TestSendB3AfterREL()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var relEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				relEntryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var b3EntryHeader = declaration.CustomsEntryHeaders.AddNew();
				b3EntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				Factory.Save();

				Assert(declaration.JE_EntrySubmittedDate.IsEmpty);
				Assert(relEntryHeader.CH_EntrySubmittedDate.IsEmpty);
				Assert(relEntryHeader.CH_EntrySubmittedDate.IsEmpty);

				var relManager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(relEntryHeader));
				relManager.OverrideCanSendThisMessage = true;
				relManager.SendMessage(MessageSubTypes.Create, false);

				Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);
				Assert(!relEntryHeader.CH_EntrySubmittedDate.IsEmpty);

				var beforeB3SubmittedDate = new ZDateTime(declaration.JE_EntrySubmittedDate.Ticks);  // to be sure its not a reference

				var b3Manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(b3EntryHeader));
				b3Manager.OverrideCanSendThisMessage = true;
				b3Manager.SendMessage(MessageSubTypes.Create, false);

				Assert(!declaration.JE_EntrySubmittedDate.IsEmpty && !(declaration.JE_EntrySubmittedDate > beforeB3SubmittedDate));
				Assert(!b3EntryHeader.CH_EntrySubmittedDate.IsEmpty);
			}
		}

		public void TestSendRELAfterB3()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX"))
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("98765"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "98765");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var relEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				relEntryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				var b3EntryHeader = declaration.CustomsEntryHeaders.AddNew();
				b3EntryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
				Factory.Save();

				Assert(declaration.JE_EntrySubmittedDate.IsEmpty);
				Assert(b3EntryHeader.CH_EntrySubmittedDate.IsEmpty);
				Assert(relEntryHeader.CH_EntrySubmittedDate.IsEmpty);

				var b3Manager = new B3ImportMessageManagerForTesting(new B3ImportMessageWrapper(b3EntryHeader));
				b3Manager.OverrideCanSendThisMessage = true;
				b3Manager.SendMessage(MessageSubTypes.Create, false);

				Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);
				Assert(!b3EntryHeader.CH_EntrySubmittedDate.IsEmpty);

				var beforeRELSubmittedDate = new ZDateTime(declaration.JE_EntrySubmittedDate.Ticks);  // to be sure its not a reference

				var relManager = new EDIReleaseImportMessageManagerForTesting(new EDIReleaseImportMessageWrapper(relEntryHeader));
				relManager.OverrideCanSendThisMessage = true;
				relManager.SendMessage(MessageSubTypes.Create, false);

				Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);
				Assert(!(declaration.JE_EntrySubmittedDate > beforeRELSubmittedDate));
				Assert(!relEntryHeader.CH_EntrySubmittedDate.IsEmpty);
			}
		}

		public void TestBondedWarehouseMenuItems()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			using (CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				BondedWarehousingHelperTest.CreateWarehouse(Factory, helper.Warehouse, "WH1");
				BondedWarehousingHelperTest.CreateWarehouse(Factory, helper.Warehouse2, "WH2");
				var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				helper.Warehouse2.OH_RL_NKClosestPort = port.RL_Code;

				#region Inward Update

				var inwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000001", "00000001", B3EntryTypeList.Codes.Warehouse10, helper.Importer, helper.Warehouse);
				BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(inwardDeclaration, helper.Part.OP_PartNum, 10m);
				inwardDeclaration.DoMerge();
				Factory.Save();

				using (var form = new ZForm(inwardDeclaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					Assert("Inventory Management menus should available for Warehouse Entry Type", bondedWarehouseMenuItem.Visible);

					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					updateBondedWarehouseMenuItem.PerformClick();
					AssertContains("Stock Levels have been updated. (WHS Receipt:", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					inwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000002", "00000002", CADEntryTypeList.Codes.Warehouse101, helper.Importer, helper.Warehouse);
					BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(inwardDeclaration, helper.Part.OP_PartNum, 10m);
					inwardDeclaration.DoMerge();
					Factory.Save();

					using (var form = new ZForm(inwardDeclaration))
					using (var menu = new EDIMenu())
					{
						form.Menu.MenuItems.Add(menu);
						menu.Declaration = inwardDeclaration;
						menu.RefreshMenu();
						var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
						Assert("Inventory Management menus should available for Warehouse Entry Type", bondedWarehouseMenuItem.Visible);

						bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
						var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
						AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						updateBondedWarehouseMenuItem.PerformClick();
						AssertContains("Stock Levels have been updated. (WHS Receipt:", UnitTestUserNotification.Instance.LastMessage.Text);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
					}

					inwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000003", "00000003", CADEntryTypeList.Codes.Warehouse102, helper.Importer, helper.Warehouse);
					BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(inwardDeclaration, helper.Part.OP_PartNum, 10m);
					inwardDeclaration.DoMerge();
					Factory.Save();

					using (var form = new ZForm(inwardDeclaration))
					using (var menu = new EDIMenu())
					{
						form.Menu.MenuItems.Add(menu);
						menu.Declaration = inwardDeclaration;
						menu.RefreshMenu();
						var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
						Assert("Inventory Management menus should available for Warehouse Entry Type", bondedWarehouseMenuItem.Visible);

						bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
						var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
						AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						updateBondedWarehouseMenuItem.PerformClick();
						AssertContains("Stock Levels have been updated. (WHS Receipt:", UnitTestUserNotification.Instance.LastMessage.Text);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
					}
				}

				#endregion

				#region Outward Update/Cancel

				var outwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000004", "00000004", B3EntryTypeList.Codes.ExWarehouse20, helper.Importer, helper.Warehouse);
				BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(outwardDeclaration, helper.Part.OP_PartNum, 2m);
				outwardDeclaration.InvoiceLines[0].JI_PreviousEntryNumber = "12345000000012-1";
				outwardDeclaration.DoMerge();
				Factory.Save();

				using (var form = new ZForm(outwardDeclaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = outwardDeclaration;
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					Assert("Inventory Management menus should available for Ex Warehouse Entry Type", bondedWarehouseMenuItem.Visible);

					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					updateBondedWarehouseMenuItem.PerformClick();
					AssertContains("Stock Release has been updated. (WHS Order:", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000034-1", 10m);

					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Cancel Inventory Stock Release");
					AssertEquals(true, cancelBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to cancel
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertContains("Stock Release has been canceled. (WHS Order:", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000034-1", 10m);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
				{
					outwardDeclaration = BondedWarehousingHelperTest.CreateWHSDeclaration(Factory, "B000000005", "00000005", CADEntryTypeList.Codes.ExWarehouse201, helper.Importer, helper.Warehouse);
					BondedWarehousingHelperTest.SetupInvoiceLinesForWHS(outwardDeclaration, helper.Part.OP_PartNum, 2m);
					outwardDeclaration.InvoiceLines[0].JI_PreviousEntryNumber = "12345000000012-1";
					outwardDeclaration.DoMerge();
					Factory.Save();

					using (var form = new ZForm(outwardDeclaration))
					using (var menu = new EDIMenu())
					{
						form.Menu.MenuItems.Add(menu);
						menu.Declaration = outwardDeclaration;
						menu.RefreshMenu();
						var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
						Assert("Inventory Management menus should available for Ex Warehouse Entry Type", bondedWarehouseMenuItem.Visible);

						bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
						var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
						AssertEquals(true, updateBondedWarehouseMenuItem.Visible);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						updateBondedWarehouseMenuItem.PerformClick();
						AssertContains("Stock Release has been updated. (WHS Order:", UnitTestUserNotification.Instance.LastMessage.Text);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 8m);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000034-1", 10m);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000045-1", 0m);

						bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
						var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Cancel Inventory Stock Release");
						AssertEquals(true, cancelBondedWarehouseMenuItem.Visible);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to cancel
						cancelBondedWarehouseMenuItem.PerformClick();
						AssertContains("Stock Release has been canceled. (WHS Order:", UnitTestUserNotification.Instance.LastMessage.Text);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000034-1", 10m);
						WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000045-1", 0m);
					}
				}

				#endregion

				#region Inward Cancel

				using (var form = new ZForm(inwardDeclaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");

					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Cancel Inventory");
					AssertEquals(true, cancelBondedWarehouseMenuItem.Visible);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertContains("Stock Levels have been canceled. (WHS Receipt:", UnitTestUserNotification.Instance.LastMessage.Text);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000012-1", 10m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000023-1", 10m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000034-1", 0m);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("12345000000045-1", 0m);
				}

				#endregion

				#region Check Waiting for Response

				inwardDeclaration.B3EntryHeader.CH_Status = "AWO";
				outwardDeclaration.B3EntryHeader.CH_Status = "AWO";
				Factory.Save();

				using (var form = new ZForm(inwardDeclaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					updateBondedWarehouseMenuItem.PerformClick();
					AssertContains("Cannot update Inventory stock levels while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Cancel Inventory");
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertContains("Cannot cancel Inventory stock levels while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var form = new ZForm(outwardDeclaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = outwardDeclaration;
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					updateBondedWarehouseMenuItem.PerformClick();
					AssertContains("Cannot update Inventory stock release while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var cancelBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("&Cancel Inventory Stock Release");
					cancelBondedWarehouseMenuItem.PerformClick();
					AssertContains("Cannot cancel Inventory stock release while waiting for a response.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				inwardDeclaration.B3EntryHeader.Delete();
				Factory.Save();

				using (var form = new ZForm(inwardDeclaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = inwardDeclaration;
					menu.RefreshMenu();
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					bondedWarehouseMenuItem.OnPopup(EventArgs.Empty);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var updateBondedWarehouseMenuItem = bondedWarehouseMenuItem.MenuItems.FindByText("Update Inventory");
					AssertNoExceptionThrown(() => updateBondedWarehouseMenuItem.PerformClick());
				}

				#endregion
			}
		}

		public void TestGetCreditCheckFromJobDeclarationWhenCustomsMenu()
		{
			var errorMessage = @"Delivery of this document is restricted because:
       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this document?
";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_IsForwardRegistered = true;
			shipment.JS_IsCFSRegistered = false;
			shipment.JS_IsBooking = false;

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_JS = shipment.PK;

			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			importer.OH_IsDebtor = true;
			importer.CompanyData.OB_AROnCreditHold = true;
			dec.JE_OH_Importer = importer.PK;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CH_JE = dec.PK;

			importer.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			var b3Command = Factory.New<DocumentCommand>();
			b3Command.SU_MenuType = "DOC";
			b3Command.SU_BusinessContext = "Customs";
			b3Command.SU_MenuName = ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData;
			b3Command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			b3Command.Parent = dec;
			Factory.Save();

			var dr = new DocumentRunner();
			dr.Run(b3Command);
			AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestB2MoveAccountedToClaimMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				var menuItem = menu.MenuItems.FindByText("Move Accounted Data to Claim Data");
				Assert(!menuItem.Visible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				var menuItem = menu.MenuItems.FindByText("Move Accounted Data to Claim Data");
				Assert(menuItem.Visible);

				var asAccountedInvoice1 = declaration.B2AsAccountedForInvoices.AddNew();
				asAccountedInvoice1.JZ_InvoiceNumber = "INV1";
				var asAccountedInvoice2 = declaration.B2AsAccountedForInvoices.AddNew();
				asAccountedInvoice2.JZ_InvoiceNumber = "INV2";
				var asAccountedInvoiceLine11 = asAccountedInvoice1.AsAccountForFilteredInvoiceLines.AddNew();
				asAccountedInvoiceLine11.CA_OriginalLineNo = "1";
				var asAccountedInvoiceLine12 = asAccountedInvoice1.AsAccountForFilteredInvoiceLines.AddNew();
				asAccountedInvoiceLine12.CA_OriginalLineNo = "2";
				var asAccountedInvoiceLine21 = asAccountedInvoice2.AsAccountForFilteredInvoiceLines.AddNew();
				asAccountedInvoiceLine21.CA_OriginalLineNo = "1";
				var asAccountDuty = asAccountedInvoiceLine21.DutiesAndTaxes.AddNew();
				asAccountDuty.C1_Override = true;
				asAccountDuty.C1_Amount = 100m;

				var asClaimedInvoice = declaration.B2AsClaimedForInvoices.AddNew();
				AssertEquals(3, declaration.B2AsClaimedForInvoices.Count);

				var asClaimedInvoice1 = asAccountedInvoice1.CorrespondingAsClaimedForInvoice;
				asClaimedInvoice1.AsClaimForFilteredInvoiceLines.AddNew();
				AssertEquals(3, asClaimedInvoice1.AsClaimForFilteredInvoiceLines.Count);

				var asClaimedInvoiceLine21 = asAccountedInvoiceLine21.CorrespondingAsClaimedForInvoiceLine;
				var asClaimedDuty = asClaimedInvoiceLine21.DutiesAndTaxes[0];
				asClaimedDuty.C1_Amount = 200m;
				asClaimedInvoiceLine21.DutiesAndTaxes.AddNew();
				AssertEquals(2, asClaimedInvoiceLine21.DutiesAndTaxes.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
				menuItem.PerformClick();
				AssertEquals("Data already exists in the As Claimed For section. Do you want to overwrite this with the As Accounted For data?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(2, declaration.B2AsClaimedForInvoices.Count);
				AssertEquals(2, declaration.B2AsClaimedForInvoices[0].AsClaimForFilteredInvoiceLines.Count);
				AssertEquals(1, declaration.B2AsClaimedForInvoices[1].AsClaimForFilteredInvoiceLines.Count);
				AssertEquals(1, declaration.B2AsClaimedForInvoices[1].AsClaimForFilteredInvoiceLines[0].DutiesAndTaxes.Count);
				AssertEquals(100m, declaration.B2AsClaimedForInvoices[1].AsClaimForFilteredInvoiceLines[0].DutiesAndTaxes[0].C1_Amount);
			}
		}

		public void TestSendMessageProgressForm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX"))
			using (CACustomsDataRegistry.Instance.SendB3CADProgressFormThreshold.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 100))
			using (var form = new JobDeclarationForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				var menuItem = GetMenuItem(menu, "Send Entry Message");

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CustomsQuantity = 100m;
				invoiceLine.JI_CustomsUnitQty = "KGM";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.RefreshMenu();
				menuItem.PerformClick();
				var progressForm = ZFormModaliser.LastFormShownForTest;
				AssertNull(progressForm);

				for (var i = 0; i < 100; i++)
				{
					var line = invoice.InvoiceLines.AddNew();
					line.JI_CustomsQuantity = i + 100m;
					line.JI_CustomsUnitQty = "KGM";
				}
				declaration.DoMerge();
				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				menu.RefreshMenu();
				menuItem.PerformClick();
				progressForm = ZFormModaliser.LastFormShownForTest;
				AssertNotNull(progressForm);
				AssertType<ProgressForm>(progressForm);
			}
		}

#if !WINZOR
		public void TestSendCADMessageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var messagesMenuItem = GetMenuItem(menu, "Messages");
				AssertEquals(true, MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(menu, "Send Entry Message")));
				AssertEquals(false, MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(menu, "Send CAD Message")));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var messagesMenuItem = GetMenuItem(menu, "Messages");
				AssertEquals(false, MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(menu, "Send Entry Message")));
				AssertEquals(true, MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(menu, "Send CAD Message")));
			}
		}

		public void TestInterfaceMenuItemsVisibility()
		{
			using (var menu = new EDIMenu())
			{
				var declaration = Factory.New<JobDeclaration>();
				menu.Declaration = declaration;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Submit")));
				AssertEquals(false, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Manual Submission")));
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Manual Release")));
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Manual Cancel")));
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Messages")));

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Submit")));
				AssertEquals(true, menu.GenerateEntriesMenuItem.Visible);
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Manual Submission")));
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Manual Release")));
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Manual Cancel")));
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Messages")));

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Submit")));
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Send G7 Export Declaration")));
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Withdraw (Cancel) G7 Export Declaration")));
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Forced Messages")));

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();
				AssertEquals(false, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Submit")));
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Send G7 Export Declaration")));
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Withdraw (Cancel) G7 Export Declaration")));
				AssertEquals(true, MenuItemsContainsAndVisible(menu.MenuItems, GetMenuItem(menu, "Forced Messages")));
			}
		}

		public void TestMenuItemsVisibility()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			using (var testMenu = new EDIMenu())
			{
				#region Export

				//DLM
				CACustomsDataRegistry.Instance.ExportDeclarationActive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.JE_TransportMode = Constants.TransportModes.Air;
				CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				testMenu.Declaration = dec;
				testMenu.RefreshMenu();

				AssertMainMenuItemsVisibility(testMenu, new[] { true, false, false, false, false, false });

				CACustomsDataRegistry.Instance.ExportDeclarationActive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				testMenu.RefreshMenu();

				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, false, false });

				//G7 Export
				CACustomsDataRegistry.Instance.SendG7ExportMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CACustomsDataRegistry.Instance.ExportDeclarationActive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				testMenu.RefreshMenu();

				AssertMainMenuItemsVisibility(testMenu, new[] { false, true, true, true, false, false });

				CACustomsDataRegistry.Instance.ExportDeclarationActive.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				testMenu.RefreshMenu();

				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, false, false });
				#endregion

				#region Import

				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				testMenu.RefreshMenu();
				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, true, true });
				AssertImportMenuItemsVisibility(testMenu, new[] { true, true, true, true, true });

				dec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
				testMenu.RefreshMenu();
				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, true, true });
				AssertImportMenuItemsVisibility(testMenu, new[] { false, false, true, false, false });

				dec.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
				testMenu.RefreshMenu();
				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, true, false });

				dec.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
				testMenu.RefreshMenu();
				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, true, false });
				AssertImportMenuItemsVisibility(testMenu, new[] { true, true, true, true, true });

				dec.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
				testMenu.RefreshMenu();
				AssertMainMenuItemsVisibility(testMenu, new[] { false, false, false, false, false, false });
				AssertImportMenuItemsVisibility(testMenu, new[] { true, true, true, true, true });

				#endregion

			}
		}

		void AssertMainMenuItemsVisibility(EDIMenu testMenu, bool[] expected)
		{
			//DLM
			AssertEquals("SendToDLMMenuItem", expected[0], MenuItemsContainsAndVisible(testMenu.MenuItems, GetMenuItem(testMenu, "Send to DLM")));

			//G7 Export
			AssertEquals("SendG7ExportCreateMenuItem", expected[1], MenuItemsContainsAndVisible(testMenu.MenuItems, GetMenuItem(testMenu, "Send G7 Export Declaration")));
			AssertEquals("CancelG7ExportCreateMenuItem", expected[2], MenuItemsContainsAndVisible(testMenu.MenuItems, GetMenuItem(testMenu, "Withdraw (Cancel) G7 Export Declaration")));
			AssertEquals("ResetG7ExportDeclaration", expected[3], MenuItemsContainsAndVisible(testMenu.MenuItems, GetMenuItem(testMenu, "Reset G7 Export Declaration")));

			//Import
			AssertEquals("ResetImportDeclaration", expected[4], MenuItemsContainsAndVisible(testMenu.MenuItems, GetMenuItem(testMenu, "Reset Import Declaration")));
			AssertEquals("MessagesMenuItem", expected[5], MenuItemsContainsAndVisible(testMenu.MenuItems, GetMenuItem(testMenu, "Messages")));
		}

		void AssertImportMenuItemsVisibility(EDIMenu testMenu, bool[] expected)
		{
			var messagesMenuItem = GetMenuItem(testMenu, "Messages");
			AssertEquals("SendQueryReleaseStatusMessage", expected[0], MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Status Query")));

			var dec = testMenu.Declaration;
			var forcedMneuItems = messagesMenuItem.MenuItems.FindByText("Forced Messages", true);
			if (expected[1])
			{
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				testMenu.RefreshMenu();
				Assert("SendEDIReleaseImportMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Message")));
				Assert("SendEDIReleaseImportCancelMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Cancel Message")));
				Assert("Forced Messages", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, forcedMneuItems));
				Assert("SendEDIReleaseImportOriginalMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of Release Original Message")));
				Assert("SendEDIReleaseImportAmendMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of Release Amendment Message")));
				Assert("AIRSValidationAllMenuItem", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "AIRS Validation - All Lines")));
				Assert("AIRSValidationNOTMenuItem", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "AIRS Validation - Lines Not Validated")));
				Assert("SendIIDMessage", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send IID Message")));
				Assert("SendIIDCancelMessage", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send IID Cancel Message")));
				Assert("SendIIDOriginalMessage", MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Original Message")));
				Assert("SendIIDChangeMessage", MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Change Message")));
				Assert("SendIIDAmendMessage", MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Amendment Message")));

				dec.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
				testMenu.RefreshMenu();
				Assert("SendEDIReleaseImportMessage", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Message")));
				Assert("SendEDIReleaseImportCancelMessage", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Cancel Message")));
				Assert("Forced Messages", MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, forcedMneuItems));
				Assert("SendEDIReleaseImportOriginalMessage", MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of Release Original Message")));
				Assert("SendEDIReleaseImportAmendMessage", MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of Release Amendment Message")));
				Assert("SendIIDMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send IID Message")));
				Assert("SendIIDCancelMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send IID Cancel Message")));
				Assert("SendIIDOriginalMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Original Message")));
				Assert("SendIIDChangeMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Change Message")));
				Assert("SendIIDAmendMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Amendment Message")));
			}
			else
			{
				Assert("SendEDIReleaseImportMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Message")));
				Assert("SendEDIReleaseImportCancelMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Release Cancel Message")));
				Assert("Forced Messages", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, forcedMneuItems));
				Assert("SendEDIReleaseImportOriginalMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of Release Original Message")));
				Assert("SendEDIReleaseImportAmendMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of Release Amendment Message")));
				Assert("SendIIDMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send IID Message")));
				Assert("SendIIDCancelMessage", !MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send IID Cancel Message")));
				Assert("SendIIDOriginalMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Original Message")));
				Assert("SendIIDChangeMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Change Message")));
				Assert("SendIIDAmendMessage", !MenuItemsContainsAndVisible(forcedMneuItems.MenuItems, GetMenuItem(testMenu, "Force Send of IID Amendment Message")));
			}

			//B3
			AssertEquals("SendB3MessageMenuItem", expected[2], MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "Send Entry Message")));

			// AIRSValidation
			dec.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			testMenu.RefreshMenu();
			AssertEquals("AIRSValidationAllMenuItem", expected[3], MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "AIRS Validation - All Lines")));
			AssertEquals("AIRSValidationNOTMenuItem", expected[4], MenuItemsContainsAndVisible(messagesMenuItem.MenuItems, GetMenuItem(testMenu, "AIRS Validation - Lines Not Validated")));
		}
#endif

		void AssertSupervisorOverrides(string menuName, Type formType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testB3CHeader = declaration.ActiveEntryHeaders.AddNew();
			testB3CHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var testRELHeader = declaration.ActiveEntryHeaders.AddNew();
			testRELHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			Env.Security.CAManualReleaseCancel.IsAllowed = true;
			Factory.Save();

			bool oldAllowed = Env.Security.CAManualCancelRelease.IsAllowed;
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			try
			{
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				Env.Security.CAManualCancelRelease.IsAllowed = false;
				GlbStaff.CurrentUser.GS_IsController = false;

				var staff = Factory.New<GlbStaff>();
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_IsActive = true;

				var se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.CAManualCancelRelease.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = staff.PK;
				Factory.Save();

				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;

					var menuItem = menu.MenuItems.FindByText(menuName);
					menuItem.PerformClick();
					var lastDialogForm = ZFormModaliser.LastFormShownDialogForTest;
					Assert("SupervisorOverridesForm", lastDialogForm is CustomsGUI.SupervisorOverridesForm);
				}

				var newFactory = new BusinessObjectFactory();
				declaration = newFactory.Load<JobDeclaration>(declaration.PK);
				se = newFactory.Load<GlbSecurity>(se.PK);
				se.GU_SecurityItemIsAllowed = false;
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;

					var menuItem = menu.MenuItems.FindByText(menuName);
					menuItem.PerformClick();
					var lastDialogForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertType("FormType", formType, lastDialogForm);
				}
			}
			finally
			{
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
				Env.Security.CAManualCancelRelease.IsAllowed = oldAllowed;
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
			}
		}

		void AssertSupervisorOverrides201(string menuName, Type formType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testB3CHeader = declaration.ActiveEntryHeaders.AddNew();
			testB3CHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var testRELHeader = declaration.ActiveEntryHeaders.AddNew();
			testRELHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			Env.Security.CAManualReleaseCancel.IsAllowed = true;
			Factory.Save();

			bool oldAllowed = Env.Security.CAManualCancelRelease.IsAllowed;
			bool oldIsController = GlbStaff.CurrentUser.GS_IsController;
			bool oldSupervisorOverridens = Env.Security.SupervisorOverrides.IsAllowed;
			bool oldAllowMessageErrors = Env.Security.AllowMessageErrors.IsAllowed;
			try
			{
				Env.Security.SupervisorOverrides.IsAllowed = false;
				Env.Security.AllowMessageErrors.IsAllowed = false;
				Env.Security.CAManualCancelRelease.IsAllowed = false;
				GlbStaff.CurrentUser.GS_IsController = false;

				var staff = Factory.New<GlbStaff>();
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_IsActive = true;

				var se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = Env.Security.CAManualCancelRelease.Code;
				se.GU_SecurityItemIsAllowed = true;
				se.GU_GS = staff.PK;
				Factory.Save();

				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;

					var menuItem = menu.MenuItems.FindByText(menuName);
					menuItem.PerformClick();
					var lastDialogForm = ZFormModaliser.LastFormShownDialogForTest;
					Assert("SupervisorOverridesForm", lastDialogForm is CustomsGUI.SupervisorOverridesForm);
				}

				var newFactory = new BusinessObjectFactory();
				declaration = newFactory.Load<JobDeclaration>(declaration.PK);
				se = newFactory.Load<GlbSecurity>(se.PK);
				se.GU_SecurityItemIsAllowed = false;
				using (var form = new ZForm(declaration))
				using (var menu = new EDIMenu())
				{
					form.Menu.MenuItems.Add(menu);
					menu.Declaration = declaration;

					var menuItem = menu.MenuItems.FindByText(menuName);
					menuItem.PerformClick();
					var lastDialogForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertType("FormType", formType, lastDialogForm);
				}
			}
			finally
			{
				Env.Security.SupervisorOverrides.IsAllowed = oldSupervisorOverridens;
				Env.Security.AllowMessageErrors.IsAllowed = oldAllowMessageErrors;
				Env.Security.CAManualCancelRelease.IsAllowed = oldAllowed;
				GlbStaff.CurrentUser.GS_IsController = oldIsController;
			}
		}

		MenuItem GetMenuItem(EDIMenu menu, string text) => menu.MenuItems.FindByText(text, true);

		sealed class EDIMenuForTesting : EDIMenu
		{
			public EDIMenuForTesting()
			{
				fNotification = new TestUserNotification();
			}

			readonly IUserNotification fNotification;
			internal EDIMenuForTesting(bool withB3ScheduleSupport)
			{
				fNotification = withB3ScheduleSupport ? new TestUserNotificationWithB3ScheduleSupport() : new TestUserNotification();
			}

			internal TestUserNotification ExposedNotification => (TestUserNotification)Notification;

			protected override IUserNotification Notification => fNotification;

			protected override AIRSValidationRunner GetAIRSValidationRunner(JobDeclaration declaration)
			{
				var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
				return new AIRSValidationRunner(declaration, service);
			}
		}
	}
}
