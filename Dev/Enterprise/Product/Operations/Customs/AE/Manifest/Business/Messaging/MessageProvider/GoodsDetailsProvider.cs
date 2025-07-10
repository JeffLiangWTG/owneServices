using CargoWise.Common;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class GoodsDetailsProvider : IGoodsDetailsProvider
{
	public GoodsDetailsProvider(AsycudaPack asycudaPack)
	{
		Pack = Argument.NotNull(asycudaPack, nameof(asycudaPack));
	}
	AsycudaPack Pack { get; }

	public int PackageQuantity => packageQuantity ??= Pack.APA_PackQty;
	int? packageQuantity;

	public string PackageType => packageType ??= GetPackageType();
	string packageType;

	public string PackageTypeCode => packageTypeCode ??= GetPackageTypeCode();
	string packageTypeCode;

	public int PackageLineNo => packageLineNo ??= Pack.APA_LineNo;
	int? packageLineNo;

	string GetPackageType() => Pack.Lookups.PackUQList.GetDescriptionFromCode(PackageTypeCode) ?? string.Empty;

	string GetPackageTypeCode() => Pack.APA_PackUQ;
}
