using System;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[Flags]
	public enum ValidationModes
	{
		None = 1 << 0,
		Original = 1 << 1,
		Amendment = 1 << 2
	}
}
