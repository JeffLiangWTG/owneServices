using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsMessageProcessor(ApplicationCodeList.Codes.INCustoms, typeof(INCUniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.IN.Business;

public sealed class INCUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
{
	public bool HasMessageNeedingASeparateFactory => true;

	public bool ShouldMessageBeProcessedInASeparateFactory(Messaging.Business.EDIMessage message) => false;

	public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(Messaging.Business.EDIMessage message, LoggingInformation logger)
	{
		var loggerForThisMessage = CreateLoggerProxy(logger);
		var linkObject = GetMessageProcessor((EDIMessage)message, logger).GetLinkedObject();

		if (linkObject == null)
		{
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, ResString.GetMultilingualString("AA79C141-31D7-4D6E-829A-0484F0DB67A4", "Unable to get the job related to this message: {0}", loggerForThisMessage.ToString()));
		}

		var branchPK = message.EM_GB;
		if (linkObject is IMessageAttachee messageAttachee)
		{
			branchPK = messageAttachee.BranchPK;
		}
		else
		{
			ErrorReporter.ReportOnce(message: $"LinkObject is not IMessageAttachee, type is {linkObject.GetType().FullName}");
		}

		var jobNumber = (linkObject as IJobNumber)?.JobNumber;
		if (jobNumber.IsNullOrEmpty())
		{
			ErrorReporter.ReportOnce(message: $"LinkObject is not IJobNumber or it has empty JobNumber, type is {linkObject.GetType().FullName}");
		}

		return new LinkedBusinessObjectMetaData(linkObject.TableName, linkObject.PK, branchPK, jobNumber);
	}

	public ProcessingResult<ZGuid> GetBranch(Messaging.Business.EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		=> linkedBusinessObjectBranchPk;

	public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(Messaging.Business.EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
	{
		if (linkedBusinessObjectMetaData.JobNumber.IsEmpty)
		{
			return SerializationKeysResult.SerialProcessingInReceivedOrder;
		}

		return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
		{
			linkedBusinessObjectMetaData.JobNumber
		});
	}

	public void ProcessMessage(Messaging.Business.EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
	{
		var loggerForThisMessage = CreateLoggerProxy(logger);

		message.EM_Status = GetMessageProcessor(message as EDIMessage, logger).Process() ? EDIMessageStatusList.Codes.Received : EDIMessageStatusList.Codes.Failed;

		var processingLog = loggerForThisMessage.ToString();
		if (!string.IsNullOrEmpty(processingLog))
		{
			message.Notes.AddNew(isCustomDescription: true, ProcessingLog, processingLog);
		}
	}

	Logger CreateLoggerProxy(LoggingInformation logger)
	{
		var loggerForThisMessage = new Logger();
		logger.OnLogInfoAdded += (log, logType) =>
		{
			if (logType != Integration.LogType.Debug)
			{
				loggerForThisMessage.Log(logType, log);
			}
		};
		return loggerForThisMessage;
	}

	BaseMessageProcessor GetMessageProcessor(EDIMessage message, LoggingInformation logger)
	{
		return MessageProcessorFactory.GetMessageProcessor(message, logger);
	}

	const string ProcessingLog = "Processing Log";
}
