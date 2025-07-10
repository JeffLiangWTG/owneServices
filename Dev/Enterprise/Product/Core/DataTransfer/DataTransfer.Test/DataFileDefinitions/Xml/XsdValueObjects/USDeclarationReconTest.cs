using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USReconciliation))]
	sealed class USDeclarationReconTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USReconciliation uSRecon = new Xsd.USReconciliation();
			AssertEquals(false, uSRecon.IsSpecified);

			uSRecon.NAFTA = Xsd.TrueFalse.@true;
			AssertEquals(true, uSRecon.IsSpecified);

			uSRecon.NAFTA = Xsd.TrueFalse.@false;
			AssertEquals(true, uSRecon.IsSpecified);
		}
	}
}
