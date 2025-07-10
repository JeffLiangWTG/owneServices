using Enterprise.Environment;

namespace Enterprise.MailManager.Testing
{
	sealed class OutgoingCustomsMailCreatorTest : MailManagerEmailSenderTest<OutgoingCustomsMailCreator>
	{
		#region Implementation

		protected override OutgoingBaseMailCreator<OutgoingCustomsMailCreator> MailCreator
		{
			get { return (OutgoingBaseMailCreator<OutgoingCustomsMailCreator>)Env.OutgoingCustomsMailManager; }
		}

		protected override bool ShouldRecipientBeForSystemCommunication
		{
			get { return true; }
		}

		#endregion
	}
}
