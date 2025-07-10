//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessageValidation
//
//    This class should be used for overriding validation in AutoEDIMessageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	public class EDIMessageValidation : AutoEDIMessageValidation
	{
		public EDIMessageValidation(AutoEDIMessage parent) : base(parent)
		{
		}

		protected override void CheckEM_ReceiveTransmit()
		{
			base.CheckEM_ReceiveTransmit();
			if (Parent.EM_ReceiveTransmit != Enterprise.Messaging.Business.EDIMessage.Direction.Transmit
				&& Parent.EM_ReceiveTransmit != Enterprise.Messaging.Business.EDIMessage.Direction.Receive)
			{
				Parent.EM_ReceiveTransmitInfo.AddError(Res.GetString("df23f955-3032-4d84-95e5-68e8f9d75e33", "All EDI Messages must either be marked receive or transmit"));
			}
		}

		protected override void CheckEM_MessageTextIsWesternEuropean()
		{
			if (ShouldCheckText)
			{
				if (!Parent.EM_MessageText.IsWindows1252OrEmpty)
				{
					Parent.EM_MessageTextInfo.AddError(EnglishCharactersValidation.GetNotificationMessage(Parent.EM_MessageTextInfo));
				}
			}
		}

		protected override void CheckEM_MessageDataIsValidZBlobSize()
		{
			//do nothing
		}

		bool ShouldCheckText
		{
			get
			{
				var message = Parent as EDIMessage;

				return !(message?.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed ?? true);
			}
		}
	}
}
