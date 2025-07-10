using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration.Customs.TW;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
{
	public static List<CusRefPacks> GetRefCusPackList(BusinessObjectFactory factory, ZString commercialPack)
	{
		return CusRefPacksHelper.LoadFilteredRefPacks(factory, Core.Constants.CountryCodes.India, RPTypeList.Codes.CommercialInvoice, commercialPack);
	}

	public override CodeDescriptionPairList GetCIPCustomsPackList(BusinessObjectFactory factory, ZString country) => GetINCIPPackList(factory);

	public override CodeDescriptionPairList GetCommercialPackList(BusinessObjectFactory factory, ZString type) => type == RPTypeList.Codes.CommercialInvoice
			? GetINCIPPackList(factory)
			: base.GetCommercialPackList(factory, type);

	CodeDescriptionPairList GetINCIPPackList(BusinessObjectFactory factory)
	{
		return UniversalReferenceDataHelper.GetCustomsUnitOfQuantityList(factory, ZDateTime.Today);
	}
}
