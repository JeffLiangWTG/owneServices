using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_MessageText = MessageString;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message.EM_IsTestMessage = true;
			message.EM_MessageType = "LDG";
			message.EM_MessageOwner = "";

			EDIMessage message2 = messages.AddNew();
			message2.EM_MessageText = MessageString;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.EXDOC;
			message2.EM_IsTestMessage = true;
			message2.EM_MessageSubType = "LDG";
			message2.EM_MessageOwner = "";

			EXDOCInterchangeProviderTestProxy provider = new EXDOCInterchangeProviderTestProxy(messages);

			EDIInterchangeCollection interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges", 2, interchanges.Count);
			ZString dateString = provider.PreparedTime.ToString("yyMMdd");
			ZString timeString = provider.GetTimeString(provider.PreparedTime.ToDateTime());

			EXDOCInterchange interchange = (EXDOCInterchange)interchanges[0];

			AssertEquals("Header", "UNB+UNOB:2+8+7+" + dateString + ":" + timeString + "+" + EDIInterchange.InterchangeNumberPlaceHolder + "++" + EXDOCInterchangeProvider.ApplicationReference.exdoc + "+++EDI+1'", interchange.EI_HeaderText
				.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }), "'")
				.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }), "+")
				.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }), ":")
				);
			AssertEquals("Footer", "UNZ+1+" + EDIInterchange.InterchangeNumberPlaceHolder + "'", interchange.EI_FooterText
				.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }), "'")
				.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }), "+")
				.Replace(System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }), ":")
				);

			AssertEquals(EDIInterchange.InterchangePartyIDs.EXDOCReceiversMailbox, interchange.EI_To);
			AssertEquals(EDIInterchange.InterchangePartyIDs.EXDOCSendersMailbox, interchange.EI_From);
			AssertEquals(MessageString, interchange.EI_BodyText);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new EXDOCInterchangeProvider(collection);

		const string MessageString = @"UNH+100+SANCRT:D:97B:UN:RF0801'BGM+M::AQ:9++13'LOC+9+BNE'LOC+12+TWTPE'LOC+8+TAIPEI'LOC+36+TW'LOC+30+AU'LOC+91+SYD'RFF+ABE:B00000123'RFF+AAE:AAEETF6AL'MEA+TE+ADE+CEL:-2.75'GIS+A::AQ:PHC'GIS+Y::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:SST'GIS+N::AQ:QI'GIS+Y::AQ:ACS'PNA+EX+1000'PNA+CN+++++10:KWING KWONG'ADR++5:456 HIGH ST+TAIPEI+654321+TW+:::TWSTATE'TDT+12+V123+1++:::ADMIRALENGRACHT+++:::8811924'DTM+136:20040130:102'PRC+IN:PP:AQ'PNA+FO+10004'PNA+AV+GRAHB'LIN+1'MEA+AAA+SQ+KGM:1456.000'PIA+5+XCA BP:CC'PIA+5+1000:BP'PIA+5+99999998:HS'IMD+++IN:::*S-RMP* WHOLE RUMP 12-14MM'IMD+++UHC:::BEEF'MOA+63:425.00'PAC+50+3+CT::AQ'PCI++NM/A1234/ENDV'EQD+CN+MARU2103333'SEL+654321'PRC+SL:PP:AQ'DTM+194:20040101:102'DTM+206:20040105:102'PNA+MP+780'PRC+PK:PP:AQ'DTM+194:20040108:102'DTM+206:20040108:102'PNA+MP+1004'UNT+47+100'";

		sealed class EXDOCInterchangeProviderTestProxy : EXDOCInterchangeProvider
		{
			public EXDOCInterchangeProviderTestProxy(NonDependentEDIMessageCollection messages) : base(messages)
			{
			}

			public new ZDateTimeOffset PreparedTime => base.PreparedTime;
		}
	}
}
