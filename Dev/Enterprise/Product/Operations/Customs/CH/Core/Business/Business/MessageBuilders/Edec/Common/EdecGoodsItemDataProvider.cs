using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsItemDataProvider : IEdecGoodsItem
{
	protected EdecGoodsItemDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}
	protected readonly CusEntryLine entryLine;

	protected JobComInvoiceLine InvoiceLine => entryLine.RandomLine;

	protected IEnumerable<JobComInvoiceLine> InvoiceLines => entryLine.InvoiceLines.Cast<JobComInvoiceLine>();

	readonly string[] commodityCodeForConfirmation = new[] { "3501.1090", "3501.9091", "3501.9099", "3502.1190", "3502.1990" };

	public string TraderItemID => entryLine.CL_LineNumber.ToString();

	public string Description => entryLine.CL_Description.IsEmpty ? InvoiceLine.JI_Description : entryLine.CL_Description;

	public string CommodityCode => InvoiceLine.JI_FormattedTariff.Left(9);

	public bool CommodityCodeConfirmation => commodityCodeForConfirmation.Contains(CommodityCode);

	public virtual string StatisticalCode => null;

	public decimal GrossMass => entryLine.CalcGrossWeight;

	public bool GrossMassConfirmation => InvoiceLine.JI_GrossMassConfirmation;

	public decimal? NetMass => entryLine.CalcNetWeight.ReturnNullIfEmpty();

	public bool NetMassConfirmation => InvoiceLine.JI_NetMassConfirmation;

	public virtual decimal? CustomsNetWeight => null;

	public decimal? AdditionalUnit => entryLine.CalcAdditionalQty.ReturnNullIfEmpty();

	public bool AdditionalUnitConfirmation => InvoiceLine.JI_AdditionalUnitConfirmation;

	public virtual string StorageType => null;

	public string PermitObligation => InvoiceLine.JI_PermitObligation;

	public string NonCustomsLawObligation => InvoiceLine.JI_NonCustomsLawObligation;

	public IEdecGoodsItemStatistic Statistic => statistic ?? (statistic = EdecGoodsItemStatisticDataProvider.New(entryLine));
	IEdecGoodsItemStatistic statistic;

	public virtual IEdecGoodsItemOrigin Origin => null;

	public virtual IEdecGoodsItemValuation Valuation => null;

	public IEnumerable<IEdecGoodsItemDetail> GoodsItemDetails => goodsItemDetails ?? (goodsItemDetails = EdecGoodsItemDetailDataProvider.NewCollection(entryLine).ToArray());
	IEnumerable<IEdecGoodsItemDetail> goodsItemDetails;

	public IEnumerable<IEdecGoodsItemPackaging> Packagings => packagings ?? (packagings = EdecGoodsItemPackagingDataProvider.NewCollection(entryLine).ToArray());
	IEnumerable<IEdecGoodsItemPackaging> packagings;

	public IEnumerable<IEdecGoodsitemProducedDocument> ProducedDocuments => producedDocuments ?? (producedDocuments = EdecGoodsitemProducedDocumentDataProvider.NewCollection(entryLine).ToArray());
	IEnumerable<IEdecGoodsitemProducedDocument> producedDocuments;

	public IEnumerable<IEdecGoodsItemPermit> Permits => permits ?? (permits = GetPermitDataProviders());
	IEnumerable<IEdecGoodsItemPermit> permits;

	IEnumerable<IEdecGoodsItemPermit> GetPermitDataProviders()
	{
		return (from line in entryLine.InvoiceLines.Cast<JobComInvoiceLine>()
				from permit in line.Permits.Cast<Permit>()
				group permit by new
				{
					permit.CSI_Code,
					permit.CSI_IssuerType,
					permit.CSI_ReferenceNumber,
					permit.CSI_DateOfIssue,
					permit.CSI_Description,
					pkIfHasDetails = (permit.PermitItemDetails.Any() ? permit.PK : ZGuid.Empty)
				} into g
				select EdecPermitDataProvider.New(g.First())).ToArray();
	}

	public IEnumerable<IEdecSpecialMention> SpecialMentions => specialMentions ?? (specialMentions = EdecSpecialMentionDataProvider.NewCollection(entryLine.InvoiceLines.OfType<JobComInvoiceLine>())).ToArray();
	IEnumerable<IEdecSpecialMention> specialMentions;

	public virtual string UNDangerousGoodsCode => null;

	public IEnumerable<IEdecNonCustomsLaw> NonCustomsLaws => nonCustomsLaws ?? (nonCustomsLaws = EdecNonCustomsLawDataProvider.NewCollection(entryLine).ToArray());
	IEnumerable<EdecNonCustomsLawDataProvider> nonCustomsLaws;

	public virtual IEnumerable<IEdecFee> Fees => null;

	public virtual IEnumerable<IEdecAdditionalTax> AdditionalTaxes => null;

	public virtual IEdecRefund RefundType => null;

	public virtual IEdecSensibleGoods SensibleGoodsType => null;

	public IEdecRefinement Refinement => refinement ?? (refinement = EdecRefinementDataProvider.New(entryLine));
	IEdecRefinement refinement;

	public IEnumerable<IEdecNotification> Notifications => notifications ?? (notifications = EdecNotificationDataProvider.NewCollection(entryLine));
	IEnumerable<IEdecNotification> notifications;
}
