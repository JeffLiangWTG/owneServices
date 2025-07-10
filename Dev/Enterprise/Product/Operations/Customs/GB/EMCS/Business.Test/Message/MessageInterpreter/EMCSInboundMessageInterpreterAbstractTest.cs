using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(EMCSInboundMessageInterpreter<>))]
	abstract class EMCSInboundMessageInterpreterAbstractTest<TMessage, TInterpreter, TDataProvider> : TestCaseWithFactory where TMessage : EMCSInboundEDIMessage where TInterpreter : EMCSInboundMessageInterpreter<TDataProvider>
	{
		protected abstract ZString MessageType { get; }

		protected abstract TMessage CreateIncomingMessageToTest();

		protected abstract ZString GetExpectedInterpretation(TMessage message);

		public void TestGetMessageInterpretation()
		{
			var val = CreateIncomingMessageToTest();
			val.EM_MessageType = MessageType;
			Factory.Save();
			var interpreter = GetInterpreter(val);
			((TestCase)(object)this).AssertXMLEquals(typeof(TInterpreter).FullName + ".GetInterpretation()", (string)GetExpectedInterpretation(val), interpreter.GetInterpretation());
		}

		TInterpreter GetInterpreter(TMessage incomingMessage)
		{
			TDataProvider provider = default;
			using (TextReader reader = incomingMessage.GetEM_MessageTextReader())
			{
				provider = GetProvider(reader);
			}

			return GetInterpreterCore(incomingMessage, provider);
		}

		protected virtual TInterpreter GetInterpreterCore(TMessage incomingMessage, TDataProvider provider)
		{
			return (TInterpreter)Activator.CreateInstance(typeof(TInterpreter), incomingMessage, provider);
		}

		protected abstract TDataProvider GetProvider(TextReader reader);
	}
}
