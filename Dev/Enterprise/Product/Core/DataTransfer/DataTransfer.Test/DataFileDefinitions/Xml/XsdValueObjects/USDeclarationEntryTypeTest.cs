using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationEntryType))]
	sealed class USDeclarationEntryTypeTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationEntryType uSDeclarationEntryType = new Xsd.USDeclarationEntryType();
			AssertEquals(false, uSDeclarationEntryType.IsSpecified);

			uSDeclarationEntryType.EntryType = "33";
			AssertEquals(true, uSDeclarationEntryType.IsSpecified);
		}
	}
}
