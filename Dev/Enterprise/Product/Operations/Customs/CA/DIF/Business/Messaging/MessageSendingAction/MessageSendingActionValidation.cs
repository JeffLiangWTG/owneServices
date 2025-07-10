
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business
{
	public class MessageSendingActionValidation : AutoMessageSendingActionValidation
	{
		public MessageSendingActionValidation(AutoMessageSendingAction action)
			: base(action)
		{
		}

		public new MessageSendingAction Parent
		{
			get { return (MessageSendingAction)base.Parent; }
		}

		protected override void CheckSend()
		{
			base.CheckSend();

			if (Parent.Send)
			{
				if (Parent.IsWaitingForResponse)
				{
					Parent.SendInfo.AddMessageError(PendingResponses);
				}

				var messageSendingError = Parent.MessageSendingError;
				if (!messageSendingError.IsEmpty)
				{
					Parent.SendInfo.AddError(messageSendingError);
				}
			}
		}

		protected override void CheckSendWithdrawal()
		{
			base.CheckSendWithdrawal();

			if (Parent.SendWithdrawal && Parent.IsWaitingForResponse)
			{
				Parent.SendWithdrawalInfo.AddMessageError(PendingResponses);
			}
		}

		internal static string PendingResponses => Res.GetString("9E3A6645-73A7-432A-BCB2-807D91FB8C1B", "This document has been submitted to Customs and a response from Customs is outstanding.");
	}
}
