using CargoWise.Types;
using Enterprise.DataConverters.Testing.Base;

namespace Enterprise.DataConverters.Testing.DataImporters
{
	internal abstract class InterbaseImporterTestBase : DataImporterTestBaseWithLogger
	{
		public void TestSqlText()
		{
			AssertEquals(ExpectedSqlText, Importer.SqlText);
		}

		protected new InterbaseImporter Importer
		{
			get { return (InterbaseImporter)base.Importer; }
		}

		protected abstract ZString ExpectedSqlText { get; }
	}
}
