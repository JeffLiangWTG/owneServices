using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IPackageProvider
{
	ZInt NumberOfPackages { get; }
	ZString PackageType { get; }
	ZString MarksAndNumbers { get; }
}
