using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD429LiquidationWrapper : DocBaseWrapper, IIDDLiquidation<IDD429DutiesAndTaxWrapper>
{
	protected IDD429LiquidationWrapper(GoodsShipmentItemType goodsShipmentItem, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factory) : base(goodsShipmentItem, factory)
	{
		this.goodsShipmentItem = goodsShipmentItem;
		this.dutiesAndTaxesSummaries = dutiesAndTaxesSummaries;
	}

	public static IDD429LiquidationWrapper New(GoodsShipmentItemType goodsShipmentItem, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factoryToWrap)
	{
		return new IDD429LiquidationWrapper(goodsShipmentItem, dutiesAndTaxesSummaries, factoryToWrap);
	}

	readonly GoodsShipmentItemType goodsShipmentItem;
	readonly ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;

	public DocBaseWrapperCollection<IDD429DutiesAndTaxWrapper> DutiesAndTax => dutiesAndTax ??= GetDutiesAndTax();
	public DocBaseWrapperCollection<IDD429DutiesAndTaxWrapper> dutiesAndTax;

	DocBaseWrapperCollection<IDD429DutiesAndTaxWrapper> GetDutiesAndTax()
	{
		var result = new IDDDutiesAndTaxWrapperCollection<IDD429DutiesAndTaxWrapper>(Factory);
		goodsShipmentItem?.DutiesAndTaxes?.ForEach(dutiesAndTax => result.Add(IDD429DutiesAndTaxWrapper.New(dutiesAndTax, dutiesAndTaxesSummaries, Factory)));
		return result;
	}
}
