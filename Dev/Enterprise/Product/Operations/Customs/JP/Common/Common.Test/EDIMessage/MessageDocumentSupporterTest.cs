using System.Text;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(MessageDocumentSupporter))]
sealed class MessageDocumentSupporterTest : DocumentSupporterTest
{
	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		return GetMessageDocumentSupportableBusinessObject("MockInboundMessage.txt");
	}

	public void TestGetBODocDataProviders()
	{
		AssertProvider("MockInboundMessage.txt", MessageDocumentSupporter.ExportPermitMessageDocument);
		AssertProvider("InspectionInfomationMessage.txt", MessageDocumentSupporter.InspectionNoticeMessageDocument);
		AssertProvider("SAS0711Message.txt", MessageDocumentSupporter.HBLCargoRegistrationInformationMessageDocument);
		AssertProvider("MismatchInformationMessage.txt", MessageDocumentSupporter.MismatchInformationDocument);
		AssertProvider("ImportClearancePermitTestMessage.txt", MessageDocumentSupporter.ImportPermitMessageDocument);
		AssertProvider("CancellationOfTransshipmentReport.txt", MessageDocumentSupporter.CancellationOfTransshipmentReport);
		AssertProvider("MoveInNoticeMessage.txt", MessageDocumentSupporter.MoveInNoticeMessageDocument);
		AssertProvider("SAS0731TestMessage.txt", MessageDocumentSupporter.HBLCargoCancellationInformationMessageDocument);
		AssertProvider("EACNoticeInfomationMessage.txt", MessageDocumentSupporter.EACNoticeInformationDocument);
		AssertProvider("TransshipmentNoticeSubmissionInformation.txt", MessageDocumentSupporter.TransshipmentNoticeSubmission);

		var message = GetDocumentSupportableBusinessObject();
		var providers = message.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".AA"), null);
		AssertNull(providers);
	}

	void AssertProvider(string fileName, string dataContextValue)
	{
		var documentSupportable = GetMessageDocumentSupportableBusinessObject(fileName);
		var providers = documentSupportable.DocumentSupporter.GetBODocDataProviders(new DataContextValue(dataContextValue), null);
		AssertEquals($"Failed for {fileName} with data context {dataContextValue}", 1, providers.Length);
	}

	public void TestDataContextSupported()
	{
		var message = (EDIMessage)GetDocumentSupportableBusinessObject();
		CombineAssertions(() =>
		{
			AssertEquals("InspectionNoticeMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.InspectionNoticeMessageDocument)));
			AssertEquals("ExportPermitMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.ExportPermitMessageDocument)));
			AssertEquals("MismatchInformationDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.MismatchInformationDocument)));
			AssertEquals("HBLCargoRegistrationInformationMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.HBLCargoRegistrationInformationMessageDocument)));
			AssertEquals("ImportPermitMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.ImportPermitMessageDocument)));
			AssertEquals("CancellationOfTransshipmentReportMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.CancellationOfTransshipmentReport)));
			AssertEquals("MoveInNoticeMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.MoveInNoticeMessageDocument)));
			AssertEquals("HBLCargoCancellationInformationMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.HBLCargoCancellationInformationMessageDocument)));
			AssertEquals("EACNoticeInformationDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.EACNoticeInformationDocument)));
			AssertEquals("TransshipmentNoticeSubmissionInformationMessageDocument", expected: true, message.DocumentSupporter.IsDataContextSupported(new DataContextValue(MessageDocumentSupporter.TransshipmentNoticeSubmission)));
		});
	}

	public void TestGetFilterValue()
	{
		var documentSupportable = GetMessageDocumentSupportableBusinessObject("InspectionInfomationMessage.txt");
		AssertEquals("Y", documentSupportable.DocumentSupporter.GetFilterValue(DocumentFilters.IsJPInspectionNoticeDocumentSupport));

		documentSupportable = GetMessageDocumentSupportableBusinessObject("ImportClearancePermitTestMessage.txt");
		AssertEquals("Y", documentSupportable.DocumentSupporter.GetFilterValue(DocumentFilters.IsJPImportPermitMessageDocumentSupport));

		documentSupportable = GetMessageDocumentSupportableBusinessObject("MismatchInformationMessage.txt");
		AssertEquals("Y", documentSupportable.DocumentSupporter.GetFilterValue(DocumentFilters.IsJPDiscrepancyNoticeDocumentSupport));
	}

	IDocumentSupportable GetMessageDocumentSupportableBusinessObject(string fileName)
	{
		var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
		ediMessage.EM_MessageData = Encoding.ASCII.GetBytes(TestDataHelper.GetResourceStream(fileName));
		return ediMessage;
	}
}
