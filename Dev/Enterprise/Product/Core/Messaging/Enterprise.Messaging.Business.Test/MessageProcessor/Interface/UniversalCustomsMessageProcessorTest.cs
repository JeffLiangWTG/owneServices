using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	[TestsSubclassesOf(typeof(IUniversalCustomsMessageProcessor))]
	public abstract class UniversalCustomsMessageProcessorTest<T> : TestCaseWithFactory
		where T : class, IUniversalCustomsMessageProcessor
	{
		public virtual void TestCorrectSubscribeToUniversalCustomsMessagingSubscribers()
		{
			AssertType<T>("Make sure expected type is accompanied with an UniversalCustomsMessageProcessor attribute. DEVELOPER NOTE: Make sure to execute AssemblyMetaDataExtractor.exe on your local to reflect newly added processors.", ObjectFactory.Get<IUniversalCustomsMessagingSubscribersProvider>().GetMessageProcessor(ApplicationCode));
		}
		protected abstract string ApplicationCode { get; }

		public static void AssertPreProcessing(IUniversalCustomsMessageProcessor messageProcessor, EDIMessage message, ProcessingResult<LinkedBusinessObjectMetaData> expectedLinkedBusinessObjectMetaData, ProcessingResult<ZGuid> expectedBranchPk, ProcessingResult<SerializationKeysResult> expectedSerializationKeysResult)
		{
			var actualResult = messageProcessor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());
			AssertEquals(nameof(messageProcessor.GetLinkedBusinessObjectMetaData), expectedLinkedBusinessObjectMetaData, actualResult);

			var actualBranch = messageProcessor.GetBranch(message, new LoggingInformation(), expectedLinkedBusinessObjectMetaData.ReturnValue.BranchPk);
			AssertEquals(expectedBranchPk, actualBranch);

			var actualKeys = messageProcessor.GetSerializationKeysResult(message, new LoggingInformation(), expectedLinkedBusinessObjectMetaData.ReturnValue);
			AssertEquals(expectedSerializationKeysResult, actualKeys);
		}
	}
}
