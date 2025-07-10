using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface IUniversalCustomsMessageProcessor
	{
		// Step 1: Returns metadata about the BusinessObject this message belongs to. If this message belongs to multiple BusinessObjects, or no relevant BusinessObject could be found, return LinkedBusinessObjectMetaData.Empty.
		// ProcessingResult.DiscardReason may be set to indicate why the message should be discarded without further processing.
		ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger);

		// Step 2: Returns the correct Branch to use when processing the message in UCQ service task, which will call ProcessMessage below.
		// The LinkedBusinessObjectMetaData.BranchPk returned by GetLinkedBusinessObjectMetaData() will be passed through to this method call, and may be used as the Branch for processing the message if required by a particular interface design.
		ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk);

		// Step 3: Indicates whether the message should be processed Serially in received order, with Unconstrained Parallelization, or with a set of Keys that control the level of Parallelization in the UCQ service task.
		// When keys are provided, any messages with intersecting keys will be processed serially as a chain in received order and any messages with keys that do not intersect will be processed in parallel.
		// The LinkedBusinessObjectMetaData.JobNumber returned by GetLinkedBusinessObjectMetaData() will be passed through to this method call, and may be used as a key if required by a particular interface design.
		ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData);

		// Indicates whether a particular message should be processed in a separate factory. This is only needed in exceptional cases where a particular message would cause a lot of changes in the business object factory.
		bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message);

		// Step 4: The main processing of the message happens here. At this point EM_LinkedUniqueID and EM_GB would already have been set by UCK, and the branch context in UCQ switch accordingly.
		void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper);
	}
}
