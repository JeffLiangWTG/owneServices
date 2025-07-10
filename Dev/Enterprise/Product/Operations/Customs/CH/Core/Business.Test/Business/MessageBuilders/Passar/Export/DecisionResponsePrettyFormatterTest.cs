using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class DecisionResponsePrettyFormatterTest<TDataProvider> : TestCaseWithFactory
															where TDataProvider : class, IDecision
{
	protected abstract IMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, TDataProvider dataProvider);

	protected virtual string DecisionTitle { get; }

	protected virtual void ConfigureSpecificFormatterData(Mock<TDataProvider> responseDetailMock) { }

	protected virtual string SpecificFormatterData { get; }

	public void TestGetFormattedText_Accepetd() => AssertGetFormattedText("A1", "desc-A1", "NTXXX Message", "NTXXX Pointer", true, false, false);

	public void TestGetFormattedText_Rejected() => AssertGetFormattedText("A1", "desc-A1", "NTXXX Message", "NTXXX Pointer", false, true, false);

	public void TestGetFormattedText_Received() => AssertGetFormattedText("A1", "desc-A1", "NTXXX Message", "NTXXX Pointer", false, false, true);

	void AssertGetFormattedText(string code, string codeDescription, string message, string pointer, bool isAccepetd, bool isRejected, bool isReceived)
	{
		var responseDetailMock = SetUpMock(code, codeDescription, message, pointer, isAccepetd, isRejected, isReceived);
		var formatter = GetMessagePrettyFormatter(Factory, responseDetailMock.Object);
		var html = formatter.GetFormattedText();

		AssertMultilineASCIIEquals("Blank description", GetExpectedFormattedText(DecisionTitle, SpecificFormatterData, GetDecisionText(isAccepetd, isRejected, isReceived), code, codeDescription, message, pointer), html);
	}

	public void TestGetRejectedDecisionFormattedText_HtmlEncoded()
	{
		var responseDetailMock = SetUpMock("code-&", "desc-&", "message-&", "pointer-&", false, true, false);
		var formatter = GetMessagePrettyFormatter(Factory, responseDetailMock.Object);
		var html = formatter.GetFormattedText();

		AssertMultilineASCIIEquals("Blank description", GetExpectedFormattedText(DecisionTitle, SpecificFormatterData, GetDecisionText(false, true, false), "code-&amp;", "desc-&amp;", "message-&amp;", "pointer-&amp;"), html);
	}

	Mock<TDataProvider> SetUpMock(string code, string codeDescription, string message, string pointer, bool isAccepetd, bool isRejected, bool isReceived)
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(UniversalReferenceConstants.RefCusCodeList.PassarTypes.N3000).CreateCode(code).WithDescription(codeDescription);
		Factory.Save();

		var decisionReasonMock = new Mock<IDecisionReason>();
		decisionReasonMock.Setup(e => e.SequenceNumber).Returns(1);
		decisionReasonMock.Setup(e => e.Code).Returns(code);
		decisionReasonMock.Setup(e => e.Message).Returns(message);
		decisionReasonMock.Setup(e => e.Pointer).Returns(pointer);
		var responseDetailMock = new Mock<TDataProvider>();
		responseDetailMock.Setup(r => r.DecisionReasons).Returns(new[] { decisionReasonMock.Object });
		responseDetailMock.Setup(r => r.IsAccepted).Returns(isAccepetd);
		responseDetailMock.Setup(r => r.IsRejected).Returns(isRejected);
		responseDetailMock.Setup(r => r.IsReceived).Returns(isReceived);
		ConfigureSpecificFormatterData(responseDetailMock);
		return responseDetailMock;
	}

	string GetDecisionText(bool isAccepetd, bool isRejected, bool isReceived) => isAccepetd ? "ACCEPTED" : isRejected ? "REJECTED" : "RECEIVED";

	string GetExpectedFormattedText(string title, string specificFormatterData, string decision, string code, string description, string message, string pointer)
	{
		return $"<h2>{title}</h2>{specificFormatterData}<h3>Decision: {decision}</h3><h3>Reasons:</h3><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">"
			+ "<thead><tr class=\"tableheadings\"><th>Code</th><th>Description</th><th>Message</th><th>Pointer</th></tr></thead>"
			+ $"<tr><td>{code}</td><td>{description}</td><td>{message}</td><td>{pointer}</td></tr></table>";
	}
}
