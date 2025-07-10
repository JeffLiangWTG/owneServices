using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DataConverters.Testing.Base
{
	internal abstract class DataImporterTestBase : TestCaseWithFactory
	{
		protected virtual string BaseTestDataPath
		{
			get { return Path.Combine(NUnit.Framework.Dat.DatServerConnection.DatFileSharePath, @"TestFiles\DataConvertersTestData\Deliver\NZ"); }
		}

		protected abstract DataImporter GetDataImporter(ZString dataSourcePath);
		protected abstract ZString TestPathFor5RowDataSource { get; }
		protected abstract ZString TestPathFor100RowDataSource { get; }
		protected abstract ZString TestPathFor1000RowDataSource { get; }
		protected abstract ZString TestPathFor2000RowDataSource { get; }
		protected abstract ZString TestPathForFullClientDataSource { get; }

		public void TestImport5RowDataSource()
		{
			var importer = GetDataImporter(TestPathFor5RowDataSource);
			importer.Import();
			AssertEquals("Logger.RecordsProcessed", 5, Logger.RecordsProcessed);
		}

		public void TestImport100RowDataSource()
		{
			var importer = GetDataImporter(TestPathFor100RowDataSource);
			importer.Import();
			AssertEquals("Logger.RecordsProcessed", 100, Logger.RecordsProcessed);
		}

		//Please do not delete, is uysed for performance testing sometimes.
		//		public void TestImport1000RowDataSource()
		//		{
		//			DataImporter Importer = GetDataImporter(TestPathFor1000RowDataSource);
		//			Importer.Import();
		//			AssertEquals("Performance Test Designed to Fail on Developer's Machines", "", Logger.ToString());
		//		}
		//
		//		public void TestImport2000RowDataSource()
		//		{
		//			DataImporter Importer = GetDataImporter(TestPathFor2000RowDataSource);
		//			Importer.Import();
		//			AssertEquals("Performance Test Designed to Fail on Developer's Machines", "", Logger.ToString());
		//		}
		//
		//		public void TestImportFullClientDataSourceRowDataSource()
		//		{
		//			DataImporter Importer = GetDataImporter(TestPathForFullClientDataSource);
		//			Importer.Import();
		//			AssertEquals("Performance Test Designed to Fail on Developer's Machines", "", Logger.ToString());
		//		}

		protected ProgressLogger Logger
		{
			get
			{
				if (fLogger == null)
				{
					fLogger = new ProgressLogger();
				}
				return fLogger;
			}
		}
		ProgressLogger fLogger;
	}
}
