using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IEMCSPackageInComing
	{
		ZString KindOfPackages { get; }
		ZLong NumberOfPackages { get; }
		ZString SealNumber { get; }
		ZString SealInformation { get; }
		ZString ShippingMarks { get; }
		ZBool IsNumberOfPackagesProvided { get; }
	}
}
