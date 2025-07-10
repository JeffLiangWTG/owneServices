using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.NZP.CMS.Testing
{
	[TestedType(typeof(CMSCreditSalesExporter))]
	public class CMSCreditSalesExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConverter()
		{
			CMSCreditSalesExporterTestClass exporter = new CMSCreditSalesExporterTestClass(0, Factory);
			FlatFileConverter converter = exporter.Converter;
			AssertEquals("Converter should be a CMSCashSalesConverter", typeof(CMSCreditSalesConverter), converter.GetType());
			AssertSame("Converter should be lazy loaded", converter, exporter.Converter);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CMSCreditSalesExporter(0, Factory);
		}

		class CMSCreditSalesExporterTestClass : CMSCreditSalesExporter
		{
			public CMSCreditSalesExporterTestClass(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
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
