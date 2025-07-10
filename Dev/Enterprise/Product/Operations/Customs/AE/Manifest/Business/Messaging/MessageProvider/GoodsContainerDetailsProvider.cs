namespace Enterprise.Customs.AE.Manifest.Business;

sealed class GoodsContainerDetailsProvider : IGoodsContainerDetailsProvider
{
	public GoodsContainerDetailsProvider(string containerId, int packageQuantity)
	{
		ContainerIdentifier = containerId;
		PackageQuantity = packageQuantity;
	}

	public string ContainerIdentifier { get; }

	public int PackageQuantity { get; }
}
