using System;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.EConversation.Testing
{
	public class EmailReaderFactoryForTest : IEmailReaderFactory
	{
		readonly Action<EmailReaderForTest> populateEmailReaderFunc;

		public EmailReaderFactoryForTest(Action<EmailReaderForTest> populateEmailReaderFunc)
		{
			this.populateEmailReaderFunc = populateEmailReaderFunc;
		}

		public IEmailReader Create(IMailboxSettings settings, ILogger logger)
		{
			var testReader = new EmailReaderForTest();
			populateEmailReaderFunc(testReader);

			return testReader;
		}
	}
}
