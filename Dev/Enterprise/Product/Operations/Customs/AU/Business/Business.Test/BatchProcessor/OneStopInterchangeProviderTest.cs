using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OneStopInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			string messageString = @"UNH+54+IFTERA:D:98B:RT:ENET54'
		BGM+ERA+EDISRW+9+AQ'
		DTM+137:20040715165046:204'
		NAD+MS+EDISRW'
		CTA+IC+:POSE'
		COM+1-stoppra@edi.net.au:EM'
		RFF+BN:1221412'
		RFF+ERN:10407'
		TDT+10++2+++++123AS'
		LOC+7++CONFI'
		TDT+20+350+1++HUA+++7725233:::HARUNA MARU'
		LOC+9+AUBNE+CONFI'
		LOC+11+THBKK'
		LOC+7+THBKK'
		EQD+CN+POCU2819798+40G0++2+5'
		HAN+:::GENL'
		NAD+CZ+CLARENCE RIVER FISHERMENS CO-OP LT'
		MEA+AAE+G+KGM:15326'
		SEL+B0209873+AB+1'
		FTX+ZO1+++FROZEN WHOLE SCHOOL WHITIN'
		RFF+AAE:3B003281111XHC'
		DGS+2.1+1950'
		FTX+AAD+++AEROSOLS (ABOVE 1L)'
		CTA+HG+:SCOTT WRIGHT'
		COM+90251132:TE'
		MEA+AAE+AAL+KGM:2345'
		EQA+RR+34'
		UNT+28+54'";
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_MessageText = messageString;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_IsTestMessage = true;
			message.EM_MessageSubType = "SSM";
			message.EM_MessageOwner = "";

			EDIMessage message2 = messages.AddNew();
			message2.EM_MessageText = messageString;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_IsTestMessage = true;
			message2.EM_MessageSubType = "SSM";
			message2.EM_MessageOwner = "";

			OneStopInterchangeProviderTestProxy provider = new OneStopInterchangeProviderTestProxy(messages);

			EDIInterchangeCollection interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges", 2, interchanges.Count);
			ZString dateString = provider.PreparedTime.ToString("yyMMdd");
			ZString timeString = provider.GetTimeString(provider.PreparedTime.ToDateTime());

			EDIInterchange interchange = interchanges[0];

			AssertEquals("Header", "UNA:+.? 'UNB+UNOC:3+EDIAL+1STOP+" + dateString + ":" + timeString + "+" + EDIInterchange.InterchangeNumberPlaceHolder + "++++++1'", interchange.EI_HeaderText);
			AssertEquals("Footer", "UNZ+1+" + EDIInterchange.InterchangeNumberPlaceHolder + "'", interchange.EI_FooterText);

			AssertEquals("1STOPTEST", interchange.EI_To);
			AssertEquals("EDIAL", interchange.EI_From);
			AssertEquals(messageString, interchange.EI_BodyText);
		}

		public void TestPackageOneStopInterchangeCSX()
		{
			string messageString = @"UNH+54+IFTERA:D:98B:RT:ENET54'
		BGM+ERA+EDISRW+9+AQ'
		DTM+137:20040715165046:204'
		NAD+MS+EDISRW'
		CTA+IC+:POSE'
		COM+1-stoppra@edi.net.au:EM'
		RFF+BN:1221412'
		RFF+ERN:10407'
		TDT+10++2+++++123AS'
		LOC+7++CXSXAD'
		TDT+20+350+1++HUA+++7725233:::HARUNA MARU'
		LOC+9+AUBNE+CONFI'
		LOC+11+THBKK'
		LOC+7+THBKK'
		EQD+CN+POCU2819798+40G0++2+5'
		HAN+:::GENL'
		NAD+CZ+CLARENCE RIVER FISHERMENS CO-OP LT'
		MEA+AAE+G+KGM:15326'
		SEL+B0209873+AB+1'
		FTX+ZO1+++FROZEN WHOLE SCHOOL WHITIN'
		RFF+AAE:3B003281111XHC'
		DGS+2.1+1950'
		FTX+AAD+++AEROSOLS (ABOVE 1L)'
		CTA+HG+:SCOTT WRIGHT'
		COM+90251132:TE'
		MEA+AAE+AAL+KGM:2345'
		EQA+RR+34'
		UNT+28+54'";

			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = messages.AddNew();
			message.EM_MessageText = messageString;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message.EM_IsTestMessage = true;
			message.EM_MessageSubType = "XSM";

			OneStopInterchangeProviderTestProxy provider = new OneStopInterchangeProviderTestProxy(messages);

			EDIInterchangeCollection interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("NumberOfInterchanges", 1, interchanges.Count);
			ZString dateString = provider.PreparedTime.ToString("yyMMdd");
			ZString timeString = provider.GetTimeString(provider.PreparedTime.ToDateTime());

			EDIInterchange interchange = interchanges[0];

			AssertEquals("Header", "UNA:+.? 'UNB+UNOC:3+EDIALT+CSXWTADL+" + dateString + ":" + timeString + "+" + EDIInterchange.InterchangeNumberPlaceHolder + "++++++1'", interchange.EI_HeaderText);
			AssertEquals("Footer", "UNZ+1+" + EDIInterchange.InterchangeNumberPlaceHolder + "'", interchange.EI_FooterText);

			AssertEquals("CSXTEST", interchange.EI_To);
			AssertEquals("EDIALT", interchange.EI_From);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new OneStopInterchangeProvider(collection);

		sealed class OneStopInterchangeProviderTestProxy : OneStopInterchangeProvider
		{
			public OneStopInterchangeProviderTestProxy(NonDependentEDIMessageCollection messages) : base(messages)
			{
			}

			public new ZDateTimeOffset PreparedTime => base.PreparedTime;
		}
	}
}
