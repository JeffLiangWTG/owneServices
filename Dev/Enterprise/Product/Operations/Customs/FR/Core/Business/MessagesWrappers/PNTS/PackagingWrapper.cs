using CargoWise.Common;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class PackagingWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IPackaging
	{
		PackagingWrapper(IAsycudaPack packaging)
		{
			this.packaging = Argument.NotNull(packaging, nameof(packaging));
		}

		readonly IAsycudaPack packaging;

		public string NumberOfPackages => numberOfPackages ?? (numberOfPackages = packaging.APA_PackQty.ToString());
		string numberOfPackages;

		public string ShippingMarks => shippingMarks ?? (shippingMarks = packaging.APA_MarksAndNumbers);
		string shippingMarks;

		public string TypeOfPackages => typeOfPackages ?? (typeOfPackages = packaging.APA_PackUQ);
		public string typeOfPackages;

		public static PackagingWrapper New(IAsycudaPack packaging) => packaging == null ? null : new PackagingWrapper(packaging);
	}
}
