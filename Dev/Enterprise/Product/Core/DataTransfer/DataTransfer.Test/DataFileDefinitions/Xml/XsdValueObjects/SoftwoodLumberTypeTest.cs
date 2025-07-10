using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.SoftwoodLumberType))]
	sealed class SoftwoodLumberTypeTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.SoftwoodLumberType softwoodLumberType = new Xsd.SoftwoodLumberType();
			AssertEquals(false, softwoodLumberType.IsSpecified);

			softwoodLumberType.ExportCharge = 10;
			AssertEquals(true, softwoodLumberType.IsSpecified);

			softwoodLumberType = new Xsd.SoftwoodLumberType();
			softwoodLumberType.ExportPrice = 10;
			AssertEquals(true, softwoodLumberType.IsSpecified);

			softwoodLumberType = new Xsd.SoftwoodLumberType();
			softwoodLumberType.ImporterDec = true;
			AssertEquals(true, softwoodLumberType.IsSpecified);
		}
	}
}
