using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(InboundMessageInterpreter<>))]
	public abstract class InboundMessageInterpreterAbstractTest<TMessage, TInterpreter, TDataProvider> : TestCaseWithFactory
		where TMessage : InboundEDIMessage
		where TInterpreter : InboundMessageInterpreter<TDataProvider>
	{
		protected abstract ZString MessageType { get; }

		protected abstract TMessage CreateIncomingMessageToTest();

		protected abstract ZString GetExpectedInterpretation(TMessage message);

		public void TestGetMessageInterpretation()
		{
			var message = CreateIncomingMessageToTest();
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var interpreter = GetInterpreter(message);

			var expected = GetExpectedInterpretation(message).AdjustEmptyXmlTags();
			var actual = interpreter.GetInterpretation().AdjustEmptyXmlTags();

			AssertXMLEquals(
				typeof(TInterpreter).FullName + ".GetInterpretation()",
				expected.RemoveLineBreakingsAndIndents(),
				actual.RemoveLineBreakingsAndIndents()
			);
		}

		protected TInterpreter GetInterpreter(TMessage incomingMessage)
		{
			TDataProvider provider = default(TDataProvider);
			using (var reader = incomingMessage.GetEM_MessageTextReader())
			{
				provider = GetProvider(reader);
			}
			return GetInterpreterCore(incomingMessage, provider);
		}

		protected virtual TInterpreter GetInterpreterCore(TMessage incomingMessage, TDataProvider provider) => (TInterpreter)Activator.CreateInstance(typeof(TInterpreter), incomingMessage, provider);

		protected abstract TDataProvider GetProvider(TextReader reader);
	}
}
