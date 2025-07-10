using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.CADImportDocumentWrapper;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADImportDocumentWrapper))]
	public class CADImportDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2025, 1, 10)]
		public void TestCADImportDocumentWrapper()
		{
			var mockCADHeader = new Mock<ICADHeader>();
			mockCADHeader.Setup(x => x.TypeCode_Box1).Returns("TypeCode_Box1");
			mockCADHeader.Setup(x => x.WsSType_Box2).Returns("WsSType_Box2");
			mockCADHeader.Setup(x => x.AccountingDate_Box3).Returns(ZDateTime.Now.AddHours(3));
			mockCADHeader.Setup(x => x.AccountSecurityCode_Box4).Returns("AccountSecurityCode_Box4");
			mockCADHeader.Setup(x => x.CADTransactionNo_Box4).Returns("CADTransactionNo_Box4");
			mockCADHeader.Setup(x => x.OfficeNo_Box5).Returns("OfficeNo_Box5");
			mockCADHeader.Setup(x => x.ModeOfTransport_Box6).Returns("ModeOfTransport_Box6");
			mockCADHeader.Setup(x => x.ReleaseDate_Box7).Returns(ZDateTime.Now.AddHours(7));
			mockCADHeader.Setup(x => x.GrossWeightKg_Box8).Returns(8);
			mockCADHeader.Setup(x => x.CarrierCodeAtImportation_Box9).Returns("CarrierCodeAtImportation_Box9");
			mockCADHeader.Setup(x => x.Pre_CARM_Box10).Returns("Pre_CARM_Box10");
			mockCADHeader.Setup(x => x.Z1_RPP).Returns("Z1_RPP");
			mockCADHeader.Setup(x => x.ImporterBN_Box11).Returns("ImporterBN_Box11");
			mockCADHeader.Setup(x => x.ImporterDetails_Box12).Returns("ImporterDetails_Box12");
			mockCADHeader.Setup(x => x.BrokerOrAgentBN_Box13).Returns("BrokerOrAgentBN_Box13");
			mockCADHeader.Setup(x => x.BrokerOrAgentDetails_Box14).Returns("BrokerOrAgentDetails_Box14");
			mockCADHeader.Setup(x => x.CargoControlNo_Box15).Returns("CargoControlNo_Box15");
			mockCADHeader.Setup(x => x.RecordOfIntentNo_Box16).Returns("RecordOfIntentNo_Box16");
			mockCADHeader.Setup(x => x.PreviousTransactionNo_Box17).Returns("PreviousTransactionNo_Box17");
			mockCADHeader.Setup(x => x.AcceptedDate_Box18).Returns(ZDateTime.Now.AddHours(18));
			mockCADHeader.Setup(x => x.OriginalTransactionNo_Box19).Returns("OriginalTransactionNo_Box19");
			mockCADHeader.Setup(x => x.PrevTransNoWarehouse_Box20).Returns("PrevTransNoWarehouse_Box20");
			mockCADHeader.Setup(x => x.PortOfUnlading_Box21).Returns("PortOfUnlading_Box21");
			mockCADHeader.Setup(x => x.Notes_Box35).Returns("Notes_Box35");
			mockCADHeader.Setup(x => x.TotalValueForDuty_Box113).Returns(113);
			mockCADHeader.Setup(x => x.TotalPSTAndHST_Box114).Returns(114);
			mockCADHeader.Setup(x => x.TotalPSTCannabisAmount_Box115).Returns(115);
			mockCADHeader.Setup(x => x.TotalProvAlcoholTaxAmount_Box116).Returns(116);
			mockCADHeader.Setup(x => x.TotalProvTobaccoAmount_Box117).Returns(117);
			mockCADHeader.Setup(x => x.TotalDeclarationRelieved_Box118).Returns(118);
			mockCADHeader.Setup(x => x.TotalAmount_Box119).Returns(119);
			mockCADHeader.Setup(x => x.TotalCustomsDuties_Box120).Returns(120);
			mockCADHeader.Setup(x => x.TotalExciseDuties_Box121).Returns(121);
			mockCADHeader.Setup(x => x.TotalExciseTaxes_Box122).Returns(122);
			mockCADHeader.Setup(x => x.TotalGST_Box123).Returns(123);
			mockCADHeader.Setup(x => x.TotalAnti_Dumping_Box124).Returns(124);
			mockCADHeader.Setup(x => x.TotalCountervailing_Box125).Returns(125);
			mockCADHeader.Setup(x => x.TotalSurtaxes_Box126).Returns(126);
			mockCADHeader.Setup(x => x.TotalSafeguards_Box127).Returns(127);
			mockCADHeader.Setup(x => x.TotalInterest_Box128).Returns(128);
			mockCADHeader.Setup(x => x.TotalDutiesAndTaxesWithInterest_Box129).Returns(129);
			mockCADHeader.Setup(x => x.TotalDutiesAndTaxes_Box130).Returns(130);

			var mockCADSubHeader = new Mock<ICADSubHeader>();
			mockCADHeader.Setup(x => x.CADSubHeaders).Returns(new[] { mockCADSubHeader.Object });

			var brokerBranch = Factory.New<GlbBranch>();
			brokerBranch.GB_Phone = "789";
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			broker.GS_WorkPhone = "654";
			broker.GS_PublishWorkPhone = true;
			broker.GS_GB_HomeBranch = brokerBranch.PK;
			broker.SignatureImage = new Bitmap(1, 2);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			var cadImportDocumentWrapper = new CADImportDocumentWrapper(mockCADHeader.Object, declaration, Factory);
			AssertEquals("TypeCode_Box1", cadImportDocumentWrapper.TypeCode_Box1);
			AssertEquals("WsSType_Box2", cadImportDocumentWrapper.WsSType_Box2);
			AssertEquals(ZDateTime.Now.AddHours(3), cadImportDocumentWrapper.AccountingDate_Box3);
			AssertEquals("AccountSecurityCode_Box4CADTransactionNo_Box4", cadImportDocumentWrapper.TransactionNoFormated_Box4);
			AssertEquals("AccountSecurityCode_Box4", cadImportDocumentWrapper.AccountSecurityCode_Box4);
			AssertEquals("CADTransactionNo_Box4", cadImportDocumentWrapper.CADTransactionNo_Box4);
			AssertEquals("OfficeNo_Box5", cadImportDocumentWrapper.OfficeNo_Box5);
			AssertEquals("ModeOfTransport_Box6", cadImportDocumentWrapper.ModeOfTransport_Box6);
			AssertEquals(ZDateTime.Now.AddHours(7), cadImportDocumentWrapper.ReleaseDate_Box7);
			AssertEquals(8m, cadImportDocumentWrapper.GrossWeightKg_Box8);
			AssertEquals("CarrierCodeAtImportation_Box9", cadImportDocumentWrapper.CarrierCodeAtImportation_Box9);
			AssertEquals("Pre_CARM_Box10", cadImportDocumentWrapper.Pre_CARM_Box10);
			AssertEquals("Z1_RPP", cadImportDocumentWrapper.Z1_RPP);
			AssertEquals("ImporterBN_Box11", cadImportDocumentWrapper.ImporterBN_Box11);
			AssertEquals("ImporterDetails_Box12", cadImportDocumentWrapper.ImporterDetails_Box12);
			AssertEquals("BrokerOrAgentBN_Box13", cadImportDocumentWrapper.BrokerOrAgentBN_Box13);
			AssertEquals("BrokerOrAgentDetails_Box14", cadImportDocumentWrapper.BrokerOrAgentDetails_Box14);
			AssertEquals("CargoControlNo_Box15", cadImportDocumentWrapper.CargoControlNo_Box15);
			AssertEquals("RecordOfIntentNo_Box16", cadImportDocumentWrapper.RecordOfIntentNo_Box16);
			AssertEquals("PreviousTransactionNo_Box17", cadImportDocumentWrapper.PreviousTransactionNo_Box17);
			AssertEquals(ZDateTime.Now.AddHours(18), cadImportDocumentWrapper.AcceptedDate_Box18);
			AssertEquals("OriginalTransactionNo_Box19", cadImportDocumentWrapper.OriginalTransactionNo_Box19);
			AssertEquals("PrevTransNoWarehouse_Box20", cadImportDocumentWrapper.PrevTransNoWarehouse_Box20);
			AssertEquals("PortOfUnlading_Box21", cadImportDocumentWrapper.PortOfUnlading_Box21);
			AssertEquals("Notes_Box35", cadImportDocumentWrapper.Notes_Box35);
			AssertEquals(113m, cadImportDocumentWrapper.TotalValueForDuty_Box113);
			AssertEquals(114m, cadImportDocumentWrapper.TotalPSTAndHST_Box114);
			AssertEquals(115m, cadImportDocumentWrapper.TotalPSTCannabisAmount_Box115);
			AssertEquals(116m, cadImportDocumentWrapper.TotalProvAlcoholTaxAmount_Box116);
			AssertEquals(117m, cadImportDocumentWrapper.TotalProvTobaccoAmount_Box117);
			AssertEquals(118m, cadImportDocumentWrapper.TotalDeclarationRelieved_Box118);
			AssertEquals(119m, cadImportDocumentWrapper.TotalAmount_Box119);
			AssertEquals(120m, cadImportDocumentWrapper.TotalCustomsDuties_Box120);
			AssertEquals(121m, cadImportDocumentWrapper.TotalExciseDuties_Box121);
			AssertEquals(122m, cadImportDocumentWrapper.TotalExciseTaxes_Box122);
			AssertEquals(123m, cadImportDocumentWrapper.TotalGST_Box123);
			AssertEquals(124m, cadImportDocumentWrapper.TotalAnti_Dumping_Box124);
			AssertEquals(125m, cadImportDocumentWrapper.TotalCountervailing_Box125);
			AssertEquals(126m, cadImportDocumentWrapper.TotalSurtaxes_Box126);
			AssertEquals(127m, cadImportDocumentWrapper.TotalSafeguards_Box127);
			AssertEquals(128m, cadImportDocumentWrapper.TotalInterest_Box128);
			AssertEquals(129m, cadImportDocumentWrapper.TotalDutiesAndTaxesWithInterest_Box129);
			AssertEquals(130m, cadImportDocumentWrapper.TotalDutiesAndTaxes_Box130);
			AssertEquals("Broker, 654", cadImportDocumentWrapper.BrokerNameAndPhone);
			AssertEquals(ZDateTime.Now, cadImportDocumentWrapper.CurrentDate);
			AssertNotNull(cadImportDocumentWrapper.BrokerSignatureImage);
			AssertEquals(1, cadImportDocumentWrapper.CADSubHeaders.Count);
		}

		[TestDate(2025, 1, 10)]
		public void TestCADSubHeaderDocumentWrapper()
		{
			var mockCADSubHeader = new Mock<ICADSubHeader>();
			mockCADSubHeader.Setup(x => x.VendorDetails_Box36).Returns("VendorDetails_Box36");
			mockCADSubHeader.Setup(x => x.PurchaserDetails_Box37).Returns("PurchaserDetails_Box37");
			mockCADSubHeader.Setup(x => x.InvoiceNo_Box38).Returns("InvoiceNo_Box38");
			mockCADSubHeader.Setup(x => x.InvoiceValue_Box39).Returns(39);
			mockCADSubHeader.Setup(x => x.InvoiceCurrencyCode_Box40).Returns("InvoiceCurrencyCode_Box40");
			mockCADSubHeader.Setup(x => x.PurchaseOrderNo_Box41).Returns("PurchaseOrderNo_Box41");
			mockCADSubHeader.Setup(x => x.FreightCharges_Box42).Returns(42);
			mockCADSubHeader.Setup(x => x.USPortOfExit_Box43).Returns("USPortOfExit_Box43");

			var mockCADLine = new Mock<ICADLine>();
			mockCADSubHeader.Setup(x => x.CADLines).Returns(new[] { mockCADLine.Object });

			var cadSubHeaderDocumentWrapper = new CADSubHeaderDocumentWrapper(mockCADSubHeader.Object, Factory);
			AssertEquals("VendorDetails_Box36", cadSubHeaderDocumentWrapper.VendorDetails_Box36);
			AssertEquals("PurchaserDetails_Box37", cadSubHeaderDocumentWrapper.PurchaserDetails_Box37);
			AssertEquals("InvoiceNo_Box38", cadSubHeaderDocumentWrapper.InvoiceNo_Box38);
			AssertEquals(39m, cadSubHeaderDocumentWrapper.InvoiceValue_Box39);
			AssertEquals("InvoiceCurrencyCode_Box40", cadSubHeaderDocumentWrapper.InvoiceCurrencyCode_Box40);
			AssertEquals("PurchaseOrderNo_Box41", cadSubHeaderDocumentWrapper.PurchaseOrderNo_Box41);
			AssertEquals(42m, cadSubHeaderDocumentWrapper.FreightCharges_Box42);
			AssertEquals("USPortOfExit_Box43", cadSubHeaderDocumentWrapper.USPortOfExit_Box43);
			AssertEquals(1, cadSubHeaderDocumentWrapper.CADLines.Count);
		}

		[TestDate(2025, 1, 10)]
		public void TestCADLineDocumentWrapper()
		{
			var mockCADLine = new Mock<ICADLine>();
			mockCADLine.Setup(x => x.CADLineNo_Box56).Returns(56);
			mockCADLine.Setup(x => x.PreviousLineNoWarehouse_Box57).Returns("PreviousLineNoWarehouse_Box57");
			mockCADLine.Setup(x => x.ClassificationNo_Box58).Returns("12341212");
			mockCADLine.Setup(x => x.ClassificationDescription_Box59).Returns("ClassificationDescription_Box59");
			mockCADLine.Setup(x => x.NarrativeDescription_Box60).Returns("NarrativeDescription_Box60");
			mockCADLine.Setup(x => x.Quantity_Box61).Returns(61);
			mockCADLine.Setup(x => x.UnitOfMeasure_Box62).Returns("UnitOfMeasure_Box62");
			mockCADLine.Setup(x => x.TimeLimitType_Box63).Returns("TimeLimitType_Box63");
			mockCADLine.Setup(x => x.ExtensionDate_Box64).Returns(ZDateTime.Now.AddHours(64));
			mockCADLine.Setup(x => x.CountryOfOrigin_Box65).Returns("CountryOfOrigin_Box65");
			mockCADLine.Setup(x => x.USState_Box66).Returns("USState_Box66");
			mockCADLine.Setup(x => x.PlaceOfExport_Box67).Returns("PlaceOfExport_Box67");
			mockCADLine.Setup(x => x.PlaceOfExportCodeState_Box68).Returns("PlaceOfExportCodeState_Box68");
			mockCADLine.Setup(x => x.DirectShipmentDate_Box69).Returns(ZDateTime.Now.AddHours(69));
			mockCADLine.Setup(x => x.TariffTreatment_Box70).Returns("TariffTreatment_Box70");
			mockCADLine.Setup(x => x.TariffCode_Box71).Returns("TariffCode_Box71");
			mockCADLine.Setup(x => x.TimeLimitFrom_Box72).Returns("TimeLimitFrom_Box72");
			mockCADLine.Setup(x => x.TimeLimitTo_Box73).Returns("TimeLimitTo_Box73");
			mockCADLine.Setup(x => x.DestinationProvince_Box74).Returns("DestinationProvince_Box74");
			mockCADLine.Setup(x => x.ValueForCurrencyConversion_Box75).Returns(75);
			mockCADLine.Setup(x => x.Currency_Box76).Returns("Currency_Box76");
			mockCADLine.Setup(x => x.ExchangeRate_Box77).Returns(77);
			mockCADLine.Setup(x => x.ValueForDuty_Box78).Returns(78);
			mockCADLine.Setup(x => x.DRPLicense_Box79).Returns("DRPLicense_Box79");
			mockCADLine.Setup(x => x.SpecialAuthOIC_Box80).Returns("SpecialAuthOIC_Box80");
			mockCADLine.Setup(x => x.SpecialAuthorityPermit_Box81).Returns("SpecialAuthorityPermit_Box81");
			mockCADLine.Setup(x => x.CustomsDuty_Box82).Returns(82);
			mockCADLine.Setup(x => x.ExciseTax_Box83).Returns(83);
			mockCADLine.Setup(x => x.ExciseDuty_Box84).Returns(84);
			mockCADLine.Setup(x => x.Surtax_Box85).Returns(85);
			mockCADLine.Setup(x => x.Anti_Dumping_Box86).Returns(86);
			mockCADLine.Setup(x => x.Safeguard_Box87).Returns(87);
			mockCADLine.Setup(x => x.Countervailing_Box88).Returns(88);
			mockCADLine.Setup(x => x.ValueForTax_Box89).Returns(89);
			mockCADLine.Setup(x => x.GST_Box90).Returns(90);
			mockCADLine.Setup(x => x.PSTAndHSTAmount_Box91).Returns(91);
			mockCADLine.Setup(x => x.ProvincialAlcoholTax_Box92).Returns(92);
			mockCADLine.Setup(x => x.ProvincialTobaccoAmount_Box93).Returns(93);
			mockCADLine.Setup(x => x.AlcohosPercent_Box94).Returns(56);
			mockCADLine.Setup(x => x.ProvincialCannabisExciseDuty_Box95).Returns(95);
			mockCADLine.Setup(x => x.CBSACaseNo_Box96).Returns("CBSACaseNo_Box96");
			mockCADLine.Setup(x => x.RulingNo_Box97).Returns("RulingNo_Box97");
			mockCADLine.Setup(x => x.AppealsCaseNo_Box98).Returns("AppealsCaseNo_Box98");
			mockCADLine.Setup(x => x.ComplianceCaseNo_Box99).Returns("ComplianceCaseNo_Box99");
			mockCADLine.Setup(x => x.LineTotalDutiesAndTaxes_Box100).Returns(100);
			mockCADLine.Setup(x => x.CommodityReason1_Box101).Returns("CommodityReason1_Box101");
			mockCADLine.Setup(x => x.Authority1_Box102).Returns("Authority1_Box102");
			mockCADLine.Setup(x => x.CommodityRemark1_Box103).Returns("CommodityRemark1_Box103");
			mockCADLine.Setup(x => x.CommodityReason2_Box105).Returns("CommodityReason2_Box105");
			mockCADLine.Setup(x => x.Authority2_Box106).Returns("Authority2_Box106");
			mockCADLine.Setup(x => x.CommodityRemark2_Box107).Returns("CommodityRemark2_Box107");
			mockCADLine.Setup(x => x.CommodityReason3_Box109).Returns("CommodityReason3_Box109");
			mockCADLine.Setup(x => x.Authority3_Box110).Returns("Authority3_Box110");
			mockCADLine.Setup(x => x.CommodityRemark3_Box111).Returns("CommodityRemark3_Box111");

			var cadLineDocumentWrapper = new CADLineDocumentWrapper(mockCADLine.Object, Factory);
			AssertEquals(new ZShort(56), cadLineDocumentWrapper.CADLineNo_Box56);
			AssertEquals("PreviousLineNoWarehouse_Box57", cadLineDocumentWrapper.PreviousLineNoWarehouse_Box57);
			AssertEquals("1234.12.12", cadLineDocumentWrapper.ClassificationNo_Box58);
			AssertEquals("ClassificationDescription_Box59", cadLineDocumentWrapper.ClassificationDescription_Box59);
			AssertEquals("NarrativeDescription_Box60", cadLineDocumentWrapper.NarrativeDescription_Box60);
			AssertEquals(61m, cadLineDocumentWrapper.Quantity_Box61);
			AssertEquals("UnitOfMeasure_Box62", cadLineDocumentWrapper.UnitOfMeasure_Box62);
			AssertEquals("TimeLimitType_Box63", cadLineDocumentWrapper.TimeLimitType_Box63);
			AssertEquals(ZDateTime.Now.AddHours(64), cadLineDocumentWrapper.ExtensionDate_Box64);
			AssertEquals("CountryOfOrigin_Box65", cadLineDocumentWrapper.CountryOfOrigin_Box65);
			AssertEquals("USState_Box66", cadLineDocumentWrapper.USState_Box66);
			AssertEquals("PlaceOfExport_Box67", cadLineDocumentWrapper.PlaceOfExport_Box67);
			AssertEquals("PlaceOfExportCodeState_Box68", cadLineDocumentWrapper.PlaceOfExportCodeState_Box68);
			AssertEquals(ZDateTime.Now.AddHours(69), cadLineDocumentWrapper.DirectShipmentDate_Box69);
			AssertEquals("TariffTreatment_Box70", cadLineDocumentWrapper.TariffTreatment_Box70);
			AssertEquals("TariffCode_Box71", cadLineDocumentWrapper.TariffCode_Box71);
			AssertEquals("TimeLimitFrom_Box72", cadLineDocumentWrapper.TimeLimitFrom_Box72);
			AssertEquals("TimeLimitTo_Box73", cadLineDocumentWrapper.TimeLimitTo_Box73);
			AssertEquals("DestinationProvince_Box74", cadLineDocumentWrapper.DestinationProvince_Box74);
			AssertEquals(75m, cadLineDocumentWrapper.ValueForCurrencyConversion_Box75);
			AssertEquals("Currency_Box76", cadLineDocumentWrapper.Currency_Box76);
			AssertEquals(77m, cadLineDocumentWrapper.ExchangeRate_Box77);
			AssertEquals(78m, cadLineDocumentWrapper.ValueForDuty_Box78);
			AssertEquals("DRPLicense_Box79", cadLineDocumentWrapper.DRPLicense_Box79);
			AssertEquals("SpecialAuthOIC_Box80", cadLineDocumentWrapper.SpecialAuthOIC_Box80);
			AssertEquals("SpecialAuthorityPermit_Box81", cadLineDocumentWrapper.SpecialAuthorityPermit_Box81);
			AssertEquals(82m, cadLineDocumentWrapper.CustomsDuty_Box82);
			AssertEquals(83m, cadLineDocumentWrapper.ExciseTax_Box83);
			AssertEquals(84m, cadLineDocumentWrapper.ExciseDuty_Box84);
			AssertEquals(85m, cadLineDocumentWrapper.Surtax_Box85);
			AssertEquals(86m, cadLineDocumentWrapper.Anti_Dumping_Box86);
			AssertEquals(87m, cadLineDocumentWrapper.Safeguard_Box87);
			AssertEquals(88m, cadLineDocumentWrapper.Countervailing_Box88);
			AssertEquals(89m, cadLineDocumentWrapper.ValueForTax_Box89);
			AssertEquals(90m, cadLineDocumentWrapper.GST_Box90);
			AssertEquals(91m, cadLineDocumentWrapper.PSTAndHSTAmount_Box91);
			AssertEquals(92m, cadLineDocumentWrapper.ProvincialAlcoholTax_Box92);
			AssertEquals(93m, cadLineDocumentWrapper.ProvincialTobaccoAmount_Box93);
			AssertEquals(56m, cadLineDocumentWrapper.AlcohosPercent_Box94);
			AssertEquals(95m, cadLineDocumentWrapper.ProvincialCannabisExciseDuty_Box95);
			AssertEquals("CBSACaseNo_Box96", cadLineDocumentWrapper.CBSACaseNo_Box96);
			AssertEquals("RulingNo_Box97", cadLineDocumentWrapper.RulingNo_Box97);
			AssertEquals("AppealsCaseNo_Box98", cadLineDocumentWrapper.AppealsCaseNo_Box98);
			AssertEquals("ComplianceCaseNo_Box99", cadLineDocumentWrapper.ComplianceCaseNo_Box99);
			AssertEquals(100m, cadLineDocumentWrapper.LineTotalDutiesAndTaxes_Box100);
			AssertEquals("CommodityReason1_Box101", cadLineDocumentWrapper.CommodityReason1_Box101);
			AssertEquals("Authority1_Box102", cadLineDocumentWrapper.Authority1_Box102);
			AssertEquals("CommodityRemark1_Box103", cadLineDocumentWrapper.CommodityRemark1_Box103);
			AssertEquals("CommodityReason2_Box105", cadLineDocumentWrapper.CommodityReason2_Box105);
			AssertEquals("Authority2_Box106", cadLineDocumentWrapper.Authority2_Box106);
			AssertEquals("CommodityRemark2_Box107", cadLineDocumentWrapper.CommodityRemark2_Box107);
			AssertEquals("CommodityReason3_Box109", cadLineDocumentWrapper.CommodityReason3_Box109);
			AssertEquals("Authority3_Box110", cadLineDocumentWrapper.Authority3_Box110);
			AssertEquals("CommodityRemark3_Box111", cadLineDocumentWrapper.CommodityRemark3_Box111);
		}

		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader.CH_JE = declaration.PK;
			Factory.Save();
			var wrapper = new CADImportDocumentWrapper(entryHeader);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("CADImportDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", declaration.PK, supporter?.SourceIdentifier);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader.CH_JE = declaration.PK;
			return new CADImportDocumentWrapper(entryHeader);
		}

		#endregion
	}
}
