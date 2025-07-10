using System;

namespace Enterprise.Integration.Packing
{
	public interface IPackageActionStrategy : IPackageCanceledActionEventArgs
	{
		bool IsActionAllowed(PackageAction action);
	}

	[Flags]
	public enum PackageAction
	{
		None = 0,
		Edit = 1,
		Delete = 2,
		PackUnpack = 4,
		// next should be 8, 16...
	}
}