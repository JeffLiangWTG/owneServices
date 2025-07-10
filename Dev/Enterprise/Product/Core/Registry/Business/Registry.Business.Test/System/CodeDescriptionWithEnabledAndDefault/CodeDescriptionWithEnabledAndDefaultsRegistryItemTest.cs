using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithEnabledAndDefaultsRegistryItem))]
	sealed class CodeDescriptionWithEnabledAndDefaultsRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionWithEnabledAndDefaultCollection>
	{
		protected override StronglyTypedRegistryItem<CodeDescriptionWithEnabledAndDefaultCollection, CodeDescriptionWithEnabledAndDefaultCollection> GetNewRegistryItem()
		{
			var defaultCollection = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			defaultCollection.AddNewSystemDefined("tel", (NoResString)"Lync", false, true);
			defaultCollection.AddNewSystemDefined("callto", (NoResString)"Skype", true, true);
			return new CodeDescriptionWithEnabledAndDefaultsRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", null, RegistryStorageFlags.System, RegistryOptions.Default, defaultCollection, true);
		}
	}
}
