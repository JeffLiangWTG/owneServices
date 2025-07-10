using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.NZP.CMS.Testing
{
	[TestedType(typeof(CMSCashSalesExporter))]
	public class CMSCashSalesExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConverter()
		{
			CMSCashSalesExporterTestClass exporter = new CMSCashSalesExporterTestClass(0, Factory);
			FlatFileConverter converter = exporter.Converter;
			AssertEquals("Converter should be a CMSCashSalesConverter", typeof(CMSCashSalesConverter), converter.GetType());
			AssertSame("Converter should be lazy loaded", converter, exporter.Converter);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CMSCashSalesExporter(0, Factory);
		}

		class CMSCashSalesExporterTestClass : CMSCashSalesExporter
		{
			public CMSCashSalesExporterTestClass(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
			{
			}

			public new FlatFileConverter Converter
			{
				get
				{
					return base.Converter;
				}
			}
		}
		#endregion
	}
}
