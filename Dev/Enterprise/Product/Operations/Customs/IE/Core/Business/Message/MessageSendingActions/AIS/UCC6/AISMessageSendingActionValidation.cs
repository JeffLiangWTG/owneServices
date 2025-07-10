
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AISMessageSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public AISMessageSendingActionValidation(AISMessageSendingAction parent) : base(parent) { }

		public new AISMessageSendingAction Parent => (AISMessageSendingAction)base.Parent;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			if (Parent.MessageType == AISOutgoingMessageTypeList.Codes.AmendmentRequest 
				&& Parent.EntryInstruction is CusEntryInstruction instruction
				&& instruction.IsInwardProcessingProcedure51
				&& instruction.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel
			)
			{
				Parent.MessageTypeInfo.AddMessageError(Res.GetString("782DF831-1355-40EB-BC87-9F70FA5FA0C1", "[BR5153] A declaration with '00100' Additional Information and Requested Procedure '51' cannot be amended (IM413). It must first be invalidated (IM414) and a new declaration (IM415) must be submitted."));
			}
		}
	}
}
