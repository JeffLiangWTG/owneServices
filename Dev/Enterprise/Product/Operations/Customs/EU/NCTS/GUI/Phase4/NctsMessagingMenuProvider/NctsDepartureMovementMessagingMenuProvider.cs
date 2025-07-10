using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsDepartureMovementMessagingMenuProvider : NctsMessagingMenuProvider
	{
		public NctsDepartureMovementMessagingMenuProvider(NctsHeader header)
			: base(header)
		{
		}

		public NctsDepartureMovementMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			yield return sendDepartureMessageMenuItem = new ZMenuItem(SendDepartureMessageMenuItemCaption, SendDepartureMessageClick);
			if (Header.IsDepartureDestinationOfficeInNctsContractingCountry)
			{
				yield return makeArrivalNotificationFromDepartureMenuItem = new ZMenuItem(ResString.GetMultilingualString("12345678-C73C-4BAD-85FE-70E7F57DD494", "Make &Arrival Notification for this Departure"), MakeArrivalNotificationFromDepartureClick);
			}
			yield return requestToCancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("87654321-C73C-4BAD-85FE-70E7F57DD494", "Request &Cancellation"), RequestToCancelClick);
			yield return amendDepartureMenuItem = new ZMenuItem(ResString.GetMultilingualString("500C8717-5739-40B0-A879-9F1E30C7B07A", "Re-open Declaration for &Amendment"), AmendDepartureClick);
			yield return importCustomsEntryLinesMenuItem = new ZMenuItem(ResString.GetMultilingualString("49FE98EC-E5DD-4558-A65D-FDAB479C3CD0", "Import Customs Entry Lines"), ImportCustomsEntryLinesMenuItem_Click);
		}

		protected virtual ResourceString SendDepartureMessageMenuItemCaption => ResString.GetMultilingualString("A3C7A476-5C68-4049-8AD8-A350116B5370", "Send &Departure Message");

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility(sendDepartureMessageMenuItem, IsSendDepartureDeclarationMenuAllowed);
			SetMenuItemVisibility(makeArrivalNotificationFromDepartureMenuItem, () => Header.MovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture);
			SetMenuItemVisibility(requestToCancelMenuItem, () => Header.IsDepartureCancellationAllowed);
			SetMenuItemVisibility(amendDepartureMenuItem, () => Header.IsDepartureAmendmentAllowed);
		}

		protected virtual bool IsSendDepartureDeclarationMenuAllowed()
		{
			var departureStatus = Header.MovementHeader.BM_CustomsStatus;
			return departureStatus == NctsTransitStatusList.Codes.Unknown
				|| departureStatus == NctsTransitStatusList.Codes.DeclarationRejected
				|| departureStatus == NctsTransitStatusList.Codes.DeclarationAccepted;
		}

		protected virtual void SendDepartureMessasgeClickCore(object sender)
		{
			if (Header.IsDepartureTabReadOnly)
			{
				if (Globals.Message.ShowConfirmation(
				ResString.GetMultilingualString("B0E64FB3-555E-4214-A44D-D70CA703BF14", "Are you sure you want to resend this message?"),
				ResString.GetMultilingualString("920DDF24-17B7-4FC4-A249-0D7C61059498", "Resend Message"),
				ResString.GetMultilingualString("F16AD969-4232-4789-8724-8A2D4C6C52FF", "yes"),
				 MessageBoxIcon.Question) != DialogResult.OK)
				{
					return;
				}
			}
			SendMessageToNcts(new NctsMessageFunctionSet.DeclarationDataMessage());
		}

		protected virtual void RequestToCancelClickCore()
		{
			var reasonCannotCancel = Header.ReasonForCannotRequestCancellation;
			if (!reasonCannotCancel.IsEmpty)
			{
				Globals.Message.ShowError(reasonCannotCancel, ResString.GetMultilingualString("482AB701-75BE-44DE-B5CE-11DA1DBD96C0", "Cannot cancel"));
			}
			else
			{
				var args = new UserResponseArgument()
				{
					Caption = ResString.GetMultilingualString("F4A18199-CE8D-4C05-9715-AA93C3274F55", "Cancellation"),
					Message = ResString.GetMultilingualString("497FE1CD-18F9-4717-A8C2-32B2DFD589FF", "Are you sure you wish to cancel?  If you are sure, supply\n\ra reason here and press Yes, otherwise press No"),
					Buttons = ZMessageBoxButtons.YesNo,
					MaximumResponseLength = 350,
					MinimumResponseLength = 1
				};
				Header.ExplanationToCustomsForWhyCancelling = Globals.Message.QueryUserResponse(args);
				if (!Header.ExplanationToCustomsForWhyCancelling.IsEmpty)
				{
					SendMessageToNcts(new NctsMessageFunctionSet.DeclarationCancellationRequestMessage(Header.ExplanationToCustomsForWhyCancelling, ""));
				}
				Header.ExplanationToCustomsForWhyCancelling = "";
			}
		}

		protected void MakeArrivalNotificationFromDepartureClick(object sender, EventArgs e)
		{
			Header.MakeArrivalNotificationFromDeparture();
		}

		protected override void ExtraValidation(NctsMessageFunctionSet messageFunction, MessageSendingNotificationCollection notificationCollection)
		{
			if (messageFunction is NctsMessageFunctionSet.DeclarationDataMessage)
			{
				if (Header.MovementHeader.IsSimplifiedNctsProcedure
					&& Header.MovementHeader.BM_ExportDate < ZDateTime.Now)
				{
					var message = ResString.GetMultilingualString("9FECC0C2-3C49-417A-8C0C-C6A391147EAC", "Date-Limit must be in the future.");
					Header.AddMesageErrorToCollection(notificationCollection, message);
				}
			}
		}

		protected ZMenuItem requestToCancelMenuItem;
		protected ZMenuItem amendDepartureMenuItem;
		protected ZMenuItem sendDepartureMessageMenuItem;
		ZMenuItem makeArrivalNotificationFromDepartureMenuItem;
		ZMenuItem importCustomsEntryLinesMenuItem;

		void SendDepartureMessageClick(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(delegate
			{
				SendDepartureMessasgeClickCore(sender);
			});
		}

		void RequestToCancelClick(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(
					delegate
					{
						RequestToCancelClickCore();
					});
		}

		void AmendDepartureClick(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(delegate
			{
				var departure = Header;
				Globals.Message.Show(departure.MakeDepartureAmendment());
			});
		}

		void ImportCustomsEntryLinesMenuItem_Click(object sender, EventArgs e)
		{
			var nctsHeader = Header;
			var attacher = new CusEntryHeaderInvoiceLinesToNctsAttacher(nctsHeader, nctsHeader.Lookups.CusEntryHeadersToAttach);

			attacher.Show((ZForm)importCustomsEntryLinesMenuItem.GetMainMenu().GetForm());
		}
	}
}
