using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public abstract class ExportMessageProcessor<TEDIMessage, TDataProvider> : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TDataProvider : IDataProvider
		where TEDIMessage : AesInboundEDIMessage<TDataProvider>
	{
		protected ExportMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAesSystem;

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
	}
}
