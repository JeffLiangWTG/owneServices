using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Moq;
using NUnit.Framework;
using ROC = Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class CC044AMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC044AMessageBuilder()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			var xmlFile = MessageBuilderUtilities.GetEmbeddedResourceFile("CC044AMessageXml.xml");
			AssertEquals(xmlFile, xmlMsgWithNoNamespaces);
		}

		public void TestConREM65IsNumeric()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("ConREM65 should be numeric (1 or 0, not Y or N)", "<ConREM65>0</ConREM65>", xmlMsgWithNoNamespaces);

			unloadingRemarkMock.Setup(m => m.Conform).Returns(YesNoList.Codes.Yes);
			xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("ConREM65 should be numeric (1 or 0, not Y or N)", "<ConREM65>1</ConREM65>", xmlMsgWithNoNamespaces);
		}

		public void TestNumberOfPieces()
		{
			packageMock.Setup(m => m.NumberOfPackages).Returns(0L);
			packageMock.Setup(m => m.NumberOfPieces).Returns(1L);
			packageMock.Setup(m => m.IsBulk).Returns(false);
			packageMock.Setup(m => m.IsUnpacked).Returns(false);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertContains("tag is present even with 0 package", "<NumOfPacGS24>0", xmlMsgWithNoNamespaces);
				AssertNotContains("Number of Pieces for PKG", "<NumOfPieGS25>1</NumOfPieGS25>", xmlMsgWithNoNamespaces);
			});

			packageMock.Setup(m => m.KindOfPackages).Returns("NG");
			packageMock.Setup(m => m.NumberOfPackages).Returns(0L);
			packageMock.Setup(m => m.NumberOfPieces).Returns(1L);
			packageMock.Setup(m => m.IsBulk).Returns(true);
			packageMock.Setup(m => m.IsUnpacked).Returns(true);
			xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertNotContains("no tag NumOfPacGS24 for NG type", "<NumOfPacGS24>0", xmlMsgWithNoNamespaces);
				AssertContains("tag NumOfPieGS25 for NG type", "<NumOfPieGS25>1</NumOfPieGS25>", xmlMsgWithNoNamespaces);
			});
		}

		public void TestPopulateTRADESTRD_NameAndAddress()
		{
			destinationTraderMock.Setup(m => m.TIN).Returns(ZString.Empty);
			destinationTraderMock.Setup(m => m.Name).Returns("Oscorp Industries3");
			destinationTraderMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			destinationTraderMock.Setup(m => m.StreetAndNumber).Returns("Street and No3");
			destinationTraderMock.Setup(m => m.PostalCode).Returns("MK16 XX3");
			destinationTraderMock.Setup(m => m.City).Returns("Milton Keynes3");
			destinationTraderMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains(@"<TRADESTRD>
    <NamTRD7>Oscorp Industries3</NamTRD7>
    <StrAndNumTRD22>Street and No3</StrAndNumTRD22>
    <PosCodTRD23>MK16 XX3</PosCodTRD23>
    <CitTRD24>Milton Keynes3</CitTRD24>
    <CouTRD25>GB</CouTRD25>
    <NADLNGRD>EN</NADLNGRD>
  </TRADESTRD>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateTRADESTRD_Null()
		{
			dataProviderMock.Setup(m => m.DestinationTrader).Returns((ITrader)null);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertNotContains("<TRADESTRD>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateStaOfTheSeaOKREM19()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<StaOfTheSeaOKREM19>1", xmlMsgWithNoNamespaces);

			unloadingRemarkMock.Setup(m => m.StateOfSealsOk).Returns(YesNoList.Codes.No);
			xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<StaOfTheSeaOKREM19>0", xmlMsgWithNoNamespaces);

			unloadingRemarkMock.Setup(m => m.StateOfSealsOk).Returns(ZString.Empty);
			xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertNotContains("StaOfTheSeaOKREM19", xmlMsgWithNoNamespaces);
		}

		protected override void SetUp()
		{
			base.SetUp();
			destinationTraderMock = new Mock<ITrader>();
			destinationTraderMock.Setup(m => m.TIN).Returns("GB0123456789003");

			unloadingRemarkMock = new Mock<IUnloadingRemarkInterface>();
			unloadingRemarkMock.Setup(m => m.StateOfSealsOk).Returns(YesNoList.Codes.Yes);
			unloadingRemarkMock.Setup(m => m.Conform).Returns(YesNoList.Codes.No);
			unloadingRemarkMock.Setup(m => m.UnloadingCompletion).Returns(YesNoList.Codes.Yes);
			unloadingRemarkMock.Setup(m => m.UnloadingDate).Returns(ZString.Empty);
			unloadingRemarkMock.Setup(m => m.NoOfSeals).Returns(new ZInt(ZString.Empty));

			var declarationControlResultMock = new Mock<IControlResult>();
			declarationControlResultMock.Setup(m => m.Description).Returns("descriptionTest");
			declarationControlResultMock.Setup(m => m.DescriptionLNG).Returns(ZString.Empty);
			declarationControlResultMock.Setup(m => m.ControlIndicator).Returns("TS");
			declarationControlResultMock.Setup(m => m.PointerToTheAttribute).Returns("pointerTest");
			declarationControlResultMock.Setup(m => m.CorrectedValue).Returns("correctedValueTest");

			var goodsItemControlResult1 = SetupGoodsItemControlResult(ZString.Empty, ZString.Empty, ROC.ResultOfControlCodes.Codes.New, "NE value");
			var goodsItemControlResult2 = SetupGoodsItemControlResult("DescriptionTest", "TS", "pointerTest", "TS value");
			var goodsItemControlResult3 = SetupGoodsItemControlResult("description DI", ZString.Empty, ROC.ResultOfControlCodes.Codes.Different, "DI value");
			var goodsItemControlResult4 = SetupGoodsItemControlResult("description OR", ZString.Empty, ROC.ResultOfControlCodes.Codes.Original, "");
			var goodsItemControlResult5 = SetupGoodsItemControlResult(ZString.Empty, ZString.Empty, ROC.ResultOfControlCodes.Codes.Other, "");
			var goodsItemControlResult6 = SetupGoodsItemControlResult("description OT", ZString.Empty, ROC.ResultOfControlCodes.Codes.Other, "");
			var goodsItemControlResult7 = SetupGoodsItemControlResult(ZString.Empty, ZString.Empty, ROC.ResultOfControlCodes.Codes.Missing, "");
			var declarationControlResultMock2 = new Mock<IControlResult>();
			declarationControlResultMock2.Setup(m => m.Description).Returns("DI test");
			declarationControlResultMock2.Setup(m => m.DescriptionLNG).Returns(ZString.Empty);
			declarationControlResultMock2.Setup(m => m.ControlIndicator).Returns(ROC.ResultOfControlCodes.Codes.Different);
			declarationControlResultMock2.Setup(m => m.PointerToTheAttribute).Returns("DI pointer");
			declarationControlResultMock2.Setup(m => m.CorrectedValue).Returns("DI corrected");

			var declarationControlResultMock3 = new Mock<IControlResult>();
			declarationControlResultMock3.Setup(m => m.Description).Returns("OT test");
			declarationControlResultMock3.Setup(m => m.DescriptionLNG).Returns(ZString.Empty);
			declarationControlResultMock3.Setup(m => m.ControlIndicator).Returns(ROC.ResultOfControlCodes.Codes.Other);
			declarationControlResultMock3.Setup(m => m.PointerToTheAttribute).Returns("OT pointer");
			declarationControlResultMock3.Setup(m => m.CorrectedValue).Returns("OT corrected");

			var declarationControlResultMock4 = new Mock<IControlResult>();
			declarationControlResultMock4.Setup(m => m.Description).Returns(ZString.Empty);
			declarationControlResultMock4.Setup(m => m.DescriptionLNG).Returns(ZString.Empty);
			declarationControlResultMock4.Setup(m => m.ControlIndicator).Returns(ROC.ResultOfControlCodes.Codes.Other);
			declarationControlResultMock4.Setup(m => m.PointerToTheAttribute).Returns("OT pointer 2");
			declarationControlResultMock4.Setup(m => m.CorrectedValue).Returns("OT pointer 2");

			var supportingDocumentMock = new Mock<ISupportingDocument>();
			supportingDocumentMock.Setup(m => m.Code).Returns("Codes");
			supportingDocumentMock.Setup(m => m.RefNumber).Returns("123456");
			supportingDocumentMock.Setup(m => m.Description).Returns("descriptionTEST");

			packageMock = new Mock<IPackage>();
			packageMock.Setup(m => m.MarksAndNumbersOfPackages).Returns("marksTest");
			packageMock.Setup(m => m.MarksAndNumbersOfPackagesLanguage).Returns(ZString.Empty);
			packageMock.Setup(m => m.KindOfPackages).Returns("PKG");
			packageMock.Setup(m => m.NumberOfPackages).Returns(1L);
			packageMock.Setup(m => m.NumberOfPieces).Returns(0L);

			var goodsItemMock = SetupUnloadedgooditem(2, "Books", new IControlResult[] { goodsItemControlResult1, goodsItemControlResult2, goodsItemControlResult3, goodsItemControlResult5, goodsItemControlResult6, goodsItemControlResult7 });
			var goodsItemMock2 = SetupUnloadedgooditem(1, "Books expected 1", new IControlResult[] { goodsItemControlResult4 });
			var goodsItemMock3 = SetupUnloadedgooditem(10, "Books expected 10", new IControlResult[] { goodsItemControlResult4 });
			var expectedgoodsItemMock = SetupGooditem("2", "Books expected");
			var expectedgoodsItemMock2 = SetupGooditem("1", "Books expected 2");

			dataProviderMock = new Mock<ICC044ADeclaration>();
			dataProviderMock.Setup(m => m.IsProduction).Returns(new ZBool("1"));
			List<Tuple<string, string>> listOfDifferenceInHeader = new List<Tuple<string, string>>();
			listOfDifferenceInHeader.Add(new Tuple<string, string>("DI pointer 2", "DI corrected 2"));
			dataProviderMock.Setup(m => m.ListOfDifferenceInHeader).Returns(listOfDifferenceInHeader);
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("MRN123");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("REG DEP1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.NewZealand);
			dataProviderMock.Setup(m => m.TotalNumberOfItems).Returns(new ZInt("2"));
			dataProviderMock.Setup(m => m.TotalNumberOfPackages).Returns(new ZLong("60"));
			dataProviderMock.Setup(m => m.TotalGrossMass).Returns(12.1234);
			dataProviderMock.Setup(m => m.DestinationTrader).Returns(destinationTraderMock.Object);
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("FR0040");
			dataProviderMock.Setup(m => m.UnloadingRemark).Returns(unloadingRemarkMock.Object);
			dataProviderMock.Setup(m => m.HeaderUnloadingNotes).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.HeaderUnloadingNotesLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { goodsItemMock.Object, goodsItemMock3.Object, goodsItemMock2.Object });
			dataProviderMock.Setup(m => m.ControlResultList).Returns(new IControlResult[] { declarationControlResultMock.Object, declarationControlResultMock2.Object, declarationControlResultMock3.Object, declarationControlResultMock4.Object });
			dataProviderMock.Setup(m => m.EnRouteEvents).Returns((IReadOnlyCollection<IEnRouteEvent>)Enumerable.Empty<IEnRouteEvent>());
			dataProviderMock.Setup(m => m.ExpectedGoodsItems).Returns(new ICommonGoodsItem[] { expectedgoodsItemMock.Object, expectedgoodsItemMock2.Object });

			messageBuilder = new CC044AMessageBuilder(dataProviderMock.Object, new NctsMessageFunctionSet.UnloadingRemarksMessage(), new ErrorCollector());
		}

		Mock<IUnloadedGoodsItem> SetupUnloadedgooditem(int intemNumber, ZString description, IControlResult[] controlResult)
		{
			var supportingDocumentMock = new Mock<ISupportingDocument>();
			supportingDocumentMock.Setup(m => m.Code).Returns("Codes");
			supportingDocumentMock.Setup(m => m.RefNumber).Returns("123456");
			supportingDocumentMock.Setup(m => m.Description).Returns("descriptionTEST");

			var goodsItemMock = new Mock<IUnloadedGoodsItem>();
			goodsItemMock.Setup(m => m.ItemNumber).Returns(intemNumber);
			goodsItemMock.Setup(m => m.CommodityCode).Returns("commodityTest");
			goodsItemMock.Setup(m => m.GoodsDescription).Returns(description);
			goodsItemMock.Setup(m => m.GrossWeight).Returns(12.1234m);
			goodsItemMock.Setup(m => m.NetWeight).Returns(10.1234m);
			goodsItemMock.Setup(m => m.SupportingDocuments).Returns(new ISupportingDocument[] { supportingDocumentMock.Object });
			goodsItemMock.Setup(m => m.ControlResults).Returns(controlResult);
			goodsItemMock.Setup(m => m.Containers).Returns(new ZString[] { "123456" });
			goodsItemMock.Setup(m => m.Packages).Returns(new IPackage[] { packageMock.Object });
			return goodsItemMock;
		}

		Mock<ICommonGoodsItem> SetupGooditem(ZString intemNumber, ZString description)
		{
			var goodsItemMock = new Mock<ICommonGoodsItem>();
			goodsItemMock.Setup(m => m.ItemNumber).Returns(new ZInt(intemNumber));
			goodsItemMock.Setup(m => m.CommodityCode).Returns("commodityTest");
			goodsItemMock.Setup(m => m.GoodsDescription).Returns(description);
			goodsItemMock.Setup(m => m.ProducedDocumentsCertificates).Returns(new IProducedDocumentCertificate[] { MessageBuilderUtilities.SetupDocumentCertificate("380", "SD1", "REF1"), MessageBuilderUtilities.SetupDocumentCertificate("18", "SD2", "REF2") });
			goodsItemMock.Setup(m => m.GrossMass).Returns(12.1234m);
			goodsItemMock.Setup(m => m.NetMass).Returns(10.1234m);
			goodsItemMock.Setup(m => m.Containers).Returns(new ZString[] { "123456" });
			goodsItemMock.Setup(m => m.Packages).Returns(new IPackage[] { packageMock.Object });
			return goodsItemMock;
		}

		Mock<ITrader> destinationTraderMock;
		Mock<IPackage> packageMock;
		Mock<ICC044ADeclaration> dataProviderMock;
		Mock<IUnloadingRemarkInterface> unloadingRemarkMock;

		CC044AMessageBuilder messageBuilder;
		IControlResult SetupGoodsItemControlResult(ZString description, ZString controlIndicator, ZString pointerToTheAttribute, ZString correctedValue)
		{
			var goodsItemControlResultMock = new Mock<IControlResult>();
			goodsItemControlResultMock.Setup(m => m.Description).Returns(description);
			goodsItemControlResultMock.Setup(m => m.ControlIndicator).Returns(controlIndicator);
			goodsItemControlResultMock.Setup(m => m.PointerToTheAttribute).Returns(pointerToTheAttribute);
			goodsItemControlResultMock.Setup(m => m.CorrectedValue).Returns(correctedValue);
			return goodsItemControlResultMock.Object;
		}
	}
}
