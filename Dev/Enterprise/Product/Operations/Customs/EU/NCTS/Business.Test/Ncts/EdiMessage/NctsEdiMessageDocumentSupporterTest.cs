using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEdiMessageDocumentSupporter))]
	public class NctsEdiMessageDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			var message = Factory.New<NctsEdiMessage>();
			message.EM_LinkedObject = header;
			return message;
		}

		protected override void DoSetupForDocument(Integration.DocumentEngine.IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			SetupEdiMessageForTest(documentSupportableBO);
			base.DoSetupForDocument(command, documentSupportableBO);
		}

		void SetupEdiMessageForTest(IDocumentSupportable documentSupportableBO)
		{
			var ediMessage = documentSupportableBO as NctsEdiMessage;
			ediMessage.EM_MessageText = "UNH+40340610115947+CUSDEC:D:96B:UN:GB029B+999'BGM+:::T1+14GB000060100C6C48'LOC+36+IT'LOC+35+GB'LOC+9+DOVER'LOC+118+GB000060'LOC+14++954131533-GB60DEP'LOC+58+:::MODENA'LOC+91+:::DOVER'LOC+50+AA111111+1100'LOC+50+BB222222+1201'LOC+50+CC333333+1302'LOC+50+DD444444+1403'LOC+50+EE555555+1504'LOC+50+FF666666+1605'LOC+45+IT021300'LOC+168+GB000001:::Central Community Transit Office+GB:::HMRC, Main Road,+CO12 3PG:::Harwich, Essex. FAX 00441255 244784'DTM+137:20140609'DTM+148:20140610'DTM+182:20140611'DTM+268:20140617'DTM+9:20140612'GIS+0:109'GIS+0:62'GIS+1:187'FII+XX++:::::::WI00062610'MEA+WT+AAD+KGM:1000'EQD+CH+:::GB'EQD+CH+:::BE'EQD+CH+:::LU'EQD+CH+:::DE'EQD+CH+:::AT'EQD+CH+:::IT'SEL+0+:::NCTS001'SEL+0+:::NCTS002'FTX+ALL++++EN'FTX+ACB++++EN'FTX+ABL+++0'FTX+ADO++A3'RFF+AMJ:C'RFF+ABE:NCT0000017'RFF+LAN:GB-AUTH-42'RFF+ABL:1'PAC+1+++AC01:09GB00000100000M0'PAC+2+++AC02:09GB00000100000M2'PCI+19+0'TDT+12+++++++:::NC15 REG:GB'NAD+AF+GB954131533000++PRINCIPAL+11TH FLOOR, ALEX HOUSE, VICTORIA AV+SOUTHEND-ON-SEA, ESSEX++SS99 1AA+GB'NAD+CZ+GB954131533111++CONSIGNOR+11TH FLOOR, ALEX HOUSE, VICTORIA AV+SOUTHEND-ON-SEA, ESSEX++SS99 1AA+GB'NAD+CN+IT11ITALIANC11++CONSIGNEE+ITALIAN OFFICE+MILAN++IT99 1IT+IT'NAD+GL+GB954131533000++S.CONSIGNOR+11TH FLOOR, ALEX HOUSE, VICTORIA AV+SOUTHEND-ON-SEA, ESSEX++SS99 1AA+GB'NAD+UC+IT27THEBOSS42++S.CONSIGNEE+23 LE DON STR+CORLEONE++123-456+IT'NAD+GA+IT27THEBOSS00++CARRIER+23 LE DON STR+CORLEONE++123-456+IT'NAD+AH+++NWG'NAD+EI+Not Controlled'UNS+D'CST+1+6506'FTX+AAA+++TEST LINE 1'MEA+WT+AAB+KGM:1000'MEA+WT+AAA+KGM:950'PAC+6+++BX:10'PCI+28+AB234'DOC+190:::705+PD REFERENCE'DOC+190:::750+PD XXXXXXX REF'DOC+916:::ZZZ+9999::PRE ENTRY LINE'DOC+916:::ZZZ+9999::TEST SD'TOD+2++1'FTX+ACB++CAL+1GBP09GB00000100000M0'CST+2+1234'FTX+AAA+++T ST2'MEA+WT+AAB+KGM:500.55'MEA+WT+AAA+KGM:400.44'UNS+S'CNT+5:1'CNT+11:10'CNT+16:1'UNT+75+40340610115947'";
		}

		public void TestDocumentSupporterType()
		{
			var nctsEdiMessage = Factory.New<NctsEdiMessage>();
			var supporter = nctsEdiMessage.DocumentSupporter;
			AssertType(typeof(NctsEdiMessageDocumentSupporter), supporter);
		}

		public void TestGetFilterValue()
		{
			// Checks that our supporter is applicable to COMMANDs with the named filter
			var nctsEdiMessage = Factory.New<NctsEdiMessage>();
			var supporter = nctsEdiMessage.DocumentSupporter;
			AssertEquals("Y", supporter.GetFilterValue(DocumentFilters.NCTS));
			AssertEquals(null, supporter.GetFilterValue(DocumentFilters.SGAIRSHP)); // random base value
		}

		public void TestGetMenuTemplateFilterValue()
		{
			// Checks that our supporter is applicable to TEMPLATE PIVOTs with the named filter
			var nctsEdiMessage = NctsIE29EdiMessage(Factory, false);
			var supporter = nctsEdiMessage.DocumentSupporter;
			var filterType = MenuTemplateFilterType.Security;
			AssertEquals("N", supporter.GetMenuTemplateFilterValue(filterType, null));
			nctsEdiMessage = NctsIE29EdiMessage(Factory, true);
			supporter = nctsEdiMessage.DocumentSupporter;
			AssertEquals("Y", supporter.GetMenuTemplateFilterValue(filterType, null));
		}

		public void TestBusinessContext()
		{
			var nctsEdiMessage = Factory.New<NctsEdiMessage>();
			var supporter = nctsEdiMessage.DocumentSupporter;
			AssertEquals(BusinessContext.CusInBondHeader, supporter.BusinessContext);
		}

		public void TestGetSupportedDataContexts()
		{
			var nctsEdiMessage = Factory.New<NctsEdiMessage>();
			var supporter = nctsEdiMessage.DocumentSupporter;
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.EuNcts)));
			AssertEquals(false, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GbCcsuk)));
		}

		public void TestGetDocumentWrappersInternal()
		{
			// Checks that we get a regular wrapper or a security wrapper based on NctsHeader.IsSecurityDeclaration
			var nctsEdiMessage = NctsIE29EdiMessage(Factory, false);
			var supporter = nctsEdiMessage.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, null);
			var wrapper = wrappers[0];
			AssertEquals("NctsIE29DocumentWrapper", wrapper.GetType().Name);
			AssertEquals(nctsEdiMessage, wrapper.WrappedObject);

			nctsEdiMessage = NctsIE29EdiMessage(Factory, true);
			supporter = nctsEdiMessage.DocumentSupporter;
			wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, null);
			wrapper = wrappers[0];
			AssertEquals("SecurityNctsIE29DocumentWrapper", wrapper.GetType().Name);
			AssertEquals(nctsEdiMessage, wrapper.WrappedObject);
		}

		public static NctsEdiMessage NctsIE29EdiMessage(BusinessObjectFactory factory, bool withSecurity)
		{
			var fakeIE029MessageText = GetEmbeddedResourceFile("CC029B_MESSAGE.xml");
			if (withSecurity)
			{
				fakeIE029MessageText = fakeIE029MessageText.Replace("<SecHEA358>0</SecHEA358>", "<SecHEA358>1</SecHEA358>");
			}
			return SetupMessageToWrap(fakeIE029MessageText, factory, withSecurity);
		}

		public static NctsEdiMessage SetupMessageToWrap(ZString ediMessageText, BusinessObjectFactory factory, bool isSecurity)
		{
			var departure = factory.New<NctsHeader>();
			departure.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			departure.MovementHeader.GoodsItems.AddNew();
			if (isSecurity)
			{
				departure.BH_FTZMove = true;
				NCTSTestHelper.CreateJobDocAddressForTest(factory, "COS", departure.SecurityConsignor);
			}
			var message = departure.Messages.AddNew();
			var inboundMessage = factory.Load<NctsEdiMessage>(message.PK);
			inboundMessage.EM_MessageType = "GB";
			inboundMessage.EM_MessageSubType = "029";
			inboundMessage.EM_MessageText = ediMessageText;
			var mocker = factory.NewMoq<NctsEdiMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "999";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "NCT";
			departure.Messages.Add(outboundMessage);
			return inboundMessage;
		}

		public static ZString GetEmbeddedResourceFile(ZString embeddedResourceFile)
		{
			var retiever = new EmbeddedResourceRetriever();
			return retiever.GetString("Enterprise.Customs.EU.NCTS.Business.Testing.Ncts.EdiMessage.TestFiles." + embeddedResourceFile);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
		}
		ZString oldCountryCode;

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);
		}
	}
}
