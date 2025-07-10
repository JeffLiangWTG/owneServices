using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	public abstract class ILEDIMessageDataObjectTest<T, TMessage, TEDIMessage> : TestCaseWithFactory
		where T : MessageDataObject<TMessage>
		where TMessage : class
		where TEDIMessage : ILEDIMessage
	{
		protected T messageDataObject;
		protected abstract Type ExpectedPrettierType { get; }

		public void TestPrettierType()
		{
			AssertType($"Prettier Type of {nameof(T)}", ExpectedPrettierType, messageDataObject.Prettier);
		}

		public void TestMessageText()
		{
			var message = Factory.New<TEDIMessage>();
			message.EM_MessageText = "MessageText";
			var messageObject = Activator.CreateInstance(typeof(T), message) as T;
			AssertEquals("MessageText should expose EDIMessage EM_MessageText.", "MessageText", messageObject.MessageText);
		}

		public void TestResponseMessage()
		{
			AssertType<TMessage>("The response message should be deserialized properly.", messageDataObject.MessageData);
		}

		protected abstract ZString GetMessageText();

		protected readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void SetUp()
		{
			base.SetUp();
			var message = Factory.New<TEDIMessage>();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = GetMessageText();
			messageDataObject = Activator.CreateInstance(typeof(T), message) as T;
		}
	}
}
