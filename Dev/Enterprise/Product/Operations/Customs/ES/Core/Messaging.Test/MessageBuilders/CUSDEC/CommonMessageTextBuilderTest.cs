using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	class CommonMessageTextBuilderTest : TestCaseWithFactory
	{
		public void TestAddNewLOCSegment_NoPlaceIdentification()
		{
			var locSection = new LOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, null, null);
			AssertEquals("", locSection.ToString(characterSet));
		}

		public void TestAddNewLOCSegment_NoResponsibleAgency()
		{
			var locSection = new LOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, "ES", PlaceLocationQualifierList.CountryOfExportationDespatch, null);
			AssertEquals("LOC+35+ES'", locSection.ToString(characterSet));
		}

		public void TestAddNewLOCSegment_NoRelatedPlaceOneIdentification()
		{
			var locSection = new LOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, "0811", PlaceLocationQualifierList.PlaceOfCustomsExamination, ZString.Empty, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms);
			AssertEquals("LOC+43+0811::148'", locSection.ToString(characterSet));
		}

		public void TestAddNewLOCSegment_NoRelatedResponsibleAgencyOne()
		{
			var locSection = new LOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, "0811", PlaceLocationQualifierList.PlaceOfCustomsExamination, "BCN010", CodeListResponsibleAgencyCodedList.EsSpanishCustoms, null);
			AssertEquals("LOC+43+0811::148+BCN010'", locSection.ToString(characterSet));
		}

		public void TestAddNewLOCSegment_NoRelatedPlaceUnloading()
		{
			var locSection = new LOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, PlaceLocationQualifierList.PlaceOfCustomsExamination, ZString.Empty);
			AssertEquals("", locSection.ToString(characterSet));
		}

		public void TestAddNewDTMSegment_NoDateTime()
		{
			var dtmSection = new DTMSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewDTMSegment(dtmSection, ZString.Empty, DateTimePeriodQualifierList.ExpiryDate, DateTimePeriodFormatQualifierList.Yymmdd);
			AssertEquals("", dtmSection.ToString(characterSet));
		}

		public void TestAddNewGISSegment_IfCustomsIndicator()
		{
			var gisSection = new GISSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, true, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			AssertEquals("GIS+1:109:141'", gisSection.ToString(characterSet));
		}

		public void TestAddNewGISSegment_IfNotCustomsIndicatorAndIndicatorFalse()
		{
			var gisSection = new GISSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewGISSegment(gisSection, false, CodeListQualifierList.CustomsSpecialCodes, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
			AssertEquals("", gisSection.ToString(characterSet));
		}

		public void TestAddNewGISSegment_IfNotCustomsIndicatorAndIndicatorTrue()
		{
			CombineAssertions(() =>
			{
				var gisSection = new GISSegmentMessageSection(1);
				CommonMessageTextBuilder.AddNewGISSegment(gisSection, true, CodeListQualifierList.CustomsSpecialCodes, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1, ProcessTypeIdentificationList.GetFromString("RMT"));
				AssertEquals("With processTypeId", "GIS+1:110:141:RMT'", gisSection.ToString(characterSet));

				gisSection = new GISSegmentMessageSection(1);
				CommonMessageTextBuilder.AddNewGISSegment(gisSection, true, CodeListQualifierList.CustomsSpecialCodes, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1, null);
				AssertEquals("Without processTypeId", "GIS+1:110:141'", gisSection.ToString(characterSet));
			});
		}

		public void TestAddNewEQDSegment_NoCountryCode()
		{
			var eqdSection = new EQDSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewEQDSegment(eqdSection, EquipmentQualifierList.Container, ZString.Empty);
			AssertEquals("", eqdSection.ToString(characterSet));
		}

		public void TestAddNewSELSegment_NoSealCode()
		{
			var selSection = new SELSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewSELSegment(selSection, 1, SealingPartyCodedList.Carrier, ZString.Empty);
			AssertEquals("", selSection.ToString(characterSet));
		}

		public void TestAddNewFTXSegment_NoFunction()
		{
			var ftxSection = new FTXSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewFTXSegment(ftxSection, ZString.Empty);
			AssertEquals("", ftxSection.ToString(characterSet));
		}

		public void TestAddNewFTXSegmentTextLiteral_NoFreeText()
		{
			var ftxSection = new FTXSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewFTXSegmentTextLiteral(ftxSection, ZString.Empty);
			AssertEquals("", ftxSection.ToString(characterSet));
		}

		public void TestAddNewRFFSegment_NoSection()
		{
			var rffSegment = CommonMessageTextBuilder.AddNewRFFSegment(null, ReferenceQualifierList.QuotaNumber, "REFNUM");
			AssertNull(rffSegment);
		}

		public void TestAddNewRFFSegment_NoReferenceType()
		{
			var rffSection = new RFFSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewRFFSegment(rffSection, null, "REFNUM");
			AssertEquals("", rffSection.ToString(characterSet));
		}

		public void TestAddNewRFFSegment_NoReferenceNumber()
		{
			var rffSection = new RFFSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewRFFSegment(rffSection, ReferenceQualifierList.QuotaNumber, ZString.Empty);
			AssertEquals("", rffSection.ToString(characterSet));
		}

		public void TestAddNewRFFSegment_NoVersionNumber()
		{
			var rffSection = new RFFSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewRFFSegment(rffSection, ReferenceQualifierList.GoodsDeclarationNumber, "REFNUM", "1", ZString.Empty);
			AssertEquals("RFF+AAE:REFNUM:1'", rffSection.ToString(characterSet));
		}

		public void TestAddNewSG4Group_NoTransportMode()
		{
			var group4Section = new SegmentGroup4MessageSection(1);
			CommonMessageTextBuilder.AddNewSG4Group(group4Section, TransportStageQualifierList.AtBorder, ZString.Empty, "3", "ES", "M-1234-GD");
			AssertEquals("TDT+11++:3'TPL+:::M-1234-GD:ES'", group4Section.ToString(characterSet));
		}

		public void TestAddNewSG4Group_NoTransportId()
		{
			var group4Section = new SegmentGroup4MessageSection(1);
			CommonMessageTextBuilder.AddNewSG4Group(group4Section, TransportStageQualifierList.AtBorder, "3", ZString.Empty, "ES", ZString.Empty);
			AssertEquals("TDT+11++3'", group4Section.ToString(characterSet));
		}

		public void TestAddNewTPLSegment()
		{
			var tplSection = new TPLSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewTPLSegment(tplSection, "M-1234-GD", "ES");
			AssertEquals("TPL+:::M-1234-GD:ES'", tplSection.ToString(characterSet));
		}

		public void TestAddNewNADWithAddress_NoAddressDetails()
		{
			var nadSection = new NADSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewNADWithAddress(nadSection, null, null, null);
			AssertEquals("", nadSection.ToString(characterSet));
		}

		public void TestAddNewNADWithAddress_NoAddressType()
		{
			var exporter = BuilderHelperTest.SetUpParty("12345678A", "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");
			var nadSection = new NADSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewNADWithAddress(nadSection, null, null, exporter);
			AssertEquals("", nadSection.ToString(characterSet));
		}

		public void TestAddNewNADWithAddress_SetNADAddressDetails()
		{
			var exporter = BuilderHelperTest.SetUpParty("12345678A", "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");
			var nadSegment = new NADSegment();
			CommonMessageTextBuilder.SetNADAddressDetails(nadSegment, exporter);
			AssertEquals("NAD++++NORON EHF+SKUTUVOGUR 7+104 REYKJAVIK++40025+IS'", nadSegment.ToString(characterSet));
		}

		public void TestAddNewNADWithEmailInSG6Group_NoId()
		{
			var exporter = BuilderHelperTest.SetUpParty(ZString.Empty, "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");
			var nadSection = new NADSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewNADWithAddress(nadSection, PartyQualifierList.Exporter, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, exporter);

			AssertEquals("NAD+EX+++NORON EHF+SKUTUVOGUR 7+104 REYKJAVIK++40025+IS'", nadSection.ToString(characterSet));
		}

		public void TestAddNewNADWithEmailInSG6Group_NoAgencyCodedList()
		{
			var exporter = BuilderHelperTest.SetUpParty("12345678A", "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");
			var nadSection = new NADSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewNADWithAddress(nadSection, PartyQualifierList.Exporter, null, exporter);

			AssertEquals("NAD+EX+12345678A++NORON EHF+SKUTUVOGUR 7+104 REYKJAVIK++40025+IS'", nadSection.ToString(characterSet));
		}

		public void TestAddNewNADWithEmail_NoAddressDetails()
		{
			var nadSection = new NADSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewNADWithEmail(nadSection, null, null);
			AssertEquals("", nadSection.ToString(characterSet));
		}

		public void TestAddNewNADWithEmail_NoAgencyCodedList()
		{
			var mockDeclarant = BuilderHelperTest.SetUpPartyEmail("1210244B", "GUTIERREZ S.A.", "MIDIRECCION@MIXMAIL.COM");

			var nadSection = new NADSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewNADWithEmail(nadSection, null, mockDeclarant);

			AssertEquals("NAD+DT+1210244B+MIDIRECCION@MIXMAIL.COM+GUTIERREZ S.A.'", nadSection.ToString(characterSet));
		}

		public void TestAddNewMOAInSG8Group_NoQualifier()
		{
			var group8Section = new SegmentGroup8MessageSection(1);
			CommonMessageTextBuilder.AddNewMOAInSG8Group(group8Section, null, 0, "EUR");
			AssertEquals("", group8Section.ToString(characterSet));
		}

		public void TestAddNewMOAInSG8Group_NoCurrencyCode()
		{
			var group8Section = new SegmentGroup8MessageSection(1);
			CommonMessageTextBuilder.AddNewMOAInSG8Group(group8Section, MonetaryAmountTypeQualifierList.MutuallyDefined, 0, ZString.Empty);
			AssertEquals("", group8Section.ToString(characterSet));
		}

		public void TestAddNewMOAInSG8Group_NoTotalAmount()
		{
			var group8Section = new SegmentGroup8MessageSection(1);
			CommonMessageTextBuilder.AddNewMOAInSG8Group(group8Section, MonetaryAmountTypeQualifierList.MutuallyDefined, 0, "EUR");
			AssertEquals("MOA+ZZZ::EUR'", group8Section.ToString(characterSet));
		}

		public void TestAddFTXSegmentForGoodsDescription_NoTextType()
		{
			var group30 = new SegmentGroup30();
			CommonMessageTextBuilder.AddFTXSegmentForGoodsDescription(group30, null, "DESCRIPTION TEXT", 5);
			AssertEquals("", group30.ToString(characterSet));
		}

		public void TestAddFTXSegmentForGoodsDescription_NoTextValue()
		{
			var group30 = new SegmentGroup30();
			CommonMessageTextBuilder.AddFTXSegmentForGoodsDescription(group30, TextSubjectQualifierList.MutuallyDefined, ZString.Empty, 5);
			AssertEquals("", group30.ToString(characterSet));
		}

		public void TestAddNewMEASegment_NoUnit()
		{
			var group30 = new SegmentGroup30();
			CommonMessageTextBuilder.AddNewMEASegment(group30, 300, ZString.Empty, MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.GrossWeight);
			AssertEquals("", group30.ToString(characterSet));
		}

		public void TestAddNewMEASegment_NoValue()
		{
			var group30 = new SegmentGroup30();
			CommonMessageTextBuilder.AddNewMEASegment(group30, 0, "KGM", MeasurementApplicationQualifierList.Measurement, 3, MeasurementDimensionCodedList.GrossWeight);
			AssertEquals("", group30.ToString(characterSet));
		}

		public void TestAddNewMEASegment_NoDimension()
		{
			var group30 = new SegmentGroup30();
			CommonMessageTextBuilder.AddNewMEASegment(group30, 300.1234, "KGM", MeasurementApplicationQualifierList.Measurement, 3);
			AssertEquals("MEA+AAE++KGM:300,123'", group30.ToString(characterSet));
		}

		public void TestAddNewTDTSegmentOnSG30_NoCode()
		{
			var group30 = new SegmentGroup30();
			CommonMessageTextBuilder.AddNewTDTSegmentOnSG30(group30, ZString.Empty, null);
			AssertEquals("", group30.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoExternalPackages()
		{
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, externalPackages: null, PackagingLevelCodedList.Outer);
			AssertEquals("", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoNumberOfPackages()
		{
			var packages = BuilderHelperTest.SetUpExternalPackages(0, new ZString[] { "TCKU2126154" });
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, packages, PackagingLevelCodedList.Outer);
			AssertEquals("", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoTags()
		{
			var packages = BuilderHelperTest.SetUpExternalPackages(3, Enumerable.Empty<ZString>());
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, packages, PackagingLevelCodedList.Outer);
			AssertEquals("PAC+3+3+CT'", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_SetPACSegmentForExternal()
		{
			var packages = BuilderHelperTest.SetUpExternalPackages(3, new ZString[] { "TCKU2126154" });
			var group31Section = new SegmentGroup31MessageSection(1);
			var sg31 = CommonMessageTextBuilder.SetPACSegmentForExternal(group31Section, packages, PackagingLevelCodedList.Outer);
			AssertEquals("PAC+3+3+CT'", sg31.ToString(characterSet));
		}

		public void TestAddNewSG31Group_ManyExternalPackages()
		{
			var numberedStrings = new List<ZString>();
			for (int i = 0; i < 100; i++)
			{
				numberedStrings.Add("string" + (i + 1));
			}

			var packages = BuilderHelperTest.SetUpExternalPackages(100, numberedStrings.ToArray());
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, packages, PackagingLevelCodedList.Outer);
			AssertEquals(@"PAC+100+3+CT'PCI++string1:string2:string3:string4:string5:string6:string7:string8:string9:string10'PCI++string11:string12:string13:string14:string15:string16:string17:string18:string19:string20'PCI++string21:string22:string23:string24:string25:string26:string27:string28:string29:string30'PCI++string31:string32:string33:string34:string35:string36:string37:string38:string39:string40'PCI++string41:string42:string43:string44:string45:string46:string47:string48:string49:string50'PCI++string51:string52:string53:string54:string55:string56:string57:string58:string59:string60'PCI++string61:string62:string63:string64:string65:string66:string67:string68:string69:string70'PCI++string71:string72:string73:string74:string75:string76:string77:string78:string79:string80'PCI++string81:string82:string83:string84:string85:string86:string87:string88:string89:string90'PCI++string91:string92:string93:string94:string95:string96:string97:string98:string99:string100'", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoInternalPackages()
		{
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, internalPackages: null, PackagingLevelCodedList.Inner);
			AssertEquals("", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoPackagesInInternalPackages()
		{
			var mockPackages = new Mock<IInternalPackagesInfoCommon>();
			mockPackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IInternalPackageIdentificationCommon>)Enumerable.Empty<IInternalPackageIdentificationCommon>());

			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, mockPackages.Object, PackagingLevelCodedList.Inner);
			AssertEquals("", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_SetPACSegmentAndAddNewSG32Group()
		{
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.SetPACSegmentAndAddNewSG32GroupForInternal(group31Section, PackagingLevelCodedList.Inner);
			AssertEquals("PAC++1'", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_SetSingleInternalPackage()
		{
			var mockPackages = new Mock<IInternalPackagesInfoCommon>();
			var internalPackage = BuilderHelperTest.SetUpInternalPackages();
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage });

			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, mockPackages.Object, PackagingLevelCodedList.Inner);
			AssertEquals("PAC+50+1+BX'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T'", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_SetMultipleInternalPackages()
		{
			var mockPackages = new Mock<IInternalPackagesInfoCommon>();
			var internalPackage = BuilderHelperTest.SetUpInternalPackages();
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage, internalPackage });

			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, mockPackages.Object, PackagingLevelCodedList.Inner);
			AssertEquals("PAC++1'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoVehiclePackages()
		{
			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, vehiclePackages: null, PackagingLevelCodedList.GetFromString("4"));
			AssertEquals("", group31Section.ToString(characterSet));
		}

		public void TestAddNewSG31Group_NoPackagesInVehiclePackages()
		{
			var mockVehiclePackages = new Mock<IVehiclePackagesInfoCommon>();
			mockVehiclePackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IVehicleCommon>)Enumerable.Empty<IVehicleCommon>());

			var group31Section = new SegmentGroup31MessageSection(1);
			CommonMessageTextBuilder.AddNewSG31Group(group31Section, mockVehiclePackages.Object, PackagingLevelCodedList.GetFromString("4"));
			AssertEquals("", group31Section.ToString(characterSet));
		}

		public void TestAddNewMOASegmentSG33Group_NoQualifier()
		{
			var group33Section = new SegmentGroup33MessageSection(1);
			CommonMessageTextBuilder.AddNewMOASegmentSG33Group(group33Section, null, 0);
			AssertEquals("", group33Section.ToString(characterSet));
		}

		public void TestAddNewMOASegmentSG33Group_NoTotalAmount()
		{
			var group33Section = new SegmentGroup33MessageSection(1);
			CommonMessageTextBuilder.AddNewMOASegmentSG33Group(group33Section, MonetaryAmountTypeQualifierList.MutuallyDefined, 0);
			AssertEquals("", group33Section.ToString(characterSet));
		}

		public void TestAddNewDOCSegment_NoDocName()
		{
			var docSection = new DOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewDOCSegment(docSection, ZString.Empty, "ES3600000002", "KN00000000000100", "AA");
			AssertEquals("DOC+AA+ES3600000002::KN00000000000100'", docSection.ToString(characterSet));
		}

		public void TestAddNewDOCSegment_NoDocSource()
		{
			var docSection = new DOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewDOCSegment(docSection, "X001", "ES3600000002", ZString.Empty, "AA");
			AssertEquals("DOC+AA:::X001+ES3600000002'", docSection.ToString(characterSet));
		}

		public void TestAddNewDOCSegment_NoDocNameCoded()
		{
			var docSection = new DOCSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewDOCSegment(docSection, "X001", "ES3600000002", "KN00000000000100", ZString.Empty);
			AssertEquals("DOC+:::X001+ES3600000002::KN00000000000100'", docSection.ToString(characterSet));
		}

		public void TestAddNewCNTSegment_NoControlValue()
		{
			var cntSection = new CNTSegmentMessageSection(1);
			CommonMessageTextBuilder.AddNewCNTSegment(cntSection, ZString.Empty, null);
			AssertEquals("", cntSection.ToString(characterSet));
		}

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new UNOAESCharacterSet();
		}
		UNOAESCharacterSet characterSet;
	}
}
