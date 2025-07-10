using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using CusEntryLine = Enterprise.Customs.BE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.BE.Business;

public class GoodsItem : ITGoodsItemImport
{
	public GoodsItem(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
		this.invoiceLine = entryLine.RandomLine as JobComInvoiceLine;
		DeliveryTerms = new TDeliveryTerms(entryLine.Header.InvoiceHeaders.First() as JobComInvoiceHeader);
		SupplementaryUnits = new TSupplementaryUnits(entryLine);
		CustomsTreatment = new CustomsTreatment(entryLine);
		Price = new MonetaryAmount(entryLine);
	}

	readonly CusEntryLine entryLine;
	readonly JobComInvoiceLine invoiceLine;

	public ITAccompanyingDocument[] AccompanyingDocument { get => Array.Empty<ITAccompanyingDocument>(); }
	public ITAdditionalInformationImportExport[] AdditionalInformation { get => Array.Empty<ITAdditionalInformationImportExport>(); }
	public ITOperator Consignee { get; }
	public ITOperator Consignor { get; }
	public ZString CUSCode { get => ZString.Empty; }
	public ITCustomsTreatmentImport CustomsTreatment { get; set; }
	public ZString destinationRegion { get => invoiceLine.ZG_RegionOfDestination; }
	public ITDV1Detail DV1Detail { get; }
	public ITGuaranteeImport Guarantee { get; }
	public ITOperator Intracom { get; }
	public ZString OriginCountry { get => invoiceLine.JI_CountryOfOrigin; }
	public ITPaymentVat PaymentVat { get; }
	ITPreviousDocumentImportExport previousDocument;
	public ITPreviousDocumentImportExport PreviousDocument
	{
		get
		{
			if (previousDocument == null)
			{
				var previousDOCBO = entryLine.PreviousDocuments.FirstOrDefault();
				if (previousDOCBO != null)
				{
					previousDocument = new PreviousDocumentImportExport(previousDOCBO);
				}
			}
			return previousDocument;
		}
	}
	public IMonetaryAmount Price { get; set; }
	public ITOperator Representative { get; }
	public ZDecimal StatisticalValueAdjustment { get => ZDecimal.Zero; }
	public ZBool StatisticalValueAdjustmentSpecified { get => ZBool.False; }
	public ZString Valuationindicators { get => ZString.Empty; }
	public ZString[] ChassisNumber { get => Array.Empty<ZString>(); }
	public ZString CommodityCode { get => invoiceLine.JI_Tariff; }
	public ZString[] ContainerIdentifier { get => entryLine.InvoiceLines.SelectMany(ji => (ji as JobComInvoiceLine).ContainersPivot).Select(co => ((CusContainerInvoiceLinePivot)co).ContainerNumber).Distinct().ToArray(); }
	public ITDeliveryTerms DeliveryTerms { get; set; }
	public ZString DestinationCountry { get => invoiceLine.ZG_CountryOfDestination; }
	public ZString FirstAdditionalCommodity { get; }
	public ZString GoodsDescription { get => entryLine.EffectiveDescription; }
	public ZDecimal GrossMass { get => entryLine.InvoiceLines.Sum(ji => (ji as JobComInvoiceLine).EffectiveGrossWeight.InKilogramsSafe); }
	public ZBool GrossMassSpecified { get => GrossMass.IsValid && !GrossMass.IsEmpty; }
	public ZString NationalAdditionalCommodity1 { get; }
	public ZString NationalAdditionalCommodity2 { get; }
	public ZString NationalAdditionalCommodity3 { get; }
	public ZDecimal NetMass { get => entryLine.EffectiveNetWeight.InKilogramsSafe; }
	public ITPackagingImportExport[] Packaging { get => PackagingImportExport.ExtractPackages(entryLine).ToArray(); }
	public ITProducedDocumentImportExport[] ProducedDocument { get => entryLine.InvoiceLines.SelectMany(ji => (ji as JobComInvoiceLine).SupportingDocuments.Cast<SupportingDocument>()).Select(csi => new ProducedDocumentImportExport(csi)).ToArray(); }
	public ZString SecondAdditionalCommodity { get; }
	public ZDecimal Sequence { get => new ZDecimal(entryLine.CL_LineNumber); }
	public ITSupplementaryUnits SupplementaryUnits { get; set; }
	public ITTransactionNature TransactionNature { get => new TransactionNature(entryLine.Header.InvoiceHeaders.First() as JobComInvoiceHeader); }
	public ZString Ucr { get => ZString.Empty; }
	public ZString Undg { get => ZString.Empty; }
}
