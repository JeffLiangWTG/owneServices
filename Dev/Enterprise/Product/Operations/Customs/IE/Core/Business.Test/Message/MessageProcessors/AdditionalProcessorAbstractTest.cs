using System.Collections;
using CargoWise.Application;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	abstract class AdditionalProcessorAbstractTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var hashTable = (Hashtable)ObjectFactory.Get("IEAdditionalMessageProcessings");
			AssertNotNull(hashTable);
			var key = string.Join("|", ApplicationCode, MessageType, MessageSubType);
			Assert(hashTable.ContainsKey(key));
			var objectHandle = hashTable[key] as ObjectHandle;
			var processorObject = objectHandle?.GetObject();
			AssertNotNull(processorObject);
			Assert(processorObject is IAdditionalMessageProcessing);
			var processor = processorObject as IAdditionalMessageProcessing;
			AssertEquals(processor.GetType(), Processor.GetType());
			Processor.Process(OriginalMessage, MessageToProcess, GetTransactionProvider(MessageToProcess));
			AssertProcessMessageResultsCore();
		}

		ITransaction GetTransactionProvider(EDIMessage targetMessage)
		{
			using (var messageTextReader = targetMessage.GetEM_MessageTextReader())
			{
				return IEXmlObjectSerializer.Deserialize<MessageAcknowledgement>(messageTextReader, withXSDValidation: false);
			}
		}

		protected abstract ZString ApplicationCode { get; }
		protected abstract ZString MessageType { get; }
		protected abstract ZString MessageSubType { get; }
		protected abstract IAdditionalMessageProcessing Processor { get; }
		protected abstract EDIMessage MessageToProcess { get; }
		protected virtual EDIMessage OriginalMessage => null;
		protected abstract void AssertProcessMessageResultsCore();
	}
}
