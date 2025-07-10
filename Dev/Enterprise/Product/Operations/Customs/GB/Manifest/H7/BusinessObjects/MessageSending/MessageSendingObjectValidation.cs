using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	public class MessageSendingObjectValidation : EU.H7.Business.MessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(MessageSendingObject parent)
			: base(parent)
		{
		}

		public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

		protected override void CheckAction()
		{
			base.CheckAction();
			if (Parent.Action == H7EDIMessageTypeList.Codes.ArrivalNotification && Parent.MRN.IsEmpty)
			{
				Parent.ActionInfo.AddError(CannotSendArrivalNotificationWithEmptyMRN);
			}
			else if (Parent.Action == H7EDIMessageTypeList.Codes.CancelDeclaration && Parent.MRN.IsEmpty)
			{
				Parent.ActionInfo.AddError(CannotSendCancelDeclarationWithEmptyMRN);
			}
		}

		protected override void CheckAmendmentReasonCode()
		{
			base.CheckAmendmentReasonCode();
			if (Parent.Action == H7EDIMessageTypeList.Codes.CancelDeclaration)
			{
				if (!Parent.IsAmendmentReasonCodeApplicableToCancellation)
				{
					Parent.AmendmentReasonCodeInfo.AddMessageError(NotApplicableToCancellationWarning);
				}

				if (Parent.AmendmentReasonCode.IsEmpty)
				{
					Parent.AmendmentReasonCodeInfo.AddMessageError(EmptyAmendmentReasonCodeWarning);
				}
			}
		}

		protected override void CheckAmendmentInvalidationReason()
		{
			base.CheckAmendmentInvalidationReason();
			if (Parent.Action == H7EDIMessageTypeList.Codes.CancelDeclaration && Parent.AmendmentInvalidationReason.IsEmpty)
			{
				Parent.AmendmentInvalidationReasonInfo.AddError(AmendmentReasonWarning);
			}
		}

		protected override void CheckQueryType()
		{
			base.CheckQueryType();
			if (Parent.Action == H7EDIMessageTypeList.Codes.QueryDeclaration)
			{
				ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.QueryTypeInfo);
				if (Parent.QueryTypeList.GetAllCodesZString().Contains(Parent.QueryType) && !Parent.IsQueryTypeApplicable)
				{
					Parent.QueryTypeInfo.AddError(Res.GetString("5130a358-596d-47c3-bfe8-333453aef8da", "This entry does not have a value for its {0}", Parent.QueryType));
				}
			}
		}

		static string CannotSendArrivalNotificationWithEmptyMRN => Res.GetString("e8b96983-c31f-4998-8482-65f5427e5c4e", "An arrival notification cannot be sent without a Movement Reference Number (MRN)");
		static string CannotSendCancelDeclarationWithEmptyMRN => Res.GetString("4c416141-5e85-4093-92ab-dd9bd41bf242", "A cancel declaration cannot be sent without a Movement Reference Number (MRN)");
		static string NotApplicableToCancellationWarning => Res.GetString("dbc8c2cb-5217-4e6e-b0b2-95a074869092", "This entry is not applicable to a cancellation because it does not have valid MRN or cancellation code");
		static string EmptyAmendmentReasonCodeWarning => Res.GetString("4b581938-3562-4fa6-92f6-83b59bab8785", "Amendment reason code must be entered when canceling a declaration");
		static string AmendmentReasonWarning => Res.GetString("4bb3ea95-f02e-42c8-94ee-2034265a7ebe", "Amendment reason must be entered when canceling a declaration");
	}
}
