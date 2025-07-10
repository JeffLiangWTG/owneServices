using System;

namespace Enterprise.BarcodeParsing.Business
{
	[Flags]
	public enum RelatedEntityRequirements
	{
		None = 0,
		MustHaveBuyer = 1,
	}
}
