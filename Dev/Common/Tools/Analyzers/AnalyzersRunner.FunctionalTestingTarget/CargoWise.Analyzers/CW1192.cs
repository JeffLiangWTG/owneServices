using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1192
	{
		// CW1192 - CustomizableDataCaptionAsmidRequiredAnalyzer
		[TranslatableDataField("test", "test", Type = typeof(string))]
		public string Name { get; set; }
	}
}
