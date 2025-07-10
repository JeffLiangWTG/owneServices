using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IvistoResponseEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestPositiveContentPrettyPrint()
	{
		const string expected = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<ie:CC599C xmlns:ie=""http://ecs.dgtaxud.ec"">
  <messageSender>NECA.IT</messageSender>
  <messageRecipient>accoglienza</messageRecipient>
  <preparationDateAndTime>2023-04-28T12:12:06</preparationDateAndTime>
  <messageIdentification>202304281212060</messageIdentification>
  <messageType>CC599C</messageType>
  <ExportOperation>
    <MRN>23ITQ0B01AA01858A3</MRN>
    <transit>0</transit>
  </ExportOperation>
  <CustomsOfficeOfExport>
    <referenceNumber>IT279100</referenceNumber>
  </CustomsOfficeOfExport>
  <CustomsOfficeOfExitActual>
    <referenceNumber>IT279100</referenceNumber>
  </CustomsOfficeOfExitActual>
  <ExitControlResult>
    <code>A2</code>
    <exitDate>2023-04-28</exitDate>
    <stateOfSeals>0</stateOfSeals>
  </ExitControlResult>
</ie:CC599C>";

		message.EM_MessageText = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");
		AssertEquals("Formatted Text", expected, message.EM_MessageInterpretation);
	}

	public void TestNegativeResponseContentPrettyPrint()
	{
		var fullSoapContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoNegativeResponse.xml");
		message.EM_MessageText = fullSoapContent;
		AssertContains("Formatted Text", fullSoapContent, message.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		message = Factory.New<ITEDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.IvistoResponse;
	}

	ITEDIMessage message;
}
