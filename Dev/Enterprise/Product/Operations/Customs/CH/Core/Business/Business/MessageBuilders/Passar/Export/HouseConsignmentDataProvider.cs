using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class HouseConsignmentDataProvider : IHouseConsignment
{
	public static HouseConsignmentDataProvider New(CusEntryHeader entryHeader) => entryHeader == null ? null : new HouseConsignmentDataProvider(entryHeader);

	HouseConsignmentDataProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = entryHeader;
	}
	readonly CusEntryHeader entryHeader;

	public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => consignmentItems ?? (consignmentItems = (entryHeader.MergedLines.Cast<CusEntryLine>().Select(entryLine => ConsignmentItemDataProvider.New(entryLine)).ToArray()));
	IReadOnlyCollection<IConsignmentItem> consignmentItems;

	#region Unmapped Properties

	public int? SequenceNumber => null;

	public string ReferenceNumberUCR => null;

	public string CountryOfDispatch => null;

	public string CountryOfDestination => null;

	public IConsignor Consignor => null;

	public IConsignee Consignee => null;

	public IReadOnlyCollection<IDocument> PreviousDocuments => null;

	public IReadOnlyCollection<IDocument> SupportingDocuments => null;

	public IReadOnlyCollection<IDocument> TransportDocuments => null;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => null;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => null;

	public IReadOnlyCollection<IPackaging> Packagings => null;

	public bool IsPartialRelease => false;

	public bool IsFullRelease => false;

	public bool IsBlocked => false;

	public decimal GrossMass => 0.0m;

	public string SecurityIndicatorFromExportDeclaration => null;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => null;

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => null;

	public string TransportChargesMethodOfPayment => null;

	#endregion
}
