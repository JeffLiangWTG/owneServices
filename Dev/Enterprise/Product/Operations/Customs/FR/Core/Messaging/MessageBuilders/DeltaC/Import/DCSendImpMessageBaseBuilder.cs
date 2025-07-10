using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Import;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCSendImpMessageBaseBuilder : MessageBuilderBase<TMessage>
	{
		public DCSendImpMessageBaseBuilder(IDeclarationImportExport declarationImport
			, EU.Business.ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric
			)
		{
			itemIDCImport = declarationImport;
			errorCollector = errorCollectorObject;
			messageTransactionType = transactionType;
			this.newSequenceNumeric = newSequenceNumeric;
			messageBuilderHelper = new MessageBuilderHelper();
		}

		protected override TMessage GenerateDeclarationMessage()
		{
			var message = new TMessage();

			if (messageTransactionType == TransactionTypes.Original)
			{
				message.EnveloppeMessage = PopulateEnvelopMessage();

				message.Declaration = new TcDecImp();
				if (ShouldPopulateMainDatasInfos)
				{
					message.Declaration.DatasDec = PopulateMainDatasInfos();
				}
				else
				{
					message.Declaration.Liquidations = PopulateLiquidationInfos();
				}
			}

			return message;
		}

		protected virtual bool ShouldPopulateMainDatasInfos => true;

		#region Populate message

		TDatasDec PopulateMainDatasInfos()
		{
			var datasDec = new TDatasDec();
			datasDec.Entete = PopulateHeader();
			datasDec.Gen = PopulateProcedure();
			datasDec.Articles = PopulateArticlesImport();
			return datasDec;
		}

		TLiquidations PopulateLiquidationInfos()
		{
			var liquidationSection = new TLiquidations();
			liquidationSection.Entete = PopulateHeader();
			liquidationSection.LiquidationArticles = PopulateLiquidation();
			return liquidationSection;
		}

		TEnveloppeMessage PopulateEnvelopMessage()
		{
			TEnveloppeMessage envelopeMessage = null;
			var messageEnvelope = itemIDCImport?.MessageEnvelope;

			if (messageEnvelope != null)
			{
				envelopeMessage = new TEnveloppeMessage();
				envelopeMessage.SchemaId = itemIDCImport.MessageEnvelope.SchemaID;
				envelopeMessage.SchemaVersion = itemIDCImport.MessageEnvelope.SchemaVersion;
				if (!messageEnvelope.PartnerId.IsEmpty)
				{
					envelopeMessage.PartyId = messageEnvelope.PartnerId;
				}
				envelopeMessage.TransactionId = itemIDCImport.MessageEnvelope.TransactionId;
				envelopeMessage.Numseq = (short)newSequenceNumeric;
			}
			return envelopeMessage;
		}

		protected virtual TEntete PopulateHeader()
		{
			if (itemIDCImport?.Header == null)
			{
				return null;
			}

			var motivationHeader = itemIDCImport?.Header;
			var motivationItem = PopulateMotivation();

			var header = new TEntete();
			header.Codact = motivationHeader?.ActionCode ?? string.Empty;
			header.Refdos = motivationHeader?.References?.OwnerDeclarationIdentification.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText) ?? string.Empty;

			var refdecItem = motivationHeader?.References?.CusDeclarationNumber;

			#region Optional properties

			header.Motivation = motivationItem;

			header.Refdec = MessageBuilderHelper.GetOptionalString(refdecItem);

			#endregion

			return header;
		}

		protected virtual TMotivation PopulateMotivation()
		{
			return null;
		}

		public TMotivation LoadMotivation()
		{
			var motivationHeader = itemIDCImport?.Header;

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
				motivationItem.Commentaire = motivationHeaderMotivation.Comment.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Justifreg = motivationHeaderMotivation.RegularJustification.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Motiv = motivationHeaderMotivation.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Nouvelledest = motivationHeaderMotivation.NewDestination.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			}

			return motivationItem;
		}

		#region Procedure Gen

		protected virtual TGenImport PopulateProcedure()
		{
			#region Tests messages objects

			var cusProcedure = itemIDCImport?.CusProcedure;

			if (cusProcedure?.Office == null || cusProcedure?.DeliveryTerms == null)
			{
				return null;
			}

			#endregion

			var genImport = LoadProcedureBase(cusProcedure);

			LoadProcedureOfficeAndOperator(genImport, cusProcedure);

			LoadProcedureTransport(genImport, cusProcedure);

			LoadProcedurePreval(genImport, cusProcedure);

			LoadProcedureOperator(genImport, cusProcedure);

			LoadProcedurePrice(genImport, cusProcedure);

			LoadProcedureDeliveryAndOthersCost(genImport, cusProcedure);

			return genImport;
		}

		TGenImport LoadProcedureBase(ICusProcedure cusProcedure)
		{
			var refTypeProc = RefTypeProc.C;

			var retReftypeProc = Enum.TryParse(cusProcedure.ProcedureType, out refTypeProc);

			var genImport = new TGenImport()
			{
				Typeproc = refTypeProc,
				Procedure1 = cusProcedure.EntryStyle,
				Procedure2 = cusProcedure.EntryStyleCode,
				Nbrart = cusProcedure.ArticleCount,
				Modpaiement = cusProcedure.PaymentMode,
			};

			LoadProcedureTransport(genImport, cusProcedure);

			return genImport;
		}

		void LoadProcedureOfficeAndOperator(TGenImport genImport, ICusProcedure cusProcedure)
		{
			genImport.Bureau = new TBureauImport();

			genImport.Bureau.Burdom = cusProcedure.Office.OfficeOfDeclaration;
			genImport.Bureau.Burrat = cusProcedure.Office.OfficeOfLodgement;
		}

		void LoadProcedureTransport(TGenImport genImport, ICusProcedure cusProcedure)
		{
			var transpportItem = cusProcedure.Transport;

			var genTransport = new TTransport();

			genTransport.Modfrotra = transpportItem?.ModeOfTRansport ?? string.Empty;
			genTransport.Inttra = transpportItem?.ModeOfTRansportInland ?? string.Empty;
			genTransport.Conteneurtra = transpportItem?.ContainerMode ?? string.Empty;
			genTransport.Natfrotra = transpportItem?.NationalityOfTransport ?? string.Empty;
			genTransport.Burfro = transpportItem?.CusOffice ?? string.Empty;

			if (genTransport.Modfrotra == "4")
			{
				genTransport.Aeroportemb = transpportItem?.IATAAirportOfLoading ?? string.Empty;
				genTransport.Aertra = transpportItem?.AirRoadType ?? string.Empty;
			}

			genImport.Transport = genTransport;
		}

		void LoadProcedurePreval(TGenImport genImport, ICusProcedure cusProcedure)
		{
			var alternateValItem = new THorsValeur()
			{
				Horsvaleur = cusProcedure.AlternateCalcValue?.CalcValue,
				Motiv = cusProcedure.AlternateCalcValue?.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText)
			};

			var prevalItem = PopulatePreval(cusProcedure);

			genImport.HorsValeur = string.IsNullOrEmpty(alternateValItem.Horsvaleur)
									? null
									: alternateValItem;

			genImport.Preval = string.IsNullOrEmpty(prevalItem?.Datpreval ?? ZString.Empty)
									? null
									: prevalItem;

			genImport.Locagr = MessageBuilderHelper.GetOptionalString(cusProcedure.AgreedGoodsLocation);

			genImport.Magasin = MessageBuilderHelper.GetOptionalString(cusProcedure.ClearanceLocation);

			genImport.Nbrcol = cusProcedure.PackageCount;
			genImport.Nattrans = cusProcedure.TransactionNature;

			genImport.EtatMembreDestinationFinale = MessageBuilderHelper.GetOptionalString(cusProcedure.ArrivalState);

			genImport.Bureau.Burunivis = MessageBuilderHelper.GetOptionalString(cusProcedure.Office.VisitingOffice);
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

		void LoadProcedureOperator(TGenImport genImport, ICusProcedure cusProcedure)
		{
			var operateur = new TOperateurImport();

			operateur.Opeben = cusProcedure.AgreementOwnerEORI;
			operateur.Numagr = cusProcedure.DeltaGAuthorisationNumber;
			operateur.Operep = cusProcedure.BranchCusBrokerageCode;
			operateur.Modrep = cusProcedure.RepresentationModeCode;

			var suppliersItem = cusProcedure.Suppliers;

			operateur.Expediteurs = suppliersItem == null
								? new Collection<TTrader>(Array.Empty<TTrader>())
								: LoadTraders(suppliersItem, false);

			var importersItem = cusProcedure.Importers;

			operateur.Destinataires = importersItem == null
								? new Collection<TTrader>(Array.Empty<TTrader>())
								: LoadTraders(importersItem, true);

			var repTaxOrganisationItem = cusProcedure.RepTaxOrganisation;

			operateur.RepFisc = LoadTrader(repTaxOrganisationItem, false);

			operateur.Numcre = MessageBuilderHelper.GetOptionalString(cusProcedure.DeferalApprovalCreditNumber);

			operateur.Numcod = MessageBuilderHelper.GetOptionalString(cusProcedure.VariousOperationCreditNumber);

			genImport.Operateur = operateur;
		}

		void LoadProcedurePrice(TGenImport genImport, ICusProcedure cusProcedure)
		{
			var entryGoodsPriceSumItem = cusProcedure.EntryGoodsPriceSum;
			var entryGoodsPriceCurrencyRateItem = cusProcedure.EntryGoodsPriceCurrencyRate;

			genImport.Prifac = entryGoodsPriceSumItem.Round(2);
			genImport.Devfac = MessageBuilderHelper.GetOptionalString(cusProcedure.EntryGoodsPriceCurrency);
			genImport.Coursdevise = entryGoodsPriceCurrencyRateItem;
		}

		void LoadProcedureGenElements(TGenImport genImport, ICusProcedure cusProcedure)
		{
			genImport.ElementsValeurGen = new TElementsValeurGen()
			{
				CumulTiers = ConvertCostAndInsurance(cusProcedure.ThirdCountryTransportCosts),
				CumulCeHorsFrInclus = ConvertCostAndInsurance(cusProcedure.EUTransportCostsInInvoice),
				CumulCeHorsFrExclus = ConvertCostAndInsurance(cusProcedure.EUTransportCostsNotInInvoice),
				CumulFrInclus = ConvertCostAndInsurance(cusProcedure.FRTransportCostsInInvoice),
				CumulFrExclus = ConvertCostAndInsurance(cusProcedure.FRTransportCostsNotInInvoice),
				CumulAerienTiers = ConvertCostAndInsurance(cusProcedure.ThirdCountryAirCosts),
				CumulAerienFr = ConvertCostAndInsurance(cusProcedure.FRAirCosts),
				AutresFraisAjout = ConvertAmountAndCurrency(cusProcedure.OthAddedCosts),
				FraisDom = ConvertCostAndInsurance(cusProcedure.DOMCostsAndInsurance)
			};
		}

		void LoadProcedureDeliveryAndOthersCost(TGenImport genImport, ICusProcedure cusProcedure)
		{
			genImport.Modgarantie = MessageBuilderHelper.GetOptionalString(cusProcedure.GuaranteeMode);

			var incotermCodeItem = cusProcedure.DeliveryTerms?.IncotermCode ?? string.Empty;

			genImport.ConditionsLivraison = string.IsNullOrEmpty(incotermCodeItem)
											? null
											: new TConditionsLivraison()
											{
												Codliv = incotermCodeItem,
												Lieuliv = cusProcedure.DeliveryTerms.DeliveryPlace,
												Codelieuincoterm = cusProcedure.DeliveryTerms.IncotermPlace,
											};

			if (cusProcedure.ValuationBypassCode.IsEmpty)
			{
				LoadProcedureGenElements(genImport, cusProcedure);
			}

			var otherDeductCost = new TAutresFraisDeduit()
			{
				Interets = ConvertAmountAndCurrency(cusProcedure.Interest),
				Commission = ConvertAmountAndCurrency(cusProcedure.Commission),
				FraisBaseTva = ConvertAmountAndCurrency(cusProcedure.VATBaseCosts)
			};

			if (genImport.ElementsValeurGen != null)
			{
				genImport.ElementsValeurGen.AutresFraisDeduit = otherDeductCost;
			}

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

		protected virtual Collection<TArticleImport> PopulateArticlesImport()
		{
			#region Tests messages objects

			if (itemIDCImport?.Articles == null)
			{
				return null;
			}

			if (!itemIDCImport.Articles.Any())
			{
				errorCollector.AddError(MessageBuilderHelper.MessageNoInvoiceLineDefine
					, new EU.Business.ErrorInfo("31", "Mandatory"));

				return null;
			}

			#endregion

			var genArticlesImport = new Collection<TArticleImport>();

			foreach (var article in itemIDCImport.Articles)
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

				LoadArticleEconomicRegime(articleImport, article);

				#region Financial and costs properties

				LoadFinancialDatas(articleImport, article);

				var cusProcedure = itemIDCImport.CusProcedure;
				if (cusProcedure.ValuationBypassCode.IsEmpty && string.IsNullOrEmpty(article.AlternateCalcValue?.CalcValue))
				{
					LoadAddedCosts(articleImport, article);

					LoadDeductedCosts(articleImport, article);
				}

				#endregion

				#region Tax properties

				LoadPreCalcEntryLines(articleImport, article);

				LoadEntryHeaderCharges(articleImport, article);

				LoadSpecialTax(articleImport, article);

				#endregion

				#region Special informations properties

				LoadSpecialInformations(articleImport, article);

				#endregion

				genArticlesImport.Add(articleImport);
			}

			return genArticlesImport;
		}

		TArticleImport LoadArticleBase(IArticle article)
		{
			var articleImport = new TArticleImport();

			articleImport.Numart = Convert.ToInt16(article.EntryNumber);
			articleImport.Nomenc = article.TariffCode;
			articleImport.Descom = article.EntryLineDescription.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			articleImport.Msb = article.GrossWeight;
			articleImport.Msn = article.CustomsQuantity;

			articleImport.Ori = article.CountryGoodsOrigineCode;
			articleImport.Pro = article.CountryGoodsSupplyCode;
			var valuationMethod = article.ValuationMethod;
			articleImport.Valevaluation = valuationMethod;

			var procedureItem = new TRegimeDouanier();
			procedureItem.Regdou = article.ProcedureCode;
			procedureItem.Regdoupre = article.PreviousCode;
			procedureItem.Compcom = article.Concession;
			articleImport.RegimeDouanier = procedureItem;

			var previousDocItem = article.PreviousDocument;
			if (previousDocItem != null)
			{
				var supportDocItem = new TPriseEnCharge();
				supportDocItem.Natdocpec = previousDocItem.Code;
				supportDocItem.Typdocpec = previousDocItem.Type;
				supportDocItem.Refdocpec = previousDocItem.RefNumber;
				articleImport.PriseEnCharge = supportDocItem;
			}

			LoadArticleBaseNotRequired(articleImport, article);

			return articleImport;
		}

		void LoadArticleBaseNotRequired(TArticleImport articleImport, IArticle article)
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

		void LoadArticleAdditionalCodes(TArticleImport articleImport, IArticle article)
		{
			var additionalCECodeItem = article.CETariffAdditionalCodes;
			articleImport.Cacos = LoadAdditionnalCodeList(additionalCECodeItem);

			var additionalFRCodeItem = article.FRTariffAdditionalCodes;
			articleImport.Canas = LoadAdditionnalCodeList(additionalFRCodeItem);

			var parDisposItem = article.PartDispos;
			articleImport.Dispoparts = LoadAdditionnalCodeList(parDisposItem);

			var quotaRefNumberItem = article.QuotaRefNumber;

			if (quotaRefNumberItem != null && quotaRefNumberItem.Any())
			{
				articleImport.Cnts = GetQuotaNumber(quotaRefNumberItem.ToList());
			}
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

		void LoadArticleSupplementaryUnitAndWarehouse(TArticleImport articleImport, IArticle article)
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

			if (itemIDCImport.HasIntoWarehouseProcedure)
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

		void LoadArticleAutorisationEco(TArticleImport articleImport, IArticle article)
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

		void LoadArticleEconomicRegime(TArticleImport articleImport, IArticle article)
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
				regimeEcoItem.Montantgar = article.EcoRegimeDatas.GuaranteeAmount.Round(0).ToZLong();
				regimeEcoItem.MontantgarValueSpecified = regimeEcoItem.Montantgar > decimal.Zero;

				articleImport.RegimeEco = regimeEcoItem;
			}
			else
			{
				articleImport.RegimeEco = null;
			}
		}

		void LoadArticleEconomicPreference(TArticleImport articleImport, IArticle article)
		{
			var preferenceItem = new TPreference();

			preferenceItem.Preftar1 = article.Preference?.CodePart1 ?? string.Empty;
			preferenceItem.Preftar2 = article.Preference?.CodePart2 ?? string.Empty;

			articleImport.Preference = string.IsNullOrEmpty(preferenceItem.Preftar1) && string.IsNullOrEmpty(preferenceItem.Preftar2)
									? null
									: preferenceItem;

			var cusProcedure = itemIDCImport?.CusProcedure;
			if ((cusProcedure?.Transport?.ContainerMode ?? ZString.Empty) == "1")
			{
				var numContainerList = new Collection<string>();
				article.Containers.Where(x => !x.IsEmpty).ForEach(x => numContainerList.Add(x));
				articleImport.Conteneurs = numContainerList;
			}
		}

		void LoadPacking(TArticleImport articleImport, IArticle article)
		{
			var colisageItem = new TColisage()
			{
				Nbrcol = article.Packing?.Count,

				Nbrpieces = article.Packing?.ItemsCount,

				Natcol = MessageBuilderHelper.GetOptionalString(article.Packing?.Type),

				Marquecolis = MessageBuilderHelper.GetOptionalString(article.Packing?.MarksAndNos.Left(MessageBuilderHelper.CW_MarksAndNosMaxLengthForMessage))
			};

			articleImport.Colisage = string.IsNullOrEmpty(colisageItem.Natcol)
									? null
									: colisageItem;
		}

		void LoadSupportingDocument(TArticleImport articleImport, IArticle article)
		{
			var supportingSpecialInfoItem = article.SpecMens;
			articleImport.Menspectextes = LoadMenSpecTexteCodeList(supportingSpecialInfoItem);

			var supportingDocumentsItem = article.SupportingDocuments;
			articleImport.Documents = LoadSupportingDocList(supportingDocumentsItem);
		}

		void LoadFinancialDatas(TArticleImport articleImport, IArticle article)
		{
			var invoiceLinePriceItem = new TDonneesFinancieresImport();
			invoiceLinePriceItem.Prifac = article.InvoiceLinePrice.Round(2);
			invoiceLinePriceItem.Devfac = MessageBuilderHelper.GetOptionalString(article.CurrencyCode);
			if (article.ShouldSendCustomsStatisticAndVatValues)
			{
				invoiceLinePriceItem.Valstat = article.StatisticalAmount.Round(0).ToZLong();
				invoiceLinePriceItem.Valdou = article.CustomsValue.Round(0).ToZLong();
				invoiceLinePriceItem.Asstva = article.TVAAssessedAmount.Round(0).ToZLong();
			}
			articleImport.DonneesFinancieres = invoiceLinePriceItem;

			articleImport.Depliv = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.France ? MessageBuilderHelper.GetOptionalString(article.DeliveryDepartment) : null;

			articleImport.Ajuval = article.ValuationAdjustPercent;
		}

		void LoadAddedCosts(TArticleImport articleImport, IArticle article)
		{
			var packingCostsItem = article.PackingCosts;
			var commissionItem = article.Commission;
			var feeItem = article.Fee;
			var resaleItem = article.Resale;
			var othCostsItem = article.OthCosts;

			var articleImportCost = new TFraisArticleAjout()
			{
				FraisEmballage = ConvertAmountAndCurrency(packingCostsItem),
				Commission = ConvertAmountAndCurrency(commissionItem),
				Redevance = ConvertAmountAndCurrency(feeItem),
				Revente = ConvertAmountAndCurrency(resaleItem),
				FraisAccessoire = ConvertAmountAndCurrency(othCostsItem)
			};

			articleImport.FraisArticleAjout = packingCostsItem == null && commissionItem == null
													&& feeItem == null && resaleItem == null && othCostsItem == null
											? null
											: articleImportCost;
		}

		void LoadDeductedCosts(TArticleImport articleImport, IArticle article)
		{
			var assemblyCostsItem = article.AssemblyCosts;
			var customsCostsItem = article.CustomsCosts;

			var articleDecucCost = new TFraisArticleDeduit()
			{
				FraisMontage = ConvertAmountAndCurrency(assemblyCostsItem),
				FraisDouane = ConvertAmountAndCurrency(customsCostsItem)
			};

			articleImport.FraisArticleDeduit = assemblyCostsItem != null || customsCostsItem != null ? articleDecucCost : null;
		}

		void LoadPreCalcEntryLines(TArticleImport articleImport, IArticle article)
		{
			var myPreCalcEntryLinesCollection = new Collection<TTaxationDetail>();

			foreach (var precalEntryLine in article.PreCalcEntryLines)
			{
				var myPreCalcEntryLine = new TTaxationDetail()
				{
					Codtax = precalEntryLine.Tax?.TaxCode ?? string.Empty,
					Typtax = precalEntryLine.Tax?.TaxType ?? string.Empty,
					Quotax = precalEntryLine.Tax?.TaxRate.Round(3) ?? default,
					Asstax = precalEntryLine.Tax?.TaxAssessed.Round(0).ToZLong() ?? default,
					Montanttax = precalEntryLine.Tax?.TaxAmount.Round(0).ToZLong() ?? default,
					UniSpe = LoadUniSpe(precalEntryLine.SuppUnit)
				};

				var isAmountZeroAndNotVAT = myPreCalcEntryLine.Montanttax == 0 && !precalEntryLine.IsVAT;

				if (!string.IsNullOrEmpty(myPreCalcEntryLine.Codtax) && !isAmountZeroAndNotVAT)
				{
					myPreCalcEntryLinesCollection.Add(myPreCalcEntryLine);
				}
			}

			if (myPreCalcEntryLinesCollection.Any())
			{
				articleImport.LignesPrecalcs = myPreCalcEntryLinesCollection;
			}
		}

		void LoadEntryHeaderCharges(TArticleImport articleImport, IArticle article)
		{
			if (article.EntryNumber == 1)
			{
				var myPreCalcEntryLinesCollection = articleImport.LignesPrecalcs ?? new Collection<TTaxationDetail>();
				foreach (var entryHeaderCharge in article.EntryHeaderCharges)
				{
					var myEntryHeaderCharge = new TTaxationDetail()
					{
						Codtax = entryHeaderCharge.TaxCode,
						Typtax = entryHeaderCharge.TaxType,
						Quotax = entryHeaderCharge.TaxRate.Round(3),
						Asstax = entryHeaderCharge.TaxAssessed.Round(0).ToZLong(),
						Montanttax = entryHeaderCharge.TaxAmount.Round(0).ToZLong(),
						Codeport = entryHeaderCharge.ChargePaymentOrDestinationID
					};

					myPreCalcEntryLinesCollection.Add(myEntryHeaderCharge);
				}

				articleImport.LignesPrecalcs = myPreCalcEntryLinesCollection;
			}
		}

		void LoadSpecialTax(TArticleImport articleImport, IArticle article)
		{
			var speTax = article.ThirdUnit;

			articleImport.TaxSpes = speTax == null || speTax.Code.IsEmpty
								? null
								: LoadUniSpes(speTax);

			var cusImportCertificationItem = article.CusImportCertification;

			articleImport.Pac = string.IsNullOrEmpty(cusImportCertificationItem)
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

		void LoadSpecialInformations(TArticleImport articleImport, IArticle article)
		{
			if (article.Applicant != null && !article.EmptyApplicant() && !(article.IsPlacingGoodsUnderBW && article.HasSpecificRegimeAuthorisation))
			{
				articleImport.EcoSpec = new TEcoSpecImport()
				{
					Natperf = article.ApplicantInwardNature.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Description = article.ApplicantDescription.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Conditions = article.ApplicantConditions.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Burapur = article.ApplicantPurOffice,
					Lieuperf = article.ApplicantInwardLocation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Formalitestransf = article.ApplicantTransFormality
				};
			}

			articleImport.Infosspec = MessageBuilderHelper.GetOptionalString(article.SpecificInfos);
		}

		#endregion

		#region Liquidation
		Collection<TLiquidationArticle> PopulateLiquidation()
		{
			var liquidation = new Collection<TLiquidationArticle>();
			foreach (var liquidationItem in itemIDCImport.Liquidation)
			{
				var myliquidationItem = new TLiquidationArticle();
				myliquidationItem.Numart = liquidationItem?.ArticleNumber ?? 0;
				myliquidationItem.TaxationDetail = LoadTaxationDetails(liquidationItem);
				liquidation.Add(myliquidationItem);
			}
			return liquidation;
		}

		#endregion

		#endregion

		#region Populate methodes tools

		TTaxationDetail LoadTaxationDetails(ILiquidationItem liquidationItem)
		{
			return messageBuilderHelper.LoadTaxationDetails<TTaxationDetail>(liquidationItem, true);
		}

		TUniSpe LoadUniSpe(ISupplementaryUnit suppUnit)
		{
			return messageBuilderHelper.LoadUniSpe<TUniSpe>(suppUnit);
		}

		Collection<TUniSpe> LoadUniSpes(ISupplementaryUnit suppUnit)
		{
			var result = new Collection<TUniSpe>();
			result.Add(LoadUniSpe(suppUnit));
			return result;
		}

		Collection<TTrader> LoadTraders(IEnumerable<IOrganisation> organisations, bool allowFallbakToVatNumberInsteadofEoriForTIN)
		{
			var organisationsList = new Collection<TTrader>();

			if (organisations == null)
			{
				return null;
			}

			foreach (var organisation in organisations)
			{
				organisationsList.Add(LoadTrader(organisation, allowFallbakToVatNumberInsteadofEoriForTIN));
			}

			return organisationsList;
		}

		TTrader LoadTrader(IOrganisation organisation, bool allowFallbakToVatNumberInsteadofEoriForTIN)
		{
			return organisation == null ? null : messageBuilderHelper.LoadTrader<TTrader>(organisation, allowFallbakToVatNumberInsteadofEoriForTIN);
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

		Collection<TMenspectexte> LoadMenSpecTexteCodeList(IEnumerable<ITariffAdditionalCode> tariffAdditionalList)
		{
			return messageBuilderHelper.LoadMenSpecTexteCodeList<TMenspectexte>(tariffAdditionalList);
		}

		Collection<TDocument> LoadSupportingDocList(IEnumerable<ISupportingDocumentOnly> supportingDocList)
		{
			var docCollection = new Collection<TDocument>();

			if (supportingDocList != null)
			{
				foreach (var supportingDoc in supportingDocList)
				{
					var myImputationsSheet = ImputationSheetListOfSupportingDoc(supportingDoc);

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
							doc.Mntd48 = supportingDoc.D48Amount.Round(0).ToZLong();
						}

						doc.Deld48 = supportingDoc?.D48Deadline;
						doc.RefPfAid = supportingDoc?.PFAIdentification ?? string.Empty;
						doc.RefPfAdoc = supportingDoc?.PFADocument ?? string.Empty;
					}

					doc.FichesImputations = myImputationsSheet;

					docCollection.Add(doc);
				}
			}

			return docCollection;
		}
		Collection<TFicheImputation> ImputationSheetListOfSupportingDoc(ISupportingDocumentOnly supportingDoc)
		{
			var myImputationsSheetCollection = new Collection<TFicheImputation>();

			if (supportingDoc != null)
			{
				foreach (var imputationSheet in supportingDoc.ImputationsSheets)
				{
					var lineNumber = imputationSheet?.LineNumber ?? string.Empty;
					if (!string.IsNullOrEmpty(lineNumber))
					{
						myImputationsSheetCollection.Add(new TFicheImputation
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

			return myImputationsSheetCollection;
		}

		#endregion

		readonly TransactionTypes messageTransactionType;
		readonly IDeclarationImportExport itemIDCImport;
		readonly EU.Business.ErrorCollector errorCollector;
		readonly MessageBuilderHelper messageBuilderHelper;
		readonly int newSequenceNumeric;
	}
}
