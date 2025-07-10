using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationCollection))]
	sealed class BillOfLadingNumberCustomisationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BillOfLadingNumberCustomisationCollection>
	{
		public void TestAddNewCoppiesSettings()
		{
			const NumberCustomisationElementCategories newCategories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;

			BillOfLadingNumberCustomisationsByServiceLevel host = new BillOfLadingNumberCustomisationsByServiceLevel();
			host.AllowNonAlphanumericCharacters = false;
			host.EnableMacroInsertion = false;
			host.Categories = NumberCustomisationElementCategories.Default;

			BillOfLadingNumberCustomisationCollection collection = new BillOfLadingNumberCustomisationCollection(host);

			BillOfLadingNumberCustomisation customisation1 = collection.AddNew();
			AssertEquals("customisation1.AllowNonAlphanumericCharacters", false, customisation1.AllowNonAlphanumericCharacters);
			AssertEquals("customisation1.EnableMacroInsertion", false, customisation1.EnableMacroInsertion);
			AssertEquals("customisation1.Categories", NumberCustomisationElementCategories.Default, customisation1.Categories);

			host.AllowNonAlphanumericCharacters = true;
			host.EnableMacroInsertion = true;
			host.Categories = newCategories;

			BillOfLadingNumberCustomisation customisation2 = collection.AddNew();
			AssertEquals("customisation2.AllowNonAlphanumericCharacters", true, customisation2.AllowNonAlphanumericCharacters);
			AssertEquals("customisation2.EnableMacroInsertion", true, customisation2.EnableMacroInsertion);
			AssertEquals("customisation2.Categories", newCategories, customisation2.Categories);
		}

		public void TestSerialisation()
		{
			BillOfLadingNumberCustomisationsByServiceLevel customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisationCollection collection = new BillOfLadingNumberCustomisationCollection(customisations);
			BillOfLadingNumberCustomisation customisation1 = collection.AddNew();
			BillOfLadingNumberCustomisation customisation2 = collection.AddNew();
			BillOfLadingNumberCustomisation customisation3 = collection.AddNew();

			customisation1.ServiceLevel = "ALL";
			customisation2.ServiceLevel = "ABC";
			customisation3.ServiceLevel = "DEF";

			AssertNotNull(collection["ALL"]);
			AssertNotNull(collection["ABC"]);
			AssertNotNull(collection["DEF"]);

			string xml;

			using (System.IO.StringWriter stream = new System.IO.StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Customisations");
				collection.WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				xml = stream.ToString();
			}

			const string expectedXml =
				"<Customisations>" +
					"<CustomisationByServiceLevel>" +
						"<ServiceLevel>ALL</ServiceLevel>" +
						"<RemoveFountainPrefix>N</RemoveFountainPrefix>" +
						"<AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols>" +
						"<CheckDigitAlgorithm>NON</CheckDigitAlgorithm>" +
						"<UseShipmentSequenceNumber>N</UseShipmentSequenceNumber>" +
						"<Elements>" +
							"<Element key=\"SequenceNumber\">" +
								"<Order>50</Order>" +
								"<CheckDigit>Y</CheckDigit>" +
								"<Detail>8</Detail>" +
							"</Element>" +
						"</Elements>" +
					"</CustomisationByServiceLevel>" +
					"<CustomisationByServiceLevel>" +
						"<ServiceLevel>ABC</ServiceLevel>" +
						"<RemoveFountainPrefix>N</RemoveFountainPrefix>" +
						"<AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols>" +
						"<CheckDigitAlgorithm>NON</CheckDigitAlgorithm>" +
						"<UseShipmentSequenceNumber>N</UseShipmentSequenceNumber>" +
						"<Elements>" +
							"<Element key=\"SequenceNumber\">" +
								"<Order>50</Order>" +
								"<CheckDigit>Y</CheckDigit>" +
								"<Detail>8</Detail>" +
							"</Element>" +
						"</Elements>" +
					"</CustomisationByServiceLevel>" +
						"<CustomisationByServiceLevel>" +
						"<ServiceLevel>DEF</ServiceLevel>" +
						"<RemoveFountainPrefix>N</RemoveFountainPrefix>" +
						"<AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols>" +
						"<CheckDigitAlgorithm>NON</CheckDigitAlgorithm>" +
						"<UseShipmentSequenceNumber>N</UseShipmentSequenceNumber>" +
						"<Elements>" +
							"<Element key=\"SequenceNumber\">" +
								"<Order>50</Order>" +
								"<CheckDigit>Y</CheckDigit>" +
								"<Detail>8</Detail>" +
							"</Element>" +
						"</Elements>" +
					"</CustomisationByServiceLevel>" +
				"</Customisations>";

			AssertMultilineASCIIEquals("", expectedXml.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));

			BillOfLadingNumberCustomisationsByServiceLevel customisations2 = new BillOfLadingNumberCustomisationsByServiceLevel();
			BillOfLadingNumberCustomisationCollection collection2 = new BillOfLadingNumberCustomisationCollection(customisations2);

			using (System.IO.StringReader stream = new System.IO.StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				collection2.ReadXml(reader);
			}

			AssertNotNull(collection2["ALL"]);
			AssertNotNull(collection2["ABC"]);
			AssertNotNull(collection2["DEF"]);
		}

		#region Implementation

		protected override BillOfLadingNumberCustomisationCollection GetCollectionToTest()
		{
			return new BillOfLadingNumberCustomisationCollection(new BillOfLadingNumberCustomisationsByServiceLevel());
		}

		int itemsMade;
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var newItem = new BillOfLadingNumberCustomisation();
			newItem.Elements[++itemsMade].Detail = "9";

			return newItem;
		}

		#endregion
	}
}
