using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemDetailWrapper : DocumentWrapper
{
	public static EVVGoodsItemDetailWrapper New(IEvvGoodsItemDetail detail, BusinessObjectFactory factory)
		=> new EVVGoodsItemDetailWrapper(Argument.NotNull(detail, nameof(detail)), Argument.NotNull(factory, nameof(factory)));

	EVVGoodsItemDetailWrapper(IEvvGoodsItemDetail detail, BusinessObjectFactory factory)
		: base(detail, factory)
	{
		this.detail = detail;
	}

	readonly IEvvGoodsItemDetail detail;

	public ZString Name => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CHAdditionalInformation, ZDateTime.Now).GetDescriptionFromCode(detail.Name) ?? detail.Name;

	public ZString Value => detail.Value;
}
