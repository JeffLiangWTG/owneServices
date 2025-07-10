using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801PackageProvider : IEMCSPackageInComing
	{
		public IE801PackageProvider(PackageType package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}
		readonly PackageType package;

		public ZString KindOfPackages => package.KindOfPackages;

		public ZLong NumberOfPackages => ZLong.ParseSafe(package.NumberOfPackages, 0);

		public ZString SealNumber => package.CommercialSealIdentification;

		public ZString SealInformation => package.SealInformation?.Value;

		public ZString ShippingMarks => package.ShippingMarks;

		public ZBool IsNumberOfPackagesProvided => package.NumberOfPackages != null;
	}
}
