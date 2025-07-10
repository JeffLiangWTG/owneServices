using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.Wow.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export.Testing
{
	public class DeclarationInvoiceExportFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestMapExport()
		{
			var factory = new BusinessObjectFactory();
			var jobDec = WowSTTestHelper.GetPopulatedDeclaration(factory);
			var adapter = new AUDeclarationValueObjectDataAdapter();
			var xsdDec = adapter.ExportToValueObject(jobDec, new ValueObjectExportContext(new NotificationBuffer()));
			var converter = new MockDeclarationInvoiceExportFlatFileConverter(new NotificationBuffer(), factory);
			var collection = converter.MapExport(xsdDec);
			AssertEquals("Collection.Count", 1, collection.Count);
			var expectedString = "1         99999             2222        76543             00000800.0000000000000";
			AssertEquals("String of Collection[0]", expectedString, collection[0].ToString());
		}

		class MockDeclarationInvoiceExportFlatFileConverter : DeclarationInvoiceExportFlatFileConverter
		{
			public MockDeclarationInvoiceExportFlatFileConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
	}
}
