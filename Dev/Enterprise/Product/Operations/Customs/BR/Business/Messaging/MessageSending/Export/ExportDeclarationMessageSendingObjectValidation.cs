using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ExportDeclarationMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public ExportDeclarationMessageSendingObjectValidation(ExportDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		public new ExportDeclarationMessageSendingObject Parent => base.Parent as ExportDeclarationMessageSendingObject;

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			ValidateMessageType();

			if (Parent.ShouldSend && Parent.Header.IsWaitingForResponse)
			{
				Parent.ShouldSendInfo.AddError(Res.GetString("542E53BF-90A7-4429-B618-C4CBB3EC6E39", "There is message waiting for response. Please wait until the messages are responded."));
			}
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			if (Parent.ShouldSend)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.MessageTypeInfo);

				var parentMRN = Parent.Header.MovementReferenceNumber;

				if (Parent.IsRectification && parentMRN.IsEmpty)
				{
					Parent.MessageTypeInfo.AddError(Res.GetString("949c4584-6ca2-4ac4-8326-c9a8b6afa47a", "Rectification Messages should only be sent if the Entry contains a MRN Number."));
				}
				else if (Parent.IsOriginal && !parentMRN.IsEmpty)
				{
					Parent.MessageTypeInfo.AddError(Res.GetString("fea9eb3b-3469-4966-9e1a-92bac9c9b3ac", "Original message has already been sent. Entry already contains a MRN Number. Use RET Message Type if you want rectify/ amend the Entry"));
				}
			}

			ValidateVOCReason();
		}

		protected override void CheckVOCReason()
		{
			base.CheckVOCReason();

			if (Parent.ShouldSend)
			{
				if (Parent.IsRectification && Parent.Header != null && !Parent.Header.CH_EntryStatus.IsEmpty && EntryStatusNeedsRectificationReason(Parent.Header.CH_EntryStatus))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.VOCReasonInfo);
				}
			}
		}

		bool EntryStatusNeedsRectificationReason(string status)
		{
			switch (status)
			{
				case Constants.EntryStatus.Registered:
				case Constants.EntryStatus.CanceledByExporter:
				case Constants.EntryStatus.CanceledDueToExpiration:
				case Constants.EntryStatus.CanceledByRFB:
				case Constants.EntryStatus.CanceledByRFBByRequest:
					return false;
				default:
					return true;
			}
		}
	}
}
