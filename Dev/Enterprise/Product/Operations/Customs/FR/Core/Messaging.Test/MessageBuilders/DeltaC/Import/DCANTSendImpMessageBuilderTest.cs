using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCANTSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCANTSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.ANT;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Preval><datpreval>31/01/2029</datpreval><heurpreval>20:12</heurpreval></Preval>", "<Entete><codact>1</codact>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.EntryInstruction.CEI_DateForDuty = new ZDateTime(2029, 01, 31, 20, 12, 12);
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
				= @"<Gen><typeproc>C</typeproc><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><Preval><datpreval>02/01/2020</datpreval><heurpreval>01</heurpreval></Preval><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
				+ "<nattrans>99</nattrans><etatMembreDestinationFinale>FR</etatMembreDestinationFinale><Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat></Bureau><Operateur><Expediteurs><Expediteur><tin>ETRANGEREoriOnly</tin><nomoperateur>GE HEALTHCARE</nomoperateur>"
				+ "<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur><paysoperateur>US</paysoperateur><codepostaloperateur>60188</codepostaloperateur><villeoperateur>STERLING</villeoperateur></Expediteur></Expediteurs><Destinataires><Destinataire><tin>FR31501335900155</tin>"
				+ "<nomoperateur>GE MEDICAL SYSTEMMS SCS</nomoperateur><rueoperateur>RUE DE LA MINIERE</rueoperateur><paysoperateur>FR</paysoperateur><codepostaloperateur>78533</codepostaloperateur><villeoperateur>BUC CEDEX</villeoperateur></Destinataire></Destinataires>"
				+ "<RepFisc><tin>FR32582075100081</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
				+ "<opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod></Operateur><prifac>4254.12</prifac><devfac>EUR</devfac><coursdevise>0.8</coursdevise><modpaiement>R</modpaiement><modgarantie>C</modgarantie>"
				+ "<ConditionsLivraison><codliv>FCA</codliv><lieuliv>STERLING</lieuliv><codelieuincoterm>3</codelieuincoterm></ConditionsLivraison><Transport><modfrotra>4</modfrotra><inttra>3</inttra><conteneurtra>0</conteneurtra><natfrotra>US</natfrotra><burfro>FRB0617A</burfro>"
				+ "<aeroportemb>ORD</aeroportemb><aertra>3</aertra></Transport><ElementsValeurGen><CumulAerienTiers><Frais><montant>124.00</montant><devfac>USD</devfac></Frais></CumulAerienTiers><AutresFraisAjout><montant>25.00</montant><devfac>USD</devfac></AutresFraisAjout></ElementsValeurGen></Gen>";

			AssertContains(messageImpGenPart, message);
		}
	}
}
