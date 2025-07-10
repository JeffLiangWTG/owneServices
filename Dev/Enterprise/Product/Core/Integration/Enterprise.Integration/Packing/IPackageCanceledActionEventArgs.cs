using CargoWise.Types;

namespace Enterprise.Integration.Packing
{
	public interface IPackageCanceledActionEventArgs
	{
		IPkgPackage Package { get; }
		ZString ReasonForNotAllowingAction { get; }
	}
}
