using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Export;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders
{
	public class DCSendExpMessageBaseBuilder : MessageBuilderBase<TMessage>
	{
		public DCSendExpMessageBaseBuilder(IDeclarationImportExport declaration
			, EU.Business.ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric
			)
		{
			itemIDC = declaration;
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

				message.Declaration = new TcDecExp();

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
			datasDec.Articles = PopulateArticlesExport(datasDec.Gen);
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
			var messageEnvelope = itemIDC?.MessageEnvelope;

			if (messageEnvelope != null)
			{
				envelopeMessage = new TEnveloppeMessage();
				envelopeMessage.SchemaId = messageEnvelope.SchemaID;
				envelopeMessage.SchemaVersion = messageEnvelope.SchemaVersion;
				if (!messageEnvelope.PartnerId.IsEmpty)
				{
					envelopeMessage.PartyId = messageEnvelope.PartnerId;
				}
				envelopeMessage.TransactionId = messageEnvelope.TransactionId;
				envelopeMessage.Numseq = (short)newSequenceNumeric;
			}
			return envelopeMessage;
		}

		protected virtual TEntete PopulateHeader()
		{
			if (itemIDC?.Header == null)
			{
				return null;
			}

			var motivationHeader = itemIDC?.Header;

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
			var motivationHeader = itemIDC?.Header;

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

		protected virtual TGenExport PopulateProcedure()
		{
			#region Tests messages objects

			var cusProcedure = itemIDC?.CusProcedure;

			if (cusProcedure?.Office == null || cusProcedure?.DeliveryTerms == null)
			{
				return null;
			}

			#endregion

			var genExport = LoadProcedureBase(cusProcedure);

			if (genExport != null)
			{
				LoadProcedureOfficeAndOperator(genExport, cusProcedure);

				LoadProcedureTransport(genExport, cusProcedure);

				LoadProcedurePreval(genExport, cusProcedure);

				LoadProcedureOperator(genExport, cusProcedure);

				LoadProcedurePrice(genExport, cusProcedure);

				LoadProcedureDeliveryAndOthersCost(genExport, cusProcedure);
			}

			return genExport;
		}

		TGenExport LoadProcedureBase(ICusProcedure cusProcedure)
		{
			var refTypeProc = RefTypeProc.C;

			var retReftypeProc = Enum.TryParse(cusProcedure.ProcedureType, out refTypeProc);

			var genExport = new TGenExport()
			{
				Typeproc = refTypeProc,
				Procedure1 = cusProcedure.EntryStyle,
				Procedure2 = cusProcedure.EntryStyleCode,
				Nbrart = cusProcedure.ArticleCount,
				Dest = cusProcedure.DestinationState,
				Itinerary = LoadItinerary(cusProcedure),

				Modpaiement = cusProcedure.PaymentMode,
			};

			LoadProcedureTransport(genExport, cusProcedure);

			return genExport;
		}

		void LoadProcedureOfficeAndOperator(TGenExport genExport, ICusProcedure cusProcedure)
		{
			genExport.Bureau = new TBureauExport();

			genExport.Bureau.Burdom = cusProcedure.Office.OfficeOfDeclaration;
			genExport.Bureau.Burrat = cusProcedure.Office.OfficeOfLodgement;
		}

		void LoadProcedureTransport(TGenExport genExport, ICusProcedure cusProcedure)
		{
			var transportItem = cusProcedure.Transport;

			var genTransport = new TTransportExport();

			genTransport.Modfrotra = transportItem?.ModeOfTRansport ?? string.Empty;
			genTransport.Inttra = transportItem?.ModeOfTRansportInland ?? string.Empty;
			genTransport.Conteneurtra = transportItem?.ContainerMode ?? string.Empty;
			genTransport.Natfrotra = transportItem?.NationalityOfTransport ?? string.Empty;
			genTransport.Idtransport = transportItem?.TransportID ?? string.Empty;
			genTransport.Natfrotra = transportItem?.NationalityOfTransport ?? string.Empty;
			genTransport.TransportMethodPayment = transportItem?.TransportMethodPayment ?? string.Empty;
			genExport.Transport = genTransport;
		}

		Collection<string> LoadItinerary(ICusProcedure cusProcedure)
		{
			var routingCountry = cusProcedure.Itinerary.Select(x => x.ToString()).ToArray();
			return routingCountry == null || !routingCountry.Any()
				? null
				: new Collection<string>(routingCountry);
		}

		void LoadProcedurePreval(TGenExport genExport, ICusProcedure cusProcedure)
		{
			var prevalItem = PopulatePreval(cusProcedure);

			genExport.Preval = string.IsNullOrEmpty(prevalItem?.Datpreval)
									? null
									: prevalItem;

			genExport.Locagr = MessageBuilderHelper.GetOptionalString(cusProcedure.AgreedGoodsLocation);

			genExport.Magasin = MessageBuilderHelper.GetOptionalString(cusProcedure.ClearanceLocation);

			genExport.Nbrcol = cusProcedure.PackageCount;
			genExport.Nattrans = cusProcedure.TransactionNature;

			var etatMembreExportationReel = MessageBuilderHelper.GetOptionalString(cusProcedure.DepartureState);
			genExport.EtatMembreExportationReel = etatMembreExportationReel == Constants.CountryCodes.France ? string.Empty : etatMembreExportationReel;

			LoadOffices(genExport, cusProcedure);
		}

		void LoadOffices(TGenExport genExport, ICusProcedure cusProcedure)
		{
			genExport.Bureau.Burunivis = MessageBuilderHelper.GetOptionalString(cusProcedure.Office?.VisitingOffice);

			genExport.Bureau.Bureausortie = MessageBuilderHelper.GetOptionalString(cusProcedure.Office?.ExitOffice);

			if (!itemIDC.IsOfficeOfLodgementDifferentFromOfficeOfExit)
			{
				var typeSortie = cusProcedure.Office?.ECSExitType;
				var motivSortie = cusProcedure.Office?.ECSMotivation;
				genExport.Bureau.Sortie = new TSortie
				{
					Typesortie = MessageBuilderHelper.GetOptionalString(typeSortie),
					Motiv = MessageBuilderHelper.GetOptionalString(motivSortie)
				};
			}
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

		void LoadProcedureOperator(TGenExport genExport, ICusProcedure cusProcedure)
		{
			var operateur = new TOperateurExport();

			operateur.Opeben = cusProcedure.AgreementOwnerEORI;
			operateur.Numagr = cusProcedure.DeltaGAuthorisationNumber;
			operateur.Operep = cusProcedure.BranchCusBrokerageCode;
			operateur.Modrep = cusProcedure.RepresentationModeCode;

			var suppliersItem = cusProcedure.Suppliers;

			operateur.Expediteurs = LoadTraders(suppliersItem);

			var importersItem = cusProcedure.Importers;

			operateur.Destinataires = LoadRecipients(importersItem);

			var repTaxOrganisationItem = cusProcedure.RepTaxOrganisation;

			operateur.RepFisc = LoadTrader(repTaxOrganisationItem);

			operateur.Numcre = MessageBuilderHelper.GetOptionalString(cusProcedure.DeferalApprovalCreditNumber);

			operateur.Numcod = MessageBuilderHelper.GetOptionalString(cusProcedure.VariousOperationCreditNumber);

			genExport.Operateur = operateur;
		}

		void LoadProcedurePrice(TGenExport genExport, ICusProcedure cusProcedure)
		{
			var entryGoodsPriceSumItem = cusProcedure.EntryGoodsPriceSum;
			var entryGoodsPriceCurrencyRateItem = cusProcedure.EntryGoodsPriceCurrencyRate;

			genExport.Prifac = entryGoodsPriceSumItem.Round(2);
			genExport.Devfac = MessageBuilderHelper.GetOptionalString(cusProcedure.EntryGoodsPriceCurrency);
			genExport.Coursdevise = entryGoodsPriceCurrencyRateItem;
		}

		void LoadProcedureDeliveryAndOthersCost(TGenExport genExport, ICusProcedure cusProcedure)
		{
			genExport.Modgarantie = MessageBuilderHelper.GetOptionalString(cusProcedure.GuaranteeMode);

			var incotermCodeItem = cusProcedure.DeliveryTerms.IncotermCode;

			genExport.ConditionsLivraison = string.IsNullOrEmpty(incotermCodeItem)
											? null
											: new TConditionsLivraison()
											{
												Codliv = incotermCodeItem,
												Lieuliv = cusProcedure.DeliveryTerms.DeliveryPlace,
												Codelieuincoterm = cusProcedure.DeliveryTerms.IncotermPlace,
											};

			string dateDepotItem = PopulateDeclEmergencyProcDate(cusProcedure);

			genExport.Datdepot = MessageBuilderHelper.GetOptionalString(dateDepotItem);
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

		protected virtual Collection<TArticleExport> PopulateArticlesExport(TGenExport gen)
		{
			#region Tests messages objects

			if (itemIDC?.Articles == null)
			{
				return new Collection<TArticleExport>();
			}

			if (!itemIDC.Articles.Any())
			{
				return new Collection<TArticleExport>();
			}

			#endregion

			var genArticlesImport = new Collection<TArticleExport>();

			foreach (var article in itemIDC.Articles)
			{
				var articleExport = LoadArticleBase(article);

				LoadArticleAdditionalCodes(articleExport, article);

				LoadArticleEconomicRegime(articleExport, article);

				LoadArticleSupplementaryUnitAndWarehouse(articleExport, article);

				LoadArticleEconomicPreference(articleExport, article);

				LoadSeals(articleExport, article);

				#region Packing And Supporting document

				LoadPacking(articleExport, article);

				LoadSupportingDocument(articleExport, article);

				#endregion

				LoadBilling(articleExport, article);

				LoadArticleAutorisationEco(articleExport, article);

				LoadArticleEconomicRegime(articleExport, article);

				#region Financial and statistic properties

				var transportMethodPayment = gen?.Transport?.TransportMethodPayment ?? string.Empty;

				LoadFinancialDatas(articleExport, article, transportMethodPayment);

				LoadStatisticDatas(articleExport, article);
				#endregion

				#region Tax properties

				LoadPreCalcEntryLines(articleExport, article);

				LoadEntryHeaderCharges(articleExport, article);

				LoadSpecialTax(articleExport, article);

				LoadDangerousGoods(articleExport, article);

				#endregion

				#region Special informations properties

				LoadSpecialInformations(articleExport, article);

				#endregion

				genArticlesImport.Add(articleExport);
			}

			return genArticlesImport;
		}

		TArticleExport LoadArticleBase(IArticle article)
		{
			var articleImport = new TArticleExport();
			articleImport.Numart = article.EntryNumber;
			articleImport.Nomenc = article.TariffCode;
			articleImport.Descom = article.EntryLineDescription.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			articleImport.Msb = article.GrossWeight;
			articleImport.Msn = article.CustomsQuantity;
			articleImport.Ori = article.CountryGoodsOrigineCode;

			var valuationMethod = article.ValuationMethod;
			articleImport.Valevaluation = valuationMethod;

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

			articleImport.DonneesFinancieres = new TDonneesFinancieresExport();

			LoadArticleBaseNotRequired(articleImport, article);

			return articleImport;
		}

		void LoadSeals(TArticleExport articleExport, IArticle article)
		{
			articleExport.Scelles = article.Seals;
			if (articleExport.Scelles == 0)
			{
				articleExport.Scelles = null;
			}
			articleExport.Scelleidentites = new Collection<string>(article.SealIds.Select(a => a.ToString()).ToList());
		}

		void LoadArticleBaseNotRequired(TArticleExport articleExport, IArticle article)
		{
			articleExport.RefLogistique = MessageBuilderHelper.GetOptionalString(article.ShippingIdReference);

			var horsTarifItem = new THorsTarif();

			var alternateCalcValueItem = article.AlternateCalcValue;

			horsTarifItem.Horstarif = alternateCalcValueItem?.CalcValue ?? string.Empty;
			horsTarifItem.Motiv = alternateCalcValueItem?.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText) ?? string.Empty;

			articleExport.HorsTarif = string.IsNullOrEmpty(horsTarifItem.Horstarif)
									? null
									: horsTarifItem;

			var observationsItem = article.Observation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

			articleExport.Observations = MessageBuilderHelper.GetOptionalString(observationsItem);
		}

		void LoadArticleAdditionalCodes(TArticleExport articleExport, IArticle article)
		{
			articleExport.Cacos = LoadAdditionnalCodeList(article.CETariffAdditionalCodes);
			articleExport.Canas = LoadAdditionnalCodeList(article.FRTariffAdditionalCodes);
			articleExport.Dispoparts = LoadAdditionnalCodeList(article.PartDispos);
		}

		Collection<string> LoadAdditionnalCodeList(IEnumerable<ITariffAdditionalCode> tariffAdditionalList)
		{
			return messageBuilderHelper.LoadAdditionnalCodeList(tariffAdditionalList);
		}

		void LoadArticleSupplementaryUnitAndWarehouse(TArticleExport articleImport, IArticle article)
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

			if (itemIDC.HasIntoWarehouseProcedure)
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

		void LoadArticleAutorisationEco(TArticleExport articleImport, IArticle article)
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

		void LoadArticleEconomicRegime(TArticleExport articleImport, IArticle article)
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

				var regimeEcoItem = new TRegimeEcoExp(); //"TODO CHECK WI00250026"
				regimeEcoItem.DecEcos = ecoRegimeDataCollection;
				regimeEcoItem.Delapur = article.EcoRegimeDatas.NumberDaysOfDischarge;

				articleImport.RegimeEco = regimeEcoItem;
			}
			else
			{
				articleImport.RegimeEco = null;
			}
		}

		void LoadArticleEconomicPreference(TArticleExport articleImport, IArticle article)
		{
			var cusProcedure = itemIDC?.CusProcedure;
			if ((cusProcedure?.Transport?.ContainerMode ?? ZString.Empty) == "1")
			{
				var numContainerCollection = new Collection<string>();
				article.Containers.Where(x => !x.IsEmpty).ForEach(x => numContainerCollection.Add(x));
				articleImport.Conteneurs = numContainerCollection;
			}
		}

		void LoadPacking(TArticleExport articleImport, IArticle article)
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

		void LoadSupportingDocument(TArticleExport articleImport, IArticle article)
		{
			articleImport.Menspectextes = LoadMenSpecTexteCodeList(article.SpecMens);
			articleImport.Documents = LoadSupportingDocList(article.SupportingDocuments);
		}

		void LoadBilling(TArticleExport articleExport, IArticle article)
		{
			articleExport.Facturation = new TFacturationExport()
			{
				Prifac = article.InvoiceLinePrice.Round(2),
				Coursdevise = article.ExchangeRate,
				Devfac = MessageBuilderHelper.GetOptionalString(article.CurrencyCode)
			};
		}

		void LoadFinancialDatas(TArticleExport articleExport, IArticle article, ZString transportMethodPayment)
		{
			articleExport.DonneesFinancieres.Devfac = MessageBuilderHelper.GetOptionalString(article.CurrencyCode);
			if (article.ShouldSendCustomsStatisticAndVatValues)
			{
				articleExport.DonneesFinancieres.Valdou = article.CustomsValue.Round(0).ToZLong();
				articleExport.DonneesFinancieres.Valstat = article.StatisticalAmount.Round(0).ToZLong();
			}
			articleExport.DonneesFinancieres.TransportMethodPayment = transportMethodPayment;
		}

		void LoadStatisticDatas(TArticleExport articleExport, IArticle article)
		{
			articleExport.DonneesStat = new TDonneesStat() //"TODO CHECK WI00250026"
			{
				Depexp = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.France ? article.ExpeditionDepartment : null
			};
		}

		void LoadPreCalcEntryLines(TArticleExport articleExport, IArticle article)
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

			articleExport.LignesPrecalcs = myPreCalcEntryLinesCollection;
		}

		void LoadEntryHeaderCharges(TArticleExport articleExport, IArticle article)
		{
			if (article.EntryNumber == 1)
			{
				var myPreCalcEntryLinesCollection = articleExport.LignesPrecalcs;
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

				articleExport.LignesPrecalcs = myPreCalcEntryLinesCollection;
			}
		}

		void LoadSpecialTax(TArticleExport articlExport, IArticle article)
		{
			var speTax = article.ThirdUnit;

			articlExport.TaxSpes = speTax == null || speTax.Code.IsEmpty
								? new Collection<TUniSpe>()
								: new Collection<TUniSpe> { LoadUniSpe(speTax) };

			var cusImportCertificationItem = article.CusImportCertification;

			articlExport.Pac = cusImportCertificationItem.IsEmpty
								? null
								: new TPacExport()
								{
									Codpac = ZString.Empty,
									Montantrest = 0,
									Certifexport1 = ZString.Empty,
									Certifexport2 = ZString.Empty,
									Mentionrestit = ZString.Empty,
									Beneficiaire = ZString.Empty,
									Mentioninvar = new Collection<string>(),
									MentionVars = new Collection<TMentionVars>(),
									Infospac = ZString.Empty,
									DateDebutChargement = ZString.Empty,
									HeureDebutChargement = ZString.Empty,
									DateFinChargement = ZString.Empty,
									HeureFinChargement = ZString.Empty
								};
			articlExport.Pac = null;    //TO DO : Either delete the whole piece of code or specify new rule to handle PAC group
		}

		void LoadSpecialInformations(TArticleExport articleExport, IArticle article)
		{
			if (article.Applicant != null && !article.EmptyApplicant() && !(article.IsPlacingGoodsUnderBW && article.HasSpecificRegimeAuthorisation))
			{
				articleExport.EcoSpec = new TEcoSpecExport()
				{
					Natperf = article.ApplicantInwardNature.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Description = article.ApplicantDescription.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Conditions = article.ApplicantConditions.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Burapur = article.ApplicantPurOffice,
					Lieuperf = article.ApplicantInwardLocation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText),
					Formalitestransf = article.ApplicantTransFormality
				};
			}

			articleExport.Infosspec = MessageBuilderHelper.GetOptionalString(article.SpecificInfos);
		}

		void LoadDangerousGoods(TArticleExport articlExport, IArticle article)
		{
			articlExport.DangerousGoodsDeltas = new Collection<string>(article.DangerousGoodsDeltas.Select(a => a.ToString()).ToArray());
		}

		#endregion

		#region Liquidation

		Collection<TLiquidationArticle> PopulateLiquidation()
		{
			var liquidation = new Collection<TLiquidationArticle>();
			foreach (var liquidationItem in itemIDC.Liquidation)
			{
				var myliquidationItem = new TLiquidationArticle();
				myliquidationItem.Numart = liquidationItem.ArticleNumber;
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
			return messageBuilderHelper.LoadTaxationDetails<TTaxationDetail>(liquidationItem, false);
		}

		TUniSpe LoadUniSpe(ISupplementaryUnit suppUnit)
		{
			return messageBuilderHelper.LoadUniSpe<TUniSpe>(suppUnit);
		}

		TTrader LoadTrader(IOrganisation organisation)
		{
			return organisation == null ? null : messageBuilderHelper.LoadTrader<TTrader>(organisation);
		}

		Collection<TTrader> LoadTraders(IEnumerable<IOrganisation> organisations)
		{
			var organisationsCollection = new Collection<TTrader>();

			if (organisations != null)
			{
				foreach (var organisation in organisations)
				{
					organisationsCollection.Add(LoadTrader(organisation));
				}
			}

			return organisationsCollection;
		}

		TDestinataire LoadRecipient(IOrganisation organisation)
		{
			if (organisation == null)
			{
				return null;
			}
			var itemRecipient = new TDestinataire()
			{
				IdDestPartenaire = organisation.PartnerDestIDInfo.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Nomoperateur = organisation.FullName.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Rueoperateur = organisation.Address.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Paysoperateur = organisation.CountryCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Codepostaloperateur = organisation.PostCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Villeoperateur = organisation.City.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Tin = organisation.OrganisationNumberEoriOnly.IsEmpty || organisation.OrganisationNumberEoriOnly.StartsWith(Core.Constants.CountryCodes.UnitedKingdom) ? ZString.Empty : organisation.OrganisationNumberEoriOnly.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText)
			};

			return itemRecipient;
		}

		Collection<TDestinataire> LoadRecipients(IEnumerable<IOrganisation> organisations)
		{
			var recipientCollection = new Collection<TDestinataire>();

			if (organisations != null)
			{
				foreach (var organisation in organisations)
				{
					recipientCollection.Add(LoadRecipient(organisation));
				}
			}

			return recipientCollection;
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
							doc.Mntd48 = supportingDoc.D48Amount.Round(0).ToZLong();
						}

						doc.Deld48 = supportingDoc?.D48Deadline;
						doc.RefPfAid = supportingDoc?.PFAIdentification;
						doc.RefPfAdoc = supportingDoc?.PFADocument;
					}

					doc.FichesImputations = myImputationsSheetList;

					docCollection.Add(doc);
				}
			}

			return docCollection;
		}

		Collection<TFicheImputation> ImputationSheetListOfSupportingDoc(ISupportingDocumentOnly supportingDoc)
		{
			var myImputationsSheetList = new Collection<TFicheImputation>();

			if (supportingDoc != null)
			{
				foreach (var imputationSheet in supportingDoc.ImputationsSheets)
				{
					var lineNumber = imputationSheet?.LineNumber ?? string.Empty;
					if (!string.IsNullOrEmpty(lineNumber))
					{
						myImputationsSheetList.Add(new TFicheImputation
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

			return myImputationsSheetList;
		}

		#endregion

		readonly TransactionTypes messageTransactionType;
		readonly IDeclarationImportExport itemIDC;
		readonly MessageBuilderHelper messageBuilderHelper;
		readonly int newSequenceNumeric;
	}
}
