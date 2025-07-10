using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI;

[TestedType(typeof(SystemDefinableRegistryImageCollectionControl))]
sealed class SystemDefinableRegistryImageCollectionControlTest : TestCaseWithFactory
{
	public void TestSystemColumnIsReadOnly()
	{
		using (var control = new SystemDefinableRegistryImageCollectionControl())
		{
			var grid = control.RegistryImageGrid;
			AssertEquals(true, grid.GetColumnStyle("SystemDefined").IsReadOnly);
		}
	}
}
