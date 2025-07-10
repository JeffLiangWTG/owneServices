namespace Enterprise.Customs.AE.Manifest.Business;

public interface IGoodsContainerDetailsProvider
{
	string ContainerIdentifier { get; }

	int PackageQuantity { get; }
}
