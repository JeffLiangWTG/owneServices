using CargoWise.Types;
using Enterprise.DataConverters.CustomsFiles.NZ.DataImporters.Excel;
using Enterprise.DataConverters.Testing.DataImporters;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.DataImporters.Excel
{
	sealed internal class ClassificationDataImporterTest : ExcelDataImporterTestBase //DataImporters.Testing.ExcelDataImporterTest
	{
		protected override ZString TestFileName
		{
			get { return "Classification.CSV"; }
		}

		protected override DataImporter GetDataImporter(ZString dataSourcePath)
		{
			return new ClassificationDataImporter(Logger, dataSourcePath, false);
		}
	}
}
