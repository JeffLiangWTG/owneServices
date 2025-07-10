using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5MessageSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public AISUCC5MessageSendingActionValidation(AISUCC5MessageSendingAction parent) : base(parent) { }

		public new AISUCC5MessageSendingAction Parent => (AISUCC5MessageSendingAction)base.Parent;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			var parent = Parent;
			var sendingMessageType = parent.MessageType.ToUpperInvariant();
			var style = parent.EntryInstruction?.CEI_Style ?? ZString.Empty;

			if (sendingMessageType == AISOutgoingMessageTypeList.Codes.AmendmentRequest
				&& parent.EntryHeader.CH_CustomsMessageRemarks.IsEmpty
				&& (style == ImportDeclarationTypeList.Codes.H1
					|| style == ImportDeclarationTypeList.Codes.H3
					|| style == ImportDeclarationTypeList.Codes.H4
					|| style == ImportDeclarationTypeList.Codes.H5
					|| style == ImportDeclarationTypeList.Codes.I1))
			{
				parent.MessageTypeInfo.AddMessageError(Res.GetString("0C1688FF-96C4-4E66-A6C6-8F69BAD29E08", "You have not entered a Reason for Amendment."));
			}
		}
	}
}
