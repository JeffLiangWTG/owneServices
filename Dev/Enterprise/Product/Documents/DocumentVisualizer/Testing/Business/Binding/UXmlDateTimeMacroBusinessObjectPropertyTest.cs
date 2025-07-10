using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Business.Binding;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business.Binding
{
	sealed class UXmlDateTimeMacroBusinessObjectPropertyTest : TestCase
	{
		public void TestPropertyTypeIsZDateTimeOrZDateTimeOffset()
		{
			var rawProperty1 = new Mock<IMacroBusinessObjectProperty>();
			var rawProperty2 = new Mock<IMacroBusinessObjectProperty>();
			rawProperty1.Setup(p => p.Value).Returns(new UXmlDateTime(ZDateTime.Now));
			rawProperty2.Setup(p => p.Value).Returns(new UXmlDateTime(ZDateTimeOffset.Now));

			var uXmlDateTimeProperty1 = new UXmlDateTimeMacroBusinessObjectProperty(rawProperty1.Object);
			var uXmlDateTimeProperty2 = new UXmlDateTimeMacroBusinessObjectProperty(rawProperty2.Object);

			AssertEquals(typeof(ZDateTime) ,uXmlDateTimeProperty1.PropertyType);
			AssertEquals(typeof(ZDateTimeOffset), uXmlDateTimeProperty2.PropertyType);
		}
	}
}
