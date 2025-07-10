using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CUSWATHeaderProvider : ImportHeaderProvider, ICUSWATHeader
	{
		public CUSWATHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public string CustomsWarehouseDepartureLocalReferenceNumber => entryInstruction.PreviousDocumentMaster.CSI_ReferenceNumber2;

		public string CustomsWarehouseDeparture => entryInstruction.PreviousDocumentMaster.AuthorizationNumber;

		public string CurrentProcedure => CachedValueHelper.GetValue(ref currentProcedure, GetCW1OrCWPAuthorizationNumberFromDeclarantOrgHeader);
		CachedValue<string> currentProcedure;

		public string DeclarationPlace => GlbBranch.CurrentBranch.GB_City;

		public IPartyID WarehouseOwner => CachedValueHelper.GetValue(ref warehouseOwner, () => ImportPartyIDProvider.NewOrNull(Declaration.Declarant));
		CachedValue<IPartyID> warehouseOwner;

		public IImportPartyContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => ImportPartyContactPersonProvider.NewOrNull(GlbStaff.CurrentUser));
		CachedValue<IImportPartyContactPerson> contactPerson;

		public string DepartureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber => CachedValueHelper.GetValue(ref departureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber, () =>
		{
			var sixthToNinthDigits = CustomsWarehouseDeparture.SubstringOrNull(5, 4);
			return sixthToNinthDigits.IsEmpty() ? null : "DE00" + sixthToNinthDigits;
		});
		CachedValue<string> departureCustomsWarehouseSupervisingCustomsOfficeReferenceNumber;

		public IReadOnlyCollection<IImportDocument> Documents => documents ?? (documents =
			RandomInvoiceHeader != null
				? RandomInvoiceHeader
					.SupportingDocuments.Cast<SupportingDocument>()
					.Where(d => !d.CSI_Code.IsEmpty)
					.Select(d => new ImportDocumentProvider(d))
					.ToArray()
				: Array.Empty<IImportDocument>());
		IReadOnlyCollection<IImportDocument> documents;

		public IReadOnlyCollection<ICUSWATLine> Lines => lines ?? (lines = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(l => new CUSWATLineProvider(l)).ToArray());
		IReadOnlyCollection<ICUSWATLine> lines;

		JobComInvoiceHeader RandomInvoiceHeader => CachedValueHelper.GetValue(ref randomInvoiceHeader, () => (JobComInvoiceHeader)entryInstruction.InvoiceLines.FirstOrDefault()?.InvoiceHeader);
		CachedValue<JobComInvoiceHeader> randomInvoiceHeader;
	}
}
