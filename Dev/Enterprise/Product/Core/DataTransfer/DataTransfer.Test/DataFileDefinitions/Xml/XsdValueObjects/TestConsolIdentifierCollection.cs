using Enterprise.DataTransfer.Xml.XsdVersion1;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(ConsolIdentifierCollection))]
	sealed class TestConsolIdentifierCollection : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ConsolIdentifier value = null;
			value = new Xsd.ConsolIdentifierCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestFindFirstIdentifier()
		{
			ConsolIdentifierCollection idCollection = new ConsolIdentifierCollection();

			ConsolIdentifier id1 = idCollection.AddNew();
			id1.Value = "val1";
			id1.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;

			ConsolIdentifier id2 = idCollection.AddNew();
			id2.Value = "val2";
			id2.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;

			ConsolIdentifier id3 = idCollection.AddNew();
			id3.Value = "val3";
			id3.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;

			ConsolIdentifier id4 = idCollection.AddNew();
			id4.Value = "val4";
			id4.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;

			AssertEquals(id3.Value, idCollection.FindFirst(Xsd.ConsolIdentifierType.MasterWaybill).Value);
		}

		public void TestFind()
		{
			ConsolIdentifierCollection consolIdentifierCollection = new ConsolIdentifierCollection();

			ConsolIdentifier identifier = consolIdentifierCollection.AddNew();
			identifier.Value = "val1";
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;

			identifier = consolIdentifierCollection.AddNew();
			identifier.Value = "val2";
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;

			identifier = consolIdentifierCollection.AddNew();
			identifier.Value = "val3";
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;

			identifier = consolIdentifierCollection.AddNew();
			identifier.Value = "val4";
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;

			identifier = consolIdentifierCollection.AddNew();
			identifier.Value = "val5";
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;

			ConsolIdentifierCollection findResult = consolIdentifierCollection.Find(Xsd.ConsolIdentifierType.MasterWaybill);
			AssertEquals(2, findResult.Count);
			AssertEquals("val3", findResult[0].Value);
			AssertEquals("val5", findResult[1].Value);
		}
	}
}
