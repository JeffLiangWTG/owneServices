using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business
{
	public abstract class MonthlyClosingDecBodyProvider : IMonthlyClosingDecBody
	{
		protected MonthlyClosingDecBodyProvider(CusReconEntry entry, bool isModificationMessage)
		{
			Entry = Argument.NotNull(entry, nameof(entry));
			IsModificationMessage = isModificationMessage;
		}
		protected readonly CusReconEntry Entry;
		protected readonly bool IsModificationMessage;

		public string ReferenceNumber => Entry.CRE_OriginalEntryNumber;

		public IImportParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => ImportPartyProvider.NewOrNull(IsConsigneeSpecified() ? ImporterAddress : null));
		CachedValue<IImportParty> consigneeCached;

		public Guid ConsigneePK => CachedValueHelper.GetValue(ref consigneePKCached, () => GetConsigneePK());
		CachedValue<Guid> consigneePKCached;

		public string DeliveryTermsCode => CachedValueHelper.GetValue(ref deliveryTermsCode, () => RandomInvoiceHeader?.JZ_IncoTerm ?? string.Empty);
		CachedValue<string> deliveryTermsCode;

		public string DeliveryTermsDescription => CachedValueHelper.GetValue(ref deliveryTermsDescription, () =>
		{
			var description = string.Empty;
			if (DeliveryTermsCode == Core.Constants.IncoTerms.Other)
			{
				description = RandomInvoiceHeader?.JZ_IncoTermDescription;
			}
			return description;
		});
		CachedValue<string> deliveryTermsDescription;

		public string DeliveryTermsPlace => CachedValueHelper.GetValue(ref deliveryTermsPlace, () => RandomInvoiceHeader?.JZ_IncoTermPlace ?? string.Empty);
		CachedValue<string> deliveryTermsPlace;

		public string DeliveryTermsKey => CachedValueHelper.GetValue(ref deliveryTermsKey, () => RandomInvoiceHeader?.ZG_AgreedPlaceCode ?? string.Empty);
		CachedValue<string> deliveryTermsKey;

		public IMoney PaymentTransaction => CachedValueHelper.GetValue(ref paymentTransaction, () => new MoneyProvider(EntryInstruction, RandomInvoiceHeader));
		CachedValue<IMoney> paymentTransaction;

		public string ForeignTradeStatisticsEntryCustomsOffice => CachedValueHelper.GetValue(ref foreignTradeStatisticsEntryCustomsOffice,
			() => Declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent));
		CachedValue<string> foreignTradeStatisticsEntryCustomsOffice;

		public ICustomsValue CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => Declaration.ZG_IsHighValueOvrd ? new CustomsValueProvider(EntryHeader) : null);
		CachedValue<ICustomsValue> customsValue;

		public IReadOnlyCollection<IImportDocument> Documents => documents ?? (documents = GetDocuments());

		IImportDocument[] GetDocuments()
		{
			var result = Array.Empty<IImportDocument>();
			if (IsModificationMessage)
			{
				result = IEnumerableExtensions.DistinctBy(EntryInstruction.Invoices
					.SelectMany(i => i.SupportingDocuments.Cast<SupportingDocument>())
					.Where(d => !d.CSI_Code.IsEmpty)
					, d => new { d.CSI_Code, d.CSI_ReferenceNumber, d.CSI_DateOfIssue })
					.Select(d => new ImportDocumentProvider(d))
					.ToArray();
			}
			else
			{
				if (CurrentSnapshot.Document != null)
				{
					result = CurrentSnapshot.Document.Select(d => new EntryDocumentFromSnapshotProvider(d)).ToArray();
				}
			}
			return result;
		}

		IImportDocument[] documents;

		DEMonthlyClosingEntrySnapshot CurrentSnapshot => CachedValueHelper.GetValue(ref currentSnapshot, () => LoadCurrentSnapshot());
		CachedValue<DEMonthlyClosingEntrySnapshot> currentSnapshot;

		DEMonthlyClosingEntrySnapshot LoadCurrentSnapshot()
		{
			var result = new DEMonthlyClosingEntrySnapshot();
			var existingCurrentEntrySnapshot = Entry.CurrentSnapshot;

			if (existingCurrentEntrySnapshot != null)
			{
				result = CusReconEntrySnapshotBuilder.Deserialize(existingCurrentEntrySnapshot.CRS_SnapshotXml);
			}

			return result;
		}

		protected BusinessObjectFactory Factory => Entry.Factory;

		protected CusEntryHeader EntryHeader => CachedValueHelper.GetValue(ref entryHeader, () => (CusEntryHeader)Entry.EntryHeader);
		CachedValue<CusEntryHeader> entryHeader;

		protected JobDeclaration Declaration => CachedValueHelper.GetValue(ref declaration, () => EntryHeader.Declaration);
		CachedValue<JobDeclaration> declaration;

		protected CusEntryInstruction EntryInstruction => CachedValueHelper.GetValue(ref entryInstruction, () => EntryHeader.EntryInstruction);
		CachedValue<CusEntryInstruction> entryInstruction;

		protected JobComInvoiceHeader RandomInvoiceHeader => CachedValueHelper.GetValue(ref randomInvoiceHeader, () => (JobComInvoiceHeader)EntryInstruction.InvoiceLines.FirstOrDefault()?.InvoiceHeader);
		CachedValue<JobComInvoiceHeader> randomInvoiceHeader;

		protected virtual bool IsModificationStatus(ZString type) => ModificationMessageStatusSet.Contains(type);

		protected IEnumerable<CusReconEntryLine> GetLinesQuery()
		{
			var linesQuery = Entry.CusReconEntryLines.Cast<CusReconEntryLine>();
			var entryHasCurrentSnapshot = Entry.CurrentSnapshot != null;
			if (IsModificationMessage)
			{
				// CRL_CustomsStatus IN(‘RC2’,’ERR’,’TX4’) and (found a CusReconSnapshot record with CRS_Type = ‘CUR’ for the related CusReconEntry OR found a CusReconSnapshot record with CRS_Type = ‘CUR’ for this CusReconEntryLine)
				linesQuery = linesQuery.Where(l => IsModificationStatus(l.CRL_CustomsStatus) && (entryHasCurrentSnapshot || l.CurrentSnapshot != null));
			}
			else
			{
				// CusReconEntryLines with CRL_CustomsStatus = ‘ ’ 
				// OR
				// CRL_CustomsStatus = ‘REJ’ and(found a CusReconSnapshot record with CRS_Type = ‘CUR’ for the related CusReconEntry OR found a CusReconSnapshot record with CRS_Type = ‘CUR’ for this CusReconEntryLine).
				linesQuery = linesQuery.Where(l => l.CRL_CustomsStatus.IsEmpty || (l.CRL_CustomsStatus == EntryStatus.REJ && (entryHasCurrentSnapshot || l.CurrentSnapshot != null)));
			}

			return linesQuery;
		}

		protected OrgAddress SupplierDocumentaryAddress => Declaration.SupplierDocumentaryAddress.Address;

		OrgAddress ImporterAddress => Entry.ImporterAddress;

		bool IsConsigneeSpecified()
		{
			var reconDeclaration = Entry.ReconDeclaration;
			return reconDeclaration?.DeclarantAddress?.OA_OH != reconDeclaration?.ImporterAddress?.OA_OH;
		}

		Guid GetConsigneePK()
		{
			var result = Guid.Empty;
			if (IsConsigneeSpecified() && ImporterAddress != null)
			{
				result = ImporterAddress.PK.ToGuid();
			}
			return result;
		}

		static readonly ImmutableHashSet<string> ModificationMessageStatusSet = ImmutableHashSet.Create(EntryStatus.RC2, EntryStatus.ERR, EntryStatus.TX4);
	}
}
