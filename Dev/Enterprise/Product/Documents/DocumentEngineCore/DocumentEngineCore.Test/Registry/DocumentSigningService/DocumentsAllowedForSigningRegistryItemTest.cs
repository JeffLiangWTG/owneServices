using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentsAllowedForSigningRegistryItem))]
	class DocumentsAllowedForSigningRegistryItemTest : StronglyTypedRegistryItemTestCase<DocumentsAllowedForSigning>
	{
		protected override StronglyTypedRegistryItem<DocumentsAllowedForSigning, DocumentsAllowedForSigning> GetNewRegistryItem()
		{
			return new DocumentsAllowedForSigningRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
