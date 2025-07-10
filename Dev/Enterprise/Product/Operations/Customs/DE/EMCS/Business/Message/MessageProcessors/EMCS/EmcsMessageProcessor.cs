using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public abstract class EmcsMessageProcessor<TEDIMessage, TDataProvider> : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TDataProvider : IDataProvider
		where TEDIMessage : EmcsInboundEDIMessage<TDataProvider>
	{
		protected EmcsMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsEmcsSystem;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => linkedObject is EMCSJobDeclaration declaration ? declaration.JE_GB : ZGuid.Invalid;

		protected override List<AttachedDocument> GetAttachedDocuments(TEDIMessage message) => message.AttachedDocuments;

		protected override ZString GetMessageIdentifier(TEDIMessage message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected EMCSJobDeclaration GetDeclarationFromEADNumber(TEDIMessage message, ZString eadNumber, ZString messageGroup, ZString sequenceNumber)
		{
			EMCSJobDeclaration result = null;
			if (message != null && !eadNumber.IsEmpty)
			{
				var emcsDeclarations = EMCSJobDeclaration.Loader.LoadFromEadAndSequenceNumber<EMCSJobDeclaration>(message.Factory, eadNumber, sequenceNumber, message.Company);
				result = emcsDeclarations
					.Where(x => x.JE_DeclarantType == (IsConsignorDeclaration(messageGroup) ? EMCSEntryTypeList.Codes.Consignor : EMCSEntryTypeList.Codes.Consignee))
					.FirstOrDefault();
			}
			return result;
		}

		protected EMCSJobDeclaration GetDeclarationFromLocalReference(TEDIMessage message, ZString localReferenceNumber, ZString messageGroup)
		{
			EMCSJobDeclaration result = null;
			if (message != null && !localReferenceNumber.IsEmpty)
			{
				var query = GetDeclarationQuery(message, messageGroup);
				query.AddToFilter(JobDeclarationSchema.JE_OwnerRef, localReferenceNumber);
				result = message.Factory.Load<EMCSJobDeclaration>(query).OrderBy(x => x.JE_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		ZDBOnlyQuery GetDeclarationQuery(TEDIMessage message, ZString messageGroup)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, EMCSJobDeclaration.EMCSApplicationCode);
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, EMCSJobDeclaration.EMCSMessageTypeCode);
			query.AddToFilter(JobDeclarationSchema.JE_DeclarantType, IsConsignorDeclaration(messageGroup) ? EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor : EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee);
			query.AddToFilter(JobDeclarationSchema.JE_GB, message.Company.Branches.GetPKs());
			return query;
		}

		protected ZString messageGroup;

		protected sealed override IRegistryItem GetEmailGroupRegistryItem()
		{
			var result = base.GetEmailGroupRegistryItem();
			if (!messageGroup.IsEmpty)
			{
				result = IsConsignorDeclaration(messageGroup) ? EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements : EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements;
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
			if (!messageGroup.IsEmpty)
			{
				result = IsConsignorDeclaration(messageGroup)
					? EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty).SendMode
					: EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty).SendMode;
			}
			return result;
		}

		protected ZBool IsConsignorDeclaration(ZString messageGroup) => messageGroup == Messaging.EmcsMessageSubTypeList.Codes.Eme;

		protected ZBool IsConsigneeDeclaration(ZString messageGroup) => messageGroup == Messaging.EmcsMessageSubTypeList.Codes.Emb;
	}
}
