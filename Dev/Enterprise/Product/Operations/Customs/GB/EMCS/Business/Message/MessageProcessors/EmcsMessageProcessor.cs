using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Messaging4_1 = Enterprise.Customs.GB.EMCS.Messaging.Version4_1;
using Version4_1 = CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public abstract class EMCSMessageProcessor<TDataProvider> : EMCSBranchCustomsMessageProcessor<EMCSInboundEDIMessage, TDataProvider>
	{
		protected EMCSMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.GbCustomsEMCS;

		protected override BusinessObject GetLinkedObject(BusinessObjectFactory factory, EMCSInboundEDIMessage message)
		{
			var emcsDeclaration = message.LinkedDeclaration ?? EMCSHelper.GetDeclarationFromInboundMessage(message);
			if (emcsDeclaration == null)
			{
				var provider = GetDataProvider(message);
				if (provider is IEMCSInboundProvider iProvider)
				{
					emcsDeclaration = EMCSHelper.GetDeclarationFromEADNumber(message, iProvider.MrnNumber, iProvider.MrnNumberSequenceNumber);
				}
			}
			return emcsDeclaration;
		}

		protected override ZGuid GetBranchPk(BusinessObject linkedObject) => linkedObject is EMCSJobDeclaration declaration ? declaration.JE_GB : ZGuid.Invalid;

		protected sealed override IRegistryItem GetEmailGroupRegistryItem()
		{
			return linkedEMCSDeclaration != null ? (linkedEMCSDeclaration.IsConsignor ? EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements : EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements) : base.GetEmailGroupRegistryItem();
		}

		protected sealed override ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch)
		{
			return registryItem != null ? ((EmcsGroupNotification)registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty)).SendGroupPK : ZGuid.Empty;
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

		protected EMCSJobDeclaration linkedEMCSDeclaration;

		protected void SendEmailNotification(EMCSInboundEDIMessage message, string messageTypeInSubject, bool isFailure, IEMCSInboundProvider iProvider, IEMCSEvent iEvent, Func<EMCSJobDeclaration, IEMCSInboundProvider, IEMCSEvent, string> getEmailBody)
		{
			if (linkedEMCSDeclaration != null)
			{
				var originalOutgoingMessage = FindOriginalOutgoingMessage(message.Factory, message.EM_ApplicationReference) ?? linkedEMCSDeclaration.Messages.LastOutgoingMessage;

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

		protected override Type DecideDataProviderType<TTDataProvider>(Type xmlObjectType)
		{
			var result = DataProviderMap.GetValueOrDefault(xmlObjectType) ?? base.DecideDataProviderType<TDataProvider>(xmlObjectType);
			return result;
		}

		public ImmutableDictionary<Type, Type> DataProviderMap => dataProviderMap ?? (dataProviderMap = ImmutableDictionary.CreateRange(new Dictionary<Type, Type>
		{
			{ typeof(Version4_1.ie704uk.Ie704Type), typeof(Messaging4_1.IE704Provider ) },
			{ typeof(Version4_1.ie801.Ie801Type), typeof(Messaging4_1.IE801Provider) },
			{ typeof(Version4_1.ie802.Ie802Type), typeof(Messaging4_1.IE802Provider) },
			{ typeof(Version4_1.ie803.Ie803Type), typeof(Messaging4_1.IE803Provider) },
			{ typeof(Version4_1.ie807.Ie807Type), typeof(Messaging4_1.IE807Provider) },
			{ typeof(Version4_1.ie810.Ie810Type), typeof(Messaging4_1.IE810Provider) },
			{ typeof(Version4_1.ie813.Ie813Type), typeof(Messaging4_1.IE813Provider) },
			{ typeof(Version4_1.ie818.Ie818Type), typeof(Messaging4_1.IE818Provider) },
			{ typeof(Version4_1.ie819.Ie819Type), typeof(Messaging4_1.IE819Provider) },
			{ typeof(Version4_1.ie829.Ie829Type), typeof(Messaging4_1.IE829Provider) },
			{ typeof(Version4_1.ie837.Ie837Type), typeof(Messaging4_1.IE837Provider) },
			{ typeof(Version4_1.ie839.Ie839Type), typeof(Messaging4_1.IE839Provider) },
			{ typeof(Version4_1.ie840.Ie840Type), typeof(Messaging4_1.IE840Provider) },
			{ typeof(Version4_1.ie871.Ie871Type), typeof(Messaging4_1.IE871Provider) },
			{ typeof(Version4_1.ie881.Ie881Type), typeof(Messaging4_1.IE881Provider) },
			{ typeof(Version4_1.ie905.Ie905Type), typeof(Messaging4_1.IE905Provider) }
		}));
		ImmutableDictionary<Type, Type> dataProviderMap;
	}
}
