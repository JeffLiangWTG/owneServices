using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	[TestedType(typeof(ICSGBCustomsBusinessResponse))]
	class ICSGBCustomsBusinessResponseTest : TestCaseWithFactory
	{
		public void TestNotificationResponse()
		{
			var response = new ICSGBCustomsBusinessResponse(NotificationResponseInterchangeBody);
			AssertStartsWith("Message should be a CC351A XML", "<cc3:CC351A xmlns:cc3=\"http://ics.dgtaxud.ec/CC351A\">", response.ResponseBodyXml);
			AssertEquals("ICSGB", response.Provider);
			AssertEquals("Am95Tr4n9aYAsi", response.CorrelationId);
			AssertEquals("351", response.GetMessageSubType(new LoggingInformation()));
		}

		public void TestOutcomeResponse()
		{
			var response = new ICSGBCustomsBusinessResponse(OutcomeResponseInterchangeBody);
			AssertStartsWith("Message should be a CC328A XML", "<cc3:CC328A xmlns:cc3=\"http://ics.dgtaxud.ec/CC328A\">", response.ResponseBodyXml);
			AssertEquals("ICSGB", response.Provider);
			AssertEquals("dwbFjvcb42T2Tt", response.CorrelationId);
			AssertEquals("328", response.GetMessageSubType(new LoggingInformation()));
		}

		public static string NotificationResponseInterchangeBody => @"
<GBCustomsBusinessResponse>
  <ResponseHeader Provider=""ICSGB"">
    <eHubTrackingID>d9c4947d-d948-4c86-943b-a1a668cdbc3e</eHubTrackingID>
    <CorrelationID>Am95Tr4n9aYAsi</CorrelationID>
  </ResponseHeader>
  <ResponseBody ContentType=""XML"" Encoding=""none"">
    <notificationResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC351A"">
      <response>
        <cc3:CC351A>
          <MesSenMES3>GB945390992000/0000000000</MesSenMES3>
          <MesRecMES6>GB945390992000/0000000000</MesRecMES6>
          <DatOfPreMES9>250106</DatOfPreMES9>
          <TimOfPreMES10>1641</TimOfPreMES10>
          <MesIdeMES19>55</MesIdeMES19>
          <MesTypMES20>CC351A</MesTypMES20>
          <CorIdeMES25>Am95Tr4n9aYAsi</CorIdeMES25>
          <HEAHEA>
            <RefNumHEA4>ABCDEFGHIJKLMNOPQRST</RefNumHEA4>
            <DocNumHEA5>12AB3C4D5E6F7G8H90</DocNumHEA5>
            <TotNumOfIteHEA305>1</TotNumOfIteHEA305>
            <NotDatTimHEA104>202501070916</NotDatTimHEA104>
            <DecRegDatTimHEA115>202501070916</DecRegDatTimHEA115>
            <DecSubDatTimHEA118>202501070916</DecSubDatTimHEA118>
          </HEAHEA>
          <TRAREP>
            <NamTRE1>A. PERSON</NamTRE1>
            <StrAndNumTRE1>STREET</StrAndNumTRE1>
            <PosCodTRE1>POST CODE</PosCodTRE1>
            <CitTRE1>CITY</CitTRE1>
            <CouCodTRE1>FR</CouCodTRE1>
            <TRAREPLNG>FR</TRAREPLNG>
            <TINTRE1>FR00123456789</TINTRE1>
          </TRAREP>
          <PERLODSUMDEC>
            <TINPLD1>GB00123456789</TINPLD1>
          </PERLODSUMDEC>
          <CUSOFFFENT730>
            <RefNumCUSOFFFENT731>ABCDEFGH</RefNumCUSOFFFENT731>
            <ExpDatOfArrFIRENT733>202501070916</ExpDatOfArrFIRENT733>
          </CUSOFFFENT730>
          <TRACARENT601>
            <TINTRACARENT602>GB00123456789</TINTRACARENT602>
          </TRACARENT601>
          <CUSINT632>
            <IteNumConCUSINT668>1</IteNumConCUSINT668>
            <CusIntCodCUSINT665>A001</CusIntCodCUSINT665>
            <CusIntTexCUSINT666>GOODS NOT TO BE LOADED</CusIntTexCUSINT666>
            <CusIntTexCUSINT667LNG>EN</CusIntTexCUSINT667LNG>
          </CUSINT632>
        </cc3:CC351A>
      </response>
      <acknowledgement method=""DELETE"" href=""/customs/imports/notifications/65cb5b8b-6262-4f0a-a462-ff5191b42244"" />
    </notificationResponse>
  </ResponseBody>
</GBCustomsBusinessResponse>";

		public static string OutcomeResponseInterchangeBody => @"
<GBCustomsBusinessResponse>
  <ResponseHeader Provider=""ICSGB"">
    <eHubTrackingID>7e2f5e6b-27b3-451f-9205-c54f0ce46d12</eHubTrackingID>
    <CorrelationID>dwbFjvcb42T2Tt</CorrelationID>
  </ResponseHeader>
  <ResponseBody ContentType=""XML"" Encoding=""none"">
    <outcomeResponse xmlns:cc3=""http://ics.dgtaxud.ec/CC328A"">
      <response>
        <cc3:CC328A>
          <MesSenMES3>GB945390992000/0000000000</MesSenMES3>
          <MesRecMES6>GB945390992000/0000000000</MesRecMES6>
          <DatOfPreMES9>250106</DatOfPreMES9>
          <TimOfPreMES10>1641</TimOfPreMES10>
          <MesIdeMES19>55</MesIdeMES19>
          <MesTypMES20>CC328A</MesTypMES20>
          <CorIdeMES25>dwbFjvcb42T2Tt</CorIdeMES25>
          <HEAHEA>
            <RefNumHEA4>MAN0000558djc3</RefNumHEA4>
            <DocNumHEA5>25GBWZXX4WT3NQQC20</DocNumHEA5>
            <TraModAtBorHEA76>3</TraModAtBorHEA76>
            <IdeOfMeaOfTraCroHEA85>MT10VHC</IdeOfMeaOfTraCroHEA85>
            <ComRefNumHEA>MAN0000558</ComRefNumHEA>
            <DecRegDatTimHEA115>202501070915</DecRegDatTimHEA115>
          </HEAHEA>
          <GOOITEGDS>
            <IteNumGDS7>1</IteNumGDS7>
            <ComRefNumGIM1>B1</ComRefNumGIM1>
            <CONNR2>
              <ConNumNR21>DANU123465785</ConNumNR21>
            </CONNR2>
          </GOOITEGDS>
          <GOOITEGDS>
            <IteNumGDS7>2</IteNumGDS7>
            <ComRefNumGIM1>B2</ComRefNumGIM1>
            <CONNR2>
              <ConNumNR21>DANU123465785</ConNumNR21>
            </CONNR2>
          </GOOITEGDS>
          <PERLODSUMDEC>
            <TINPLD1>GB945390992000</TINPLD1>
          </PERLODSUMDEC>
          <CUSOFFFENT730>
            <RefNumCUSOFFFENT731>XI000011</RefNumCUSOFFFENT731>
            <ExpDatOfArrFIRENT733>202501061641</ExpDatOfArrFIRENT733>
          </CUSOFFFENT730>
        </cc3:CC328A>
      </response>
      <acknowledgement method=""DELETE"" href=""/customs/imports/outcomes/dwbFjvcb42T2Tt"" />
    </outcomeResponse>
  </ResponseBody>
</GBCustomsBusinessResponse>";
	}
}
