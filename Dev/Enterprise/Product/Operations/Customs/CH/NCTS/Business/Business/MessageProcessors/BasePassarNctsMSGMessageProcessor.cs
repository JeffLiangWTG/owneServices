using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using CoreConstants = Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business;

public abstract class BasePassarNctsMSGMessageProcessor<TPassarResponse> : PassarGetMessageAcknowledgeMessageProcessor<TPassarResponse>
	where TPassarResponse : IPassarResponseDetail
{
	protected BasePassarNctsMSGMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected abstract string MovementType { get; }

	protected override BusinessObject FindLinkedObject(EDIMessage message, TPassarResponse xmlObject)
	{
		return ShouldFindLinkedObjectByMRN(xmlObject)
			? FindLinkedObjectByMRN(message, xmlObject as IPassarResponseWithMRN)
			: base.FindLinkedObject(message, xmlObject);
	}

	protected virtual bool ShouldFindLinkedObjectByMRN(TPassarResponse xmlObject) => xmlObject is IPassarResponseWithMRN;

	protected virtual bool FindLinkedObjectByMRNAnyVersion => false;

	protected virtual bool ShouldUpdateMRNVersion => FindLinkedObjectByMRNAnyVersion;

	protected BusinessObject FindLinkedObjectByMRN(EDIMessage message, IPassarResponseWithMRN responseWithMRN)
	{
		var loader = new NctsHeader.Loader(message.Factory);
		var nctsHeader = FindLinkedObjectByMRNAnyVersion
			? loader.FindByMovementReferenceNumberAnyVersion(MovementType, responseWithMRN.MRN)
			: loader.FindByMovementReferenceNumber(MovementType, responseWithMRN.MRN, responseWithMRN.MRNVersion);
		return MovementType == NctsMovementType.Codes.Departure ? nctsHeader?.MovementHeader : nctsHeader;
	}

	protected sealed override void ProcessResponseMessage(CHEDIMessage message, TPassarResponse customsResponse)
	{
		if (NctsHeader.GetLinkedNctsHeader(message.EM_LinkedObject) is NctsHeader nctsHeader)
		{
			ProcessResponseMessageCore(message, customsResponse, nctsHeader);

			if (ShouldUpdateMRNVersion && customsResponse is IPassarResponseWithMRN responseWithMRN)
			{
				var newMrnVersion = int.TryParse(responseWithMRN.MRNVersion, out var value) ? value : 0;
				var currentMrnVersion = nctsHeader.MovementReferenceNumber.Contains(".") && int.TryParse(nctsHeader.MovementReferenceNumber.Split('.')[1], out var currentVersion) ? currentVersion : 0;

				if (newMrnVersion > currentMrnVersion)
				{
					nctsHeader.MovementReferenceNumberSetter(responseWithMRN.MRN.AppendEntryNumVersion(responseWithMRN.MRNVersion));
					nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus = CoreConstants.EntryStatusCodes.New;
				}
			}
		}
	}

	protected abstract void ProcessResponseMessageCore(CHEDIMessage message, TPassarResponse customsResponse, NctsHeader nctsHeader);
}
