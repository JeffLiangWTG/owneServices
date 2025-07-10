using CargoWise.Types;
using Enterprise.DataConverters.CustomsFiles.NZ.DataImporters.Excel;
using Enterprise.DataConverters.Testing.DataImporters;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.DataImporters.Excel
{
	sealed internal class ProductDataImporterTest : ExcelDataImporterTestBase
	{
		protected override ZString TestFileName
		{
			get { return "Product.CSV"; }
		}

		protected override DataImporter GetDataImporter(ZString dataSourcePath)
		{
			return new ProductDataImporter(Logger, dataSourcePath, false);
		}
	}
}
