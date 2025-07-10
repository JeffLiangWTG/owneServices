using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.SystemMerge.Business.Testing;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.Xml.Testing
{
	internal class SysMergeProductXmlValueObjectSerializerTest : TestCaseWithFactory
	{
		public void TestImportOrgSupplierPartWithMissingOrg()
		{
			SysMergeProductValueObjectDataAdapter adapter = new SysMergeProductValueObjectDataAdapter();
			SysMergeProductXmlValueObjectSerializerForTesting serializer = new SysMergeProductXmlValueObjectSerializerForTesting(adapter);

			OrgSupplierPart product = new SysMergeTestHelper(Factory).GetNewProduct();
			Xsd.OrgSupplierPart xsdProduct = adapter.ExportToValueObject(product, new ValueObjectExportContext(new NotificationBuffer()));

			BusinessObjectFactory importingFactory = NewFactory();
			ValueObjectImportContext context = new ValueObjectImportContext(importingFactory, new NotificationBuffer());

			AssertNotNull("Should import product", serializer.CreateOrUpdateFromValueObjectExposed(xsdProduct, context));
			Assert(context.LastNotificationMessage.Contains("imported successfully"));
			Assert(!context.LastNotificationMessage.Contains("Product references missing Organizations"));

			xsdProduct.OrgPartRelations.AddNew();

			importingFactory = NewFactory();
			context = new ValueObjectImportContext(importingFactory, new NotificationBuffer());

			AssertNull("Should not import product with missing org", serializer.CreateOrUpdateFromValueObjectExposed(xsdProduct, context));
			Assert(!context.LastNotificationMessage.Contains("imported successfully"));
			Assert(context.LastNotificationMessage.Contains("Product references missing Organizations"));
		}

		public void TestImportOrgSupplierPartWithPartNumLongerThanMaxLength()
		{
			var adapter = new SysMergeProductValueObjectDataAdapter();
			var serializer = new SysMergeProductXmlValueObjectSerializerForTesting(adapter);

			var product = new SysMergeTestHelper(Factory).GetNewProduct();
			var xsdProduct = adapter.ExportToValueObject(product, new ValueObjectExportContext(new NotificationBuffer()));
			xsdProduct.PartNum = "TEST".PadRight(OrgSupplierPartSchema.OP_PartNum.MaxLength + 1, '1');

			var importingFactory = NewFactory();
			var context = new ValueObjectImportContext(importingFactory, new NotificationBuffer());

			AssertNull("Should import product", serializer.CreateOrUpdateFromValueObjectExposed(xsdProduct, context));
			var errorMessage = $"Import of product [({xsdProduct.PK}) - {xsdProduct.PartNum} - {xsdProduct.Desc}] skipped. Reason: Product Number cannot not be longer than {OrgSupplierPartSchema.OP_PartNum.MaxLength} characters.";
			AssertContains(errorMessage, context.LastNotificationMessage);
		}
	}
}
