using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public abstract class ImportMessageProcessor<TEDIMessage, TDataProvider> : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TDataProvider : IDataProvider
		where TEDIMessage : AtlasInboundEDIMessage<TDataProvider>
	{
		protected ImportMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAtlasSystem;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is CusEntryHeader entryHeader)
			{
				var declaration = entryHeader.Declaration;
				if (declaration != null)
				{
					result = declaration.JE_GB;
				}
			}

			return result;
		}

		protected override List<AttachedDocument> GetAttachedDocuments(TEDIMessage message) => message.AttachedDocuments;

		protected override ZString GetMessageIdentifier(TEDIMessage message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected sealed override ZGuid GetEmailGroupPK(IRegistryItem registryItem, IGlbBranch branch)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				result = ((ImportGroupNotification)registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty)).SendGroupPK;
			}
			return result;
		}

		protected sealed override IRegistryItem GetEmailGroupRegistryItem() => DECustomsDataRegistry.Instance.SendImportAcknowledgements;

		protected void SkipSnapshotUpdate(JobDeclaration declaration) => declaration.SkipSnapshotUpdate = true;
	}
}
