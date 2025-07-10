using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IPackage
{
	ZInt? NumberOfPacks { get; }
	ZString PackageType { get; }
	ZInt? NumberOfPieces { get; }
	ZString MarksAndNumbers { get; }
}
