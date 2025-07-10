using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemDutyAndTaxesWrapperCollection : DocumentWrapperCollectionWithLanguage<IEvvGoodsItemDutyOrTax, EVVGoodsItemDutyOrTaxWrapper>
{
	public static EVVGoodsItemDutyAndTaxesWrapperCollection New(IEnumerable<IEvvGoodsItemDutyOrTax> dutyAndTaxes, BusinessObjectFactory factory, ZString documentLanguage)
		=> new EVVGoodsItemDutyAndTaxesWrapperCollection(dutyAndTaxes.EmptyIfNull(), factory, documentLanguage);

	EVVGoodsItemDutyAndTaxesWrapperCollection(IEnumerable<IEvvGoodsItemDutyOrTax> dutyAndTaxes, BusinessObjectFactory factory, ZString documentLanguage) : base(dutyAndTaxes, factory, documentLanguage)
	{
	}

	protected override DocumentWrapper WrapObject(IEvvGoodsItemDutyOrTax objectToWrap, ZString documentLanguage)
	{
		return EVVGoodsItemDutyOrTaxWrapper.New(objectToWrap, Factory,documentLanguage);
	}
}
