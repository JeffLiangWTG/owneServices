namespace Enterprise.Customs.AE.Manifest.Business;

public interface IGoodsDetailsProvider
{
	int PackageQuantity { get; }

	string PackageType { get; }

	string PackageTypeCode { get; }

	int PackageLineNo { get; }
}
