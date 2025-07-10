using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class RuleErrorPrettyFormatterTest : TestCase
{
	public void TestGetFormattedText()
	{
		var responseDetail = Mock.Of<IRuleErrorResponseDetail>();

		var ruleError1 = Mock.Of<IRuleError>();
		Mock.Get(ruleError1).Setup(x => x.RuleName).Returns("R001");
		Mock.Get(ruleError1).Setup(x => x.Reference).Returns("");
		Mock.Get(ruleError1).Setup(x => x.Descriptions).Returns(new[]
		{
				Mock.Of<IRuleErrorDescription>(m => m.Language == "de" && m.Text == "de-text-1"),
				Mock.Of<IRuleErrorDescription>(m => m.Language == "fr" && m.Text == "fr-text-1"),
				Mock.Of<IRuleErrorDescription>(m => m.Language == "it" && m.Text == "it-text-1"),
			});

		var ruleError2 = Mock.Of<IRuleError>();
		Mock.Get(ruleError2).Setup(x => x.RuleName).Returns("R002");
		Mock.Get(ruleError2).Setup(x => x.Reference).Returns("traderItemID:1");
		Mock.Get(ruleError2).Setup(x => x.Descriptions).Returns(new[]
		{
				Mock.Of<IRuleErrorDescription>(m => m.Language == "de" && m.Text == "de-text-2"),
				Mock.Of<IRuleErrorDescription>(m => m.Language == "fr" && m.Text == "fr-text-2"),
				Mock.Of<IRuleErrorDescription>(m => m.Language == "it" && m.Text == "it-text-2"),
			});

		Mock.Get(responseDetail).Setup(x => x.RuleErrors).Returns(new List<IRuleError>() { ruleError1, ruleError2 });

		var interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
		var expectedMessage = UnindentHtml(@"
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<thead>
		<tr class=""tableheadings"">
			<th>Error code</th>
			<th>Error description</th>
			<th>Entry line #</th>
		</tr>
	</thead>
	<tr>
		<td>R001</td>
		<td>de-text-1</td>
		<td>&nbsp;</td>
	</tr>
	<tr>
		<td>R002</td>
		<td>de-text-2</td>
		<td>1</td>
	</tr>
</table>");
		HtmlAssertEquals("Formatted text", expectedMessage, interpretedMessage, isHtmlMessage: true);
	}

	public void TestGetFormattedText_Language()
	{
		var responseDetail = Mock.Of<IRuleErrorResponseDetail>();

		var ruleError = Mock.Of<IRuleError>();
		Mock.Get(ruleError).Setup(x => x.RuleName).Returns("R001");
		Mock.Get(ruleError).Setup(x => x.Reference).Returns("");
		Mock.Get(ruleError).Setup(x => x.Descriptions).Returns(new[]
		{
				Mock.Of<IRuleErrorDescription>(m => m.Language == "de" && m.Text == "***de-text***"),
				Mock.Of<IRuleErrorDescription>(m => m.Language == "fr" && m.Text == "***fr-text***"),
				Mock.Of<IRuleErrorDescription>(m => m.Language == "it" && m.Text == "***it-text***"),
			});

		Mock.Get(responseDetail).Setup(x => x.RuleErrors).Returns(new List<IRuleError>() { ruleError });

		CombineAssertions(() =>
		{
			var interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
			Assert("default language", interpretedMessage.Contains("***de-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR-CH";
			interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
			Assert("French", interpretedMessage.Contains("***fr-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-CH";
			interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
			Assert("Italian", interpretedMessage.Contains("***it-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "DE-CH";
			interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
			Assert("German", interpretedMessage.Contains("***de-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "SV-SV";
			interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
			Assert("other language", interpretedMessage.Contains("***de-text***"));
		});
	}

	public void TestGetFormattedText_HtmlEntities()
	{
		var responseDetail = Mock.Of<IRuleErrorResponseDetail>();

		var ruleError = Mock.Of<IRuleError>();
		Mock.Get(ruleError).Setup(x => x.RuleName).Returns("R001");
		Mock.Get(ruleError).Setup(x => x.Reference).Returns("");
		Mock.Get(ruleError).Setup(x => x.Descriptions).Returns(new[]
		{
				Mock.Of<IRuleErrorDescription>(m => m.Language == "de" && m.Text == "XXX<>XXX"),
			});

		Mock.Get(responseDetail).Setup(x => x.RuleErrors).Returns(new List<IRuleError>() { ruleError });

		CombineAssertions(() =>
		{
			var interpretedMessage = new RuleErrorPrettyFormatter(responseDetail).GetFormattedText();
			Assert(interpretedMessage.Contains("XXX&lt;&gt;XXX"));
		});
	}

	static string UnindentHtml(string html)
	{
		return Regex.Replace(html, @"\s*(?=\<)|(?<=\>)\s*", "");
	}
}
