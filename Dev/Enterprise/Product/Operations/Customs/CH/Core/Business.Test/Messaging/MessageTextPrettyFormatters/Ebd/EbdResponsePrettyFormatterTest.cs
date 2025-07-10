using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EbdResponsePrettyFormatterTest : TestCase
{
	public void TestAcceptance()
	{
		var responseDetail = Mock.Of<IDocumentImportResponseDetail>();
		Mock.Get(responseDetail).Setup(x => x.IsAcceptance).Returns(true);
		Mock.Get(responseDetail).Setup(x => x.IsRejection).Returns(false);

		var accompanyingDocument = Mock.Of<IAccompanyingDocument>();
		Mock.Get(accompanyingDocument).Setup(x => x.Filename).Returns("***TEST.pdf***");
		Mock.Get(responseDetail).Setup(x => x.AccompanyingDocuments).Returns(new List<IAccompanyingDocument>() { accompanyingDocument });

		var interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();

		AssertContains("title", "Document successfully uploaded", interpretedMessage);
		AssertContains("filename", "***TEST.pdf***", interpretedMessage);
	}

	public void TestRejection()
	{
		var responseDetail = Mock.Of<IDocumentImportResponseDetail>();
		Mock.Get(responseDetail).Setup(x => x.IsAcceptance).Returns(false);
		Mock.Get(responseDetail).Setup(x => x.IsRejection).Returns(true);

		var accompanyingDocument = Mock.Of<IAccompanyingDocument>();
		Mock.Get(accompanyingDocument).Setup(x => x.Filename).Returns("***TEST.pdf***");
		Mock.Get(accompanyingDocument).Setup(x => x.Informations).Returns(
			new[] { Mock.Of<IAccompanyingDocumentMessage>(m => m.Text == "***reason***") });
		Mock.Get(responseDetail).Setup(x => x.AccompanyingDocuments).Returns(new List<IAccompanyingDocument>() { accompanyingDocument });

		var interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();

		AssertContains("title", "Document upload failed", interpretedMessage);
		AssertContains("filename", "***TEST.pdf***", interpretedMessage);
		AssertContains("reason", "***reason***", interpretedMessage);
	}

	public void TestGetFormattedText_Language()
	{
		var responseDetail = Mock.Of<IDocumentImportResponseDetail>();

		var accompanyingDocument = Mock.Of<IAccompanyingDocument>();
		Mock.Get(accompanyingDocument).Setup(x => x.Filename).Returns("TEST.pdf");
		Mock.Get(accompanyingDocument).Setup(x => x.Informations).Returns(new[]
		{
				Mock.Of<IAccompanyingDocumentMessage>(m => m.Language == "de" && m.Text == "***de-text***"),
				Mock.Of<IAccompanyingDocumentMessage>(m => m.Language == "fr" && m.Text == "***fr-text***"),
				Mock.Of<IAccompanyingDocumentMessage>(m => m.Language == "it" && m.Text == "***it-text***"),
			});

		Mock.Get(responseDetail).Setup(x => x.AccompanyingDocuments).Returns(new List<IAccompanyingDocument>() { accompanyingDocument });
		Mock.Get(responseDetail).Setup(x => x.IsAcceptance).Returns(false);
		Mock.Get(responseDetail).Setup(x => x.IsRejection).Returns(true);

		CombineAssertions(() =>
		{
			var interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();
			Assert("default language", interpretedMessage.Contains("***de-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR-CH";
			interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();
			Assert("French", interpretedMessage.Contains("***fr-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "IT-CH";
			interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();
			Assert("Italian", interpretedMessage.Contains("***it-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "DE-CH";
			interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();
			Assert("German", interpretedMessage.Contains("***de-text***"));

			GlbStaff.CurrentUser.GS_WorkingLanguage = "SV-SV";
			interpretedMessage = new EbdResponsePrettyFormatter(responseDetail).GetFormattedText();
			Assert("other language", interpretedMessage.Contains("***de-text***"));
		});
	}
}
