using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(AccHeaderFlatFileExporter))]
	public class AccHeaderFlatFileExporterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AccHeaderFlatFileExporter(Factory);
		}

		public void TestConverter()
		{
			AccHeaderFlatFileExporterTestClass exporter = new AccHeaderFlatFileExporterTestClass(Factory);
			AccountingFlatFileConverter converter = exporter.Converter;
			AssertSame("Converter was not lazy loaded", converter, exporter.Converter);
			AssertEquals("Converter should be of type 'AccHeaderFlatFileConverter'", typeof(AccHeaderFlatFileConverter), converter.GetType());
		}

		class AccHeaderFlatFileExporterTestClass : AccHeaderFlatFileExporter
		{
			public AccHeaderFlatFileExporterTestClass(BusinessObjectFactory factory) : base(factory)
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
