using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	class AmendmentObjectWrapperTest : TestCaseWithFactory
	{
		public void TestDifferences()
		{
			var property = typeof(AmendmentObjectWrapper).GetTypeInfo().GetProperty("Differences");
			AssertContains(typeof(Difference).ToString(), property.PropertyType.FullName);
		}

		public void TestAmendmentObjects()
		{
			var objectWrapper = new AmendmentObjectWrapper();
			var differences = new List<Difference>();
			objectWrapper.Differences = differences;
			AssertEquals("Zero objects", 0, objectWrapper.AmendmentObjects.Count());

			var mockPointerParser = new Mock<IPointerParser>();
			var pointerParser = mockPointerParser.Object;
			var nodetree = System.Array.Empty<Node>();

			differences.Add(getDifference(pointerParser, nodetree, "xpath1", new ZString[] { "1/2/3", "1/2/4", "1/2/5" }));
			AssertEquals("One distinct object", 1, objectWrapper.AmendmentObjects.Count());

			differences.Add(getDifference(pointerParser, nodetree, "xpath1", new ZString[] { "1/2/3", "1/2/4", "1/2/5" }));
			AssertEquals("One distinct object", 1, objectWrapper.AmendmentObjects.Count());

			differences.Add(getDifference(pointerParser, nodetree, "xpath1", new ZString[] { "1/2/6", "1/2/7", "1/2/8" }));
			AssertEquals("Two distinct objects", 2, objectWrapper.AmendmentObjects.Count());
		}

		Difference getDifference(IPointerParser pointerParser, Node[] nodetree, ZString xPath, ZString[] pointers)
		{
			var mockDifference = new Mock<Difference>(new object[] { pointerParser, ZString.Empty, ZString.Empty, nodetree });
			mockDifference.Setup(m => m.XPath).Returns(xPath);
			mockDifference.Setup(m => m.Type).Returns("x");
			mockDifference.Setup(m => m.WCOIDPointers).Returns(pointers);

			return mockDifference.Object;
		}

		public void TestXML()
		{
			var objectWrapper = new AmendmentObjectWrapper();
			objectWrapper.Xml = XElement.Parse("<Root>Test</Root>");
			AssertEquals("<Root>Test</Root>", objectWrapper.Xml.ToString());
		}
	}
}
