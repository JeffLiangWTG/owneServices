using System.Linq;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business
{
	sealed class MacroBusinessObjectPropertyCollectionTest : TestCase
	{
		public void TestCreateNewWithNullCollection()
		{
			var properties = new MacroBusinessObjectPropertyCollection(null);

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ICustomProperty>(), properties);
		}

		public void TestAddProperties()
		{
			var property1 = new Mock<IMacroBusinessObjectProperty>();
			var property2 = new Mock<IMacroBusinessObjectProperty>();
			var property3 = new Mock<IMacroBusinessObjectProperty>();

			property1.Setup(p => p.PropertyName).Returns("P1");
			property2.Setup(p => p.PropertyName).Returns("P1");
			property3.Setup(p => p.PropertyName).Returns("P3");

			var properties = new MacroBusinessObjectPropertyCollection(new[] { property1.Object, property2.Object, property3.Object });

			AssertContainsExactElementsInAnyOrder(new[] { "P1", "P3" }, properties.Select(p => p.Identifier));

			Assert(true);
		}
	}
}