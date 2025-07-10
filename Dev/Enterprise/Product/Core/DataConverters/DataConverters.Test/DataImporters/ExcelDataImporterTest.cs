using CargoWise.Types;
using Enterprise.DataConverters.Testing.Base;

namespace Enterprise.DataConverters.Testing.DataImporters
{
	internal abstract class ExcelDataImporterTestBase : DataImporterTestBase
	{
		protected abstract ZString TestFileName { get; }
		protected override ZString TestPathFor5RowDataSource
		{
			get { return BaseTestDataPath + @"\5\" + TestFileName; }
		}

		protected override ZString TestPathFor100RowDataSource
		{
			get { return BaseTestDataPath + @"\100\" + TestFileName; }
		}

		protected override ZString TestPathFor1000RowDataSource
		{
			get { return BaseTestDataPath + @"\1000\" + TestFileName; }
		}

		protected override ZString TestPathFor2000RowDataSource
		{
			get { return BaseTestDataPath + @"\2000\" + TestFileName; }
		}

		protected override ZString TestPathForFullClientDataSource
		{
			get { return BaseTestDataPath + @"\FULL\" + TestFileName; }
		}
	}
}
