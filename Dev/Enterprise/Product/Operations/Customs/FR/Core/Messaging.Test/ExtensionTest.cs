using System.Collections.ObjectModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using MessagingResponseDeltaCExport = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Response.Export;
using MessagingResponseDeltaCImport = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Response.Import;
using MessagingSendDeltaCExport = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Export;
using MessagingSendDeltaCImport = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Import;

namespace Enterprise.Customs.FR.Messaging.Testing
{
	class ExtensionTest : TestCaseWithFactory
	{
		#region Serialization/Deserialization test

		#region Declarations

		const short numArtTested = 1;
		const string errCodeTested = "ERR1";
		const string errDescriptionTested = "My description error";

		readonly ZString messageDeltaCSend =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
				<Message>
				<EnveloppeMessage>
				<numseq>5</numseq>
				</EnveloppeMessage>
				<Declaration>
				<DatasDec>
				<Entete>
				<codact>2</codact>
				<refdos>8461132</refdos>
				</Entete>
				<Gen>
				<typeproc>C</typeproc>
				<procedure1>IM</procedure1>
				<procedure2>A</procedure2>
				<nbrart>2</nbrart>
				<nbrcol>1</nbrcol>
				<locagr>34796082500052/1</locagr>
				<magasin>CDGSO1</magasin>
				<nattrans>99</nattrans>
				<etatMembreDestinationFinale>FR</etatMembreDestinationFinale>
				<Bureau>
				<burdom>FRB0617A</burdom>
				<burrat>FRB0617A</burrat>
				</Bureau>
				<Operateur>
				<Expediteurs>
				<Expediteur>
				<tin>ETRANGER</tin>
				<nomoperateur>GE HEALTHCARE</nomoperateur>
				<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur>
				<paysoperateur>US</paysoperateur>
				<codepostaloperateur>60188</codepostaloperateur>
				<villeoperateur>STERLING</villeoperateur>
				</Expediteur>
				</Expediteurs>
				<Destinataires>
				<Destinataire>
				<tin>FR31501335900155</tin>
				<nomoperateur>GE MEDICAL SYSTEMMS SCS</nomoperateur>
				<rueoperateur>RUE DE LA MINIERE</rueoperateur>
				<paysoperateur>FR</paysoperateur>
				<codepostaloperateur>78533</codepostaloperateur>
				<villeoperateur>BUC CEDEX</villeoperateur>
				</Destinataire>
				</Destinataires>
				<opeben>FR34430738400570</opeben>
				<numagr>00000169</numagr>
				<operep>FR34430738400570</operep>
				<modrep>2</modrep>
				<numcre>ALLI</numcre>
				<numcod>ALLI</numcod>
				</Operateur>
				<prifac>4254.12</prifac>
				<devfac>EUR</devfac>
				<modpaiement>R</modpaiement>
				<modgarantie>C</modgarantie>
				<ConditionsLivraison>
				<codliv>FCA</codliv>
				<lieuliv>STERLING</lieuliv>
				<codelieuincoterm>3</codelieuincoterm>
				</ConditionsLivraison>
				<Transport>
				<modfrotra>4</modfrotra>
				<inttra>3</inttra>
				<conteneurtra>0</conteneurtra>
				<natfrotra>US</natfrotra>
				<burfro>FRB0617A</burfro>
				<aeroportemb>ORD</aeroportemb>
				<aertra>3</aertra>
				</Transport>
				<ElementsValeurGen>
				<CumulAerienTiers>
				<Frais>
				<montant>124.00</montant>
				<devfac>USD</devfac>
				</Frais>
				</CumulAerienTiers>
				<AutresFraisAjout>
				<montant>25.00</montant>
				<devfac>USD</devfac>
				</AutresFraisAjout>
				</ElementsValeurGen>
				</Gen>
				<Articles>
				<Article>
				<numart>1</numart>
				<nomenc>8537109170</nomenc>
				<Dispoparts>
				<dispopart>Y053</dispopart>
				<dispopart>Y069</dispopart>
				<dispopart>Y949</dispopart>
				</Dispoparts>
				<descom>Tableaux, panneaux, consoles, pupitres, armoires et autres supports comportant plusieurs appareils des n° 8535 ou 8536, pour la commande ou la distribution electrique, y compris ceux incorporant des instruments ou appareils du chapitre 90 ainsi que les apparei</descom>
				<msb>1.172411</msb>
				<msn>1.035241</msn>
				<ori>US</ori>
				<pro>US</pro>
				<valevaluation>1</valevaluation>
				<UniSpe>
				<unispe>NAR</unispe>
				<nbrunispe>1</nbrunispe>
				</UniSpe>
				<RegimeDouanier>
				<regdou>40</regdou>
				<regdoupre>00</regdoupre>
				<compcom>000</compcom>
				</RegimeDouanier>
				<Preference>
				<preftar1>1</preftar1>
				<preftar2>00</preftar2>
				</Preference>
				<Colisage>
				<nbrcol>1</nbrcol>
				<natcol>CT</natcol>
				<marquecolis>ADR</marquecolis>
				</Colisage>
				<PriseEnCharge>
				<natdocpec>741</natdocpec>
				<typdocpec>Z</typdocpec>
				<refdocpec>05781504706</refdocpec>
				</PriseEnCharge>
				<Documents>
				<Document>
				<doc>N787</doc>
				<refdoc>N° GROS 61AF137</refdoc>
				<datdoc>06/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				<Document>
				<doc>N741</doc>
				<refdoc>05781504706</refdoc>
				<datdoc>05/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				<Document>
				<doc>N740</doc>
				<refdoc>8461132</refdoc>
				<datdoc>05/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				<Document>
				<doc>N380</doc>
				<refdoc>293337842</refdoc>
				<datdoc>05/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				</Documents>
				<DonneesFinancieres>
				<prifac>1101.01</prifac>
				<devfac>EUR</devfac>
				</DonneesFinancieres>
				<depliv>95</depliv>
				</Article>
				<Article>
				<numart>2</numart>
				<nomenc>8544200090</nomenc>
				<descom>Fils, cables (y compris les cables coaxiaux) et autres conducteurs isoles pour l electricite (meme laques ou oxydes anodiquement), munis ou non de pieces de connexion; cables de fibres optiques, constitues de fibres gainees individuellement, meme comportant de</descom>
				<msb>3.357589</msb>
				<msn>2.964759</msn>
				<ori>US</ori>
				<pro>US</pro>
				<valevaluation>1</valevaluation>
				<RegimeDouanier>
				<regdou>40</regdou>
				<regdoupre>00</regdoupre>
				<compcom>000</compcom>
				</RegimeDouanier>
				<Preference>
				<preftar1>1</preftar1>
				<preftar2>00</preftar2>
				</Preference>
				<Colisage>
				<nbrcol>0</nbrcol>
				<natcol>CT</natcol>
				<marquecolis>ADR</marquecolis>
				</Colisage>
				<PriseEnCharge>
				<natdocpec>741</natdocpec>
				<typdocpec>Z</typdocpec>
				<refdocpec>05781504706</refdocpec>
				</PriseEnCharge>
				<Documents>
				<Document>
				<doc>N787</doc>
				<refdoc>N° GROS 61AF137</refdoc>
				<datdoc>06/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				<Document>
				<doc>N741</doc>
				<refdoc>05781504706</refdoc>
				<datdoc>05/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				<Document>
				<doc>N740</doc>
				<refdoc>8461132</refdoc>
				<datdoc>05/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				<Document>
				<doc>N380</doc>
				<refdoc>293337842</refdoc>
				<datdoc>05/03/2019</datdoc>
				<indd48>0</indd48>
				</Document>
				</Documents>
				<DonneesFinancieres>
				<prifac>3153.11</prifac>
				<devfac>EUR</devfac>
				</DonneesFinancieres>
				<depliv>95</depliv>
				</Article>
				</Articles>
				</DatasDec>
				</Declaration>
				</Message>
				";
		static readonly ZString messageDeltaCReponseStart = @"<?xml version=""1.0"" encoding=""utf-8""?><TReponseDatas xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">";

		static readonly ZString messageDeltaCReponseEnd = @"<Erreur><ErreurArticle><numart>0</numart>"
										+ "<ReponseErreur><erreurCode>ERR1</erreurCode><erreurDescription>My description error</erreurDescription></ReponseErreur></ErreurArticle></Erreur><Alerte><AlerteArticle><numart>1</numart></AlerteArticle></Alerte></TReponseDatas>";
		readonly ZString messageDeltaCReponse = messageDeltaCReponseStart + messageDeltaCReponseEnd;
		ZString[] messageDeltaCReponseList => new ZString[] { @"<?xml version=""1.0"" encoding=""utf-8""?>", @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", messageDeltaCReponseEnd };

		#endregion

		#region Test DeltaC Send (MessageCDecImp.xsd)

		public void TestSerializeSendDeltaCImport()
		{
			#region Serialization
			var itemMessage = new MessagingSendDeltaCImport.TMessage();
			var itemDecImp = new MessagingSendDeltaCImport.TcDecImp();
			var itemEnvelopMessage = new MessagingSendDeltaCImport.TEnveloppeMessage();
			itemEnvelopMessage.Numseq = 888;
			itemMessage.EnveloppeMessage = itemEnvelopMessage;

			var itemDatasDec = new MessagingSendDeltaCImport.TDatasDec();
			var itemEntete = new MessagingSendDeltaCImport.TEntete();
			itemEntete.Codact = "2";
			itemEntete.Refdos = "8461132";
			itemDatasDec.Entete = itemEntete;

			itemMessage.Declaration = itemDecImp;
			itemDecImp.DatasDec = itemDatasDec;

			ZString messageSerialized = Extensions.Serialize(itemMessage);

			#endregion

			#region Assertion

			AssertNotNullOrEmpty(messageSerialized);
			AssertEquals(320, messageSerialized.Length);

			ZString[] messageDeltaCSendValidList = { @"<?xml version=""1.0"" encoding=""utf-8""?>", @"xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", @"<EnveloppeMessage><numseq>888</numseq></EnveloppeMessage>" };

			foreach (var item in messageDeltaCSendValidList)
			{
				AssertContains(item, messageSerialized);
			}

			#endregion
		}

		public void TestSerializeSendDeltaCExport()
		{
			#region Serialization

			var itemMessage = new MessagingSendDeltaCExport.TMessage();
			var itemDecExp = new MessagingSendDeltaCExport.TcDecExp();
			var itemEnvelopMessage = new MessagingSendDeltaCExport.TEnveloppeMessage();
			itemEnvelopMessage.Numseq = 888;
			itemMessage.EnveloppeMessage = itemEnvelopMessage;

			var itemDatasDec = new MessagingSendDeltaCExport.TDatasDec();
			var itemEntete = new MessagingSendDeltaCExport.TEntete();
			itemEntete.Codact = "2";
			itemEntete.Refdos = "8461132";
			itemDatasDec.Entete = itemEntete;

			itemMessage.Declaration = itemDecExp;
			itemDecExp.DatasDec = itemDatasDec;

			ZString messageSerialized = Extensions.Serialize(itemMessage);

			#endregion

			#region Assertion

			AssertNotNullOrEmpty(messageSerialized);
			AssertEquals(320, messageSerialized.Length);

			ZString messageDeltaCSendValid = @"<EnveloppeMessage><numseq>888</numseq></EnveloppeMessage>";

			AssertContains(messageDeltaCSendValid, messageSerialized);

			#endregion
		}

		public void TestDeserializeSendDeltaCImport()
		{
			var itemMessage = Extensions.Deserialize<MessagingSendDeltaCImport.TMessage>(messageDeltaCSend);
			var itemDecImp = itemMessage.Declaration;
			var itemDatasDec = itemDecImp.DatasDec;

			var itemEntete = itemDatasDec.Entete;
			var itemArticles = itemDatasDec.Articles;

			AssertNotNull(itemDatasDec);
			AssertNotNull(itemEntete);
			AssertNotNull(itemArticles);
			AssertEquals(2, itemArticles.Count);

			var itemArticle1 = itemDatasDec.Articles[0];
			AssertEquals((short)1, itemArticle1.Numart);

			var itemArticle2 = itemDatasDec.Articles[1];
			AssertEquals((short)2, itemArticle2.Numart);
		}

		public void TestDeserializeSendDeltaCExport()
		{
			var itemMessage = Extensions.Deserialize<MessagingSendDeltaCExport.TMessage>(messageDeltaCSend);
			var itemDecExp = itemMessage.Declaration;
			var itemDatasDec = itemDecExp.DatasDec;

			var itemEntete = itemDatasDec.Entete;
			var itemArticles = itemDatasDec.Articles;

			AssertNotNull(itemDatasDec);
			AssertNotNull(itemEntete);
			AssertNotNull(itemArticles);
			AssertEquals(2, itemArticles.Count);

			var itemArticle1 = itemDatasDec.Articles[0];
			AssertEquals((short)1, itemArticle1.Numart);

			var itemArticle2 = itemDatasDec.Articles[1];
			AssertEquals((short)2, itemArticle2.Numart);
		}

		#endregion

		#region Test DeltaC Response (MessageReponseCDecImp.xsd)

		public void TestSerializeResponseDeltaCImport()
		{
			#region Serialization

			var itemReponseDeclaration = new MessagingResponseDeltaCImport.TReponseDatas();
			var itemReponseAlert = new MessagingResponseDeltaCImport.TAlerteArticle();
			itemReponseAlert.Numart = numArtTested;
			itemReponseDeclaration.Alerte = new Collection<MessagingResponseDeltaCImport.TAlerteArticle> { itemReponseAlert };

			var itemReponseArticleError = new MessagingResponseDeltaCImport.TErreurArticle();
			var itemReponseErreur = new MessagingResponseDeltaCImport.TReponseErreur();

			itemReponseErreur.ErreurCode = errCodeTested;
			itemReponseErreur.ErreurDescription = errDescriptionTested;
			itemReponseArticleError.ReponseErreur = new Collection<MessagingResponseDeltaCImport.TReponseErreur> { itemReponseErreur };
			itemReponseDeclaration.Erreur = new MessagingResponseDeltaCImport.TErreur();
			itemReponseDeclaration.Erreur.ErreurArticle = new Collection<MessagingResponseDeltaCImport.TErreurArticle> { itemReponseArticleError };

			ZString messageSerialized = Extensions.Serialize(itemReponseDeclaration);

			#endregion

			#region Assertion

			AssertNotNullOrEmpty("DeltaCImport: Serialized Message Not null", messageSerialized);
			AssertEquals("DeltaCImport: Serialized Message Length", 419, messageSerialized.Length);

			foreach (var item in messageDeltaCReponseList)
			{
				AssertContains(item, messageSerialized);
			}

			#endregion
		}

		public void TestSerializeResponseDeltaCExport()
		{
			#region Serialization

			var itemReponseDeclaration = new MessagingResponseDeltaCExport.TReponseDatas();
			var itemReponseAlert = new MessagingResponseDeltaCExport.TAlerteArticle();
			itemReponseAlert.Numart = numArtTested;
			itemReponseDeclaration.Alerte = new Collection<MessagingResponseDeltaCExport.TAlerteArticle> { itemReponseAlert };

			var itemReponseArticleError = new MessagingResponseDeltaCExport.TErreurArticle();
			var itemReponseErreur = new MessagingResponseDeltaCExport.TReponseErreur();

			itemReponseErreur.ErreurCode = errCodeTested;
			itemReponseErreur.ErreurDescription = errDescriptionTested;
			itemReponseArticleError.ReponseErreur = new Collection<MessagingResponseDeltaCExport.TReponseErreur> { itemReponseErreur };
			itemReponseDeclaration.Erreur = new MessagingResponseDeltaCExport.TErreur();
			itemReponseDeclaration.Erreur.ErreurArticle = new Collection<MessagingResponseDeltaCExport.TErreurArticle> { itemReponseArticleError };

			ZString messageSerialized = Extensions.Serialize(itemReponseDeclaration);

			#endregion

			#region Assertion

			AssertNotNullOrEmpty("DeltaCExport: Serialized Message Not null", messageSerialized);
			AssertEquals("DeltaCExport: Serialized Message Length", 419, messageSerialized.Length);

			AssertContains("DeltaCExport: Serialized Message", messageDeltaCReponseEnd, messageSerialized);

			#endregion
		}

		public void TestDeserializeResponseDeltaCImport()
		{
			var itemReponseDeclaration = Extensions.Deserialize<MessagingResponseDeltaCImport.TReponseDatas>(messageDeltaCReponse);

			AssertNotNull(itemReponseDeclaration);
			AssertNotNull(itemReponseDeclaration.Alerte);
			AssertNotNull(itemReponseDeclaration.Erreur);
			AssertEquals(1, itemReponseDeclaration.Alerte.Count);
			AssertEquals(1, itemReponseDeclaration.Erreur.ErreurArticle.Count);

			var itemAlert1 = itemReponseDeclaration.Alerte[0];
			var itemErreur1 = itemReponseDeclaration.Erreur.ErreurArticle[0];

			AssertEquals(numArtTested, itemAlert1.Numart);
			AssertEquals(1, itemErreur1.ReponseErreur.Count);
			AssertEquals(errCodeTested, itemErreur1.ReponseErreur[0].ErreurCode);
			AssertEquals(errDescriptionTested, itemErreur1.ReponseErreur[0].ErreurDescription);
		}
		public void TestDeserializeResponseDeltaCExport()
		{
			var itemReponseDeclaration = Extensions.Deserialize<MessagingResponseDeltaCExport.TReponseDatas>(messageDeltaCReponse);

			AssertNotNull(itemReponseDeclaration);
			AssertNotNull(itemReponseDeclaration.Alerte);
			AssertNotNull(itemReponseDeclaration.Erreur);
			AssertEquals(1, itemReponseDeclaration.Alerte.Count);
			AssertEquals(1, itemReponseDeclaration.Erreur.ErreurArticle.Count);

			var itemAlert1 = itemReponseDeclaration.Alerte[0];
			var itemErreur1 = itemReponseDeclaration.Erreur.ErreurArticle[0];

			AssertEquals(numArtTested, itemAlert1.Numart);
			AssertEquals(1, itemErreur1.ReponseErreur.Count);
			AssertEquals(errCodeTested, itemErreur1.ReponseErreur[0].ErreurCode);
			AssertEquals(errDescriptionTested, itemErreur1.ReponseErreur[0].ErreurDescription);
		}
		#endregion

		#endregion

		#region XML CLeaner

		public void TestCleanXML()
		{
			string xmlString = "<root><abcde /></root>";
			AssertEquals("<root />", Extensions.CleanXML(xmlString));
			xmlString = "<root><abcde num=\"123\" /></root>";
			AssertEquals("<root>\r\n  <abcde num=\"123\" />\r\n</root>", Extensions.CleanXML(xmlString));
			xmlString = "<root><abc>adjjs</abc>ezrtzeg<abcde />rfzqaf</root>";
			AssertEquals("<root>\r\n  <abc>adjjs</abc>ezrtzegrfzqaf</root>", Extensions.CleanXML(xmlString));
			xmlString = "<root><abc>ardfgeh</abc></root>";
			AssertEquals("<root>\r\n  <abc>ardfgeh</abc>\r\n</root>", Extensions.CleanXML(xmlString));
			xmlString = "<root>I like trains      <abc /></root>";
			AssertEquals("<root>I like trains      </root>", Extensions.CleanXML(xmlString));
			xmlString = "<root><abc><abcde /></abc></root>";
			AssertEquals("<root />", Extensions.CleanXML(xmlString));
			xmlString = "<root><abc><abcde num=\"123\" /></abc></root>";
			AssertEquals("<root>\r\n  <abc>\r\n    <abcde num=\"123\" />\r\n  </abc>\r\n</root>", Extensions.CleanXML(xmlString));
		}

		#endregion

		public void TestGetFlatXml()
		{
			var xml = @"
<Root>
  <Node1>
    <Node2>
    </Node2>
  </Node1>
	<Node3>
	</Node3>
</Root>";
			AssertEquals("<Root><Node1><Node2></Node2></Node1><Node3></Node3></Root>", Extensions.GetFlatXml(xml));
		}
	}
}

