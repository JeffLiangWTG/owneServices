using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(AccLinesFlatFileExporter))]
	public class AccLinesFlatFileExporterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccLinesFlatFileExporter(0, Factory);
		}

		public void TestFilterProvider()
		{
			AccLinesFlatFileExporter exporter = new AccLinesFlatFileExporter(3, Factory);
			AssertEquals("Current Batch Number should 3", 3, exporter.FilterProvider.CurrentBatchNo);
		}

		public void TestConverter()
		{
			AccLinesFlatFileExporterTestClass exporter = new AccLinesFlatFileExporterTestClass(Factory);
			AccountingFlatFileConverter converter = exporter.Converter;
			AssertSame("Converter was not lazy loaded", converter, exporter.Converter);
			AssertEquals("Converter should be of type 'AccLinesFlatFileConverter'", typeof(AccLinesFlatFileConverter), converter.GetType());
		}

		class AccLinesFlatFileExporterTestClass : AccLinesFlatFileExporter
		{
			public AccLinesFlatFileExporterTestClass(BusinessObjectFactory factory) : base(0, factory)
			{
			}

			public new AccountingFlatFileConverter Converter
			{
				get
				{
					return base.Converter;
				}
			}
		}
	}
}
