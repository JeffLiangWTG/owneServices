using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD426LiquidationWrapper : DocBaseWrapper, IIDDLiquidation<IDD426DutiesAndTaxWrapper>
{
	protected IDD426LiquidationWrapper(GoodsShipmentItemType goodsShipmentItem, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factory) : base(goodsShipmentItem, factory)
	{
		this.goodsShipmentItem = goodsShipmentItem;
		this.dutiesAndTaxesSummaries = dutiesAndTaxesSummaries;
	}

	public static IDD426LiquidationWrapper New(GoodsShipmentItemType goodsShipmentItem, ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries, BusinessObjectFactory factoryToWrap)
	{
		return new IDD426LiquidationWrapper(goodsShipmentItem, dutiesAndTaxesSummaries, factoryToWrap);
	}

	readonly GoodsShipmentItemType goodsShipmentItem;
	readonly ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;

	public DocBaseWrapperCollection<IDD426DutiesAndTaxWrapper> DutiesAndTax => dutiesAndTax ??= GetDutiesAndTax();
	public DocBaseWrapperCollection<IDD426DutiesAndTaxWrapper> dutiesAndTax;

	DocBaseWrapperCollection<IDD426DutiesAndTaxWrapper> GetDutiesAndTax()
	{
		var result = new IDDDutiesAndTaxWrapperCollection<IDD426DutiesAndTaxWrapper>(Factory);
		goodsShipmentItem?.DutiesAndTaxes?.ForEach(dutiesAndTax => result.Add(IDD426DutiesAndTaxWrapper.New(dutiesAndTax, dutiesAndTaxesSummaries, Factory)));
		return result;
	}
}
