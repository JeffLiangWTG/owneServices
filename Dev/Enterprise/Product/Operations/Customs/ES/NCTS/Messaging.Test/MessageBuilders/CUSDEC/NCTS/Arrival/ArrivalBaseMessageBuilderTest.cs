using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	public abstract class ArrivalBaseMessageBuilderTest : NCTSEDIFACTMessageBuilderTest<ArrivalMessageBuilder, IArrivalMessageDataProvider, CUSDECMessage>
	{
		public void TestAddNewGISSegment_NoIndicatorCode()
		{
			mockProvider.Setup(m => m.GoodsInContainerIndicator).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("GIS+0:109:141'", messageText);
		}

		public void TestAddNewGISSegment_FalseIndicatorCode()
		{
			mockProvider.Setup(m => m.ReceiverComplianceForAutoDischarge).Returns(false);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("GIS+U:63:148'", messageText);
		}

		public void TestAddNewSG1Group_NoPreviousSummaryNumber()
		{
			mockReferenceGroup.Setup(m => m.PreviousSummaryDeclarationNumber).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+ABT:08119505000'", messageText);
		}

		public void TestAddNewSG1Group_FalseIncidentInEvent()
		{
			mockRouteEvents1.Setup(m => m.IncidentInEvent).Returns(false);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+18+20010422:POLICIA LOCAL:ES:ZARAGOZA:ES:ES'FTX+ABM+++VUELCO DEL CAMION QUE TRANPORTABA LA MERCANCIA+ES'", messageText);
		}

		public void TestAddNewSG1Group_NoFormData()
		{
			mockRouteEvents1.Setup(m => m.IncidentFormData).Returns((IArrivalFormData)null);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+18+20010422:POLICIA LOCAL:ES:ZARAGOZA:ES:ES'FTX+ABM+++VUELCO DEL CAMION QUE TRANPORTABA LA MERCANCIA+ES'", messageText);
		}

		public void TestAddNewSG1Group_NoNewSealsInEventNum()
		{
			mockRouteEvents1.Setup(m => m.NewSealsInEventNum).Returns(ZString.Empty);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+21+NOMBREPRECIN1111:ES'PCI+21+NOMBREPRECIN2222:ES'", messageText);
		}

		public void TestAddNewSG1Group_EmptyNewSealsInformation()
		{
			mockRouteEvents1.Setup(m => m.NewSealsInformation).Returns(Array.Empty<IArrivalNewSealsInformation>());
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+21", messageText);
		}

		public void TestAddNewSG1Group_NoNewTransportNationality()
		{
			mockRouteEvents1.Setup(m => m.NewTransportNationality).Returns(ZString.Empty);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+23+20010422:POLICIA LOCAL:ES:ZARAGOZA:ES:ES'FTX+TRA+++M5044SV+ES'PCI+30+MARAT34556677'PCI+30+MARAT34556899'", messageText);
		}

		public void TestAddNewPACSegment_NoValue()
		{
			mockRouteEvents1.Setup(m => m.EventCountry).Returns(ZString.Empty);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC+2+++ES'", messageText);
		}

		public void TestAddNewPACSegment_FalseIncidentInEvent()
		{
			mockRouteEvents1.Setup(m => m.IncidentInEvent).Returns(false);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC+3+1'", messageText);
		}

		public void TestAddNewPCISegment_NoSealData()
		{
			mockRouteEvents1.Setup(m => m.NewSealsInformation).Returns(Array.Empty<IArrivalNewSealsInformation>());
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+21", messageText);
		}

		public void TestAddNewPCISegment_NoNewContainerId()
		{
			mockRouteEvents1.Setup(m => m.NewContainerIDs).Returns(Array.Empty<ZString>());
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PCI+30", messageText);
		}

		public void TestAddNewFTXSegment_NoFormText()
		{
			var mockIncidentFormData = SetUpFormData(ZString.Empty);
			mockRouteEvents1.Setup(m => m.IncidentFormData).Returns(mockIncidentFormData.Object);
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("FTX+ABM", messageText);
		}

		public void TestPopulateSG4Groups_NoTransitTransportMedium()
		{
			mockProvider.Setup(m => m.TransitTransportMedium).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+30++:CAMION'TPL+:::M-1234-GD:ES'", messageText);
		}

		public void TestAddNewNADWithoutAddressInSG6Group_NoAddressDetails()
		{
			mockProvider.Setup(m => m.Declarant).Returns((IPartyNameProvider)null);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("NAD+DT", messageText);
		}

		public void TestAddNewSG7Group_NoUnloadingObservations()
		{
			mockProvider.Setup(m => m.UnloadingObservations).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TOD+5'FTX+CLR+++MERCANCÍA RECIBIDA CON LAS MARCAS BORROSAS Y NO LEGIBLESAAAAAAAAAAAAAA:AAAAAAA'", messageText);
		}

		public abstract void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory1();

		public abstract void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory2();

		public abstract void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory3();

		public abstract void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory4();

		public abstract void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory5();

		public void TestAddNewSG31Group_NoExternalPackages()
		{
			mockLine1.Setup(m => m.ExternalPackages).Returns((IExternalPackagesInfoCommon)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC+2+3+CT'", messageText);
		}

		public void TestAddNewSG31Group_NoNumberOfPackages()
		{
			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(ZLong.Zero, new ZString[] { "CONTEN1111", "CONTEN2222", "CONTEN3333" });
			mockLine1.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC+2+3+CT'", messageText);
		}

		public virtual void TestAddNewSG31Group_NoTags()
		{
			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(2, Array.Empty<ZString>());
			mockLine1.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC+2+3+CT'PAC+50+1+BX'", messageText);
		}

		protected override ZString DeclarantIdForUNBSegment => "76688664B";
		protected abstract ZBool IsOnlyArrivalNotification { get; }
		protected abstract ZBool IsOnlyUnloadingRemarks { get; }
		protected abstract ZBool IsArrivalWithAVI { get; }
		protected abstract ZBool IsArrivalWithOBS { get; }
		protected abstract ZBool IsArrivalWithTNN { get; }

		protected override ArrivalMessageBuilder CreateMessageBuilder() => new ArrivalMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override ArrivalMessageBuilder CreateMessageBuilderWithNullProvider() => new ArrivalMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IArrivalMessageDataProvider> mockProvider;

		protected void SetUpMockProvider()
		{
			mockProvider = new Mock<IArrivalMessageDataProvider>();

			mockProvider.Setup(m => m.Factory).Returns(Factory);
			mockProvider.Setup(m => m.IsTest).Returns(ZBool.True);
			mockProvider.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory));
			mockProvider.Setup(m => m.BrokerCode).Returns("AZ");
			mockProvider.Setup(m => m.CertificateName).Returns("CertName");
			mockProvider.Setup(m => m.CertificateThumbPrint).Returns("CertThumbPrint");
			mockProvider.Setup(m => m.CertificateBytes).Returns(BuilderHelperTest.GetCertificateBytes());
			mockProvider.Setup(m => m.DecryptedCertificatePassphrase).Returns(BuilderHelperTest.CertificatePassword);
			mockProvider.Setup(m => m.BusinessObjectReference).Returns("Reference");

			var certificate = Factory.New<MasterFiles.Business.GlbExternalPassword>();
			mockProvider.Setup(m => m.CertificatePK).Returns(certificate.PK);
		}

		protected override void SetUp()
		{
			SetUpMockProvider();
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns(DeclarantIdForUNBSegment);
			mockProvider.Setup(m => m.IsOnlyArrivalNotification).Returns(IsOnlyArrivalNotification);
			mockProvider.Setup(m => m.IsOnlyUnloadingRemarks).Returns(IsOnlyUnloadingRemarks);
			mockProvider.Setup(m => m.IsArrivalWithAVI).Returns(IsArrivalWithAVI);
			mockProvider.Setup(m => m.IsArrivalWithOBS).Returns(IsArrivalWithOBS);
			mockProvider.Setup(m => m.IsArrivalWithTNN).Returns(IsArrivalWithTNN);
			mockProvider.Setup(m => m.DocumentMessageName).Returns(ExpectedMessageType);
			mockProvider.Setup(m => m.LocalReferenceNumber).Returns("123456789");
			mockProvider.Setup(m => m.CustomsProcedureCategory1).Returns("T1");
			mockProvider.Setup(m => m.CustomsProcedureCategory2).Returns("FR000061");
			mockProvider.Setup(m => m.CustomsTransitDestinationOffice).Returns("002801");

			var mockCustomsOfficesOfDestination = SetUpCustomsOfficesOfDestination();
			mockProvider.Setup(m => m.CustomsOfficesOfDestination).Returns(mockCustomsOfficesOfDestination.Object);
			mockProvider.Setup(m => m.CountryOfDestination).Returns("ES");
			mockProvider.Setup(m => m.DateOfArrival).Returns(new ZDateTime(1999, 01, 01));
			mockProvider.Setup(m => m.DateOfUnloading).Returns(new ZDateTime(1999, 01, 31));
			mockProvider.Setup(m => m.GoodsInContainerIndicator).Returns("0");
			mockProvider.Setup(m => m.UnloadingComplianceIndicator).Returns("1");
			mockProvider.Setup(m => m.SealsInGoodState).Returns("M");
			mockProvider.Setup(m => m.AttachedDocumentTypeA).Returns("A");
			mockProvider.Setup(m => m.ReceiverComplianceForAutoDischarge).Returns(true);
			mockProvider.Setup(m => m.DirectlyLoadedOnCompletion).Returns("1");
			mockProvider.Setup(m => m.SealsStateDiscrepancies).Returns(new string[] { "2", "1" });
			mockProvider.Setup(m => m.TIRCompletionNumber).Returns("8");
			mockProvider.Setup(m => m.TIRIsCompleteUnloading).Returns("T");
			mockProvider.Setup(m => m.SealCodes).Returns(new ZString[] { "123456789G0000017", "123456789XXXXX", "123456789XXXZZ" });
			mockProvider.Setup(m => m.SealCodesWithDiscrepancies).Returns(new ZString[] { "M123456789AA", "M123456789ZZ" });

			mockReferenceGroup = SetUpReferenceGroup();
			mockProvider.Setup(m => m.ReferenceGroup).Returns(mockReferenceGroup.Object);
			mockProvider.Setup(m => m.TransitTransportMedium).Returns("CAMION");
			mockProvider.Setup(m => m.TransportId).Returns("M-1234-GD");
			mockProvider.Setup(m => m.TransportNationality).Returns("ES");

			var mockConsignor = BuilderHelperTest.SetUpParty("12123123F", "SMITH LTD.", "OVERHAUSEN 1123", "HAMBURG", "H1234", "004");
			mockProvider.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = BuilderHelperTest.SetUpParty("43921123", "BUTLER S.A.", "SANTA ANA 15", "MADRID", "28009", "021");
			mockProvider.Setup(m => m.Consignee).Returns(mockConsignee);

			var mockDeclarant = BuilderHelperTest.SetUpPartyName("76688664B", "JUAN MARTINEZ");
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

			mockProvider.Setup(m => m.UnloadingObservations).Returns("MERCANCÍA RECIBIDA CON LAS MARCAS BORROSAS Y NO LEGIBLESAAAAAAAAAAAAAAAAAAAAA");
			mockProvider.Setup(m => m.IsDeclarationInEuros).Returns(true);
			mockProvider.Setup(m => m.TotalNumberOfGoods).Returns(4);
			mockProvider.Setup(m => m.TotalNumberOfPackageElements).Returns(2L);

			var mockPackages = BuilderHelperTest.SetUpInternalPackages();

			mockLine1 = SetUpLine(1, "T1", "M3", "HB12345678901", "12345678", new ZString[] { "3", "6" }, new[] { mockPackages });
			var mockDocument1 = BuilderHelperTest.SetUpDocument("380", "187");
			var mockDocument2 = BuilderHelperTest.SetUpDocument("X001", "ES3600000002");
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument1, mockDocument2 });

			var mockLine2 = SetUpLine(2, "T1", "M3", "HB12345678901", "12345678", new ZString[] { "3", "6" }, new[] { mockPackages, mockPackages });
			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(2, new ZString[] { "CONTEN1111", "CONTEN2222", "CONTEN3333" });
			mockLine2.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			mockLine2.Setup(m => m.Documents).Returns(Array.Empty<IDocumentsCommon>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}
		Mock<IArrivalReferencesGroup> mockReferenceGroup;
		Mock<IArrivalRouteEvent> mockRouteEvents1;
		protected Mock<IArrivalLine> mockLine1;

		Mock<IArrivalCustomsEffectiveDestinationOffice> SetUpCustomsOfficesOfDestination()
		{
			var mockCustomsOfficesOfDestination = new Mock<IArrivalCustomsEffectiveDestinationOffice>();
			mockCustomsOfficesOfDestination.Setup(m => m.CustomsDestinationOfficeCode).Returns("0811");
			mockCustomsOfficesOfDestination.Setup(m => m.CustomsDestinationLocationCode).Returns("XY1234");
			return mockCustomsOfficesOfDestination;
		}

		Mock<IArrivalReferencesGroup> SetUpReferenceGroup()
		{
			var mockReferenceGroup = new Mock<IArrivalReferencesGroup>();
			mockReferenceGroup.Setup(m => m.TransitNumber).Returns("99BE00012312345670");
			mockReferenceGroup.Setup(m => m.PreviousSummaryDeclarationNumber).Returns("08119505000");

			mockRouteEvents1 = SetUpRouteEvents();
			var mockRouteEvents2 = SetUpRouteEvents();
			mockReferenceGroup.Setup(m => m.RouteEvents).Returns(new[] { mockRouteEvents1.Object, mockRouteEvents2.Object });
			return mockReferenceGroup;
		}

		Mock<IArrivalRouteEvent> SetUpRouteEvents()
		{
			var mockRouteEvents = new Mock<IArrivalRouteEvent>();
			mockRouteEvents.Setup(m => m.EventPlace).Returns("ZARAGOZA");
			mockRouteEvents.Setup(m => m.EventPlaceLanguage).Returns("ES");
			mockRouteEvents.Setup(m => m.EventCountry).Returns("ES");
			mockRouteEvents.Setup(m => m.IncidentInEvent).Returns(true);

			var mockIncidentFormData = SetUpFormData("VUELCO DEL CAMION QUE TRANPORTABA LA MERCANCIA");
			mockRouteEvents.Setup(m => m.IncidentFormData).Returns(mockIncidentFormData.Object);

			mockRouteEvents.Setup(m => m.NewSealsInEventNum).Returns("2");

			var mockNewSealsInformation1 = SetUpNewSealsInformation("NOMBREPRECIN1111", "ES");
			var mockNewSealsInformation2 = SetUpNewSealsInformation("NOMBREPRECIN2222", "ES");
			mockRouteEvents.Setup(m => m.NewSealsInformation).Returns(new[] { mockNewSealsInformation1.Object, mockNewSealsInformation2.Object });
			mockRouteEvents.Setup(m => m.NewTransportNationality).Returns("ES");

			var mockTransferFormData = SetUpFormData("M5044SV");
			mockRouteEvents.Setup(m => m.TransferFormData).Returns(mockTransferFormData.Object);
			mockRouteEvents.Setup(m => m.NewContainerIDs).Returns(new ZString[] { "MARAT34556677", "MARAT34556899" });
			return mockRouteEvents;
		}

		Mock<IArrivalFormData> SetUpFormData(ZString text)
		{
			var mockFormData = new Mock<IArrivalFormData>();
			mockFormData.Setup(m => m.FormDate).Returns(new ZDateTime(2001, 04, 22));
			mockFormData.Setup(m => m.FormAuthority).Returns("POLICIA LOCAL");
			mockFormData.Setup(m => m.FormAuthorityLanguage).Returns("ES");
			mockFormData.Setup(m => m.FormLocation).Returns("ZARAGOZA");
			mockFormData.Setup(m => m.FormLocationLanguage).Returns("ES");
			mockFormData.Setup(m => m.FormCountry).Returns("ES");
			mockFormData.Setup(m => m.FormTextLanguage).Returns("ES");
			mockFormData.Setup(m => m.FormText).Returns(text);
			return mockFormData;
		}

		Mock<IArrivalNewSealsInformation> SetUpNewSealsInformation(ZString id, ZString language)
		{
			var mockNewSealsInformation = new Mock<IArrivalNewSealsInformation>();
			mockNewSealsInformation.Setup(m => m.SealId).Returns(id);
			mockNewSealsInformation.Setup(m => m.SealIdLanguage).Returns(language);
			return mockNewSealsInformation;
		}

		protected Mock<IArrivalLine> SetUpLine(ZInt goodsItemNumber, ZString goodsCustomsProcedureCategory2, ZString goodsCustomsProcedureCategory3, ZString goodsCustomsProcedureCategory4, ZString goodsCustomsProcedureCategory5, IReadOnlyCollection<ZString> c44documents, IReadOnlyCollection<IInternalPackageIdentificationCommon> packages)
		{
			var mockLine = new Mock<IArrivalLine>();
			mockLine.Setup(m => m.GoodsItemNumber).Returns(goodsItemNumber);
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory1).Returns("88033090");
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory2).Returns(goodsCustomsProcedureCategory2);
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory3).Returns(goodsCustomsProcedureCategory3);
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory4).Returns(goodsCustomsProcedureCategory4);
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory5).Returns(goodsCustomsProcedureCategory5);
			mockLine.Setup(m => m.GoodsDescription).Returns("CALZADO DE PIEL DE COCODRILO PARA SEÑORAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
			mockLine.Setup(m => m.NotSubmittedC44Documents).Returns(c44documents);
			mockLine.Setup(m => m.GrossWeightInKG).Returns(134);
			mockLine.Setup(m => m.NetWeightInKG).Returns(100);
			mockLine.Setup(m => m.TotalGoodValueInEuros).Returns(8527.45);

			var mockInternalPackages = new Mock<IInternalPackagesInfoCommon>();
			mockInternalPackages.Setup(m => m.Packages).Returns(packages);
			mockLine.Setup(m => m.InternalPackages).Returns(mockInternalPackages.Object);

			return mockLine;
		}
	}
}
