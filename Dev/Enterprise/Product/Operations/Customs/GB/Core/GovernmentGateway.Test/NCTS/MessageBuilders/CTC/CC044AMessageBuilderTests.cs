using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;
using ROC = Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	class CC044AMessageBuilderTest : TestCaseWithFactory
	{
		public void TestCC044AMessageBuilder_NumOfPacGS24_NumOfPieGS25()
		{
			var expected = GetDataProviderMock(GetUnloadingRemarkMock());
			var destinationTraderMock = new Mock<ITrader>();
			var producedDocumentCertificate1 = SetupProducedDocumentCertificate("DocType1", "DocReference1", "Compliment1");
			var goodsItemControlResult1 = SetupGoodsItemControlResult("Comment1", "", "", ROC.ResultOfControlCodes.Codes.Other);
			var sgiCodeMock1 = SetupGoodsItemSgiCode("SGI1", 1.1m);

			dataProviderMock = GetDataProviderMock(GetUnloadingRemarkMock());

			var testData = new (ZInt packageNumber, ZInt pieceNumber, bool isPBulk, bool isUnpacked, bool shouldIncludeGS24, bool shouldIncludeGS25)[]
			{
				//IsBulk=False IsUnpacked=False
				(1, 1, false, false, true, false),
				(1, 0, false, false, true, false),
				(0, 0, false, false, true, false),
				(0, 1, false, false, true, false),
				//IsBulk=True IsUnpacked=False
				(1, 1, true, false, false, false),
				(1, 0, true, false, false, false),
				(0, 0, true, false, false, false),
				(0, 1, true, false, false, false),
				//IsBulk=False IsUnpacked=True
				(1, 1, false, true, false, true),
				(1, 0, false, true, false, true),
				(0, 0, false, true, false, true),
				(0, 1, false, true, false, true)
			};

			CombineAssertions(() =>
			{
				foreach (var (packageNumber, pieceNumber, isBulk, isUnpacked, shouldIncludeGS24, shouldIncludeGS25) in testData)
				{
					var packageMock1 = SetupGoodsItemPackage("Mark1", "PKG", packageNumber, pieceNumber, isBulk, isUnpacked);
					var expectedGoodsItemMock1 = GetCommonGoodsItemMock(1, 1m, new IProducedDocumentCertificate[] { producedDocumentCertificate1 }, new IPackage[] { packageMock1 });
					var unloadedGoodsItemMock1 = GetUnloadedGoodsItemMock(1, 1m, new IProducedDocumentCertificate[] { producedDocumentCertificate1 }, new IPackage[] { packageMock1 }, new IControlResult[] { goodsItemControlResult1 }, new ISgiCode[] { sgiCodeMock1 }, true);
					dataProviderMock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { unloadedGoodsItemMock1.Object });
					messageBuilder = new CC044AXmlMessageBuilder(expected.Object, dataProviderMock.Object, new ErrorCollector());
					var xml = messageBuilder.GetXMLMessageWithoutNamespaces().Replace("\r\n", "").Replace("\t", "");
					if (shouldIncludeGS24)
					{
						AssertContains($"should include NumOfPacGS24:{packageNumber}", $"<NumOfPacGS24>{packageNumber}</NumOfPacGS24>", xml);
					}
					else
					{
						AssertNotContains($"should not include NumOfPacGS24", "<NumOfPacGS24>", xml);
					}

					if (shouldIncludeGS25)
					{
						AssertContains($"should include NumOfPacGS25:{pieceNumber}", $"<NumOfPieGS25>{pieceNumber}</NumOfPieGS25>", xml);
					}
					else
					{
						AssertNotContains("should not include NumOfPacGS25", "<NumOfPieGS25>", xml);
					}
				}
			});
			destinationTraderMock.VerifyAll();
		}

		public void TestNoMaterialDifferenceWritesNoRocNorGoodsItemsEvenIfTheSupposedDifferenceHasResultTypeDI()
		{
			// This simulates the original unloaded value ("NZ"), with a 'DI' value for the control indicator, but since there is no material change (old=NZ, new=NZ) we still don'' write a difference
			var controlResult = CTCMessageBuilderUtilities.SetupControlResult(description: "", controlIndicator: "DI", pointerToTheAttribute: Business.NctsHeader.MeansOfTransportAtDepartureNationalityPointer, correctedValue: Core.Constants.CountryCodes.NewZealand);
			dataProviderMock.Setup(m => m.ControlResultList).Returns(new IControlResult[] { controlResult });

			var actual = GetDataProviderMock(GetUnloadingRemarkMock("Y", "Y", "Y"));
			var expected = GetDataProviderMock(GetUnloadingRemarkMock("Y", "Y", "Y"));
			actual.Setup(m => m.HeaderUnloadingNotes).Returns("");
			var xmlMsgWithNoNamespaces = new CC044AXmlMessageBuilder(expected.Object, actual.Object, new ErrorCollector()).GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertNotContains("No goods items are mentioned", "GOOITEGDS", xmlMsgWithNoNamespaces);
				AssertNotContains("No header result of control.  One exists, and it has an old-fashioned 'DI'control indicator, but it's not written 'cos actual (NZ) is same as expected (NZ)", "RESOFCON534", xmlMsgWithNoNamespaces);
			});
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC044AMessageBuilderHeaderConforms()
		{
			var actualAndExpectedAreBothTheSameThatMeansItIsConforming = GetDataProviderMock(GetUnloadingRemarkMock("Y", "Y", "Y"));
			actualAndExpectedAreBothTheSameThatMeansItIsConforming.Setup(m => m.HeaderUnloadingNotes).Returns("");
			messageBuilder = new CC044AXmlMessageBuilder(actualAndExpectedAreBothTheSameThatMeansItIsConforming.Object, actualAndExpectedAreBothTheSameThatMeansItIsConforming.Object, new ErrorCollector());
			var expectedXml = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("CC044AMessageXmlHeaderConforms.xml");
			AssertContains(expectedXml, messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC044AMessageBuilderHeaderDifferences()
		{
			var dataProviderMockForActual = GetDataProviderMock(GetUnloadingRemarkMock("Y", "N", "Y"));
			dataProviderMockForActual.Setup(m => m.TotalGrossMass).Returns(3500); // new/expected mass
			dataProviderMockForActual.Setup(m => m.TotalNumberOfPackages).Returns(666); // new/expected packs
			dataProviderMockForActual.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("MT10VHC"); // new/expected vehicle
			dataProviderMockForActual.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns("FR"); // new/expected nationality 

			messageBuilder = new CC044AXmlMessageBuilder(dataProviderMock.Object, dataProviderMockForActual.Object, new ErrorCollector());
			var expectedXml = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("CC044AMessageXmlHeaderDifferences.xml");
			AssertContains(expectedXml, messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestCC044AMessageBuilder_PopulateSEAINFSLIType()
		{
			var expected = GetDataProviderMock().Object;
			var actualProvider = GetDataProviderMock(GetUnloadingRemarkMock(stateOfSealsOK: "N", rawNoOfSeals: 1));
			messageBuilder = new CC044AXmlMessageBuilder(expected, actualProvider.Object, new ErrorCollector());
			AssertContains("XML contains number of seals", "<SeaNumSLI2>1</SeaNumSLI2>", messageBuilder.GetXMLMessageWithoutNamespaces());

			actualProvider = GetDataProviderMock(GetUnloadingRemarkMock(rawNoOfSeals: 0));
			messageBuilder = new CC044AXmlMessageBuilder(expected, actualProvider.Object, new ErrorCollector());
			AssertContains("XML contains number of seals", "<SeaNumSLI2>0</SeaNumSLI2>", messageBuilder.GetXMLMessageWithoutNamespaces());

			actualProvider = GetDataProviderMock(GetUnloadingRemarkMock(stateOfSealsOK: "", rawNoOfSeals: 4));
			messageBuilder = new CC044AXmlMessageBuilder(expected, actualProvider.Object, new ErrorCollector());
			AssertNotContains("XML does not contain SEAINFSLI as state of seals is blank", "<SEAINFSLI>", messageBuilder.GetXMLMessageWithoutNamespaces());

			actualProvider.Setup(m => m.Seals).Returns(new List<ISealID>() { CTCMessageBuilderUtilities.SetupSeal("") }); // blank seal
			AssertNotContains("XML does not contain even an empty <SEAIDSID/>", "<SEAIDSID", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestCC044AMessageBuilder_NewGoodsItem()
		{
			newGoodsItemMock = new Mock<IUnloadedGoodsItem>();
			var newGoodsItemControlResult = SetupGoodsItemControlResult("", "", "", ROC.ResultOfControlCodes.Codes.New);
			goodsItemControlResult_withOK = SetupGoodsItemControlResult("", "", "", "OK");
			goodsItemControlResult_withPointer = SetupGoodsItemControlResult("Moved from Pointer to ControlIndicator", "", "", ROC.ResultOfControlCodes.Codes.Other);
			newGoodsItemMock.Setup(m => m.ResultsOfControl).Returns(new IControlResult[] { newGoodsItemControlResult, goodsItemControlResult1, goodsItemControlResult_withOK, goodsItemControlResult_withPointer });
			newGoodsItemMock.Setup(m => m.IsNew).Returns(true);
			newGoodsItemMock.Setup(m => m.IsMissing).Returns(false);
			//newGoodsItemMock.Setup(m => m.HasDifferences).Returns(false);

			newGoodsItemMock.Setup(m => m.ItemNumber).Returns("3");
			newGoodsItemMock.Setup(m => m.CommodityCode).Returns("333333330000");
			newGoodsItemMock.Setup(m => m.GoodsDescription).Returns("GoodsDescription3");
			newGoodsItemMock.Setup(m => m.GoodsDescriptionLanguage).Returns("EN-US");
			newGoodsItemMock.Setup(m => m.GrossMass).Returns(3.3333m);
			newGoodsItemMock.Setup(m => m.NetMass).Returns(3.3333m);
			newGoodsItemMock.Setup(m => m.ProducedDocumentsCertificates).Returns(new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 });
			newGoodsItemMock.Setup(m => m.Containers).Returns(new ZString[] { "Container1", "Container2" });
			newGoodsItemMock.Setup(m => m.Packages).Returns(new IPackage[] { packageMock1, packageMock2 });
			newGoodsItemMock.Setup(m => m.SGICodes).Returns(new ISgiCode[] { sgiCodeMock1, sgiCodeMock2 });
			dataProviderMock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { unloadedGoodsItemMock1.Object, unloadedGoodsItemMock2.Object, newGoodsItemMock.Object });

			var expected = GetDataProviderMock(); // the content of the expected header is not important, we are comparing lines only
			expected.Setup(m => m.ExpectedGoodsItems).Returns(new ICommonGoodsItem[] { expectedGoodsItemMock1.Object, expectedGoodsItemMock2.Object });

			messageBuilder = new CC044AXmlMessageBuilder(expected.Object, dataProviderMock.Object, new ErrorCollector());
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("XML contains new unloaded item with RoC=NE", @"<GOOITEGDS>
    <IteNumGDS7>3</IteNumGDS7>
    <ComCodTarCodGDS10>33333333</ComCodTarCodGDS10>
    <GooDesGDS23>GoodsDescription3</GooDesGDS23>
    <GooDesGDS23LNG>EN</GooDesGDS23LNG>
    <GroMasGDS46>3.333</GroMasGDS46>
    <NetMasGDS48>3.333</NetMasGDS48>
    <PRODOCDC2>
      <DocTypDC21>DocType1</DocTypDC21>
      <DocRefDC23>DocReference1</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment1</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <PRODOCDC2>
      <DocTypDC21>DocType2</DocTypDC21>
      <DocRefDC23>DocReference2</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment2</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <RESOFCONROC>
      <ConIndROC1>NE</ConIndROC1>
    </RESOFCONROC>
    <RESOFCONROC>
      <DesROC2>Comment1</DesROC2>
      <ConIndROC1>OT</ConIndROC1>
    </RESOFCONROC>
    <RESOFCONROC>
      <DesROC2>Moved from Pointer to ControlIndicator</DesROC2>
      <ConIndROC1>OT</ConIndROC1>
    </RESOFCONROC>
    <CONNR2>
      <ConNumNR21>Container1</ConNumNR21>
    </CONNR2>
    <CONNR2>
      <ConNumNR21>Container2</ConNumNR21>
    </CONNR2>
    <PACGS2>
      <MarNumOfPacGS21>Mark1</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>PKG</KinOfPacGS23>
      <NumOfPacGS24>1</NumOfPacGS24>
    </PACGS2>
    <PACGS2>
      <MarNumOfPacGS21>Mark2</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>BOX</KinOfPacGS23>
      <NumOfPacGS24>2</NumOfPacGS24>
    </PACGS2>
    <SGICODSD2>
      <SenGooCodSD22>SGI1</SenGooCodSD22>
      <SenQuaSD23>1.111</SenQuaSD23>
    </SGICODSD2>
    <SGICODSD2>
      <SenGooCodSD22>SGI2</SenGooCodSD22>
      <SenQuaSD23>2.222</SenQuaSD23>
    </SGICODSD2>
  </GOOITEGDS>", xmlMsgWithNoNamespaces);
			newGoodsItemMock.VerifyAll();
		}

		public void TestCC044AMessageBuilder_MissingGoodsItem2()
		{
			missingGoodsItemMock = new Mock<IUnloadedGoodsItem>();
			goodsItemControlResult_withOK = SetupGoodsItemControlResult("", "", "", "OK");
			goodsItemControlResult_withPointer = SetupGoodsItemControlResult("Moved from Pointer to ControlIndicator", "", "", ROC.ResultOfControlCodes.Codes.Other);
			var missingGoodsItemControlResult = SetupGoodsItemControlResult("", "", "", ROC.ResultOfControlCodes.Codes.Missing);
			missingGoodsItemMock.Setup(m => m.ResultsOfControl).Returns(new IControlResult[] { missingGoodsItemControlResult, goodsItemControlResult1, goodsItemControlResult_withOK, goodsItemControlResult_withPointer });
			missingGoodsItemMock.Setup(m => m.ItemNumber).Returns("2");
			missingGoodsItemMock.Setup(m => m.IsMissing).Returns(true);
			missingGoodsItemMock.Setup(m => m.IsNew).Returns(false);
			missingGoodsItemMock.Setup(m => m.HasDifferences).Returns(false);
			dataProviderMock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { unloadedGoodsItemMock1.Object, missingGoodsItemMock.Object });

			var expected = GetDataProviderMock(); // the content of the expected header is not important, we are comparing lines only
			expected.Setup(m => m.ExpectedGoodsItems).Returns(new ICommonGoodsItem[] { expectedGoodsItemMock1.Object, expectedGoodsItemMock2.Object });
			messageBuilder = new CC044AXmlMessageBuilder(expected.Object, dataProviderMock.Object, new ErrorCollector());
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("XML contains missing unloaded item with RoC=OR and RoC=DI", @"<GOOITEGDS>
    <IteNumGDS7>2</IteNumGDS7>
    <ComCodTarCodGDS10>2Commodi</ComCodTarCodGDS10>
    <GooDesGDS23>2GoodsDescription2</GooDesGDS23>
    <GooDesGDS23LNG>EN</GooDesGDS23LNG>
    <GroMasGDS46>2</GroMasGDS46>
    <NetMasGDS48>2</NetMasGDS48>
    <PRODOCDC2>
      <DocTypDC21>DocType1</DocTypDC21>
      <DocRefDC23>DocReference1</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment1</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <PRODOCDC2>
      <DocTypDC21>DocType2</DocTypDC21>
      <DocRefDC23>DocReference2</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment2</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <RESOFCONROC>
      <ConIndROC1>OR</ConIndROC1>
    </RESOFCONROC>
    <CONNR2>
      <ConNumNR21>Container1</ConNumNR21>
    </CONNR2>
    <CONNR2>
      <ConNumNR21>Container2</ConNumNR21>
    </CONNR2>
    <PACGS2>
      <MarNumOfPacGS21>Mark1</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>PKG</KinOfPacGS23>
      <NumOfPacGS24>1</NumOfPacGS24>
    </PACGS2>
    <PACGS2>
      <MarNumOfPacGS21>Mark2</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>BOX</KinOfPacGS23>
      <NumOfPacGS24>2</NumOfPacGS24>
    </PACGS2>
  </GOOITEGDS>
  <GOOITEGDS>
    <IteNumGDS7>2</IteNumGDS7>
    <RESOFCONROC>
      <ConIndROC1>DI</ConIndROC1>
    </RESOFCONROC>
    <RESOFCONROC>
      <DesROC2>Comment1</DesROC2>
      <ConIndROC1>OT</ConIndROC1>
    </RESOFCONROC>
    <RESOFCONROC>
      <DesROC2>Moved from Pointer to ControlIndicator</DesROC2>
      <ConIndROC1>OT</ConIndROC1>
    </RESOFCONROC>
  </GOOITEGDS>", xmlMsgWithNoNamespaces);
			missingGoodsItemMock.VerifyAll();
		}

		public void TestCC044AMessageBuilder_DifferenceInGoodsItem2()
		{
			differencesGoodsItemMock = new Mock<IUnloadedGoodsItem>();
			var differencesGoodsItemControlResult = SetupGoodsItemControlResult("", "", "", ROC.ResultOfControlCodes.Codes.Different);
			goodsItemControlResult_withOK = SetupGoodsItemControlResult("", "", "", "OK");
			goodsItemControlResult_withPointer = SetupGoodsItemControlResult("Moved from Pointer to ControlIndicator", "", "", ROC.ResultOfControlCodes.Codes.Other);
			differencesGoodsItemMock.Setup(m => m.ResultsOfControl).Returns(new IControlResult[] { differencesGoodsItemControlResult, goodsItemControlResult1, goodsItemControlResult_withOK, goodsItemControlResult_withPointer });
			differencesGoodsItemMock.Setup(m => m.ItemNumber).Returns("2");
			differencesGoodsItemMock.Setup(m => m.CommodityCode).Returns("999999990000");
			differencesGoodsItemMock.Setup(m => m.GoodsDescription).Returns("GoodsDescription9");
			differencesGoodsItemMock.Setup(m => m.GoodsDescriptionLanguage).Returns("EN-US");
			differencesGoodsItemMock.Setup(m => m.GrossMass).Returns(9m);
			differencesGoodsItemMock.Setup(m => m.NetMass).Returns(9m);
			differencesGoodsItemMock.Setup(m => m.ProducedDocumentsCertificates).Returns(new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 });
			differencesGoodsItemMock.Setup(m => m.Containers).Returns(new ZString[] { "Container1", "Container2" });
			differencesGoodsItemMock.Setup(m => m.Packages).Returns(new IPackage[] { packageMock1, packageMock2 });
			differencesGoodsItemMock.Setup(m => m.SGICodes).Returns(new ISgiCode[] { sgiCodeMock1, sgiCodeMock2 });
			differencesGoodsItemMock.Setup(m => m.IsNew).Returns(false);
			differencesGoodsItemMock.Setup(m => m.IsMissing).Returns(false);
			differencesGoodsItemMock.Setup(m => m.HasDifferences).Returns(true);
			dataProviderMock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { unloadedGoodsItemMock1.Object, differencesGoodsItemMock.Object });

			var expected = GetDataProviderMock(); // the content of the expected header is not important, we are comparing lines only
			expected.Setup(m => m.ExpectedGoodsItems).Returns(new ICommonGoodsItem[] { expectedGoodsItemMock1.Object, expectedGoodsItemMock2.Object });

			messageBuilder = new CC044AXmlMessageBuilder(expected.Object, dataProviderMock.Object, new ErrorCollector());
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("XML contains unloaded item with differences with RoC=OR and RoC=DI", @"<GOOITEGDS>
    <IteNumGDS7>2</IteNumGDS7>
    <ComCodTarCodGDS10>2Commodi</ComCodTarCodGDS10>
    <GooDesGDS23>2GoodsDescription2</GooDesGDS23>
    <GooDesGDS23LNG>EN</GooDesGDS23LNG>
    <GroMasGDS46>2</GroMasGDS46>
    <NetMasGDS48>2</NetMasGDS48>
    <PRODOCDC2>
      <DocTypDC21>DocType1</DocTypDC21>
      <DocRefDC23>DocReference1</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment1</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <PRODOCDC2>
      <DocTypDC21>DocType2</DocTypDC21>
      <DocRefDC23>DocReference2</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment2</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <RESOFCONROC>
      <ConIndROC1>OR</ConIndROC1>
    </RESOFCONROC>
    <CONNR2>
      <ConNumNR21>Container1</ConNumNR21>
    </CONNR2>
    <CONNR2>
      <ConNumNR21>Container2</ConNumNR21>
    </CONNR2>
    <PACGS2>
      <MarNumOfPacGS21>Mark1</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>PKG</KinOfPacGS23>
      <NumOfPacGS24>1</NumOfPacGS24>
    </PACGS2>
    <PACGS2>
      <MarNumOfPacGS21>Mark2</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>BOX</KinOfPacGS23>
      <NumOfPacGS24>2</NumOfPacGS24>
    </PACGS2>
  </GOOITEGDS>
  <GOOITEGDS>
    <IteNumGDS7>2</IteNumGDS7>
    <ComCodTarCodGDS10>99999999</ComCodTarCodGDS10>
    <GooDesGDS23>GoodsDescription9</GooDesGDS23>
    <GooDesGDS23LNG>EN</GooDesGDS23LNG>
    <GroMasGDS46>9</GroMasGDS46>
    <NetMasGDS48>9</NetMasGDS48>
    <PRODOCDC2>
      <DocTypDC21>DocType1</DocTypDC21>
      <DocRefDC23>DocReference1</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment1</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <PRODOCDC2>
      <DocTypDC21>DocType2</DocTypDC21>
      <DocRefDC23>DocReference2</DocRefDC23>
      <DocRefDCLNG>EN</DocRefDCLNG>
      <ComOfInfDC25>Compliment2</ComOfInfDC25>
      <ComOfInfDC25LNG>EN</ComOfInfDC25LNG>
    </PRODOCDC2>
    <RESOFCONROC>
      <ConIndROC1>DI</ConIndROC1>
    </RESOFCONROC>
    <RESOFCONROC>
      <DesROC2>Comment1</DesROC2>
      <ConIndROC1>OT</ConIndROC1>
    </RESOFCONROC>
    <RESOFCONROC>
      <DesROC2>Moved from Pointer to ControlIndicator</DesROC2>
      <ConIndROC1>OT</ConIndROC1>
    </RESOFCONROC>
    <CONNR2>
      <ConNumNR21>Container1</ConNumNR21>
    </CONNR2>
    <CONNR2>
      <ConNumNR21>Container2</ConNumNR21>
    </CONNR2>
    <PACGS2>
      <MarNumOfPacGS21>Mark1</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>PKG</KinOfPacGS23>
      <NumOfPacGS24>1</NumOfPacGS24>
    </PACGS2>
    <PACGS2>
      <MarNumOfPacGS21>Mark2</MarNumOfPacGS21>
      <MarNumOfPacGS21LNG>EN</MarNumOfPacGS21LNG>
      <KinOfPacGS23>BOX</KinOfPacGS23>
      <NumOfPacGS24>2</NumOfPacGS24>
    </PACGS2>
    <SGICODSD2>
      <SenGooCodSD22>SGI1</SenGooCodSD22>
      <SenQuaSD23>1.111</SenQuaSD23>
    </SGICODSD2>
    <SGICODSD2>
      <SenGooCodSD22>SGI2</SenGooCodSD22>
      <SenQuaSD23>2.222</SenQuaSD23>
    </SGICODSD2>
  </GOOITEGDS>", xmlMsgWithNoNamespaces);
			differencesGoodsItemMock.VerifyAll();
		}

		public void TestZeroMassesNotSent()
		{
			expectedGoodsItemMock1 = GetCommonGoodsItemMock(1, 0m, new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 }, new IPackage[] { packageMock1, packageMock2 });
			expectedGoodsItemMock2 = GetCommonGoodsItemMock(2, 0m, new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 }, new IPackage[] { packageMock1, packageMock2 });

			unloadedGoodsItemMock1 = GetUnloadedGoodsItemMock(1, 0m,
				new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 },
				new IPackage[] { packageMock1, packageMock2 },
				new IControlResult[] { goodsItemControlResult1, goodsItemControlResult2 },
				new ISgiCode[] { sgiCodeMock1, sgiCodeMock2 });

			differencesGoodsItemMock = new Mock<IUnloadedGoodsItem>();
			var differencesGoodsItemControlResult = SetupGoodsItemControlResult("", "", "", ROC.ResultOfControlCodes.Codes.Different);
			goodsItemControlResult_withOK = SetupGoodsItemControlResult("", "", "", "OK");
			goodsItemControlResult_withPointer = SetupGoodsItemControlResult("Moved from Pointer to ControlIndicator", "", "", ROC.ResultOfControlCodes.Codes.Other);
			differencesGoodsItemMock.Setup(m => m.ResultsOfControl).Returns(new IControlResult[] { differencesGoodsItemControlResult, goodsItemControlResult1, goodsItemControlResult_withOK, goodsItemControlResult_withPointer });
			differencesGoodsItemMock.Setup(m => m.ItemNumber).Returns("2");
			differencesGoodsItemMock.Setup(m => m.CommodityCode).Returns("999999990000");
			differencesGoodsItemMock.Setup(m => m.GoodsDescription).Returns("GoodsDescription9");
			differencesGoodsItemMock.Setup(m => m.GoodsDescriptionLanguage).Returns("EN-US");
			differencesGoodsItemMock.Setup(m => m.GrossMass).Returns(0m);
			differencesGoodsItemMock.Setup(m => m.NetMass).Returns(0m);
			differencesGoodsItemMock.Setup(m => m.ProducedDocumentsCertificates).Returns(new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 });
			differencesGoodsItemMock.Setup(m => m.Containers).Returns(new ZString[] { "Container1", "Container2" });
			differencesGoodsItemMock.Setup(m => m.Packages).Returns(new IPackage[] { packageMock1, packageMock2 });
			differencesGoodsItemMock.Setup(m => m.SGICodes).Returns(new ISgiCode[] { sgiCodeMock1, sgiCodeMock2 });
			differencesGoodsItemMock.Setup(m => m.IsNew).Returns(false);
			differencesGoodsItemMock.Setup(m => m.IsMissing).Returns(false);
			differencesGoodsItemMock.Setup(m => m.HasDifferences).Returns(true);
			dataProviderMock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { unloadedGoodsItemMock1.Object, differencesGoodsItemMock.Object });

			var expected = GetDataProviderMock();
			expected.Setup(m => m.ExpectedGoodsItems).Returns(new ICommonGoodsItem[] { expectedGoodsItemMock1.Object, expectedGoodsItemMock2.Object });

			messageBuilder = new CC044AXmlMessageBuilder(expected.Object, dataProviderMock.Object, new ErrorCollector());
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();

			AssertNotContains("GroMasGDS46", xmlMsgWithNoNamespaces, ignoreCase: true);
			AssertNotContains("NetMasGDS48", xmlMsgWithNoNamespaces, ignoreCase: true);
		}

		public void TestPopulateTRADESTRD_NameAndAddress()
		{
			var actualOrExpectedDontCare = dataProviderMock.Object;
			messageBuilder = new CC044AXmlMessageBuilder(actualOrExpectedDontCare, actualOrExpectedDontCare, new ErrorCollector());
			destinationTraderMock.Setup(m => m.TIN).Returns(ZString.Empty);
			destinationTraderMock.Setup(m => m.Name).Returns("Oscorp Industries3");
			destinationTraderMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");
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
			destinationTraderMock.VerifyAll();
		}

		public void TestPopulateTRADESTRD_Null()
		{
			var actualOrExpectedDontCare = dataProviderMock.Object;
			messageBuilder = new CC044AXmlMessageBuilder(actualOrExpectedDontCare, actualOrExpectedDontCare, new ErrorCollector());
			dataProviderMock.Setup(m => m.DestinationTrader).Returns((ITrader)null);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertNotContains("TRADESTRD", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateUNLREMREMType_StaOfTheSeaOKREM19()
		{
			var errorCollector = new ErrorCollector();

			CombineAssertions("Convert StateOfSealsOk", () =>
			{
				var providerExpected = GetDataProviderMock();
				var providerActual = GetDataProviderMock(GetUnloadingRemarkMock(stateOfSealsOK: "Y"));
				var messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				var expectedContent = "<StaOfTheSeaOKREM19>1</StaOfTheSeaOKREM19>";
				AssertContains("Y => 1", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());

				providerActual = GetDataProviderMock(GetUnloadingRemarkMock(stateOfSealsOK: "N"));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				expectedContent = "<StaOfTheSeaOKREM19>0</StaOfTheSeaOKREM19>";
				AssertContains("N => 0", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());

				providerActual = GetDataProviderMock(GetUnloadingRemarkMock(stateOfSealsOK: ""));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				var unexpectedContent = "<StaOfTheSeaOKREM19>";
				AssertNotContains("Empty => Not exists", unexpectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());
			});
		}
		public void TestPopulateUNLREMREMType_ConREM65()
		{
			var errorCollector = new ErrorCollector();

			CombineAssertions("Convert Conform", () =>
			{
				var providerExpected = GetDataProviderMock();
				var providerActual = GetDataProviderMock(GetUnloadingRemarkMock(conform: "Y"));
				var messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				var expectedContent = "<ConREM65>1</ConREM65>";
				AssertContains("Y => 1", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());

				providerActual = GetDataProviderMock(GetUnloadingRemarkMock(conform: "N"));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				expectedContent = "<ConREM65>0</ConREM65>";
				AssertContains("N => 0", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());

				providerActual = GetDataProviderMock(GetUnloadingRemarkMock(conform: ""));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				expectedContent = "<ConREM65>0</ConREM65>";
				AssertContains("Empty => 0", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());
			});
		}
		public void TestPopulateUNLREMREMType_UnlComREM66()
		{
			var errorCollector = new ErrorCollector();

			CombineAssertions("Convert UnloadingCompletion", () =>
			{
				var providerExpected = GetDataProviderMock();
				var providerActual = GetDataProviderMock(GetUnloadingRemarkMock(unloadingCompletion: "Y"));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				var expectedContent = "<UnlComREM66>1</UnlComREM66>";
				AssertContains("Y => 1", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());

				providerActual = GetDataProviderMock(GetUnloadingRemarkMock(unloadingCompletion: "N"));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				expectedContent = "<UnlComREM66>0</UnlComREM66>";
				AssertContains("N => 0", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());

				providerActual = GetDataProviderMock(GetUnloadingRemarkMock(unloadingCompletion: ""));
				messageBuilder = new CC044AXmlMessageBuilder(providerExpected.Object, providerActual.Object, errorCollector);
				expectedContent = "<UnlComREM66>0</UnlComREM66>";
				AssertContains("Empty => 0", expectedContent, messageBuilder.GetXMLMessageWithoutNamespaces());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			destinationTraderMock = new Mock<ITrader>();
			destinationTraderMock.Setup(m => m.TIN).Returns("GB0123456789003");

			producedDocumentCertificate1 = SetupProducedDocumentCertificate("DocType1", "DocReference1", "Compliment1");
			producedDocumentCertificate2 = SetupProducedDocumentCertificate("DocType2", "DocReference2", "Compliment2");

			goodsItemControlResult1 = SetupGoodsItemControlResult("Comment1", "", "", ROC.ResultOfControlCodes.Codes.Other);
			goodsItemControlResult2 = SetupGoodsItemControlResult("Comment2", "", "", ROC.ResultOfControlCodes.Codes.Other);

			packageMock1 = SetupGoodsItemPackage("Mark1", "PKG", 1, 1, false, false);
			packageMock2 = SetupGoodsItemPackage("Mark2", "BOX", 2, 2, false, false);

			sgiCodeMock1 = SetupGoodsItemSgiCode("SGI1", 1.1111m);
			sgiCodeMock2 = SetupGoodsItemSgiCode("SGI2", 2.2222m);

			expectedGoodsItemMock1 = GetCommonGoodsItemMock(1, 1m, new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 }, new IPackage[] { packageMock1, packageMock2 });
			expectedGoodsItemMock2 = GetCommonGoodsItemMock(2, 2m, new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 }, new IPackage[] { packageMock1, packageMock2 });

			unloadedGoodsItemMock1 = GetUnloadedGoodsItemMock(1, 1m,
				new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 },
				new IPackage[] { packageMock1, packageMock2 },
				new IControlResult[] { goodsItemControlResult1, goodsItemControlResult2 },
				new ISgiCode[] { sgiCodeMock1, sgiCodeMock2 });

			unloadedGoodsItemMock2 = GetUnloadedGoodsItemMock(2, 2m,
				new IProducedDocumentCertificate[] { producedDocumentCertificate1, producedDocumentCertificate2 },
				new IPackage[] { packageMock1, packageMock2 },
				new IControlResult[] { goodsItemControlResult1, goodsItemControlResult2 },
				new ISgiCode[] { sgiCodeMock1, sgiCodeMock2 });

			dataProviderMock = GetDataProviderMock(GetUnloadingRemarkMock());
		}

		ISgiCode SetupGoodsItemSgiCode(ZString sgiCode, ZDecimal sgiQty)
		{
			var sgiMock = new Mock<ISgiCode>();
			sgiMock.Setup(m => m.Code).Returns(sgiCode);
			sgiMock.Setup(m => m.Qty).Returns(sgiQty);
			return sgiMock.Object;
		}

		Mock<ICC044ADeclaration> GetDataProviderMock()
		{
			var providerMock = new Mock<ICC044ADeclaration>();
			providerMock.Setup(m => m.MovementReferenceNumber).Returns("MRN123");
			providerMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("REG DEP1");
			providerMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns("EN-US ");
			providerMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.NewZealand);
			providerMock.Setup(m => m.TotalNumberOfItems).Returns(33);
			providerMock.Setup(m => m.TotalNumberOfPackages).Returns(66);
			providerMock.Setup(m => m.TotalGrossMass).Returns(99.9999);

			providerMock.Setup(m => m.HeaderUnloadingNotes).Returns("Unloading Notes");
			providerMock.Setup(m => m.HeaderUnloadingNotesLanguage).Returns("EN-US");
			providerMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("GB000001");
			providerMock.Setup(m => m.Seals).Returns(new List<ISealID>() { CTCMessageBuilderUtilities.SetupSeal("SEAL1"), CTCMessageBuilderUtilities.SetupSeal("SEAL2"), CTCMessageBuilderUtilities.SetupSeal("SEAL3") });
			providerMock.Setup(m => m.IsProduction).Returns(false);

			return providerMock;
		}

		Mock<ICC044ADeclaration> GetDataProviderMock(IUnloadingRemark unloadingRemark)
		{
			var mock = GetDataProviderMock();
			mock.Setup(m => m.DestinationTrader).Returns(destinationTraderMock.Object);
			mock.Setup(m => m.UnloadingRemark).Returns(unloadingRemark);
			mock.Setup(m => m.ControlResultList).Returns(GetResultsOfControlMock());
			mock.Setup(m => m.ExpectedGoodsItems).Returns(new ICommonGoodsItem[] { expectedGoodsItemMock1.Object, expectedGoodsItemMock2.Object });
			mock.Setup(m => m.UnloadedGoodsItems).Returns(new IUnloadedGoodsItem[] { unloadedGoodsItemMock1.Object, unloadedGoodsItemMock2.Object });
			return mock;
		}

		static Mock<IUnloadedGoodsItem> GetUnloadedGoodsItemMock(int id, ZDecimal mass, IProducedDocumentCertificate[] docCertificates, IPackage[] packages, IControlResult[] controlResults, ISgiCode[] sgiCodes, bool isNew = false)
		{
			var unloadedMock = new Mock<IUnloadedGoodsItem>();
			unloadedMock.Setup(m => m.ItemNumber).Returns($"{id}");
			unloadedMock.Setup(m => m.CommodityCode).Returns($"{id}CommodityCode{id}");
			unloadedMock.Setup(m => m.GoodsDescription).Returns($"{id}GoodsDescription{id}");
			unloadedMock.Setup(m => m.GoodsDescriptionLanguage).Returns("EN-US");
			unloadedMock.Setup(m => m.GrossMass).Returns(mass);
			unloadedMock.Setup(m => m.NetMass).Returns(mass);
			unloadedMock.Setup(m => m.ProducedDocumentsCertificates).Returns(docCertificates);
			unloadedMock.Setup(m => m.ResultsOfControl).Returns(controlResults);
			unloadedMock.Setup(m => m.Containers).Returns(new ZString[] { "Container1", "Container2" });
			unloadedMock.Setup(m => m.Packages).Returns(packages);
			unloadedMock.Setup(m => m.SGICodes).Returns(sgiCodes);
			unloadedMock.Setup(m => m.IsNew).Returns(isNew);
			unloadedMock.Setup(m => m.IsMissing).Returns(false);
			unloadedMock.Setup(m => m.HasDifferences).Returns(false);
			return unloadedMock;
		}

		static Mock<ICommonGoodsItem> GetCommonGoodsItemMock(int id, ZDecimal mass, IProducedDocumentCertificate[] docCertificates, IPackage[] packages)
		{
			var goodsItemMock = new Mock<ICommonGoodsItem>();
			goodsItemMock.Setup(m => m.ItemNumber).Returns(new ZInt(id));
			goodsItemMock.Setup(m => m.CommodityCode).Returns($"{id}CommodityCode{id}");
			goodsItemMock.Setup(m => m.GoodsDescription).Returns($"{id}GoodsDescription{id}");
			goodsItemMock.Setup(m => m.GoodsDescriptionLanguage).Returns("EN-US");
			goodsItemMock.Setup(m => m.GrossMass).Returns(mass);
			goodsItemMock.Setup(m => m.NetMass).Returns(mass);
			goodsItemMock.Setup(m => m.ProducedDocumentsCertificates).Returns(docCertificates);
			goodsItemMock.Setup(m => m.Containers).Returns(new ZString[] { "Container1", "Container2" });
			goodsItemMock.Setup(m => m.Packages).Returns(packages);
			return goodsItemMock;
		}

		static IUnloadingRemark GetUnloadingRemarkMock(string stateOfSealsOK = "N", string conform = "N", string unloadingCompletion = "N", int noOfSeals = 3, int rawNoOfSeals = 4)
		{
			var unloadingRemarkMock = new Mock<IUnloadingRemark>();
			unloadingRemarkMock.Setup(m => m.StateOfSealsOk).Returns(stateOfSealsOK);
			unloadingRemarkMock.Setup(m => m.UnloadingRemark).Returns("Remark1");
			unloadingRemarkMock.Setup(m => m.UnloadingRemarkLanguage).Returns("EN-US");
			unloadingRemarkMock.Setup(m => m.Conform).Returns(conform);
			unloadingRemarkMock.Setup(m => m.UnloadingCompletion).Returns(unloadingCompletion);
			unloadingRemarkMock.Setup(m => m.UnloadingDate).Returns("111213");
			unloadingRemarkMock.Setup(m => m.NoOfSeals).Returns(noOfSeals);
			unloadingRemarkMock.Setup(m => m.RawNoOfSeals).Returns(rawNoOfSeals);
			return unloadingRemarkMock.Object;
		}

		IControlResult[] GetResultsOfControlMock()
		{
			var controlResult1 = CTCMessageBuilderUtilities.SetupControlResult("rocDecription1", "rocIndicator1", "rocPointer1", "rocValue1");
			var controlResult2 = CTCMessageBuilderUtilities.SetupControlResult("rocDecription2", "rocIndicator2", "rocPointer2", "rocValue2");
			var resultsOfControlMock = new IControlResult[] { controlResult1, controlResult2 };
			return resultsOfControlMock;
		}

		IProducedDocumentCertificate SetupProducedDocumentCertificate(ZString documentType, ZString documentReference, ZString complementOfInformation)
		{
			var producedDocument = new Mock<IProducedDocumentCertificate>();
			producedDocument.Setup(m => m.DocumentType).Returns(documentType);
			producedDocument.Setup(m => m.DocumentReference).Returns(documentReference);
			producedDocument.Setup(m => m.DocumentReferenceLanguage).Returns("EN-US");
			producedDocument.Setup(m => m.ComplementOfInformation).Returns(complementOfInformation);
			producedDocument.Setup(m => m.ComplementOfInformationLanguage).Returns("EN-US");
			return producedDocument.Object;
		}

		IPackage SetupGoodsItemPackage(ZString marksAndNumbers, ZString kindOfPackages, ZInt numberOfPackages, ZInt numberOfPieces, ZBool isBulk, ZBool isUnpacked)
		{
			var packageMock = new Mock<IPackage>();
			packageMock.Setup(m => m.MarksAndNumbersOfPackages).Returns(marksAndNumbers);
			packageMock.Setup(m => m.MarksAndNumbersOfPackagesLanguage).Returns("EN-US");
			packageMock.Setup(m => m.KindOfPackages).Returns(kindOfPackages);
			packageMock.Setup(m => m.NumberOfPackages).Returns(new ZLong(numberOfPackages));
			packageMock.Setup(m => m.NumberOfPieces).Returns(new ZLong(numberOfPieces));
			packageMock.Setup(m => m.IsBulk).Returns(isBulk);
			packageMock.Setup(m => m.IsUnpacked).Returns(isUnpacked);
			return packageMock.Object;
		}

		IControlResult SetupGoodsItemControlResult(ZString description, ZString language, ZString controlIndicator, ZString pointerToTheAttribute)
		{
			var goodsItemControlResultMock = new Mock<IControlResult>();
			goodsItemControlResultMock.Setup(m => m.Description).Returns(description);
			goodsItemControlResultMock.Setup(m => m.DescriptionLNG).Returns(language);
			goodsItemControlResultMock.Setup(m => m.ControlIndicator).Returns(controlIndicator);
			goodsItemControlResultMock.Setup(m => m.PointerToTheAttribute).Returns(pointerToTheAttribute);
			return goodsItemControlResultMock.Object;
		}

		Mock<ICC044ADeclaration> dataProviderMock;
		Mock<ITrader> destinationTraderMock;
		CC044AXmlMessageBuilder messageBuilder;
		Mock<IUnloadedGoodsItem> unloadedGoodsItemMock1;
		Mock<IUnloadedGoodsItem> unloadedGoodsItemMock2;
		Mock<IUnloadedGoodsItem> newGoodsItemMock;
		Mock<IUnloadedGoodsItem> missingGoodsItemMock;
		Mock<IUnloadedGoodsItem> differencesGoodsItemMock;
		Mock<ICommonGoodsItem> expectedGoodsItemMock1;
		Mock<ICommonGoodsItem> expectedGoodsItemMock2;
		IPackage packageMock1;
		IPackage packageMock2;
		IProducedDocumentCertificate producedDocumentCertificate1;
		IProducedDocumentCertificate producedDocumentCertificate2;
		IControlResult goodsItemControlResult1;
		IControlResult goodsItemControlResult2;
		IControlResult goodsItemControlResult_withOK;
		IControlResult goodsItemControlResult_withPointer;
		ISgiCode sgiCodeMock1;
		ISgiCode sgiCodeMock2;
	}
}
