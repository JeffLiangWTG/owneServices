using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRelatedItemRegistryItem))]
	sealed class CodeDescriptionBoolRelatedItemRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolRelatedItemCollection>
	{
		protected override StronglyTypedRegistryItem<CodeDescriptionBoolRelatedItemCollection, CodeDescriptionBoolRelatedItemCollection> GetNewRegistryItem()
		{
			CodeDescriptionBoolRelatedItemRegistryEditorInfo editorInfo = new CodeDescriptionBoolRelatedItemRegistryEditorInfo((NoResString)"Bool");
			return new CodeDescriptionBoolRelatedItemRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, 3, editorInfo, new CodeDescriptionBoolRelatedItemCollection(), null);
		}
	}
}
