using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPENTHeaderProvider : AESHeaderProvider, IEXPENTHeader
	{
		public EXPENTHeaderProvider(ExportEntryMessageSendingAction action) : base(action?.MessagingObject)
		{
			this.action = Argument.NotNull(action, nameof(action));
		}
		readonly ExportEntryMessageSendingAction action;

		bool IAESHeader.IsContainerized => GetIsContainerizedCore;

		bool GetIsContainerizedCore => base.IsContainerized && Declaration.JE_ContainerMode != Core.Constants.ContainerModes.LCL;

		IReadOnlyCollection<ITransportEquipment> IAESHeader.TransportEquipments => transportEquipments ?? (transportEquipments = GetIsContainerizedCore ? Declaration.CusContainers
			.Where(x => !x.CO_ContainerNumber.IsEmpty).Select(x => TransportEquipmentProvider.NewOrNull(EntryInstruction, x)).ToArray() : Array.Empty<ITransportEquipment>());
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		public IAESParty Consignee => CachedValueHelper.GetValue(ref consignee, () => action.SubmitConsignee && EntryHeader.InvoiceLines.AllSame(x => x.JI_OA_ConsigneeAddress) ? PartyProvider.NewOrNull(Declaration.ImporterDocumentaryAddress.Address) : null);
		CachedValue<IAESParty> consignee;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					var filteredPreviousDocuments = DistinctInvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>());
					if (!CreateTimeOfLatestExportDataMessage.IsEmpty)
					{
						filteredPreviousDocuments = filteredPreviousDocuments.Where(x => x.CSI_SystemCreateTimeUtc > CreateTimeOfLatestExportDataMessage);
					}
					previousDocuments = filteredPreviousDocuments.Select(PreviousDocumentProvider.NewOrNull).ToArray();
				}
				return previousDocuments;
			}
		}
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<IEXPENTLine> Lines
		{
			get
			{
				if (lines == null)
				{
					var shouldSendCusEntryLinePKs = action.EntryLines.Cast<ExportEntryLine>().Where(x => x.ShouldSend).Select(x => x.MessagingObject.PK);
					lines = EntryHeader.MergedLines.Where(x => shouldSendCusEntryLinePKs.Contains(x.PK)).Select(x => new EXPENTLineProvider(x, action, this)).ToArray();
				}
				return lines;
			}
		}
		IReadOnlyCollection<IEXPENTLine> lines;

		internal ZDateTime CreateTimeOfLatestExportDataMessage => CachedValueHelper.GetValue(ref createTimeOfLatestExportDataMessageCached, () => EntryHeader.Messages
			.Cast<DEEDIMessage>()
			.Where(x => x.EM_ApplicationReference.StartsWith(ExportDataApplicationReferencePrefix))
			.OrderByDescending(x => x.EM_SystemCreateTimeUtc)
			.FirstOrDefault()?.EM_SystemCreateTimeUtc ?? ZDateTime.Empty);
		CachedValue<ZDateTime> createTimeOfLatestExportDataMessageCached;

		const string ExportDataApplicationReferencePrefix = "DEXPD";

		protected override IDeliveryTerms GetDeliveryTermsFromInvoice(JobComInvoiceHeader invoice)
		{
			return new AESHeaderDeliveryTermsProvider(invoice);
		}
	}
}
