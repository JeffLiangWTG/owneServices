using System;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum ComparisonOptions
	{
		None,
		Default = None,
		NationalLanguage,
	}
}
