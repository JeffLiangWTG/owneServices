using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseInboundMessageProcessor<T> : BaseMessageProcessor
{
	protected BaseInboundMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsEdec;

	protected virtual ZString GetEntryHeaderReference(T customsResponse) => ZString.Empty;

	protected virtual ZString GetEntryHeaderEntryNum(T customsResponse) => ZString.Empty;

	protected abstract void ProcessResponseMessage(CHEDIMessage message, T customsResponse);

	protected abstract T DeserializeResponse(CHEDIMessage message);

	protected sealed override void PreProcessMessageCore(EDIMessage ediMessage)
	{
		if (ediMessage is CHEDIMessage message)
		{
			var xmlObject = DeserializeResponse(message);
			LinkToObject(message, xmlObject);
			if (message.EM_LinkedObject != null)
			{
				message.EM_Status = EDIMessage.Status.PreProcessedOK;
			}
		}
	}

	protected sealed override void ProcessMessageCore(CHEDIMessage message)
	{
		var xmlObject = DeserializeResponse(message);
		if (!RequiresPreProcessing)
		{
			LinkToObject(message, xmlObject);
		}
		if (message.EM_LinkedObject != null)
		{
			ProcessResponseMessage(message, xmlObject);
		}
	}

	void LinkToObject(CHEDIMessage message, T xmlObject)
	{
		var linkedObject = FindLinkedObject(message, xmlObject);
		if (linkedObject != null)
		{
			message.EM_LinkedObject = linkedObject;
			if (linkedObject is IBranchProvider branchProvider)
			{
				message.EM_GB = message.Interchange.EI_GB = branchProvider.Branch.PK;
			}
		}
	}

	protected abstract BusinessObject FindLinkedObject(EDIMessage message, T xmlObject);

	protected BusinessObject FindLinkedObjectByReference(EDIMessage message, T xmlObject)
	{
		BusinessObject result = null;
		if (message.Factory != null && xmlObject != null)
		{
			var referenceNumber = GetEntryHeaderReference(xmlObject);
			result = new CusEntryHeader.Loader(message.Factory).FindByBGMReference(referenceNumber);
		}
		return result;
	}
}
