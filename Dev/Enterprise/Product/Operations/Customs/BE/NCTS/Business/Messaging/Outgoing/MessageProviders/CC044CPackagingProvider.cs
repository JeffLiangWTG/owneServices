using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC044CPackagingProvider : BasePackagingProvider
	{
		public CC044CPackagingProvider(NctsPackage package) : base(package) { }

		public override string TypeOfPackages => StatusIsNew
			? Package.B5_UnitType.ToString()
			: string.Empty;

		public override int? NumberOfPackages => Package.IsBulk ? null : (StatusIsNew ? Package.B5_UnitCount.ToZInt() : null);

		public override string ShippingMarks => StatusIsNew
			? Package.B5_MarksAndNumbers.ToString()
			: string.Empty;

		bool StatusIsNew => Package.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW;
	}
}
