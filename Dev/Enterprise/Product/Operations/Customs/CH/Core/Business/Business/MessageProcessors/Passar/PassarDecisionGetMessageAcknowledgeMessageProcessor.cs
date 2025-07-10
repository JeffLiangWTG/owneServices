using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class PassarDecisionGetMessageAcknowledgeMessageProcessor<T> : PassarGetMessageAcknowledgeMessageProcessor<T>  where T : IPassarResponseDetail
{
	public PassarDecisionGetMessageAcknowledgeMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSG };

	protected virtual Event MessageRejectedEvent => null;

	protected override void ProcessResponseMessage(CHEDIMessage message, T customsResponse)
	{
		var customsResponseDecision = customsResponse as IDecision;
		var customsResponseWithGDRN = customsResponse as IPassarResponseWithGDRN;

		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			if (customsResponseDecision.IsAccepted)
			{
				var entryNum = $"{customsResponseWithGDRN.GDRN}.{customsResponseWithGDRN.GDRNVersion}";
				var oldValue = entryHeader.MovementReferenceNumber;
				entryHeader.MovementReferenceNumberSetter(entryNum, issueDate: new ZDateTime(customsResponseDecision.DecisionDateAndTime).ToLocalBranchTime());
				LogInformationIfValueUpdated(Res.GetString("749b6944-e078-4a03-a180-2f0701db966e", "Movement Reference Number"), oldValue, entryNum, message.EM_MessageNum);

				entryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;
			}
			else
			{
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;

				if (MessageRejectedEvent != null)
				{
					entryHeader.Logs.AddNew(MessageRejectedEvent);
				}
			}
		}
	}
}
