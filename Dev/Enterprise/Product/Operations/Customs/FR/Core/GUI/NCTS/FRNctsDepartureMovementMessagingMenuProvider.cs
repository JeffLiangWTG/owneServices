using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.ServiceTask;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class FRNctsDepartureMovementMessagingMenuProvider : EU.NCTS.GUI.NctsDepartureMovementMessagingMenuProvider
	{
		public FRNctsDepartureMovementMessagingMenuProvider(NctsHeader header, EU.NCTS.DataTransfer.NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
			this.header = header;
		}

		public FRNctsDepartureMovementMessagingMenuProvider(NctsHeader header, NctsMovementForm nctsMovementForm) : base(header)
		{
			this.header = header;
			ParentForm = nctsMovementForm;
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			return base.CreateMenuItems().Concat(new[]
			{
				replyToEnquiryMenuItem = new ZMenuItem(LabelMenuReplyToEnquiry, SendReplyToEnquiry_Click),
				portMessagingMenuItem = GetPortMessagingMenuItem(),
				writeOffMenuItem = new ZMenuItem(ResString.GetMultilingualString("3B4A41D2-428E-4B53-9098-BE25003D42D3", "Write Off"), WriteOffClick)
			});
		}

		ZMenuItem replyToEnquiryMenuItem;
		ZMenuItem portMessagingMenuItem;
		ZMenuItem writeOffMenuItem;

		ZMenuItem GetPortMessagingMenuItem()
		{
			var menuItem = new ZMenuItem(ResString.GetMultilingualString("10CA6E06-0630-4D3E-92B7-CBB76556EE63", "Port Messaging"));
			menuItem.AddFormsMenuItems(header, ModuleIDs.Customs.EU.NctsMovementModule, CreateRegularizationMessagingMenuItemInfos());

			return menuItem;
		}

		IEnumerable<IMenuItemInfo> CreateRegularizationMessagingMenuItemInfos()
		{
			yield return new SystemMenuItemInfo
			{
				ID = new ZGuid("68d625be-adb9-4c38-8978-640f8537869c")  // PK of DOA document menu
			};

			yield return new SystemMenuItemInfo
			{
				ID = new ZGuid("9912d44a-84fc-451e-877e-bd97a32343d6")  // PK of CAED document menu
			};
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility(replyToEnquiryMenuItem, () => header.IsQueried);
			SetMenuItemVisibility(portMessagingMenuItem, () => true);
			SetMenuItemVisibility(writeOffMenuItem, () => IsWriteOffAllowed());
		}

		void SendReplyToEnquiry_Click(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(
				   delegate
				   {
					   if (this.header.IsDepartureTabReadOnly)
					   {
						   if (Globals.Message.ShowConfirmation(
							ResString.GetMultilingualString("82BDF0A7-CB75-4040-AC1E-D8E249BE9EA2", "Are you sure you want to resend this message?"),
							ResString.GetMultilingualString("F4F0E5E6-03E4-4E1A-979C-C302B9A99648", "Resend Message"),
							ResString.GetMultilingualString("E84090B6-5A6F-461E-AF51-D047E2261587", "yes"),
							MessageBoxIcon.Question) != DialogResult.OK)
						   {
							   return;
						   }
					   }
					   SendMessageToNcts(new NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage());
				   });
		}

		protected bool IsWriteOffAllowed()
		{
			return Env.Security.FRNctsManualWriteOff.IsAllowed
				&& header.BH_HeaderType.In(new ZString[] { EU.NCTS.Business.NctsMovementType.Codes.Departure, EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival })
				&& header.MovementHeader.BM_CustomsStatus.Equals(NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture);
		}

		void WriteOffClick(object sender, EventArgs e)
		{
			var menuItem = sender as ZMenuItem;
			if (SaveAndContinue(menuItem))
			{
				var result = Globals.Message.ShowConfirmation(Res.GetString("3D262C67-D004-4A64-A5DE-8523D55354E5", "Please confirm by typing \"Yes\" if you wish to manually close this job. By doing that, the departure status will be set to AWO (Goods Written-off) and guarantees will be written off (if any)."), Res.GetString("793B696F-338B-4CAD-995F-869C1AA67A9D", "Manually Write Off"), Res.GetString("07082DEF-38BA-4E83-B57C-FF80E15028EA", "Yes"), MessageBoxIcon.Information);
				if (result == DialogResult.OK)
				{
					header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
					var errorMessageList = new List<ZString>();
					DTCC045AProcessor.UpdateGuaranteeTransactionsIfNeeded(header, errorMessageList);
					header.Factory.Save();

					if (errorMessageList.Count == 0)
					{
						Globals.Message.Show(Res.GetString("a0f854d3-089e-4763-ad9e-fae5a2403b96", "Success!"));
					}
					else
					{
						Globals.Message.ShowWarning(ZString.Join("\n", errorMessageList.ToArray()));
					}
				}
			}
		}

		protected override ResourceString SendDepartureMessageMenuItemCaption
		{
			get
			{
				var result = base.SendDepartureMessageMenuItemCaption;
				if (FRCustomsDataRegistry.DeltaTFallbackIsActive)
				{
					result = ResString.GetMultilingualString("C3C76CFC-550F-41B6-88F1-109B854E001D", "Create Fallback TAD Document");
				}
				return result;
			}
		}

		protected override bool IsSendDepartureDeclarationMenuAllowed()
		{
			var departureStatus = header.MovementHeader.BM_CustomsStatus;
			return base.IsSendDepartureDeclarationMenuAllowed()
				|| departureStatus == NctsTransitStatusList.Codes.ReadyForAmendment;
		}

		protected override void SendDepartureMessasgeClickCore(object sender)
		{
			var menu = sender as ZMenuItem;
			if (SaveAndContinue(menu))
			{
				if (FRCustomsDataRegistry.DeltaTFallbackIsActive)
				{
					new EU.NCTS.Business.NctsTadEdocSaver(Header).RenderTadAndStoreInEdocs(Header);
					Header.Factory.Save();
				}
				else
				{
					var continueWithSend = true;
					if (header.DeltaTFallbackAnnouncedButNotActive)
					{
						continueWithSend = Globals.Message.Show(Res.GetString("F9B34DF5-E95B-4637-967C-2826F1358A39", "Delta T fallback has been announced but you have not yet activated it in the CW1 registry.  You are advised to view this eLearning material and activate Delta T fallback as appropriate"),
						Res.GetString("8A9841843-FB7F-430A-8B9E-971F4090FDC8", "Continue with send?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
					}

					if (continueWithSend)
					{
						if (header.MovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.ReadyForAmendment)
						{
							SendAmendmentMessage();
						}
						else if ((header.MovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.DeclarationAccepted || header.DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.AmendmentAccepted || header.DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.AmendmentRefused) && header.IsPrelodgedMovement)
						{
							SendPrelodgeValidationMessage();
						}
						else
						{
							base.SendDepartureMessasgeClickCore(sender);
						}
					}
				}
			}
		}

		void SendAmendmentMessage()
		{
			var amendmentWithdrawalReasonAndComment = new AmendmentWithdrawalReasonAndComment();

			if (ZFormModaliser.ShowDialogAndDispose(new AmendmentReasonAndCommentForm(amendmentWithdrawalReasonAndComment)) == DialogResult.OK)
			{
				SendMessageToNcts(new NctsMessageFunctionSet.DeclarationAmendmentMessage(amendmentWithdrawalReasonAndComment.ReasonText, amendmentWithdrawalReasonAndComment.CommentText));
			}
		}

		void SendPrelodgeValidationMessage()
		{
			SendMessageToNcts(new FRNctsMessageFunctionSet.PrelodgeValidationMessage());
		}

		protected override void RequestToCancelClickCore()
		{
			var amendmentWithdrawalReasonAndComment = new AmendmentWithdrawalReasonAndComment();

			if (ZFormModaliser.ShowDialogAndDispose(new AmendmentReasonAndCommentForm(amendmentWithdrawalReasonAndComment)) == DialogResult.OK)
			{
				header.ExplanationToCustomsForWhyCancelling = amendmentWithdrawalReasonAndComment.ReasonText;
				SendMessageToNcts(new NctsMessageFunctionSet.DeclarationCancellationRequestMessage(amendmentWithdrawalReasonAndComment.ReasonText, amendmentWithdrawalReasonAndComment.CommentText));
			}
		}

		public static ZString LabelMenuReplyToEnquiry => ResString.GetMultilingualString("A4F439E6-B092-4CE1-AA71-824F52508B93", "Reply to Enquiry");

		readonly NctsHeader header;
	}
}
