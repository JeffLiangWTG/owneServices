using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCVALSendExpMessageBuilderTest : DCSendMessageBuilderTest<DCVALSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => EntryActionCodeList.Codes.VAL;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>2</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}

		public void TestTinNotExportedIfNotEu_Export()
		{
			var buyerEU = Factory.NewWithValidTestData<OrgHeader>();
			buyerEU.OH_Code = "BUYERFR";
			buyerEU.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "404336911", Core.Constants.CountryCodes.France);

			var supplierEU = Factory.NewWithValidTestData<OrgHeader>();
			supplierEU.OH_Code = "SUPPLIERFR";
			supplierEU.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "752845621", Core.Constants.CountryCodes.France);

			var buyerUK = Factory.NewWithValidTestData<OrgHeader>();
			buyerUK.OH_Code = "BUYERUK";
			buyerUK.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "404336911", Core.Constants.CountryCodes.UnitedKingdom);

			var supplierUK = Factory.NewWithValidTestData<OrgHeader>();
			supplierUK.OH_Code = "SUPPLIERUK";
			supplierUK.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "752845621", Core.Constants.CountryCodes.UnitedKingdom);

			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(true, false);
			var invoice = entry.MergedLines[0].InvoiceLines[0].InvoiceHeader;
			invoice.JZ_OH_Buyer = buyerEU.PK;
			entry.Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierEU.MainAddress.PK;
			Factory.Save();

			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("Expected tin value when buyer belongs to European Union", "<Destinataire><tin>FR404336911</tin>", message);
			AssertContains("Expected tin value when supplier belongs to European Union", "<Expediteur><tin>FR752845621</tin>", message);

			invoice.JZ_OH_Buyer = buyerUK.PK;
			entry.Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierUK.MainAddress.PK;
			entry.Declaration.JE_OH_Buyer = buyerUK.PK;

			message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertNotContains("tin value must not be exported when buyer belongs to UK", "<Destinataire><tin>", message);
			AssertNotContains("tin value must not be exported when supplier belongs to UK", "<Expediteur><tin>", message);
		}

		public void TestNoMessageTvaCodeNotFoundError_ProcedureOperatorRepFiscIsNull()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			AssertEquals("No MessageTvaCodeNotFound error in DCSendExpMessageBaseBuilder", false, deltaHelper.GetMessageErrorsForTest(true, false).Contains(MessageBuilderHelper.MessageTvaCodeNotFound));
		}

		public void TestNoExitTagWhenOfficeOfExitDiffersFromOfficeOfLodgement()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(true, false);
			entry.Declaration.JE_ExportExitType = ExportExitTypeList.Codes.ECS;
			var message = deltaHelper.GetMessage(MessageType, entry);

			AssertEquals(false, entry.Declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);
			AssertContains("<Sortie>", message);

			var officeOfExit = entry.Declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);
			officeOfExit.CY_Data = "FRXXXXX";
			var message2 = deltaHelper.GetMessage(MessageType, entry);
			AssertEquals(true, entry.Declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);
			AssertNotContains("<Sortie>", message2);
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

			var messageExpGenPart
				= @"<Gen><typeproc>C</typeproc><procedure1>IM</procedure1><procedure2>A</procedure2><nbrart>2</nbrart><nbrcol>1</nbrcol><locagr>34796082500052/1</locagr><magasin>CDGSO1</magasin>"
+ "<nattrans>99</nattrans><dest>GB</dest><etatMembreExportationReel>DE</etatMembreExportationReel><Bureau><burdom>FRB0617A</burdom><burrat>FRB0617A</burrat><bureausortie>AT330100</bureausortie><Sortie><typesortie>STC</typesortie><motiv>BLABLA</motiv></Sortie></Bureau><Operateur><Expediteurs><Expediteur><tin>ETRANGER</tin><nomoperateur>GE HEALTHCARE</nomoperateur>"
+ "<rueoperateur>775 EAST DRIVE DOCK DOORS</rueoperateur><paysoperateur>US</paysoperateur><codepostaloperateur>60188</codepostaloperateur><villeoperateur>STERLING</villeoperateur></Expediteur></Expediteurs><Destinataires><Destinataire><tin>FR31501335900156</tin>"
+ "<nomoperateur>GE MEDICAL SYSTEMMS SCS</nomoperateur><rueoperateur>RUE DE LA MINIERE</rueoperateur><paysoperateur>FR</paysoperateur><codepostaloperateur>78533</codepostaloperateur><villeoperateur>BUC CEDEX</villeoperateur></Destinataire></Destinataires><RepFisc><tin>"
+ "FR32582075100080</tin><nomoperateur>Cosfibel HK Limited</nomoperateur><rueoperateur>FLAT CTOD 3FL SUMWAY MANSION</rueoperateur><paysoperateur>HK</paysoperateur><codepostaloperateur>55102</codepostaloperateur><villeoperateur>KENNEDY TOWN</villeoperateur></RepFisc>"
+ "<opeben>FR34430738400570</opeben><numagr>00000169</numagr><operep>FR34430738400570</operep><modrep>2</modrep><numcre>ALLI</numcre><numcod>ALLI</numcod></Operateur><prifac>4254.12</prifac><devfac>EUR</devfac><coursdevise>0.8</coursdevise><modpaiement>R</modpaiement><modgarantie>C</modgarantie>"
+ "<ConditionsLivraison><codliv>FCA</codliv><lieuliv>STERLING</lieuliv><codelieuincoterm>3</codelieuincoterm></ConditionsLivraison><Transport><modfrotra>4</modfrotra><inttra>3</inttra><conteneurtra>0</conteneurtra><idtransport>ID12345</idtransport><natfrotra>US</natfrotra><transportMethodPayment>B</transportMethodPayment></Transport></Gen>";

			AssertContains(messageExpGenPart, message);
		}

		public void TestPopulateArticles()
		{
			CreateDeclarationMockDNM(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();

			ZString messageExpGenArticlesPart = "<Articles>" +
				"<Article>" +
				"<numart>1</numart><nomenc>8537109170</nomenc>" +
				"<Dispoparts>" +
				"<dispopart>Y053</dispopart><dispopart>Y069</dispopart><dispopart>Y949</dispopart>" +
				"</Dispoparts>" +
				"<descom>" +
				"Tableaux, panneaux, consoles, pupitres, armoires et autres supports comportant plusieurs appareils des n° 8535 ou 8536, pour la commande ou la distribution electrique, y compris ceux incorporant des instruments ou appareils du chapitre 90 ainsi que les apparei" +
				"</descom>" +
				"<msb>1.172411</msb><msn>1.035241</msn><ori>US</ori><scelles>1</scelles>" +
				"<Scelleidentites><scelleidentite>123</scelleidentite></Scelleidentites>" +
				"<valevaluation>1</valevaluation><UniSpe><unispe>NAR</unispe><nbrunispe>1</nbrunispe></UniSpe>" +
				"<RegimeDouanier><regdou>40</regdou><regdoupre>00</regdoupre><compcom>000</compcom></RegimeDouanier>" +
				"<RegimeEco>" +
				"<DecEcos><DecEco><refdec>CLE_0002</refdec><typedececo>3</typedececo></DecEco><DecEco><refdec>PRE_004</refdec><typedececo>5</typedececo></DecEco></DecEcos><delapur>12</delapur>" +
				"</RegimeEco>" +
				"<Colisage>" +
					"<nbrcol>1</nbrcol>" +
					"<nbrpieces>0</nbrpieces>" +
					"<natcol>CT</natcol>" +
					"<marquecolis>Test0123456789abcdefghijkl1122334455667788</marquecolis>" +
					"</Colisage>" +
				"<PriseEnCharge><natdocpec>741</natdocpec><typdocpec>Z</typdocpec><refdocpec>05781504706</refdocpec></PriseEnCharge>" +
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
					"<Document><doc>N380</doc><refdoc>293337842</refdoc><datdoc>05/03/2019</datdoc><indd48>0</indd48>" +
					"</Document>" +
				"</Documents>" +
				"<Facturation><prifac>1101.01</prifac><devfac>EUR</devfac><coursdevise>0</coursdevise></Facturation>" +
				"<DonneesFinancieres><valdou>61</valdou><valstat>50</valstat><devfac>EUR</devfac><transportMethodPayment>B</transportMethodPayment></DonneesFinancieres>" +
				"<DonneesStat><depexp>12</depexp></DonneesStat>" +
				"<LignesPrecalcs>" +
				"<LignePrecalc><codtax>A445</codtax><typtax>0</typtax><quotax>20.000</quotax><asstax>401</asstax><montanttax>81</montanttax></LignePrecalc><LignePrecalc><UniSpe><unispe>DTN</unispe><qualifunispe>1</qualifunispe><nbrunispe>50</nbrunispe></UniSpe><codtax>U165</codtax><typtax>0</typtax><quotax>12</quotax><asstax>400</asstax><montanttax>48</montanttax></LignePrecalc><LignePrecalc><codtax>V905</codtax><typtax>2</typtax><quotax>1</quotax><asstax>2800</asstax><montanttax>2800</montanttax><codeport>810</codeport></LignePrecalc>" +
				"</LignesPrecalcs>" +
				"<TaxSpes><TaxSpe><unispe>HLT</unispe><qualifunispe>1</qualifunispe><nbrunispe>150</nbrunispe></TaxSpe></TaxSpes>" +
				"<EcoSpec>" +
				"<natperf>inwardTest</natperf><description>ApplicantDescription</description><conditions>ApplicantConditions</conditions><burapur>ApplicantPurOffice</burapur><lieuperf>ApplicantInwardLocation</lieuperf><formalitestransf>ApplicantTransFormality</formalitestransf>" +
				"</EcoSpec>" +
				"<DangerousGoodsDeltas><dangerousGoodsDelta>3</dangerousGoodsDelta><dangerousGoodsDelta>4</dangerousGoodsDelta></DangerousGoodsDeltas>" +
				"</Article>" +
				"<Article>" +
				"<numart>2</numart><nomenc>8544200090</nomenc>" +
					"<descom>" +
						"Fils, cables (y compris les cables coaxiaux) et autres conducteurs isoles pour l electricite (meme laques ou oxydes anodiquement), munis ou non de pieces de connexion; cables de fibres optiques, constitues de fibres gainees individuellement, meme comportant de" +
					"</descom>" +
					"<msb>3.357589</msb><msn>2.964759</msn><ori>US</ori><scelles>1</scelles><Scelleidentites><scelleidentite>123</scelleidentite></Scelleidentites>" +
					"<valevaluation>1</valevaluation><RegimeDouanier><regdou>40</regdou><regdoupre>00</regdoupre><compcom>000</compcom></RegimeDouanier>" +
					"<RegimeEco><delapur>0</delapur></RegimeEco>" +
					"<Colisage>" +
						"<nbrcol>0</nbrcol>" +
						"<nbrpieces>0</nbrpieces>" +
						"<natcol>CT</natcol>" +
						"<marquecolis>Test0123456789abcdefghijkl1122334455667788</marquecolis>" +
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
					"<Facturation><prifac>3153.11</prifac><devfac>EUR</devfac><coursdevise>0</coursdevise></Facturation>" +
					"<DonneesFinancieres><valdou>61</valdou><valstat>50</valstat><devfac>EUR</devfac><transportMethodPayment>B</transportMethodPayment></DonneesFinancieres>" +
					"<DonneesStat><depexp>12</depexp></DonneesStat>" +
					"<LignesPrecalcs>" +
						"<LignePrecalc>" +
							"<codtax>A455</codtax><typtax>0</typtax><quotax>20</quotax><asstax>12</asstax><montanttax>0</montanttax>" +
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

			AssertContains(messageExpGenArticlesPart, message);
			AssertContains("EcoSpec", message);
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
						AssertContains("Parent node of <depexp> is successfully populated.", "<Article>", message);
						AssertNotContains("<depexp> is not populated for DROMs", "<depexp>", message);
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
			AssertContains("Parent node of <depexp> is successfully populated.", "<Article>", message);
			AssertContains("<depexp> is populated for FR", "<depexp>", message);
		}

		public void TestVatCustomsStatisticalValues()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(true, false);
			entry.EntryInstruction.ZG_BypassCode = ZString.Empty;

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = EntryActionCodeList.Codes.VAL;
			var errCollector = new ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			var message = messageBuilder.GetMessage();

			AssertContains("<valdou>50</valdou>", message);
			AssertContains("<valstat>50</valstat>", message);
			AssertNotContains("<asstva>", message);
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
			myDeclGenArticle1.Setup(m => m.CETariffAdditionalCodes).Returns(new[] { myCETariffAdditionalCode.Object });
			myDeclGenArticle1.Setup(m => m.FRTariffAdditionalCodes).Returns(new[] { myFRTariffAdditionalCode.Object });
			myDeclGenArticle1.Setup(m => m.PartDispos).Returns(new[] { myPartDispos.Object });
			myDeclGenArticle1.Setup(m => m.QuotaRefNumber).Returns(new ZString[] { "test cnts" });
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object });
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = Extensions.GetFlatXml(messageBuilder.GetMessage());
			AssertContains("<Cacos><caco>12345</caco></Cacos>", message);
			AssertContains("<Canas><cana>54321</cana></Canas>", message);
			AssertContains("<Dispoparts><dispopart>67890</dispopart></Dispoparts>", message);
			AssertNotContains("<Cnts>", message);
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
			AssertContains("<RegimeEco><DecEcos><DecEco><refdec>CLE_0002</refdec><typedececo>3</typedececo></DecEco><DecEco><refdec>PRE_004</refdec><typedececo>5</typedececo></DecEco></DecEcos><delapur>12</delapur></RegimeEco>", message);
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

		public void TestNoErrorAnymoreWhenPartnerIdNotFound()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var errorCollector = new ErrorCollector();
			var messageEnveloppeMock = new Mock<IMessageEnvelope>();
			messageEnveloppeMock.Setup(m => m.SchemaID).Returns("MessageCDecExp");
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
	}
}
