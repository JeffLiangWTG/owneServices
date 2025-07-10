using System;
using System.Collections;
using System.Xml.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlSchemaNavigatorTest : TestCase
	{
		public void TestRootCurrentIsXmlSchema()
		{
			AssertEquals("Current when at the XmlSchema root", true, Navigator.Current is XmlSchema);
			AssertNull("No current element if at the XmlSchema root", Navigator.CurrentElement);

			Navigator.NavigateToElement("XmlInterchange");
			Navigator.NavigateBack();

			AssertEquals("Current when at the XmlSchema root", true, Navigator.Current is XmlSchema);
			AssertNull("No current element if at the XmlSchema root", Navigator.CurrentElement);
		}

		public void TestChildSchemaObjects()
		{
			XmlSchemaObject[] childSchemaObjects = ToArray(Navigator.ChildSchemaObjects);
			AssertEquals("There should be 1 child schema object (the XmlInterchange element)", 1, childSchemaObjects.Length);
			AssertEquals("First child schema object should be element XmlInterchange", "XmlInterchange", ((XmlSchemaElement)childSchemaObjects[0]).Name);

			Navigator.NavigateToElement("XmlInterchange");
			Navigator.NavigateToElement("InterchangeInfo");
			XmlSchemaObject[] interchangeElementChildSchemaObjects = ToArray(Navigator.ChildSchemaObjects);
			AssertEquals("There should be at least 5 child schema objects on the XmlInterchange elements", true, interchangeElementChildSchemaObjects.Length > 5);
		}

		public void TestChildSchemaObjects_WhenNoChildSchemaObjects()
		{
			Navigator.NavigateToElement("XmlInterchange");
			Navigator.NavigateToElement("InterchangeInfo");
			Navigator.NavigateToElement("Date");

			XmlSchemaObject[] childSchemaObjects = ToArray(Navigator.ChildSchemaObjects);
			AssertEquals("There should be no child schema objects", 0, childSchemaObjects.Length);
			AssertEquals("First child schema object should be element XmlInterchange", 0, childSchemaObjects.Length);

			Navigator.NavigateToElement("ElementThatDoesntExist");
			childSchemaObjects = ToArray(Navigator.ChildSchemaObjects);
			AssertEquals("There should be no child schema objects", 0, childSchemaObjects.Length);
			AssertEquals("First child schema object should be element XmlInterchange", 0, childSchemaObjects.Length);
		}

		public void TestCurrentElementHasAttribute()
		{
			Navigator.NavigateToElement("XmlInterchange");
			AssertEquals("Attribute that exists", true, Navigator.CurrentElementHasAttribute("Version"));
			AssertEquals("Attribute that doesnt exist", false, Navigator.CurrentElementHasAttribute("NonExistantAttribute"));
		}

		public void TestNavigateToElement_AndNavigateBack()
		{
			Navigator.NavigateToElement("XmlInterchange");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "XmlInterchange", Navigator.CurrentElement.Name);

			Navigator.NavigateToElement("Payload");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "Payload", Navigator.CurrentElement.Name);

			Navigator.NavigateBack();
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "XmlInterchange", Navigator.CurrentElement.Name);

			Navigator.NavigateToElement("Payload");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "Payload", Navigator.CurrentElement.Name);
		}

		public void TestNavigateToElement_WhenElementIsEmpty()
		{
			Navigator.NavigateToElement("XmlInterchange");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "XmlInterchange", Navigator.CurrentElement.Name);

			Navigator.NavigateToElement("InterchangeInfo", true);
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "InterchangeInfo", Navigator.CurrentElement.Name);

			Navigator.NavigateToElement("Payload");
			AssertEquals("A NavigateBack should be performed implicitly after we specify InterchangeInfo is IsElementEmpty=true", "Payload", Navigator.CurrentElement.Name);
		}

		public void TestNavigateToElement_AndNavigateBack_WhenElementsCannotBeFound()
		{
			Navigator.NavigateToElement("XmlInterchange");
			AssertEquals("Current - XmlInterchange", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement - XmlInterchange", "XmlInterchange", Navigator.CurrentElement.Name);

			Navigator.NavigateToElement("NonExistantElement");
			AssertNull("Current - XmlInterchange/NonExistantElement", Navigator.Current);
			AssertNull("CurrentElement - XmlInterchange/NonExistantElement", Navigator.CurrentElement);

			Navigator.NavigateToElement("NonExistantElement");
			AssertNull("Current - XmlInterchange/NonExistantElement/NonExistantElement", Navigator.Current);
			AssertNull("CurrentElement - XmlInterchange/NonExistantElement/NonExistantElement", Navigator.CurrentElement);

			Navigator.NavigateBack();
			AssertNull("Current - XmlInterchange/NonExistantElement", Navigator.Current);
			AssertNull("CurrentElement - XmlInterchange/NonExistantElement", Navigator.CurrentElement);

			Navigator.NavigateBack();
			AssertEquals("Current - back at XmlInterchange", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement - back at XmlInterchange", "XmlInterchange", Navigator.CurrentElement.Name);
		}

		public void TestNavigateToChoiceElement()
		{
			Schema = TestXmlSchemaDefinitions.Instance.SchemaWithMandatoryElementsAndAttributes;
			Navigator = new XmlSchemaNavigator(Schema);

			Navigator.NavigateToElement("WithMandatoryElementsAndAttributes");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "WithMandatoryElementsAndAttributes", Navigator.CurrentElement.Name);

			Navigator.NavigateToElement("Choice1");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "Choice1", Navigator.CurrentElement.Name);

			Navigator.NavigateBack();
			Navigator.NavigateToElement("Choice2");
			AssertEquals("Current", true, Navigator.Current is XmlSchemaElement);
			AssertEquals("CurrentElement", "Choice2", Navigator.CurrentElement.Name);
		}

		public void TestNavigateBack_TwiceIfWasEmptyElement()
		{
			Navigator.NavigateToElement("XmlInterchange");
			Navigator.NavigateToElement("InterchangeInfo");
			Navigator.NavigateToElement("EDIOrganisation", true);

			AssertEquals("Navigator should be at EDIOrganisation for the test", "EDIOrganisation", Navigator.CurrentElement.Name);
			Navigator.NavigateBack(true);
			AssertEquals("Navigator should be back at the root node now", "XmlInterchange", Navigator.CurrentElement.Name);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestNavigateBack_WhenThereIsNoBack()
		{
			Navigator.NavigateBack();
		}

		public void TestNavigateBack_WhenThereIsNoBack2()
		{
			Navigator.NavigateToElement("XmlInterchange");
			Navigator.NavigateBack();
			try
			{
				Navigator.NavigateBack();
				Fail("Expected an InvalidOperationException here");
			}
			catch (InvalidOperationException)
			{
				Assert(true);
			}
		}

		public void TestDepth()
		{
			AssertEquals(0, Navigator.Depth);
			Navigator.NavigateToElement("XmlInterchange");
			AssertEquals(1, Navigator.Depth);
			Navigator.NavigateBack();
			AssertEquals(0, Navigator.Depth);
		}

		#region Implementation

		XmlSchemaObject[] ToArray(IEnumerable schemaObjectEnumerable)
		{
			ArrayList result = new ArrayList();
			foreach (XmlSchemaObject schemaObject in schemaObjectEnumerable)
			{
				result.Add(schemaObject);
			}
			return (XmlSchemaObject[])result.ToArray(typeof(XmlSchemaObject));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Schema = XmlSchemaDefinitions.Instance.XmlInterchangeSchema;
			Navigator = new XmlSchemaNavigator(Schema);
		}

		XmlSchema Schema;
		XmlSchemaNavigator Navigator;

		#endregion
	}
}
