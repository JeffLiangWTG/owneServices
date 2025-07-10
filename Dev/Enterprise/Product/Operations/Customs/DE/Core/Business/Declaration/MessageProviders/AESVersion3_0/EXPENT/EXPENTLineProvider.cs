using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPENTLineProvider : AESLineProvider, IEXPENTLine
	{
		public EXPENTLineProvider(CusEntryLine entryLine, ExportEntryMessageSendingAction action, IEXPENTHeader headerProvider) : base(entryLine)
		{
			this.action = Argument.NotNull(action, nameof(action));
			this.headerProvider = Argument.NotNull(headerProvider, nameof(headerProvider));
		}
		readonly ExportEntryMessageSendingAction action;
		readonly IEXPENTHeader headerProvider;

		public IAESParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () =>
		{
			IAESParty consignee = null;
			if (action.SubmitConsignee && headerProvider.Consignee == null)
			{
				consignee = PartyProvider.NewOrNull(RandomInvoiceLine.ConsigneeAddress);
			}

			return consignee;
		});
		CachedValue<IAESParty> consigneeCached;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					var filteredPreviousDocuments = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(l => l.PreviousDocuments.Cast<PreviousDocument>());
					var createTimeOfLatestExportDataMessage = ((EXPENTHeaderProvider)headerProvider).CreateTimeOfLatestExportDataMessage;
					if (!createTimeOfLatestExportDataMessage.IsEmpty)
					{
						filteredPreviousDocuments = filteredPreviousDocuments.Where(x => x.CSI_SystemCreateTimeUtc > createTimeOfLatestExportDataMessage);
					}
					previousDocuments = filteredPreviousDocuments.Select(PreviousDocumentProvider.NewOrNull).ToArray();
				}
				return previousDocuments;
			}
		}
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IDeliveryTerms DeliveryTerms => null;

		public string TransactionType => CachedValueHelper.GetValue(ref transactionType, () => !EntryHeader.GetAllSameValue(x => x.JZ_ValuationCode).IsEmpty ? null : RandomInvoiceHeader?.JZ_ValuationCode.ValueOrNullIfEmpty());
		CachedValue<string> transactionType;

		public string CountryOfOrigin => RandomInvoiceLine.JI_CountryOfOrigin;

		public string RegionOfDispatch => RandomInvoiceLine.JI_StateOrRegionOfOrigin;
	}
}
