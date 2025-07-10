using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(FindWindowQueryCostsRegistryItemEditorUserControl))]
	sealed class FindWindowQueryCostsRegistryItemEditorUserControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new FindWindowQueryCosts();
	}
}
