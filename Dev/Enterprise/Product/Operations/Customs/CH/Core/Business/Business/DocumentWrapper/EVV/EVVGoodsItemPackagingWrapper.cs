using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemPackagingWrapper : DocumentWrapper
{
	const string EmptyProperty = "---";

	public static EVVGoodsItemPackagingWrapper New(IEvvGoodsItemPackaging packaging, BusinessObjectFactory factory)
		=> new EVVGoodsItemPackagingWrapper(Argument.NotNull(packaging, nameof(packaging)), Argument.NotNull(factory, nameof(factory)));

	EVVGoodsItemPackagingWrapper(IEvvGoodsItemPackaging packaging, BusinessObjectFactory factory)
		: base(packaging, factory)
	{
		this.packaging = packaging;
	}

	readonly IEvvGoodsItemPackaging packaging;

	public ZString PackagingType => string.IsNullOrEmpty(packaging.PackagingType)
		? EmptyProperty
		: RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, ZDateTime.Now).GetDescriptionFromCode(packaging.PackagingType) ?? packaging.PackagingType;

	public ZString Quantity => string.IsNullOrEmpty(packaging.Quantity) ? EmptyProperty : packaging.Quantity;

	public ZString PackagingReferenceNumber => string.IsNullOrEmpty(packaging.PackagingReferenceNumber) ? EmptyProperty : packaging.PackagingReferenceNumber;
}
