using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCANTSendExpMessageBuilderTest : DCSendMessageBuilderTest<DCANTSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => EntryActionCodeList.Codes.ANT;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>1</codact>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.Declaration.JE_DateOfArrival = new ZDateTime("31/01/2029");
		}

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestPopulateGen()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);

			var messageExpGenPart
				= @"<Gen><typeproc>C</typeproc><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><Preval><datpreval>02/01/2020</datpreval><heurpreval>01</heurpreval></Preval><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
			+ "<nattrans>99</nattrans><dest>GB</dest><etatMembreExportationReel>DE</etatMembreExportationReel><Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat><bureausortie>AT330100</bureausortie><Sortie><typesortie>STC</typesortie><motiv>BLABLA</motiv></Sortie></Bureau><Operateur><Expediteurs><Expediteur><tin>ETRANGER</tin><nomoperateur>GE HEALTHCARE</nomoperateur>"
			+ "<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur><paysoperateur>US</paysoperateur><codepostaloperateur>60188</codepostaloperateur><villeoperateur>STERLING</villeoperateur></Expediteur></Expediteurs><Destinataires><Destinataire><tin>FR31501335900156</tin>"
			+ "<nomoperateur>GE MEDICAL SYSTEMMS SCS</nomoperateur><rueoperateur>RUE DE LA MINIERE</rueoperateur><paysoperateur>FR</paysoperateur><codepostaloperateur>78533</codepostaloperateur><villeoperateur>BUC CEDEX</villeoperateur></Destinataire></Destinataires><RepFisc><tin>"
			+ "FR32582075100080</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
			+ "<opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod></Operateur><prifac>4254.12</prifac><devfac>EUR</devfac><coursdevise>0.8</coursdevise><modpaiement>R</modpaiement><modgarantie>C</modgarantie>"
			+ "<ConditionsLivraison><codliv>FCA</codliv><lieuliv>STERLING</lieuliv><codelieuincoterm>3</codelieuincoterm></ConditionsLivraison><Transport><modfrotra>4</modfrotra><inttra>3</inttra><conteneurtra>0</conteneurtra><idtransport>ID12345</idtransport><natfrotra>US</natfrotra><transportMethodPayment>B</transportMethodPayment></Transport></Gen>";

			AssertContains(messageExpGenPart, message);
		}
	}
}
