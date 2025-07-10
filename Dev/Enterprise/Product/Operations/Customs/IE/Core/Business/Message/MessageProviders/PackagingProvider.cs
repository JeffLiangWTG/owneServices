using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.Business
{
	public class PackagingProvider : IPackaging
	{
		public PackagingProvider(string type, int quantity, string shippingMarks)
		{
			PackageType = type;
			PackageQuantity = quantity;
			ShippingMarks = shippingMarks;
		}

		public string PackageType { get; }

		public int? PackageQuantity { get; }

		public string ShippingMarks { get; }
	}
}
