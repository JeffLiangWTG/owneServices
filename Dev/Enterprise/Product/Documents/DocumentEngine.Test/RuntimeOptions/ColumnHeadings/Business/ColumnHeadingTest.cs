using System.IO;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ColumnHeading))]
	public class ColumnHeadingTest : ValueObjectTestCase
	{
		public static void AssertPropertyValues(object columnHeading, string displayLabel, string description, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden)
		{
			AssertPropertyValues((ColumnHeading)columnHeading, displayLabel, description, headingText, originalColumnNumber, currentPosition, widthInPixels, hidden, false);
		}

		public static void AssertPropertyValues(object columnHeading, string displayLabel, string description, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, bool hideIfEmpty)
		{
			AssertPropertyValues((ColumnHeading)columnHeading, displayLabel, description, headingText, originalColumnNumber, currentPosition, widthInPixels, hidden, hideIfEmpty);
		}

		public static void AssertPropertyValues(ColumnHeading columnHeading, string displayLabel, string description, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, string tagName = "")
		{
			AssertEquals("CurrentPosition", currentPosition, columnHeading.CurrentPosition);
			AssertEquals("DisplayLabel", displayLabel, columnHeading.DisplayLabel);
			AssertEquals("Description", description, columnHeading.Description);
			AssertEquals("HeadingText", headingText, columnHeading.HeadingText);
			AssertEquals("TagName", tagName, columnHeading.TagName);
			AssertEquals("Hidden", hidden, columnHeading.Hidden);
			AssertEquals("OriginalColumnNumber", originalColumnNumber, columnHeading.OriginalColumnNumber);
			AssertEquals("WidthInPixels", widthInPixels, columnHeading.WidthInPixels);
		}

		public static void AssertPropertyValues(ColumnHeading columnHeading, string displayLabel, string description, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, bool hideIfEmpty)
		{
			AssertEquals("CurrentPosition", currentPosition, columnHeading.CurrentPosition);
			AssertEquals("DisplayLabel", displayLabel, columnHeading.DisplayLabel);
			AssertEquals("Description", description, columnHeading.Description);
			AssertEquals("HeadingText", headingText, columnHeading.HeadingText);
			AssertEquals("Hidden", hidden, columnHeading.Hidden);
			AssertEquals("OriginalColumnNumber", originalColumnNumber, columnHeading.OriginalColumnNumber);
			AssertEquals("WidthInPixels", widthInPixels, columnHeading.WidthInPixels);
			AssertEquals("HideIfDescriptionEmpty", hideIfEmpty, columnHeading.HideIfDescriptionEmpty);
		}

		public static void AssertPropertyValues(object columnHeading, string displayLabel, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, bool showPerformanceWarning, bool hideIfEmpty)
		{
			AssertPropertyValues((ColumnHeading)columnHeading, displayLabel, headingText, originalColumnNumber, currentPosition, widthInPixels, hidden, showPerformanceWarning, hideIfEmpty);
		}

		public static void AssertPropertyValues(ColumnHeading columnHeading, string displayLabel, string headingText, int originalColumnNumber, int currentPosition, int widthInPixels, bool hidden, bool showPerformanceWarning, bool hideIfEmpty)
		{
			AssertEquals("CurrentPosition", currentPosition, columnHeading.CurrentPosition);
			AssertEquals("DisplayLabel", displayLabel, columnHeading.DisplayLabel);
			AssertEquals("HeadingText", headingText, columnHeading.HeadingText);
			AssertEquals("Hidden", hidden, columnHeading.Hidden);
			AssertEquals("OriginalColumnNumber", originalColumnNumber, columnHeading.OriginalColumnNumber);
			AssertEquals("WidthInPixels", widthInPixels, columnHeading.WidthInPixels);
			AssertEquals("ShowPerformanceWarning", showPerformanceWarning, columnHeading.ShowPerformanceWarning);
			AssertEquals("HideIfDescriptionEmpty", hideIfEmpty, columnHeading.HideIfDescriptionEmpty);
		}

		void AssertTestData(ColumnHeading heading)
		{
			AssertPropertyValues(heading, "Show this", "Desc", "Boo", 2, 1, 3, true, true);
		}

		void FillWithTestData(ColumnHeading heading)
		{
			heading.DisplayLabel = "Show this";
			heading.CurrentPosition = 1;
			heading.Description = "Desc";
			heading.HeadingText = "Boo";
			heading.Hidden = true;
			heading.OriginalColumnNumber = 2;
			heading.WidthInPixels = 3;
			heading.ShowPerformanceWarning = true;
			heading.HideIfDescriptionEmpty = true;
		}

		void FillWithTestData()
		{
			FillWithTestData(Heading);
		}

		public void TestClone()
		{
			FillWithTestData();
			ColumnHeading clone = Heading.Clone();
			AssertTestData(clone);
		}

		public void TestOverloadedConstructor()
		{
			ColumnHeading columnHeading = new ColumnHeading("x", "y", "z", 1, 2, 3, true);
			AssertPropertyValues(columnHeading, "x", "y", "z", 1, 2, 3, true, false);
		}

		public void TestProperties()
		{
			AssertPropertyValues(Heading, "Test", "Test Description", "Test Heading Text", 0, 0, 0, false, false);
			FillWithTestData();
			AssertTestData(Heading);
		}

		public void TestSerialisation()
		{
			ColumnHeading toBeSerialised = new ColumnHeading();
			FillWithTestData(toBeSerialised);
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ColumnHeading));

			string xML;
			using (StringWriter stream = new StringWriter())
			{
				serialiser.Serialize(stream, toBeSerialised);
				xML = stream.ToString();
			}

			ColumnHeading deSerialised;
			using (StringReader stream = new StringReader(xML))
			{
				deSerialised = (ColumnHeading)serialiser.Deserialize(stream);
			}

			string expectedXML =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<ColumnHeading>
  <DisplayLabel>Show this</DisplayLabel>
  <OriginalColumnNumber>2</OriginalColumnNumber>
  <Description>Desc</Description>
  <HeadingText>Boo</HeadingText>
  <TagName />
  <Hidden>true</Hidden>
  <WidthInPixels>3</WidthInPixels>
  <CurrentPosition>1</CurrentPosition>
  <HideIfDescriptionEmpty>true</HideIfDescriptionEmpty>
</ColumnHeading>";

			AssertPropertyValues(toBeSerialised, deSerialised.DisplayLabel, deSerialised.Description, deSerialised.HeadingText, deSerialised.OriginalColumnNumber, deSerialised.CurrentPosition, deSerialised.WidthInPixels, deSerialised.Hidden, deSerialised.HideIfDescriptionEmpty);
			AssertEquals("XML looks like this", expectedXML, xML);
		}
		public void TestSerialisation_WithTagName()
		{
			ColumnHeading toBeSerialised = new ColumnHeading();
			FillWithTestData(toBeSerialised);
			toBeSerialised.TagName = "Aragon";
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ColumnHeading));

			string xML;
			using (StringWriter stream = new StringWriter())
			{
				serialiser.Serialize(stream, toBeSerialised);
				xML = stream.ToString();
			}

			ColumnHeading deSerialised;
			using (StringReader stream = new StringReader(xML))
			{
				deSerialised = (ColumnHeading)serialiser.Deserialize(stream);
			}

			string expectedXML =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<ColumnHeading>
  <DisplayLabel>Show this</DisplayLabel>
  <OriginalColumnNumber>2</OriginalColumnNumber>
  <Description>Desc</Description>
  <HeadingText>Boo</HeadingText>
  <TagName>Aragon</TagName>
  <Hidden>true</Hidden>
  <WidthInPixels>3</WidthInPixels>
  <CurrentPosition>1</CurrentPosition>
  <HideIfDescriptionEmpty>true</HideIfDescriptionEmpty>
</ColumnHeading>";

			AssertPropertyValues(toBeSerialised, deSerialised.DisplayLabel, deSerialised.Description, deSerialised.HeadingText, deSerialised.OriginalColumnNumber, deSerialised.CurrentPosition, deSerialised.WidthInPixels, deSerialised.Hidden, deSerialised.TagName);
			AssertEquals("XML looks like this", expectedXML, xML);
		}

		public void TestToString()
		{
			AssertEquals("ToString() should have returned Description", "Test Description", Heading.ToString());
			Heading.Description = string.Empty;
			AssertEquals("ToString() should have returned DisplayedLabel ", "Test", Heading.ToString());
		}

		public void TestIsSafeToRemove()
		{
			ColumnHeading a = new ColumnHeading("a");
			ColumnHeading b = new ColumnHeading("b");
			ColumnHeading c = new ColumnHeading("c");
			ColumnHeading d = new ColumnHeading("d");
			ColumnHeading e = new ColumnHeading("e");

			a.Hidden = true;
			b.Hidden = true;
			e.Hidden = true;

			a.ReferencedBy.Add(b);
			b.ReferencedBy.Add(c);
			b.ReferencedBy.Add(d);
			c.ReferencedBy.Add(e);

			AssertEquals("Isn't safe to remove because is referenced by [b] which is not safe to remove", false, a.IsSafeToRemove);
			AssertEquals("Isn't safe to remove because is referenced by [c, d] which are not safe to remove", false, b.IsSafeToRemove);
			AssertEquals("Isn't safe to remove because is displayed", false, c.IsSafeToRemove);
			AssertEquals("Isn't safe to remove because is displayed", false, d.IsSafeToRemove);
			AssertEquals("Is safe to remove because is neither referenced nor displayed", true, e.IsSafeToRemove);
		}

		#region Implementation

		ColumnHeading Heading
		{
			get
			{
				if (heading == null)
				{
					heading = new ColumnHeading("Test");
					heading.HeadingText = "Test Heading Text";
					heading.ShowPerformanceWarning = true;
					heading.Description = "Test Description";
				}
				return heading;
			}
		}
		ColumnHeading heading;

		#endregion
	}
}
