using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business.Testing;
using CoreConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT182ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestInboundPrettyForPhase5Interpretation()
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory, CoreConstants.RefCusCodeListTypes.Codes.CHCustomsOffice);
		const string MRN = "21CH16360164625756";
		const string MRNVersion1 = "1";
		var sessionGuid = ZGuid.NewZGuid();
		 _ = Helper.CreateNctsHeaderAndSentMessage(mrn: $"{MRN}.{MRNVersion1}").nctsHeader;
		var receivedMessage = Helper.CreateReceivedMessage(sessionGuid);
		receivedMessage.EM_MessageText = MessageResponse;

		var html = new NT182ResponsePrettyFormatter(receivedMessage).GetFormattedText();

		AssertMultilineASCIIEquals("NT182 Formatted Message", expectedFormattedHtml, html);
	}

	ZString MessageResponse => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
    <Event>
        <EventTime>2022-11-08 07:08:36.976</EventTime>
        <EventType>IAK</EventType>
        <ContextCollection>
            <Context>
                <Type>ResponseMessage</Type>
                <Value><![CDATA[{TestingData.GetNT182(mrnVersion: "2")}]]></Value>
            </Context>
        </ContextCollection>
    </Event>
</UniversalEvent>";

	NctsMessageProcessorTestHelper Helper => helper ??= new NctsMessageProcessorTestHelper(Factory);
	NctsMessageProcessorTestHelper helper;
	readonly string expectedFormattedHtml = @"<h2>Events during the journey</h2><table><tr><td>MRN:</td><td>20DE16137020157570.2</td></tr><tr><td>Date:</td><td>2020-11-12T14:12:29</td></tr><tr><td>Customs Office Departure:</td><td>CH123456 </td></tr><tr><td>Customs Office Incident:</td><td>DE002104 </td></tr></table><h3>Incident</h3><table><tr><td>Number:</td><td>1</td></tr><tr><td>Code:</td><td>The carrier is obliged to deviate from the itinerary prescribed in accordance with Article 298 of UCC/IA Regulation due to circumstances beyond his control.</td></tr><tr><td>Text:</td><td>incident</td></tr></table><h4>Endorsement</h4><table><tr><td>Authority:</td><td>Police</td></tr><tr><td>Country:</td><td>DE</td></tr><tr><td>Date:</td><td>12-11-2020</td></tr><tr><td>Place:</td><td>City</td></tr></table><h4>Location</h4><table><tr><td>Qualifier:</td><td>A</td></tr><tr><td>UNLOCODE:</td><td>unlocode</td></tr><tr><td>GNNS Latitude:</td><td>&nbsp;</td></tr><tr><td>GNNS Longitude:</td><td>&nbsp;</td></tr></table><h4>Transhipment</h4><table><tr><td>Container:</td><td>Yes</td></tr><tr><td>Transport Means:</td><td>CH departureTransportMeansId 01</td></tr></table><h4>Transport Equipment</h4><table><tr><td>Number:</td><td>1</td></tr><tr><td>Container:</td><td>containerId_1</td></tr><tr><td>Number of Seals:</td><td>1</td></tr><tr><td>Seals:</td><td>sealId1</td></tr><tr><td>Goods References:</td><td>42</td></tr><tr><td>Number:</td><td>2</td></tr><tr><td>Container:</td><td>containerId_2</td></tr><tr><td>Number of Seals:</td><td>0</td></tr></table><h3>Incident</h3><table><tr><td>Number:</td><td>2</td></tr><tr><td>Code:</td><td>The carrier is obliged to deviate from the itinerary prescribed in accordance with Article 298 of UCC/IA Regulation due to circumstances beyond his control.</td></tr><tr><td>Text:</td><td>incident</td></tr></table><h4>Endorsement</h4><table><tr><td>Authority:</td><td>Police</td></tr><tr><td>Country:</td><td>DE</td></tr><tr><td>Date:</td><td>12-11-2020</td></tr><tr><td>Place:</td><td>City</td></tr></table><h4>Location</h4><table><tr><td>Qualifier:</td><td>A</td></tr><tr><td>UNLOCODE:</td><td>unlocode</td></tr><tr><td>GNNS Latitude:</td><td>&nbsp;</td></tr><tr><td>GNNS Longitude:</td><td>&nbsp;</td></tr></table><h4>Transhipment</h4><table><tr><td>Container:</td><td>Yes</td></tr><tr><td>Transport Means:</td><td>CH departureTransportMeansId 01</td></tr></table><h4>Transport Equipment</h4><table><tr><td>Number:</td><td>1</td></tr><tr><td>Container:</td><td>containerId_1</td></tr><tr><td>Number of Seals:</td><td>1</td></tr><tr><td>Seals:</td><td>sealId2</td></tr><tr><td>Goods References:</td><td>42</td></tr><tr><td>Number:</td><td>2</td></tr><tr><td>Container:</td><td>containerId_2</td></tr><tr><td>Number of Seals:</td><td>0</td></tr></table>";
}
