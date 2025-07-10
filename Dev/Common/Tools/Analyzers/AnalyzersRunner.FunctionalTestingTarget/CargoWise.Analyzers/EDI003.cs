//EDI003:Do Not Exceed BizO Max Length

using CargoWise.ComponentModel;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	static class EDI003
	{
		[MaxLength(20)]
		public static string MyProperty { get; set; } = string.Empty;

		public static void AssignProperty()
		{
			MyProperty = "012345678901234567890";

			var exceedingString = "Test String That Exceeds Maximum Character Limits";
			MyProperty = exceedingString;
		}
	}
}
