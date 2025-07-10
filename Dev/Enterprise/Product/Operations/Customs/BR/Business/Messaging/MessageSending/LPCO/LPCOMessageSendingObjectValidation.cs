using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageSendingObjectValidation : ZValidation
	{
		public LPCOMessageSendingObjectValidation(LPCOMessageSendingObject parent) : base(parent)
		{
			Parent = parent;
		}

		public LPCOMessageSendingObject Parent;

		public override Type AutoValidationType => typeof(LPCOMessageSendingObjectValidation);

		public override void ValidateAll()
		{
			ValidateShouldSend();
		}

		public void ValidateShouldSend()
		{
			ValidateCalculatedProperty(Parent.ShouldSendInfo);
		}

		protected void CheckShouldSend()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.IsWaitingForResponse)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("A2AD4FA0-B404-434B-9D73-6BF13B8F3FAB", "There is still a message waiting for response. Please wait until the message is responded."));
				}
				if (!Parent.Parent.LPCOHeader.CPH_Number.IsEmpty & Parent.MessageType == LPCOMessageTypesList.Codes.ORI)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("6B84B640-B350-46F9-91BE-5FC2D0BAFD29", "Original message cannot be resent because this LPCO already contains a LPCO Number."));
				}
			}
		}
	}
}
