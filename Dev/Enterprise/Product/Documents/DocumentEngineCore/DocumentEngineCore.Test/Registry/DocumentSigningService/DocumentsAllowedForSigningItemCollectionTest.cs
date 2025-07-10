using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentsAllowedForSigningItemCollection))]
	public class DocumentsAllowedForSigningItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DocumentsAllowedForSigningItemCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		protected override DocumentsAllowedForSigningItemCollection GetCollectionToTest() => new DocumentsAllowedForSigningItemCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentsAllowedForSigningItem();

		#endregion
	}
}
