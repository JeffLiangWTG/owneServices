using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionRequestMessageSendingObjectValidation : JobDeclarationMiscMessageSendingObjectCoreValidation
	{
		public PenaltyExemptionRequestMessageSendingObjectValidation(PenaltyExemptionRequestMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		protected new PenaltyExemptionRequestMessageSendingObject Parent => base.Parent as PenaltyExemptionRequestMessageSendingObject;

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				if (Parent.SessionalData5FE.CSI_Status != CustomsEntryStatusTypeList.Codes.ANT)
				{
					Parent.ShouldSendInfo.AddError(ErrorMessage_5FENotAccepted);
				}
				else if (Parent.PenaltyAmountFrom5FK <= 0)
				{
					Parent.ShouldSendInfo.AddError(ErrorMessage_TotalPenaltyMustBeGreaterThanZero);
				}
			}
		}

		public ZString ErrorMessage_5FENotAccepted => Res.GetString("324CBCA7-3DD8-47BD-BB69-8BC75E8CAD8F", "5FE has not been accepted yet. If 5FE is not accepted, this message does not need to be sent. Please check.");
		public ZString ErrorMessage_TotalPenaltyMustBeGreaterThanZero => Res.GetString("CF6A9F1A-B77A-4EE5-A7FA-F52A8C824A5D", "The Penalty Amount (5FK) must be greater than 0 to send this message.");
	}
}
