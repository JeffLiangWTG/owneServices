using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IPackage
	{
		ZString MarksAndNumbersOfPackages { get; }
		ZString MarksAndNumbersOfPackagesLanguage { get; }
		ZString KindOfPackages { get; }
		ZLong NumberOfUnits { get; }
		ZLong NumberOfPackages { get; }
		ZLong NumberOfPieces { get; }
		ZBool IsBulk { get; }
		ZBool IsUnpacked { get; }
	}
}
