using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public abstract class PassarGetMessageAcknowledgeMessageProcessor<T> : BaseGetMessageInboundMessageProcessor where T : IPassarResponseDetail
{
	public PassarGetMessageAcknowledgeMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override void UpdateTransaction(CusPollingTransaction transaction)
	{
		transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.Closed;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
	}

	protected sealed override void ProcessMessageCore(CHEDIMessage message)
	{
		base.ProcessMessageCore(message);

		var xmlObject = DeserializeResponse(message);
		if (xmlObject != null)
		{
			if (xmlObject.OppositeInformation == null
				|| (xmlObject.OppositeInformation.Text.StartsWith($"{CustomsMessageHelper.PartnerTopicPrefix}{GlbCompany.CurrentCompany.LicenceEnterpriseCode}") && xmlObject.OppositeInformation.Text.EndsWith(GlbCompany.CurrentCompany.LicenceServerID)))
			{
				var linkedObject = FindLinkedObject(message, xmlObject);
				if (linkedObject != null)
				{
					message.EM_LinkedObject = linkedObject;

					if (linkedObject is IBranchProvider branchProvider)
					{
						message.EM_GB = message.Interchange.EI_GB = branchProvider.Branch.PK;
					}

					ProcessResponseMessage(message, xmlObject);
				}
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Warning;
				Logger?.Log(LogType.Information, $"Message skipped because '{xmlObject.OppositeInformation.Text}' does not belong to this CW instance ({CustomsMessageHelper.PartnerTopicPrefix}{GlbCompany.CurrentCompany.LicenceEnterpriseCode}XXX{GlbCompany.CurrentCompany.LicenceServerID}).");
			}
		}
	}

	protected virtual T DeserializeResponse(CHEDIMessage message) => (T)message?.MessageDetail;

	protected virtual BusinessObject FindLinkedObject(EDIMessage message, T xmlObject) => FindLinkedObjectByCorrelationIdentifier(message, xmlObject);

	protected BusinessObject FindLinkedObjectByCorrelationIdentifier(EDIMessage message, T xmlObject)
	{
		var correlationIdentifier = xmlObject?.CorrelationIdentifier;
		return string.IsNullOrEmpty(correlationIdentifier) ? null
			: message.Factory.GetOutgoingMessageFromApplicationReference(ApplicationCode, correlationIdentifier)?.EM_LinkedObject;
	}

	protected BusinessObject FindLinkedObjectByGDRN(EDIMessage message, T xmlObject)
		=> FindLinkedEntryHeaderByEntryNum(message, (xmlObject as IPassarResponseWithGDRN)?.GDRN, anyVersion: true);

	protected BusinessObject FindLinkedObjectByEntryNum(EDIMessage message, T xmlObject, bool anyVersion = false)
		=> FindLinkedEntryHeaderByEntryNum(message, GetCustomsEntryNumber(xmlObject), anyVersion);

	protected virtual string GetCustomsEntryNumber(T customsResponse) => null;

	protected abstract void ProcessResponseMessage(CHEDIMessage message, T customsResponse);
}
