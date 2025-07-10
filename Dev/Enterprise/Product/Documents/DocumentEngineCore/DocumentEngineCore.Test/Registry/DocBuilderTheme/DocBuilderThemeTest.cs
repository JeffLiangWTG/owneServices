using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderTheme))]
	class DocBuilderThemeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCopyThemeItemsTo()
		{
			var expected = (DocBuilderTheme)GetNewBusinessObject();
			var actual = new DocBuilderTheme("Target", ZBool.True);
			expected.CopyThemeItemsTo(actual);

			AssertEquals("Name", "Target", actual.Name);
			AssertEquals("IsCustomizable", ZBool.True, actual.IsCustomizable);
			AssertThemeItemEquals(expected.DocumentHeading, actual.DocumentHeading);
			AssertThemeItemEquals(expected.PageNumberHeading, actual.PageNumberHeading);
			AssertThemeItemEquals(expected.PrimaryHeading, actual.PrimaryHeading);
			AssertThemeItemEquals(expected.PrimaryBody, actual.PrimaryBody);
			AssertThemeItemEquals(expected.SecondaryHeading, actual.SecondaryHeading);
			AssertThemeItemEquals(expected.SecondaryBody, actual.SecondaryBody);
		}

		public void TestConstructor()
		{
			var theme = new DocBuilderTheme("Is Customizable", true);
			AssertEquals("theme.Name", "Is Customizable", theme.Name);
			AssertEquals("theme.NameMultilingual", (NoResString)"Is Customizable", theme.NameMultilingual);
			AssertEquals("theme.IsCustomizable", true, theme.IsCustomizable);

			var nameMultilingual = ResString.GetMultilingualString("C75D6EE9-4916-412B-B1FF-F7BDE03D0EF6", "Test");
			theme = new DocBuilderTheme("Is Not Customizable", nameMultilingual, false);
			AssertEquals("theme.Name", "Is Not Customizable", theme.Name);
			AssertEquals("theme.NameMultilingual", nameMultilingual, theme.NameMultilingual);
			AssertEquals("theme.IsCustomizable", false, theme.IsCustomizable);
		}

		public void TestItems()
		{
			var theme = new DocBuilderTheme("Test", true);
			AssertEquals("theme.Items.Length", 6, theme.Items.Length);
			AssertEquals("theme.Items[0].Name", DocBuilderThemeItemList.Codes.DocumentHeading, theme.Items[0].Name);
			AssertEquals("theme.Items[1].Name", DocBuilderThemeItemList.Codes.PageNumberHeading, theme.Items[1].Name);
			AssertEquals("theme.Items[2].Name", DocBuilderThemeItemList.Codes.PrimaryHeading, theme.Items[2].Name);
			AssertEquals("theme.Items[3].Name", DocBuilderThemeItemList.Codes.PrimaryBody, theme.Items[3].Name);
			AssertEquals("theme.Items[4].Name", DocBuilderThemeItemList.Codes.SecondaryHeading, theme.Items[4].Name);
			AssertEquals("theme.Items[5].Name", DocBuilderThemeItemList.Codes.SecondaryBody, theme.Items[5].Name);

			AssertEquals("theme.Items[0].Description", DocBuilderThemeItemList.Descriptions.DocumentHeading, theme.Items[0].Description);
			AssertEquals("theme.Items[1].Description", DocBuilderThemeItemList.Descriptions.PageNumberHeading, theme.Items[1].Description);
			AssertEquals("theme.Items[2].Description", DocBuilderThemeItemList.Descriptions.PrimaryHeading, theme.Items[2].Description);
			AssertEquals("theme.Items[3].Description", DocBuilderThemeItemList.Descriptions.PrimaryBody, theme.Items[3].Description);
			AssertEquals("theme.Items[4].Description", DocBuilderThemeItemList.Descriptions.SecondaryHeading, theme.Items[4].Description);
			AssertEquals("theme.Items[5].Description", DocBuilderThemeItemList.Descriptions.SecondaryBody, theme.Items[5].Description);
		}

		public void TestItemsReadOnly()
		{
			var theme = new DocBuilderTheme("Test", true);
			AssertItemReadOnly(theme.DocumentHeading, false, false, false, false, false, false, false);
			AssertItemReadOnly(theme.PageNumberHeading, false, false, false, false, false, false, false);
			AssertItemReadOnly(theme.PrimaryHeading, false, false, false, false, false, false, false);
			AssertItemReadOnly(theme.PrimaryBody, true, false, true, true, true, true, true);
			AssertItemReadOnly(theme.SecondaryHeading, false, false, false, false, false, false, false);
			AssertItemReadOnly(theme.SecondaryBody, true, false, true, true, true, true, true);
		}

		public void TestReadXml()
		{
			var xml =
				@"<DocBuilderTheme>
  <Name>Test</Name>
  <DocBuilderThemeItems>
    <DocBuilderThemeItem>
      <Name>Document Heading</Name>
      <Color1>1</Color1>
      <Color2>2</Color2>
			<FontFamily>Arial</FontFamily>
			<FontSize>20</FontSize>
			<FontBold>Y</FontBold>
			<FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Page Number Heading</Name>
      <Color1>3</Color1>
      <Color2>4</Color2>
			<FontFamily>Arial</FontFamily>
			<FontSize>10</FontSize>
			<FontBold>Y</FontBold>
			<FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Primary Heading</Name>
      <Color1>5</Color1>
      <Color2>6</Color2>
			<FontFamily>Arial</FontFamily>
			<FontSize>8</FontSize>
			<FontBold>Y</FontBold>
			<FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Primary Body</Name>
      <Color1>7</Color1>
      <Color2>8</Color2>
			<FontFamily>Arial</FontFamily>
			<FontSize>8</FontSize>
			<FontBold>N</FontBold>
			<FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Secondary Heading</Name>
      <Color1>9</Color1>
      <Color2>10</Color2>
			<FontFamily>Arial</FontFamily>
			<FontSize>8</FontSize>
			<FontBold>Y</FontBold>
			<FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Secondary Body</Name>
      <Color1>11</Color1>
      <Color2>12</Color2>
			<FontFamily>Arial</FontFamily>
			<FontSize>8</FontSize>
			<FontBold>N</FontBold>
			<FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
  </DocBuilderThemeItems>
</DocBuilderTheme>";
			var settings = new XmlReaderSettings();
			var reader = XmlReader.Create(new StringReader(xml), settings);

			var theme = new DocBuilderTheme(string.Empty, true);
			theme.ReadXml(reader);
			CombineAssertions(delegate()
			{
				AssertEquals("theme.Name", "Test", theme.Name);
				AssertEquals("theme.Items.Length", 6, theme.Items.Length);
				AssertEquals("theme.Items[0].Name", "Document Heading", theme.Items[0].Name);
				AssertEquals("theme.Items[0].Color1", 1, theme.Items[0].Color1.ToArgb());
				AssertEquals("theme.Items[0].Color2", 2, theme.Items[0].Color2.ToArgb());
				AssertEquals("theme.Items[1].Name", "Page Number Heading", theme.Items[1].Name);
				AssertEquals("theme.Items[1].Color1", 3, theme.Items[1].Color1.ToArgb());
				AssertEquals("theme.Items[1].Color2", 4, theme.Items[1].Color2.ToArgb());
				AssertEquals("theme.Items[2].Name", "Primary Heading", theme.Items[2].Name);
				AssertEquals("theme.Items[2].Color1", 5, theme.Items[2].Color1.ToArgb());
				AssertEquals("theme.Items[2].Color2", 6, theme.Items[2].Color2.ToArgb());
				AssertEquals("theme.Items[3].Name", "Primary Body", theme.Items[3].Name);
				AssertEquals("theme.Items[3].Color1", 7, theme.Items[3].Color1.ToArgb());
				AssertEquals("theme.Items[3].Color2", 8, theme.Items[3].Color2.ToArgb());
				AssertEquals("theme.Items[4].Name", "Secondary Heading", theme.Items[4].Name);
				AssertEquals("theme.Items[4].Color1", 9, theme.Items[4].Color1.ToArgb());
				AssertEquals("theme.Items[4].Color2", 10, theme.Items[4].Color2.ToArgb());
				AssertEquals("theme.Items[5].Name", "Secondary Body", theme.Items[5].Name);
				AssertEquals("theme.Items[5].Color1", 11, theme.Items[5].Color1.ToArgb());
				AssertEquals("theme.Items[5].Color2", 12, theme.Items[5].Color2.ToArgb());
			});
		}

		public void TestWriteXml()
		{
			var builder = new StringBuilder();
			var settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			settings.Indent = true;
			var writer = XmlWriter.Create(builder, settings);

			var theme = (DocBuilderTheme)GetNewBusinessObject();
			theme.WriteXml(writer);
			writer.Close();

			AssertMultilineASCIIEquals("theme.WriteXml(writer)",
				@"<DocBuilderTheme>
  <Name>Test</Name>
  <DocBuilderThemeItems>
    <DocBuilderThemeItem>
      <Name>Document Heading</Name>
      <Color1>1</Color1>
      <Color2>2</Color2>
      <FontFamily>Arial</FontFamily>
      <FontSize>20</FontSize>
      <FontBold>Y</FontBold>
      <FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Page Number Heading</Name>
      <Color1>3</Color1>
      <Color2>4</Color2>
      <FontFamily>Arial</FontFamily>
      <FontSize>10</FontSize>
      <FontBold>Y</FontBold>
      <FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Primary Heading</Name>
      <Color1>5</Color1>
      <Color2>6</Color2>
      <FontFamily>Arial</FontFamily>
      <FontSize>8</FontSize>
      <FontBold>Y</FontBold>
      <FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Primary Body</Name>
      <Color1>7</Color1>
      <Color2>8</Color2>
      <FontFamily>Arial</FontFamily>
      <FontSize>8</FontSize>
      <FontBold>N</FontBold>
      <FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Secondary Heading</Name>
      <Color1>9</Color1>
      <Color2>10</Color2>
      <FontFamily>Arial</FontFamily>
      <FontSize>8</FontSize>
      <FontBold>Y</FontBold>
      <FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
    <DocBuilderThemeItem>
      <Name>Secondary Body</Name>
      <Color1>11</Color1>
      <Color2>12</Color2>
      <FontFamily>Arial</FontFamily>
      <FontSize>8</FontSize>
      <FontBold>N</FontBold>
      <FontItalic>N</FontItalic>
      <FontColor>-1</FontColor>
    </DocBuilderThemeItem>
  </DocBuilderThemeItems>
</DocBuilderTheme>", builder.ToString());
		}

		#region Implementation

		void AssertThemeItemEquals(DocBuilderThemeItem expected, DocBuilderThemeItem actual)
		{
			AssertEquals("Name", expected.Name, actual.Name);
			AssertEquals("Color1Argb", expected.Color1Argb, actual.Color1Argb);
			AssertEquals("Color2Argb", expected.Color2Argb, actual.Color2Argb);
			AssertEquals("FontName", expected.FontName, actual.FontName);
			AssertEquals("FontSize", expected.FontSize, actual.FontSize);
			AssertEquals("FontColorArgb", expected.FontColorArgb, actual.FontColorArgb);
			AssertEquals("FontBold", expected.FontBold, actual.FontBold);
			AssertEquals("FontItalic", expected.FontItalic, actual.FontItalic);
		}

		void AssertItemReadOnly(
			DocBuilderThemeItem themeItem, bool expectedColor1Argb_ReadOnly, bool expectedColor2Argb_ReadOnly, bool expectedFontName_ReadOnly,
			bool expectedFontSize_ReadOnly, bool expectedFontColorArgb_ReadOnly, bool expectedFontBold_ReadOnly, bool expectedFontItalic_ReadOnly)
		{
			CombineAssertions(delegate()
			{
				AssertEquals("themeItem.Color1ArgbInfo.ReadOnly", expectedColor1Argb_ReadOnly, themeItem.Color1ArgbInfo.ReadOnly);
				AssertEquals("themeItem.Color2ArgbInfo.ReadOnly", expectedColor2Argb_ReadOnly, themeItem.Color2ArgbInfo.ReadOnly);
				AssertEquals("themeItem.FontNameInfo.ReadOnly", expectedFontName_ReadOnly, themeItem.FontNameInfo.ReadOnly);
				AssertEquals("themeItem.FontSizeInfo.ReadOnly", expectedFontSize_ReadOnly, themeItem.FontSizeInfo.ReadOnly);
				AssertEquals("themeItem.FontColorArgbInfo.ReadOnly", expectedFontColorArgb_ReadOnly, themeItem.FontColorArgbInfo.ReadOnly);
				AssertEquals("themeItem.FontBoldInfo.ReadOnly", expectedFontBold_ReadOnly, themeItem.FontBoldInfo.ReadOnly);
				AssertEquals("themeItem.FontItalicInfo.ReadOnly", expectedFontItalic_ReadOnly, themeItem.FontItalicInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = new DocBuilderTheme("Test", true);
			result.DocumentHeading.Color1 = Color.FromArgb(1);
			result.DocumentHeading.Color2 = Color.FromArgb(2);
			result.DocumentHeading.Font = new Font("Arial", 20, FontStyle.Bold);
			result.DocumentHeading.FontColor = Color.White;
			result.PageNumberHeading.Color1 = Color.FromArgb(3);
			result.PageNumberHeading.Color2 = Color.FromArgb(4);
			result.PageNumberHeading.Font = new Font("Arial", 10, FontStyle.Bold);
			result.PageNumberHeading.FontColor = Color.White;
			result.PrimaryHeading.Color1 = Color.FromArgb(5);
			result.PrimaryHeading.Color2 = Color.FromArgb(6);
			result.PrimaryHeading.Font = new Font("Arial", 8, FontStyle.Bold);
			result.PrimaryHeading.FontColor = Color.White;
			result.PrimaryBody.Color1 = Color.FromArgb(7);
			result.PrimaryBody.Color2 = Color.FromArgb(8);
			result.PrimaryBody.Font = new Font("Arial", 8, FontStyle.Regular);
			result.PrimaryBody.FontColor = Color.White;
			result.SecondaryHeading.Color1 = Color.FromArgb(9);
			result.SecondaryHeading.Color2 = Color.FromArgb(10);
			result.SecondaryHeading.Font = new Font("Arial", 8, FontStyle.Bold);
			result.SecondaryHeading.FontColor = Color.White;
			result.SecondaryBody.Color1 = Color.FromArgb(11);
			result.SecondaryBody.Color2 = Color.FromArgb(12);
			result.SecondaryBody.Font = new Font("Arial", 8, FontStyle.Regular);
			result.SecondaryBody.FontColor = Color.White;
			return result;
		}

		#endregion
	}
}
