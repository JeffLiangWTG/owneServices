using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class CommodityDataProvider : ICommodity
{
	public static CommodityDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new CommodityDataProvider(entryLine);

	CommodityDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
	}
	readonly CusEntryLine entryLine;

	public string DescriptionOfGoods => entryLine.CL_Description;

	public string CUSCode => null;

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ?? (dangerousGoods = DangerousGoodsDataProvider.NewCollection(entryLine)?.ToArray());
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	public IGoodsMeasure GoodsMeasure => goodsMeasure ?? (goodsMeasure = GoodsMeasureDataProvider.New(entryLine));
	IGoodsMeasure goodsMeasure;

	public ICommodityCode CommodityCode => entryLine.Header.EntryInstruction.IsSimplified ? null : commodityCode ??= CommodityCodeDataProvider.New(entryLine);
	ICommodityCode commodityCode;

	public ICommoditySpecification CommoditySpecification => commoditySpecification ?? (commoditySpecification = CommoditySpecificationDataProvider.New(entryLine));
	ICommoditySpecification commoditySpecification;
}
