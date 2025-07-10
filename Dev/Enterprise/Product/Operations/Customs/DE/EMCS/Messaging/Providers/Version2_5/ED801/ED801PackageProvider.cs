using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED801PackageProvider : IEMCSPackageInComing
	{
		public ED801PackageProvider(ED801EBodyEadContainerBodyEadEsadPackage package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}
		readonly ED801EBodyEadContainerBodyEadEsadPackage package;

		public ZString KindOfPackages => package.KindOfPackages;
		public ZLong NumberOfPackages => ZLong.ParseSafe(package.NumberOfPackages, 0);
		public ZString SealNumber => package.CommercialSealIdentification;
		public ZString SealInformation => package.SealInformation;
		public ZString ShippingMarks => package.ShippingMarks;
		public ZBool IsNumberOfPackagesProvided => package.NumberOfPackages != null;
	}
}
