using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD428LiquidationWrapper : DocBaseWrapper, IIDDLiquidation<IDD428DutiesAndTaxWrapper>
{
	protected IDD428LiquidationWrapper(GoodsShipmentItemType goodsShipmentItem, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factory) : base(goodsShipmentItem, factory)
	{
		this.goodsShipmentItem = goodsShipmentItem;
		this.dutiesAndTaxesSummaries = dutiesAndTaxesSummaries;
	}

	public static IDD428LiquidationWrapper New(GoodsShipmentItemType goodsShipmentItem, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factoryToWrap)
	{
		return new IDD428LiquidationWrapper(goodsShipmentItem, dutiesAndTaxesSummaries, factoryToWrap);
	}

	readonly GoodsShipmentItemType goodsShipmentItem;
	readonly ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;

	public DocBaseWrapperCollection<IDD428DutiesAndTaxWrapper> DutiesAndTax => dutiesAndTax ??= GetDutiesAndTax();
	public DocBaseWrapperCollection<IDD428DutiesAndTaxWrapper> dutiesAndTax;

	DocBaseWrapperCollection<IDD428DutiesAndTaxWrapper> GetDutiesAndTax()
	{
		var result = new IDDDutiesAndTaxWrapperCollection<IDD428DutiesAndTaxWrapper>(Factory);
		goodsShipmentItem?.DutiesAndTaxes?.ForEach(dutiesAndTax => result.Add(IDD428DutiesAndTaxWrapper.New(dutiesAndTax, dutiesAndTaxesSummaries, Factory)));
		return result;
	}
}
