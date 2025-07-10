using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDSendExpMessageBaseBuilder : MessageBuilderBase<TMessage>
	{
		public DDSendExpMessageBaseBuilder(IDeclarationImportExport declarationExport
			, EU.Business.ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
		{
			itemIDDExport = declarationExport;
			errorCollector = errorCollectorObject;
			messageTransactionType = transactionType;
			this.newSequenceNumeric = (short)newSequenceNumeric;
			messageBuilderHelper = new MessageBuilderHelper();
		}

		protected override TMessage GenerateDeclarationMessage()
		{
			if (errorCollector.ErrorCount > 0) //Todo:del after implementqtrion class)
			{
			}

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
				message.Declaration.Dse = PopulateMainDatasInfos();
			}
			else
			{
				message.Declaration.DseComp = PopulateLiquidationInfos();
			}
		}

		protected virtual bool ShouldPopulateMainDatasInfos => true;

		Tdse PopulateMainDatasInfos()
		{
			var datasDec = new Tdse();
			datasDec.Entete = PopulateHeader();
			datasDec.Gen = PopulateProcedure();
			datasDec.Articles = PopulateArticlesExport();
			return datasDec;
		}

		TdseComp PopulateLiquidationInfos()
		{
			var liquidationSection = new TdseComp();
			liquidationSection.Entete = PopulateHeader();
			liquidationSection.GenComp = PopulateGenComp();
			liquidationSection.ArticlesComp = PopulateArticlesComp();
			return liquidationSection;
		}

		TEnveloppeMessage PopulateEnvelopMessage()
		{
			TEnveloppeMessage envelopeMessage = null;
			var messageEnvelope = itemIDDExport?.MessageEnvelope;
			if (messageEnvelope != null)
			{
				envelopeMessage = new TEnveloppeMessage();
				envelopeMessage.SchemaId = itemIDDExport.MessageEnvelope.SchemaID;
				envelopeMessage.SchemaVersion = itemIDDExport.MessageEnvelope.SchemaVersion;
				if (!messageEnvelope.PartnerId.IsEmpty)
				{
					envelopeMessage.PartyId = messageEnvelope.PartnerId;
				}

				envelopeMessage.TransactionId = itemIDDExport.MessageEnvelope.TransactionId;
				envelopeMessage.Numseq = newSequenceNumeric;
			}

			return envelopeMessage;
		}

		protected virtual TEntete PopulateHeader()
		{
			if (itemIDDExport?.Header == null)
			{
				return null;
			}

			var motivationHeader = itemIDDExport?.Header;
			var motivationItem = PopulateMotivation(motivationHeader);

			var retConvert = Enum.TryParse(motivationHeader?.ActionCode ?? string.Empty, out CodeAct codActEnum) &&
							 Enum.IsDefined(typeof(CodeAct), codActEnum);
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
				motivationItem.Commentaire = commentaire.IsEmpty
					? (ZString)"."
					: commentaire.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

				motivationItem.Justifreg = motivationHeaderMotivation.RegularJustification.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Motiv = motivationHeaderMotivation.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
				motivationItem.Nouvelledest = motivationHeaderMotivation.NewDestination.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			}

			return motivationItem;
		}

		#endregion

		#region Procedure Gen

		protected virtual TGenDse PopulateProcedure()
		{
			#region Tests messages objects

			var cusProcedure = itemIDDExport?.CusProcedure;

			if (cusProcedure?.Office == null)
			{
				return null;
			}

			#endregion

			var genExport = LoadProcedureBase(cusProcedure);

			LoadProcedureOfficeAndOperator(genExport, cusProcedure);

			LoadProcedureTransport(genExport, cusProcedure);

			LoadProcedurePreval(genExport, cusProcedure);

			LoadProcedureOperator(genExport, cusProcedure);

			LoadProcedureDeliveryAndOthersCost(genExport, cusProcedure);

			return genExport;
		}

		TGenDse LoadProcedureBase(ICusProcedure cusProcedure)
		{
			var genExport = new TGenDse();
			genExport.Procedure1 = cusProcedure.EntryStyle;
			genExport.Procedure2 = cusProcedure.EntryStyleCode;
			genExport.Nbrart = cusProcedure.ArticleCount;
			genExport.Dest = cusProcedure.DestinationState;
			genExport.Itinerary = LoadItinerary(cusProcedure);
			return genExport;
		}

		void LoadProcedureOfficeAndOperator(TGenDse genExport, ICusProcedure cusProcedure)
		{
			genExport.Bureau = new TBureau();
			genExport.Bureau.Burdom = cusProcedure.Office?.OfficeOfDeclaration ?? string.Empty;
			genExport.Bureau.Burrat = cusProcedure.Office?.OfficeOfLodgement ?? string.Empty;
			genExport.Bureau.Bureausortie = MessageBuilderHelper.GetOptionalString(cusProcedure.Office?.ExitOffice);

			if (!itemIDDExport.IsOfficeOfLodgementDifferentFromOfficeOfExit)
			{
				var typeSortie = cusProcedure.Office?.ECSExitType;
				var motivSortie = cusProcedure.Office?.ECSMotivation;
				genExport.Bureau.Sortie = new TBureauSortie()
				{
					Typesortie = MessageBuilderHelper.GetOptionalString(typeSortie),
					Motiv = MessageBuilderHelper.GetOptionalString(motivSortie)
				};
			}
		}

		void LoadProcedureTransport(TGenDse genExport, ICusProcedure cusProcedure)
		{
			var transportItem = cusProcedure.Transport;

			var genTransport = new TTransportExp();

			genTransport.Inttra = transportItem?.ModeOfTRansportInland ?? string.Empty;
			genTransport.Conteneurtra = transportItem?.ContainerMode ?? string.Empty;
			genTransport.Idtransport = transportItem?.TransportID ?? string.Empty;
			genTransport.TransportMethodPayment = transportItem?.TransportMethodPayment ?? string.Empty;
			genExport.TransportExp = genTransport;
		}

		Collection<string> LoadItinerary(ICusProcedure cusProcedure)
		{
			return new Collection<string>(cusProcedure.Itinerary.Select(x => x.ToString()).ToArray());
		}

		void LoadProcedurePreval(TGenDse genExport, ICusProcedure cusProcedure)
		{
			var prevalItem = PopulatePreval(cusProcedure);

			genExport.Preval = string.IsNullOrEmpty(prevalItem?.Datpreval ?? ZString.Empty)
				? null
				: prevalItem;

			genExport.Locagr = MessageBuilderHelper.GetOptionalString(cusProcedure.AgreedGoodsLocation);

			genExport.Magasin = MessageBuilderHelper.GetOptionalString(cusProcedure.ClearanceLocation);

			genExport.Nbrcol = cusProcedure.PackageCount;

			var etatMembreExportationReel = MessageBuilderHelper.GetOptionalString(cusProcedure.DepartureState);
			genExport.EtatMembreExportationReel = etatMembreExportationReel == Constants.CountryCodes.France ? string.Empty : etatMembreExportationReel;

			genExport.SpecificCircumstanceIndicator = MessageBuilderHelper.GetOptionalString(cusProcedure.SpecificCircumstanceIndicator);
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

		void LoadProcedureOperator(TGenDse genExport, ICusProcedure cusProcedure)
		{
			var operateur = new TOperateur()
			{
				Opeben = cusProcedure.AgreementOwnerEORI,
				Numagr = cusProcedure.DeltaGAuthorisationNumber,
				Operep = cusProcedure.BranchCusBrokerageCode,
				Modrep = cusProcedure.RepresentationModeCode,
			};

			var suppliersItem = cusProcedure.Suppliers.FirstOrDefault();

			operateur.Expediteur = LoadTrader(suppliersItem, true)?.Tin;

			var importersItem = cusProcedure.Importers.FirstOrDefault();

			operateur.Destinataire = LoadTrader(importersItem, false);

			var deliveryAddress = cusProcedure.ImporterDeliveryAddress;

			operateur.Destinatairefinal = LoadRecipient(deliveryAddress);

			var repTaxOrganisationItem = cusProcedure.RepTaxOrganisation;

			operateur.RepFisc = LoadTrader(repTaxOrganisationItem, true);

			operateur.Numcre = MessageBuilderHelper.GetOptionalString(cusProcedure.DeferalApprovalCreditNumber);

			operateur.Numcod = MessageBuilderHelper.GetOptionalString(cusProcedure.VariousOperationCreditNumber);

			genExport.Operateur = operateur;
		}

		void LoadProcedureDeliveryAndOthersCost(TGenDse genExport, ICusProcedure cusProcedure)
		{
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

		protected virtual Collection<TArticleDse> PopulateArticlesExport()
		{
			#region Tests messages objects

			if (itemIDDExport?.Articles == null)
			{
				return new Collection<TArticleDse>();
			}

			if (!itemIDDExport.Articles.Any())
			{
				return new Collection<TArticleDse>();
			}

			#endregion

			var genArticlesExport = new Collection<TArticleDse>();

			foreach (var article in itemIDDExport.Articles)
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

				LoadArticleAutorisationEco(articleExport, article);

				LoadSpecialTax(articleExport, article);

				genArticlesExport.Add(articleExport);
			}

			return genArticlesExport;
		}

		TArticleDse LoadArticleBase(IArticle article)
		{
			var articleExport = new TArticleDse();

			articleExport.Numart = article.EntryNumber;
			articleExport.Nomenc = article.TariffCode;
			articleExport.Descom = article.EntryLineDescription.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);
			articleExport.Msb = article.GrossWeight.Round(0);
			articleExport.Msn = article.CustomsQuantity.Round(0);

			var procedureItem = new TRegimeDouanier();
			procedureItem.Regdou = article.ProcedureCode;
			procedureItem.Regdoupre = article.PreviousCode;
			procedureItem.Compcom = article.Concession;
			articleExport.RegimeDouanier = procedureItem;

			var supportDocItem = new TPriseEnCharge();
			var previousDocItem = article.PreviousDocument;
			supportDocItem.Natdocpec = previousDocItem?.Code ?? string.Empty;
			supportDocItem.Typdocpec = previousDocItem?.Type ?? string.Empty;
			supportDocItem.Refdocpec = previousDocItem?.RefNumber ?? string.Empty;
			articleExport.PriseEnCharge = supportDocItem;

			var invoiceLinePriceItem = new TFacturation();
			invoiceLinePriceItem.Prifac = article.InvoiceLinePrice.Round(2);
			invoiceLinePriceItem.Devfac = article.CurrencyCode;
			invoiceLinePriceItem.TransportMethodPayment = MessageBuilderHelper.GetOptionalString(article.TransportMethodPayment);
			articleExport.Facturation = invoiceLinePriceItem;

			LoadArticleBaseNotRequired(articleExport, article);

			return articleExport;
		}

		void LoadSeals(TArticleDse articleExport, IArticle article)
		{
			articleExport.Scelles = article.Seals;
			if (articleExport.Scelles == 0)
			{
				articleExport.Scelles = null;
			}

			articleExport.Scelleidentites = new Collection<string>(article.SealIds.Select(a => a.ToString()).ToArray());
		}

		void LoadArticleBaseNotRequired(TArticleDse articleExport, IArticle article)
		{
			articleExport.RefLogistique = MessageBuilderHelper.GetOptionalString(article.ShippingIdReference);

			var horsTarifItem = new THorsTarif();

			var alternateCalcValueItem = article.AlternateCalcValue;

			horsTarifItem.Horstarif = alternateCalcValueItem?.CalcValue ?? string.Empty;
			if (horsTarifItem.Horstarif == "E")
			{
				horsTarifItem.Motiv = alternateCalcValueItem?.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText) ?? string.Empty;
			}

			articleExport.HorsTarif = string.IsNullOrEmpty(horsTarifItem.Horstarif)
				? null
				: horsTarifItem;

			var observationsItem = article.Observation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

			articleExport.Observations = MessageBuilderHelper.GetOptionalString(observationsItem);
		}

		void LoadArticleAdditionalCodes(TArticleDse articleExport, IArticle article)
		{
			articleExport.Cacos = LoadAdditionnalCodeList(article.CETariffAdditionalCodes);
			articleExport.Canas = LoadAdditionnalCodeList(article.FRTariffAdditionalCodes);
			articleExport.Dispoparts = LoadAdditionnalCodeList(GetPartDispos(article));

			var dangerousGoodsDeltasItem = article.DangerousGoodsDeltas;
			articleExport.DangerousGoodsDeltas = dangerousGoodsDeltasItem == null || !dangerousGoodsDeltasItem.Any()
				? new Collection<string>()
				: new Collection<string>(dangerousGoodsDeltasItem.Select(x => x.ToString()).ToArray());

			var quotaRefNumberItem = article.SealIds;
			if (quotaRefNumberItem != null && quotaRefNumberItem.Any())
			{
				articleExport.Scelleidentites = new Collection<string>(quotaRefNumberItem.Select(x => x.ToString()).ToArray());
			}
		}

		void LoadArticleSupplementaryUnitAndWarehouse(TArticleDse articleExport, IArticle article)
		{
			var unitSpeItem = new TUniSpe()
			{
				Unispe = article.SuppUnit?.Code ?? string.Empty,
				Qualifunispe = article.SuppUnit?.Qualif ?? string.Empty,
				Nbrunispe = article.SuppUnit?.Qty ?? default
			};

			articleExport.UniSpe = string.IsNullOrEmpty(unitSpeItem.Unispe) || unitSpeItem.Nbrunispe == decimal.Zero
									? null
									: unitSpeItem;

			if (itemIDDExport.HasIntoWarehouseProcedure)
			{
				var warehouseItem = new TEntrepot()
				{
					Enttyp = article.WarehouseType,
					Entref = article.WarehouseReference,
					Entpays = article.WarehouseCountryCode
				};

				articleExport.Entrepot = warehouseItem;
			}
			else
			{
				articleExport.Entrepot = null;
			}
		}

		void LoadArticleAutorisationEco(TArticleDse articleExport, IArticle article)
		{
			var ecoRegimeAuthorizationItem = new TAutorisationEco();

			if (article.EcoRegimeAuthorization != null)
			//EcoRegimeAuthorization is null when the instruction shows no authorization of expected type.
			{
				ecoRegimeAuthorizationItem.Autorisationeco = article.EcoRegimeAuthorization.EcoRegimeAuthorizationNumber;
				ecoRegimeAuthorizationItem.Autorisationpays = article.EcoRegimeAuthorization.EcoRegimeCountryCode;

				articleExport.AutorisationEco = ecoRegimeAuthorizationItem;
			}
			else
			{
				articleExport.AutorisationEco = null;
			}
		}

		void LoadArticleEconomicRegime(TArticleDse articleExport, IArticle article)
		{
			if (article.EcoRegimeDatas != null && article.EcoRegimeDatas.DecEcos != null && article.EcoRegimeAuthorization != null)
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

				articleExport.RegimeEco = regimeEcoItem;
			}
			else
			{
				articleExport.RegimeEco = null;
			}
		}

		void LoadArticleEconomicPreference(TArticleDse articleExport, IArticle article)
		{
			var cusProcedure = itemIDDExport?.CusProcedure;
			if ((cusProcedure?.Transport?.ContainerMode ?? ZString.Empty) == "1")
			{
				var numContainerCollection = new Collection<string>();
				article.Containers.Where(x => !x.IsEmpty).ForEach(x => numContainerCollection.Add(x));
				articleExport.Conteneurs = numContainerCollection;
			}
		}

		void LoadPacking(TArticleDse articleExport, IArticle article)
		{
			var colisageItem = new TColisageEx()
			{
				Nbrcol = article.Packing?.Count,

				Nbrpieces = article.Packing?.ItemsCount,

				Natcol = MessageBuilderHelper.GetOptionalString(article.Packing?.Type),

				Marquecolis = MessageBuilderHelper.GetOptionalString(article.Packing?.MarksAndNos.Left(MessageBuilderHelper.CW_MarksAndNosMaxLengthForMessage))
			};

			articleExport.Colisage = string.IsNullOrEmpty(colisageItem.Natcol)
									? null
									: colisageItem;
		}

		void LoadSupportingDocument(TArticleDse articleExport, IArticle article)
		{
			articleExport.Menspecs = messageBuilderHelper.LoadMenSpecTexteCodeList<TMenspectexte>(article.SpecMens);

			var supportingDocumentsItem = GetSupportingDocumentsItem(article);
			articleExport.Documents = LoadSupportingDocList(supportingDocumentsItem);
		}

		protected virtual IEnumerable<ISupportingDocumentOnly> GetSupportingDocumentsItem(IArticle article)
		{
			return article.SupportingDocuments;
		}

		void LoadSpecialTax(TArticleDse articlExport, IArticle article)
		{
			articlExport.Pac = new TPacExport()
			{
				Codpac = ZString.Empty,
				Montantrest = ZLong.Zero,
				Certifexport1 = ZString.Empty,
				Certifexport2 = ZString.Empty,
				Mentionrestit = ZString.Empty,
				Beneficiaire = ZString.Empty,
				Mentioninvar = new Collection<string>(),
				MentionVars = new Collection<TMentionVars>(),
				Infospac = article.PACInformations,
				DateDebutChargement = ZString.Empty,
				HeureDebutChargement = ZString.Empty,
				DateFinChargement = ZString.Empty,
				HeureFinChargement = ZString.Empty
			};

			articlExport.Pac = null;    //TO DO : Either delete the whole piece of code or specify new rule to handle PAC group
		}

		#endregion

		#region GenComp
		protected virtual TGenDsEcomp PopulateGenComp()
		{
			#region Tests messages objects

			var cusProcedure = itemIDDExport?.CusProcedure;

			if (cusProcedure?.Office == null)
			{
				return null;
			}

			#endregion

			var genCompExport = LoadGenComp(cusProcedure);

			genCompExport.OperateurComp = LoadGenCompOperateurComp(cusProcedure);

			LoadGenCompConditionsLivraison(genCompExport, cusProcedure);

			LoadGenCompTransport(genCompExport, cusProcedure);

			return genCompExport;
		}

		protected TGenDsEcomp LoadGenComp(ICusProcedure cusProcedure)
		{
			var genCompExport = new TGenDsEcomp()
			{
				Nattrans = cusProcedure.TransactionNature,
			};

			return genCompExport;
		}

		protected virtual TOperateurDseComp LoadGenCompOperateurComp(ICusProcedure cusProcedure)
		{
			var operateurComp = new TOperateurDseComp
			{
				Numcod = MessageBuilderHelper.GetOptionalString(cusProcedure.VariousOperationCreditNumber),
			};

			return operateurComp;
		}

		protected void LoadGenCompConditionsLivraison(TGenDsEcomp genCompExport, ICusProcedure cusProcedure)
		{
			var conditionsLivraison = new TConditionsLivraison
			{
				Codliv = cusProcedure.DeliveryTerms.IncotermCode,
				Lieuliv = cusProcedure.DeliveryTerms.DeliveryPlace,
				Codelieuincoterm = cusProcedure.DeliveryTerms.IncotermPlace,
			};

			genCompExport.ConditionsLivraison = conditionsLivraison;
		}

		protected void LoadGenCompTransport(TGenDsEcomp genCompExport, ICusProcedure cusProcedure)
		{
			var transport = new TTransportExpComp
			{
				Inttra = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.ModeOfTRansportInland),
				Conteneurtra = LoadContainerMode(cusProcedure),
				Natfrotra = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.NationalityOfTransport),
			};

			if (cusProcedure.EntryStyle != "FR")
			{
				transport.Modfrotra = MessageBuilderHelper.GetOptionalString(cusProcedure.Transport.ModeOfTRansport);
			}

			genCompExport.TransportExpComp = transport;
		}

		protected virtual ZString LoadContainerMode(ICusProcedure cusProcedure)
		{
			return cusProcedure.Transport.ContainerMode;
		}

		#endregion

		#region ArticlesComp
		protected virtual Collection<TArticleDseComp> PopulateArticlesComp()
		{
			#region Tests messages objects

			if (itemIDDExport?.Articles == null)
			{
				return new Collection<TArticleDseComp>();
			}

			if (!itemIDDExport.Articles.Any())
			{
				return new Collection<TArticleDseComp>();
			}

			#endregion

			var genArticlesExport = new Collection<TArticleDseComp>();

			foreach (var article in itemIDDExport.Articles)
			{
				var articleExport = LoadArticleCompBase(article);

				LoadArticleCompAdditionnalCodes(articleExport, article);

				#region Supporting document

				LoadArticleCompSupportingDocument(articleExport, article);

				#endregion

				LoadArticleCompFinancialDatas(articleExport, article);
				LoadArticleCompPreCalcEntryLines(articleExport, article);
				LoadArticleCompSpecialTax(articleExport, article);
				LoadArticleCompStatisticDatas(articleExport, article);
				genArticlesExport.Add(articleExport);
			}

			return genArticlesExport;
		}

		TArticleDseComp LoadArticleCompBase(IArticle article)
		{
			var articleExport = new TArticleDseComp();

			articleExport.Numart = article.EntryNumber;

			articleExport.Valevaluation = article.ValuationMethod;

			articleExport.Ori = article.CountryGoodsOrigineCode;

			LoadArticleCompBaseNotRequired(articleExport, article);

			return articleExport;
		}

		void LoadArticleCompBaseNotRequired(TArticleDseComp articleExport, IArticle article)
		{
			var horsTarifItem = new THorsTarif();

			var alternateCalcValueItem = article.AlternateCalcValue;

			horsTarifItem.Horstarif = MessageBuilderHelper.GetOptionalString(alternateCalcValueItem?.CalcValue);
			if (horsTarifItem.Horstarif == "E")
			{
				horsTarifItem.Motiv = alternateCalcValueItem?.Motivation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText) ?? string.Empty;
			}

			articleExport.HorsTarif = string.IsNullOrEmpty(horsTarifItem.Horstarif)
									? null
									: horsTarifItem;

			var observationsItem = article.Observation.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersLongText);

			articleExport.Observations = MessageBuilderHelper.GetOptionalString(observationsItem);
		}

		void LoadArticleCompAdditionnalCodes(TArticleDseComp articleExport, IArticle article)
		{
			var additionalCECodeItem = GetCETariffAdditionalCodes(article);
			articleExport.Cacos = LoadAdditionnalCodeList(additionalCECodeItem);

			var additionalFRCodeItem = GetFRTariffAdditionalCodes(article);
			articleExport.Canas = LoadAdditionnalCodeList(additionalFRCodeItem);

			var parDisposItem = GetPartDispos(article);
			articleExport.Dispoparts = LoadAdditionnalCodeList(parDisposItem);
		}

		protected virtual IEnumerable<ITariffAdditionalCode> GetCETariffAdditionalCodes(IArticle article)
		{
			return article.CETariffAdditionalCodes;
		}

		protected virtual IEnumerable<ITariffAdditionalCode> GetFRTariffAdditionalCodes(IArticle article)
		{
			return article.FRTariffAdditionalCodes;
		}

		protected virtual IEnumerable<ITariffAdditionalCode> GetPartDispos(IArticle article)
		{
			return article.PartDispos;
		}

		void LoadArticleCompSupportingDocument(TArticleDseComp articleExport, IArticle article)
		{
			var supportingDocumentsItem = GetSupportingDocumentsItem(article);

			articleExport.Documents = LoadSupportingDocList(supportingDocumentsItem);
		}

		void LoadArticleCompFinancialDatas(TArticleDseComp articleExport, IArticle article)
		{
			if (article.ShouldSendCustomsStatisticAndVatValues)
			{
				articleExport.DonneesFinancieresComp = new TDonneesFinancieresDseComp
				{
					Valdou = article.CustomsValue.Round(0).ToZInt(),
				};
			}
		}

		void LoadArticleCompStatisticDatas(TArticleDseComp articleExport, IArticle article)
		{
			articleExport.DonneesStat = new TDonneesStat()
			{
				Depexp = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.France ? article.ExpeditionDepartment : null
			};
		}

		void LoadArticleCompPreCalcEntryLines(TArticleDseComp articleExport, IArticle article)
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

			articleExport.LignesPrecalcs = myPreCalcEntryLinesCollection;
		}

		void LoadArticleCompSpecialTax(TArticleDseComp articleExport, IArticle article)
		{
			var speTax = article.ThirdUnit;

			articleExport.TaxSpes = speTax == null || speTax.Code.IsEmpty
								? new Collection<TUniSpe>()
								: new Collection<TUniSpe> { LoadUniSpe(speTax) };

			var cusImportCertificationItem = article.CusImportCertification;

			articleExport.Pac = cusImportCertificationItem.IsEmpty
								? null
								: new TPacExport()
								{
									Codpac = article.PACCode,
									Montantrest = article.Restitution.Round(0).ToZInt(),
									Certifexport1 = article.ExportCertification1,
									Certifexport2 = article.ExportCertification2,
									Mentionrestit = article.RestitutionMention,
									Beneficiaire = article.Recipient,
									Mentioninvar = new Collection<string> { article.InvariantMention },
									MentionVars = new Collection<TMentionVars>
									{
										new TMentionVars
										{
											Mentionvar = article.VariantMention,
											Tauxvar = article.VariantRate,
										}
									},
									Infospac = article.PACInformations,
									DateDebutChargement = article.LoadingStartDate.ToString(),
									HeureDebutChargement = article.LoadingStartHour.ToString(),
									DateFinChargement = article.LoadingEndDate.ToString(),
									HeureFinChargement = article.LoadingEndHour.ToString(),
								};

			articleExport.Pac = null;   //TO DO : Either delete the whole piece of code or specify new rule to handle PAC group
		}

		#endregion

		#region Populate methodes tools

		TUniSpe LoadUniSpe(ISupplementaryUnit suppUnit)
		{
			return messageBuilderHelper.LoadUniSpe<TUniSpe>(suppUnit);
		}

		TTrader LoadTrader(IOrganisation organisation, bool allowFallbakToVatNumberInsteadofEoriForTIN)
		{
			return organisation == null ? null : messageBuilderHelper.LoadTrader<TTrader>(organisation, allowFallbakToVatNumberInsteadofEoriForTIN);
		}

		TTraderFinal LoadRecipient(IOrganisation organisation)
		{
			if (organisation == null)
			{
				return null;
			}

			var itemRecipient = new TTraderFinal()
			{
				IdDestPartenaire = organisation.PartnerDestIDInfo.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Tin = organisation.EoriCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Nomoperateur = organisation.FullName.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Rueoperateur = organisation.Address.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Paysoperateur = organisation.CountryCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Codepostaloperateur = organisation.PostCode.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText),
				Villeoperateur = organisation.City.SubstringSafe(0, MessageBuilderBaseExtension.NumberOfCharactersShortText)
			};

			return itemRecipient;
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
							doc.Mntd48 = supportingDoc.D48Amount.Round(0).ToZLong();
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
		readonly IDeclarationImportExport itemIDDExport;
		readonly EU.Business.ErrorCollector errorCollector;
		readonly MessageBuilderHelper messageBuilderHelper;
		readonly short newSequenceNumeric;
	}
}
