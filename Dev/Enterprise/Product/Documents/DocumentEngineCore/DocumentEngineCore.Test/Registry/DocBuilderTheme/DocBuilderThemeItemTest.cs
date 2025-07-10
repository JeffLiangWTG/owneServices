using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderThemeItem))]
	class DocBuilderThemeItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCopyTo()
		{
			var expected = new MockDocBuilderThemeItem();
			expected.Name = "Source";
			expected.Color1Argb = 1;
			expected.Color2Argb = 2;
			expected.FontName = "Courier New";
			expected.FontSize = 3;
			expected.FontColorArgb = 4;
			expected.FontBold = ZBool.True;
			expected.FontItalic = ZBool.True;

			var actual = new MockDocBuilderThemeItem();
			expected.CopyTo(actual);
			AssertEquals("Name", expected.Name, actual.Name);
			AssertEquals("Color1Argb", expected.Color1Argb, actual.Color1Argb);
			AssertEquals("Color2Argb", expected.Color2Argb, actual.Color2Argb);
			AssertEquals("FontName", expected.FontName, actual.FontName);
			AssertEquals("FontSize", expected.FontSize, actual.FontSize);
			AssertEquals("FontColorArgb", expected.FontColorArgb, actual.FontColorArgb);
			AssertEquals("FontBold", expected.FontBold, actual.FontBold);
			AssertEquals("FontItalic", expected.FontItalic, actual.FontItalic);
		}

		public void TestSetFont()
		{
			var themeItem = new MockDocBuilderThemeItem();

			themeItem.Font = new Font("Arial", 8);
			AssertEquals("themeItem.FontName", "Arial", themeItem.FontName);
			AssertEquals("themeItem.FontSize", 8, themeItem.FontSize);
			AssertEquals("themeItem.FontBold", ZBool.False, themeItem.FontBold);
			AssertEquals("themeItem.FontItalic", ZBool.False, themeItem.FontItalic);

			themeItem.Font = new Font("Courier New", 12, FontStyle.Bold);
			AssertEquals("themeItem.FontName", "Courier New", themeItem.FontName);
			AssertEquals("themeItem.FontSize", 12, themeItem.FontSize);
			AssertEquals("themeItem.FontBold", ZBool.True, themeItem.FontBold);
			AssertEquals("themeItem.FontItalic", ZBool.False, themeItem.FontItalic);

			themeItem.Font = new Font("Times New Roman", 16, FontStyle.Italic);
			AssertEquals("themeItem.FontName", "Times New Roman", themeItem.FontName);
			AssertEquals("themeItem.FontSize", 16, themeItem.FontSize);
			AssertEquals("themeItem.FontBold", ZBool.False, themeItem.FontBold);
			AssertEquals("themeItem.FontItalic", ZBool.True, themeItem.FontItalic);

			themeItem.Font = new Font("Verdana", 20, FontStyle.Bold | FontStyle.Italic);
			AssertEquals("themeItem.FontName", "Verdana", themeItem.FontName);
			AssertEquals("themeItem.FontSize", 20, themeItem.FontSize);
			AssertEquals("themeItem.FontBold", ZBool.True, themeItem.FontBold);
			AssertEquals("themeItem.FontItalic", ZBool.True, themeItem.FontItalic);
		}

		public void TestGetFont()
		{
			var themeItem = new MockDocBuilderThemeItem();
			AssertEquals("themeItem.Font.Name", "Courier New", themeItem.Font.Name);
			AssertEquals("themeItem.Font.Size", 12f, themeItem.Font.Size);
			AssertEquals("themeItem.Font.Style", FontStyle.Bold | FontStyle.Italic, themeItem.Font.Style);
		}

		public void TestGetFont_WhenNotFound_ReturnsDefaultFont()
		{
			var themeItem = new MockDocBuilderThemeItem();
			themeItem.FontName = "Kelvin is not a font";
			AssertEquals("Should return default font Arial", "Arial", themeItem.Font.Name);
		}

		[RequiresSoftware(RequiredSoftware.OfficeFonts)]
		public void TestGetFontWithInValidStyle()
		{
			var themeItem = new MockDocBuilderThemeItem();

			themeItem.FontName = "Brush Script MT";
			themeItem.FontSize = 16;
			themeItem.FontBold = ZBool.False;
			themeItem.FontItalic = ZBool.False;

			AssertEquals("themeItem.Font.Name", "Brush Script MT", themeItem.Font.Name);
			AssertEquals("themeItem.Font.Size", 16f, themeItem.Font.Size);
			AssertEquals("themeItem.Font.Style: Regular is not an available style, should pick italic.", FontStyle.Italic, themeItem.Font.Style);
		}

		public void TestReadXml()
		{
			var xml =
@"<DocBuilderThemeItem>
  <Name>Mock</Name>
  <Color1>-65536</Color1>
  <Color2>-16776961</Color2>
  <FontFamily>Arial</FontFamily>
  <FontSize>10</FontSize>
  <FontBold>Y</FontBold>
  <FontItalic>Y</FontItalic>
	<FontColor>-1</FontColor>
</DocBuilderThemeItem>";
			var settings = new XmlReaderSettings();
			var reader = XmlReader.Create(new StringReader(xml), settings);

			var themeItem = new MockDocBuilderThemeItem();
			themeItem.ReadXml(reader);
			CombineAssertions(delegate()
			{
				AssertEquals("themeItem.Name", "Mock", themeItem.Name);
				AssertEquals("themeItem.Color1.ToArgb()", Color.Red.ToArgb(), themeItem.Color1.ToArgb());
				AssertEquals("themeItem.Color2.ToArgb()", Color.Blue.ToArgb(), themeItem.Color2.ToArgb());
				AssertEquals("themeItem.Font.FontFamily.Name", "Arial", themeItem.Font.FontFamily.Name);
				AssertEquals("themeItem.Font.Size", 10f, themeItem.Font.Size);
				AssertEquals("themeItem.Font.Style", FontStyle.Bold | FontStyle.Italic, themeItem.Font.Style);
				AssertEquals("themeItem.FontColor.ToArgb()", Color.White.ToArgb(), themeItem.FontColor.ToArgb());
			});
		}

		public void TestWriteXml()
		{
			var builder = new StringBuilder();
			var settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			settings.Indent = true;
			var writer = XmlWriter.Create(builder, settings);

			var themeItem = new MockDocBuilderThemeItem();
			themeItem.WriteXml(writer);
			writer.Close();

			AssertMultilineASCIIEquals("themeItem.WriteXml(writer)",
@"<DocBuilderThemeItem>
  <Name>Mock</Name>
  <Color1>-65536</Color1>
  <Color2>-16776961</Color2>
  <FontFamily>Courier New</FontFamily>
  <FontSize>12</FontSize>
  <FontBold>Y</FontBold>
  <FontItalic>Y</FontItalic>
  <FontColor>-16744448</FontColor>
</DocBuilderThemeItem>", builder.ToString());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MockDocBuilderThemeItem();
		}

		class MockDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal MockDocBuilderThemeItem()
			{
				Name = "Mock";
				Color1 = Color.Red;
				Color2 = Color.Blue;
				FontColor = Color.Green;
				FontName = "Courier New";
				FontSize = 12;
				FontBold = ZBool.True;
				FontItalic = ZBool.True;
			}
		}
	}
}
