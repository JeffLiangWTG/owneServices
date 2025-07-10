using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class EdecIMPGoodsItemDataProvider : EdecGoodsItemDataProvider
{
	public static EdecIMPGoodsItemDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new EdecIMPGoodsItemDataProvider(entryLine);

	EdecIMPGoodsItemDataProvider(CusEntryLine entryLine) : base(entryLine)
	{
	}

	public override string StatisticalCode => InvoiceLine.JI_Procedure == ProcedureCodesEdec.ExemptFromDuty && !InvoiceLine.InAndOutwardProcessingRepair ? null : (string)InvoiceLine.StatisticalCode;

	public override decimal? CustomsNetWeight => entryLine.CalcCustomsNetWeight.ReturnNullIfEmpty();

	public override string StorageType => InvoiceLine.JI_StorageType;

	public override IEdecGoodsItemOrigin Origin => origin ?? (origin = EdecGoodsItemOriginDataProvider.New(entryLine));
	IEdecGoodsItemOrigin origin;

	public override IEdecGoodsItemValuation Valuation => valuation ?? (valuation = EdecGoodsItemValuationDataProvider.New(entryLine));
	IEdecGoodsItemValuation valuation;

	public override IEnumerable<IEdecFee> Fees => fees ?? (fees = EdecIMPFeeDataProvider.NewCollection(entryLine));
	IEnumerable<IEdecFee> fees;

	public override IEnumerable<IEdecAdditionalTax> AdditionalTaxes => additionalTaxes ?? (additionalTaxes = EdecAdditionalTaxDataProvider.NewCollection(entryLine).ToArray());
	IEnumerable<IEdecAdditionalTax> additionalTaxes;
}
