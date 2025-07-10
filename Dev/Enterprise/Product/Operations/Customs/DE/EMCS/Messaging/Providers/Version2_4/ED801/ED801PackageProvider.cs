using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801PackageProvider : IEMCSPackageInComing
	{
		public ED801PackageProvider(ED801DBodyEadContainerBodyEadPackage package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}
		readonly ED801DBodyEadContainerBodyEadPackage package;

		public ZString KindOfPackages => package.KindOfPackages;
		public ZLong NumberOfPackages => ZLong.ParseSafe(package.NumberOfPackages, 0);
		public ZString SealNumber => package.CommercialSealIdentification;
		public ZString SealInformation => package.SealInformation;
		public ZString ShippingMarks => package.ShippingMarks;
		public ZBool IsNumberOfPackagesProvided => package.NumberOfPackages != null;
	}
}
