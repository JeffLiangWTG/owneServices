using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessageProcessors;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public abstract class FREDIMessageDataObjectTest<T, TMessage, TEDIMessage> : TestCaseWithFactory
		where T : MessageDataObject<TMessage>
		where TMessage : class
		where TEDIMessage : FREDIMessage
	{
		protected T messageDataObject;
		protected abstract Type ExpectedPrettierType { get; }

		public void TestPrettierType()
		{
			AssertType($"Prettier Type of {nameof(T)}", ExpectedPrettierType, messageDataObject.Prettier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var message = Factory.New<TEDIMessage>();
			message.EM_MessageText = GetMessageText();
			message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			messageDataObject = Activator.CreateInstance(typeof(T), message) as T;
		}

		protected abstract ZString GetMessageText();
		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		public void TestMessageText()
		{
			var message = Factory.New<TEDIMessage>();
			message.EM_MessageText = "MessageText";
			var messageObject = Activator.CreateInstance(typeof(T), message) as T;
			AssertEquals("MessageText should expose EDIMessage EM_MessageText.", "MessageText", messageObject.MessageText);
		}

		public void TestResponseMessage()
		{
			AssertType<TMessage>("The response message should be deserialized properly.", messageDataObject.ResponseMessage);
		}
	}
}
