using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Testing;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDVALSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDVALSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.VAL;

		protected override ZString[] ItemsNotContains => new ZString[] { "<refdec>", "<Operateur><opedest>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>1</codact>", "<dispopart>INC</dispopart>", "<dispopart>IsC</dispopart>", "<Document><doc>0001</doc><indd48>1</indd48><mntd48>1</mntd48><deld48>10</deld48></Document>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("VAL message doesn't populate DSIComp where Aeroportemb and Aertratag exist.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true, true);
			var message = deltaHelper.GetFlatMessage(MessageType, entry);

			AssertContains("<Operateur><opedest>FR32159700500065</opedest>", message);
		}

		public void TestEntrepotGroupWhenWarehouseTypeIsEmpty()
		{
			CreateDeclarationMock(MessageType, IsImport);

			myDeclGenArticle1.Setup(m => m.WarehouseType).Returns(ZString.Empty);
			myDeclGenArticle1.Setup(m => m.WarehouseReference).Returns("12345");
			myDeclGenArticle1.Setup(m => m.WarehouseCountryCode).Returns("FR");
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = Extensions.GetFlatXml(messageBuilder.GetMessage());
			AssertContains("<Entrepot><entref>12345</entref><entpays>FR</entpays></Entrepot>", Extensions.GetFlatXml(message));
		}

		public void TestNoMessageTvaCodeNotFoundError_ProcedureOperatorRepFiscIsNull()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			AssertEquals("No MessageTvaCodeNotFound error in DDSendExpMessageBaseBuilder", false, deltaHelper.GetMessageErrorsForTest(false, true).Contains(MessageBuilderHelper.MessageTvaCodeNotFound));
		}

		public void TestPopulateMessageEnvelope()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);

			var messageImpEnvelopPart = @"<EnveloppeMessage><schemaID>MessageDDecImp</schemaID><schemaVersion>1032013</schemaVersion><partyId>33159700500064</partyId><transactionId>94321-B00175768</transactionId><numseq>999</numseq></EnveloppeMessage>";

			AssertContains(messageImpEnvelopPart, message);
		}

		public void TestPopulateHeader()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);
			var messageHeaderPart = @"<Entete><codact>2</codact><refdos>8461132</refdos></Entete>";

			AssertContains(messageHeaderPart, message);
		}

		public void TestPopulateGen()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);

			var messageImpGenPart
				= @"<Gen><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
				+ "<Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat></Bureau><Operateur><opedest>FR34430738400571</opedest><opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod><Expediteur><tin>ETRANGER</tin><nomoperateur>GE HEALTHCARE</nomoperateur>"
				+ "<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur><paysoperateur>US</paysoperateur><codepostaloperateur>60188</codepostaloperateur><villeoperateur>STERLING</villeoperateur></Expediteur>"
				+ "<RepFisc><tin>FR32582075100080</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
				+ "</Operateur><etatMembreDestinationFinale>GB</etatMembreDestinationFinale></Gen>";

			AssertContains(messageImpGenPart, message);
		}

		public void TestPopulateArticles()
		{
			CreateDeclarationMockDNM(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			ZString messageImpGenArticlesPart = @"<Articles>" +
									"<Article>" +
										"<numart>1</numart><nomenc>8537109170</nomenc>" +
											"<Dispoparts>" +
												"<dispopart>Y053</dispopart><dispopart>Y069</dispopart><dispopart>Y949</dispopart>" +
											"</Dispoparts>" +
											"<descom>" +
												"Tableaux, panneaux, consoles, pupitres, armoires et autres supports comportant plusieurs appareils des n° 8535 ou 8536, pour la commande ou la distribution electrique, y compris ceux incorporant des instruments ou appareils du chapitre 90 ainsi que les apparei" +
											"</descom>" +
											"<msb>1</msb><msn>1</msn><ori>US</ori><pro>US</pro>" +
											"<UniSpe>" +
												"<unispe>NAR</unispe><nbrunispe>1</nbrunispe>" +
											"</UniSpe>" +
											"<RegimeDouanier>" +
												"<regdou>40</regdou><regdoupre>00</regdoupre><compcom>000</compcom>" +
											"</RegimeDouanier>" +
											"<AutorisationEco><autorisationeco>3100</autorisationeco><autorisationpays>FR</autorisationpays></AutorisationEco>" +
											"<RegimeEco>" +
												"<DecEcos>" +
													"<DecEco><refdec>CLE_0002</refdec><typedececo>3</typedececo></DecEco><DecEco><refdec>PRE_004</refdec><typedececo>5</typedececo></DecEco>" +
												"</DecEcos>" +
												"<montantgar>24</montantgar><delapur>12</delapur>" +
											"</RegimeEco>" +
												"<Preference><preftar1>1</preftar1><preftar2>00</preftar2></Preference>" +
												"<Colisage>" +
													"<nbrcol>1</nbrcol>" +
													"<nbrpieces>0</nbrpieces>" +
													"<natcol>CT</natcol>" +
													"<marquecolis>Test20230104013012345678911223344556677889</marquecolis>" +
												"</Colisage>" +
												"<PriseEnCharge>" +
													"<natdocpec>741</natdocpec><typdocpec>Z</typdocpec><refdocpec>05781504706</refdocpec>" +
												"</PriseEnCharge>" +
												"<Documents>" +
													"<Document>" +
														"<doc>N787</doc>" +
														"<refdoc>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA</refdoc>" +
														"<datdoc>06/03/2019</datdoc>" +
														"<indd48>0</indd48>" +
														"<FichesImputations>" +
															"<FicheImputation>" +
																"<numLigne>1</numLigne>" +
																"<refProduit>BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB</refProduit>" +
																"<nomProduit>CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC</nomProduit>" +
															"</FicheImputation>" +
														"</FichesImputations>" +
													"</Document>" +
													"<Document>" +
														"<doc>N741</doc><refdoc>05781504706</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
													"</Document>" +
													"<Document>" +
														"<doc>N740</doc><refdoc>8461132</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
													"</Document>" +
													"<Document>" +
														"<doc>N380</doc><refdoc>293337842</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
													"</Document>" +
												"</Documents>" +
												"<DonneesFinancieres><prifac>1101.01</prifac><devfac>EUR</devfac></DonneesFinancieres>" +
									"</Article>" +
									"<Article>" +
									"<numart>2</numart><nomenc>8544200090</nomenc>" +
									"<descom>" +
										"Fils, cables (y compris les cables coaxiaux) et autres conducteurs isoles pour l electricite (meme laques ou oxydes anodiquement), munis ou non de pieces de connexion; cables de fibres optiques, constitues de fibres gainees individuellement, meme comportant de" +
									"</descom>" +
									"<msb>3</msb><msn>3</msn><ori>US</ori><pro>US</pro><RegimeDouanier><regdou>40</regdou><regdoupre>00</regdoupre><compcom>000</compcom></RegimeDouanier>" +
									"<AutorisationEco><autorisationeco>3100</autorisationeco><autorisationpays>FR</autorisationpays></AutorisationEco>" +
									"<RegimeEco><delapur>0</delapur></RegimeEco><Preference><preftar1>1</preftar1><preftar2>00</preftar2></Preference>" +
									"<Colisage>" +
										"<nbrcol>0</nbrcol>" +
										"<nbrpieces>0</nbrpieces>" +
										"<natcol>CT</natcol>" +
										"<marquecolis>Test20230104013012345678911223344556677889</marquecolis>" +
									"</Colisage>" +
									"<PriseEnCharge><natdocpec>741</natdocpec><typdocpec>Z</typdocpec><refdocpec>05781504706</refdocpec></PriseEnCharge>" +
									"<Documents>" +
										"<Document>" +
											"<doc>N787</doc><refdoc>N° GROS 61AF137</refdoc><datdoc>06/03/2019</datdoc><indd48>0</indd48>" +
										"</Document>" +
										"<Document>" +
											"<doc>N741</doc><refdoc>05781504706</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
										"</Document>" +
										"<Document>" +
											"<doc>N740</doc><refdoc>8461132</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
										"</Document>" +
										"<Document>" +
											"<doc>N380</doc><refdoc>293337842</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
										"</Document>" +
									"</Documents>" +
									"<DonneesFinancieres><prifac>3153.11</prifac><devfac>EUR</devfac></DonneesFinancieres>" +
								"</Article>" +
							"</Articles>";

			AssertContains("DCSendImportMessage", messageImpGenArticlesPart, message);
		}

		public void TestArticleAdditionalCodes()
		{
			CreateDeclarationMock(MessageType, IsImport);

			myDeclGenArticle1.Setup(m => m.CETariffAdditionalCodes).Returns<IEnumerable<ITariffAdditionalCode>>(null);
			myDeclGenArticle1.Setup(m => m.FRTariffAdditionalCodes).Returns<IEnumerable<ITariffAdditionalCode>>(null);
			myDeclGenArticle1.Setup(m => m.PartDispos).Returns<IEnumerable<ITariffAdditionalCode>>(null);
			myDeclGenArticle1.Setup(m => m.QuotaRefNumber).Returns<IEnumerable<ZString>>(null);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertNotContains("<Cacos>", message);
			AssertNotContains("<Canas>", message);
			AssertNotContains("<Dispoparts>", message);
			AssertNotContains("<Cnts>", message);

			var myCETariffAdditionalCode = new Mock<ITariffAdditionalCode> { CallBase = true };
			myCETariffAdditionalCode.Setup(m => m.Code).Returns("12345");
			myDeclGenArticle1.Setup(m => m.CETariffAdditionalCodes).Returns(new[] { myCETariffAdditionalCode.Object });
			var myFRTariffAdditionalCode = new Mock<ITariffAdditionalCode> { CallBase = true };
			myFRTariffAdditionalCode.Setup(m => m.Code).Returns("54321");
			myDeclGenArticle1.Setup(m => m.FRTariffAdditionalCodes).Returns(new[] { myFRTariffAdditionalCode.Object });
			var myPartDispos = new Mock<ITariffAdditionalCode> { CallBase = true };
			myPartDispos.Setup(m => m.Code).Returns("67890");
			myDeclGenArticle1.Setup(m => m.PartDispos).Returns(new[] { myPartDispos.Object });
			myDeclGenArticle1.Setup(m => m.QuotaRefNumber).Returns(new ZString[] { "test cnts" });
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = Extensions.GetFlatXml(messageBuilder.GetMessage());
			AssertContains("<Cacos><caco>12345</caco></Cacos>", message);
			AssertContains("<Canas><cana>54321</cana></Canas>", message);
			AssertContains("<Dispoparts><dispopart>67890</dispopart></Dispoparts>", message);
			AssertContains("<Cnts><numcnt>test cnts</numcnt></Cnts>", message);
		}

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}

		public void TestNoErrorAnymoreWhenPartnerIdNotFound()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var errorCollector = new ErrorCollector();
			var messageEnveloppeMock = new Mock<IMessageEnvelope>();
			messageEnveloppeMock.Setup(m => m.SchemaID).Returns("MessageCDecImp");
			messageEnveloppeMock.Setup(m => m.SchemaVersion).Returns("1032013");
			messageEnveloppeMock.Setup(m => m.PartnerId).Returns(ZString.Empty);
			messageEnveloppeMock.Setup(m => m.TransactionId).Returns("94321-B00175768");
			messageEnveloppeMock.Setup(m => m.NumSeq).Returns(new ZShort("0"));
			declarationMock.Setup(m => m.MessageEnvelope).Returns(messageEnveloppeMock.Object);

			errorCollector.WipeErrors();
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertEquals(0, errorCollector.ErrorCount);
		}

		public void TestAutorisationEcoAndRegimeEcoGroupGivenEcoRegimeAuthorization()
		{
			CreateDeclarationMock(MessageType, IsImport);

			myDeclGenArticle1.Setup(m => m.EcoRegimeAuthorization).Returns<IEcoRegimeAuthorization>(null);
			myDeclGenArticle1.Setup(m => m.EcoRegimeDatas).Returns<IEcoRegimeDatas>(null);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertNotContains("<AutorisationEco>", message);
			AssertNotContains("<RegimeEco>", message);

			var myEcoRegimeAuthorization = new Mock<IEcoRegimeAuthorization> { CallBase = true };
			myEcoRegimeAuthorization.Setup(m => m.EcoRegimeAuthorizationNumber).Returns("12345");
			myEcoRegimeAuthorization.Setup(m => m.EcoRegimeCountryCode).Returns("FR");
			myDeclGenArticle1.Setup(m => m.EcoRegimeAuthorization).Returns(myEcoRegimeAuthorization.Object);
			EconomicRegimeMock(myDeclGenArticle1);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);
			AssertContains("<AutorisationEco><autorisationeco>12345</autorisationeco><autorisationpays>FR</autorisationpays></AutorisationEco>", message);
			AssertContains("<RegimeEco><DecEcos><DecEco><refdec>CLE_0002</refdec><typedececo>3</typedececo></DecEco><DecEco><refdec>PRE_004</refdec><typedececo>5</typedececo></DecEco></DecEcos><montantgar>24</montantgar><delapur>12</delapur></RegimeEco>", message);
		}
	}
}
