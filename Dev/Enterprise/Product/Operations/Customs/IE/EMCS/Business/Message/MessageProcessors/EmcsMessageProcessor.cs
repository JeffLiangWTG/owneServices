using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public abstract class EMCSMessageProcessor<TDataProvider> : MessageProcessor<EMCSInboundEDIMessage, TDataProvider>
	{
		protected EMCSMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsEMCS;

		protected override BusinessObject GetLinkedObject(BusinessObjectFactory factory, EMCSInboundEDIMessage message)
		{
			EMCSJobDeclaration emcsDeclaration = null;
			var outMessage = FindOriginalOutgoingMessage(factory, message);
			if (outMessage != null && outMessage.Branch.GB_GC == message.Branch.GB_GC)
			{
				emcsDeclaration = outMessage.EM_LinkedObject as EMCSJobDeclaration;
			}

			if (emcsDeclaration == null)
			{
				var provider = GetDataProvider(message);

				if (provider is IEMCSInboundProvider iProvider)
				{
					emcsDeclaration = GetDeclarationFromEADNumber(message, iProvider.MrnNumber, iProvider.MrnNumberSequenceNumber);
				}
			}
			return emcsDeclaration;
		}

		protected EMCSJobDeclaration GetDeclarationFromEADNumber(EMCSInboundEDIMessage message, ZString eadNumber, ZString sequenceNumber)
		{
			EMCSJobDeclaration result = null;
			if (!eadNumber.IsEmpty)
			{
				result = EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(message.Factory, eadNumber, sequenceNumber, message.Company).FirstOrDefault();
			}
			return result;
		}

		protected override ZGuid GetBranchPk(BusinessObject linkedObject) => linkedObject is EMCSJobDeclaration declaration ? declaration.JE_GB : ZGuid.Invalid;

		protected void SendEmailNotification(EMCSInboundEDIMessage message, string messageTypeInSubject, bool isFailure, IEMCSInboundProvider iProvider, IEMCSEvent iEvent, Func<EMCSJobDeclaration, IEMCSInboundProvider, IEMCSEvent, string> getEmailBody)
		{
			if (linkedEMCSDeclaration != null)
			{
				var originalOutgoingMessage = FindOriginalOutgoingMessage(message.Factory, message)
					?? linkedEMCSDeclaration.Messages.LastOutgoingMessage as EDIMessage;

				GenerateHtmlEmailAndSendToOriginalOrGroup(
					factory: message.Factory,
					relatedJob: linkedEMCSDeclaration,
					messageTypeInSubject: messageTypeInSubject,
					body: getEmailBody(linkedEMCSDeclaration, iProvider, iEvent),
					isFailure: isFailure,
					branchForEmailLogo: linkedEMCSDeclaration.Branch,
					sourceBusinessObject: linkedEMCSDeclaration,
					getEmailAddressToSendTo: () => GetEmailAddressToSendToFromQueuedUser(originalOutgoingMessage)
				);
			}
		}

		protected EMCSJobDeclaration linkedEMCSDeclaration;

		protected sealed override IRegistryItem GetEmailGroupRegistryItem()
		{
			var result = base.GetEmailGroupRegistryItem();
			if (linkedEMCSDeclaration != null)
			{
				result = linkedEMCSDeclaration.IsConsignor ? EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements : EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements;
			}
			return result;
		}

		protected sealed override ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				result = ((EmcsGroupNotification)registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty)).SendGroupPK;
			}
			return result;
		}

		protected sealed override ZString GetEmailSendMode(IGlbBranch branch)
		{
			var result = base.GetEmailSendMode(branch);
			if (linkedEMCSDeclaration != null)
			{
				result = linkedEMCSDeclaration.IsConsignor
					? EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty).SendMode
					: EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty).SendMode;
			}
			return result;
		}

		protected override void ReplaceXMLElementIfNeeded(EDIMessage message)
		{
			var messageText = message.EM_MessageText;
			if (messageText.Contains("EMCS:PHASE3"))
			{
				messageText = messageText.Replace("EMCS:PHASE3", "EMCS:PHASE4").Replace("V2.02", "V3.01");
				message.EM_MessageText = ReplaceExtraXMLElementIfNeeded(messageText);
			}
		}

		protected virtual ZString ReplaceExtraXMLElementIfNeeded(ZString text)
		{
			return text;
		}
	}
}

