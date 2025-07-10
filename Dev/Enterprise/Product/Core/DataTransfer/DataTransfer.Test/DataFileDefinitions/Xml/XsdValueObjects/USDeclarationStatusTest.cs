using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationStatus))]
	sealed class USDeclarationStatusTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationStatus uSDeclarationStatus = new Xsd.USDeclarationStatus();
			AssertEquals(false, uSDeclarationStatus.IsSpecified);

			uSDeclarationStatus.DutyDueDate = ZDate.Today;
			AssertEquals(true, uSDeclarationStatus.IsSpecified);

			uSDeclarationStatus.DutyDueDate = ZDate.Empty;
			uSDeclarationStatus.LiquidationDate = ZDateTime.Today.Date;
			AssertEquals(true, uSDeclarationStatus.IsSpecified);

			uSDeclarationStatus.LiquidationDate = ZDate.Empty;
			uSDeclarationStatus.EntryDate = ZDateTime.Today.Date;
			AssertEquals(true, uSDeclarationStatus.IsSpecified);

			uSDeclarationStatus.EntryDate = ZDate.Empty;
			uSDeclarationStatus.Dispositions.AddNew();
			AssertEquals(true, uSDeclarationStatus.IsSpecified);

			uSDeclarationStatus = new Xsd.USDeclarationStatus();
			AssertEquals(false, uSDeclarationStatus.IsSpecified);

			uSDeclarationStatus.OGADispositions.AddNew();
			AssertEquals(true, uSDeclarationStatus.IsSpecified);
		}
	}
}
