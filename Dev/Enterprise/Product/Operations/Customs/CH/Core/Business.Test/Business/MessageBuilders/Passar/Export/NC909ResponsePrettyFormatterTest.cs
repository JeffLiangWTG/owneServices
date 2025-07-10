using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class NC909ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INC909ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NC909ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NC909ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedTextLineNumber() => CombineAssertions(() =>
	{
		var expectedText = GetExpectedFormattedText("Process", "52", "desc-52", "Element expected", "Line No.", "1234");
		var actualText = GetFormattedText("Process", "52", "desc-52", "Element expected", "1234", null);

		AssertMultilineASCIIEquals("Result including Line Number", expectedText, actualText);
	});

	public void TestGetFormattedTextLineNumber_HtmlEncoded() => CombineAssertions(() =>
	{
		var expectedText = GetExpectedFormattedText("type-&amp;", "code-&amp;", "desc-&amp;", "message-&amp;", "Line No.", "line-&amp;");
		var actualText = GetFormattedText("type-&", "code-&", "desc-&", "message-&", "line-&", null);

		AssertMultilineASCIIEquals("Result including Line Number HTML encoded", expectedText, actualText);
	});

	public void TestGetFormattedTextPointer()
	{
		var expectedText = GetExpectedFormattedText("Process", "52", "desc-52", "Element expected", "Pointer", "1234");
		var actualText = GetFormattedText("Process", "52", "desc-52", "Element expected", null, "1234");

		AssertMultilineASCIIEquals("Result including Line Number", expectedText, actualText);
	}

	public void TestGetFormattedTextPointer_HtmlEncoded()
	{
		var expectedText = GetExpectedFormattedText("type-&amp;", "code-&amp;", "desc-&amp;", "message-&amp;", "Pointer", "line-&amp;");
		var actualText = GetFormattedText("type-&", "code-&", "desc-&", "message-&", null, "line-&");

		AssertMultilineASCIIEquals("Result including Line Number HTML encoded", expectedText, actualText);
	}

	public void TestGetFormattedText_UnkownErrorCode()
	{
		var expectedText = GetExpectedFormattedText("&nbsp;", "99", "&nbsp;", "&nbsp;", "Line No.", "&nbsp;");
		var actualText = GetFormattedText(null, "99", null, null, null, null);

		AssertMultilineASCIIEquals("Blank description", expectedText, actualText);
	}

	string GetFormattedText(string errorType, string errorCode, string errorDescription, string errorMessage, string errorLineNumber, string errorPointer)
	{
		if (!string.IsNullOrEmpty(errorDescription))
		{
			var testHelper = new RefDataTestHelper(Factory);
			testHelper.CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N2000).CreateCode(errorCode).WithDescription(errorDescription);
			Factory.Save();
		}

		var errorMock = new Mock<INC909Error>();
		errorMock.Setup(e => e.ErrorType).Returns(errorType);
		errorMock.Setup(e => e.ErrorCode).Returns(errorCode);
		errorMock.Setup(e => e.ErrorMessage).Returns(errorMessage);
		errorMock.Setup(e => e.ErrorLineNumber).Returns(errorLineNumber);
		errorMock.Setup(e => e.ErrorPointer).Returns(errorPointer);
		var responseDetailMock = new Mock<INC909ResponseDetail>();
		responseDetailMock.Setup(r => r.Errors).Returns(new[] { errorMock.Object });

		return new NC909ResponsePrettyFormatter(Factory, responseDetailMock.Object).GetFormattedText();
	}

	string GetExpectedFormattedText(string type, string code, string description, string message, string lineNumberOrPointerHeader, string lineNumberOrPointer)
	{
		return "<h2>The sent message is invalid</h2><h3>Errors:</h3><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">"
			+ $"<thead><tr class=\"tableheadings\"><th>Type</th><th>Code</th><th>Description</th><th>Message</th><th>{lineNumberOrPointerHeader}</th></tr></thead>"
			+ $"<tr><td>{type}</td><td>{code}</td><td>{description}</td><td>{message}</td><td>{lineNumberOrPointer}</td></tr></table>";
	}
}
