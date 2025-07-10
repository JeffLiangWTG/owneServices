using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDSendImpMessageBaseBuilder : MessageBuilderBase<TMessage>
	{
		public DDSendImpMessageBaseBuilder(IDeclarationImportExport declarationImport
			, EU.Business.ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
		{
			itemIDDImport = declarationImport;
			errorCollector = errorCollectorObject;
			messageTransactionType = transactionType;
			this.newSequenceNumeric = newSequenceNumeric;
			messageBuilderHelper = new MessageBuilderHelper();
		}

		protected override TMessage GenerateDeclarationMessage()
		{
			if (errorCollector.ErrorCount > 0)//Todo:del after implementqtrion class)
			{ }

			var message = new TMessage();

			if (messageTransactionType == TransactionTypes.Original)
			{
				message.EnveloppeMessage = PopulateEnvelopMessage();
				PopulateDeclaration(message);
			}

			return message;
		}

		#region Populate message

		protected void PopulateDeclaration(TMessage message)
		{
			message.Declaration = new Declaration();

			if (ShouldPopulateMainDatasInfos)
			{
				message.Declaration.Dsi = PopulateMainDatasInfos();
			}
			else
			{
				message.Declaration.DsiComp = PopulateLiquidationInfos();
			}
		}

		protected virtual bool ShouldPopulateMainDatasInfos => true;

		Tdsi PopulateMainDatasInfos()
		{
			var datasDec = new Tdsi();
			datasDec.Entete = PopulateHeader();
			datasDec.Gen = PopulateProcedure();
			datasDec.Articles = PopulateArticlesImport();
			return datasDec;
		}

		TdsiComp PopulateLiquidationInfos()
		{
			var liquidationSection = new TdsiComp();
			liquidationSection.Entete = PopulateHeader();
			liquidationSection.GenComp = PopulateGenComp();
			liquidationSection.ArticlesComp = PopulateArticlesComp();
			return liquidationSection;
		}

		TEnveloppeMessage PopulateEnvelopMessage()
		{
			TEnveloppeMessage envelopeMessage = null;
			var messageEnvelope = itemIDDImport?.MessageEnvelope;
			if (messageEnvelope != null)
			{
				envelopeMessage = new TEnveloppeMessage();
				envelopeMessage.SchemaId = itemIDDImport.MessageEnvelope.SchemaID;
				envelopeMessage.SchemaVersion = itemIDDImport.MessageEnvelope.SchemaVersion;
				if (!messageEnvelope.PartnerId.IsEmpty)
				{
					envelopeMessage.PartyId = messageEnvelope.PartnerId;
				}
				envelopeMessage.TransactionId = itemIDDImport.MessageEnvelope.TransactionId;
				envelopeMessage.Numseq = (short)newSequenceNumeric;
			}

			return envelopeMessage;
		}

		protected virtual TEntete PopulateHeader()
		{
			if (itemIDDImport?.Header == null)
			{
				return null;
			}
			var motivationHeader = itemIDDImport?.Header;
			var motivationItem = PopulateMotivation(motivationHeader);

			var retConvert = Enum.TryParse(motivationHeader?.ActionCode ?? string.Empty, out CodeAct codActEnum) && Enum.IsDefined(typeof(CodeAct), codActEnum);
			var header = new TEntete();
			header.Codact = retConvert ? codActEnum : CodeAct.Item0;
			header.Refdos = motivationHeader?.References?.OwnerDeclarationIdentification.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText) ?? string.Empty;

			#region Optional properties

			header.Motivation = motivationItem;

			header.Refdec = PopulateRefdecItem(motivationHeader);
			#endregion

			return header;
		}

		protected virtual string PopulateRefdecItem(IHeader motivationHeader)
		{
			var refdecItem = motivationHeader?.References?.CusDeclarationNumber;
			return MessageBuilderHelper.GetOptionalString(refdecItem);
		}

		protected virtual TMotivation PopulateMotivation(IHeader motivationHeader)
		{
			var motivationHeaderMotivation = motivationHeader?.Motivation;
			var motivationItem = new TMotivation();
			if (motivationHeaderMotivation == null)
			{
				motivationItem.Commentaire = string.Empty;
				motivationItem.Justifreg = string.Empty;
				motivationItem.Motiv = string.Empty;
				motivationItem.Nouvelledest = string.Empty;
			}
			else
			{
				var commentaire = motivationHeaderMotivation.Comment;
				motivationItem.Commentaire =
					commentaire.IsEmpty ?
					(ZString)"."
					: commentaire.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

				motivationItem.Justifreg = motivationHeaderMotivation.RegularJustification.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Motiv = motivationHeaderMotivation.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Nouvelledest = motivationHeaderMotivation.NewDestination.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			}

			return motivationItem;
		}
		#endregion

		#region Procedure Gen

		protected virtual TGenDsi PopulateProcedure()
		{
			#region Tests messages objects

			var cusProcedure = itemIDDImport?.CusProcedure;

			if (cusProcedure?.Office == null)
			{
				return null;
			}

			#endregion

			var genImport = LoadProcedureBase(cusProcedure);

			LoadProcedureOfficeAndOperator(genImport, cusProcedure);

			LoadProcedurePreval(genImport, cusProcedure);

			LoadProcedureOperator(genImport, cusProcedure);

			LoadProcedureDeliveryAndOthersCost(genImport, cusProcedure);

			return genImport;
		}

		TGenDsi LoadProcedureBase(ICusProcedure cusProcedure)
		{
			var genImport = new TGenDsi();
			genImport.Procedure1 = cusProcedure.EntryStyle;
			genImport.Procedure2 = cusProcedure.EntryStyleCode;
			genImport.Nbrart = cusProcedure.ArticleCount;
			return genImport;
		}

		void LoadProcedureOfficeAndOperator(TGenDsi genImport, ICusProcedure cusProcedure)
		{
			genImport.Bureau = new TBureau();

			genImport.Bureau.Burdom = cusProcedure.Office.OfficeOfDeclaration;
			genImport.Bureau.Burrat = cusProcedure.Office.OfficeOfLodgement;
		}

		void LoadProcedurePreval(TGenDsi genImport, ICusProcedure cusProcedure)
		{
			var prevalItem = PopulatePreval(cusProcedure);

			genImport.Preval = string.IsNullOrEmpty(prevalItem?.Datpreval ?? ZString.Empty)
									? null
									: prevalItem;

			genImport.Locagr = MessageBuilderHelper.GetOptionalString(cusProcedure.AgreedGoodsLocation);

			genImport.Magasin = MessageBuilderHelper.GetOptionalString(cusProcedure.ClearanceLocation);

			genImport.Nbrcol = cusProcedure.PackageCount;

			genImport.EtatMembreDestinationFinale = MessageBuilderHelper.GetOptionalString(cusProcedure.DestinationState);
		}

		protected virtual TPreval PopulatePreval(ICusProcedure cusProcedure)
		{
			return LoadPreval(cusProcedure);
		}

		TPreval LoadPreval(ICusProcedure cusProcedure)
		{
			var prevalItem = new TPreval()
			{
				Datpreval = cusProcedure.EstimatedAssessmentDate,
				Heurpreval = cusProcedure.EstimatedAssessmentHour,
			};
			return prevalItem;
		}

		void LoadProcedureOperator(TGenDsi genImport, ICusProcedure cusProcedure)
		{
			var operateur = new TOperateur();

			operateur.Opeben = cusProcedure.AgreementOwnerEORI;
			operateur.Opedest = cusProcedure.ImporterEORINumber;
			operateur.Numagr = cusProcedure.DeltaGAuthorisationNumber;
			operateur.Operep = cusProcedure.BranchCusBrokerageCode;
			operateur.Modrep = cusProcedure.RepresentationModeCode;

			var suppliersItem = cusProcedure.Suppliers.FirstOrDefault();

			operateur.Expediteur = LoadTrader(suppliersItem);

			var repTaxOrganisationItem = cusProcedure.RepTaxOrganisation;

			operateur.RepFisc = LoadTrader(repTaxOrganisationItem);

			operateur.Numcre = MessageBuilderHelper.GetOptionalString(cusProcedure.DeferalApprovalCreditNumber);

			operateur.Numcod = MessageBuilderHelper.GetOptionalString(cusProcedure.VariousOperationCreditNumber);

			genImport.Operateur = operateur;
		}

		void LoadProcedureDeliveryAndOthersCost(TGenDsi genImport, ICusProcedure cusProcedure)
		{
			string dateDepotItem = PopulateDeclEmergencyProcDate(cusProcedure);
			genImport.Datdepot = MessageBuilderHelper.GetOptionalString(dateDepotItem);
		}

		protected virtual ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure)
		{
			return LoadDeclEmergencyProcDate(cusProcedure);
		}

		ZString LoadDeclEmergencyProcDate(ICusProcedure cusProcedure)
		{
			return MessageBuilderHelper.GetOptionalString(cusProcedure.DeclEmergencyProcDate);
		}
		#endregion

		#region Article

		protected virtual Collection<TArticleDsi> PopulateArticlesImport()
		{
			#region Tests messages objects

			if (itemIDDImport?.Articles == null)
			{
				return new Collection<TArticleDsi>();
			}

			if (!itemIDDImport.Articles.Any())
			{
				return new Collection<TArticleDsi>();
			}

			#endregion

			var genArticlesImport = new Collection<TArticleDsi>();

			foreach (var article in itemIDDImport.Articles)
			{
				var articleImport = LoadArticleBase(article);

				LoadArticleAdditionalCodes(articleImport, article);

				LoadArticleEconomicRegime(articleImport, article);

				LoadArticleSupplementaryUnitAndWarehouse(articleImport, article);

				LoadArticleEconomicPreference(articleImport, article);

				#region Packing And Supporting document

				LoadPacking(articleImport, article);

				LoadSupportingDocument(articleImport, article);

				#endregion

				LoadArticleAutorisationEco(articleImport, article);

				genArticlesImport.Add(articleImport);
			}

			return genArticlesImport;
		}

		TArticleDsi LoadArticleBase(IArticle article)
		{
			var articleImport = new TArticleDsi();

			articleImport.Numart = article.EntryNumber;
			articleImport.Nomenc = article.TariffCode;
			articleImport.Descom = article.EntryLineDescription.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			articleImport.Msb = article.GrossWeight.Round(0);
			articleImport.MsnValueSpecified = true;
			articleImport.Msn = article.CustomsQuantity.Round(0);
			articleImport.Ori = article.CountryGoodsOrigineCode;
			articleImport.Pro = article.CountryGoodsSupplyCode;

			var procedureItem = new TRegimeDouanier();
			procedureItem.Regdou = article.ProcedureCode;
			procedureItem.Regdoupre = article.PreviousCode;
			procedureItem.Compcom = article.Concession;
			articleImport.RegimeDouanier = procedureItem;

			var supportDocItem = new TPriseEnCharge();
			var previousDocItem = article.PreviousDocument;
			supportDocItem.Natdocpec = previousDocItem?.Code ?? string.Empty;
			supportDocItem.Typdocpec = previousDocItem?.Type ?? string.Empty;
			supportDocItem.Refdocpec = previousDocItem?.RefNumber ?? string.Empty;
			articleImport.PriseEnCharge = supportDocItem;

			var invoiceLinePriceItem = new TDonneesFinancieresDsi();
			invoiceLinePriceItem.Prifac = article.InvoiceLinePrice.Round(2);
			invoiceLinePriceItem.Devfac = article.CurrencyCode;
			if (article.ProcedureCode.StartsWith("7"))
			{
				invoiceLinePriceItem.Valdou = article.CustomsValue.Round(0).ToZInt();
			}
			articleImport.DonneesFinancieres = invoiceLinePriceItem;

			LoadArticleBaseNotRequired(articleImport, article);

			return articleImport;
		}

		void LoadArticleBaseNotRequired(TArticleDsi articleImport, IArticle article)
		{
			articleImport.RefLogistique = MessageBuilderHelper.GetOptionalString(article.ShippingIdReference);

			var horsTarifItem = new THorsTarif();

			var alternateCalcValueItem = article.AlternateCalcValue;

			horsTarifItem.Horstarif = alternateCalcValueItem?.CalcValue ?? string.Empty;
			horsTarifItem.Motiv = alternateCalcValueItem?.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText) ?? string.Empty;

			articleImport.HorsTarif = string.IsNullOrEmpty(horsTarifItem.Horstarif)
									? null
									: horsTarifItem;

			var observationsItem = article.Observation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

			articleImport.Observations = MessageBuilderHelper.GetOptionalString(observationsItem);
		}

		void LoadArticleAdditionalCodes(TArticleDsi articleImport, IArticle article)
		{
			articleImport.Cacos = messageBuilderHelper.LoadAdditionnalCodeList(article.CETariffAdditionalCodes);
			articleImport.Canas = messageBuilderHelper.LoadAdditionnalCodeList(article.FRTariffAdditionalCodes);

			var parDisposItem = GetPartDispos(article);
			articleImport.Dispoparts = messageBuilderHelper.LoadAdditionnalCodeList(parDisposItem);

			var quotaRefNumberItem = article.QuotaRefNumber;

			if (quotaRefNumberItem != null && quotaRefNumberItem.Any())
			{
				articleImport.Cnts = GetQuotaNumber(quotaRefNumberItem.ToList());
			}
		}

		protected virtual IEnumerable<ITariffAdditionalCode> GetPartDispos(IArticle article)
		{
			return article.PartDispos;
		}

		Collection<string> GetQuotaNumber(List<ZString> itemQuotaNumber)
		{
			var cnts = new List<string>();
			if (itemQuotaNumber != null && itemQuotaNumber.Any())
			{
				cnts.AddRange(itemQuotaNumber[0].ToString().Split(new char[] { ';' }));
			}

			return new Collection<string>(cnts);
		}

		void LoadArticleSupplementaryUnitAndWarehouse(TArticleDsi articleImport, IArticle article)
		{
			var unitSpeItem = new TUniSpe()
			{
				Unispe = article.SuppUnit?.Code ?? string.Empty,
				Qualifunispe = article.SuppUnit?.Qualif ?? string.Empty,
				Nbrunispe = article.SuppUnit?.Qty ?? default
			};

			articleImport.UniSpe = string.IsNullOrEmpty(unitSpeItem.Unispe) || unitSpeItem.Nbrunispe == decimal.Zero
									? null
									: unitSpeItem;

			if (itemIDDImport.HasIntoWarehouseProcedure)
			{
				var warehouseItem = new TEntrepot()
				{
					Enttyp = article.WarehouseType,
					Entref = article.WarehouseReference,
					Entpays = article.WarehouseCountryCode
				};

				articleImport.Entrepot = warehouseItem;
			}
			else
			{
				articleImport.Entrepot = null;
			}
		}

		void LoadArticleAutorisationEco(TArticleDsi articleImport, IArticle article)
		{
			var ecoRegimeAuthorizationItem = new TAutorisationEco();

			if (article.EcoRegimeAuthorization != null)
			//EcoRegimeAuthorization is null when the instruction shows no authorization of expected type.
			{
				ecoRegimeAuthorizationItem.Autorisationeco = article.EcoRegimeAuthorization.EcoRegimeAuthorizationNumber;
				ecoRegimeAuthorizationItem.Autorisationpays = article.EcoRegimeAuthorization.EcoRegimeCountryCode;

				articleImport.AutorisationEco = ecoRegimeAuthorizationItem;
			}
			else
			{
				articleImport.AutorisationEco = null;
			}
		}

		void LoadArticleEconomicRegime(TArticleDsi articleImport, IArticle article)
		{
			if (article.EcoRegimeDatas != null && article.EcoRegimeDatas != null && article.EcoRegimeDatas.DecEcos != null)
			//EcoRegimeDatas is null when the instruction shows no authorization of expected type.
			{
				var ecoRegimeDataCollection = new Collection<TDecEco>();

				foreach (var decEco in article.EcoRegimeDatas.DecEcos)
				{
					var ecoRegimeDatasItem = new TDecEco();

					ecoRegimeDatasItem.Refdec = decEco.CusDeclarationID;
					ecoRegimeDatasItem.Typedececo = decEco.EcoRegimeDeclTypeCode;

					if (!string.IsNullOrEmpty(ecoRegimeDatasItem.Refdec) || !string.IsNullOrEmpty(ecoRegimeDatasItem.Typedececo))
					{
						ecoRegimeDataCollection.Add(ecoRegimeDatasItem);
					}
				}

				var regimeEcoItem = new TRegimeEco();
				regimeEcoItem.DecEcos = ecoRegimeDataCollection;
				regimeEcoItem.Delapur = article.EcoRegimeDatas.NumberDaysOfDischarge;
				regimeEcoItem.Montantgar = article.EcoRegimeDatas.GuaranteeAmount.Round(0).ToZInt();
				regimeEcoItem.MontantgarValueSpecified = regimeEcoItem.Montantgar > 0;

				articleImport.RegimeEco = regimeEcoItem;
			}
			else
			{
				articleImport.RegimeEco = null;
			}
		}

		void LoadArticleEconomicPreference(TArticleDsi articleImport, IArticle article)
		{
			var preferenceItem = new TPreference();

			preferenceItem.Preftar1 = article.Preference?.CodePart1 ?? string.Empty;
			preferenceItem.Preftar2 = article.Preference?.CodePart2 ?? string.Empty;

			articleImport.Preference = string.IsNullOrEmpty(preferenceItem.Preftar1) && string.IsNullOrEmpty(preferenceItem.Preftar2)
									? null
									: preferenceItem;

			var cusProcedure = itemIDDImport?.CusProcedure;
			if ((cusProcedure?.Transport?.ContainerMode ?? ZString.Empty) == "1")
			{
				var numContainerCollection = new Collection<string>();
				article.Containers.Where(x => !x.IsEmpty).ForEach(x => numContainerCollection.Add(x));
				articleImport.Conteneurs = numContainerCollection;
			}
		}

		void LoadPacking(TArticleDsi articleImport, IArticle article)
		{
			var colisageItem = new TColisageImp()
			{
				Nbrcol = article.Packing?.Count ?? 0,
				Nbrpieces = article.Packing?.ItemsCount,
				Natcol = MessageBuilderHelper.GetOptionalString(article.Packing?.Type),
				Marquecolis = MessageBuilderHelper.GetOptionalString(article.Packing?.MarksAndNos.Left(MessageBuilderHelper.CW_MarksAndNosMaxLengthForMessage))
			};

			articleImport.Colisage = string.IsNullOrEmpty(colisageItem.Natcol)
									? null
									: colisageItem;
		}

		void LoadSupportingDocument(TArticleDsi articleImport, IArticle article)
		{
			articleImport.Menspecs = messageBuilderHelper.LoadMenSpecTexteCodeList<TMenspectexte>(article.SpecMens);

			var supportingDocumentsItem = GetSupportingDocumentsItem(article);
			articleImport.Documents = LoadSupportingDocList(supportingDocumentsItem);
		}

		protected virtual IEnumerable<ISupportingDocumentOnly> GetSupportingDocumentsItem(IArticle article)
		{
			return article.SupportingDocuments;
		}

		#endregion

		#region GenComp
		protected virtual TGenDsIcomp PopulateGenComp()
		{
			#region Tests messages objects

			var cusProcedure = itemIDDImport?.CusProcedure;

			if (cusProcedure?.Office == null)
			{
				return null;
			}

			#endregion

			var genCompImport = LoadGenComp(cusProcedure);

			LoadGenCompHorsValeur(genCompImport, cusProcedure);

			genCompImport.OperateurComp = LoadGenCompOperateurComp(cusProcedure);

			LoadGenCompConditionsLivraison(genCompImport, cusProcedure);

			LoadGenCompTransport(genCompImport, cusProcedure);

			if (cusProcedure.ValuationBypassCode.IsEmpty)
			{
				LoadGenCompElementsValeurGen(genCompImport, cusProcedure);
			}

			return genCompImport;
		}

		TGenDsIcomp LoadGenComp(ICusProcedure cusProcedure)
		{
			var genCompImport = new TGenDsIcomp()
			{
				Nattrans = cusProcedure.TransactionNature,
			};

			return genCompImport;
		}

		void LoadGenCompHorsValeur(TGenDsIcomp genCompImport, ICusProcedure cusProcedure)
		{
			var horsValeur = new THorsValeur
			{
				Horsvaleur = MessageBuilderHelper.GetOptionalString(cusProcedure.ValuationBypassCode),
			};

			if (horsValeur.Horsvaleur?.ToUpper() == "I")
			{
				horsValeur.Motiv = cusProcedure.ValuationBypassReason;
			}

			genCompImport.HorsValeur = string.IsNullOrEmpty(horsValeur.Horsvaleur)
									? null
									: horsValeur;
		}

		protected virtual TOperateurDsiComp LoadGenCompOperateurComp(ICusProcedure cusProcedure)
		{
			var operateurComp = new TOperateurDsiComp
			{
				Opedestfinalintracomm = MessageBuilderHelper.GetOptionalString(cusProcedure.VATOrganization),
			};

			return operateurComp;
		}

		void LoadGenCompConditionsLivraison(TGenDsIcomp genCompImport, ICusProcedure cusProcedure)
		{
			var conditionsLivraison = new TConditionsLivraison
			{
				Codliv = cusProcedure.DeliveryTerms.IncotermCode,
				Lieuliv = cusProcedure.DeliveryTerms.DeliveryPlace,
				Codelieuincoterm = cusProcedure.DeliveryTerms.IncotermPlace,
			};

			genCompImport.ConditionsLivraison = conditionsLivraison;
		}

		void LoadGenCompTransport(TGenDsIcomp genCompImport, ICusProcedure cusProcedure)
		{
			var transport = new TTransport
			{
				Inttra = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.ModeOfTRansportInland),

				Conteneurtra = LoadContainerMode(cusProcedure),

				Natfrotra = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.NationalityOfTransport),

				Burfro = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.CusOffice),
			};

			var transportMode = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.ModeOfTRansport);

			if (cusProcedure.EntryStyle != "FR")
			{
				transport.Modfrotra = transportMode;
			}

			if (transportMode == "4")
			{
				transport.Aeroportemb = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.IATAAirportOfLoading);

				transport.Aertra = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.AirRoadType);
			}

			genCompImport.Transport = transport;
		}

		protected virtual ZString LoadContainerMode(ICusProcedure cusProcedure)
		{
			return cusProcedure.Transport.ContainerMode;
		}

		void LoadGenCompElementsValeurGen(TGenDsIcomp genCompImport, ICusProcedure cusProcedure)
		{
			genCompImport.ElementsValeurGen = new TElementsValeurGen()
			{
				CumulTiers = ConvertCostAndInsurance(cusProcedure.ThirdCountryTransportCosts),
				CumulCeHorsFrInclus = ConvertCostAndInsurance(cusProcedure.EUTransportCostsInInvoice),
				CumulCeHorsFrExclus = ConvertCostAndInsurance(cusProcedure.EUTransportCostsNotInInvoice),
				CumulFrInclus = ConvertCostAndInsurance(cusProcedure.FRTransportCostsInInvoice),
				CumulFrExclus = ConvertCostAndInsurance(cusProcedure.FRTransportCostsNotInInvoice),
				CumulAerienTiers = ConvertCostAndInsurance(cusProcedure.ThirdCountryAirCosts),
				CumulAerienFr = ConvertCostAndInsurance(cusProcedure.FRAirCosts),
				AutresFraisAjout = ConvertAmountAndCurrency(cusProcedure.OthAddedCosts),
				AutresFraisDeduit = new TAutresFraisDeduit
				{
					Interets = ConvertAmountAndCurrency(cusProcedure.Interest),
					Commission = ConvertAmountAndCurrency(cusProcedure.Commission),
					FraisBaseTva = ConvertAmountAndCurrency(cusProcedure.VATBaseCosts),
				},
			};
		}

		#endregion

		#region ArticlesComp
		protected virtual Collection<TArticleDsiComp> PopulateArticlesComp()
		{
			#region Tests messages objects

			if (itemIDDImport?.Articles == null)
			{
				return new Collection<TArticleDsiComp>();
			}

			if (!itemIDDImport.Articles.Any())
			{
				return new Collection<TArticleDsiComp>();
			}

			#endregion

			var genArticlesImport = new Collection<TArticleDsiComp>();

			foreach (var article in itemIDDImport.Articles)
			{
				var articleImport = LoadArticleCompBase(article);

				LoadArticleCompAdditionnalCodes(articleImport, article);

				#region Supporting document

				LoadArticleCompSupportingDocument(articleImport, article);

				#endregion

				LoadArticleCompFinancialDatas(articleImport, article);
				LoadArticleCompElementsValeurGen(articleImport, article);
				LoadArticleCompPreCalcEntryLines(articleImport, article);
				LoadEntryHeaderCharges(articleImport, article);
				LoadArticleCompSpecialTax(articleImport, article);

				genArticlesImport.Add(articleImport);
			}

			return genArticlesImport;
		}

		TArticleDsiComp LoadArticleCompBase(IArticle article)
		{
			var articleImport = new TArticleDsiComp();

			articleImport.Numart = article.EntryNumber;

			articleImport.Depliv = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.France ? MessageBuilderHelper.GetOptionalString(article.DeliveryDepartment) : null;

			articleImport.Valevaluation = article.ValuationMethod;

			articleImport.Ajuval = article.ValuationAdjustPercent;

			LoadArticleCompBaseNotRequired(articleImport, article);

			return articleImport;
		}

		void LoadArticleCompBaseNotRequired(TArticleDsiComp articleImport, IArticle article)
		{
			var horsTarifItem = new THorsTarif();

			var alternateCalcValueItem = article.AlternateCalcValue;

			horsTarifItem.Horstarif = MessageBuilderHelper.GetOptionalString(alternateCalcValueItem?.CalcValue);
			if (horsTarifItem.Horstarif == "E")
			{
				horsTarifItem.Motiv = alternateCalcValueItem?.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText) ?? string.Empty;
			}

			articleImport.HorsTarif = string.IsNullOrEmpty(horsTarifItem.Horstarif)
									? null
									: horsTarifItem;

			var observationsItem = article.Observation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

			articleImport.Observations = MessageBuilderHelper.GetOptionalString(observationsItem);
		}

		void LoadArticleCompAdditionnalCodes(TArticleDsiComp articleImport, IArticle article)
		{
			var additionalCECodeItem = GetCETariffAdditionalCodes(article);
			articleImport.Cacos = LoadAdditionnalCodeList(additionalCECodeItem);

			var additionalFRCodeItem = GetFRTariffAdditionalCodes(article);
			articleImport.Canas = LoadAdditionnalCodeList(additionalFRCodeItem);

			var parDisposItem = GetPartDispos(article);
			articleImport.Dispoparts = LoadAdditionnalCodeList(parDisposItem);
		}

		protected virtual IEnumerable<ITariffAdditionalCode> GetCETariffAdditionalCodes(IArticle article)
		{
			return article.CETariffAdditionalCodes;
		}

		protected virtual IEnumerable<ITariffAdditionalCode> GetFRTariffAdditionalCodes(IArticle article)
		{
			return article.FRTariffAdditionalCodes;
		}

		void LoadArticleCompSupportingDocument(TArticleDsiComp articleImport, IArticle article)
		{
			var supportingDocumentsItem = GetSupportingDocumentsItem(article);
			articleImport.Documents = LoadSupportingDocList(supportingDocumentsItem);
		}

		void LoadArticleCompFinancialDatas(TArticleDsiComp articleImport, IArticle article)
		{
			if (article.ShouldSendCustomsStatisticAndVatValues)
			{
				articleImport.DonneesFinancieresComp = new TDonneesFinancieresDsiComp
				{
					Valstat = article.StatisticalAmount.Round(0).ToZInt(),
					ValstatValueSpecified = true,
					Valdou = article.CustomsValue.Round(0).ToZInt(),
					ValdouValueSpecified = true,
					Asstva = article.TVAAssessedAmount.Round(0).ToZInt(),
					AsstvaValueSpecified = true
				};
			}
		}

		void LoadArticleCompElementsValeurGen(TArticleDsiComp genCompImport, IArticle article)
		{
			var cusProcedure = itemIDDImport.CusProcedure;
			if (cusProcedure.ValuationBypassCode.IsEmpty && string.IsNullOrEmpty(article.AlternateCalcValue?.CalcValue))
			{
				genCompImport.FraisArticleAjout = new TFraisArticleAjout()
				{
					FraisEmballage = ConvertAmountAndCurrency(article.PackingCosts),
					Commission = ConvertAmountAndCurrency(article.Commission),
					Redevance = ConvertAmountAndCurrency(article.Fee),
					Revente = ConvertAmountAndCurrency(article.Resale),
					FraisAccessoire = ConvertAmountAndCurrency(article.OthCosts),
				};

				genCompImport.FraisArticleDeduit = new TFraisArticleDeduit()
				{
					FraisMontage = ConvertAmountAndCurrency(article.AssemblyCosts),
					FraisDouane = ConvertAmountAndCurrency(article.CustomsCosts),
				};
			}
		}

		protected virtual void LoadEntryHeaderCharges(TArticleDsiComp articleImport, IArticle article)
		{
		}

		void LoadArticleCompPreCalcEntryLines(TArticleDsiComp articleImport, IArticle article)
		{
			var myPreCalcEntryLinesCollection = new Collection<TTaxationDetail>();

			foreach (var precalEntryLine in article.PreCalcEntryLines)
			{
				var myPreCalcEntryLine = new TTaxationDetail()
				{
					Codtax = precalEntryLine.Tax?.TaxCode ?? string.Empty,
					Typtax = precalEntryLine.Tax?.TaxType ?? string.Empty,
					Montanttax = (precalEntryLine.Tax?.TaxAmount.Round(0) ?? default).ToZInt(),
					Codeport = precalEntryLine.Tax?.ChargePaymentOrDestinationID ?? string.Empty,
					Statutliquidation = precalEntryLine.Tax?.LiquidationStatus ?? string.Empty,
					Codtaxeeu = precalEntryLine.Tax?.EUTaxCode ?? string.Empty,
					UniSpe = LoadUniSpe(precalEntryLine.SuppUnit)
				};

				if (myPreCalcEntryLine.Typtax != "1")
				{
					myPreCalcEntryLine.Quotax = precalEntryLine.Tax?.TaxRate.Round(3) ?? default;
					myPreCalcEntryLine.Asstax = (precalEntryLine.Tax?.TaxAssessed.Round(0) ?? default).ToZInt();
				}

				if (!string.IsNullOrEmpty(myPreCalcEntryLine.Codtax))
				{
					myPreCalcEntryLinesCollection.Add(myPreCalcEntryLine);
				}
			}

			articleImport.LignesPrecalcs = myPreCalcEntryLinesCollection;
		}

		void LoadArticleCompSpecialTax(TArticleDsiComp articleImport, IArticle article)
		{
			var speTax = article.ThirdUnit;

			articleImport.TaxSpes = speTax == null || speTax.Code.IsEmpty
								? null
								: new Collection<TUniSpe>(new List<TUniSpe>() { LoadUniSpe(speTax) });

			var cusImportCertificationItem = article.CusImportCertification;

			articleImport.Pac = cusImportCertificationItem.IsEmpty
								? null
								: new TPacImport()
								{
									Certifcontingent = article.CusImportCertification,
									Certifdroitcommun = article.CEQuotaCertification,
									Sucretaux1 = article.SugarRate1,
									Sucretaux2 = article.SugarRate2,
									Sucretaux3 = article.SugarRate3,
									SucrePolarisation = article.SugarPolarisation
								};

			articleImport.Pac = null;   //TO DO : Either delete the whole piece of code or specify new rule to handle PAC group
		}

		#endregion

		#region Populate methodes tools

		TUniSpe LoadUniSpe(ISupplementaryUnit suppUnit)
		{
			return messageBuilderHelper.LoadUniSpe<TUniSpe>(suppUnit);
		}

		TTrader LoadTrader(IOrganisation organisation)
		{
			return organisation == null ? null : messageBuilderHelper.LoadTrader<TTrader>(organisation);
		}

		TMontantDev ConvertAmountAndCurrency(IAmountAndCurrency amountAndCurrency)
		{
			if (amountAndCurrency == null || string.IsNullOrEmpty(amountAndCurrency.Currency) || amountAndCurrency.Amount <= 0m)
			{
				return null;
			}

			var itemMontantDev = new TMontantDev()
			{
				Montant = amountAndCurrency.Amount,
				Devfac = amountAndCurrency.Currency
			};

			return itemMontantDev;
		}

		TMontantDevAssurance ConvertCostAndInsurance(ICostsAndInsurance costsAndInsurance)
		{
			if (costsAndInsurance == null)
			{
				return null;
			}

			var itemMontantDevAssurance = new TMontantDevAssurance()
			{
				Frais = ConvertAmountAndCurrency(costsAndInsurance?.Costs),
				Assurance = ConvertAmountAndCurrency(costsAndInsurance?.Insurance)
			};

			return itemMontantDevAssurance;
		}

		Collection<string> LoadAdditionnalCodeList(IEnumerable<ITariffAdditionalCode> tariffAdditionalList)
		{
			return messageBuilderHelper.LoadAdditionnalCodeList(tariffAdditionalList);
		}

		Collection<TDocument> LoadSupportingDocList(IEnumerable<ISupportingDocumentOnly> supportingDocList)
		{
			var docCollection = new Collection<TDocument>();

			if (supportingDocList != null)
			{
				foreach (var supportingDoc in supportingDocList)
				{
					var myImputationsSheetList = ImputationSheetListOfSupportingDoc(supportingDoc);

					var doc = new TDocument()
					{
						Doc = supportingDoc?.Code ?? string.Empty,
						Refdoc = supportingDoc?.RefNumber.Left(35) ?? string.Empty,
						Indd48 = supportingDoc != null && supportingDoc.IsD48AndNotClosed ? Indicateur.Item1 : Indicateur.Item0
					};

					if (doc.Indd48 == Indicateur.Item0)
					{
						doc.Datdoc = supportingDoc?.DateIssue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
					}
					else
					{
						if ((!supportingDoc?.D48Amount.IsDefault ?? false) || !supportingDoc.IsUnderInvoiceLine)
						{
							doc.Mntd48 = supportingDoc?.D48Amount.Round(0).ToZLong();
						}

						doc.Deld48 = supportingDoc?.D48Deadline;
						doc.RefPfAid = supportingDoc?.PFAIdentification ?? string.Empty;
						doc.RefPfAdoc = supportingDoc?.PFADocument ?? string.Empty;
					}

					doc.FichesImputations = myImputationsSheetList;

					docCollection.Add(doc);
				}
			}

			return docCollection;
		}

		Collection<TFicheImputation> ImputationSheetListOfSupportingDoc(ISupportingDocumentOnly supportingDoc)
		{
			var myImputationsSheetListCollection = new Collection<TFicheImputation>();

			if (supportingDoc != null)
			{
				foreach (var imputationSheet in supportingDoc.ImputationsSheets)
				{
					var lineNumber = imputationSheet?.LineNumber ?? string.Empty;
					if (!string.IsNullOrEmpty(lineNumber))
					{
						myImputationsSheetListCollection.Add(new TFicheImputation
						{
							NumLigne = lineNumber,
							RefProduit = imputationSheet?.ProductReference.Left(35) ?? string.Empty,
							NomProduit = imputationSheet?.ProductName.Left(260) ?? string.Empty,
							UnitImput = imputationSheet?.ImputationUnit ?? string.Empty,
							NbrImput = imputationSheet?.ImputationQuantity ?? default,
							NbrImputValueSpecified = imputationSheet?.ShouldWriteImputationQuantity ?? false,
							PoidsProv = imputationSheet?.ProvisionalWeight.Weight ?? default,
							PoidsProvValueSpecified = imputationSheet?.ShouldWriteProvisionalWeight ?? false,
							UnitPoidsProv = imputationSheet?.ProvisionalWeight.Unit ?? default,
							DevImput = imputationSheet?.ImputationCurrency ?? string.Empty,
							MontImput = imputationSheet?.ImputationAmount ?? default,
							MontImputValueSpecified = imputationSheet?.ShouldWriteImputationAmount ?? false,
							Msn = imputationSheet?.NetWeight ?? default,
							MsnValueSpecified = imputationSheet?.ShouldWriteNetWeight ?? false,
						});
					}
				}
			}

			return myImputationsSheetListCollection;
		}

		#endregion

		readonly TransactionTypes messageTransactionType;
		readonly IDeclarationImportExport itemIDDImport;
		readonly EU.Business.ErrorCollector errorCollector;
		readonly MessageBuilderHelper messageBuilderHelper;
		readonly int newSequenceNumeric;
	}
}
