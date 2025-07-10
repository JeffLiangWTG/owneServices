using Rnwood.SmtpServer;
using Rnwood.SmtpServer.Verbs;

namespace Enterprise.MailManager.ExternalMailInterface
{
	sealed class RejectedRcptVerbForTest : IVerb
	{
		public RejectedRcptVerbForTest()
		{
			SubVerbMap = new VerbMap();
			SubVerbMap.SetVerbProcessor("TO", new RejectedRcptToVerbForTest());
		}

		public VerbMap SubVerbMap { get; private set; }

		public void Process(IConnection connection, SmtpCommand command)
		{
			var subrequest = new SmtpCommand(command.ArgumentsText);
			var verbProcessor = SubVerbMap.GetVerbProcessor(subrequest.Verb);

			if (verbProcessor != null)
			{
				verbProcessor.Process(connection, subrequest);
			}
			else
			{
				connection.WriteResponse(
					new SmtpResponse(StandardSmtpResponseCode.CommandParameterNotImplemented, "Subcommand {0} not implemented", subrequest.Verb));
			}
		}
	}
}
