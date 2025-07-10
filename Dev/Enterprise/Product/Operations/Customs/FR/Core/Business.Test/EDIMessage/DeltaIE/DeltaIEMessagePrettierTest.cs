using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaIEMessagePrettierTest : TestCaseWithFactory
	{
		public void TestMessageInterpretationWithCorrectJson()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DEC;
			message.EM_MessageText = "{\"root\":{\"key1\":\"string\",\"key2\":12345,\"key3\":\"2022-12-12T14:00:31.601Z\",\"key4\":[],\"key5\":[123,\"123\",{\"a\":5,\"b\":6,\"c\":null,\"d\":true}],\"key6\":{\"a\":1,\"b\":3,\"c\":{\"d\":4}}}}";
			var messageObject = new DeltaIEMessageDataObject(message);
			var prettier = new DeltaIEMessagePrettier(messageObject);
			var expectedResult = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PrettyJson.html");
			AssertEquals("Message should be human readable.", expectedResult, prettier.GetMessageInterpretation());
		}

		public void TestMessageInterpretationWithIncorrectJson()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DEC;
			message.EM_MessageText = "{2365[4258";
			var messageObject = new DeltaIEMessageDataObject(message);
			var prettier = new DeltaIEMessagePrettier(messageObject);
			var expectedResult = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.PrettyJsonWithError.html");
			AssertEquals("Message should be human readable.", expectedResult, prettier.GetMessageInterpretation());
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	public abstract class DeltaIEMessagePrettierTest<T, TMessageDataObject> : TestCaseWithFactory
		where T : class
		where TMessageDataObject : MessageDataObject<T>
	{
		public void TestMessageInterpretation()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageText = GetMessageText();
			var messageObject = Activator.CreateInstance(typeof(TMessageDataObject), message) as TMessageDataObject;
			AssertEquals("Message should be human readable.", GetExpectedMessageInterpretation(), messageObject.Prettier.GetMessageInterpretation());
		}

		protected abstract ZString GetMessageText();

		protected abstract ZString GetExpectedMessageInterpretation();

		readonly protected Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
