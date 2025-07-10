using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE801PackageProvider : IEMCSPackageInComing
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
