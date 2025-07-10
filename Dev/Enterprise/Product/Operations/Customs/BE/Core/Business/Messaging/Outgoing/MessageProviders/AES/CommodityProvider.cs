using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CommodityProvider : ICommodity
{
	readonly JobComInvoiceLine invoiceLine;

	public CommodityProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	public string DescriptionOfGoods => invoiceLine.JI_Description;

	public string CusCode => invoiceLine.ZG_CusNumber;

	public string HarmonizedSystemSubHeadingCode => ZString.Empty;

	public string CombinedNomenclatureCode => ZString.Empty;

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = invoiceLine.UNDGs.Select((x, i) => new DangerousGoodsProvider(x, i + 1)).ToArray<IDangerousGoods>());
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	public decimal GrossMass => 0m;

	public decimal? NetMass => null;

	public decimal SupplementaryQty => 0m;

	public ICalculationOfTaxes CalculationOfTaxes => calculationOfTaxes ?? (calculationOfTaxes = new CalculationOfTaxesProvider(invoiceLine.CusEntryLine));
	ICalculationOfTaxes calculationOfTaxes;

	public ICommodityCode CommodityCode => commodityCode ?? (commodityCode = new CommodityCodeProvider(invoiceLine));
	ICommodityCode commodityCode;

	public IGoodsMeasure GoodsMeasure => goodsMeasure ?? (goodsMeasure = new GoodsMeasureProvider((CusEntryLine)invoiceLine.CusEntryLine));

	public decimal InvoiceLine => invoiceLine.JI_LinePrice;

	public string QuotaOrderNumber => invoiceLine.JI_ConcessionOrder;

	public string TypeOfGoods => null;

	IGoodsMeasure goodsMeasure;
}
