using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.CDS
{
	public class JobDeclarationMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
	{
		public JobDeclarationMessageSendingObjectValidation(JobDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		public new JobDeclarationMessageSendingObject Parent => (JobDeclarationMessageSendingObject)base.Parent;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			if (Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo);
				if (!Parent.MessageType.IsQueryMessageFunctionCode() && Parent.Header.IsCancelledWithCustoms)  // Query functionalty not yet implemented
				{
					Parent.MessageTypeInfo.AddMessageError(ShouldNotSendThatMessageTypeWhenCancelled);
				}

				switch (Parent.MessageType)
				{
					case CDSEDIMessageTypeList.Codes.NilAmendment:
						if (Parent.Header.CH_MasterUCR.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendNilAmendmentWithoutMUCR);
						}

						if (Parent.MovementReferenceNumber.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendNilAmendmentWithEmptyMRN);
						}
						break;
					case CDSEDIMessageTypeList.Codes.MasterQueryDeclaration:
						if (Parent.Header.CH_MasterUCR.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendMasterQueryDeclarationWithoutMUCR);
						}
						break;
					case CDSEDIMessageTypeList.Codes.AmendDeclaration:
						if (Parent.MovementReferenceNumber.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendAmendDeclarationWithEmptyMRN);
						}
						break;
					case CDSEDIMessageTypeList.Codes.CancelDeclaration:
						if (Parent.MovementReferenceNumber.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendCancelDeclarationWithEmptyMRN);
						}
						break;
					case CDSEDIMessageTypeList.Codes.ArrivalNotification:
						if (Parent.MovementReferenceNumber.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendArrivalNotificationWithEmptyMRN);
						}
						break;
					case CDSEDIMessageTypeList.Codes.FecChallenge:
						if (Parent.MovementReferenceNumber.IsEmpty)
						{
							Parent.MessageTypeInfo.AddError(CannotSendFecChallengeWithEmptyMRN);
						}
						break;
				}

				if (ILECodes.Contains(Parent.MessageType)
					&& (Parent.Header.Declaration?.ZG_Gateway ?? ZString.Empty) == GatewayList.Codes.MCP_CUSDECOnly
					&& !ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.GBAllowSendILEToMCP, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now))
				{
					Parent.MessageTypeInfo.AddError(DisableILEViaMCPMessageMCPGatewayError);
				}
			}
		}

		protected override void CheckVOCReason()
		{
			if (Parent.ShouldSend && Parent.IsAmendDeleteFecOrNil && Parent.VOCReason.IsEmpty)
			{
				Parent.VOCReasonInfo.AddError(base.GetVOCReason());
			}
		}

		protected override void CheckChangeAcknowledgementIndicator()
		{
			if (Parent.ShouldSend && Parent.IsAmendDeleteFecOrNil)
			{
				if (Parent.ChangeAcknowledgementIndicator.IsEmpty)
				{
					Parent.ChangeAcknowledgementIndicatorInfo.AddMessageError(GetMessageError(Parent.Header, AmendmentReasonCodeWarning));
				}

				if (Parent.IsAmend)
				{
					if (!Parent.Header.IsApplicableToAmendment(Parent.ChangeAcknowledgementIndicator))
					{
						Parent.ChangeAcknowledgementIndicatorInfo.AddMessageError(GetMessageError(Parent.Header, NotApplicableToAmendmentWarning));
					}
				}
				else if (Parent.IsCancel)
				{
					if (!Parent.Header.IsApplicableToCancellation(Parent.ChangeAcknowledgementIndicator))
					{
						Parent.ChangeAcknowledgementIndicatorInfo.AddMessageError(GetMessageError(Parent.Header, NotApplicableToCancellationWarning));
					}
				}
			}
		}

		const string AmendmentReasonCodeWarning = "Amendment reason code must be entered when amending or cancelling a declaration";
		const string NotApplicableToAmendmentWarning = "This entry is not applicable to an amendment because it does not have valid MRN or amendment Code";
		const string NotApplicableToCancellationWarning = "This entry is not applicable to a cancellation because it does not have valid MRN or cancellation code";
		public const string ShouldNotSendThatMessageTypeWhenCancelled = "This entry is cancelled; it is recommended not to send that message type.";
		public const string CannotSendNilAmendmentWithoutMUCR = "A NIL amendment cannot be sent without an inventory consignment reference (Master UCR). To provide confirmation that the current version of the declaration is correct following a rejected amendment request, use message type 'FEC'.";
		public const string CannotSendNilAmendmentWithEmptyMRN = "A NIL amendment cannot be sent without a Movement Reference Number (MRN)";
		const string DisableILEViaMCPMessageMCPGatewayError = "Do not send export inventory linking messages via MCP. Select a direct-to-CDS profile. MCP have taken the decision not to offer the exports inventory linking API externally.";
		public const string CannotSendMasterQueryDeclarationWithoutMUCR = "A Master query declaration cannot be sent without an inventory consignment reference (Master UCR).";
		public const string CannotSendAmendDeclarationWithEmptyMRN = "An amendment declaration cannot be sent without a Movement Reference Number (MRN)";
		public const string CannotSendCancelDeclarationWithEmptyMRN = "A cancel declaration cannot be sent without a Movement Reference Number (MRN)";
		public const string CannotSendArrivalNotificationWithEmptyMRN = "An arrival notification cannot be sent without a Movement Reference Number (MRN)";
		public const string CannotSendFecChallengeWithEmptyMRN = "An FEC challenge cannot be sent without a Movement Reference Number (MRN)";

		static readonly ImmutableList<ZString> ILECodes = ImmutableList.Create(new ZString[]
		{
			GbCusDecMessageFunctionsList.Codes.Associate,
			GbCusDecMessageFunctionsList.Codes.Disassociate,
			GbCusDecMessageFunctionsList.Codes.Close,
			GbCusDecMessageFunctionsList.Codes.QueryDeclaration,
			GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation,
			GbCusDecMessageFunctionsList.Codes.DepartureFromLocation,
			GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation,
			CDSEDIMessageTypeList.Codes.MasterQueryDeclaration
		});
	}
}
