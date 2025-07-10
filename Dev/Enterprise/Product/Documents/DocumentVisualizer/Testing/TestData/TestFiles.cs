using System.IO;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
	static class TestFiles
	{
		public static FlexCelWorksheet CreateWorksheet() => CreateWorksheet(XlsFilePath);
		public static FlexCelWorksheet CreateWorksheet(string filePath) => FlexCelWorksheet.FromFile(filePath);

		public static string XlsFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\TestWorksheet.xls");
		public static string XlsFileForBordersTestPath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\TestTemplate_Borders.xls");
		public static string UniversalXmlFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\UniversalXml.xml");
		public static string MessagingConfigTestTemplateFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\TestTemplate_MessagingConfig.xls");
		public static string ColorsTestTemplateFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\TestTemplate_Colors.xlsx");
		public static string ImageTestTemplateFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\TestData\TestTemplate_Image.xls");
	}
}
