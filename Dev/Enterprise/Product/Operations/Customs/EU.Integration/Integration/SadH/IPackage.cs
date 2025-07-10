using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IPackage
	{
		ZString PackageMarks { get; }   // PKG-MARKS
		ZInt PackageCount { get; }  // PKG-COUNT
		ZString PackageKind { get; }    // PKG-KIND
	}
}
