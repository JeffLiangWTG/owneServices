using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationWarehouse))]
	sealed class USDeclarationWarehouseTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationWarehouse uSDeclarationWarehouse = new Xsd.USDeclarationWarehouse();
			AssertEquals(false, uSDeclarationWarehouse.IsSpecified);

			uSDeclarationWarehouse.EntryFilerCode = "TST";
			AssertEquals(true, uSDeclarationWarehouse.IsSpecified);

			uSDeclarationWarehouse.EntryFilerCode = "";
			uSDeclarationWarehouse.EntryNumber = "TEST";
			AssertEquals(true, uSDeclarationWarehouse.IsSpecified);

			uSDeclarationWarehouse.EntryNumber = "";
			uSDeclarationWarehouse.Port = "PORT";
			AssertEquals(true, uSDeclarationWarehouse.IsSpecified);

			uSDeclarationWarehouse.Port = "";
			AssertEquals(false, uSDeclarationWarehouse.IsSpecified);
		}
	}
}
