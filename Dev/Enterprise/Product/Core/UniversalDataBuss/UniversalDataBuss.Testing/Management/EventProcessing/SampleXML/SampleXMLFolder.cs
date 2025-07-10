using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing.Testing
{
	public static class SampleXMLFolder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static string Path
		{
			get { return TestCase.BaseSourcePath + @"\Enterprise\Product\Core\UniversalDataBuss\UniversalDataBuss.Testing\Management\EventProcessing\SampleXML"; }
		}
	}
}
