using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Testing;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDANTSendExpMessageBuilderTest : DDSendMessageBuilderTest<DDANTSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => EntryActionCodeList.Codes.ANT;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Destinatairefinal><tin>", "<MetaData>", "<Motivation>", "<refdec>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>3</codact>", "<Preval><datpreval>31/01/2029</datpreval><heurpreval>20:12</heurpreval></Preval>", "<dispopart>INC</dispopart>", "<dispopart>IsC</dispopart>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.EntryInstruction.CEI_DateForDuty = new ZDateTime(2029, 01, 31, 20, 12, 12);
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, false, true);
			entry.Declaration.JE_ExportDate = new ZDateTime("31/01/2029");
			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("<Destinatairefinal><tin>FR32159700500065</tin>", message);
		}

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}

		public void TestPopulateGen()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);

			var messageExpGenPart
				= @"<Gen><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><Preval><datpreval>02/01/2020</datpreval><heurpreval>01</heurpreval></Preval><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
				+ "<dest>GB</dest><Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat><bureausortie>AT330100</bureausortie><Sortie><typesortie>STC</typesortie><motiv>BLABLA</motiv></Sortie></Bureau><Operateur><expediteur>ETRANGER</expediteur><opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod>"
				+ "<destinataire><tin>FR3150EORIONLY</tin><nomoperateur>GE MEDICAL SYSTEMMS SCS</nomoperateur><rueoperateur>RUE DE LA MINIERE</rueoperateur><paysoperateur>FR</paysoperateur><codepostaloperateur>78533</codepostaloperateur><villeoperateur>BUC CEDEX</villeoperateur></destinataire><RepFisc><tin>"
				+ "FR32582075100080</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
				+ "</Operateur><TransportExp><inttra>3</inttra><conteneurtra>0</conteneurtra><idtransport>ID12345</idtransport><transportMethodPayment>B</transportMethodPayment></TransportExp>"
				+ "<specificCircumstanceIndicator>Circum</specificCircumstanceIndicator><etatMembreExportationReel>DE</etatMembreExportationReel></Gen>";

			AssertContains(messageExpGenPart, message);
		}
	}
}
