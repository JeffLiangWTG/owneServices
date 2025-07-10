using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public abstract class NCTSMessagePrettierTest<T, TMessageDataObject> : TestCaseWithFactory
		where T : class
		where TMessageDataObject : MessageDataObject<T>
	{
		public void TestMessageInterpretation()
		{
			var message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageText = GetMessageText();
			var messageObject = Activator.CreateInstance(typeof(TMessageDataObject), message) as TMessageDataObject;
			AssertEqualsIgnoreLineBreaks("Message should be human readable.", GetExpectedMessageInterpretation(), messageObject.Prettier.GetMessageInterpretation());
		}

		protected abstract ZString GetMessageText();

		protected abstract ZString GetExpectedMessageInterpretation();

		readonly protected Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
