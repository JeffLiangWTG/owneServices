using Enterprise.EConversation.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiJobConversationMessageValidation : JobConversationMessageValidation
	{
		public EdiJobConversationMessageValidation(EdiJobConversationMessage parent)
			: base(parent)
		{
		}

		protected override void CheckJCM_PostedTimeUtcIsValidZDateTimeRange()
		{
		}
	}
}
