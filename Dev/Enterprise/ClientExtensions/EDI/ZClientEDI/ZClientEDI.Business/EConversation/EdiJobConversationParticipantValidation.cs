using System;
using Enterprise.Client.EDI;
using Enterprise.EConversation.Business;

namespace ZClientEDI.Business.EConversation
{
	public class EdiJobConversationParticipantValidation : JobConversationParticipantValidation
	{
		public EdiJobConversationParticipantValidation(AutoJobConversationParticipant parent) : base(parent)
		{
		}

		protected new EdiJobConversationParticipant Parent => (EdiJobConversationParticipant)base.Parent;

		protected override void CheckEmailAddress()
		{
			base.CheckEmailAddress();
			var emailAddressFromRegistry = EDIDataRegistry.Instance.IncidentFromEmailAddress.Value;
			var errorMessage = "Subscription not supported for Customer Service Email Address (Registry > WiseTech Global Client Extensions > System Email Addresses > Customer Service Email Address)";
			if (Parent.EmailAddress.ToString().Equals(emailAddressFromRegistry, StringComparison.OrdinalIgnoreCase) && Parent.JCP_IsSubscribed)
			{
				Parent.EmailAddressInfo.AddError(errorMessage);
			}
		}
	}
}
