using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class PackageWrapper : IPackage
	{
		public PackageWrapper(NctsPackage package)
		{
			this.package = Argument.NotNull(package, nameof(package));
		}

		public ZString MarksAndNumbersOfPackages => MarksAndNumbersOfPackagesCore;
		protected virtual ZString MarksAndNumbersOfPackagesCore => package.B5_MarksAndNumbers;

		public ZString MarksAndNumbersOfPackagesLanguage => ZString.Empty;

		public ZString KindOfPackages => KindOfPackagesCore;
		protected virtual ZString KindOfPackagesCore => package.B5_UnitType;

		public ZLong NumberOfUnits => package.B5_UnitCount;

		public ZLong NumberOfPackages => CachedValueHelper.GetValue(ref numberOfPackages, GetNumberOfPackages);
		CachedValue<ZLong> numberOfPackages;
		protected virtual ZLong GetNumberOfPackages() => !IsBulk && !IsUnpacked ? package.B5_UnitCount : ZLong.Zero;

		public ZLong NumberOfPieces => CachedValueHelper.GetValue(ref numberOfPieces, () => !IsBulk && IsUnpacked ? package.B5_UnitCount : ZLong.Zero);
		CachedValue<ZLong> numberOfPieces;

		public ZBool IsBulk => package.IsBulk;

		public ZBool IsUnpacked => package.IsUnpacked;

		protected readonly NctsPackage package;
	}
}
