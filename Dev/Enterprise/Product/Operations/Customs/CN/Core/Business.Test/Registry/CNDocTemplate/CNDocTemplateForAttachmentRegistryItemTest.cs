using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNDocTemplateForAttachmentRegistryItem))]
	class CNDocTemplateForAttachmentRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CNDocTemplateForAttachmentCollection>
	{
		protected override StronglyTypedRegistryItem<CNDocTemplateForAttachmentCollection, CNDocTemplateForAttachmentCollection> GetNewRegistryItem()
			=> new CNDocTemplateForAttachmentRegistryItem("", null, null, null, RegistryStorageFlags.Company, CNDocTemplateForAttachmentCollection.GetDefault());

		protected override CNDocTemplateForAttachmentCollection ValidValue
		{
			get
			{
				var collection = new CNDocTemplateForAttachmentCollection();
				var docTemplate = collection.AddNew();

				docTemplate.OrganizationPK = ZGuid.Empty;
				docTemplate.DataContext = ".CustomsDeclarationDocument";
				docTemplate.DocumentTemplate = "CN Customs Invoice(System)";
				docTemplate.DocumentType = "INV";
				docTemplate.DocumentDescription = "Invoice";
				docTemplate.AttachmentType = "00000001";

				return collection;
			}
		}
	}
}
