using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentDeliveryDefaultLanguagesRegistryItem))]
	class DocumentDeliveryDefaultLanguagesRegistryItemTest : StronglyTypedRegistryItemTestCase<DocumentDeliveryDefaultLanguagesCollection>
	{
		protected override StronglyTypedRegistryItem<DocumentDeliveryDefaultLanguagesCollection, DocumentDeliveryDefaultLanguagesCollection> GetNewRegistryItem()
		{
			var collection = new DocumentDeliveryDefaultLanguagesCollection();
			var entry = collection.AddNew();
			entry.Fallback = Enterprise.Core.Constants.DocumentDeliveryDefaultLanguagesFallbackType.System;
			entry.Order = 1;
			return new DocumentDeliveryDefaultLanguagesRegistryItem("", null, null, null, RegistryStorageFlags.System, collection);
		}
	}
}
