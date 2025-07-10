using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business.Testing;
using CoreConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT019ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestInboundPrettyForPhase5Interpretation()
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory, CoreConstants.RefCusCodeListTypes.Codes.CHCustomsOffice);
		var sessionGuid = ZGuid.NewZGuid();
		var receivedMessage = Helper.CreateReceivedMessage(sessionGuid);
		receivedMessage.EM_MessageText = MessageResponse;

		var html = new NT019ResponsePrettyFormatter(receivedMessage).GetFormattedText();

		AssertMultilineASCIIEquals("NT019 Formatted Message", expectedFormattedHtml, html);
	}

	ZString MessageResponse => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
    <Event>
        <EventTime>2022-11-08 07:08:36.976</EventTime>
        <EventType>IAK</EventType>
        <ContextCollection>
            <Context>
                <Type>ResponseMessage</Type>
                <Value><![CDATA[{TestingData.GetNT019(mrn: "21CH16360164625756")}]]></Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";

	NctsMessageProcessorTestHelper Helper => helper ??= new NctsMessageProcessorTestHelper(Factory);
	NctsMessageProcessorTestHelper helper;
	readonly string expectedFormattedHtml = @"<h2>Discrepancies at Destination</h2><table><tr><td>MRN:</td><td>21CH16360164625756.1</td></tr><tr><td>Customs Office of Departure:</td><td>CH001471 </td></tr></table><h3>Notification</h3><table><tr><td>Notification Date:</td><td>19-11-2021</td></tr></table><h4>Guarantor</h4><table></table><h4>Holder Of The Transit Procedure</h4><table></table>";
}
