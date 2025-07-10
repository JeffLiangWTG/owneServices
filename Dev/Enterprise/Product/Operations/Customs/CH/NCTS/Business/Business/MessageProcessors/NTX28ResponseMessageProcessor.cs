using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NTx28ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INTx28ResponseDetail>
{
	public NTx28ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override string MessageFriendlyNameCore => (NoResString)"NT028/NT528 - Departure Response Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarDepartureResponse, MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse };

	protected override BusinessObject FindLinkedObject(EDIMessage message, INTx28ResponseDetail xmlObject) => FindLinkedObjectByCorrelationIdentifier(message, xmlObject);

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INTx28ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		if (customsResponse.IsAccepted)
		{
			var mrnEntryNum = customsResponse.MRN.AppendEntryNumVersion(customsResponse.MRNVersion);
			var oldValue = nctsHeader.MovementReferenceNumber;
			nctsHeader.MovementReferenceNumberSetter(mrnEntryNum, expiryDate: customsResponse.ActivationDeadline);
			LogInformationIfValueUpdated(Res.GetString("931428f4-a624-4151-8bde-0765e0d6ed14", "Movement Reference Number"), oldValue, mrnEntryNum, message.EM_MessageNum);
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			nctsHeader.MovementHeader.BM_EntryDate = customsResponse.DecisionDateAndTime.UtcToLocalBranchTime();
		}
		else
		{
			nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Invalid;
			nctsHeader.Logs.AddNew(Events.DeclarationRejected);
		}
		message.EM_Status = EDIMessage.Status.ProcessedOK;
	}
}
