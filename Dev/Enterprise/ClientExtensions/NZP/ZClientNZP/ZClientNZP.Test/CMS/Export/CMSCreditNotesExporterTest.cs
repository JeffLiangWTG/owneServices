using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.NZP.CMS.Testing
{
	[TestedType(typeof(CMSCreditNotesExporter))]
	public class CMSCreditNotesExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConverter()
		{
			CMSCreditNotesExporterTestClass exporter = new CMSCreditNotesExporterTestClass(0, Factory);
			FlatFileConverter converter = exporter.Converter;
			AssertEquals("Converter should be a CMSCashSalesConverter", typeof(CMSCreditNotesConverter), converter.GetType());
			AssertSame("Converter should be lazy loaded", converter, exporter.Converter);
		}

		#region Implmentation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CMSCreditNotesExporter(0, Factory);
		}

		class CMSCreditNotesExporterTestClass : CMSCreditNotesExporter
		{
			public CMSCreditNotesExporterTestClass(int batchNumber, BusinessObjectFactory factory) : base(batchNumber, factory)
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
