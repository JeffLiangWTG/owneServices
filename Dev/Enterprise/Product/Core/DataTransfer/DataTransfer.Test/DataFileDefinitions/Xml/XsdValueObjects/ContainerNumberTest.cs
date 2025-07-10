using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Container))]
	sealed class ContainerNumberTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Container value = null;
			value = new Xsd.ContainerCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
