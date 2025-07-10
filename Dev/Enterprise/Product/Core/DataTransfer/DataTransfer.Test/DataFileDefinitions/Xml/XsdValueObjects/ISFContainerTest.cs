using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ISFContainer))]
	sealed class ISFContainerTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.ISFContainer container = new Xsd.ISFContainer();
			AssertEquals("Should not be specified by default", false, container.IsSpecified);

			container.DescriptionCode = "12";
			AssertEquals("ISFContainer.IsSpecified", true, container.IsSpecified);

			container.DescriptionCode = "";
			container.ContainerNumber = "TURE213121";
			AssertEquals("ISFContainer.IsSpecified", true, container.IsSpecified);

			container.ContainerNumber = "";
			container.ISOType = "40FR";
			AssertEquals("ISFContainer.IsSpecified", true, container.IsSpecified);
		}
	}
}
