using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentsAllowedForSigning))]
	public class DocumentsAllowedForSigningTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultDocumentsAllowedForSigningItems()
		{
			var documents = GetBusinessObjectToSerialise() as DocumentsAllowedForSigning;

			AssertEquals("Should have 3 default values", 3, documents.DocumentsAllowedForSigningItems.Count);
			AssertDefaultDocumentsAllowedForSigningItems(documents.DocumentsAllowedForSigningItems);
		}

		void AssertDefaultDocumentsAllowedForSigningItems(DocumentsAllowedForSigningItemCollection items)
		{
			AssertEquals("DocBuilder Invoice", items[0].DocumentName);
			AssertEquals("Yes", items[0].DocumentType);

			AssertEquals("Periodic Invoice", items[1].DocumentName);
			AssertEquals("Yes", items[1].DocumentType);

			AssertEquals("Periodic Invoice && Invoice Detail", items[2].DocumentName);
			AssertEquals("Yes", items[2].DocumentType);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => new DocumentsAllowedForSigning();

		#endregion
	}
}
