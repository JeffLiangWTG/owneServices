using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public DuimpMessageSendingObjectValidation(DuimpMessageSendingObject parent) : base(parent)
		{
		}
		public new DuimpMessageSendingObject Parent => base.Parent as DuimpMessageSendingObject;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			if (Parent.ShouldSend)
			{
				var header = Parent.Header;

				if (Parent.MessageType == ImportEntryActionCodeList.Codes.DEL)
				{
					if (header.EntryNumber.IsEmpty)
					{
						Parent.MessageTypeInfo.AddError(Res.GetString("77218C09-555D-4389-B9A3-61846CBBFB5A", "There is no Entry Number."));
					}
				}
				else if (Parent.MessageType == ImportEntryActionCodeList.Codes.DIA || Parent.MessageType == ImportEntryActionCodeList.Codes.REG)
				{
					if (header.EntryNumber.IsEmpty || header.CH_CustomsPostedStatus != CustomsPostedStatusList.Codes.Accepted ||
						header.AllEntryLines.Any(x => x.CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.Accepted && x.CL_CustomsPostedStatus != CustomsPostedStatusList.Codes.Deleted))
					{
						Parent.MessageTypeInfo.AddError(Res.GetString("d21070a5-d448-48ff-ab86-79f85553f2c4", "There are changes that have not been submitted to Customs. Please send an ORI - Original message before sending this message."));
					}
				}
			}
		}

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				if (Parent.Header.IsWaitingForResponse)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("BFC0364F-C604-4E1A-9B93-88922EB07A41", "There is message waiting for response. Please wait until the messages are responded."));
				}
			}

			ValidateMessageType();
		}
	}
}
