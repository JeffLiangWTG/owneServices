using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TemplateRegistryItem))]
	public class TemplateRegistryItemTest : StronglyTypedRegistryItemTestCase<TemplateCriteria>
	{
		protected override StronglyTypedRegistryItem<TemplateCriteria, TemplateCriteria> GetNewRegistryItem()
		{
			return new TemplateRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
