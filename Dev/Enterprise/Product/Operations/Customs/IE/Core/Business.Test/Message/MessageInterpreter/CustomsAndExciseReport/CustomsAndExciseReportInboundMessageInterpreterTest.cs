using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Testing
{
	abstract class CustomsAndExciseReportInboundMessageInterpreterTest<TInterpreter, TDataProvider> : TestCaseWithFactory
		where TInterpreter : BaseInboundMessageInterpreter<TDataProvider>
	{
		public void TestMessageInterpretation()
		{
			var message = Factory.New<CustomsAndExciseReportInboundMessage>();
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var interpreter = GetInterpreter(message);

			var expected = ExpectedInterpretation.AdjustEmptyXmlTags();
			var actual = interpreter.GetInterpretation().AdjustEmptyXmlTags();

			AssertXMLEquals(
				typeof(TInterpreter).FullName + ".GetInterpretation()",
				expected.RemoveLineBreakingsAndIndents(),
				actual.RemoveLineBreakingsAndIndents()
			);
		}

		protected TInterpreter GetInterpreter(CustomsAndExciseReportInboundMessage incomingMessage)
		{
			TDataProvider provider = default(TDataProvider);
			using (var reader = incomingMessage.GetEM_MessageTextReader())
			{
				provider = GetProvider(reader);
			}
			return GetInterpreterCore(incomingMessage, provider);
		}

		protected virtual TInterpreter GetInterpreterCore(CustomsAndExciseReportInboundMessage incomingMessage, TDataProvider provider) => (TInterpreter)Activator.CreateInstance(typeof(TInterpreter), incomingMessage, provider);

		protected abstract TDataProvider GetProvider(TextReader reader);

		protected abstract ZString MessageType { get; }

		protected abstract ZString ExpectedInterpretation { get; }
	}
}
