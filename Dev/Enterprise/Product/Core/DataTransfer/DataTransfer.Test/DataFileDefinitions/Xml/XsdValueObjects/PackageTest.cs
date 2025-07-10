using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Package))]
	sealed class PackageTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Package value = null;
			value = new Xsd.PackageCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
