using System;
using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Testing;
using Moq;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	public abstract class DDSendMessageBuilderTest<TMessageBuilder> : TestCaseWithFactory
		where TMessageBuilder : IMessageBuilderBase
	{
		protected virtual void FillSomeFieldsForTheEntry(CusEntryHeader entry) { }

		protected abstract ZBool IsImport { get; }

		protected abstract ZString MessageType { get; }

		protected abstract ZString[] ItemsNotContains { get; }

		protected abstract ZString[] ItemsContains { get; }

		public void TestSend()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(false, IsImport);
			FillSomeFieldsForTheEntry(entry);
			var message = deltaHelper.GetFlatMessage(MessageType, entry);

			foreach (var item in ItemsNotContains)
			{
				AssertNotContains(item, message);
			}

			foreach (var item in ItemsContains)
			{
				AssertContains(item, message);
			}
		}

		public void TestGetMessageBuilder()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);
			var messageUtf8StartValid = @"<?xml version=""1.0"" encoding=""utf-8""?><Message";

			AssertContains(messageUtf8StartValid, message);
		}

		public virtual void TestAeroportembAndAertratag()
		{
			CreateDeclarationMockDNM(MessageType, IsImport);

			declarationMock.Setup(x => x.CusProcedure.Transport.ModeOfTRansport).Returns("4");
			declarationMock.Setup(x => x.CusProcedure.Transport.AirRoadType).Returns("1");
			declarationMock.Setup(x => x.CusProcedure.Transport.IATAAirportOfLoading).Returns("CAN");

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertContains("<aeroportemb>", message);
			AssertContains("<aertra>", message);

			declarationMock.Setup(x => x.CusProcedure.Transport.ModeOfTRansport).Returns("1");
			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertNotContains("<aeroportemb>", message);
			AssertNotContains("<aertra>", message);
		}

		protected void HelpTestingPopulateDeclEmergencyProcDate()
		{
			CreateDeclarationMock(MessageType, IsImport);

			myDeclGen.Setup(m => m.DeclEmergencyProcDate).Returns(ZString.Empty);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);

			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			AssertNotContains("<datdepot>", message);

			var todayDateString = ZDate.Today.ToString("dd/MM/yyyy");
			myDeclGen.Setup(m => m.DeclEmergencyProcDate).Returns(todayDateString);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);

			messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			message = messageBuilder.GetMessage();
			AssertContains($"<datdepot>{todayDateString}</datdepot>", message);
		}

		protected TMessageBuilder CreateMessageBuilder(string actionCode, bool isImport = true)
		{
			var declarationObject = declarationMock.Object;
			var messageBuilder = Activator.CreateInstance(typeof(TMessageBuilder), declarationObject, new ErrorCollector(), TransactionTypes.Original, 999);

			if (isImport)
			{
				messageBuilder = actionCode switch
				{
					EntryActionCodeList.Codes.REC => new DDRECSendImpMessageBuilder(declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
					EntryActionCodeList.Codes.ANT => new DDANTSendImpMessageBuilder(declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
					EntryActionCodeList.Codes.MDA => new DDMDASendImpMessageBuilder(declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
					_ => Activator.CreateInstance(typeof(TMessageBuilder), declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
				};
			}
			else
			{
				messageBuilder = actionCode switch
				{
					EntryActionCodeList.Codes.REC => new DDRECSendExpMessageBuilder(declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
					EntryActionCodeList.Codes.ANT => new DDANTSendExpMessageBuilder(declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
					EntryActionCodeList.Codes.MDA => new DDMDASendExpMessageBuilder(declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
					_ => Activator.CreateInstance(typeof(TMessageBuilder), declarationObject, new ErrorCollector(), TransactionTypes.Original, 999),
				};
			}

			return (TMessageBuilder)messageBuilder;
		}

		#region Mocking Functions

		protected void CreateDeclarationMock(string actionCode, bool import = true)
		{
			#region Meta Datas mock

			var myMetaDatas = new Mock<IMetaData> { CallBase = true };
			myMetaDatas.Setup(m => m.Application).Returns("DELTAD");
			myMetaDatas.Setup(m => m.DeclarationReference).Returns("859623");
			myMetaDatas.Setup(m => m.EntryNumberType).Returns(import ? "IMP" : "EXP");

			#endregion

			#region MessageEnvelop properties mock

			var myMessageEnvelop = new Mock<IMessageEnvelope> { CallBase = true };
			myMessageEnvelop.Setup(m => m.SchemaID).Returns(import ? "MessageDDecImp" : "MessageDDecExp");
			myMessageEnvelop.Setup(m => m.SchemaVersion).Returns("1032013");
			myMessageEnvelop.Setup(m => m.PartnerId).Returns("33159700500064");
			myMessageEnvelop.Setup(m => m.TransactionId).Returns("94321-B00175768");
			myMessageEnvelop.Setup(m => m.NumSeq).Returns(new ZShort("0"));

			#endregion

			#region Entete properties mock

			var myDeclHeader = new Mock<IHeader> { CallBase = true };

			if (actionCode == EntryActionCodeList.Codes.REC)
			{
				var myMessageRectificationMotiv = new Mock<IMotivation> { CallBase = true };
				myMessageRectificationMotiv.Setup(m => m.NewDestination).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.RegularJustification).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.Comment).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.RegularJustification).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.Motivation).Returns("Rectification explanation");

				myDeclHeader.Setup(m => m.Motivation).Returns(myMessageRectificationMotiv.Object);
			}

			myDeclHeader.Setup(m => m.ActionCode).Returns(EntryActionCodeList.GetMessageCodeNumber(actionCode).ToString());

			#region Reference properties mock

			var myDeclReferences = new Mock<IReferences> { CallBase = true };
			myDeclReferences.Setup(m => m.CusDeclarationNumber).Returns(new ZString(null));
			myDeclReferences.Setup(m => m.OwnerDeclarationIdentification).Returns("8461132");

			myDeclHeader.Setup(m => m.References).Returns(myDeclReferences.Object);

			#endregion

			#endregion

			#region Gen properties mock

			myDeclGen = new Mock<ICusProcedure> { CallBase = true };

			var myImpAlternateCalcValue = new Mock<IAlternateCalcValue> { CallBase = true };
			myImpAlternateCalcValue.Setup(m => m.CalcValue).Returns(new ZString(null));
			myImpAlternateCalcValue.Setup(m => m.Motivation).Returns(new ZString(null));

			myDeclGen.Setup(m => m.AlternateCalcValue).Returns(myImpAlternateCalcValue.Object);

			myDeclGen.Setup(m => m.ProcedureType).Returns("D");
			myDeclGen.Setup(m => m.EntryStyle).Returns("IM");
			myDeclGen.Setup(m => m.EntryStyleCode).Returns("A");
			myDeclGen.Setup(m => m.ArticleCount).Returns(new ZShort("2"));
			myDeclGen.Setup(m => m.Itinerary).Returns(Array.Empty<ZString>());
			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);

			#region Prevalidation

			myDeclGen.Setup(m => m.EstimatedAssessmentDate).Returns(new ZString("02/01/2020"));
			myDeclGen.Setup(m => m.EstimatedAssessmentHour).Returns(new ZString("01"));
			myDeclGen.Setup(m => m.DeclEmergencyProcDate).Returns(new ZString(null));

			myDeclGen.Setup(m => m.PackageCount).Returns(new ZInt("1"));
			myDeclGen.Setup(m => m.AgreedGoodsLocation).Returns("34796082500052/1");
			myDeclGen.Setup(m => m.ClearanceLocation).Returns("CDGSO1");
			myDeclGen.Setup(m => m.TransactionNature).Returns("99");
			myDeclGen.Setup(m => m.OrigineState).Returns("FR");
			myDeclGen.Setup(m => m.SpecificCircumstanceIndicator).Returns("Circum");
			myDeclGen.Setup(m => m.DepartureState).Returns("DE");

			#region Office properties mock

			var myDeclGenOffice = new Mock<ICusOffice> { CallBase = true };
			myDeclGenOffice.Setup(m => m.OfficeOfDeclaration).Returns("FRB0617A");
			myDeclGenOffice.Setup(m => m.OfficeOfLodgement).Returns("FRB0617A");
			myDeclGenOffice.Setup(m => m.VisitingOffice).Returns(new ZString(null));
			if (!import)
			{
				myDeclGenOffice.Setup(m => m.ECSExitType).Returns("STC");
				myDeclGenOffice.Setup(m => m.ECSMotivation).Returns("BLABLA");
				myDeclGenOffice.Setup(m => m.ExitOffice).Returns("AT330100");
			}
			myDeclGen.Setup(m => m.Office).Returns(myDeclGenOffice.Object);

			#endregion

			#endregion

			#region Operator

			#region Supplier properties mock

			var myDeclImportSupplier = new Mock<IOrganisation> { CallBase = true };

			myDeclImportSupplier.Setup(m => m.OrganisationNumber).Returns("ETRANGER");
			myDeclImportSupplier.Setup(m => m.FullName).Returns("GE HEALTHCARE");
			myDeclImportSupplier.Setup(m => m.Address).Returns("775 EAST DRIVE DOCK DOORS");
			myDeclImportSupplier.Setup(m => m.CountryCode).Returns("US");
			myDeclImportSupplier.Setup(m => m.PostCode).Returns("60188");
			myDeclImportSupplier.Setup(m => m.City).Returns("STERLING");

			myDeclGen.Setup(m => m.Suppliers).Returns(new List<IOrganisation> { myDeclImportSupplier.Object });

			#endregion

			#region Importer properties mock

			var myDeclImportImporter = new Mock<IOrganisation> { CallBase = true };

			myDeclImportImporter.Setup(m => m.OrganisationNumber).Returns("FR31501335900155");
			myDeclImportImporter.Setup(m => m.OrganisationNumberEoriOnly).Returns("FR3150EORIONLY");
			myDeclImportImporter.Setup(m => m.FullName).Returns("GE MEDICAL SYSTEMMS SCS");
			myDeclImportImporter.Setup(m => m.Address).Returns("RUE DE LA MINIERE");
			myDeclImportImporter.Setup(m => m.CountryCode).Returns("FR");
			myDeclImportImporter.Setup(m => m.PostCode).Returns("78533");
			myDeclImportImporter.Setup(m => m.City).Returns("BUC CEDEX");
			myDeclImportImporter.Setup(m => m.PartnerDestIDInfo).Returns("");

			myDeclGen.Setup(m => m.Importers).Returns(new List<IOrganisation> { myDeclImportImporter.Object });

			#endregion

			#region Representative tax

			var myDeclRepresentative = new Mock<IOrganisation> { CallBase = true };
			myDeclRepresentative.Setup(m => m.OrganisationNumber).Returns("FR32582075100080");
			myDeclRepresentative.Setup(m => m.FullName).Returns("Cosfibel HK Limited");
			myDeclRepresentative.Setup(m => m.Address).Returns("FLAT CTOD 3FL SUMWAY MANSION");
			myDeclRepresentative.Setup(m => m.CountryCode).Returns("HK");
			myDeclRepresentative.Setup(m => m.PostCode).Returns("55102");
			myDeclRepresentative.Setup(m => m.City).Returns("KENNEDY TOWN");
			myDeclGen.Setup(m => m.RepTaxOrganisation).Returns(myDeclRepresentative.Object);

			myDeclGen.Setup(m => m.AgreementOwnerEORI).Returns("FR34430738400570");
			if (import)
			{
				myDeclGen.Setup(m => m.ImporterEORINumber).Returns("FR34430738400571");
			}

			myDeclGen.Setup(m => m.DeltaGAuthorisationNumber).Returns("00000169");
			myDeclGen.Setup(m => m.BranchCusBrokerageCode).Returns("FR34430738400570");
			myDeclGen.Setup(m => m.RepresentationModeCode).Returns("2");
			myDeclGen.Setup(m => m.DeferalApprovalCreditNumber).Returns("ALLI");
			myDeclGen.Setup(m => m.VariousOperationCreditNumber).Returns("ALLI");

			#endregion

			#endregion

			myDeclGen.Setup(m => m.EntryGoodsPriceSum).Returns(4254.12);
			myDeclGen.Setup(m => m.EntryGoodsPriceCurrency).Returns("EUR");
			myDeclGen.Setup(m => m.EntryGoodsPriceCurrencyRate).Returns(new ZDecimal(null));
			myDeclGen.Setup(m => m.PaymentMode).Returns("R");
			myDeclGen.Setup(m => m.GuaranteeMode).Returns("C");
			if (!import)
			{
				myDeclGen.Setup(m => m.OrigineState).Returns("FR");
			}
			myDeclGen.Setup(m => m.DestinationState).Returns("GB");

			#region Delivery Terms properties mock

			var myDeclImpDeliveryTerms = new Mock<IDeliveryTerms> { CallBase = true };
			myDeclImpDeliveryTerms.Setup(m => m.IncotermCode).Returns("FCA");
			myDeclImpDeliveryTerms.Setup(m => m.DeliveryPlace).Returns("STERLING");
			myDeclImpDeliveryTerms.Setup(m => m.IncotermPlace).Returns("3");

			myDeclGen.Setup(m => m.DeliveryTerms).Returns(myDeclImpDeliveryTerms.Object);

			#endregion

			#region Transport properties mock

			myDeclImpTransport = new Mock<ITransport> { CallBase = true };
			myDeclImpTransport.Setup(m => m.ModeOfTRansport).Returns("4");
			myDeclImpTransport.Setup(m => m.ModeOfTRansportInland).Returns("3");
			myDeclImpTransport.Setup(m => m.ContainerMode).Returns("0");
			myDeclImpTransport.Setup(m => m.NationalityOfTransport).Returns("US");
			myDeclImpTransport.Setup(m => m.CusOffice).Returns("FRB0617A");
			myDeclImpTransport.Setup(m => m.IATAAirportOfLoading).Returns("ORD");
			myDeclImpTransport.Setup(m => m.AirRoadType).Returns("3");
			if (!import)
			{
				myDeclImpTransport.Setup(m => m.TransportID).Returns("ID12345");
				myDeclImpTransport.Setup(m => m.TransportMethodPayment).Returns("B");
			}

			myDeclGen.Setup(m => m.Transport).Returns(myDeclImpTransport.Object);

			#endregion

			#region ElementsValeurGen properties mock
			myDeclGen.Setup(m => m.ThirdCountryTransportCosts).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.EUTransportCostsInInvoice).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.EUTransportCostsNotInInvoice).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.FRTransportCostsInInvoice).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.FRTransportCostsNotInInvoice).Returns<ICostsAndInsurance>(null);

			myDeclGen.Setup(m => m.ThirdCountryAirCosts).Returns(CostsAndInsuranceMock(new ZDecimal("124.00"), "USD", new ZDecimal(null), new ZString(null)));
			myDeclGen.Setup(m => m.FRAirCosts).Returns<ICostsAndInsurance>(null);

			myDeclGen.Setup(m => m.OthAddedCosts).Returns(AmountAndCurrencyMock(new ZDecimal("25.00"), "USD"));

			myDeclGen.Setup(m => m.Interest).Returns<IAmountAndCurrency>(null);
			myDeclGen.Setup(m => m.Commission).Returns<IAmountAndCurrency>(null);
			myDeclGen.Setup(m => m.VATBaseCosts).Returns<IAmountAndCurrency>(null);
			myDeclGen.Setup(m => m.DOMCostsAndInsurance).Returns<IAmountAndCurrency>(null);

			#endregion

			#endregion

			#region Articles properties mock

			#region Article1  properties mock

			myDeclGenArticle1 = new Mock<IArticle> { CallBase = true };
			IdentificationMock(myDeclGenArticle1, "1", "8537109170");
			myDeclGenArticle1.Setup(m => m.AlternateCalcValue).Returns(myImpAlternateCalcValue.Object);
			TariffsAdditionnalMock(myDeclGenArticle1);

			#region Special Tariff Provision properties mock

			var myDispoPart1 = new Mock<ITariffAdditionalCode> { CallBase = true };
			myDispoPart1.Setup(m => m.Code).Returns("Y053");
			myDispoPart1.Setup(m => m.Description).Returns(new ZString(null));

			var myDispoPart2 = new Mock<ITariffAdditionalCode> { CallBase = true };
			myDispoPart2.Setup(m => m.Code).Returns("Y069");
			myDispoPart2.Setup(m => m.Description).Returns(new ZString(null));

			var myDispoPart3 = new Mock<ITariffAdditionalCode> { CallBase = true };
			myDispoPart3.Setup(m => m.Code).Returns("Y949");
			myDispoPart3.Setup(m => m.Description).Returns(new ZString(null));

			myDeclGenArticle1.Setup(m => m.PartDispos).Returns(new List<ITariffAdditionalCode>() { myDispoPart1.Object, myDispoPart2.Object, myDispoPart3.Object });

			#endregion

			myDeclGenArticle1.Setup(m => m.EntryLineDescription).Returns("Tableaux, panneaux, consoles, pupitres, armoires et autres supports comportant plusieurs appareils des n° 8535 ou 8536" + ", pour la commande ou la distribution electrique, y compris ceux incorporant des instruments ou appareils du chapitre 90 ainsi que les apparei");

			GoodsMock(myDeclGenArticle1, "1.172411", "1.035241", "US", "US", "1", new ZString("NAR"), new ZString(null), new ZDecimal(1));

			CusProcedureImportMock(myDeclGenArticle1);

			WarehouseMock(myDeclGenArticle1);
			EconomicRegimeMock(myDeclGenArticle1);

			myDeclGenArticle1.Setup(m => m.EcoRegimeAuthorization).Returns(EcoRegimeAuthorizationMock());
			myDeclGenArticle1.Setup(m => m.EcoRegimeDatas).Returns(EcoRegimeDatasMock());

			PreferenceMock(myDeclGenArticle1);

			ContainerAndPackingMock(myDeclGenArticle1, "1");

			var myPreviousDoc = SupportingDocumentMock("Z", "05781504706", "", "741");
			myDeclGenArticle1.Setup(m => m.PreviousDocument).Returns(myPreviousDoc.Object);

			myDeclGenArticle1.Setup(m => m.SpecMens).Returns<IEnumerable<ITariffAdditionalCode>>(null);

			myDeclGenArticle1.Setup(m => m.DangerousGoodsDeltas).Returns(new ZString[] { "3", "4" });

			#region Supporting Documents properties mock

			var mySupportingDoc1Art1 = SupportingDocumentOnlyMock("", "N° GROS 61AF137", "06/03/2019", "N787", false, "", "", "");
			var mySupportingDoc2Art1 = SupportingDocumentOnlyMock("", "05781504706", "05/03/2019", "N741", false, "", "", "");
			var mySupportingDoc3Art1 = SupportingDocumentOnlyMock("", "8461132", "05/03/2019", "N740", false, "", "", "");
			var mySupportingDoc4Art1 = SupportingDocumentOnlyMock("", "293337842", "05/03/2019", "N380", false, "", "", "");

			myDeclGenArticle1.Setup(m => m.SupportingDocuments).Returns(new List<ISupportingDocumentOnly>() { mySupportingDoc1Art1, mySupportingDoc2Art1, mySupportingDoc3Art1, mySupportingDoc4Art1 });
			#endregion

			FinancialDatasMock(myDeclGenArticle1, 1101.01, "EUR");
			ArticleAddedAndDeductedCostsMock(myDeclGenArticle1);

			#region PreCalculated taxes

			var myPreCalcEntryLine1 = PreCalcEntryLineMock("A445", "0", 20.0002m, 400.999m, 80.999m, "", "", 0m, true);
			var myPreCalcEntryLine2 = PreCalcEntryLineMock(Business.UniversalReferenceConstants.RefCusRateCodes.U165, "0", 12m, 400m, 48m, "DTN", "1", 50m, false);
			var myPreCalcEntryLine3 = PreCalcEntryLineMock("C435", "0", 0m, 400m, 0m, "", "", 0m, false);
			myDeclGenArticle1.Setup(m => m.PreCalcEntryLines).Returns(new List<IPreCalcEntryLine>() { myPreCalcEntryLine1, myPreCalcEntryLine2, myPreCalcEntryLine3 });

			#endregion

			#region Entry header charges

			var myEntryHeaderCharge = EntryHeaderChargeMock("V905", 2800m, "810");
			myDeclGenArticle1.Setup(m => m.EntryHeaderCharges).Returns(new List<ITax>() { myEntryHeaderCharge });

			#endregion

			SpecificsMock(myDeclGenArticle1, "HLT", "1", 150m);

			SealMock(myDeclGenArticle1, import);

			#endregion

			#region Article 2 properties mock

			var myDeclGenArticle2 = new Mock<IArticle> { CallBase = true };

			IdentificationMock(myDeclGenArticle2, "2", "8544200090");
			myDeclGenArticle2.Setup(m => m.AlternateCalcValue).Returns<IAlternateCalcValue>(null);
			TariffsAdditionnalMock(myDeclGenArticle2);

			#region Special Tariff Provision properties mock

			myDeclGenArticle2.Setup(m => m.PartDispos).Returns(new List<ITariffAdditionalCode>() { });
			#endregion

			myDeclGenArticle2.Setup(m => m.EntryLineDescription).Returns("Fils, cables (y compris les cables coaxiaux) et autres conducteurs isoles pour l electricite (meme laques ou oxydes anodiquement)" + ", munis ou non de pieces de connexion; cables de fibres optiques, constitues de fibres gainees individuellement, meme comportant de");

			GoodsMock(myDeclGenArticle2, "3.357589", "2.964759", "US", "US", "1", new ZString(null), new ZString(null), new ZDecimal(null));

			CusProcedureImportMock(myDeclGenArticle2);

			WarehouseMock(myDeclGenArticle2);

			myDeclGenArticle2.Setup(m => m.EcoRegimeAuthorization).Returns(EcoRegimeAuthorizationMock());
			myDeclGenArticle2.Setup(m => m.EcoRegimeDatas).Returns(EcoRegimeDatasMock());

			PreferenceMock(myDeclGenArticle2);

			ContainerAndPackingMock(myDeclGenArticle2, "0");

			#region Previous document properties mock

			var myPreviousDocArt2 = SupportingDocumentMock("Z", "05781504706", "", "741");

			myDeclGenArticle2.Setup(m => m.PreviousDocument).Returns(myPreviousDocArt2.Object);
			#endregion

			myDeclGenArticle2.Setup(m => m.SpecMens).Returns<IEnumerable<ITariffAdditionalCode>>(null);
			myDeclGenArticle2.Setup(m => m.DangerousGoodsDeltas).Returns(Array.Empty<ZString>());
			#region Supporting Documents properties mock

			var mySupportingDoc1Art2 = SupportingDocumentOnlyMock("", "N° GROS 61AF137", "06/03/2019", "N787", false, "", "", "");
			var mySupportingDoc2Art2 = SupportingDocumentOnlyMock("", "05781504706", "05/03/2019", "N741", false, "", "", "");
			var mySupportingDoc3Art2 = SupportingDocumentOnlyMock("", "8461132", "05/03/2019", "N740", false, "", "", "");
			var mySupportingDoc4Art2 = SupportingDocumentOnlyMock("", "293337842", "05/03/2019", "N380", false, "", "", "");

			myDeclGenArticle2.Setup(m => m.SupportingDocuments).Returns(new List<ISupportingDocumentOnly>() { mySupportingDoc1Art2, mySupportingDoc2Art2, mySupportingDoc3Art2, mySupportingDoc4Art2 });
			#endregion

			FinancialDatasMock(myDeclGenArticle2, 3153.11, "EUR");
			ArticleAddedAndDeductedCostsMock(myDeclGenArticle2);

			#region PreCalculated taxes

			var myPreCalcEntryLine4 = PreCalcEntryLineMock("A455", "0", 20m, 12m, 0m, "", "", 0m, true);
			myDeclGenArticle2.Setup(m => m.PreCalcEntryLines).Returns(new List<IPreCalcEntryLine>() { myPreCalcEntryLine4 });

			#endregion
			SpecificsMock(myDeclGenArticle2, "HLT", "1", 275m);

			SealMock(myDeclGenArticle2, import);

			#endregion

			#endregion

			#region Liquidation

			var myLiquidationItem1 = LiquidationItemMock("1", Business.UniversalReferenceConstants.RefCusRateCodes.U165, FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy, 3.1m, 200m, 6m, "1", "", "", "", "");
			var myLiquidationItem2 = LiquidationItemMock("2", "G065", FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy, 20m, 50m, 10m, "2", "810", "50", "1", "DTN");

			#endregion

			#region Core Message

			declarationMock = new Mock<IDeclarationImportExport> { CallBase = true };
			declarationMock.Setup(m => m.MetaData).Returns(myMetaDatas.Object);
			declarationMock.Setup(m => m.MessageEnvelope).Returns(myMessageEnvelop.Object);
			declarationMock.Setup(m => m.Header).Returns(myDeclHeader.Object);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1.Object, myDeclGenArticle2.Object });
			declarationMock.Setup(m => m.HasIntoWarehouseProcedure).Returns(true);
			declarationMock.Setup(m => m.LiquidationHeader).Returns(myDeclHeader.Object);
			declarationMock.Setup(m => m.Liquidation).Returns(new List<ILiquidationItem>() { myLiquidationItem1, myLiquidationItem2 });

			#endregion
		}

		protected void CreateDeclarationMockDNM(string actionCode, bool import = true)
		{
			#region Meta Datas mock

			var myMetaDatas = new Mock<IMetaData> { CallBase = true };
			myMetaDatas.Setup(m => m.Application).Returns("DELTAD");
			myMetaDatas.Setup(m => m.DeclarationReference).Returns("859623");
			myMetaDatas.Setup(m => m.EntryNumberType).Returns(import ? "IMP" : "EXP");

			#endregion

			#region MessageEnvelop properties mock

			var myMessageEnvelop = new Mock<IMessageEnvelope> { CallBase = true };
			myMessageEnvelop.Setup(m => m.SchemaID).Returns(import ? "MessageDDecImp" : "MessageDDecExp");
			myMessageEnvelop.Setup(m => m.SchemaVersion).Returns("1032013");
			myMessageEnvelop.Setup(m => m.PartnerId).Returns("33159700500064");
			myMessageEnvelop.Setup(m => m.TransactionId).Returns("94321-B00175768");
			myMessageEnvelop.Setup(m => m.NumSeq).Returns(new ZShort("0"));

			#endregion

			#region Entete properties mock

			var myDeclHeader = new Mock<IHeader> { CallBase = true };

			if (actionCode == EntryActionCodeList.Codes.REC)
			{
				var myMessageRectificationMotiv = new Mock<IMotivation> { CallBase = true };
				myMessageRectificationMotiv.Setup(m => m.NewDestination).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.RegularJustification).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.Comment).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.RegularJustification).Returns(new ZString(""));
				myMessageRectificationMotiv.Setup(m => m.Motivation).Returns("Rectification explanation");

				myDeclHeader.Setup(m => m.Motivation).Returns(myMessageRectificationMotiv.Object);
			}

			myDeclHeader.Setup(m => m.ActionCode).Returns(EntryActionCodeList.GetMessageCodeNumber(actionCode).ToString());

			#region Reference properties mock

			var myDeclReferences = new Mock<IReferences> { CallBase = true };
			myDeclReferences.Setup(m => m.CusDeclarationNumber).Returns(new ZString(null));
			myDeclReferences.Setup(m => m.OwnerDeclarationIdentification).Returns("8461132");

			myDeclHeader.Setup(m => m.References).Returns(myDeclReferences.Object);

			#endregion

			#endregion

			#region Gen properties mock

			myDeclGen = new Mock<ICusProcedure> { CallBase = true };

			var myImpAlternateCalcValue = new Mock<IAlternateCalcValue> { CallBase = true };
			myImpAlternateCalcValue.Setup(m => m.CalcValue).Returns(new ZString(null));
			myImpAlternateCalcValue.Setup(m => m.Motivation).Returns(new ZString(null));

			myDeclGen.Setup(m => m.AlternateCalcValue).Returns(myImpAlternateCalcValue.Object);

			myDeclGen.Setup(m => m.ProcedureType).Returns("D");
			myDeclGen.Setup(m => m.EntryStyle).Returns("IM");
			myDeclGen.Setup(m => m.EntryStyleCode).Returns("A");
			myDeclGen.Setup(m => m.ArticleCount).Returns(new ZShort("2"));
			myDeclGen.Setup(m => m.Itinerary).Returns(Array.Empty<ZString>());
			myDeclGen.Setup(m => m.ValuationBypassCode).Returns(ZString.Empty);

			#region Prevalidation

			myDeclGen.Setup(m => m.EstimatedAssessmentDate).Returns(new ZString("02/01/2020"));
			myDeclGen.Setup(m => m.EstimatedAssessmentHour).Returns(new ZString("01"));
			myDeclGen.Setup(m => m.DeclEmergencyProcDate).Returns(new ZString(null));

			myDeclGen.Setup(m => m.PackageCount).Returns(new ZInt("1"));
			myDeclGen.Setup(m => m.AgreedGoodsLocation).Returns("34796082500052/1");
			myDeclGen.Setup(m => m.ClearanceLocation).Returns("CDGSO1");
			myDeclGen.Setup(m => m.TransactionNature).Returns("99");
			myDeclGen.Setup(m => m.OrigineState).Returns("FR");
			myDeclGen.Setup(m => m.SpecificCircumstanceIndicator).Returns("Circum");
			myDeclGen.Setup(m => m.DepartureState).Returns("FR");

			#region Office properties mock

			var myDeclGenOffice = new Mock<ICusOffice> { CallBase = true };
			myDeclGenOffice.Setup(m => m.OfficeOfDeclaration).Returns("FRB0617A");
			myDeclGenOffice.Setup(m => m.OfficeOfLodgement).Returns("FRB0617A");
			myDeclGenOffice.Setup(m => m.VisitingOffice).Returns(new ZString(null));
			if (!import)
			{
				myDeclGenOffice.Setup(m => m.ECSExitType).Returns("STC");
				myDeclGenOffice.Setup(m => m.ECSMotivation).Returns("BLABLA");
				myDeclGenOffice.Setup(m => m.ExitOffice).Returns("AT330100");
			}
			myDeclGen.Setup(m => m.Office).Returns(myDeclGenOffice.Object);

			#endregion

			#endregion

			#region Operator

			#region Supplier properties mock

			var myDeclImportSupplier = new Mock<IOrganisation> { CallBase = true };

			myDeclImportSupplier.Setup(m => m.OrganisationNumber).Returns("ETRANGER");
			myDeclImportSupplier.Setup(m => m.FullName).Returns("GE HEALTHCARE");
			myDeclImportSupplier.Setup(m => m.Address).Returns("775 EAST DRIVE DOCK DOORS");
			myDeclImportSupplier.Setup(m => m.CountryCode).Returns("US");
			myDeclImportSupplier.Setup(m => m.PostCode).Returns("60188");
			myDeclImportSupplier.Setup(m => m.City).Returns("STERLING");

			myDeclGen.Setup(m => m.Suppliers).Returns(new List<IOrganisation> { myDeclImportSupplier.Object });

			#endregion

			#region Importer properties mock

			var myDeclImportImporter = new Mock<IOrganisation> { CallBase = true };

			myDeclImportImporter.Setup(m => m.OrganisationNumber).Returns("FR31501335900155");
			myDeclImportImporter.Setup(m => m.OrganisationNumberEoriOnly).Returns("FR3150EORIONLY");
			myDeclImportImporter.Setup(m => m.FullName).Returns("GE MEDICAL SYSTEMMS SCS");
			myDeclImportImporter.Setup(m => m.Address).Returns("RUE DE LA MINIERE");
			myDeclImportImporter.Setup(m => m.CountryCode).Returns("FR");
			myDeclImportImporter.Setup(m => m.PostCode).Returns("78533");
			myDeclImportImporter.Setup(m => m.City).Returns("BUC CEDEX");
			myDeclImportImporter.Setup(m => m.PartnerDestIDInfo).Returns("");

			myDeclGen.Setup(m => m.Importers).Returns(new List<IOrganisation> { myDeclImportImporter.Object });

			#endregion

			#region Representative tax

			var myDeclRepresentative = new Mock<IOrganisation> { CallBase = true };
			myDeclRepresentative.Setup(m => m.OrganisationNumber).Returns("FR32582075100080");
			myDeclRepresentative.Setup(m => m.FullName).Returns("Cosfibel HK Limited");
			myDeclRepresentative.Setup(m => m.Address).Returns("FLAT CTOD 3FL SUMWAY MANSION");
			myDeclRepresentative.Setup(m => m.CountryCode).Returns("HK");
			myDeclRepresentative.Setup(m => m.PostCode).Returns("55102");
			myDeclRepresentative.Setup(m => m.City).Returns("KENNEDY TOWN");
			myDeclGen.Setup(m => m.RepTaxOrganisation).Returns(myDeclRepresentative.Object);

			myDeclGen.Setup(m => m.AgreementOwnerEORI).Returns("FR34430738400570");
			if (import)
			{
				myDeclGen.Setup(m => m.ImporterEORINumber).Returns("FR34430738400571");
			}

			myDeclGen.Setup(m => m.DeltaGAuthorisationNumber).Returns("00000169");
			myDeclGen.Setup(m => m.BranchCusBrokerageCode).Returns("FR34430738400570");
			myDeclGen.Setup(m => m.RepresentationModeCode).Returns("2");
			myDeclGen.Setup(m => m.DeferalApprovalCreditNumber).Returns("ALLI");
			myDeclGen.Setup(m => m.VariousOperationCreditNumber).Returns("ALLI");

			#endregion

			#endregion

			myDeclGen.Setup(m => m.EntryGoodsPriceSum).Returns(4254.12);
			myDeclGen.Setup(m => m.EntryGoodsPriceCurrency).Returns("EUR");
			myDeclGen.Setup(m => m.EntryGoodsPriceCurrencyRate).Returns(new ZDecimal(null));
			myDeclGen.Setup(m => m.PaymentMode).Returns("R");
			myDeclGen.Setup(m => m.GuaranteeMode).Returns("C");
			if (!import)
			{
				myDeclGen.Setup(m => m.OrigineState).Returns("FR");
			}
			myDeclGen.Setup(m => m.DestinationState).Returns("GB");

			#region Delivery Terms properties mock

			var myDeclImpDeliveryTerms = new Mock<IDeliveryTerms> { CallBase = true };
			myDeclImpDeliveryTerms.Setup(m => m.IncotermCode).Returns("FCA");
			myDeclImpDeliveryTerms.Setup(m => m.DeliveryPlace).Returns("STERLING");
			myDeclImpDeliveryTerms.Setup(m => m.IncotermPlace).Returns("3");

			myDeclGen.Setup(m => m.DeliveryTerms).Returns(myDeclImpDeliveryTerms.Object);

			#endregion

			#region Transport properties mock

			myDeclImpTransport = new Mock<ITransport> { CallBase = true };
			myDeclImpTransport.Setup(m => m.ModeOfTRansport).Returns("4");
			myDeclImpTransport.Setup(m => m.ModeOfTRansportInland).Returns("3");
			myDeclImpTransport.Setup(m => m.ContainerMode).Returns("0");
			myDeclImpTransport.Setup(m => m.NationalityOfTransport).Returns("US");
			myDeclImpTransport.Setup(m => m.CusOffice).Returns("FRB0617A");
			myDeclImpTransport.Setup(m => m.IATAAirportOfLoading).Returns("ORD");
			myDeclImpTransport.Setup(m => m.AirRoadType).Returns("3");
			if (!import)
			{
				myDeclImpTransport.Setup(m => m.TransportID).Returns("ID12345");
				myDeclImpTransport.Setup(m => m.TransportMethodPayment).Returns("B");
			}

			myDeclGen.Setup(m => m.Transport).Returns(myDeclImpTransport.Object);

			#endregion

			#region ElementsValeurGen properties mock
			myDeclGen.Setup(m => m.ThirdCountryTransportCosts).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.EUTransportCostsInInvoice).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.EUTransportCostsNotInInvoice).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.FRTransportCostsInInvoice).Returns<ICostsAndInsurance>(null);
			myDeclGen.Setup(m => m.FRTransportCostsNotInInvoice).Returns<ICostsAndInsurance>(null);

			myDeclGen.Setup(m => m.ThirdCountryAirCosts).Returns(CostsAndInsuranceMock(new ZDecimal("124.00"), "USD", new ZDecimal(null), new ZString(null)));
			myDeclGen.Setup(m => m.FRAirCosts).Returns<ICostsAndInsurance>(null);

			myDeclGen.Setup(m => m.OthAddedCosts).Returns(AmountAndCurrencyMock(new ZDecimal("25.00"), "USD"));

			myDeclGen.Setup(m => m.Interest).Returns<IAmountAndCurrency>(null);
			myDeclGen.Setup(m => m.Commission).Returns<IAmountAndCurrency>(null);
			myDeclGen.Setup(m => m.VATBaseCosts).Returns<IAmountAndCurrency>(null);
			myDeclGen.Setup(m => m.DOMCostsAndInsurance).Returns<IAmountAndCurrency>(null);

			#endregion

			#endregion

			#region Articles properties mock

			#region Article1  properties mock

			myDeclGenArticle1DNM = new Mock<IArticle> { CallBase = true };
			IdentificationMockDNM(myDeclGenArticle1DNM, "1", "8537109170");
			myDeclGenArticle1DNM.Setup(m => m.AlternateCalcValue).Returns(myImpAlternateCalcValue.Object);
			TariffsAdditionnalMockDNM(myDeclGenArticle1DNM);

			#region Special Tariff Provision properties mock

			var myDispoPart1 = new Mock<ITariffAdditionalCode> { CallBase = true };
			myDispoPart1.Setup(m => m.Code).Returns("Y053");
			myDispoPart1.Setup(m => m.Description).Returns(new ZString(null));

			var myDispoPart2 = new Mock<ITariffAdditionalCode> { CallBase = true };
			myDispoPart2.Setup(m => m.Code).Returns("Y069");
			myDispoPart2.Setup(m => m.Description).Returns(new ZString(null));

			var myDispoPart3 = new Mock<ITariffAdditionalCode> { CallBase = true };
			myDispoPart3.Setup(m => m.Code).Returns("Y949");
			myDispoPart3.Setup(m => m.Description).Returns(new ZString(null));

			myDeclGenArticle1DNM.Setup(m => m.PartDispos).Returns(new List<ITariffAdditionalCode> { myDispoPart1.Object, myDispoPart2.Object, myDispoPart3.Object });

			#endregion

			myDeclGenArticle1DNM.Setup(m => m.EntryLineDescription).Returns("Tableaux, panneaux, consoles, pupitres, armoires et autres supports comportant plusieurs appareils des n° 8535 ou 8536"
				  + ", pour la commande ou la distribution electrique, y compris ceux incorporant des instruments ou appareils du chapitre 90 ainsi que les apparei");

			GoodsMockDNM(myDeclGenArticle1DNM, "1.172411", "1.035241", "US", "US", "1", new ZString("NAR"), new ZString(null), new ZDecimal(1));

			CusProcedureImportMockDNM(myDeclGenArticle1DNM);

			WarehouseMockDNM(myDeclGenArticle1DNM);
			EconomicRegimeMockDNM(myDeclGenArticle1DNM);

			myDeclGenArticle1DNM.Setup(m => m.EcoRegimeAuthorization).Returns(EcoRegimeAuthorizationMock());

			PreferenceMockDNM(myDeclGenArticle1DNM);

			ContainerAndPackingMockDNM(myDeclGenArticle1DNM, "1");

			var myPreviousDoc = SupportingDocumentMock("Z", "05781504706", "", "741");

			myDeclGenArticle1DNM.Setup(m => m.PreviousDocument).Returns(myPreviousDoc.Object);
			myDeclGenArticle1DNM.Setup(m => m.SpecMens).Returns((IEnumerable<ITariffAdditionalCode>)null);
			myDeclGenArticle1DNM.Setup(m => m.DangerousGoodsDeltas).Returns(new ZString[] { "3", "4" });

			#region Supporting Documents properties mock

			var mySupportingDoc1Art1 = SupportingDocumentOnlyMock("", new string('A', 40), "06/03/2019", "N787", false, "1", new string('B', 40), new string('C', 270));
			var mySupportingDoc2Art1 = SupportingDocumentOnlyMock("", "05781504706", "05/03/2019", "N741", false, "", "", "");
			var mySupportingDoc3Art1 = SupportingDocumentOnlyMock("", "8461132", "05/03/2019", "N740", false, "", "", "");
			var mySupportingDoc4Art1 = SupportingDocumentOnlyMock("", "293337842", "05/03/2019", "N380", false, "", "", "");

			myDeclGenArticle1DNM.Setup(m => m.SupportingDocuments).Returns(new List<ISupportingDocumentOnly>
			{
				mySupportingDoc1Art1,
				mySupportingDoc2Art1,
				mySupportingDoc3Art1,
				mySupportingDoc4Art1
			});

			#endregion

			FinancialDatasMockDNM(myDeclGenArticle1DNM, 1101.01, "EUR");
			ArticleAddedAndDeductedCostsMockDNM(myDeclGenArticle1DNM);

			#region PreCalculated taxes

			var myPreCalcEntryLine1 = PreCalcEntryLineMock("A445", "0", 20.0002m, 400.999m, 80.999m, "", "", 0m, true);
			var myPreCalcEntryLine2 = PreCalcEntryLineMock(Business.UniversalReferenceConstants.RefCusRateCodes.U165, "0", 12m, 400m, 48m, "DTN", "1", 50m, false);
			var myPreCalcEntryLine3 = PreCalcEntryLineMock("C435", "0", 0m, 400m, 0m, "", "", 0m, false);
			myDeclGenArticle1DNM.Setup(m => m.PreCalcEntryLines).Returns(new List<IPreCalcEntryLine>() { myPreCalcEntryLine1, myPreCalcEntryLine2, myPreCalcEntryLine3 });

			#endregion

			#region Entry header charges

			var myEntryHeaderCharge = EntryHeaderChargeMock("V905", 2800m, "810");
			myDeclGenArticle1DNM.Setup(m => m.EntryHeaderCharges).Returns(new List<ITax>() { myEntryHeaderCharge });

			#endregion

			SpecificsMockDNM(myDeclGenArticle1DNM, "HLT", "1", 150m);

			SealMockDNM(myDeclGenArticle1DNM, import);

			#endregion

			#region Article 2 properties mock

			var myDeclGenArticle2 = new Mock<IArticle> { CallBase = true };

			IdentificationMockDNM(myDeclGenArticle2, "2", "8544200090");
			myDeclGenArticle2.Setup(m => m.AlternateCalcValue).Returns((IAlternateCalcValue)null);
			TariffsAdditionnalMockDNM(myDeclGenArticle2);

			#region Special Tariff Provision properties mock

			myDeclGenArticle2.Setup(m => m.PartDispos).Returns(new List<ITariffAdditionalCode>() { });
			#endregion

			myDeclGenArticle2.Setup(m => m.EntryLineDescription).Returns(
				"Fils, cables (y compris les cables coaxiaux) et autres conducteurs isoles pour l electricite (meme laques ou oxydes anodiquement)"
				+ ", munis ou non de pieces de connexion; cables de fibres optiques, constitues de fibres gainees individuellement, meme comportant de");

			GoodsMockDNM(myDeclGenArticle2, "3.357589", "2.964759", "US", "US", "1", new ZString(null), new ZString(null), new ZDecimal(null));

			CusProcedureImportMockDNM(myDeclGenArticle2);

			WarehouseMockDNM(myDeclGenArticle2);

			myDeclGenArticle2.Setup(m => m.EcoRegimeAuthorization).Returns(EcoRegimeAuthorizationMock());
			myDeclGenArticle2.Setup(m => m.EcoRegimeDatas).Returns(EcoRegimeDatasMock());

			PreferenceMockDNM(myDeclGenArticle2);

			ContainerAndPackingMockDNM(myDeclGenArticle2, "0");

			#region Previous document properties mock

			var myPreviousDocArt2 = SupportingDocumentMock("Z", "05781504706", "", "741");
			myDeclGenArticle2.Setup(m => m.PreviousDocument).Returns(myPreviousDocArt2.Object);

			#endregion

			myDeclGenArticle2.Setup(m => m.SpecMens).Returns((IEnumerable<ITariffAdditionalCode>)null);
			myDeclGenArticle2.Setup(m => m.DangerousGoodsDeltas).Returns(Array.Empty<ZString>());

			#region Supporting Documents properties mock

			var mySupportingDoc1Art2 = SupportingDocumentOnlyMock("", "N° GROS 61AF137", "06/03/2019", "N787", false, "", "", "");
			var mySupportingDoc2Art2 = SupportingDocumentOnlyMock("", "05781504706", "05/03/2019", "N741", false, "", "", "");
			var mySupportingDoc3Art2 = SupportingDocumentOnlyMock("", "8461132", "05/03/2019", "N740", false, "", "", "");
			var mySupportingDoc4Art2 = SupportingDocumentOnlyMock("", "293337842", "05/03/2019", "N380", false, "", "", "");

			myDeclGenArticle2.Setup(m => m.SupportingDocuments).Returns(new List<ISupportingDocumentOnly>
			{
				mySupportingDoc1Art2, mySupportingDoc2Art2, mySupportingDoc3Art2, mySupportingDoc4Art2
			});
			#endregion

			FinancialDatasMockDNM(myDeclGenArticle2, 3153.11, "EUR");
			ArticleAddedAndDeductedCostsMockDNM(myDeclGenArticle2);

			#region PreCalculated taxes

			var myPreCalcEntryLine4 = PreCalcEntryLineMock("A455", "0", 20m, 12m, 0m, "", "", 0m, true);
			myDeclGenArticle2.Setup(m => m.PreCalcEntryLines).Returns(new List<IPreCalcEntryLine>() { myPreCalcEntryLine4 });

			#endregion
			SpecificsMockDNM(myDeclGenArticle2, "HLT", "1", 275m);

			SealMockDNM(myDeclGenArticle2, import);

			#endregion

			#endregion

			#region Liquidation

			var myLiquidationItem1 = LiquidationItemMock("1", Business.UniversalReferenceConstants.RefCusRateCodes.U165, FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy, 3.1m, 200m, 6m, "1", "", "", "", "");
			var myLiquidationItem2 = LiquidationItemMock("2", "G065", FeeTypeCodeConverter.StandardAdvalorumOrSecondQuantiy, 20m, 50m, 10m, "2", "810", "50", "1", "DTN");

			#endregion

			#region Core Message

			declarationMock = new Mock<IDeclarationImportExport> { CallBase = true };
			declarationMock.Setup(m => m.MetaData).Returns(myMetaDatas.Object);
			declarationMock.Setup(m => m.MessageEnvelope).Returns(myMessageEnvelop.Object);
			declarationMock.Setup(m => m.Header).Returns(myDeclHeader.Object);
			declarationMock.Setup(m => m.CusProcedure).Returns(myDeclGen.Object);
			declarationMock.Setup(m => m.Articles).Returns(new List<IArticle>() { myDeclGenArticle1DNM.Object, myDeclGenArticle2.Object });
			declarationMock.Setup(m => m.HasIntoWarehouseProcedure).Returns(true);
			declarationMock.Setup(m => m.LiquidationHeader).Returns(myDeclHeader.Object);
			declarationMock.Setup(m => m.Liquidation).Returns(new List<ILiquidationItem>() { myLiquidationItem1, myLiquidationItem2 });

			#endregion
		}

		void SpecificsMock(Mock<IArticle> articleMock, ZString code, ZString qualif, ZDecimal qty)
		{
			var mySupUnit = SupplementaryUnitMock(code, qualif, qty);
			articleMock.Setup(m => m.ThirdUnit).Returns(mySupUnit);

			#region PAC

			articleMock.Setup(m => m.CusImportCertification).Returns("CusImportCertification");
			articleMock.Setup(m => m.CEQuotaCertification).Returns(new ZString(null));
			articleMock.Setup(m => m.SugarRate1).Returns(new ZDecimal(ZString.Empty));
			articleMock.Setup(m => m.SugarRate2).Returns(new ZDecimal(ZString.Empty));
			articleMock.Setup(m => m.SugarRate3).Returns(new ZDecimal(ZString.Empty));
			articleMock.Setup(m => m.SugarPolarisation).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.PACInformations).Returns(new ZString(null));

			#endregion

			#region Eco spec organisation

			var myEcoSpecOrg = new Mock<IOrganisation> { CallBase = true };
			myEcoSpecOrg.Setup(m => m.OrganisationNumber).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.FullName).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.Address).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.CountryCode).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.PostCode).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.City).Returns(new ZString(null));

			articleMock.Setup(m => m.Applicant).Returns(myEcoSpecOrg.Object);
			articleMock.Setup(m => m.ApplicantInwardNature).Returns("inwardTest");
			articleMock.Setup(m => m.ApplicantDescription).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantConditions).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantPurOffice).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantInwardLocation).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantTransFormality).Returns(new ZString(null));
			#endregion

			articleMock.Setup(m => m.SpecificInfos).Returns(new ZString(null));
		}

		void SpecificsMockDNM(Mock<IArticle> articleMock, ZString code, ZString qualif, ZDecimal qty)
		{
			var mySupUnit = SupplementaryUnitMock(code, qualif, qty);
			articleMock.Setup(m => m.ThirdUnit).Returns(mySupUnit);

			#region PAC

			articleMock.Setup(m => m.CusImportCertification).Returns("CusImportCertification");
			articleMock.Setup(m => m.CEQuotaCertification).Returns(new ZString(null));
			articleMock.Setup(m => m.SugarRate1).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.SugarRate2).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.SugarRate3).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.SugarPolarisation).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.PACInformations).Returns(new ZString(null));

			#endregion

			#region Eco spec organisation

			var myEcoSpecOrg = new Mock<IOrganisation> { CallBase = true };
			myEcoSpecOrg.Setup(m => m.OrganisationNumber).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.FullName).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.Address).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.CountryCode).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.PostCode).Returns(new ZString(null));
			myEcoSpecOrg.Setup(m => m.City).Returns(new ZString(null));

			articleMock.Setup(m => m.Applicant).Returns(myEcoSpecOrg.Object);
			articleMock.Setup(m => m.ApplicantInwardNature).Returns("inwardTest");
			articleMock.Setup(m => m.ApplicantDescription).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantConditions).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantPurOffice).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantInwardLocation).Returns(new ZString(null));
			articleMock.Setup(m => m.ApplicantTransFormality).Returns(new ZString(null));
			#endregion

			articleMock.Setup(m => m.SpecificInfos).Returns(new ZString(null));
		}

		void SealMock(Mock<IArticle> myDeclGenArticle, bool import)
		{
			if (!import)
			{
				myDeclGenArticle.Setup(m => m.Seals).Returns(1);
				myDeclGenArticle.Setup(m => m.SealIds).Returns(new List<ZString>() { "123" });
			}
		}

		void SealMockDNM(Mock<IArticle> myDeclGenArticle, bool import)
		{
			if (!import)
			{
				myDeclGenArticle.Setup(m => m.Seals).Returns(1);
				myDeclGenArticle.Setup(m => m.SealIds).Returns(new List<ZString>() { "123" });
			}
		}

		void ContainerAndPackingMock(Mock<IArticle> articleMock, ZString count)
		{
			articleMock.Setup(m => m.Containers).Returns(new List<ZString>() { new ZString(null) });

			var myPacking = new Mock<IPacking> { CallBase = true };

			myPacking.Setup(m => m.Count).Returns(new ZInt(count));
			myPacking.Setup(m => m.ItemsCount).Returns(new ZInt(ZString.Empty));
			myPacking.Setup(m => m.Type).Returns("CT");
			myPacking.Setup(m => m.MarksAndNos).Returns("ADR");
			articleMock.Setup(m => m.Packing).Returns(myPacking.Object);
		}

		void ContainerAndPackingMockDNM(Mock<IArticle> articleMock, ZString count)
		{
			articleMock.Setup(m => m.Containers).Returns(new List<ZString>() { new ZString(null) });

			var myPacking = new Mock<IPacking> { CallBase = true };

			myPacking.Setup(m => m.Count).Returns(new ZInt(count));
			myPacking.Setup(m => m.ItemsCount).Returns(new ZInt(ZString.Empty));
			myPacking.Setup(m => m.Type).Returns("CT");
			myPacking.Setup(m => m.MarksAndNos).Returns("Test202301040130123456789112233445566778899abc");
			articleMock.Setup(m => m.Packing).Returns(myPacking.Object);
		}

		void PreferenceMock(Mock<IArticle> articleMock)
		{
			var myPreference = new Mock<IPreference> { CallBase = true };

			myPreference.Setup(m => m.CodePart1).Returns("1");
			myPreference.Setup(m => m.CodePart2).Returns("00");

			articleMock.Setup(m => m.Preference).Returns(myPreference.Object);
		}

		void PreferenceMockDNM(Mock<IArticle> articleMock)
		{
			var myPreference = new Mock<IPreference> { CallBase = true };

			myPreference.Setup(m => m.CodePart1).Returns("1");
			myPreference.Setup(m => m.CodePart2).Returns("00");

			articleMock.Setup(m => m.Preference).Returns(myPreference.Object);
		}

		void CusProcedureImportMock(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.ProcedureCode).Returns("40");
			articleMock.Setup(m => m.PreviousCode).Returns("00");
			articleMock.Setup(m => m.Concession).Returns("000");
		}

		void CusProcedureImportMockDNM(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.ProcedureCode).Returns("40");
			articleMock.Setup(m => m.PreviousCode).Returns("00");
			articleMock.Setup(m => m.Concession).Returns("000");
		}

		void TariffsAdditionnalMock(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.CETariffAdditionalCodes).Returns<IEnumerable<ITariffAdditionalCode>>(null);
			articleMock.Setup(m => m.FRTariffAdditionalCodes).Returns<IEnumerable<ITariffAdditionalCode>>(null);
		}

		void TariffsAdditionnalMockDNM(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.CETariffAdditionalCodes).Returns((IEnumerable<ITariffAdditionalCode>)null);
			articleMock.Setup(m => m.FRTariffAdditionalCodes).Returns((IEnumerable<ITariffAdditionalCode>)null);
		}

		void GoodsMock(Mock<IArticle> articleMock
			, ZString grossWeight, ZString customsQuantity, ZString countryGoodsOrigineCode
			, ZString countryGoodsSupplyCode, ZString valuationMethod
			, ZString code, ZString qualif, ZDecimal qty)
		{
			var mySupUnit = SupplementaryUnitMock(code, qualif, qty);

			articleMock.Setup(m => m.GrossWeight).Returns(new ZDecimal(grossWeight));
			articleMock.Setup(m => m.CustomsQuantity).Returns(new ZDecimal(customsQuantity));
			articleMock.Setup(m => m.CountryGoodsOrigineCode).Returns(countryGoodsOrigineCode);
			articleMock.Setup(m => m.CountryGoodsSupplyCode).Returns(countryGoodsSupplyCode);
			articleMock.Setup(m => m.ValuationMethod).Returns(valuationMethod);
			articleMock.Setup(m => m.SuppUnit).Returns(mySupUnit);

			articleMock.Setup(m => m.QuotaRefNumber).Returns<IEnumerable<ZString>>(null);
		}

		void GoodsMockDNM(Mock<IArticle> articleMock
			, ZString grossWeight, ZString customsQuantity, ZString countryGoodsOrigineCode
			, ZString countryGoodsSupplyCode, ZString valuationMethod
			, ZString code, ZString qualif, ZDecimal qty)
		{
			var mySupUnit = SupplementaryUnitMock(code, qualif, qty);

			articleMock.Setup(m => m.GrossWeight).Returns(new ZDecimal(grossWeight));
			articleMock.Setup(m => m.CustomsQuantity).Returns(new ZDecimal(customsQuantity));
			articleMock.Setup(m => m.CountryGoodsOrigineCode).Returns(countryGoodsOrigineCode);
			articleMock.Setup(m => m.CountryGoodsSupplyCode).Returns(countryGoodsSupplyCode);
			articleMock.Setup(m => m.ValuationMethod).Returns(valuationMethod);
			articleMock.Setup(m => m.SuppUnit).Returns(mySupUnit);

			articleMock.Setup(m => m.QuotaRefNumber).Returns((IEnumerable<ZString>)null);
		}

		ISupplementaryUnit SupplementaryUnitMock(ZString code, ZString qualif, ZDecimal qty)
		{
			var mySupUnit = new Mock<ISupplementaryUnit> { CallBase = true };
			mySupUnit.Setup(m => m.Code).Returns(code);
			mySupUnit.Setup(m => m.Qualif).Returns(qualif);
			mySupUnit.Setup(m => m.Qty).Returns(qty);

			return mySupUnit.Object;
		}

		void IdentificationMock(Mock<IArticle> articleMock, ZString entryNumber, ZString tariffCode)
		{
			articleMock.Setup(m => m.EntryNumber).Returns(new ZShort(entryNumber));
			articleMock.Setup(m => m.ShippingIdReference).Returns(new ZString(null));
			articleMock.Setup(m => m.Observation).Returns(new ZString(null));
			articleMock.Setup(m => m.TariffCode).Returns(tariffCode);
		}

		void IdentificationMockDNM(Mock<IArticle> articleMock, ZString entryNumber, ZString tariffCode)
		{
			articleMock.Setup(m => m.EntryNumber).Returns(new ZShort(entryNumber));
			articleMock.Setup(m => m.ShippingIdReference).Returns(new ZString(null));
			articleMock.Setup(m => m.Observation).Returns(new ZString(null));
			articleMock.Setup(m => m.TariffCode).Returns(tariffCode);
		}

		void WarehouseMock(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.WarehouseType).Returns(new ZString(null));
			articleMock.Setup(m => m.WarehouseReference).Returns(new ZString(null));
			articleMock.Setup(m => m.WarehouseCountryCode).Returns(new ZString(null));
		}

		void WarehouseMockDNM(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.WarehouseType).Returns(new ZString(null));
			articleMock.Setup(m => m.WarehouseReference).Returns(new ZString(null));
			articleMock.Setup(m => m.WarehouseCountryCode).Returns(new ZString(null));
		}

		protected void EconomicRegimeMock(Mock<IArticle> articleMock)
		{
			var ecoRegimeDeclDatas1 = new Mock<IEcoRegimeDeclDatas> { CallBase = true };
			ecoRegimeDeclDatas1.Setup(m => m.CusDeclarationID).Returns("CLE_0002");
			ecoRegimeDeclDatas1.Setup(m => m.EcoRegimeDeclTypeCode).Returns("3");

			var ecoRegimeDeclDatas2 = new Mock<IEcoRegimeDeclDatas> { CallBase = true };
			ecoRegimeDeclDatas2.Setup(m => m.CusDeclarationID).Returns("PRE_004");
			ecoRegimeDeclDatas2.Setup(m => m.EcoRegimeDeclTypeCode).Returns("5");

			var ecoRegimeDatas = new Mock<IEcoRegimeDatas> { CallBase = true };
			ecoRegimeDatas.Setup(m => m.GuaranteeAmount).Returns(23.65m);
			ecoRegimeDatas.Setup(m => m.NumberDaysOfDischarge).Returns(12);
			ecoRegimeDatas.Setup(m => m.DecEcos).Returns(new List<IEcoRegimeDeclDatas> { ecoRegimeDeclDatas1.Object, ecoRegimeDeclDatas2.Object });
			articleMock.Setup(m => m.EcoRegimeDatas).Returns(ecoRegimeDatas.Object);
		}

		void EconomicRegimeMockDNM(Mock<IArticle> articleMock)
		{
			var ecoRegimeDeclDatas1 = new Mock<IEcoRegimeDeclDatas> { CallBase = true };
			ecoRegimeDeclDatas1.Setup(m => m.CusDeclarationID).Returns("CLE_0002");
			ecoRegimeDeclDatas1.Setup(m => m.EcoRegimeDeclTypeCode).Returns("3");

			var ecoRegimeDeclDatas2 = new Mock<IEcoRegimeDeclDatas> { CallBase = true };
			ecoRegimeDeclDatas2.Setup(m => m.CusDeclarationID).Returns("PRE_004");
			ecoRegimeDeclDatas2.Setup(m => m.EcoRegimeDeclTypeCode).Returns("5");

			var ecoRegimeDatas = new Mock<IEcoRegimeDatas> { CallBase = true };
			ecoRegimeDatas.Setup(m => m.GuaranteeAmount).Returns(23.65m);
			ecoRegimeDatas.Setup(m => m.NumberDaysOfDischarge).Returns(12);
			ecoRegimeDatas.Setup(m => m.DecEcos).Returns(new List<IEcoRegimeDeclDatas> { ecoRegimeDeclDatas1.Object, ecoRegimeDeclDatas2.Object });
			articleMock.Setup(m => m.EcoRegimeDatas).Returns(ecoRegimeDatas.Object);
		}

		void ArticleAddedAndDeductedCostsMock(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.PackingCosts).Returns<IAmountAndCurrency>(null);
			articleMock.Setup(m => m.Commission).Returns<IAmountAndCurrency>(null);
			articleMock.Setup(m => m.Fee).Returns<IAmountAndCurrency>(null);
			articleMock.Setup(m => m.Resale).Returns<IAmountAndCurrency>(null);
			articleMock.Setup(m => m.OthCosts).Returns<IAmountAndCurrency>(null);
			articleMock.Setup(m => m.AssemblyCosts).Returns<IAmountAndCurrency>(null);
			articleMock.Setup(m => m.CustomsCosts).Returns<IAmountAndCurrency>(null);
		}

		void ArticleAddedAndDeductedCostsMockDNM(Mock<IArticle> articleMock)
		{
			articleMock.Setup(m => m.PackingCosts).Returns((IAmountAndCurrency)null);
			articleMock.Setup(m => m.Commission).Returns((IAmountAndCurrency)null);
			articleMock.Setup(m => m.Fee).Returns((IAmountAndCurrency)null);
			articleMock.Setup(m => m.Resale).Returns((IAmountAndCurrency)null);
			articleMock.Setup(m => m.OthCosts).Returns((IAmountAndCurrency)null);
			articleMock.Setup(m => m.AssemblyCosts).Returns((IAmountAndCurrency)null);
			articleMock.Setup(m => m.CustomsCosts).Returns((IAmountAndCurrency)null);
		}

		void FinancialDatasMock(Mock<IArticle> articleMock, ZDecimal invoiceLinePrice, ZString currencyCode)
		{
			articleMock.Setup(m => m.InvoiceLinePrice).Returns(invoiceLinePrice);
			articleMock.Setup(m => m.CurrencyCode).Returns(currencyCode);
			articleMock.Setup(m => m.TransportMethodPayment).Returns("B");
			articleMock.Setup(m => m.ExchangeRate).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.CIFPrice).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.CIFApplicationFlag).Returns(new ZBool(null));
			articleMock.Setup(m => m.StatisticalAmount).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.CustomsValue).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.TVAAssessedAmount).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.DeliveryDepartment).Returns("95");
			articleMock.Setup(m => m.ExpeditionDepartment).Returns("12");
			articleMock.Setup(m => m.ValuationAdjustPercent).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.ShouldSendCustomsStatisticAndVatValues).Returns(true);
		}

		void FinancialDatasMockDNM(Mock<IArticle> articleMock, ZDecimal invoiceLinePrice, ZString currencyCode)
		{
			articleMock.Setup(m => m.InvoiceLinePrice).Returns(invoiceLinePrice);
			articleMock.Setup(m => m.CurrencyCode).Returns(currencyCode);
			articleMock.Setup(m => m.TransportMethodPayment).Returns("B");
			articleMock.Setup(m => m.ExchangeRate).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.CIFPrice).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.CIFApplicationFlag).Returns(new ZBool(null));
			articleMock.Setup(m => m.StatisticalAmount).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.CustomsValue).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.TVAAssessedAmount).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.DeliveryDepartment).Returns("95");
			articleMock.Setup(m => m.ExpeditionDepartment).Returns("12");
			articleMock.Setup(m => m.ValuationAdjustPercent).Returns(new ZDecimal(null));
			articleMock.Setup(m => m.ShouldSendCustomsStatisticAndVatValues).Returns(true);
		}

		ICostsAndInsurance CostsAndInsuranceMock(ZDecimal costAmount, ZString costCurrency, ZDecimal insuranceAmount, ZString insuranceCurrency)
		{
			var myCostsAndInsurance = new Mock<ICostsAndInsurance> { CallBase = true };

			myCostsAndInsurance.Setup(m => m.Costs).Returns(AmountAndCurrencyMock(costAmount, costCurrency));
			myCostsAndInsurance.Setup(m => m.Insurance).Returns(AmountAndCurrencyMock(insuranceAmount, insuranceCurrency));

			return myCostsAndInsurance.Object;
		}

		protected IAmountAndCurrency AmountAndCurrencyMock(ZDecimal amount, ZString currency)
		{
			var myIAmountAndCurrencyMock = new Mock<IAmountAndCurrency> { CallBase = true };

			if (amount.IsEmpty)
			{
				myIAmountAndCurrencyMock.Setup(m => m.Amount).Returns(new ZDecimal(null));
			}
			else
			{
				myIAmountAndCurrencyMock.Setup(m => m.Amount).Returns(amount);
			}

			if (currency.IsEmpty)
			{
				myIAmountAndCurrencyMock.Setup(m => m.Currency).Returns(new ZString(null));
			}
			else
			{
				myIAmountAndCurrencyMock.Setup(m => m.Currency).Returns(currency);
			}

			return myIAmountAndCurrencyMock.Object;
		}

		Mock<ISupportingDocument> SupportingDocumentMock(ZString type, ZString refNumber, ZString dateIssue, ZString code)
		{
			var mySupportingDocumentMock = new Mock<ISupportingDocument> { CallBase = true };
			if (type.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.Type).Returns(new ZString(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.Type).Returns(type);
			}

			if (refNumber.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.RefNumber).Returns(new ZString(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.RefNumber).Returns(refNumber);
			}

			if (dateIssue.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.DateIssue).Returns(new ZDateTime(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.DateIssue).Returns(new ZDateTime(dateIssue));
			}

			if (code.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.Code).Returns(new ZString(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.Code).Returns(code);
			}

			mySupportingDocumentMock.Setup(m => m.PFADocument).Returns(new ZString(null));
			mySupportingDocumentMock.Setup(m => m.PFAIdentification).Returns(new ZString(null));
			return mySupportingDocumentMock;
		}

		ISupportingDocumentOnly SupportingDocumentOnlyMock(ZString type, ZString refNumber, ZString dateIssue, ZString code, ZBool isD48, ZString lineNumber, ZString productReference, ZString productName)
		{
			var mySupportingDocumentMock = new Mock<ISupportingDocumentOnly> { CallBase = true };
			if (type.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.Type).Returns(new ZString(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.Type).Returns(type);
			}

			if (refNumber.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.RefNumber).Returns(new ZString(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.RefNumber).Returns(refNumber);
			}

			if (dateIssue.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.DateIssue).Returns(new ZDateTime(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.DateIssue).Returns(new ZDateTime(dateIssue));
			}

			if (code.IsEmpty)
			{
				mySupportingDocumentMock.Setup(m => m.Code).Returns(new ZString(null));
			}
			else
			{
				mySupportingDocumentMock.Setup(m => m.Code).Returns(code);
			}

			mySupportingDocumentMock.Setup(m => m.PFADocument).Returns(new ZString(null));
			mySupportingDocumentMock.Setup(m => m.PFAIdentification).Returns(new ZString(null));

			var myImputationsSheetMock = new Mock<IImputationSheet> { CallBase = true };
			myImputationsSheetMock.Setup(m => m.ImputationAmount).Returns(new ZDecimal(null));
			myImputationsSheetMock.Setup(m => m.ImputationQuantity).Returns(new ZDecimal(null));
			myImputationsSheetMock.Setup(m => m.ImputationCurrency).Returns(new ZString(null));
			myImputationsSheetMock.Setup(m => m.ImputationUnit).Returns(new ZString(null));
			myImputationsSheetMock.Setup(m => m.LineNumber).Returns(lineNumber);
			myImputationsSheetMock.Setup(m => m.NetWeight).Returns(new ZDecimal(null));
			myImputationsSheetMock.Setup(m => m.ProductReference).Returns(productReference);
			myImputationsSheetMock.Setup(m => m.ProductName).Returns(productName);

			var myWeightMock = new Mock<IWeight> { CallBase = true };
			myWeightMock.Setup(m => m.Unit).Returns(new ZString(null));
			myWeightMock.Setup(m => m.Weight).Returns(new ZDecimal(null));

			myImputationsSheetMock.Setup(m => m.ProvisionalWeight).Returns(myWeightMock.Object);

			mySupportingDocumentMock.Setup(m => m.ImputationsSheets).Returns(new List<IImputationSheet>() { myImputationsSheetMock.Object });
			mySupportingDocumentMock.Setup(m => m.IsD48AndNotClosed).Returns(isD48);
			mySupportingDocumentMock.Setup(m => m.D48Amount).Returns(new ZDecimal(null));
			mySupportingDocumentMock.Setup(m => m.D48Deadline).Returns(0);

			return mySupportingDocumentMock.Object;
		}

		IPreCalcEntryLine PreCalcEntryLineMock(ZString taxCode, ZString taxType, ZDecimal taxRate, ZDecimal taxAssessed, ZDecimal taxAmount, ZString suppUnitCode, ZString suppUnitQualif, ZDecimal suppUnitQty, ZBool isVAT)
		{
			var myPreCalcEntryLine = new Mock<IPreCalcEntryLine> { CallBase = true };
			myPreCalcEntryLine.Setup(m => m.IsVAT).Returns(isVAT);

			var myPreCalcEntryLineSuppUnit = new Mock<ISupplementaryUnit> { CallBase = true };
			myPreCalcEntryLineSuppUnit.Setup(m => m.Qty).Returns(suppUnitQty);
			myPreCalcEntryLineSuppUnit.Setup(m => m.Qualif).Returns(suppUnitQualif);
			myPreCalcEntryLineSuppUnit.Setup(m => m.Code).Returns(suppUnitCode);
			myPreCalcEntryLine.Setup(m => m.SuppUnit).Returns(myPreCalcEntryLineSuppUnit.Object);

			var myPreCalcEntryLineTax = new Mock<ITax> { CallBase = true };
			myPreCalcEntryLineTax.Setup(m => m.TaxRate).Returns(taxRate);
			myPreCalcEntryLineTax.Setup(m => m.TaxAmount).Returns(taxAmount);
			myPreCalcEntryLineTax.Setup(m => m.TaxAssessed).Returns(taxAssessed);
			myPreCalcEntryLineTax.Setup(m => m.TaxCode).Returns(taxCode);
			myPreCalcEntryLineTax.Setup(m => m.TaxType).Returns(taxType);
			myPreCalcEntryLineTax.Setup(m => m.ChargePaymentOrDestinationID).Returns(ZString.Empty);
			myPreCalcEntryLineTax.Setup(m => m.LiquidationStatus).Returns(ZString.Empty);
			myPreCalcEntryLineTax.Setup(m => m.EUTaxCode).Returns(ZString.Empty);
			myPreCalcEntryLine.Setup(m => m.Tax).Returns(myPreCalcEntryLineTax.Object);

			return myPreCalcEntryLine.Object;
		}

		ITax EntryHeaderChargeMock(ZString taxCode, ZDecimal taxAmount, ZString portCode)
		{
			var myEntryHeaderCharge = new Mock<ITax> { CallBase = true };
			myEntryHeaderCharge.Setup(m => m.TaxRate).Returns(1m);
			myEntryHeaderCharge.Setup(m => m.TaxAmount).Returns(taxAmount);
			myEntryHeaderCharge.Setup(m => m.TaxAssessed).Returns(taxAmount);
			myEntryHeaderCharge.Setup(m => m.TaxCode).Returns(taxCode);
			myEntryHeaderCharge.Setup(m => m.TaxType).Returns(FeeTypeCodeConverter.FlatRate);
			myEntryHeaderCharge.Setup(m => m.TaxMethodOfPayment).Returns(new ZString(null));
			myEntryHeaderCharge.Setup(m => m.ChargePaymentOrDestinationID).Returns(portCode);
			return myEntryHeaderCharge.Object;
		}

		IEcoRegimeDatas EcoRegimeDatasMock()
		{
			var myEcoRegimeDatas = new Mock<IEcoRegimeDatas> { CallBase = true };
			var myEcoRegimeDeclDatas = new Mock<IEcoRegimeDeclDatas> { CallBase = true };
			myEcoRegimeDeclDatas.Setup(m => m.CusDeclarationID).Returns(new ZString(null));
			myEcoRegimeDeclDatas.Setup(m => m.EcoRegimeDeclTypeCode).Returns(new ZString(null));
			myEcoRegimeDatas.Setup(m => m.DecEcos).Returns(new List<IEcoRegimeDeclDatas>() { myEcoRegimeDeclDatas.Object });
			myEcoRegimeDatas.Setup(m => m.GuaranteeAmount).Returns(new ZDecimal(ZString.Empty));
			myEcoRegimeDatas.Setup(m => m.NumberDaysOfDischarge).Returns(0);

			return myEcoRegimeDatas.Object;
		}

		IEcoRegimeAuthorization EcoRegimeAuthorizationMock()
		{
			var myEcoRegimeAuthorization = new Mock<IEcoRegimeAuthorization> { CallBase = true };
			myEcoRegimeAuthorization.Setup(m => m.EcoRegimeAuthorizationNumber).Returns("3100");
			myEcoRegimeAuthorization.Setup(m => m.EcoRegimeCountryCode).Returns("FR");

			return myEcoRegimeAuthorization.Object;
		}

		ILiquidationItem LiquidationItemMock(ZString articleNumber, ZString taxCode, ZString taxType, ZDecimal taxRate, ZDecimal taxAssessed, ZDecimal taxAmount, ZString mop, ZString portCode, ZString qty, ZString qualif, ZString code)
		{
			var myLiquidationItem = new Mock<ILiquidationItem> { CallBase = true };
			myLiquidationItem.Setup(m => m.ArticleNumber).Returns(new ZShort(articleNumber));

			var myTaxationDetail = new Mock<ITaxationDetail> { CallBase = true };

			var myTaxationDetailLineSuppUnit = new Mock<ISupplementaryUnit>();
			myTaxationDetailLineSuppUnit.Setup(m => m.Qty).Returns(new ZDecimal(qty));
			myTaxationDetailLineSuppUnit.Setup(m => m.Qualif).Returns(qualif);
			myTaxationDetailLineSuppUnit.Setup(m => m.Code).Returns(code);

			var myTaxationDetailTax = new Mock<ITax> { CallBase = true };
			myTaxationDetailTax.Setup(m => m.TaxRate).Returns(taxRate);
			myTaxationDetailTax.Setup(m => m.TaxMethodOfPayment).Returns(mop);
			myTaxationDetailTax.Setup(m => m.TaxAmount).Returns(taxAmount);
			myTaxationDetailTax.Setup(m => m.TaxAssessed).Returns(taxAssessed);
			myTaxationDetailTax.Setup(m => m.TaxCode).Returns(taxCode);
			myTaxationDetailTax.Setup(m => m.TaxType).Returns(taxType);
			myTaxationDetailTax.Setup(m => m.ChargePaymentOrDestinationID).Returns(portCode);

			myTaxationDetail.Setup(m => m.SuppUnit).Returns(myTaxationDetailLineSuppUnit.Object);
			myTaxationDetail.Setup(m => m.Tax).Returns(myTaxationDetailTax.Object);

			myLiquidationItem.Setup(m => m.TaxDetail).Returns(myTaxationDetail.Object);

			return myLiquidationItem.Object;
		}

		#endregion

		protected Mock<IDeclarationImportExport> declarationMock;
		protected Mock<ITransport> myDeclImpTransport;
		protected Mock<ICusProcedure> myDeclGen;
		protected Mock<IArticle> myDeclGenArticle1;
		protected Mock<IArticle> myDeclGenArticle1DNM;
	}
}
