using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface IPackage
	{
		ZString KindOfPackages { get; }
		ZInt NumberOfPackages { get; }
		ZInt NumberOfPieces { get; }
		ZString MarksAndNumbersOfPackages { get; }
		ZString MarksAndNumbersOfPackagesLNG { get; }
	}
}
