using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentsAllowedForSigningItem))]
	public class DocumentsAllowedForSigningItemTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateDocumentName()
		{
			var item1 = new DocumentsAllowedForSigningItem("Document1", "Yes");
			AssertNoErrors(item1.DocumentNameInfo);

			var collection = new DocumentsAllowedForSigningItemCollection();
			collection.Add(item1);

			var item2 = new DocumentsAllowedForSigningItem();
			collection.Add(item2);
			item2.DocumentName = "Document1";
			AssertHasError(item2.DocumentNameInfo, "This document name already exists.");
			AssertHasError(item2.DocumentTypeInfo, "Please enter a valid document type.");

			item2.DocumentName = "";
			AssertHasError(item2.DocumentNameInfo, "Please enter a valid document name.");
			AssertHasError(item2.DocumentTypeInfo, "Please enter a valid document type.");

			item2.DocumentName = "Document2";
			AssertNoErrors(item2.DocumentNameInfo);
			AssertHasError(item2.DocumentTypeInfo, "Please enter a valid document type.");
		}

		public void TestValidateDocumentType()
		{
			var item = new DocumentsAllowedForSigningItem();

			item.DocumentType = "";
			AssertHasError(item.DocumentNameInfo, "Please enter a valid document name.");
			AssertHasError(item.DocumentTypeInfo, "Please enter a valid document type.");

			item.DocumentType = "XXX";
			AssertHasError(item.DocumentNameInfo, "Please enter a valid document name.");
			AssertHasError(item.DocumentTypeInfo, "Enter a valid selection.");

			item.DocumentType = "All";
			AssertNoErrors(item.DocumentTypeInfo);
			AssertHasError(item.DocumentNameInfo, "Please enter a valid document name.");
		}

		#region Implementation

		protected new DocumentsAllowedForSigningItem BizObj => (DocumentsAllowedForSigningItem)base.BizObj;

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => new DocumentsAllowedForSigningItem();

		#endregion
	}
}
