using System;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	[Flags]
	enum RegenFlags
	{
		None = 0,
		Compile = 1 << 0,
		MinorVersion = 1 << 1,
		BumpVersionOnly = 1 << 2,
		Merge = 1 << 3,
	}
}
