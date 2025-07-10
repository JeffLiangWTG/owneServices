using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	public abstract class ILEDIMessagePrettierTest<T, TMessageDataObject> : TestCaseWithFactory
		where T : class
		where TMessageDataObject : MessageDataObject<T>
	{
		public void TestMessageInterpretation()
		{
			var message = GetNewMessage();
			message.EM_MessageText = GetMessageText();
			var messageObject = Activator.CreateInstance(typeof(TMessageDataObject), message) as TMessageDataObject;
			var expected = GetExpectedMessageInterpretation().Replace("\r\n", "");
			var actual = messageObject.Prettier.GetMessageInterpretation().Replace("\r\n", "");
			AssertEquals("Message should be human readable.", expected, actual);
		}

		protected abstract ILEDIMessage GetNewMessage();

		protected abstract ZString GetMessageText();

		protected abstract ZString GetExpectedMessageInterpretation();

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
