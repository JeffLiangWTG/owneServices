using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ConsignmentDataProvider : IConsignment
{
	public static ConsignmentDataProvider New(CusEntryHeader entryHeader) => entryHeader == null ? null : new ConsignmentDataProvider(entryHeader);

	ConsignmentDataProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = entryHeader;
		declaration = entryHeader.Declaration;
	}
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;

	public bool ContainerIndicator => containerIndicator ??= !entryHeader.EntryInstruction.IsSimplified && ContainersPivot().Any(x => x.C2_JI.In(entryHeader.EntryInstruction.InvoiceLines.Select(x => x.PK)));
	bool? containerIndicator;

	public string CountryOfDestination => declaration.JE_GoodsDestination;

	public string CountryOfDispatch => null;

	public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR, () =>
		declaration.JE_UCR.IsEmpty || entryHeader.InvoiceHeaders.Any(x => !x.JZ_UCR.IsEmpty) ? null : declaration.JE_UCR.ToString());
	CachedValue<string> referenceNumberUCR;

	public bool? PartialDelivery => entryHeader.EntryInstruction.IsSimplified ? null : entryHeader.EntryInstruction.CEI_PartialDelivery;

	public bool Preference => preference ??= SupportingDocuments.Any(doc => UniversalReferenceConstants.SupportingDocumentTypeCodes.IsSupportingDocumentWithPreference(doc.Type));
	bool? preference;

	public IConsignor Consignor => consignor ??= entryHeader.EntryInstruction.IsSimplified ? null : ConsignorDataProvider.New(declaration.ConsignorDocAddress, contactType: OrgConstants.ContactAllocationType.CUS, getContactByName: true);
	IConsignor consignor;

	public IConsignee Consignee => consignee ??= ConsigneeDataProvider.New(declaration.ImporterDocumentaryAddress);
	IConsignee consignee;

	public IPlaceOfLoadingOrUnloading PlaceOfLoading => null;

	public IPlaceOfLoadingOrUnloading PlaceOfUnloading => null;

	public IExporter Exporter => exporter ??= ExporterDataProvider.New(declaration.SupplierDocumentaryAddress);
	IExporter exporter;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipments => entryHeader.EntryInstruction.IsSimplified ? null : transportEquipments ??= TransportEquipmentDataProvider.NewCollection(GetLinkedContainers()).ToArray();
	IReadOnlyCollection<ITransportEquipment> transportEquipments;

	public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => null;

	public IReadOnlyCollection<IHouseConsignment> HouseConsignments => houseConsignments ??= new[] { HouseConsignmentDataProvider.New(entryHeader) };
	IReadOnlyCollection<IHouseConsignment> houseConsignments;

	public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments => null;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= AdditionalSupplyChainActorDataProvider.NewCollection(entryHeader.EntryInstruction?.SupplyChainActors.Cast<CusSupplyChainActorReference>()).ToArray();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

#if NETFRAMEWORK
	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??= DocumentDataProvider.NewCollection(entryHeader.EntryInstruction.PreviousDocuments
		.Cast<PreviousDocument>()
		.Concat(entryHeader.InvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>()))
		.DistinctBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber }))
		.ToArray();
#else
	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??= DocumentDataProvider.NewCollection(IEnumerableExtensions.DistinctBy(entryHeader.EntryInstruction.PreviousDocuments
			.Cast<PreviousDocument>()
			.Concat(entryHeader.InvoiceHeaders.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>())),
			x => new { x.CSI_Code, x.CSI_ReferenceNumber }))
		.ToArray();
#endif
	IReadOnlyCollection<IDocument> previousDocuments;

#if NETFRAMEWORK
	public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??= DocumentDataProvider.NewCollection(entryHeader.EntryInstruction.SupportingDocuments
		.Cast<SupportingDocument>()
		.Concat(entryHeader.InvoiceHeaders.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>()))
		.DistinctBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber }))
		.ToArray();
#else
	public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??= DocumentDataProvider.NewCollection(IEnumerableExtensions.DistinctBy(entryHeader.EntryInstruction.SupportingDocuments
			.Cast<SupportingDocument>()
			.Concat(entryHeader.InvoiceHeaders.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>())),
			x => new { x.CSI_Code, x.CSI_ReferenceNumber }))
		.ToArray();
#endif
	IReadOnlyCollection<IDocument> supportingDocuments;

#if NETFRAMEWORK
	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= DocumentDataProvider.NewCollection(entryHeader.EntryInstruction.TransportDocuments
		.Cast<TransportDocument>()
		.Concat(entryHeader.InvoiceHeaders.SelectMany(x => x.TransportDocuments.Cast<TransportDocument>()))
		.DistinctBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber }))
		.ToArray();
#else
	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= DocumentDataProvider.NewCollection(IEnumerableExtensions.DistinctBy(entryHeader.EntryInstruction.TransportDocuments
			.Cast<TransportDocument>()
			.Concat(entryHeader.InvoiceHeaders.SelectMany(x => x.TransportDocuments.Cast<TransportDocument>())),
			x => new { x.CSI_Code, x.CSI_ReferenceNumber }))
		.ToArray();
#endif
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= AdditionalInformationDataProvider.NewCollection(entryHeader.EntryInstruction?.AdditionalInformations, generateSequenceNumber: true).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => null;

	public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => null;

	public IReadOnlyCollection<ITransportCharges> TransportCharges => entryHeader.EntryInstruction.IsSimplified ? null : transportCharges ??= [TransportChargesDataProvider.New(entryHeader.EntryInstruction)];
	IReadOnlyCollection<ITransportCharges> transportCharges;

	public IImporter Importer => null;

	public IPartialShipment PartialShipment => null;

	IEnumerable<BaseCusContainer> GetLinkedContainers() => declaration.CusContainers.Where(x => x.PK.In(ContainersPivot().Select(x => x.C2_CO)));

	IEnumerable<CusContainerInvoiceLinePivot> ContainersPivot() => declaration.Invoices
		.SelectMany(x => x.InvoiceLines)
		.Cast<JobComInvoiceLine>()
		.SelectMany(x => x.ContainersPivot)
		.Cast<CusContainerInvoiceLinePivot>();
}
