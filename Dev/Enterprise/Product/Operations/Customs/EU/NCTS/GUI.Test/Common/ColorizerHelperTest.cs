using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class ColorizerHelperTest : TestCase
	{
		[RequiresSTA]
		public void TestGetXMLVersion()
		{
			const string input = "<span style=\"color: #0000ff;\">&lt;</span><?xml version=\"1.0\" encoding=\"UTF-8\"?><span style=\"color: #0000ff;\">&lt;</span>";

			var expectedOutput = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

			AssertEquals("ColorizeHelper doesn't returns the correct XML version.", expectedOutput, ColorizerHelper.GetXMLVersionString(input));
		}

		[RequiresSTA]
		public void TestColorizeHtmlText_ShouldReturnEmptyString_WhenInputIsNull()
		{
			string input = null;
			var expectedOutput = string.Empty;

			var result = ColorizerHelper.ColorizeHtmlText(input);

			AssertEquals("ColorizerHelper doesn't returns null on null input.", expectedOutput, result);
		}

		[RequiresSTA]
		public void TestColorizeHtmlText_ShouldReturnInlineTags_WhenElementHasNoChildTags()
		{
			var input = GetEmbeddedResourceFile("colorizeXMLInput.html");
			var expectedOutput = GetEmbeddedResourceFile("colorizedXMLOutput.html");

			var result = ColorizerHelper.ColorizeHtmlText(input);

			AssertEquals("ColorizerHelper doesn't returns Inline Tags!!", expectedOutput, result);
		}

		[RequiresSTA]
		public void TestColorizeHtmlText_ShouldReturnEmptyString_WhenInputIsHTML()
		{
			var input = GetEmbeddedResourceFile("colorizeHtmlInput.html");

			var expectedOutput = GetEmbeddedResourceFile("colorizedHtmlOutput.html");

			var result = ColorizerHelper.ColorizeHtmlText(input);

			AssertEquals("ColorizerHelper doesn't returns correct expected colorizedHtmlOutput", expectedOutput, result);
		}

		public string GetEmbeddedResourceFile(string embeddedResourceFile)
		{
			var retriever = new EmbeddedResourceRetriever();
			return retriever.GetString("Enterprise.Customs.EU.NCTS.GUI.Testing.Common.TestFiles." + embeddedResourceFile);
		}
	}
}
