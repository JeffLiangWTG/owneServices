using System.IO;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	static class TestTemplateWithDocData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public static string FilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\Core_Legacy\Template\TestTemplateWithDocData.xls");
	}
}
