using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	class TemplateExtensionsTest : TestCase
	{
		public void TestGetValueUsingMacro()
		{
			var parameter = new NamedParameter("val", "Collection.Skip(1).First", 5);

			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();

			var context = new IMacroLibrary[] { new StandardLibrary() }.CreateContext();

			AssertEquals("parameter value", child2, parameter.GetValue<DummyChildBusinessObject>(new MacroScope(dummy), context));
		}
	}
}