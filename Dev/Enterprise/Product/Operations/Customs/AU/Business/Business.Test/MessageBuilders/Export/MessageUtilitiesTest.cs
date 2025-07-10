using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageUtilitiesTest : TransactionedTestCase
	{
		public void TestPopulatePAC()
		{
			var pac = new PACSegment();
			MessageUtilities.PopulatePAC(pac, "code", CodeListIdentificationCodeList.GeographicLocation, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			AssertEquals("PAC", "PAC+++CODE:229:6'", pac.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulatePAC2()
		{
			var pac = new PACSegment();
			MessageUtilities.PopulatePAC(pac, 10, "code", CodeListIdentificationCodeList.GeographicLocation, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			AssertEquals("PAC", "PAC+10++CODE:229:6'", pac.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateRFF()
		{
			var rff = new RFFSegment();
			MessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.ExportPermitNumber, "ident", null);
			AssertEquals("RFF", "RFF+EP:IDENT'", rff.ToString(new UNOCCMRCharacterSet()));
			rff = new RFFSegment();
			MessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.ExportPermitNumber, "ident", "linenumber");
			AssertEquals("RFF", "RFF+EP:IDENT:LINENUMBER'", rff.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateUNH()
		{
			var unh = new UNHSegment();
			MessageUtilities.PopulateUNH(unh, "messagenumcode", MessageTypeList.CustomsCargoReportMessage, MessageVersionNumberList.DraftVersionUnEdifactDirectory, MessageReleaseNumberList.Release1999B, ControllingAgencyList.UnCefact);
			AssertEquals("UNH", "UNH+MESSAGENUMCODE+CUSCAR:D:99B:UN'", unh.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateBGM()
		{
			var bgm = new BGMSegment();
			MessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.CargoDeclarationArrival, "EXD", "num", "ver", MessageFunctionCodeList.Original);
			AssertEquals("BGM", "BGM+933:::EXD+NUM:VER+9'", bgm.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateLOC()
		{
			var loc = new LOCSegment();
			MessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.CountryOfOrigin, "code", CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			AssertEquals("LOC", "LOC+27+CODE::6'", loc.ToString(new UNOCCMRCharacterSet()));
			loc = new LOCSegment();
			MessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.CountryOfOrigin, "code", CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope, "related", CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			AssertEquals("LOC", "LOC+27+CODE::6+RELATED::95'", loc.ToString(new UNOCCMRCharacterSet()));
			loc = new LOCSegment();
			MessageUtilities.PopulateLOC(loc, LocationFunctionCodeQualifierList.CountryOfOrigin, "", CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope, "related", CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			AssertEquals("LOC", "LOC+27++RELATED::95'", loc.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateDTM()
		{
			var dtm = new DTMSegment();
			MessageUtilities.PopulateDTM(dtm, DateTimePeriodFunctionCodeQualifierList.ArrivalDateTimeEstimated, "111111", DateTimePeriodFormatCodeList.Ccyymmdd);
			AssertEquals("DTM", "DTM+132:111111:102'", dtm.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateGIS()
		{
			var gis = new GISSegment();
			MessageUtilities.PopulateGIS(gis, AUCProcessingIndicatorDescriptionCodeList.NonConfirmingDeclaration, CodeListIdentificationCodeList.GeographicLocation, CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope);
			AssertEquals("GIS", "GIS+N:229:6'", gis.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateCST()
		{
			var cst = new CSTSegment();
			MessageUtilities.PopulateCST(cst, "190", "action");
			AssertEquals("CST", "CST+190+ACTION::95'", cst.ToString(new UNOCCMRCharacterSet()));

			cst = new CSTSegment();
			MessageUtilities.PopulateCST(cst, "10", CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			AssertEquals("CST", "CST++10::95'", cst.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateFTX()
		{
			var ftx = new FTXSegment();
			MessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.GoodsDescription, "text");
			AssertEquals("FTX", "FTX+AAA+++TEXT'", ftx.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateMEA()
		{
			var mea = new MEASegment();
			MessageUtilities.PopulateMEA(mea, MeasurementAttributeCodeList.UnitOfMeasureUsedForInvoicedQuantities, MeasuredAttributeCodeList.AcidityOfJuice, "unit", "1000");
			AssertEquals("MEA", "MEA+ABW+AEV+UNIT:1000'", mea.ToString(new UNOCCMRCharacterSet()));
			MessageUtilities.PopulateMEA(mea, MeasurementAttributeCodeList.UnitOfMeasureUsedForInvoicedQuantities, null, "unit", "1000");
			AssertEquals("MEA", "MEA+ABW++UNIT:1000'", mea.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateEQD()
		{
			var eqd = new EQDSegment();
			MessageUtilities.PopulateEQD(eqd, EquipmentTypeCodeQualifierList.NoSpecialEquipmentNeeded, "1234567890", CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			AssertEquals("EQD", "EQD+AH+1234567890::95'", eqd.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateMOA()
		{
			var moa = new MOASegment();
			MessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.FobValue, "1000", "AUD");
			AssertEquals("MOA", "MOA+63:1000:AUD'", moa.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateNAD()
		{
			var nad = new NADSegment();
			MessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Consignee, null, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, "name", "city");
			AssertEquals("NAD", "NAD+CN+++NAME++CITY'", nad.ToString(new UNOCCMRCharacterSet()));
			nad = new NADSegment();
			MessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Consignee, "id", CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			AssertEquals("NAD", "NAD+CN+ID::95'", nad.ToString(new UNOCCMRCharacterSet()));

			nad = new NADSegment();
			MessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.AuthorizedImporter, "1234567890", CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
			AssertEquals("NAD", "NAD+AT+1234567890::95'", nad.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateTDT()
		{
			var tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.InlandTransport, "10", TransportMeansDescriptionCodeList.Aircraft, null, null, null);
			AssertEquals("TDT", "TDT+1+10++6'", tdt.ToString(new UNOCCMRCharacterSet()));
			tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.InlandTransport, "10", TransportMeansDescriptionCodeList.Aircraft, null, null, "id");
			AssertEquals("TDT", "TDT+1+10++6++++ID::11'", tdt.ToString(new UNOCCMRCharacterSet()));
			tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.InlandTransport, "10", TransportMeansDescriptionCodeList.Aircraft, "id", CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, null);
			AssertEquals("TDT", "TDT+1+10++6+ID::3'", tdt.ToString(new UNOCCMRCharacterSet()));

			tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.MainCarriageTransport, "123456", "S(SEA)", "", null, "12345678", CodeListResponsibleAgencyCodeList.LloydsRegisterOfShipping);
			AssertEquals("TDT", "TDT+20+123456+S(SEA)+++++12345678::11'", tdt.ToString(new UNOCCMRCharacterSet()));
			tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.MainCarriageTransport, "", "A(AIR)", "123546", CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, "", null);
			AssertEquals("TDT", "TDT+20++A(AIR)++123546::3'", tdt.ToString(new UNOCCMRCharacterSet()));
			tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.MainCarriageTransport, "", "O(OTHER)", "", null, "", null);
			AssertEquals("TDT", "TDT+20++O(OTHER)'", tdt.ToString(new UNOCCMRCharacterSet()));
			tdt = new TDTSegment();
			MessageUtilities.PopulateTDT(tdt, TransportStageCodeQualifierList.MainCarriageTransport, "", "P(POST)", "", null, "", null);
			AssertEquals("TDT", "TDT+20++P(POST)'", tdt.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateCNT()
		{
			var cnt = new CNTSegment();
			MessageUtilities.PopulateCNT(cnt, ControlTotalTypeCodeQualifierList.TotalNumberOfPackages, "30");
			AssertEquals("CNT", "CNT+11:30'", cnt.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateUNS()
		{
			var uns = new UNSSegment();
			MessageUtilities.PopulateUNS(uns, SectionIdentificationList.HeaderDetailSectionSeparation);
			AssertEquals("UNS", "UNS+D'", uns.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateUNT()
		{
			var unt = new UNTSegment();
			MessageUtilities.PopulateUNT(unt, "10", "20");
			AssertEquals("UNT", "UNT+10+20'", unt.ToString(new UNOCCMRCharacterSet()));
		}

		public void TestPopulateDMS()
		{
			var docNumber = "1";
			var dms = new DMSSegment();
			MessageUtilities.PopulateDMS(dms, docNumber);
			AssertEquals("DMS", "DMS+1'", dms.ToString(new UNOCCMRCharacterSet()));
		}
	}
}
