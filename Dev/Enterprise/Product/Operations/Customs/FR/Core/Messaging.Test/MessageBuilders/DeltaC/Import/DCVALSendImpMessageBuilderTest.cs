using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCVALSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCVALSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.VAL;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>2</codact>" };

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}

		public void TestTinNotExportedIfNotEu()
		{
			var importerEU = Factory.NewWithValidTestData<OrgHeader>();
			importerEU.OH_Code = "IMPORTERFR";
			importerEU.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "404336911", Core.Constants.CountryCodes.France);

			var supplierEU = Factory.NewWithValidTestData<OrgHeader>();
			supplierEU.OH_Code = "SUPPLIERFR";
			supplierEU.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "752845621", Core.Constants.CountryCodes.France);

			var importerUK = Factory.NewWithValidTestData<OrgHeader>();
			importerUK.OH_Code = "IMPORTERUK";
			importerUK.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "404336911", Core.Constants.CountryCodes.UnitedKingdom);

			var supplierUK = Factory.NewWithValidTestData<OrgHeader>();
			supplierUK.OH_Code = "SUPPLIERUK";
			supplierUK.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "752845621", Core.Constants.CountryCodes.UnitedKingdom);

			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(true, true);
			var invoice = entry.MergedLines[0].InvoiceLines[0].InvoiceHeader;
			invoice.JZ_OH_Supplier = supplierEU.PK;
			entry.Declaration.ImporterDocumentaryAddress.E2_OA_Address = importerEU.MainAddress.PK;
			entry.Declaration.JE_OH_Importer = importerEU.PK;
			Factory.Save();

			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("Expected tin value when importer belongs to European Union", "<Destinataire><tin>FR404336911</tin>", message);
			AssertContains("Expected tin value when supplier belongs to European Union", "<Expediteur><tin>FR752845621</tin>", message);

			invoice.JZ_OH_Supplier = supplierUK.PK;
			entry.Declaration.ImporterDocumentaryAddress.E2_OA_Address = importerUK.MainAddress.PK;
			entry.Declaration.JE_OH_Importer = importerUK.PK;

			message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertNotContains("tin value must not be exported when supplier belongs to UK", "<Expediteur><tin>", message);
			AssertNotContains("tin value must not be exported when importer belongs to UK", "<Destinataire><tin>", message);
		}

		public void TestNoMessageTvaCodeNotFoundError_ProcedureOperatorRepFiscIsNull()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			AssertEquals("No MessageTvaCodeNotFound error in DCSendImpMessageBaseBuilder", false, deltaHelper.GetMessageErrorsForTest(true, true).Contains(MessageBuilderHelper.MessageTvaCodeNotFound));
		}

		public void TestVatCustomsStatisticalValues()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(true, true);
			entry.EntryInstruction.ZG_BypassCode = ZString.Empty;
			var message = deltaHelper.GetMessage(MessageType, entry);

			AssertNotContains("<valdou>", message);
			AssertNotContains("<valstat>", message);
			AssertNotContains("<asstva>", message);

			entry.EntryInstruction.ZG_BypassCode = ValuationBypassCodeList.Codes.VBC_A;
			message = deltaHelper.GetMessage(MessageType, entry);
			AssertContains("<valdou>50</valdou>", message);
			AssertContains("<valstat>50</valstat>", message);
			AssertContains("<asstva>50</asstva>", message);
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
				= @"<Gen><typeproc>C</typeproc><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
				+ "<nattrans>99</nattrans><etatMembreDestinationFinale>FR</etatMembreDestinationFinale><Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat></Bureau><Operateur><Expediteurs><Expediteur><tin>ETRANGEREoriOnly</tin><nomoperateur>GE HEALTHCARE</nomoperateur>"
				+ "<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur><paysoperateur>US</paysoperateur><codepostaloperateur>60188</codepostaloperateur><villeoperateur>STERLING</villeoperateur></Expediteur></Expediteurs><Destinataires><Destinataire><tin>FR31501335900155</tin>"
				+ "<nomoperateur>GE MEDICAL SYSTEMMS SCS</nomoperateur><rueoperateur>RUE DE LA MINIERE</rueoperateur><paysoperateur>FR</paysoperateur><codepostaloperateur>78533</codepostaloperateur><villeoperateur>BUC CEDEX</villeoperateur></Destinataire></Destinataires>"
				+ "<RepFisc><tin>FR32582075100081</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
				+ "<opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod></Operateur><prifac>4254.12</prifac><devfac>EUR</devfac><coursdevise>0.8</coursdevise><modpaiement>R</modpaiement><modgarantie>C</modgarantie>"
				+ "<ConditionsLivraison><codliv>FCA</codliv><lieuliv>STERLING</lieuliv><codelieuincoterm>3</codelieuincoterm></ConditionsLivraison><Transport><modfrotra>4</modfrotra><inttra>3</inttra><conteneurtra>0</conteneurtra><natfrotra>US</natfrotra><burfro>FRB0617A</burfro>"
				+ "<aeroportemb>ORD</aeroportemb><aertra>3</aertra></Transport><ElementsValeurGen><CumulAerienTiers><Frais><montant>124.00</montant><devfac>USD</devfac></Frais></CumulAerienTiers><AutresFraisAjout><montant>25.00</montant><devfac>USD</devfac></AutresFraisAjout></ElementsValeurGen></Gen>";

			AssertContains(messageImpGenPart, message);
		}

		public void TestPopulateArticles()
		{
			CreateDeclarationMockDNM(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();

			ZString messageImpGenArticlesPart = @"<Articles>" +
						"<Article>" +
							"<numart>1</numart>" +
							"<nomenc>8537109170</nomenc>" +
							"<Dispoparts>" +
								"<dispopart>Y053</dispopart>" +
								"<dispopart>Y069</dispopart>" +
								"<dispopart>Y949</dispopart>" +
							"</Dispoparts>" +
							"<descom>" +
							"Tableaux, panneaux, consoles, pupitres, armoires et autres supports comportant plusieurs appareils des n° 8535 ou 8536, pour la commande ou la distribution electrique, y compris ceux incorporant des instruments ou appareils du chapitre 90 ainsi que les apparei" +
							"</descom>" +
							"<msb>1.172411</msb>" +
							"<msn>1.035241</msn>" +
							"<ori>US</ori>" +
							"<pro>US</pro>" +
							"<valevaluation>1</valevaluation>" +
							"<UniSpe><unispe>NAR</unispe>" +
								"<nbrunispe>1</nbrunispe>" +
							"</UniSpe>" +
								"<RegimeDouanier>" +
									"<regdou>40</regdou>" +
									"<regdoupre>00</regdoupre>" +
									"<compcom>000</compcom>" +
							"</RegimeDouanier>" +
							"<RegimeEco>" +
								"<DecEcos>" +
									"<DecEco>" +
										"<refdec>CLE_0002</refdec>" +
										"<typedececo>3</typedececo>" +
									"</DecEco>" +
									"<DecEco>" +
										"<refdec>PRE_004</refdec>" +
										"<typedececo>5</typedececo>" +
									"</DecEco>" +
								"</DecEcos>" +
								"<montantgar>24</montantgar>" +
								"<delapur>12</delapur>" +
							"</RegimeEco>" +
							"<Preference><preftar1>1</preftar1><preftar2>00</preftar2></Preference>" +
							"<Colisage>" +
								"<nbrcol>1</nbrcol>" +
								"<nbrpieces>0</nbrpieces>" +
								"<natcol>CT</natcol>" +
								"<marquecolis>Test0123456789abcdefghijkl1122334455667788</marquecolis>" +
							"</Colisage>" +
							"<PriseEnCharge>" +
								"<natdocpec>741</natdocpec>" +
								"<typdocpec>Z</typdocpec>" +
								"<refdocpec>05781504706</refdocpec>" +
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
											"<doc>N741</doc>" +
												"<refdoc>05781504706</refdoc>" +
												"<datdoc>05/03/2019</datdoc>" +
												"<indd48>0</indd48>" +
										"</Document>" +
										"<Document>" +
											"<doc>N740</doc>" +
											"<refdoc>8461132</refdoc>" +
											"<datdoc>05/03/2019</datdoc>" +
											"<indd48>0</indd48>" +
										"</Document>" +
										"<Document>" +
											"<doc>N380</doc>" +
											"<refdoc>293337842</refdoc>" +
											"<datdoc>05/03/2019</datdoc>" +
											"<indd48>0</indd48>" +
										"</Document>" +
										"</Documents>" +
											"<DonneesFinancieres>" +
												"<prifac>1101.01</prifac>" +
												"<devfac>EUR</devfac>" +
												"<valstat>50</valstat>" +
												"<valdou>61</valdou>" +
												"<asstva>71</asstva>" +
											"</DonneesFinancieres>" +
											"<depliv>95</depliv>" +
											"<ajuval>0</ajuval>" +
											"<LignesPrecalcs>" +
												"<LignePrecalc>" +
													"<codtax>A445</codtax>" +
													"<typtax>0</typtax>" +
													"<quotax>20.000</quotax>" +
													"<asstax>401</asstax>" +
													"<montanttax>81</montanttax>" +
												"</LignePrecalc>" +
												"<LignePrecalc>" +
													"<UniSpe>" +
														"<unispe>DTN</unispe>" +
														"<qualifunispe>1</qualifunispe>" +
														"<nbrunispe>50</nbrunispe>" +
													"</UniSpe>" +
													"<codtax>U165</codtax>" +
													"<typtax>0</typtax>" +
													"<quotax>12</quotax>" +
													"<asstax>400</asstax>" +
													"<montanttax>48</montanttax>" +
												"</LignePrecalc>" +
												"<LignePrecalc>" +
													"<codtax>V905</codtax>" +
													"<typtax>2</typtax>" +
													"<quotax>1</quotax>" +
													"<asstax>2800</asstax>" +
													"<montanttax>2800</montanttax>" +
													"<codeport>810</codeport>" +
												"</LignePrecalc>" +
											"</LignesPrecalcs>" +
											"<TaxSpes>" +
												"<TaxSpe>" +
												"<unispe>HLT</unispe><qualifunispe>1</qualifunispe><nbrunispe>150</nbrunispe></TaxSpe>" +
												"</TaxSpes>" +
												"<EcoSpec>" +
													"<natperf>inwardTest</natperf>" +
													"<description>ApplicantDescription</description>" +
													"<conditions>ApplicantConditions</conditions>" +
													"<burapur>ApplicantPurOffice</burapur>" +
													"<lieuperf>ApplicantInwardLocation</lieuperf>" +
													"<formalitestransf>ApplicantTransFormality</formalitestransf>" +
												"</EcoSpec>" +
							"</Article>" +
						"<Article>" +
							"<numart>2</numart>" +
							"<nomenc>8544200090</nomenc>" +
							"<descom>" +
							"Fils, cables (y compris les cables coaxiaux) et autres conducteurs isoles pour l electricite (meme laques ou oxydes anodiquement), munis ou non de pieces de connexion; cables de fibres optiques, constitues de fibres gainees individuellement, meme comportant de" +
							"</descom>" +
							"<msb>3.357589</msb><msn>2.964759</msn><ori>US</ori><pro>US</pro><valevaluation>1</valevaluation>" +
							"<RegimeDouanier><regdou>40</regdou><regdoupre>00</regdoupre><compcom>000</compcom></RegimeDouanier>" +
							"<RegimeEco><delapur>0</delapur></RegimeEco><Preference><preftar1>1</preftar1><preftar2>00</preftar2></Preference>" +
							"<Colisage>" +
								"<nbrcol>0</nbrcol>" +
								"<nbrpieces>0</nbrpieces>" +
								"<natcol>CT</natcol>" +
								"<marquecolis>Test0123456789abcdefghijkl1122334455667788</marquecolis>" +
							"</Colisage>" +
							"<PriseEnCharge><natdocpec>741</natdocpec><typdocpec>Z</typdocpec><refdocpec>05781504706</refdocpec></PriseEnCharge>" +
							"<Documents>" +
								"<Document><doc>N787</doc><refdoc>N° GROS 61AF137</refdoc><datdoc>06/03/2019</datdoc><indd48>0</indd48>" +
								"</Document>" +
								"<Document>" +
								"<doc>N741</doc><refdoc>05781504706</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
							"</Document>" +
								"<Document>" +
								"<doc>N740</doc><refdoc>8461132</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
								"</Document>" +
								"<Document><doc>N380</doc><refdoc>293337842</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
								"</Document>" +
							"</Documents>" +
							"<DonneesFinancieres>" +
							"<prifac>3153.11</prifac><devfac>EUR</devfac><valstat>50</valstat><valdou>61</valdou><asstva>71</asstva>" +
							"</DonneesFinancieres>" +
							"<depliv>95</depliv><ajuval>0</ajuval>" +
							"<LignesPrecalcs>" +
								"<LignePrecalc>" +
									"<codtax>A455</codtax><typtax>0</typtax><quotax>20</quotax>" +
									"<asstax>12</asstax><montanttax>0</montanttax>" +
								"</LignePrecalc>" +
							"</LignesPrecalcs>" +
							"<TaxSpes>" +
							"<TaxSpe><unispe>HLT</unispe><qualifunispe>1</qualifunispe><nbrunispe>275</nbrunispe></TaxSpe>" +
							"</TaxSpes>" +
							"<EcoSpec>" +
							"<natperf>inwardTest</natperf><description>ApplicantDescription</description><conditions>ApplicantConditions</conditions>" +
							"<burapur>ApplicantPurOffice</burapur><lieuperf>ApplicantInwardLocation</lieuperf><formalitestransf>ApplicantTransFormality</formalitestransf>" +
							"</EcoSpec>" +
						"</Article>" +
					"</Articles>";

			AssertContains("DCSendImportMessage", messageImpGenArticlesPart, message);
		}

		public void TestPopulateArticles_DepartmentNodeIsNotWrittenOutForDROMCountries()
		{
			foreach (var countryCode in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				if (countryCode != Core.Constants.CountryCodes.France)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						CreateDeclarationMock(MessageType, IsImport);

						var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
						var message = messageBuilder.GetMessage();
						AssertContains("Parent node of <depliv> is successfully populated.", "<Article>", message);
						AssertNotContains("<depliv> is not populated for DROMs", "<depliv>", message);
					}
				}
			}
		}

		public void TestPopulateArticles_DepartmentNodeIsWrittenOutForFrance()
		{
			AssertEquals("Pre-requisite, country code if current company is FR", Core.Constants.CountryCodes.France, GlbCompany.CurrentCompany.Country.Code);

			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("Parent node of <depliv> is successfully populated.", "<Article>", message);
			AssertContains("<depliv> is populated for FR", "<depliv>", message);
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

		public void TestPopulateNoEcoSpec_ArticlesImpEmptyApplicant()
		{
			CreateDeclarationMock(MessageType, IsImport);

			myDeclGenArticle1.Setup(m => m.EntryNumber).Returns(new ZShort(1));
			myDeclGenArticle1.Setup(m => m.ApplicantInwardNature).Returns(ZString.Empty);
			myDeclGenArticle1.Setup(m => m.ApplicantDescription).Returns(ZString.Empty);
			myDeclGenArticle1.Setup(m => m.ApplicantConditions).Returns(ZString.Empty);
			myDeclGenArticle1.Setup(m => m.ApplicantPurOffice).Returns(ZString.Empty);
			myDeclGenArticle1.Setup(m => m.ApplicantInwardLocation).Returns(ZString.Empty);
			myDeclGenArticle1.Setup(m => m.ApplicantTransFormality).Returns(ZString.Empty);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not contain element EcoSpec for empty Applicant", "<EcoSpec>", message);
		}

		public void TestPopulateNoEcoSpec_IsPlacingGoodsUnderBW()
		{
			CreateDeclarationMock(MessageType, IsImport);
			myDeclGenArticle1.Setup(m => m.IsPlacingGoodsUnderBW).Returns(true);
			myDeclGenArticle1.Setup(m => m.HasSpecificRegimeAuthorisation).Returns(true);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertNotContains("Should not contain element EcoSpec for IsPlacingGoodsUnderBW(71P) and also HasSpecificRegimeAuthorisation", "<EcoSpec>", message);

			myDeclGenArticle1.Setup(m => m.HasSpecificRegimeAuthorisation).Returns(false);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertContains("Should contain element EcoSpec for entry when no SpecificRegimeAuthorisation though IsPlacingGoodsUnderBW(71P)", "<EcoSpec>", message);

			myDeclGenArticle1.Setup(m => m.IsPlacingGoodsUnderBW).Returns(false);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertContains("Should contain element EcoSpec for entry other than IsPlacingGoodsUnderBW(71P)", "<EcoSpec>", message);
		}

		public void TestElementsValeurGenIsNotPresentWhenValuationByPassCodeIsNotEmpty()
		{
			CreateDeclarationMock(MessageType, IsImport);
			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("<ElementsValeurGen>", message);

			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ValuationBypassCodeList.Codes.VBC_A);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("<ElementsValeurGen>", message);
		}

		public void TestFraisArticleAjoutAndFraisArticleDeduit_PresentWhenValuationByPassCodeAndTariffByPassCodeAreEmpty()
		{
			CreateDeclarationMock(MessageType, IsImport);
			myDeclGenArticle1.Setup(m => m.PackingCosts).Returns(AmountAndCurrencyMock(new ZDecimal("25.00"), "USD"));
			myDeclGenArticle1.Setup(m => m.AssemblyCosts).Returns(AmountAndCurrencyMock(new ZDecimal("25.00"), "USD"));
			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("ValuationByPassCode and TariffByPassCode are Empty", "<FraisArticleAjout>", message);
			AssertContains("ValuationByPassCode and TariffByPassCode are Empty", "<FraisArticleDeduit>", message);

			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ValuationBypassCodeList.Codes.VBC_A);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("ValuationByPassCode is NOT empty but TariffByPassCode is Empty", "<FraisArticleAjout>", message);
			AssertNotContains("ValuationByPassCode is NOT empty but TariffByPassCode is Empty", "<FraisArticleDeduit>", message);

			var myImpAlternateCalcValue = new Mock<IAlternateCalcValue> { CallBase = true };
			myImpAlternateCalcValue.Setup(m => m.CalcValue).Returns("E");
			myImpAlternateCalcValue.Setup(m => m.Motivation).Returns(new ZString(null));
			myDeclGenArticle1.Setup(m => m.AlternateCalcValue).Returns(myImpAlternateCalcValue.Object);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("ValuationByPassCode is NOT empty and TariffByPassCode is NOT Empty", "<FraisArticleAjout>", message);
			AssertNotContains("ValuationByPassCode is NOT empty and TariffByPassCode is NOT Empty", "<FraisArticleDeduit>", message);

			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("ValuationByPassCode is empty but TariffByPassCode is NOT Empty", "<FraisArticleAjout>", message);
			AssertNotContains("ValuationByPassCode is empty but TariffByPassCode is NOT Empty", "<FraisArticleDeduit>", message);
		}
	}
}
