using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class PackagingProvider : BasePackagingProvider
	{
		public PackagingProvider(NctsPackage package) : base(package) { }

		public override string TypeOfPackages => Package.B5_UnitType;

		public override int? NumberOfPackages => Package.IsBulk ? null : (int?)Package.B5_UnitCount.ToZInt();

		public override string ShippingMarks => Package.B5_MarksAndNumbers;
	}
}
