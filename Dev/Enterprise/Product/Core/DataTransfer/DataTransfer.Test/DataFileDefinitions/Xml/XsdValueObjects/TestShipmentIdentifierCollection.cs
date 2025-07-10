using Enterprise.DataTransfer.Xml.XsdVersion1;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(ShipmentIdentifierCollection))]
	sealed class TestShipmentIdentifierCollection : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ShipmentIdentifier value = null;
			value = new Xsd.ShipmentIdentifierCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestAddNew()
		{
			var collection = new ShipmentIdentifierCollection();

			collection.AddNew(ShipmentIdentifierType.Housebill, "hello");
			collection.AddNew(ShipmentIdentifierType.Other, "world");

			AssertEquals(2, collection.Count);
			AssertEquals("hello", collection.FindFirst(ShipmentIdentifierType.Housebill).Value);
			AssertEquals("world", collection.FindFirst(ShipmentIdentifierType.Other).Value);
		}

		public void TestFindIdentifier()
		{
			ShipmentIdentifierCollection identifierCollection = new ShipmentIdentifierCollection();

			ShipmentIdentifier id1 = identifierCollection.AddNew();
			id1.Value = "val1";
			id1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			ShipmentIdentifier id2 = identifierCollection.AddNew();
			id2.Value = "val2";
			id2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			ShipmentIdentifier id3 = identifierCollection.AddNew();
			id3.Value = "val3";
			id3.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			ShipmentIdentifier id4 = identifierCollection.AddNew();
			id4.Value = "val4";
			id4.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.CoLoadMaster;

			ShipmentIdentifier id5 = identifierCollection.AddNew();
			id5.Value = "val5";
			id5.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			AssertEquals(id3.Value, identifierCollection.FindFirst(Xsd.ShipmentIdentifierType.Housebill).Value);
			AssertEquals(id4.Value, identifierCollection.FindFirst(Xsd.ShipmentIdentifierType.CoLoadMaster).Value);
			AssertEquals(id1.Value, identifierCollection.FindFirst(Xsd.ShipmentIdentifierType.Other).Value);
		}

		public void TestFind()
		{
			ShipmentIdentifierCollection shipmentIdentifierCollection = new ShipmentIdentifierCollection();

			ShipmentIdentifier identifier = shipmentIdentifierCollection.AddNew();
			identifier.Value = "val1";
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			identifier = shipmentIdentifierCollection.AddNew();
			identifier.Value = "val2";
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			identifier = shipmentIdentifierCollection.AddNew();
			identifier.Value = "val3";
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			identifier = shipmentIdentifierCollection.AddNew();
			identifier.Value = "val4";
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			identifier = shipmentIdentifierCollection.AddNew();
			identifier.Value = "val5";
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			ShipmentIdentifierCollection findResult = shipmentIdentifierCollection.Find(Xsd.ShipmentIdentifierType.Housebill);
			AssertEquals(2, findResult.Count);
			AssertEquals("val3", findResult[0].Value);
			AssertEquals("val5", findResult[1].Value);
		}
	}
}
