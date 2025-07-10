using Enterprise.ZArchitecture.Core;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1159
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Do Not Invoke Old Res.GetString Methods", Justification = "Used for Analyzer test")]
		public void TestMethod()
		{
			// CW1159 Res.GetString, Res.GetData, ResString.GetMultilingualString should use the source generated static method instead
			_ = ResString.GetMultilingualString("abcd", "key");
		}
	}
}
