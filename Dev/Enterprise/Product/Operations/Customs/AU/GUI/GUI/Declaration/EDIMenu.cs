using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Status = Enterprise.Customs.AU.Declaration.Business.EXDOCComplianceStatusCodesForCusEntryNumber.Descriptions;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class EDIMenu : Customs.GUI.EDIMenu, IConsolidatedDeclarationMenuBuilder
	{
		protected internal EDIMenu()
		{
		}

		protected delegate EDIMenu ConstructorDelegate();
		protected static readonly Overridable<ConstructorDelegate> OverridableNewDelegate = new Overridable<ConstructorDelegate>();

		public static EDIMenu New()
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden == null ? new EDIMenu() : overridden();
		}

		#region Declaration

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		#endregion

		#region MessageController

		SendsMessagesToCustomsGUI MessageController
		{
			get
			{
				if (fMessageController == null)
				{
					fMessageController = (SendsMessagesToCustomsGUI)GetNewMessageInitiator();
				}
				return fMessageController;
			}
		}
		SendsMessagesToCustomsGUI fMessageController;

		protected override void SetMessageInitiatorIfRequired(Customs.Business.BaseJobDeclaration newValue)
		{
			if (newValue is JobDeclaration auDeclaration && !auDeclaration.HasMessageInitiator)
			{
				auDeclaration.MessageInitiator = (SendsMessagesToCustomsGUI)GetNewMessageInitiator();
			}
		}

		protected override Customs.GUI.SendsMessagesToCustomsGUI GetNewMessageInitiator()
		{
			return new SendsMessagesToCustomsGUI();
		}

		#endregion

		protected override ConsolidatedEntryMenuProvider GetConsolidatedEntryMenuProvider() => new AUConsolidatedEntryMenuProvider(Form);

		internal List<MenuItem> EdificeMenuItem;
		internal List<MenuItem> CMRMenuItem;
		internal List<MenuItem> ExportDeclarationMenuItem;
		internal List<MenuItem> QuarantineMenuItem;
		internal List<MenuItem> DrawbackMenuItem;
		internal List<MenuItem> ReplaceReissueMenuItem;
		internal List<MenuItem> RefreshREXDataMenuItem;

		internal MenuItem SubmitOriginalExportDecMenuItem;
		internal MenuItem SubmitReplacementExportDecMenuItem;
		internal MenuItem OtherCustomsMessagesMenuItem;
		internal MenuItem WarrelSendOriginalMenuItem;
		internal MenuItem WarrelSendReplacementMenuItem;
		internal MenuItem WithdrawWarrelMenuItem;
		internal MenuItem WarretSendOriginalMenuItem;
		internal MenuItem WarretSendReplacementMenuItem;
		internal MenuItem DeprecSendOriginalMenuItem;
		internal MenuItem DeprecSendReplacementMenuItem;
		internal MenuItem WithdrawDeprecMenuItem;
		internal MenuItem DeprelSendOriginalMenuItem;
		internal MenuItem DeprelSendRepacementMenuItem;
		internal MenuItem WithdrawDeprelMenuItem;
		internal MenuItem ChangeStatusToHoldAwaitingMenuItem;
		internal MenuItem EdificeDividerItem1;
		internal MenuItem ChangeStatusToDeclarationWorkCompleteMenuItem;
		internal MenuItem ResetExportDeclarationMenuItem;
		internal MenuItem ResetExportDeclarationAndLinesMenuItem;

		internal MenuItem CMRSetStatusToHoldAwaitingMenuItem;
		internal MenuItem CMRClearStatusToHoldAwaitingMenuItem;

		internal MenuItem CMRSendPreLodgeMenuItem;
		internal MenuItem CMRSendLodgeWithoutPayMenuItem;
		internal MenuItem CMRSetStatusToDeclarationWorkCompleteMenuItem;
		internal MenuItem CMRClearStatusToDeclarationWorkCompleteMenuItem;

		internal MenuItem CMRDoCPForWithdrawalMenuItem;
		internal MenuItem CMRSendLodgeWithPayMenuItem;
		internal MenuItem CMRSendAmendmentMenuItem;
		internal MenuItem ImpMessagingHelpMenuItem;
		internal MenuItem ExpMessagingHelpMenuItem;
		internal MenuItem CMRRegenerateCPDecQuestionsMenuItem;
		internal MenuItem CMRLogAuthorityToPayGivenMenuItem;

		internal MenuItem CreateContingencyMenuItem;

		internal MenuItem SendPaymentMenuItem;
		internal MenuItem sendWithdrawalMenuItem;
		internal MenuItem throwAwayMergedLinesMenuItem;
		internal MenuItem doCommunityProtectionDeclarationMenuItem;
		internal MenuItem dequeueLodgementOrPaymentMessageMenuItem;

		internal MenuItem submitRFPTransferMenuItem;
		internal MenuItem submitRFPEnquiryMenuItem;
		internal MenuItem submitRFPAcceptMenuItem;
		internal MenuItem submitRFPDeclineMenuItem;
		internal MenuItem submitRFPAmendmentMenuItem;
		internal MenuItem submitRFPWithdrawalMenuItem;
		internal MenuItem submitRFPOrderMenuItem;
		internal MenuItem submitRFPLodgeMenuItem;

		internal MenuItem submitREXForwardMenuItem;
		internal MenuItem submitREXTransferMenuItem;
		internal MenuItem submitREXCancellationMenuItem;
		internal MenuItem submitREXRequestAmendmentMenuItem;

		internal MenuItem replaceReissueMenuSeperatorItem;
		internal MenuItem submitRequestReplacementCertificateMenuItem;
		internal MenuItem submitRequestReissueCertificateMenuItem;
		internal MenuItem submitPreviewCertificateMenuItem;
		internal MenuItem submitTransferEDNMenuItem;
		internal MenuItem submitCancelEDNMenuItem;
		internal MenuItem readREXDataMenuItem;

		internal MenuItem CargoOnlineLodgementSystemMenuItem;
		internal MenuItem COLSSeperator;
		internal MenuItem COLSNewLodgementMenuItem;
		internal MenuItem COLSAddAdditionalDocumentMenuItem;
		internal MenuItem COLSReassessmentMenuItem;
		internal MenuItem COLSPaymentStatusMenuItem;
		internal MenuItem COLSLodgementStatusMenuItem;
		internal MenuItem COLSAddAttachmentMenuItem;
		internal MenuItem COLSMakeAnEnquiryMenuItem;
		internal MenuItem COLSSwitchAepLodgementMenuItem;

		#region Implementation

		#region SetupTopLevelMenu

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			SetupEdificeMenu();
			SetupCMRMenu();
			SetupExportMenuItem();
			SetupQuarantineMenu();
			SetupDrawbackMenu();

			MenuItems.Add("-");
			MenuItems.AddRange(EdificeMenuItem.ToArray());
			MenuItems.AddRange(CMRMenuItem.ToArray());
			MenuItems.AddRange(ExportDeclarationMenuItem.ToArray());
			MenuItems.AddRange(QuarantineMenuItem.ToArray());
			MenuItems.AddRange(DrawbackMenuItem.ToArray());
			if (Env.CurrentUser.IsDeveloperLogin)
			{
				AddTestDataCreatorMenuItem();
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption
		{
			get { return true; }
		}

		protected override bool SupportsBOMExpander
		{
			get { return Declaration != null && Declaration.IsDrawback; }
		}

		#endregion

		#region SetupEdificeMenu

		protected void SetupEdificeMenu()
		{
			EdificeMenuItem = new List<MenuItem>();

			ChangeStatusToHoldAwaitingMenuItem = new ZMenuItem("Set 'Hold Awaiting' Status", ChangeStatusToHoldAwaiting_Click);
			ChangeStatusToDeclarationWorkCompleteMenuItem = new ZMenuItem("Set 'Declaration Work Finished' Status", ChangeStatusToDeclarationWorkComplete_Click);

			MenuItem edificeThrowAwayMergedLinesMenuItem = new ZMenuItem("Reset Declaration (Throw Away Merged Lines)", ThrowAwayMergedLines_Click);
			EdificeDividerItem1 = new ZMenuItem("-");

			EdificeMenuItem.Add(EdificeDividerItem1);
			EdificeMenuItem.Add(ChangeStatusToHoldAwaitingMenuItem);
			EdificeMenuItem.Add(ChangeStatusToDeclarationWorkCompleteMenuItem);
			EdificeMenuItem.Add(new ZMenuItem("-"));
			EdificeMenuItem.Add(edificeThrowAwayMergedLinesMenuItem);
		}

		#endregion

		#region SetupCMRMenu

		protected void SetupCMRMenu()
		{
			CMRMenuItem = new List<MenuItem>();

			SendPaymentMenuItem = new ZMenuItem("Send Payment Message", SendPaymentMessage_Click);
			sendWithdrawalMenuItem = new ZMenuItem("Send Withdrawal Message", SendWithdrawalMessage_Click);
			throwAwayMergedLinesMenuItem = new ZMenuItem("Reset Declaration (Throw Away Merged Lines)", ThrowAwayMergedLines_Click);
			doCommunityProtectionDeclarationMenuItem = new ZMenuItem("Answer Declaration Questions", DoCommunityProtectionDeclarationForCMR_Click);

			CMRSendAmendmentMenuItem = new ZMenuItem("Send Amendment Message", SendAmendmentMessage_Click);

			ImpMessagingHelpMenuItem = new ZMenuItem(CMRMessage.MessagingHelpMenuCaption, MessagingHelp_Click);

			CMRSetStatusToHoldAwaitingMenuItem = new ZMenuItem("Set 'Hold Awaiting' Status", ChangeStatusToHoldAwaiting_Click);
			CMRClearStatusToHoldAwaitingMenuItem = new ZMenuItem("Clear 'Hold Awaiting' Status", ChangeStatusToHoldAwaiting_Click);

			CMRSendPreLodgeMenuItem = new ZMenuItem("Send PreLodgement Message", SendPreLodgementMessage_Click);
			CMRSendLodgeWithoutPayMenuItem = new ZMenuItem("Send Lodgement Message WITHOUT Payment Approved", SendLodgementMessageWithoutPay_Click);
			CMRSendLodgeWithPayMenuItem = new ZMenuItem("Send Lodgement Message WITH Payment Approved", SendLodgementMessageWithPay_Click);
			dequeueLodgementOrPaymentMessageMenuItem = new ZMenuItem("Dequeue Scheduled Lodgement or Payment", DequeueLodgementOrPaymentMessage_Click);

			CMRSetStatusToDeclarationWorkCompleteMenuItem = new ZMenuItem(SetDeclarationWorkFinishedMenuText, ChangeStatusToDeclarationWorkComplete_Click);
			CMRClearStatusToDeclarationWorkCompleteMenuItem = new ZMenuItem(ClearDeclarationWorkFinishedMenuText, ChangeStatusToDeclarationWorkComplete_Click);

			CMRDoCPForWithdrawalMenuItem = new ZMenuItem("Answer Declaration Questions for Withdrawal", DoCommunityProtectionDeclarationForCMRWithdrawal_Click);
			CMRRegenerateCPDecQuestionsMenuItem = new ZMenuItem("Regenerate and Answer Declaration Questions", RegenerateDeclarationQuestions_Click);
			CMRLogAuthorityToPayGivenMenuItem = new ZMenuItem("Add a log 'EFT Payment Authority' Given by Importer", CheckInAuthorityToPayGiven_Click);

			CreateContingencyMenuItem = new ZMenuItem("Create Contingency Data", CreateContingencyEventHandler);

			CMRMenuItem.Add(doCommunityProtectionDeclarationMenuItem);
			CMRMenuItem.Add(CMRRegenerateCPDecQuestionsMenuItem);

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(CMRSendPreLodgeMenuItem);
			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(CMRSendLodgeWithPayMenuItem);
			CMRMenuItem.Add(CMRSendLodgeWithoutPayMenuItem);
			CMRMenuItem.Add(SendPaymentMenuItem);
			CMRMenuItem.Add(dequeueLodgementOrPaymentMessageMenuItem);

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(CMRSendAmendmentMenuItem);

			SetupCOLSMenu();

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(ImpMessagingHelpMenuItem);

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(CMRDoCPForWithdrawalMenuItem);
			CMRMenuItem.Add(sendWithdrawalMenuItem);

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(CMRSetStatusToHoldAwaitingMenuItem);
			CMRMenuItem.Add(CMRClearStatusToHoldAwaitingMenuItem);

			CMRMenuItem.Add(CMRSetStatusToDeclarationWorkCompleteMenuItem);
			CMRMenuItem.Add(CMRClearStatusToDeclarationWorkCompleteMenuItem);

			CMRMenuItem.Add(CMRLogAuthorityToPayGivenMenuItem);

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(throwAwayMergedLinesMenuItem);

			CMRMenuItem.Add(new ZMenuItem("-"));
			CMRMenuItem.Add(CreateContingencyMenuItem);
		}

		#endregion

		#region SetupExportMenuItem

		internal const string SubmitExportDeclarationMenuItemText = "Send Original";
		internal const string SubmitExportDeclarationReplacementMenuItemText = "Send Replacement";

		protected void SetupExportMenuItem()
		{
			ExportDeclarationMenuItem = new List<MenuItem>();
			SubmitOriginalExportDecMenuItem = new ZMenuItem(SubmitExportDeclarationMenuItemText, SubmitExportDeclarationEventHandler);
			SubmitReplacementExportDecMenuItem = new ZMenuItem(SubmitExportDeclarationReplacementMenuItemText, SubmitExportDeclarationEventHandler);
			OtherCustomsMessagesMenuItem = new ZMenuItem("Other Customs Messages");
			WarrelSendOriginalMenuItem = new ZMenuItem("Send Original Warehouse Export Release Notice (WARREL)", WarrelEventHandler);
			WarrelSendReplacementMenuItem = new ZMenuItem("Send Replacement Warehouse Export Release Notice (WARREL)", WarrelEventHandler);
			WithdrawWarrelMenuItem = new ZMenuItem("Send Withdraw Warehouse Export Release Notice (WARREL)", WithdrawWarrelEventHandler);
			WarretSendOriginalMenuItem = new ZMenuItem("Send Original Warehouse Export Return Notice (WARRET)", WarretEventHandler);
			WarretSendReplacementMenuItem = new ZMenuItem("Send Replacement Warehouse Export Return Notice (WARRET)", WarretEventHandler);
			DeprecSendOriginalMenuItem = new ZMenuItem("Send Original Depot Export Receival Notice (DEPREC)", DeprecEventHandler);
			DeprecSendReplacementMenuItem = new ZMenuItem("Send Replacement Depot Export Receival Notice (DEPREC)", DeprecEventHandler);
			WithdrawDeprecMenuItem = new ZMenuItem("Send Withdraw Depot Export Receival Notice (DEPREC)", WithdrawDeprecEventHandler);
			DeprelSendOriginalMenuItem = new ZMenuItem("Send Original Depot Export Release Notice (DEPREL)", DeprelEventHandler);
			DeprelSendRepacementMenuItem = new ZMenuItem("Send Replacement Depot Export Release Notice (DEPREL)", DeprelEventHandler);
			WithdrawDeprelMenuItem = new ZMenuItem("Send Withdraw Depot Export Release Notice (DEPREL)", WithdrawDeprelEventHandler);
			MenuItem withdrawDeclarationMenuItem = new ZMenuItem("Withdraw Declaration", WithdrawDeclarationEventHandler);
			ExpMessagingHelpMenuItem = new ZMenuItem(CMRMessage.MessagingHelpMenuCaption, MessagingHelp_Click);
			ResetExportDeclarationMenuItem = new ZMenuItem("Reset Declaration", ResetExportDeclaration_Click);
			ResetExportDeclarationAndLinesMenuItem = new ZMenuItem("Reset Declaration (Throw Away Merged Lines)", ResetExportDeclaration_Click);
			CreateContingencyMenuItem = new ZMenuItem("Create Contingency Data", CreateContingencyEventHandler);

			ExportDeclarationMenuItem.Add(SubmitOriginalExportDecMenuItem);
			ExportDeclarationMenuItem.Add(SubmitReplacementExportDecMenuItem);
			ExportDeclarationMenuItem.Add(withdrawDeclarationMenuItem);
			ExportDeclarationMenuItem.Add(ExpMessagingHelpMenuItem);
			ExportDeclarationMenuItem.Add(new ZMenuItem("-"));
			ExportDeclarationMenuItem.Add(OtherCustomsMessagesMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(WarrelSendOriginalMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(WarrelSendReplacementMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(WithdrawWarrelMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(new ZMenuItem("-"));
			OtherCustomsMessagesMenuItem.MenuItems.Add(WarretSendOriginalMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(WarretSendReplacementMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(new ZMenuItem("-"));
			OtherCustomsMessagesMenuItem.MenuItems.Add(DeprecSendOriginalMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(DeprecSendReplacementMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(WithdrawDeprecMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(new ZMenuItem("-"));
			OtherCustomsMessagesMenuItem.MenuItems.Add(DeprelSendOriginalMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(DeprelSendRepacementMenuItem);
			OtherCustomsMessagesMenuItem.MenuItems.Add(WithdrawDeprelMenuItem);
			ExportDeclarationMenuItem.Add(new ZMenuItem("-"));
			ExportDeclarationMenuItem.Add(ResetExportDeclarationMenuItem);
			ExportDeclarationMenuItem.Add(ResetExportDeclarationAndLinesMenuItem);
			ExportDeclarationMenuItem.Add(new ZMenuItem("-"));
			ExportDeclarationMenuItem.Add(CreateContingencyMenuItem);
		}

		#endregion

		#region SetupQuarantineMenuItems

		protected void SetupQuarantineMenu()
		{
			rfpMessageMenuItem = new ZMenuItem("Request For Permit");
			submitRFPOrderMenuItem = new ZMenuItem("Submit RFP Order", SubmitRFPOrderMenuItem_Click);
			submitRFPLodgeMenuItem = new ZMenuItem("Submit RFP Lodge", SubmitRFPLodgeMenuItem_Click);
			submitRFPAmendmentMenuItem = new ZMenuItem("Submit RFP Amendment", SubmitRFPAmendMenuItem_Click);
			submitRFPWithdrawalMenuItem = new ZMenuItem("Submit RFP Withdrawal", SubmitRFPWithdrawlMenuItem_Click);
			submitRFPTransferMenuItem = new ZMenuItem("Submit RFP Transfer", SubmitRFPTransferMenuItem_Click);
			submitRFPEnquiryMenuItem = new ZMenuItem("Submit RFP Enquiry", SubmitRFPCopyMenuItem_Click);
			submitRFPAcceptMenuItem = new ZMenuItem("Submit RFP Accept Transfer", SubmitRFPAcceptTransferMenuItem_Click);
			submitRFPDeclineMenuItem = new ZMenuItem("Submit RFP Decline Transfer", SubmitRFPDeclineTransferMenuItem_Click);

			submitREXForwardMenuItem = new ZMenuItem("Submit REX Forward", SubmitREXForwardMenuItem_Click);
			submitREXTransferMenuItem = new ZMenuItem("Submit REX Transfer", SubmitREXTransferMenuItem_Click);
			submitREXCancellationMenuItem = new ZMenuItem("Submit REX Cancellation", SubmitREXCancellationMenuItem_Click);

			replaceReissueMenuSeperatorItem = new ZMenuItem("-");
			submitREXRequestAmendmentMenuItem = new ZMenuItem("Request Manual REX Amendment", SubmitREXRequestAmendmentMenuItem_Click);
			submitRequestReplacementCertificateMenuItem = new ZMenuItem("Request Replacement Certificate", SubmitRequestReplacementCertificateMenuItem_Click);
			submitRequestReissueCertificateMenuItem = new ZMenuItem("Request Reissue of Certificate", SubmitRequestReissueCertificateMenuItem_Click);
			submitPreviewCertificateMenuItem = new ZMenuItem("Preview Certificate", SubmitPreviewCertificateMenuItem_Click);
			submitTransferEDNMenuItem = new ZMenuItem("Transfer EDN", SubmitTransferEDNMenuItem_Click);
			submitCancelEDNMenuItem = new ZMenuItem("Cancel EDN", SubmitCancelEDNMenuItem_Click);
			readREXDataMenuItem = new ZMenuItem("Refresh REX Data", RefreshREXData_Click);

			rfpMessageMenuItem.MenuItems.Add(submitRFPOrderMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitRFPLodgeMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitRFPAmendmentMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitREXRequestAmendmentMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitRFPWithdrawalMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitREXCancellationMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitRFPTransferMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitRFPEnquiryMenuItem);
			rfpMessageMenuItem.MenuItems.Add(new ZMenuItem("-"));
			rfpMessageMenuItem.MenuItems.Add(submitRFPAcceptMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitRFPDeclineMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitREXForwardMenuItem);
			rfpMessageMenuItem.MenuItems.Add(submitREXTransferMenuItem);

			submitCertRequestMenuItem = new ZMenuItem("Submit Certificate Request", SubmitCertRequestMenuItem_Click);
			QuarantineMenuItem = new List<MenuItem>();
			QuarantineMenuItem.Add(new ZMenuItem("-"));
			QuarantineMenuItem.Add(rfpMessageMenuItem);
			QuarantineMenuItem.Add(submitCertRequestMenuItem);
			QuarantineMenuItem.Add(new ZMenuItem("-"));
			QuarantineMenuItem.Add(new ZMenuItem("Reset Declaration", ResetQuarantineDeclaration_Click));

			ReplaceReissueMenuItem = new List<MenuItem>();
			ReplaceReissueMenuItem.Add(replaceReissueMenuSeperatorItem);
			ReplaceReissueMenuItem.Add(submitRequestReplacementCertificateMenuItem);
			ReplaceReissueMenuItem.Add(submitRequestReissueCertificateMenuItem);
			ReplaceReissueMenuItem.Add(submitPreviewCertificateMenuItem);
			ReplaceReissueMenuItem.Add(new ZMenuItem("-"));
			ReplaceReissueMenuItem.Add(submitTransferEDNMenuItem);
			ReplaceReissueMenuItem.Add(submitCancelEDNMenuItem);

			rfpMessageMenuItem.MenuItems.AddRange(ReplaceReissueMenuItem.ToArray());

			RefreshREXDataMenuItem = new List<MenuItem>();
			RefreshREXDataMenuItem.Add(new ZMenuItem("-"));
			RefreshREXDataMenuItem.Add(readREXDataMenuItem);
			rfpMessageMenuItem.MenuItems.AddRange(RefreshREXDataMenuItem.ToArray());
		}

		void SetupCOLSMenu()
		{
			COLSSeperator = new ZMenuItem("-");
			CargoOnlineLodgementSystemMenuItem = new ZMenuItem("Cargo Online Lodgement System (COLS)");
			COLSNewLodgementMenuItem = new ZMenuItem("New Lodgement", COLSNewLodgementMenuItem_Click);
			COLSAddAttachmentMenuItem = new ZMenuItem("Add Attachment", COLSAddAttachmentMenuItem_Click);
			COLSAddAdditionalDocumentMenuItem = new ZMenuItem("Add Additional Document", COLSAddAdditionalDocumentMenuItem_Click);
			COLSSwitchAepLodgementMenuItem = new ZMenuItem("Switch AEP Lodgement", COLSSwitchAEPLodgementMenuItem_Click);
			COLSLodgementStatusMenuItem = new ZMenuItem("Request Lodgement Status", COLSLodgementStatusMenuItem_Click);
			COLSReassessmentMenuItem = new ZMenuItem("Request Reassessment", COLSReassessmentMenuItem_Click);
			COLSPaymentStatusMenuItem = new ZMenuItem("Request Payment Status", COLSPaymentStatusMenuItem_Click);
			COLSMakeAnEnquiryMenuItem = new ZMenuItem("Make an Enquiry", COLSMakeAnEnquiryMenuItem_Click);

			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSNewLodgementMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSAddAttachmentMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSAddAdditionalDocumentMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(new ZMenuItem("-"));
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSReassessmentMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSMakeAnEnquiryMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSSwitchAepLodgementMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(new ZMenuItem("-"));
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSLodgementStatusMenuItem);
			CargoOnlineLodgementSystemMenuItem.MenuItems.Add(COLSPaymentStatusMenuItem);

			CMRMenuItem.Add(COLSSeperator);
			CMRMenuItem.Add(CargoOnlineLodgementSystemMenuItem);
		}

		ZBool IsNEXDOCSActive => (ZBool)Declaration.QuarantineInvoice?.IsNEXDOCSActive;

		#region OnMenuItem_Click

		bool WaitingForManualAmendmentResponse
		{
			get
			{
				var result = false;
				if (IsNEXDOCSActive && Declaration.PendingManualAmendmentResponse)
				{
					Globals.Message.ShowError("The system is waiting for a response to a manual amendment request. Cannot send Request for Export messages currently.");
					result = true;
				}

				return result;
			}
		}

		void SubmitRFPOrderMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var messageType = IsNEXDOCSActive ? NEXDOCMessageType.Codes.Order : EXDOCMessageTypeCodes.Codes.ORD;
				SendMessage(messageType, "Order", manager => manager.CanSendOrderMessage, false);
			}
		}

		void SubmitRFPLodgeMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var messageType = IsNEXDOCSActive ? NEXDOCMessageType.Codes.Lodge : EXDOCMessageTypeCodes.Codes.LDG;
				SendMessage(messageType, "Lodge", manager => manager.CanSendOriginalMessage, true);
			}
		}

		void SubmitRFPAmendMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var messageType = IsNEXDOCSActive ? NEXDOCMessageType.Codes.Amend : EXDOCMessageTypeCodes.Codes.RPL;
				SendMessage(messageType, "Amendment", manager => manager.CanSendAmendmentMessage, true);
			}
		}

		void SubmitRFPWithdrawlMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var messageType = IsNEXDOCSActive ? NEXDOCMessageType.Codes.Withdrawal : EXDOCMessageTypeCodes.Codes.CAN;
				SendMessage(messageType, "Withdrawal", manager => manager.CanSendWithdrawlMessage, true);
			}
		}

		void SubmitRFPTransferMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(EXDOCMessageTypeCodes.Codes.TRF, "Transfer", manager => manager.CanSendTransferMessage, true);
		}

		void SubmitRFPCopyMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(EXDOCMessageTypeCodes.Codes.CPY, "Copy", manager => manager.CanSendCopyMessage, false);
		}

		void SubmitRFPAcceptTransferMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(EXDOCMessageTypeCodes.Codes.AcceptTransferIn, "Transfer Acceptance", manager => manager.CanSendTransferInMessage, false);
		}

		void SubmitRFPDeclineTransferMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(EXDOCMessageTypeCodes.Codes.DeclineTransferIn, "Transfer Rejection", manager => manager.CanSendTransferInMessage, false);
		}

		void SubmitCertRequestMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(EXDOCMessageTypeCodes.Codes.CRQ, "Certificate Request", manager => manager.CanSendCertificateRequestMessage, true);
		}

		void SubmitREXForwardMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				SendMessage(NEXDOCMessageType.Codes.REXForward, "Forward", manager => manager.CanSendForwardMessage, true);
			}
		}

		void SubmitREXTransferMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				SendMessage(NEXDOCMessageType.Codes.REXTransfer, "Transfer", manager => manager.CanSendTransferMessage, true);
			}
		}

		void SubmitREXCancellationMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var promptResult = Globals.Message.Show("Are you sure you want to cancel this REX?", submitREXCancellationMenuItem.Text, MessageBoxButtons.YesNo, DialogResult.Yes);
				if (promptResult == DialogResult.Yes)
				{
					SendMessage(NEXDOCMessageType.Codes.Cancellation, "Cancellation", manager => manager.CanSendNEXDOCCancellation, true);
				}
			}
		}

		void SubmitREXRequestAmendmentMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var quarantineHeader = Declaration.QuarantineInvoice?.QuarantineExDocHeader;
				if (quarantineHeader != null)
				{
					var amendmentWithdrawalReason = new CMRAmendmentWithdrawalReason();
					ZFormModaliser.ShowDialogAndDispose(new AmendmentReasonForm(amendmentWithdrawalReason, REXRequestAmendmentPreamble), this.MainForm);

					if (!amendmentWithdrawalReason.IsCancelled && !amendmentWithdrawalReason.ReasonText.IsEmpty)
					{
						using (new DisposableAction(() => quarantineHeader.ManualAmendmentReasonForMessaging = amendmentWithdrawalReason.ReasonText, () => quarantineHeader.ManualAmendmentReasonForMessaging = ZString.Empty))
						{
							var messageType = IsNEXDOCSActive ? NEXDOCMessageType.Codes.ManualAmend : EXDOCMessageTypeCodes.Codes.RPL;
							var caption = submitREXRequestAmendmentMenuItem.Text;
							SendMessage(messageType, caption, manager => manager.CanSendAmendmentMessage, true);
						}
					}
				}
				else
				{
					Globals.Message.ShowError("A Quarantine Invoice is required.");
				}
			}
		}

		internal ResourceStringData REXRequestAmendmentPreamble => Res.GetData("4F5BC2D6-2E66-4CCB-B1BB-8A47FB14737E", "You are requesting a Department Officer to manually amend this REX.  If you continue, this REX will be locked until the officer manually approves the requested changes.  You need to send a reason for requesting this amendment.");

		void SubmitRequestReplacementCertificateMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var caption = submitRequestReplacementCertificateMenuItem.Text;
				if (Globals.Message.Show("Are you sure you want to Request a Replacement Certificate?", caption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
				{
					SendMessage(NEXDOCMessageType.Codes.ReplacementCertificate, caption, manager => manager.CanSendRequestReplacementCertificate, true);
				}
			}
		}

		void SubmitRequestReissueCertificateMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				var quarantineHeader = Declaration.QuarantineInvoice?.QuarantineExDocHeader;
				if (quarantineHeader != null)
				{
					var certificateReissueHeader = new CertificateReissueHeader(Declaration.Factory, Declaration.QuarantineCertificateNumbersForSelection);

					var res = ZFormModaliser.ShowDialogAndDispose(new REXCertificateSelectionForm(certificateReissueHeader), MainForm);
					if (res == DialogResult.OK)
					{
						if (certificateReissueHeader.IsValid)
						{
							var caption = submitRequestReissueCertificateMenuItem.Text;
							SendMessage(NEXDOCMessageType.Codes.ReissueCertificate, caption, manager => manager.CanSendRequestReissueCertificate, true, certificateReissueHeader);
						}
						else
						{
							Globals.Message.ShowError("A Certificate is required.");
						}
					}
				}
				else
				{
					Globals.Message.ShowError("A Quarantine Invoice is required.");
				}
			}
		}

		void SubmitPreviewCertificateMenuItem_Click(object sender, EventArgs e)
		{
			if (!WaitingForManualAmendmentResponse)
			{
				SendMessage(NEXDOCMessageType.Codes.PreviewCertificate, submitPreviewCertificateMenuItem.Text, manager => manager.CanSendPreviewCertificate, true);
			}
		}

		#region COLS

		void COLSNewLodgementMenuItem_Click(object sender, EventArgs e)
		{
			var form = new DeclarationAgreementForm(new COLSDeclarationAcceptance());
			form.Text = "New COLS Lodgement";
			if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
			{
				var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, form.DeclarationAgreementControl.AdditionalCommentTextBox.Text);
				if (CanSendCOLSMessages(messageSender, true))
				{
					var (dialogResult, docsToBeSent) = GetCOLSDocumentsToBeSent(true);
					if (dialogResult == DialogResult.OK)
					{
						SendColsMessages(messageSender, s => s.SendLodgementMessage(docsToBeSent));
					}
				}
			}
		}

		void COLSLodgementStatusMenuItem_Click(object sender, EventArgs e)
		{
			var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, string.Empty);
			if (CanSendCOLSMessages(messageSender, false))
			{
				SendColsMessages(messageSender, s => s.SendLodgementStatusMessage());
			}
		}

		void COLSAddAttachmentMenuItem_Click(object sender, EventArgs e)
		{
			var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, "");
			if (CanSendCOLSMessages(messageSender, false))
			{
				var (dialogResult, docsToBeSent) = GetCOLSDocumentsToBeSent(true);
				if (dialogResult == DialogResult.OK)
				{
					SendColsMessages(messageSender, s => s.SendAttachmentMessages(docsToBeSent));
				}
			}
		}

		(DialogResult, CusStorageDocPivot[]) GetCOLSDocumentsToBeSent(bool mustSelectOneOrMoreDocuments)
		{
			var colsHeader = Declaration.QuarantineCOLSHeader;
			var messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
			var docsToBeSent = Array.Empty<CusStorageDocPivot>();
			var dialogResult = DialogResult.OK;
			if (messageSendingActionParent.ShouldAllowUsersToSelectSendingObjects)
			{
				var form = new AUCOLSAttachmentsSelectionForm(messageSendingActionParent, mustSelectOneOrMoreDocuments);
				dialogResult = ZFormModaliser.ShowDialogAndDispose(form);
				if (dialogResult == DialogResult.OK)
				{
					docsToBeSent = messageSendingActionParent.SelectedDocuments;
				}
			}
			else
			{
				docsToBeSent = colsHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ToArray();
			}

			return (dialogResult, docsToBeSent);
		}

		void COLSAddAdditionalDocumentMenuItem_Click(object sender, EventArgs e)
		{
			var proceed = true;
			var form = new DeclarationAgreementForm(new COLSDeclarationAcceptance());
			form.Text = "Add Additional Document";
			if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
			{
				var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, form.DeclarationAgreementControl.AdditionalCommentTextBox.Text);
				if (CanSendCOLSMessages(messageSender, false))
				{
					var (dialogResult, docsToBeSent) = GetCOLSDocumentsToBeSent(false);
					if (dialogResult == DialogResult.OK)
					{
						var quarantineColsHeader = Declaration.QuarantineCOLSHeader;
						if (quarantineColsHeader.LRNStatus == COLSEntryStatusList.Codes.LrnActive)
						{
							var warningMessage = Res.GetString("B50D4E8F-7B72-4243-BB72-DF50FA008A1C", @"The current LRN status indicates that this COLS entry is active and does not need to be reopened in order to attach more documents.
By sending this request, all pending attachment messages will be discarded and the status of the LRN reset to CLS (Closed).
If you are sure this is the correct course of action and still wish to proceed, confirm below:");
							proceed = Globals.Message.ShowConfirmation(warningMessage, "LRN is active", "PROCEED", MessageBoxIcon.Warning) == DialogResult.OK;
							if (proceed)
							{
								DiscardCurrentColsAndPrepareToSendNewRequest();
							}
						}

						if (proceed)
						{
							SendColsMessages(messageSender, s => s.SendAdditionalDocumentMessage(docsToBeSent));
						}
					}
				}
			}
		}

		void DiscardCurrentColsAndPrepareToSendNewRequest()
		{
			var quarantineColsHeader = Declaration.QuarantineCOLSHeader;
			quarantineColsHeader.LRNCusEntryNumber.CE_EntryStatus = COLSEntryStatusList.Codes.LrnInactive;
			quarantineColsHeader.DiscardPendingAttachmentMessages();
			quarantineColsHeader.Logs.AddNew(new EventValue(AutoEvents.Authorised, reference: "COLS AAD Override", eventTime: ZDateTimeOffset.Now));
		}

		void COLSSwitchAEPLodgementMenuItem_Click(object sender, EventArgs e)
		{
			var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, string.Empty);
			if (CanSendCOLSMessages(messageSender, false))
			{
				SendColsMessages(messageSender, s => s.SendSwitchAEPLodgementMessage());
			}
		}

		void COLSReassessmentMenuItem_Click(object sender, EventArgs e)
		{
			var colsHeader = Declaration.QuarantineCOLSHeader;
			var form = new AUCOLSReassessmentForm(colsHeader, new COLSDeclarationAcceptance());
			if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
			{
				var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, form.DeclarationAgreementControl.AdditionalCommentTextBox.Text);

				if (CanSendCOLSMessages(messageSender, true))
				{
					var dialogResult = DialogResult.OK;
					CusStorageDocPivot[] docsToBeSent = null;
					if (form.RequireDocumentation)
					{
						(dialogResult, docsToBeSent) = GetCOLSDocumentsToBeSent(true);
					}
					if (dialogResult == DialogResult.OK)
					{
						SendColsMessages(messageSender, s => s.SendReassessmentMessage(docsToBeSent, form.ReassessmentReason, form.RequireDocumentation));
					}
				}
			}
		}

		void COLSPaymentStatusMenuItem_Click(object sender, EventArgs e)
		{
			var form = new AUCOLSPaymentStatusForm();
			if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
			{
				var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, form.ClientAccountNumberextBox.Text);
				if (CanSendCOLSMessages(messageSender, false))
				{
					SendColsMessages(messageSender, s => s.SendPaymentStatusMessage());
				}
			}
		}

		void COLSMakeAnEnquiryMenuItem_Click(object sender, EventArgs e)
		{
			var colsHeader = Declaration.QuarantineCOLSHeader;
			var enquiryAdditionalInfo = new COLSEnquiryAdditionalInformation(colsHeader);
			var form = new AUCOLSEnquiryRequestAgreementForm(enquiryAdditionalInfo);
			if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
			{
				var messageSender = new COLSMessageSender(Declaration.QuarantineCOLSHeader, form.DeclarationAgreementControl.AdditionalCommentTextBox.Text);
				if (CanSendCOLSMessages(messageSender, false))
				{
					var dialogResult = DialogResult.OK;
					CusStorageDocPivot[] docsToBeSent = null;
					if (enquiryAdditionalInfo.DocumentRequired)
					{
						(dialogResult, docsToBeSent) = GetCOLSDocumentsToBeSent(true);
					}
					if (dialogResult == DialogResult.OK)
					{
						SendColsMessages(messageSender, s => s.SendEnquiryMessage(enquiryAdditionalInfo, docsToBeSent));
					}
				}
			}
		}

		void SendColsMessages(COLSMessageSender colsMessageSender, Func<COLSMessageSender, string> send)
		{
			try
			{
				var message = send(colsMessageSender);
				Declaration.Factory.Save();
				Declaration.QuarantineCOLSHeader.ReloadMessages();
				Globals.Message.Show(message.IsNullOrEmpty() ? "The message has been sent." : message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				colsMessageSender.RollBack();
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		bool CanSendCOLSMessages(COLSMessageSender messageSender, bool needValidation)
		{
			return CheckHasChanges() && IsDeclarationAvailable && (!needValidation || messageSender.Validation.CheckBusinessObjectLevelValidation(Declaration.MessageInitiator));
		}

		#endregion

		#region TransferEDNMenuItem_Click

		void SubmitTransferEDNMenuItem_Click(object sender, EventArgs e)
		{
			var caption = submitTransferEDNMenuItem.Text;
			var currentProduceType = Declaration.Invoices.Cast<JobComInvoiceHeader>().FirstOrDefault()?.QuarantineExDocHeader?.QH_ProduceType ?? ZString.Empty;

			var listValidationFilter = new ZDBOnlyQuery(typeof(Customs.Business.BaseJobDeclaration));
			listValidationFilter.AddToFilter(JoinCondition.And, JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, Declaration.PK);
			listValidationFilter.AddSubQuery(EXDOCExclusionFilter, JoinCondition.And);

			var filteredCollection = new JobDeclarationCollection(Declaration.Factory, listValidationFilter);
			filteredCollection.SetOverrideNotificationWhenAdditionalFilterNotMet("Declaration must have an AQIS REX number.");

			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipment Type", "Property", new ZString(CMRUnderbondRequestCodes.Codes.Quarantine), isRemovable: false));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Quarantine Produce Type", "Property", currentProduceType, isRemovable: false));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("RFP Number", "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.IsNotBlank, isRemovable: false));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Entry #", "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.IsBlank, isRemovable: false));

			var declaration = BusinessObjectModulePicker.PickOneRecordFromModuleScreen<JobDeclaration>(filteredCollection, ModuleIDs.Customs.JobDeclaration);
			if (declaration != null && CheckDeclarationCanBeAnEDNTransferTarget(declaration, currentProduceType, caption))
			{
				using (new DisposableAction(() => Declaration.RelatedDeclarationForTransferEDN = declaration, () => Declaration.RelatedDeclarationForTransferEDN = null))
				{
					SendMessage(NEXDOCMessageType.Codes.TransferEDN, caption, manager => manager.CanSendTransferEDN, true);
				}
			}
		}

		ZDBOnlySubQuery EXDOCExclusionFilter
		{
			get
			{
				var ediMessage = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID, notIn: true);
				ediMessage.AddToFilter(EDIMessageSchema.EM_LinkTable, QuarantineExDocHeaderSchema.Constants.TableName);
				ediMessage.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.EXDOC);

				var quarantineExDocHeader = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
				quarantineExDocHeader.AddSubQuery(ediMessage, JoinCondition.And);

				var invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				invoiceHeaderQuery.AddSubQuery(quarantineExDocHeader, JoinCondition.And);

				return invoiceHeaderQuery;
			}
		}

		bool CheckDeclarationCanBeAnEDNTransferTarget(JobDeclaration declaration, ZString currentProduceType, ZString caption)
		{
			var message = ZString.Empty;

			if (declaration.PK == Declaration.PK)
			{
				message = "EDN cannot be transferred to itself.";
			}
			else
			{
				var quarantineHeader = declaration.QuarantineInvoice?.QuarantineExDocHeader;
				if (quarantineHeader == null || quarantineHeader.QH_ProduceType != currentProduceType || quarantineHeader.RexNumber.IsEmpty || !quarantineHeader.IsNEXDOCSActive)
				{
					message = $"EDN can only be transferred to a Quarantine Declaration of produce type '{currentProduceType}' that has a REX Number.";
				}
			}

			if (!message.IsEmpty)
			{
				Globals.Message.ShowError(message, caption);
				return false;
			}

			return true;
		}

		#endregion

		void SubmitCancelEDNMenuItem_Click(object sender, EventArgs e)
		{
			var caption = submitCancelEDNMenuItem.Text;
			if (Globals.Message.Show("Are you sure you want to Cancel this EDN?", caption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
			{
				SendMessage(NEXDOCMessageType.Codes.CancelEDN, caption, manager => manager.CanSendCancelEDN, true);
			}
		}

		void RefreshREXData_Click(object sender, EventArgs e)
		{
			var caption = readREXDataMenuItem.Text;
			if (Globals.Message.Show("This will update the REX with the latest data from NEXDOC, do you want to continue?", caption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
			{
				SendMessage(NEXDOCMessageType.Codes.ReadREX, caption, manager => manager.CandSendReadREX, false);
			}
		}

		#endregion

		#region Send Quarantine Message

		void SendMessage(string exDocMessageTypeCode, string messageCaption, CanSendDelegate canSendDelegate, bool shouldValidate, object additionalInformation = null)
		{
			if (CheckHasChanges() && IsDeclarationAvailable)
			{
				var manager = new RFPMultiMessageManager(Declaration, exDocMessageTypeCode);
				if (shouldValidate)
				{
					SetValidateQuarantineObjects();
				}

				if (!canSendDelegate(manager))
				{
					Globals.Message.ShowError(manager.Notifications, "Cannot Send " + messageCaption);
				}
				else if (!shouldValidate || manager.Validation.CheckBusinessObjectLevelValidation(Declaration.MessageInitiator, manager.ShouldSendMessagesInTestMode))
				{
					SendRFPOff(manager, additionalInformation);
				}
			}
		}

		void SetValidateQuarantineObjects()
		{
			Declaration.MarkAsNeedingValidationIncludingChildren();
			if (Declaration.Invoices.Count > 0)
			{
				Declaration.QuarantineInvoice.QuarantineExDocHeader.MarkAsNeedingValidationIncludingChildren();
				Declaration.QuarantineExdocLineLines.MarkAsNeedingValidationIncludingChildren();
			}
		}

		void SendRFPOff(RFPMultiMessageManager messageManager, object additionalInformation)
		{
			if (Globals.Message.ShowConfirmation("WARNING. You are giving information to a Commonwealth entity. Giving false or misleading information to a Commonwealth entity is a serious offence.", "WARNING", "YES", MessageBoxIcon.Exclamation) == DialogResult.OK)
			{
				if (messageManager.SendMessages(Declaration.MessageInitiator, additionalInformation))
				{
					try
					{
						Declaration.Factory.Save();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(e);
					}
				}
			}
		}

		delegate bool CanSendDelegate(RFPMultiMessageManager manager);

		#endregion

		internal MenuItem rfpMessageMenuItem;
		MenuItem submitCertRequestMenuItem;

		#endregion

		#region SetupDrawbackMenu

		protected void SetupDrawbackMenu()
		{
			DrawbackMenuItem = new List<MenuItem>();
			MenuItem attachExportDeclarationsToDrawback = new ZMenuItem("Attach Export Declarations to Drawback", AttachExportDeclarationsToDrawback_Click);
			//		ZMenuItem SendDrawbackOriginalMenuItem = new ZMenuItem("Send Drawback Message", new EventHandler(SendDrawbackMessage_Click));
			MenuItem resetDrawbackMenuItem = new ZMenuItem("Reset Drawback to Original", ResetDrawback_Click);
			MenuItem doDrawbackDeclarationQuestionsMenuItem = new ZMenuItem("Answer Drawback Declaration Questions", DoAnswerDrawbackDeclarationQuestions_Click);

			DrawbackMenuItem.Add(attachExportDeclarationsToDrawback);
			DrawbackMenuItem.Add(doDrawbackDeclarationQuestionsMenuItem);
			//			DrawbackMenuItem.Add(SendDrawbackOriginalMenuItem);
			DrawbackMenuItem.Add(new ZMenuItem("-"));
			DrawbackMenuItem.Add(resetDrawbackMenuItem);

			//TODO: messaging development suspended until customs system works properly,
			//TODO: amendment and withdrawl to be implemented.
		}

		#endregion

		#region Refresh

		public const string ClearDeclarationWorkFinishedMenuText = "Clear 'Declaration Work Finished' Status";
		public const string SetDeclarationWorkFinishedMenuText = "Set 'Declaration Work Finished' Status";

		public override void RefreshMenu()
		{
			base.RefreshMenu();

			#region Pre-Condition Asserts

			if (ExportDeclarationMenuItem == null)
			{
				throw new ApplicationException("Export Declaration Menu is null");
			}

			if (EdificeMenuItem == null)
			{
				throw new ApplicationException("EdificeMenuItem is null");
			}

			if (CMRMenuItem == null)
			{
				throw new ApplicationException("CMRMenuItem is null");
			}

			if (QuarantineMenuItem == null)
			{
				throw new ApplicationException("QuarantineMenuItem is null");
			}

			if (DrawbackMenuItem == null)
			{
				throw new ApplicationException("DrawbackMenuItem is null");
			}

			if (ChangeStatusToDeclarationWorkCompleteMenuItem == null)
			{
				throw new ApplicationException("ChangeStatusToDeclarationWorkCompleteMenuItem is null");
			}

			if (CMRSetStatusToDeclarationWorkCompleteMenuItem == null)
			{
				throw new ApplicationException("CMR ChangeStatusToDeclarationWorkCompleteMenuItem is null");
			}

			if (ChangeStatusToHoldAwaitingMenuItem == null)
			{
				throw new ApplicationException("ChangeStatusToHoldAwaitingMenuItem is null");
			}

			#endregion

			var declaration = Declaration;
			var isIntegrated = declaration?.IsDeclarationIntegrated ?? false;
			var isMessagingAvailable = declaration != null && !declaration.IsWarehousedByExternalAgent && !declaration.IsDeclarationByExternalBroker && !isIntegrated;

			ExportDeclarationMenuItem.SetAllVisible(declaration != null && declaration.IsExport && !declaration.IsQuarantine && isMessagingAvailable);
			EdificeMenuItem.SetAllVisible(declaration != null && declaration.IsImport && !declaration.IsImportCMR && isMessagingAvailable);
			
			CMRMenuItem.SetAllVisible(declaration != null && declaration.IsImportCMR && isMessagingAvailable);
			QuarantineMenuItem.SetAllVisible(declaration != null && declaration.IsQuarantine && !isIntegrated);
			DrawbackMenuItem.SetAllVisible(declaration != null && declaration.IsDrawback && !isIntegrated);

			if (declaration != null)
			{
				ChangeStatusToDeclarationWorkCompleteMenuItem.Text = declaration.IsDeclarationWorkFinished ? ClearDeclarationWorkFinishedMenuText : SetDeclarationWorkFinishedMenuText;
				ChangeStatusToHoldAwaitingMenuItem.Text = declaration.IsHolding ? "Clear 'Hold Awaiting' Status" : "Set 'Hold Awaiting' Status";
				ControlVisibilityOfDeclarationWorkCompleteMenuItems();
				ControlVisibilityOfHoldAwaitingMenuItems();
				ControlVisibilityOfCOLSMenu();

				if (EdificeMenuItem.Count > 0 && EdificeMenuItem[0].Visible)
				{
					RefreshEdificeMenus();
				}

				if (CMRMenuItem.Count > 0 && CMRMenuItem[0].Visible)
				{
					RefreshCMRMenus();
				}

				if (ExportDeclarationMenuItem.Count > 0 && ExportDeclarationMenuItem[0].Visible)
				{
					RefreshExportMenu();
				}

				if (QuarantineMenuItem.Count > 0 && QuarantineMenuItem[0].Visible)
				{
					RefreshQuarantineMenu();
				}

				if (DrawbackMenuItem.Count > 0 && DrawbackMenuItem[0].Visible)
				{
					RefreshDrawbackMenu();
				}
			}
		}

		protected void RefreshEdificeMenus()
		{
			#region Pre-Condition Asserts
			if (Declaration == null)
			{
				throw new ApplicationException("Declaration == null");
			}
			#endregion
		}

		protected void RefreshCMRMenus()
		{
			#region Pre-Condition Asserts
			if (Declaration == null)
			{
				throw new ApplicationException("Declaration == null");
			}
			#endregion

			dequeueLodgementOrPaymentMessageMenuItem.Visible = Declaration.IsQueuedEntryLodgementsEnabled || Declaration.IsQueuedEntryPaymentsEnabled;
			dequeueLodgementOrPaymentMessageMenuItem.Enabled = IsQueuedForLodgementOrPayment(Declaration) && !Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration.IsConsolidated(Declaration);
			CMRSendLodgeWithPayMenuItem.Enabled = CMRSendLodgeWithoutPayMenuItem.Enabled = SendPaymentMenuItem.Enabled = IsSubmitEnabledOnConsolidatedDeclaration && !dequeueLodgementOrPaymentMessageMenuItem.Enabled;
			CMRSendPreLodgeMenuItem.Enabled = CMRSendAmendmentMenuItem.Enabled = sendWithdrawalMenuItem.Enabled = throwAwayMergedLinesMenuItem.Enabled = IsSubmitEnabledOnConsolidatedDeclaration;
		}

		bool IsQueuedForLodgementOrPayment(JobDeclaration declaration) => declaration.IsQueuedEntryLodgement || declaration.IsQueuedEntryPayment;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected void RefreshQuarantineMenu()
		{
			#region Pre-Condition Asserts

			if (Declaration == null)
			{
				throw new ApplicationException("Quarantine Declaration == null");
			}

			if (ReplaceReissueMenuItem == null)
			{
				throw new ApplicationException("ReplaceReissueMenuItem is null");
			}

			#endregion

			submitCertRequestMenuItem.Visible = Declaration.IsAQISCertificateRequest;
			rfpMessageMenuItem.Visible = !Declaration.IsAQISCertificateRequest;

			if (Declaration.Invoices.Count > 0)
			{
				var isNEXDOCSActive = Declaration.Invoices[0].IsNEXDOCSActive;
				submitRFPTransferMenuItem.Visible = !isNEXDOCSActive;
				submitRFPEnquiryMenuItem.Visible = !isNEXDOCSActive;
				submitRFPAcceptMenuItem.Visible = !isNEXDOCSActive;
				submitRFPDeclineMenuItem.Visible = !isNEXDOCSActive;

				submitREXCancellationMenuItem.Visible = isNEXDOCSActive;
				submitREXForwardMenuItem.Visible = isNEXDOCSActive;
				submitREXTransferMenuItem.Visible = isNEXDOCSActive;
				submitREXRequestAmendmentMenuItem.Visible = isNEXDOCSActive;
				RefreshREXDataMenuItem.SetAllVisible(isNEXDOCSActive);

				ReplaceReissueMenuItem.SetAllVisible(isNEXDOCSActive);
				submitRequestReissueCertificateMenuItem.Visible = isNEXDOCSActive;
				submitTransferEDNMenuItem.Visible = isNEXDOCSActive;

				submitPreviewCertificateMenuItem.Visible = isNEXDOCSActive;
				replaceReissueMenuSeperatorItem.Visible = isNEXDOCSActive;

				var quarantineExDocHeader = Declaration.Invoices[0].QuarantineExDocHeader;
				if (quarantineExDocHeader != null)
				{
					var rfpNumber = quarantineExDocHeader.QH_RequestForPermitNumber;
					var rfpStatusDesc = quarantineExDocHeader.QH_RequestForPermitNumberStatusDescription;
					var rfpExportPermitNumber = quarantineExDocHeader.QH_ExportPermitNumber;

					if (isNEXDOCSActive)
					{
						rfpMessageMenuItem.Text = "Request For Export";
						submitRFPOrderMenuItem.Text = "Submit REX Order";
						submitRFPLodgeMenuItem.Text = "Submit REX Lodge";
						submitRFPAmendmentMenuItem.Text = "Submit REX Amendment";
						submitRFPWithdrawalMenuItem.Text = "Submit REX Withdrawal";
						submitREXCancellationMenuItem.Text = "Submit REX Cancellation";

						var rfpHasNumberAndStatus = !(rfpNumber.IsEmpty || rfpStatusDesc.IsEmpty);
						var rfpNotWithdrawnCancelledOrSuspended = !(rfpStatusDesc == Status.WtdrnWithdrawn || rfpStatusDesc == Status.CancCancelled || rfpStatusDesc == Status.SuspSuspended);

						submitRFPAmendmentMenuItem.Enabled = rfpHasNumberAndStatus && rfpNotWithdrawnCancelledOrSuspended && !(rfpStatusDesc == Status.OrdrOrder);
						submitREXRequestAmendmentMenuItem.Enabled = submitRFPAmendmentMenuItem.Enabled;
						submitRFPWithdrawalMenuItem.Enabled = rfpHasNumberAndStatus && rfpNotWithdrawnCancelledOrSuspended && !(rfpStatusDesc == Status.CompCompleted || rfpStatusDesc == Status.EmhcEmergencyHealthCertificate) && rfpExportPermitNumber.IsEmpty;
						submitREXCancellationMenuItem.Enabled = !rfpExportPermitNumber.IsEmpty && rfpNotWithdrawnCancelledOrSuspended;

						submitRFPOrderMenuItem.Enabled = rfpStatusDesc.IsEmpty || (rfpNotWithdrawnCancelledOrSuspended && !(rfpStatusDesc == Status.InitInitial || rfpStatusDesc == Status.InspInspected || rfpStatusDesc == Status.FinlFinal || rfpStatusDesc == Status.CompCompleted || rfpStatusDesc == Status.CtrdCertificateReady || rfpStatusDesc == Status.EmhcEmergencyHealthCertificate));
						submitRFPLodgeMenuItem.Enabled = submitRFPOrderMenuItem.Enabled;

						var hasActiveEDN = !Declaration.CustomsAuthorityNumber.IsEmpty && Declaration.JE_EntryStatus != Common.AU.CustomsEntryStatus.Cancelled.Code && !Declaration.IsECNNumberTransferred;
						submitTransferEDNMenuItem.Enabled = hasActiveEDN;
						submitCancelEDNMenuItem.Enabled = hasActiveEDN;

						var hasActiveRFP = !rfpNumber.IsEmpty && rfpNotWithdrawnCancelledOrSuspended;
						submitREXForwardMenuItem.Enabled = hasActiveRFP;
						submitREXTransferMenuItem.Enabled = hasActiveRFP;
						submitRequestReplacementCertificateMenuItem.Enabled = hasActiveRFP;
						submitRequestReissueCertificateMenuItem.Enabled = submitRequestReplacementCertificateMenuItem.Enabled && !(rfpStatusDesc == Status.InitInitial || rfpStatusDesc == Status.OrdrOrder || rfpStatusDesc == Status.CtrdCertificateReady);
						submitPreviewCertificateMenuItem.Enabled = rfpStatusDesc == Status.CtrdCertificateReady;
						RefreshREXDataMenuItem.Skip(1).ForEach(mi => mi.Enabled = !rfpNumber.IsEmpty);
					}
					else
					{
						rfpMessageMenuItem.Text = "Request For Permit";
						submitRFPOrderMenuItem.Text = "Submit RFP Order";
						submitRFPLodgeMenuItem.Text = "Submit RFP Lodge";
						submitRFPAmendmentMenuItem.Text = "Submit RFP Amendment";
						submitRFPWithdrawalMenuItem.Text = "Submit RFP Withdrawal";

						submitRFPEnquiryMenuItem.Enabled = !(rfpStatusDesc.IsEmpty || rfpStatusDesc == Status.CancCancelled || rfpStatusDesc == Status.CompCompleted);
						submitRFPWithdrawalMenuItem.Enabled = submitRFPEnquiryMenuItem.Enabled;
						submitRFPAmendmentMenuItem.Enabled = submitRFPEnquiryMenuItem.Enabled && !(rfpStatusDesc == Status.OrdrOrder);
						submitRFPTransferMenuItem.Enabled = submitRFPEnquiryMenuItem.Enabled && !(rfpStatusDesc == Status.SuspSuspended || rfpStatusDesc == Status.EmhcEmergencyHealthCertificate);
						submitRFPOrderMenuItem.Enabled = rfpStatusDesc.IsEmpty || (submitRFPEnquiryMenuItem.Enabled && !(rfpStatusDesc == Status.InitInitial || rfpStatusDesc == Status.FinlFinal || rfpStatusDesc == Status.InspInspected || rfpStatusDesc == Status.SuspSuspended || rfpStatusDesc == Status.HcrdHealthCertificateReady || rfpStatusDesc == Status.CtrdCertificateReady || rfpStatusDesc == Status.EmhcEmergencyHealthCertificate));
						submitRFPLodgeMenuItem.Enabled = submitRFPOrderMenuItem.Enabled;
					}
				}
			}
		}

		protected void RefreshExportMenu()
		{
			SubmitOriginalExportDecMenuItem.Visible = Declaration.DeclarationNumber.IsEmpty;
			SubmitReplacementExportDecMenuItem.Visible = !Declaration.DeclarationNumber.IsEmpty;
			WarrelSendOriginalMenuItem.Visible = !Declaration.IsWARRELMessageLodged;
			WarrelSendReplacementMenuItem.Visible = Declaration.IsWARRELMessageLodged;
			WarretSendOriginalMenuItem.Visible = !Declaration.IsWARRETMessageLodged;
			WarretSendReplacementMenuItem.Visible = Declaration.IsWARRETMessageLodged;
			DeprecSendOriginalMenuItem.Visible = !Declaration.IsDEPRECMessageLodged;
			DeprecSendReplacementMenuItem.Visible = Declaration.IsDEPRECMessageLodged;
			DeprelSendOriginalMenuItem.Visible = !Declaration.IsDEPRELMessageLodged;
			DeprelSendRepacementMenuItem.Visible = Declaration.IsDEPRELMessageLodged;
			ResetExportDeclarationMenuItem.Text = Declaration.DeclarationExportCusEntryNumber == null ? "Reset Declaration (Throw Away Merged Lines)" : "Reset Declaration";
			ResetExportDeclarationMenuItem.Visible = Declaration.DeclarationExportCusEntryNumber != null;
			ResetExportDeclarationAndLinesMenuItem.Visible = Declaration.DeclarationExportCusEntryNumber == null;
		}

		protected void RefreshDrawbackMenu()
		{
			#region Pre-Condition Asserts
			if (Declaration == null)
			{
				throw new ApplicationException("Declaration == null");
			}
			#endregion
		}

		#endregion

		#region TestDataInserter for Developer Login Testing

		protected void AddTestDataCreatorMenuItem()
		{
			MenuItem developerTestDataMenuItem = new ZMenuItem("Developer Test Data");
			MenuItem setupValidAirShipmentMenuItem = new ZMenuItem("Setup Valid Air Shipment");
			MenuItem setupValidSeaShipmentMenuItem = new ZMenuItem("Setup Valid Sea Shipment");

			developerTestDataMenuItem.MenuItems.Add(setupValidAirShipmentMenuItem);
			developerTestDataMenuItem.MenuItems.Add(setupValidSeaShipmentMenuItem);

			setupValidAirShipmentMenuItem.Click += SetupValidAirShipmentMenuItem_Click;
			setupValidSeaShipmentMenuItem.Click += SetupValidSeaShipmentMenuItem_Click;

			this.MenuItems.Add(new ZMenuItem("-"));
			this.MenuItems.Add(developerTestDataMenuItem);
		}

		#endregion

		#region Menu Item Handlers

		#region Developer Menu Item Handlers

		void SetupValidAirShipmentMenuItem_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException("Create Valid Air Shipment");
		}

		void SetupValidSeaShipmentMenuItem_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException("Create Valid Sea Shipment");
		}

		#endregion

		#region CMR

		#region Pre-Lodge

		const string CannotSendPreMessageWithdrawlText = "You cannot send a Pre-Lodge message after a declaration has been withdrawn. You must reset the declaration first (see Brokerage Menu).";
		const string CannotSendPreMessageMultiplesText = "You cannot send a Pre-Lodge message at this time, multiple entries exist on this job, please check the entry and message status of all entries on the enties tab.";
		const string CannotSendPreMessageWaitingText = "You cannot send a Pre-Lodge message while waiting for a response or if a successful declaration has been lodged. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";

		internal void SendPreLodgementMessage_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfNeeded())
			{
				SendPreLodgementMessage((ZMenuItem)sender);
			}
		}

		void SendPreLodgementMessage(ZMenuItem menu)
		{
			if (Declaration.IsImportCMR && (ConsolidatedDeclaration != null || !IsQueuedForConsolidation(menu)))
			{
				var manager = CreateMessageManager(CMRMessageTypes.PreLodge);
				if (Declaration.CustomsEntryHeaders.Count > 0 && !manager.CanSendOriginalMessage)
				{
					var entryStatus = Declaration.JE_EntryStatus;
					if (entryStatus == CMRImportEntryAdvice.Withdrawn.Code)
					{
						Globals.Message.ShowError(CannotSendPreMessageWithdrawlText, "Cannot Send Pre-Lodge, Declaration Withdrawn");
					}
					else if (entryStatus == CMRImportEntryAdvice.MultiStatus.Code)
					{
						Globals.Message.ShowError(CannotSendPreMessageMultiplesText, "Cannot Send Pre-Lodge, MultiStatus");
					}
					else
					{
						Globals.Message.ShowError(CannotSendPreMessageWaitingText, "Cannot Send Pre-Lodge");
					}
				}
				else
				{
					if (MessageController.NotifyUsersOfNotifications(manager.CheckBusinessObjectLevelValidationIfRequired(), Declaration))
					{
						var result = Declaration.SendMessageWithBondedWarehouseAutomation(() => manager.SendMessages(Declaration.MessageInitiator), MessageAction.Original, saveFactory: manager.Factory.Save, additionalBondedWarehouseRequirement: false, reportHasChanges: false);
						if (result)
						{
							ImportAggregateDeclarationIntoConsolidatedEntry(!Declaration.IsQueuedEntryLodgement);
							ShowQueuedEntryLodgementInfo();
						}
					}
				}
			}
		}

		void ShowQueuedEntryLodgementInfo()
		{
			if (Declaration.IsQueuedEntryLodgement)
			{
				Globals.Message.Show($"Original Entry message Generated and Queued to be sent by Service Tasks on: {Declaration.JE_EDITransmitDate.ToShortDateString()}");
			}
		}

		#endregion

		bool ShowDialogWhenStopSelectMenuInHoldAwaiting()
		{
			var isholding = false;
			if (ConsolidatedDeclaration != null)
			{
				isholding = ConsolidatedDeclaration.JobDeclarations.Cast<JobDeclaration>().Any(x => x.IsHolding);
				if (isholding)
				{
					Globals.Message.ShowWarning("This Consolidated Entry cannot be paid while its linked declarations have a Hold Awaiting status.");
				}
			}
			else
			{
				isholding = Declaration.IsHolding;
				if (isholding)
				{
					Globals.Message.ShowWarning(string.Format(CultureInfo.CurrentCulture, "This Customs Declaration cannot be paid while it has a Hold Awaiting status. {0}", Declaration.JE_EntryStatusDescription));
				}
			}
			return isholding;
		}

		#region Send Lodgement

		internal void SendLodgementMessageWithPay_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfNeeded())
			{
				SendLodgement((ZMenuItem)sender, CMRMessageTypes.LodgeWithPay, () => MessageController.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(Declaration, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false) == ContinueWithSave.Yes);
			}
		}

		internal void SendLodgementMessageWithoutPay_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfNeeded())
			{
				SendLodgement((ZMenuItem)sender, CMRMessageTypes.LodgeWithoutPay, () => MessageController.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(Declaration, Customs.Business.EntryMessageStatusFilterType.CanSendOriginal, false, false) == ContinueWithSave.Yes);
			}
		}

		internal void DequeueLodgementOrPaymentMessage_Click(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				if (Declaration.DequeueScheduledMessages())
				{
					Globals.Message.Show("Queued message is dequeued.");
				}
				else
				{
					Globals.Message.ShowError("No queued messages to dequeue.");
				}
			}
		}

		const string CannotSendMessageWithdrawlText = "You cannot send an original message after a declaration has been withdrawn. You must reset the declaration first (see Brokerage Menu).";
		const string CannotSendMessageMultiplesText = "You cannot send an original message at this time, multiple entries exist on this job, please check the entry and message status of all entries on the enties tab.";
		const string CannotSendMessageWaitingText = "You cannot send an original message while waiting for a response or if a successful declaration has been lodged. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";

		internal void SendLodgement(ZMenuItem menu, CMRMessageTypes messageType, Func<bool> answerQuestions)
		{
			if (!ShowDialogWhenStopSelectMenuInHoldAwaiting() && (ConsolidatedDeclaration != null || !IsQueuedForConsolidation(menu)) && Declaration.IsImportCMR)
			{
				var manager = CreateMessageManager(messageType);
				if (Declaration.CustomsEntryHeaders.Count > 0 && !manager.CanSendOriginalMessage)
				{
					if (Declaration.JE_EntryStatus == CMRImportEntryAdvice.Withdrawn.Code)
					{
						Globals.Message.ShowError(CannotSendMessageWithdrawlText, "Cannot Send Original, Declaration Withdrawn");
					}

					else if (Declaration.JE_EntryStatus == CMRImportEntryAdvice.MultiStatus.Code)
					{
						Globals.Message.ShowError(CannotSendMessageMultiplesText, "Cannot Send Original, MultiStatus");
					}
					else
					{
						Globals.Message.ShowError(CannotSendMessageWaitingText, "Cannot Send Original");
					}
				}
				else
				{
					if ((!RequiresPromptStatementPreferenceInheriting(Declaration) || PromptStatementPreferenceInheriting())
						&& (Declaration.IsBondedWarehousingDisabled || CheckRequiredFieldsForBondedWarehousingAreEntered(checkEntryDetails: Declaration.IsExWarehouse))
						&& MessageController.NotifyUsersOfNotifications(manager.CheckBusinessObjectLevelValidationIfRequired(), Declaration)
						&& answerQuestions.Invoke())
					{
						var result = Declaration.SendMessageWithBondedWarehouseAutomation(() => manager.SendMessages(Declaration.MessageInitiator), MessageAction.Original, saveFactory: manager.Factory.Save, additionalBondedWarehouseRequirement: true, reportHasChanges: false);
						if (result)
						{
							ImportAggregateDeclarationIntoConsolidatedEntry(!Declaration.IsQueuedEntryLodgement);
							ShowQueuedEntryLodgementInfo();
						}
					}
				}
			}

			bool RequiresPromptStatementPreferenceInheriting(JobDeclaration declaration)
			{
				return declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.InheritingPreferenceDetailsFromHeader);
			}

			bool PromptStatementPreferenceInheriting()
			{
				return Globals.Message.Show(
					message: "I CONFIRM THAT THE INVOICE HEADER PREFERENCE APPLIES TO ALL TARIFF LINES THAT HAVE NO SPECIFIC PREFERENCE SET.",
					caption: "Confirm Preference Inheriting",
					buttons: MessageBoxButtons.OKCancel,
					icon: MessageBoxIcon.Question
				) == DialogResult.OK;
			}
		}

		#endregion

		#region Send Payment

		internal void SendPaymentMessage_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfNeeded())
			{
				SendPaymentMessage((ZMenuItem)sender);
			}
		}

		void SendPaymentMessage(ZMenuItem menu)
		{
			if (!ShowDialogWhenStopSelectMenuInHoldAwaiting() && (ConsolidatedDeclaration != null || !IsQueuedForConsolidation(menu)))
			{
				if (Declaration.IsImportCMR)
				{
					if (!CMRImportMessageStatusList.IsAwaitingResponse(Declaration.JE_MessageStatus))
					{
						var payInfos = GetEFTPaymentInfos();
						if (payInfos.Count > 0)
						{
							if (ShowEFTPaymentFormAndSend(payInfos))
							{
								var messageManager = CreateMessageManager(CMRMessageTypes.Payment);
								messageManager.EFTPaymentInformations = payInfos;

								if (messageManager.SendMessages(Declaration.MessageInitiator))
								{
									ImportAggregateDeclarationIntoConsolidatedEntry(!Declaration.IsQueuedEntryPayment);

									try
									{
										messageManager.Factory.Save();

										if (Declaration.IsQueuedEntryPayment)
										{
											Globals.Message.Show($"Original Entry message Generated and Queued to be sent by Service Tasks on: {Declaration.EntryHeader.ScheduledPaymentDate.ToBestReadableDateTimeString()}");
										}
									}
									catch (ZSaveException ex)
									{
										ZExceptionReporting.HandleSaveException(ex);
									}
								}
							}
						}
						else
						{
							Globals.Message.ShowWarning("There are no entries you can make a payment for now. Please lodge an entry first.");
						}
					}
					else
					{
						Globals.Message.ShowWarning("The job is waiting for responses now. Please wait until response messages comes back");
					}
				}
			}
		}

		protected virtual EFTPaymentInformationCollection GetEFTPaymentInfos()
		{
			return new EFTPaymentInformationCollection(Declaration);
		}

		protected virtual bool ShowEFTPaymentFormAndSend(EFTPaymentInformationCollection payInfos)
		{
			using (var form = new EFTPaymentForm(payInfos))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				return form.IsOKToSend;
			}
		}

		#endregion

		#region Send Withdrawal

		const string NoEntryToSendWithdrawalMessage = "There is no entry you can send a withdrawal message for. You can only send an withdrawal message for an entry after you have sent an original message for the entry.";

		internal void SendWithdrawalMessage_Click(object sender, EventArgs e)
		{
			if (Declaration.IsImportCMR && !IsQueuedForConsolidation((ZMenuItem)sender) && CheckHasChangesAndMergeIfNeeded() && (Declaration.IsBondedWarehousingDisabled || CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false)))
			{
				MessageAttacheeSelectionCollection candidiateEntriesToSend = GetMessageAttacheeCollectionForWithdrawal();
				if (candidiateEntriesToSend.Count == 0)
				{
					Globals.Message.ShowError(NoEntryToSendWithdrawalMessage, "Cannot Send withdrawal");
				}
				else
				{
					IMDMultiMessageManager manager = CreateMessageManager(CMRMessageTypes.Withdrawal);

					if (MessageController.NotifyUsersOfNotifications(manager.CheckBusinessObjectLevelValidationIfRequired(), Declaration))
					{
						bool isOKToSend = candidiateEntriesToSend.Count == 1;
						if (candidiateEntriesToSend.Count > 1)
						{
							using (MessageSelectionForm form = new MessageSelectionForm(candidiateEntriesToSend))
							{
								if (!Globals.IsTest)
								{
									ZFormModaliser.ShowDialogWithoutDispose(form);
								}

								isOKToSend = form.IsOKToSend;
							}
						}

						if (isOKToSend)
						{
							var entriesToSend = candidiateEntriesToSend.GetMessageAttacheesToSendMessagesFor().OfType<CusEntryHeader>().ToArray();
							if (MessageController.GenerateWithdrawDecQuestionAndShowCPQAForm(Declaration, entriesToSend) == ContinueWithSave.Yes)
							{
								CMRAmendmentWithdrawalReason reason = GetAmendmentWithdrawalReason();
								manager.AmendmentWithdrawalReason = reason;
								if (MessageController.GetAmendmentWithdrawalReason(reason) == ContinueWithSave.Yes)
								{
									Declaration.SendMessageWithBondedWarehouseAutomation(() => manager.SendWithdrawalMessages(entriesToSend), MessageAction.Withdrawal, saveFactory: manager.Factory.Save, reportHasChanges: false);
								}
							}
						}
					}
				}
			}
		}

		protected virtual CMRAmendmentWithdrawalReason GetAmendmentWithdrawalReason()
		{
			return new CMRAmendmentWithdrawalReason();
		}

		#endregion

		#region Send Amendment

		const string NoEntryToSendAmendment = "No original declaration has been lodged, or there are messages waiting for a responses. You are unable to send an amendment message until a valid response to an original message is received. If a response has been received, exit the job, then re-open the job to refresh the status.";

		internal void SendAmendmentMessage_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfNeeded())
			{
				SendAmendmentMessage((ZMenuItem)sender);
			}
		}

		internal void SendAmendmentMessage(ZMenuItem menuItem)
		{
			if (Declaration.IsImportCMR && (ConsolidatedDeclaration != null || !IsQueuedForConsolidation(menuItem))
			&& (Declaration.IsBondedWarehousingDisabled || CheckRequiredFieldsForBondedWarehousingAreEntered(checkEntryDetails: Declaration.IsExWarehouse)))
			{
				var manager = CreateMessageManager(CMRMessageTypes.Amendment);

				if (Declaration.IsSAC)
				{
					Globals.Message.ShowError("You cannot amend a SAC declaration", "Cannot Amend a SAC");
				}
				else if (Declaration.JE_EntryStatus == CMRImportEntryAdvice.Withdrawn.Code)
				{
					Globals.Message.ShowError("You cannot amend a withdrawn declaration", "Cannot Amend a Withdrawn");
				}
				else
				{
					var candidiateEntriesToSend = GetMessageAttacheeCollectionForAmendment();

					if (candidiateEntriesToSend.Count == 0)
					{
						Globals.Message.ShowError(NoEntryToSendAmendment, "Cannot Send Amendment");
					}
					else if (MessageController.NotifyUsersOfNotifications(manager.CheckBusinessObjectLevelValidationIfRequired(), Declaration))
					{
						var isOKToSend = candidiateEntriesToSend.Count == 1;
						if (candidiateEntriesToSend.Count > 1)
						{
							using (var form = new MessageSelectionForm(candidiateEntriesToSend))
							{
								ZFormModaliser.ShowDialogWithoutDispose(form);
								isOKToSend = form.IsOKToSend;
							}
						}

						if (isOKToSend)
						{
							var entriesToSend = candidiateEntriesToSend.GetMessageAttacheesToSendMessagesFor().OfType<CusEntryHeader>().ToArray();
							if (SetupDeclarationQuestionAndReason(manager, entriesToSend))
							{
								Declaration.SendMessageWithBondedWarehouseAutomation(() => manager.SendMessagesFromAmendmentMenu(entriesToSend), MessageAction.Amendment, saveFactory: manager.Factory.Save, reportHasChanges: false);
								ImportAggregateDeclarationIntoConsolidatedEntry(true);
							}
						}
					}
				}
			}
		}

		bool SetupDeclarationQuestionAndReason(IMDMultiMessageManager manager, CusEntryHeader[] entriesToSend)
		{
			var result = ConsolidatedDeclaration == null ? MessageController.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(Declaration, entriesToSend) == ContinueWithSave.Yes : AnswerConsolidatedDeclarationQuestions(false);
			if (result)
			{
				var reason = GetAmendmentWithdrawalReason();
				reason.ReasonText = Declaration.OutstandingAmendmentLogManger.AllOustandingAmendmentReferences;
				manager.AmendmentWithdrawalReason = reason;
				result = MessageController.GetAmendmentWithdrawalReason(reason) == ContinueWithSave.Yes;
			}
			return result;
		}

		protected internal virtual MessageAttacheeSelectionCollection GetMessageAttacheeCollectionForAmendment()
		{
			return new MessageAttacheeSelectionCollection(Declaration, MessageAttacheeMessageType.Amend);
		}

		protected internal virtual MessageAttacheeSelectionCollection GetMessageAttacheeCollectionForWithdrawal()
		{
			return new MessageAttacheeSelectionCollection(Declaration, MessageAttacheeMessageType.Withdraw);
		}

		#endregion

		void MessagingHelp_Click(object sender, EventArgs e)
		{
			WebUrlLauncher.Launch(CMRMessage.MessagingHelpUpdateNoteURL);
		}

#if DEBUG
		protected internal virtual
#endif
		IMDMultiMessageManager CreateMessageManager(CMRMessageTypes messageType)
		{
			return new IMDMultiMessageManager(Declaration, messageType);
		}

		#region Lodgement Questions

		void DoCommunityProtectionDeclarationForCMR_Click(object sender, EventArgs e)
		{
			MessageController.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(Declaration, Customs.Business.EntryMessageStatusFilterType.All, false, true);
		}

		internal void DoCommunityProtectionDeclarationForCMRWithdrawal_Click(object sender, EventArgs e)
		{
			if (GetConfirmationForGenerateWithdrawDeclarationQuestions() == DialogResult.OK)
			{
				MessageController.GenerateWithdrawDecQuestionAndShowCPQAForm(Declaration);
			}
		}

		void RegenerateDeclarationQuestions_Click(object sender, EventArgs e)
		{
			if (GetConfirmationForRegenerateDeclarationQuestions() == DialogResult.OK)
			{
				MessageController.GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(Declaration, Customs.Business.EntryMessageStatusFilterType.All, true, true);
			}
		}

		#endregion

		#region Log Authority to pay

		internal void CheckInAuthorityToPayGiven_Click(object sender, EventArgs e)
		{
			StmALog liveAuthorityToPayLog = Declaration.LiveAuthorityToPayLog;
			if (liveAuthorityToPayLog != null)
			{
				Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("There already exists an EFT payment authority added by " + liveAuthorityToPayLog.SL_GS_NKUser + " at " + liveAuthorityToPayLog.SL_EventTime);
			}
			else if (GetConfirmationFromUsers("Add a Log EFP Payment Authority Given", "Has the Importer given you an authority to make an EFT payment for this entry? If yes, please click OK. The system will add a log under your name. Otherwise click 'Cancel'.") == DialogResult.OK)
			{
				Declaration.AddAuthorityToPayLog();
				Form.FireSaveButton();
			}
		}

		#endregion

		#region Drawback

		public const string CannotSendDrawbackMessageWaitingText = "You cannot send an Original Drawback message while waiting for a response or if a successful Drawback has been lodged. Exit the job, and then select the job again, to refresh the status information and see if a reply has been received.";

		void SendDrawbackMessage_Click(object sender, EventArgs e)
		{
			if (Declaration.IsDrawback)
			{
				DrawbackMultiMessageManager manager = new DrawbackMultiMessageManager(Declaration, CMRMessage.MessageSubTypes.Original);

				if (!manager.CanSendOriginalMessage)
				{
					Globals.Message.ShowError(CannotSendDrawbackMessageWaitingText, "Cannot Send Drawback Original");
				}
				else
				{
					if (DrawbackPayeeDeclarationYes())
					{
						SendDrawbackOff(manager);
					}
				}
			}
		}

		void SendDrawbackOff(DrawbackMultiMessageManager messageManager)
		{
			if (messageManager.SendMessage(Declaration.MessageInitiator))
			{
				Form.FireSaveButton();
			}
		}

		bool DrawbackPayeeDeclarationYes()
		{
			return Globals.Message.ShowConfirmation("I declare that the payee was the legal owner of the goods at the time of export or the payee was assigned the right to claim drawback of import duty paid on the goods.",
				"Drawback declaration", "yes", MessageBoxIcon.None) == DialogResult.OK;
		}

		void AttachExportDeclarationsToDrawback_Click(object sender, EventArgs e)
		{
			ExportDeclarationAttacher attacher = new ExportDeclarationAttacher(Declaration, Declaration.Lookups.ExportDeclarations);
			attacher.NameOfAnElementInDestinationCollection = "export declaration";
			attacher.UnableToAttachText = "as the invoice number is already attached.";
			attacher.Show((ZForm)GetMainMenu().GetForm());
		}

		void ResetDrawback_Click(object sender, EventArgs e)
		{
			if (Declaration.IsDrawback)
			{
				if (Env.Security.CustomsResetToOriginal.IsAllowed)
				{
					if (GetConfirmationForResettingDrawback() == DialogResult.OK)
					{
						DrawbackMessageManager manager = new DrawbackMessageManager(Declaration);
						manager.ResetToOriginal();
					}
				}
				else
				{
					Env.Security.ShowError(Env.Security.CustomsResetToOriginal);
				}
			}
		}

		DialogResult GetConfirmationForResettingDrawback()
		{
			string caption = "Reset Drawback?";
			string message = "Resetting the Drawback should only be done as a last resort as it may lead to you getting out of sync with Customs. " +
					"Are you sure you wish to continue?";
			return GetConfirmationFromUsers(caption, message);
		}

		void DoAnswerDrawbackDeclarationQuestions_Click(object sender, EventArgs e)
		{
			if (Declaration.IsDrawback)
			{
				Declaration.GenerateDrawbackQuestionsIfNecessary();
				ZFormModaliser.ShowDialogAndDispose(new CMRDrawbackDecsForm(Declaration));
			}
		}

		#endregion

		#endregion

		#region Exit 1

		#region Declaration

		#region Declaration Exports

		void SubmitExportDeclarationEventHandler(object sender, EventArgs e)
		{
			var readyForSend = Declaration.DeclarationExportCusEntryNumber == null ? CheckHasChangesAndMergeIfNeeded() : CheckHasChanges();
			if (readyForSend)
			{
				Declaration.SendExportDeclaration();
			}
		}

		void WithdrawDeclarationEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges() && Declaration.IsExport)
			{
				Declaration.WithdrawDeclaration();
			}
		}

		#endregion

		#region Other Customs Message

		void WarrelEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendWarrelMessage();
			}
		}

		void WithdrawWarrelEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendWarrelWithdrawl();
			}
		}

		void WarretEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendWarretMessage();
			}
		}

		void DeprecEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendDeprecMessage();
			}
		}

		void WithdrawDeprecEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendDeprecWithdrawl();
			}
		}

		void DeprelEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendDeprelMessage();
			}
		}

		void WithdrawDeprelEventHandler(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				Declaration.SendDeprelWithdrawl();
			}
		}

		#endregion

		#endregion

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		void CreateContingencyEventHandler(object sender, EventArgs e)
		{
			if (Declaration.IsImportCMR)
			{
				if (Declaration.IsMergeDone)
				{
					new CMRExportForm(MainForm, new DeclarationIMDExporter(Declaration)).Export();
				}
				else
				{
					Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("You cannot Create Contingency Data until the Declaration is Merged. Selecting Brokerage > Answer Declaration Questions will Merge the Declaration.");
				}
			}
			else if (Declaration.IsExport)
			{
				new CMRExportForm(MainForm, new DeclarationEDNExporter(Declaration)).Export();
			}
		}

		internal void ResetExportDeclaration_Click(object sender, EventArgs e)
		{
			if (Declaration.IsExport)
			{
				if (Declaration.IsEXPDeclaration && Declaration.DeclarationExportCusEntryNumber == null && !IsQueuedForConsolidation((ZMenuItem)sender))
				{
					ThrowAwayMergedLines();
				}
				else if (GetConfirmationForResettingExportDeclaration() == DialogResult.OK)
				{
					Declaration.ResetDeclaration();
				}
			}
		}

		void ResetQuarantineDeclaration_Click(object sender, EventArgs e)
		{
			if (Declaration.IsQuarantine)
			{
				if (GetConfirmationForResettingQuarantineDeclaration() == DialogResult.OK)
				{
					Declaration.ResetDeclaration();
				}
			}
		}

		void ChangeStatusToHoldAwaiting_Click(object sender, EventArgs e)
		{
			if (!Declaration.IsHolding)
			{
				if (Declaration.IsWaitingForExportResponse || CMRImportMessageStatusList.IsAwaitingResponse(Declaration.JE_MessageStatus))
				{
					Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("You can't set the declaration to this status because you are currently waiting for a message response.");
				}
				else
				{
					PopupStatusChangeForm statusChangeForm = new PopupStatusChangeForm();
					statusChangeForm.Show(Declaration, true);
				}
			}
			else
			{
				Declaration.RemoveHold();
			}
		}

		void ControlVisibilityOfHoldAwaitingMenuItems()
		{
			bool isHolding = Declaration.IsHolding;

			CMRSetStatusToHoldAwaitingMenuItem.Visible = !isHolding;
			CMRClearStatusToHoldAwaitingMenuItem.Visible = isHolding;
		}

		void ControlVisibilityOfCOLSMenu()
		{
			var colsIntegrationEnabled = QuarantineColsHeader.IsCOLSFunctionEnabled;
			CargoOnlineLodgementSystemMenuItem.Enabled = colsIntegrationEnabled;
			CargoOnlineLodgementSystemMenuItem.Visible &= colsIntegrationEnabled;
			COLSSeperator.Visible &= colsIntegrationEnabled;

			var quarantineColsHeader = Declaration.QuarantineCOLSHeader;
			var lrnHasValue = quarantineColsHeader != null && !quarantineColsHeader.LRN.IsEmpty;
			var allowSendAddAdditionalDocument = lrnHasValue && quarantineColsHeader.LRNStatus == COLSEntryStatusList.Codes.LrnActive;
			if (quarantineColsHeader != null && !quarantineColsHeader.IsAwaitingResponse)
			{
				COLSAddAttachmentMenuItem.Enabled = quarantineColsHeader.LRNStatus == COLSEntryStatusList.Codes.LrnActive && quarantineColsHeader.EDocPivotCollection.Count > 0;
				COLSMakeAnEnquiryMenuItem.Enabled = quarantineColsHeader.LRNStatus == COLSEntryStatusList.Codes.LrnInactive;
				COLSAddAdditionalDocumentMenuItem.Enabled = allowSendAddAdditionalDocument;
				COLSLodgementStatusMenuItem.Enabled = lrnHasValue;
				COLSSwitchAepLodgementMenuItem.Enabled = lrnHasValue;
				COLSReassessmentMenuItem.Enabled = quarantineColsHeader.LRNStatus == COLSEntryStatusList.Codes.LrnInactive;
				COLSNewLodgementMenuItem.Enabled = quarantineColsHeader.LRN.IsEmpty;
				COLSPaymentStatusMenuItem.Enabled = true;
			}
			else
			{
				CargoOnlineLodgementSystemMenuItem.Enabled &= quarantineColsHeader != null;

				COLSAddAttachmentMenuItem.Enabled = false;
				COLSAddAdditionalDocumentMenuItem.Enabled = allowSendAddAdditionalDocument;
				COLSLodgementStatusMenuItem.Enabled = false;
				COLSMakeAnEnquiryMenuItem.Enabled = false;
				COLSSwitchAepLodgementMenuItem.Enabled = false;
				COLSReassessmentMenuItem.Enabled = false;
				COLSNewLodgementMenuItem.Enabled = false;
				COLSPaymentStatusMenuItem.Enabled = false;
			}
		}

		void ChangeStatusToDeclarationWorkComplete_Click(object sender, EventArgs e)
		{
			if (!Declaration.IsDeclarationWorkFinished)
			{
				PopupStatusChangeForm statusChangeForm = new PopupStatusChangeForm();
				statusChangeForm.Show(Declaration, false);
			}
			else
			{
				Declaration.RemoveDeclarationWorkComplete();
			}
		}

		void ControlVisibilityOfDeclarationWorkCompleteMenuItems()
		{
			bool isCompleted = Declaration.IsDeclarationWorkFinished;

			CMRSetStatusToDeclarationWorkCompleteMenuItem.Visible = !isCompleted;
			CMRClearStatusToDeclarationWorkCompleteMenuItem.Visible = isCompleted;
		}

		internal void ThrowAwayMergedLines_Click(object sender, EventArgs e)
		{
			if (Declaration.IsImport && !IsQueuedForConsolidation((ZMenuItem)sender))
			{
				ThrowAwayMergedLines();
			}
		}

		void ThrowAwayMergedLines()
		{
			if (Env.Security.CustomsResetToOriginal.IsAllowed)
			{
				if (Declaration.IsMergeDone)
				{
					var shouldSaveData = false;
					if ((Declaration.CanThrowAwayMerge ? GetConfirmationForThrowingAwayMerge() == DialogResult.OK : GetConfirmationForThrowingAwayMerge() == DialogResult.OK && GetConfirmationForThrowingAwayMergeAfterLodged() == DialogResult.OK) && CancelWHSTransactionIfNeeded(out shouldSaveData))
					{
						Cursor oldCursor = Cursor.Current;
						Cursor.Current = Cursors.WaitCursor;
						try
						{
							Declaration.ThrowAwayMerge();
							if (shouldSaveData)
							{
								Form.FireSaveButton();
							}
						}
						finally
						{
							Cursor.Current = oldCursor;
						}
					}
				}
				else
				{
					Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation("You have no merged lines to throw away.");
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsResetToOriginal);
			}
		}

		bool CancelWHSTransactionIfNeeded(out bool shouldSaveData)
		{
			shouldSaveData = false;
			var isOkToContinue = true;
			if (Declaration.IsWHSUniversalXMLActive && Declaration.HasWHSTransaction && Declaration.SupportsBondedWarehousing)
			{
				if (Globals.Message.ShowConfirmation("Resetting a declaration will result in existing WHS transactions being canceled.\r\nAre you certain you want to continue?", "WHS Transaction Exists", "yes", MessageBoxIcon.Warning) == DialogResult.OK)
				{
					if (TopLevelBusinessObject.HasChanges && Form.FireSaveButton() == ContinueWithSave.No)
					{
						isOkToContinue = false;
					}

					if (isOkToContinue)
					{
						shouldSaveData = true;
						var eventResponse = Customs.Business.WarehouseTransactionStatusList.IsInwardCode(Declaration.WarehouseTransactionStatus) ? Declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded() : Declaration.PublishCancelEventForWHSOutwardAndSaveIfNeeded();
						isOkToContinue = eventResponse.ResultType != UniversalResult.HadErrors;
						if (!isOkToContinue)
						{
							Globals.Message.Show(string.Format("Could not cancel WHS transactions due to the following error:\r\n{0}", eventResponse.ErrorMessage));
						}
					}
				}
				else
				{
					isOkToContinue = false;
				}
			}
			return isOkToContinue;
		}

		#endregion

		#region Data Transfer
		protected override DataTransferImpl GetDataTransferImpl()
		{
			return new AUDataTransfer();
		}
		#endregion

		protected override Customs.GUI.BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			var declaration = supporter as JobDeclaration;
			if (declaration == null)
			{
				ErrorReporter.ReportOnce("For AU, supporter must be JobDeclaration");
				return null;
			}
			else
			{
				return new BondedWarehouseOperationDeterminer(declaration);
			}
		}

		#endregion

		#region Dialog Confirmations

		DialogResult GetConfirmationForGenerateWithdrawDeclarationQuestions()
		{
			const string message = "This will delete existing lodgement & CP questions and answers. Are you sure you wish to continue?";
			return GetConfirmationFromUsers("Lodgement & CP Questions for Withdrawal", message);
		}

		DialogResult GetConfirmationForRegenerateDeclarationQuestions()
		{
			const string message = "System will regenerate lodgement & CP questions and might delete existing questions and answers. Are you sure you wish to continue?";
			return GetConfirmationFromUsers("Regenerate Lodgement & CP Questions", message);
		}

		DialogResult GetConfirmationForResettingExportDeclaration()
		{
			const string message = "Resetting the declaration should only be done as a last resort as it may lead to you getting out of sync with Customs. Are you sure you wish to continue?";
			return GetConfirmationFromUsers("Reset Declaration?", message);
		}

		DialogResult GetConfirmationForResettingQuarantineDeclaration()
		{
			const string message = "Resetting the declaration should only be done as a last resort as it may lead to you getting out of sync with Quarantine. Are you sure you wish to continue?";
			return GetConfirmationFromUsers("Reset Quarantine Declaration?", message);
		}

		DialogResult GetConfirmationForThrowingAwayMerge()
		{
			const string message = "Throwing away the Merged Lines in a Declaration will result in all CP Declarations and Messages sent to Customs being discarded.\r\nAre you certain you want to continue?";
			return GetConfirmationFromUsers("Throw Away Merge?", message);
		}

		protected internal DialogResult GetConfirmationForThrowingAwayMergeAfterLodged()
		{
			string message = "WARNING: This Declaration already has a Lodged Entry. There is no reason to use the Reset Declaration option for a lodged entry declaration.\r\n";

			if (!Declaration.IsImportCMR)
			{
				message += "Amendment should be made to the entry via a Post Warrant Amendment in the Compile system.\r\n";
			}

			message += "Failure to manage this properly may result in a duplication of lodged/paid entries with Customs and may result in Customs penalties.\r\nAre you certain you want to continue?";

			return GetConfirmationFromUsers("Throw Away Merge?", message);
		}

		#endregion

		#region Consolidated Declaration

		ZMenuItem IConsolidatedDeclarationMenuBuilder.BuildMenu()
		{
			SetupConsolidatedDeclarationMenu();

			var result = new ZMenuItem(Text);
			result.Popup += delegate
			{ RefreshConsolidatedDeclarationMenu(); };
			result.MenuItems.Add(answerConsolidatedDeclarationQuestionsMenuItem);
			result.MenuItems.Add(regenerateConsolidatedDeclarationQuestionsMenuItem);
			result.MenuItems.Add("-");
			result.MenuItems.Add(sendConsolidatedPreLodgeMessageMenuItem);
			result.MenuItems.Add("-");
			result.MenuItems.Add(sendConsolidatedLodgeWithPayMessageMenuItem);
			result.MenuItems.Add(sendConsolidatedLodgeWithoutPayMessageMenuItem);
			result.MenuItems.Add(sendConsolidatedPaymentMessageMenuItem);
			result.MenuItems.Add(dequeueConsolidatedLodgementOrPaymentMessageMenuItem);
			result.MenuItems.Add("-");
			result.MenuItems.Add(sendConsolidatedAmendmentMessageMenuItem);
			result.MenuItems.Add("-");
			result.MenuItems.Add(answerConsolidatedDeclarationWithdrawQuestionsMenuItem);
			result.MenuItems.Add(sendConsolidatedWithdrawMessageMenuItem);

			return result;
		}

		protected ZMenuItem answerConsolidatedDeclarationQuestionsMenuItem;
		protected ZMenuItem regenerateConsolidatedDeclarationQuestionsMenuItem;
		protected ZMenuItem answerConsolidatedDeclarationWithdrawQuestionsMenuItem;
		protected ZMenuItem sendConsolidatedPreLodgeMessageMenuItem;
		protected ZMenuItem sendConsolidatedLodgeWithPayMessageMenuItem;
		protected ZMenuItem sendConsolidatedLodgeWithoutPayMessageMenuItem;
		protected ZMenuItem sendConsolidatedPaymentMessageMenuItem;
		protected ZMenuItem sendConsolidatedAmendmentMessageMenuItem;
		protected ZMenuItem sendConsolidatedWithdrawMessageMenuItem;
		protected ZMenuItem dequeueConsolidatedLodgementOrPaymentMessageMenuItem;

		internal void RefreshConsolidatedDeclarationMenu()
		{
			var consolidatedDeclarationNotNull = ConsolidatedDeclaration != null;
			var leadDeclaration = (JobDeclaration)ConsolidatedDeclaration?.LeadDeclaration;
			var isQueuedEntriesFunctionEnabled = leadDeclaration != null && leadDeclaration.IsQueuedEntriesFunctionEnabled;
			var isQueuedForLodgementOrPayment = leadDeclaration != null && IsQueuedForLodgementOrPayment(leadDeclaration);

			answerConsolidatedDeclarationQuestionsMenuItem.Enabled = consolidatedDeclarationNotNull;
			regenerateConsolidatedDeclarationQuestionsMenuItem.Enabled = consolidatedDeclarationNotNull;
			sendConsolidatedPreLodgeMessageMenuItem.Enabled = consolidatedDeclarationNotNull && !isQueuedForLodgementOrPayment;
			sendConsolidatedLodgeWithPayMessageMenuItem.Enabled = consolidatedDeclarationNotNull && !isQueuedForLodgementOrPayment;
			sendConsolidatedLodgeWithoutPayMessageMenuItem.Enabled = consolidatedDeclarationNotNull && !isQueuedForLodgementOrPayment;
			sendConsolidatedPaymentMessageMenuItem.Enabled = consolidatedDeclarationNotNull && !isQueuedForLodgementOrPayment;
			sendConsolidatedAmendmentMessageMenuItem.Enabled = consolidatedDeclarationNotNull && !isQueuedForLodgementOrPayment;
			answerConsolidatedDeclarationWithdrawQuestionsMenuItem.Enabled = consolidatedDeclarationNotNull;
			sendConsolidatedWithdrawMessageMenuItem.Enabled = consolidatedDeclarationNotNull && !isQueuedForLodgementOrPayment;

			dequeueConsolidatedLodgementOrPaymentMessageMenuItem.Visible = isQueuedEntriesFunctionEnabled || isQueuedForLodgementOrPayment;
			dequeueConsolidatedLodgementOrPaymentMessageMenuItem.Enabled = isQueuedForLodgementOrPayment;
		}

		void SetupConsolidatedDeclarationMenu()
		{
			if (answerConsolidatedDeclarationQuestionsMenuItem == null)
			{
				answerConsolidatedDeclarationQuestionsMenuItem = new ZMenuItem("Answer Declaration &Questions");
				answerConsolidatedDeclarationQuestionsMenuItem.Click += AnswerConsolidatedDeclarationQuestionsMenuItem_Click;
			}
			if (regenerateConsolidatedDeclarationQuestionsMenuItem == null)
			{
				regenerateConsolidatedDeclarationQuestionsMenuItem = new ZMenuItem("&Regenerate and Answer Declaration Questions");
				regenerateConsolidatedDeclarationQuestionsMenuItem.Click += RegenerateConsolidatedDeclarationQuestionsMenuItem_Click;
			}
			if (answerConsolidatedDeclarationWithdrawQuestionsMenuItem == null)
			{
				answerConsolidatedDeclarationWithdrawQuestionsMenuItem = new ZMenuItem("Answer Declaration Questions for Withdrawal");
				answerConsolidatedDeclarationWithdrawQuestionsMenuItem.Click += AnswerConsolidatedDeclarationWithdrawQuestionsMenuItem_Click;
			}
			if (sendConsolidatedPreLodgeMessageMenuItem == null)
			{
				sendConsolidatedPreLodgeMessageMenuItem = new ZMenuItem("Send PreLodgement Message");
				sendConsolidatedPreLodgeMessageMenuItem.Click += sendConsolidatedPreLodgeMessageMenuItem_Click;
			}
			if (sendConsolidatedLodgeWithPayMessageMenuItem == null)
			{
				sendConsolidatedLodgeWithPayMessageMenuItem = new ZMenuItem("Send Lodgement Message WITH Payment Approved");
				sendConsolidatedLodgeWithPayMessageMenuItem.Click += sendConsolidatedLodgeWithPayMessageMenuItem_Click;
			}
			if (sendConsolidatedLodgeWithoutPayMessageMenuItem == null)
			{
				sendConsolidatedLodgeWithoutPayMessageMenuItem = new ZMenuItem("Send Lodgement Message WITHOUT Payment Approved");
				sendConsolidatedLodgeWithoutPayMessageMenuItem.Click += sendConsolidatedLodgeWithoutPayMessageMenuItem_Click;
			}
			if (sendConsolidatedPaymentMessageMenuItem == null)
			{
				sendConsolidatedPaymentMessageMenuItem = new ZMenuItem("Send Payment Message");
				sendConsolidatedPaymentMessageMenuItem.Click += sendConsolidatedPaymentMessageMenuItem_Click;
			}
			if (sendConsolidatedAmendmentMessageMenuItem == null)
			{
				sendConsolidatedAmendmentMessageMenuItem = new ZMenuItem("Send Amendment Message");
				sendConsolidatedAmendmentMessageMenuItem.Click += sendConsolidatedAmendmentMessageMenuItem_Click;
			}
			if (sendConsolidatedWithdrawMessageMenuItem == null)
			{
				sendConsolidatedWithdrawMessageMenuItem = new ZMenuItem("Send Withdrawal Message");
				sendConsolidatedWithdrawMessageMenuItem.Click += sendConsolidatedWithdrawMessageMenuItem_Click;
			}
			if (dequeueConsolidatedLodgementOrPaymentMessageMenuItem == null)
			{
				dequeueConsolidatedLodgementOrPaymentMessageMenuItem = new ZMenuItem("Dequeue Scheduled Lodgement or Payment");
				dequeueConsolidatedLodgementOrPaymentMessageMenuItem.Click += dequeueConsolidatedLodgementOrPaymentMessageMenuItem_Click;
			}
		}

		void dequeueConsolidatedLodgementOrPaymentMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration is ConsolidatedDeclaration auConsolidatedDeclaration)
			{
				if (!auConsolidatedDeclaration.HasChanges)
				{
					auConsolidatedDeclaration.DequeueScheduledMessages();
					Globals.Message.ShowInformation("Queued message is dequeued.", "Dequeue Scheduled Message");
				}
				else
				{
					Globals.Message.ShowError("The Consolidated Declaration has unsaved changes.", "Dequeue Scheduled Message");
				}
			}
		}

		void sendConsolidatedPreLodgeMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
			{
				SendPreLodgementMessage((ZMenuItem)sender);
			}
		}

		void sendConsolidatedLodgeWithPayMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
			{
				SendLodgement((ZMenuItem)sender, CMRMessageTypes.LodgeWithPay, () => AnswerConsolidatedDeclarationQuestions(false));
			}
		}

		void sendConsolidatedLodgeWithoutPayMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
			{
				SendLodgement((ZMenuItem)sender, CMRMessageTypes.LodgeWithoutPay, () => AnswerConsolidatedDeclarationQuestions(false));
			}
		}

		void sendConsolidatedPaymentMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender, recalculateFees: false))
			{
				SendPaymentMessage((ZMenuItem)sender);
			}
		}

		void sendConsolidatedAmendmentMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
			{
				SendAmendmentMessage((ZMenuItem)sender);
				RefreshConsolidatedDeclarationMenu();
			}
		}

		void sendConsolidatedWithdrawMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
			{
				SendWithdrawalMessageForConsolidatedEntry((ZMenuItem)sender);
			}
		}

		void SendWithdrawalMessageForConsolidatedEntry(ZMenuItem menu)
		{
			if (Declaration.IsImportCMR &&
			(Declaration.IsBondedWarehousingDisabled || CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false)))
			{
				var aggregateEntryHeader = Declaration.EntryHeader;
				if (!((IMessageAttachee)aggregateEntryHeader).IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw))
				{
					Globals.Message.ShowError(NoEntryToSendWithdrawalMessage, "Cannot Send withdrawal");
				}
				else
				{
					var manager = CreateMessageManager(CMRMessageTypes.Withdrawal);

					if (MessageController.NotifyUsersOfNotifications(manager.CheckBusinessObjectLevelValidationIfRequired(), Declaration))
					{
						GenerateWithdrawQuestions();
						if (AnswerConsolidatedDeclarationQuestions(true, generateQuestions: false))
						{
							var reason = GetAmendmentWithdrawalReason();
							manager.AmendmentWithdrawalReason = reason;
							if (MessageController.GetAmendmentWithdrawalReason(reason) == ContinueWithSave.Yes)
							{
								if (Declaration.SendMessageWithBondedWarehouseAutomation(() => manager.SendWithdrawalMessages(new[] { aggregateEntryHeader }), MessageAction.Withdrawal, saveFactory: manager.Factory.Save, reportHasChanges: false))
								{
									ImportAggregateDeclarationIntoConsolidatedEntry(true);
								}
							}
						}
					}
				}
			}
		}

		void AnswerConsolidatedDeclarationQuestionsMenuItem_Click(object sender, EventArgs e)
		{
			if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
			{
				AnswerConsolidatedDeclarationQuestions(showCPQAFormAlways: true, generateQuestions: ConsolidatedDeclaration.Questions.Count == 0);
			}
		}

		void RegenerateConsolidatedDeclarationQuestionsMenuItem_Click(object sender, EventArgs e)
		{
			if (GetConfirmationForRegenerateDeclarationQuestions() == DialogResult.OK)
			{
				if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
				{
					AnswerConsolidatedDeclarationQuestions(true);
				}
			}
		}

		void AnswerConsolidatedDeclarationWithdrawQuestionsMenuItem_Click(object sender, EventArgs e)
		{
			if (IsReadyForWithdrawQuestions())
			{
				if (GetConfirmationForGenerateWithdrawDeclarationQuestions() == DialogResult.OK)
				{
					if (ConsolidatedDeclaration != null && ValidateAndSetupAggregateDeclaration((ZMenuItem)sender))
					{
						GenerateWithdrawQuestions();
						AnswerConsolidatedDeclarationQuestions(true, generateQuestions: false);
					}
				}
			}
		}

		bool IsReadyForWithdrawQuestions()
		{
			var leadDeclaration = ConsolidatedDeclaration?.LeadDeclaration as JobDeclaration;
			var messageAttachee = leadDeclaration?.EntryHeader as IMessageAttachee;
			var result = messageAttachee?.IsValidToSendThisMessageType(MessageAttacheeMessageType.Withdraw) ?? false;
			if (!result)
			{
				Globals.Message.ShowError(CannotAnswerWithdrawQuestionsText, "Cannot answer withdraw questions");
			}

			return result;
		}

		const string CannotAnswerWithdrawQuestionsText = "There is no entry valid for answering withdraw questions. You can only answer withdraw questions for an entry after you have sent an original message for the entry.";

		void GenerateOriginalOrAmendmentQuestions()
		{
			Declaration.CPQAManager.GenerateQuestionsForConsolidatedEntryOriginalOrAmendment();
		}

		void GenerateWithdrawQuestions()
		{
			Declaration.CPQAManager.GenerateQuestionsForConsolidatedEntryWithdrawal();
		}

		bool AnswerConsolidatedDeclarationQuestions(bool showCPQAFormAlways, bool generateQuestions = true)
		{
			var isReadyToProceed = false;

			if (ConsolidatedDeclaration != null)
			{
				if (generateQuestions)
				{
					GenerateOriginalOrAmendmentQuestions();
				}

				if (MessageController.ShowCPQAFormForConsolidatedDeclaration(ConsolidatedDeclaration, Declaration, showCPQAFormAlways) == ContinueWithSave.Yes)
				{
					try
					{
						ConsolidatedDeclaration.Factory.Save();
						isReadyToProceed = true;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(e);
						return false;
					}
				}
			}

			return isReadyToProceed;
		}

		bool ValidateAndSetupAggregateDeclaration(ZMenuItem menu, bool recalculateFees = true)
		{
			if (SaveDataFirst.Confirm(ConsolidatedDeclaration, Form))
			{
				if (CheckConsolidatedDeclarationForOtherUsers_OKToProceed(menu))
				{
					if (recalculateFees)
					{
						try
						{
							ConsolidatedDeclaration.CalculateHeaderFees();
							ConsolidatedDeclaration.Factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
							return false;
						}
					}

					Declaration = (JobDeclaration)ConsolidatedDeclaration.BuildAggregateJobDeclaration();
					return true;
				}
			}

			return false;
		}

		void ImportAggregateDeclarationIntoConsolidatedEntry(bool deriveConsolidatedStatus)
		{
			if (ConsolidatedDeclaration != null)
			{
				ConsolidatedDeclaration.DeriveConsolidatedStatusAndImportAggregateDeclaration(Declaration, deriveConsolidatedStatus);

				var leadDeclaration = (JobDeclaration)ConsolidatedDeclaration.LeadDeclaration;
				leadDeclaration.SetMessageStatusDescription();
				leadDeclaration.RefreshBinding();
			}
		}

		public ConsolidatedDeclaration ConsolidatedDeclaration
		{
			get => fConsolidatedDeclaration;
			set => fConsolidatedDeclaration = value;
		}
		ConsolidatedDeclaration fConsolidatedDeclaration;

		Customs.Business.ConsolidatedDeclaration IConsolidatedDeclarationMenuBuilder.ConsolidatedDeclaration
		{
			get => ConsolidatedDeclaration;
			set => ConsolidatedDeclaration = (ConsolidatedDeclaration)value;
		}

		#endregion

		#endregion

		internal DataTransferImpl DataTransferImplInternal => DataTransferImpl;

		internal MenuItem bondedWarehouseMenuItemInternal => bondedWarehouseMenuItem;
	}
}
