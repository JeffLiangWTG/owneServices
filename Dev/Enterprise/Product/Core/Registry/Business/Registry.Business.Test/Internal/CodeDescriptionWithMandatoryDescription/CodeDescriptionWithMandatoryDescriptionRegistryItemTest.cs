using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithMandatoryDescriptionRegistryItem))]
	sealed class CodeDescriptionWithMandatoryDescriptionRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionWithMandatoryDescriptionCollection>
	{
		protected override StronglyTypedRegistryItem<CodeDescriptionWithMandatoryDescriptionCollection, CodeDescriptionWithMandatoryDescriptionCollection> GetNewRegistryItem()
		{
			var defaultValues = new CodeDescriptionWithMandatoryDescriptionCollection
			{
				{ "AAA", (NoResString)"AAA Description" },
				{ "ZZZ", (NoResString)"ZZZ Description" }
			};
			return new CodeDescriptionWithMandatoryDescriptionRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, defaultValues);
		}
	}
}
