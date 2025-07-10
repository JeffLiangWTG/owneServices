using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

public abstract class EComBaseRequestPrettyFormatterTest : TestCaseWithFactory
{
	#region Message Response

	protected abstract ZString GetEM_MessageText();

	protected abstract ZString FormattedText();
	protected abstract bool IsTransmitMessage();
	ZString MessageIsEmpty => "<h2>Message is empty</h2>";

	#endregion

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => new EComRequestPrettyFormatter(null, (IEdecComplaintRequestDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(GetEM_MessageText()).MessageDetail, false).GetFormattedText());
	}

	public void TestEM_MessageIntepretation_Request()
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);
		RefCusCodeTestHelper.CreateEComplaintFieldNames(Factory);

		var expectedMessageInterpretation = FormattedText();

		var interpretation = new EComRequestPrettyFormatter(Factory, null, false).GetFormattedText();
		AssertEquals($"When ResponseDetail is empty, EM_MessageInterpretation should return '{MessageIsEmpty}'", MessageIsEmpty, interpretation);

		interpretation = new EComRequestPrettyFormatter(Factory, (IEdecComplaintRequestDetail)MessageSchemaDecider.GetSpecificMessageAnalyzer(GetEM_MessageText()).MessageDetail, IsTransmitMessage()).GetFormattedText();
		var expectedText = expectedMessageInterpretation.Replace(System.Environment.NewLine, string.Empty);
		AssertEquals("EM_MessageInterpretation", expectedText, interpretation);
	}
}
