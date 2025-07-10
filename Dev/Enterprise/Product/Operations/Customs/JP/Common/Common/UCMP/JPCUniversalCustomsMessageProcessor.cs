using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsMessageProcessor(ApplicationCodeList.Codes.JPCustoms, typeof(Enterprise.Customs.JP.Common.JPCUniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.JP.Common;

public sealed class JPCUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
{
	ProcessingResult<LinkedBusinessObjectMetaData> IUniversalCustomsMessageProcessor.GetLinkedBusinessObjectMetaData(Messaging.Business.EDIMessage message, LoggingInformation logger)
	{
		var linkObject = message.EM_LinkedObject;

		if (linkObject != null)
		{
			var branchPk = (linkObject as IBranchProvider)?.Branch?.PK ?? message.EM_GB;

			var jobNumber = linkObject is Integration.Customs.ASYCUDA.IAsycudaManifestHeader header
				? header.AMA_JobReference.ToString()
				: (linkObject as IJobNumber)?.JobNumber ?? string.Empty;

			return new LinkedBusinessObjectMetaData(linkObject.TableName, linkObject.PK, branchPk, jobNumber);
		}
		else
		{
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty);
		}
	}

	ProcessingResult<SerializationKeysResult> IUniversalCustomsMessageProcessor.GetSerializationKeysResult(Messaging.Business.EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
	{
		return linkedBusinessObjectMetaData.JobNumber.IsEmpty
			? SerializationKeysResult.SerialProcessingInReceivedOrder
			: new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { linkedBusinessObjectMetaData.JobNumber });
	}

	void IUniversalCustomsMessageProcessor.ProcessMessage(Messaging.Business.EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
	{
		var processor = MessageProcessorFactory.GetMessageProcessor(message, logger);
		processor.ProcessMessage(message);
	}

	bool IUniversalCustomsMessageProcessor.ShouldMessageBeProcessedInASeparateFactory(Messaging.Business.EDIMessage message) => false;

	ProcessingResult<ZGuid> IUniversalCustomsMessageProcessor.GetBranch(Messaging.Business.EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;
}
