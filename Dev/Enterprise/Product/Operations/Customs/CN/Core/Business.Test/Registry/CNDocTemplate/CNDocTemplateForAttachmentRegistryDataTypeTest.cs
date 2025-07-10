using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDocTemplateForAttachmentRegistryDataType))]
	class CNDocTemplateForAttachmentRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CNDocTemplateForAttachmentRegistryDataType>
	{
		protected override CNDocTemplateForAttachmentRegistryDataType GetNewDataType() => new CNDocTemplateForAttachmentRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CNDocTemplateForAttachmentCollection();
			var docTemplate1 = collection1.AddNew();
			docTemplate1.OrganizationPK = ZGuid.Empty;
			docTemplate1.OrganizationPK = ZGuid.Empty;
			docTemplate1.DataContext = ".CustomsDeclarationDocument";
			docTemplate1.DocumentTemplate = "CN Customs Invoice(System)";
			docTemplate1.DocumentType = "INV";
			docTemplate1.DocumentDescription = "Invoice";
			docTemplate1.AttachmentType = "00000001";

			var collection2 = new CNDocTemplateForAttachmentCollection();
			var docTemplate2 = collection1.AddNew();
			docTemplate2.OrganizationPK = ZGuid.Empty;
			docTemplate2.DataContext = ".CustomsDeclarationDocument";
			docTemplate2.DocumentTemplate = "CN Customs Invoice(System)";
			docTemplate2.DocumentType = "INV";
			docTemplate2.DocumentDescription = "Invoice";
			docTemplate2.AttachmentType = "00000004";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new CNDocTemplateForAttachmentRegistryDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new CNDocTemplateForAttachmentRegistryDataType().Serialise(collection2))
			};
		}

		protected override string ExpectedEditorName => "CNDocTemplateForAttachmentRegistryItemEditor";
	}
}
