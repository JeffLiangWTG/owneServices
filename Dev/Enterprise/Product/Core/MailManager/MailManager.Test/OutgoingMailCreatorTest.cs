using Enterprise.Environment;

namespace Enterprise.MailManager.Testing
{
	sealed class OutgoingMailCreatorTest : MailManagerEmailSenderTest<OutgoingMailCreator>
	{
		#region Implementation

		protected override OutgoingBaseMailCreator<OutgoingMailCreator> MailCreator
		{
			get { return (OutgoingBaseMailCreator<OutgoingMailCreator>)Env.OutgoingMailManager; }
		}

		protected override bool ShouldRecipientBeForSystemCommunication
		{
			get { return false; }
		}

		#endregion
	}
}
