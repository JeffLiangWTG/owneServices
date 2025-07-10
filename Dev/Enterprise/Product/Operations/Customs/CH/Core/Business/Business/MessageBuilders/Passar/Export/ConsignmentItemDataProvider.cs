using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class ConsignmentItemDataProvider : IConsignmentItem
{
	public static ConsignmentItemDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new ConsignmentItemDataProvider(entryLine);

	ConsignmentItemDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
	}
	readonly CusEntryLine entryLine;

	public int GoodsItemNumber => entryLine.CL_LineNumber;

	public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR, () =>
	{
		var ucr = entryLine.RandomLine.InvoiceHeader.JZ_UCR;
		if (ucr.IsEmpty)
		{
			ucr = entryLine.Declaration.JE_UCR;
			if (!ucr.IsEmpty && entryLine.Header.InvoiceHeaders.All(x => x.JZ_UCR.IsEmpty))
			{
				ucr = ZString.Empty;
			}
		}
		return ucr.ReturnNullIfEmpty();
	});
	CachedValue<string> referenceNumberUCR;

	public ICommodity Commodity => commodity ?? (commodity = CommodityDataProvider.New(entryLine));
	ICommodity commodity;

	public IReadOnlyCollection<IPackaging> Packagings => packagings ?? (packagings = ConsignmentItemPackagingDataProvider.NewCollection(entryLine).ToArray());
	IReadOnlyCollection<IPackaging> packagings;

	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??= DocumentDataProvider.NewCollection(entryLine.InvoiceLines
		.Cast<JobComInvoiceLine>()
		.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>())
		.DistinctBy(d => new { d.CSI_Code, d.CSI_ReferenceNumber }))
		.ToArray();
	IReadOnlyCollection<IDocument> previousDocuments;

	public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??= DocumentDataProvider.NewCollection(entryLine.InvoiceLines
		.Cast<JobComInvoiceLine>()
		.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>())
		.DistinctBy(d => new { d.CSI_Code, d.CSI_ReferenceNumber }))
		.ToArray();
	IReadOnlyCollection<IDocument> supportingDocuments;

	public IRefinement Refinement => refinement ?? (refinement = ConsignmentItemRefinementDataProvider.New(entryLine));
	IRefinement refinement;

	public IRefund Refund => refund ?? (refund = RefundDataProvider.New(entryLine.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(l => l.IsReturnedGoodsWithRefundRequest)));
	IRefund refund;

	#region Unmapped Properties

	public int? DeclarationGoodsItemNumber => null;

	public string CountryOfDispatch => null;

	public string CountryOfDestination => null;

	public string DeclarationType => null;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => null;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => null;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => null;

	public string UnloadingRemarkCode => null;

	public string UnloadingRemarkText => null;

	public bool IsReleased => false;

	public bool IsBlocked => false;

	public IConsignee Consignee => null;

	public IReadOnlyCollection<IDocument> TransportDocuments => null;

	public ITransportCharges TransportCharges => null;

	#endregion
}
