using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IPackageCommon
{
	ZString PackageType { get; }
	ZString Marks { get; }
}

public interface IPackageCommonNumbers : IPackageCommon
{
	ZInt PackagesQty { get; }
	ZInt PiecesQty { get; }
}

public interface ICommonPackageWithSequenceAndPackNum : IPackageCommon
{
	ZString SequenceNumber { get; }
	ZString NumberOfPackages { get; }
}
