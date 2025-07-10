using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Testing;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDANTSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDANTSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.ANT;

		protected override ZString[] ItemsNotContains => new ZString[] { "<refdec>", "<Operateur><opedest>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>3</codact>", "<Preval><datpreval>31/01/2029</datpreval><heurpreval>20:12</heurpreval></Preval>", "<dispopart>INC</dispopart>", "<dispopart>IsC</dispopart>", "<Document><doc>0001</doc><indd48>1</indd48><mntd48>1</mntd48><deld48>10</deld48></Document>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.EntryInstruction.CEI_DateForDuty = new ZDateTime(2029, 01, 31, 20, 12, 12);
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("ANT message doesn't populate DSIComp where Aeroportemb and Aertratag exist.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true, true);
			entry.Declaration.JE_DateOfArrival = new ZDateTime("31/01/2029");
			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("<Operateur><opedest>FR32159700500065</opedest>", message);
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

			var messageImpGenPart
				= @"<Gen><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><Preval><datpreval>02/01/2020</datpreval><heurpreval>01</heurpreval></Preval><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
				+ "<Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat></Bureau><Operateur><opedest>FR34430738400571</opedest><opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod><Expediteur><tin>ETRANGER</tin><nomoperateur>GE HEALTHCARE</nomoperateur>"
				+ "<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur><paysoperateur>US</paysoperateur><codepostaloperateur>60188</codepostaloperateur><villeoperateur>STERLING</villeoperateur></Expediteur>"
				+ "<RepFisc><tin>FR32582075100080</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
				+ "</Operateur><etatMembreDestinationFinale>GB</etatMembreDestinationFinale></Gen>";

			AssertContains(messageImpGenPart, message);
		}
	}
}
