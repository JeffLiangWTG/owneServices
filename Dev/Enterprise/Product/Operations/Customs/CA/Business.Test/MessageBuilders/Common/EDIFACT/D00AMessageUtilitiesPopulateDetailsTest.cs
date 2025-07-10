using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class D00AMessageUtilitiesPopulateDetailsTest : TestCaseWithFactory
	{
		public void TestPopulateUNH()
		{
			var uNH = new UNHSegment();
			D00AMessageUtilities.PopulateUNH(uNH, "123456", "GSMCAR", "D", "00A", "UN", "SUPRPT");
			AssertEquals("UNH", "UNH+123456+GSMCAR:D:00A:UN:SUPRPT'", uNH.ToString(new CACharSet()));
		}

		public void TestPopulateBGM()
		{
			var bGM = new BGMSegment();
			D00AMessageUtilities.PopulateBGM(bGM, DocumentNameCodeList.CustomsManifest, "", "ABCD1234", MessageFunctionCodeList.Original);
			AssertEquals("BGM", "BGM+85+ABCD1234+9'", bGM.ToString(new CACharSet()));
		}

		public void TestPopulateUNS()
		{
			var uNS = new UNSSegment();
			D00AMessageUtilities.PopulateUNS(uNS, "D");
			AssertEquals("UNS", "UNS+D'", uNS.ToString(new CACharSet()));
		}

		public void TestPopulateCST()
		{
			var cST = new CSTSegment();
			D00AMessageUtilities.PopulateCST(cST, "687", CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);
			AssertEquals("CST", "CST++687::96'", cST.ToString(new CACharSet()));
		}

		public void TestPopulateTDT1()
		{
			var tDT = new TDTSegment();
			D00AMessageUtilities.PopulateTDT(tDT, TransportStageCodeQualifierList.MainCarriageTransport, "1", "8080");
			AssertEquals("TDT", "TDT+20++1++8080'", tDT.ToString(new CACharSet()));
		}

		public void TestPopulateTDT2()
		{
			var tDT = new TDTSegment();
			D00AMessageUtilities.PopulateTDT(tDT, TransportStageCodeQualifierList.AtBorder, "7", "9991", "NAME", "SEAPRINCESS");
			AssertEquals("TDT", "TDT+11++7++9991::96+++:::SEAPRINCESS'", tDT.ToString(new CACharSet()));
		}

		public void TestPopulateTDT3()
		{
			var tDT = new TDTSegment();
			D00AMessageUtilities.PopulateTDT(tDT, TransportStageCodeQualifierList.AtBorder, "3", "", "CARRIER NAME", "");
			AssertEquals("TDT", "TDT+11++3++:::CARRIER NAME'", tDT.ToString(new CACharSet()));
		}

		public void TestPopulateDOC1()
		{
			var dOC = new DOCSegment();
			D00AMessageUtilities.PopulateDOC(dOC, MessageConstants.ModeOfTransportMessageCodes.Air, "CCN");
			AssertEquals("DOC", "DOC+741+CCN'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateDOC2()
		{
			var dOC = new DOCSegment();
			D00AMessageUtilities.PopulateDOC(dOC, MessageConstants.ModeOfTransportMessageCodes.Rail, "CCN");
			AssertEquals("DOC", "DOC+720+CCN'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateDOC3()
		{
			var dOC = new DOCSegment();
			D00AMessageUtilities.PopulateDOC(dOC, MessageConstants.ModeOfTransportMessageCodes.Highway, "CCN");
			AssertEquals("DOC", "DOC+730+CCN'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateDOC4()
		{
			var dOC = new DOCSegment();
			D00AMessageUtilities.PopulateDOC(dOC, MessageConstants.ModeOfTransportMessageCodes.Marine, "CCN");
			AssertEquals("DOC", "DOC+704+CCN'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateDOC5()
		{
			var dOC = new DOCSegment();
			D00AMessageUtilities.PopulateDOC(dOC, DocumentNameCodeList.UniversalMultipurposeTransportDocument, "UCR");
			AssertEquals("DOC", "DOC+701+UCR'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateRFF()
		{
			var rFF = new RFFSegment();
			D00AMessageUtilities.PopulateRFF(rFF, ReferenceFunctionCodeQualifierList.DeclarantsReferenceNumber, "SRN");
			AssertEquals("RFF", "RFF+ABE:SRN'", rFF.ToString(new CACharSet()));
		}

		public void TestPopulateLOC1()
		{
			var lOC = new LOCSegment();
			D00AMessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.CustomsOfficeOfExit, "0009", CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);
			AssertEquals("LOC", "LOC+42+0009::96'", lOC.ToString(new CACharSet()));
		}

		public void TestPopulateLOC2()
		{
			var lOC = new LOCSegment();
			D00AMessageUtilities.PopulateLOC(lOC, LocationFunctionCodeQualifierList.PlaceOfDestination, "CA", "MONTREAL", "OLD PORT");
			AssertEquals("LOC", "LOC+8+CA:::MONTREAL+OLD PORT'", lOC.ToString(new CACharSet()));
		}

		public void TestPopulateGEI1()
		{
			var gEI = new GEISegment();
			D00AMessageUtilities.PopulateGEI(gEI, MessageConstants.CustomsProcedureCodes.FROB);
			AssertEquals("GEI", "GEI+6+:::26'", gEI.ToString(new CACharSet()));
		}

		public void TestPopulateGEI2()
		{
			var gEI = new GEISegment();
			D00AMessageUtilities.PopulateGEI(gEI, "3", "28");
			AssertEquals("GEI", "GEI+3+28'", gEI.ToString(new CACharSet()));
		}

		public void TestPopulateFTX()
		{
			var fTX = new FTXSegment();
			D00AMessageUtilities.PopulateFTX(fTX, TextSubjectCodeQualifierList.SpecialInstructions, "SPECIAL INSTRUCTIONS");
			AssertEquals("GEI", "FTX+SIN+++SPECIAL INSTRUCTIONS'", fTX.ToString(new CACharSet()));
		}

		public void TestPopulateEQDContainer1()
		{
			var eQD = new EQDSegment();
			D00AMessageUtilities.PopulateEQDContainer(eQD, "ABCD1234567XX", "DE", "4LG1", false);
			AssertEquals("EQD", "EQD+CN+ABCD1234567DE4LG1::5++++5'", eQD.ToString(new CACharSet()));
		}

		public void TestPopulateEQDContainer2()
		{
			var eQD = new EQDSegment();
			D00AMessageUtilities.PopulateEQDContainer(eQD, "ABCD1234", "DE", "4LG1", true);
			AssertEquals("EQD", "EQD+CN+ABCD1234   DE4LG1::5++++4'", eQD.ToString(new CACharSet()));
		}

		public void TestPopulateEQDContainer3()
		{
			var eQD = new EQDSegment();
			D00AMessageUtilities.PopulateEQDContainer(eQD, "ABCD1234567", "", "", false);
			AssertEquals("EQD", "EQD+CN+ABCD1234567++++5'", eQD.ToString(new CACharSet()));
		}

		public void TestPopulateEQDContainer4()
		{
			var eQD = new EQDSegment();
			D00AMessageUtilities.PopulateEQDContainer(eQD, "ABCD1234567", string.Empty, string.Empty);
			AssertEquals("EQD", "EQD+CN+ABCD1234567'", eQD.ToString(new CACharSet()));
		}

		public void TestPopulateEQDContainer5()
		{
			var eQD = new EQDSegment();
			D00AMessageUtilities.PopulateEQDContainer(eQD, "ABCD1234567", "AU", "20GP");
			AssertEquals("EQD", "EQD+CN+ABCD1234567AU20GP::5'", eQD.ToString(new CACharSet()));
		}

		public void TestPopulatePAC()
		{
			var pAC = new PACSegment();
			D00AMessageUtilities.PopulatePAC(pAC, 200, "BOX");
			AssertEquals("PAC", "PAC+200++BOX'", pAC.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeight1()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAWeight(mEA, 12345678.1234m, "KG");
			AssertEquals("MEA", "MEA+WT+AAE+KGM:12345678.1234'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeight2()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAWeight(mEA, 1m, Constants.Weight.LongTons);
			AssertEquals("MEA", "MEA+WT+AAE+KGM:1016.0469'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeight3()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAWeight(mEA, 1m, "XX");
			AssertEquals("MEA", "MEA+WT+AAE+XX:1'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeightInKG()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAWeightInKG(mEA, 1000.5m, Constants.Weight.Pounds);
			AssertEquals("MEA", "MEA+WT+AAD+KGM:454'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 200.075m, Constants.Volume.CubicFeet);
			AssertEquals("MEA", "MEA+VOL+:::E+WSD:200.075'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume2()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 1m, Constants.Volume.CubicYards);
			AssertEquals("MEA", "MEA+VOL+:::X+WSD:0.7646'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume3()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 200.075m, MessageConstants.ACIVolumeUnits.GallonsUK);
			AssertEquals("MEA", "MEA+VOL+:::G+WSD:200.075'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume4()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 200.075m, "AA");
			AssertEquals("MEA", "MEA+VOL+:::AA+WSD:200.075'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume5()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 200.075m, Constants.Volume.Litre);
			AssertEquals("MEA", "MEA+VOL+:::V+WSD:200.075'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume6()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 1m, Constants.Volume.MegaLitre);
			AssertEquals("MEA", "MEA+VOL+:::X+WSD:1000'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAVolume7()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEAVolume(mEA, 2m, MessageConstants.ACIVolumeUnits.LoadForEnterprise);
			AssertEquals("MEA", "MEA+VOL+:::L+WSD:2'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEA()
		{
			var mEA = new MEASegment();
			D00AMessageUtilities.PopulateMEA(mEA, MeasurementAttributeCodeList._1stSpecifiedTariffQuantity, 123.45678m, "DZN");
			AssertEquals("MEA", "MEA+AAR++DZN:123.4568'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateSGP()
		{
			var sGP = new SGPSegment();
			D00AMessageUtilities.PopulateSGP(sGP, "IJKL4065400");
			AssertEquals("SGP", "SGP+IJKL4065400'", sGP.ToString(new CACharSet()));
		}

		public void TestPopulateDGS()
		{
			var dGS = new DGSSegment();
			D00AMessageUtilities.PopulateDGS(dGS, "1234");
			AssertEquals("DGS", "DGS+++1234'", dGS.ToString(new CACharSet()));
		}

		public void TestPopulatePCI()
		{
			var pCI = new PCISegment();
			var marks = new List<ZString>();
			marks.Add("1");
			D00AMessageUtilities.PopulatePCI(pCI, marks);
			AssertEquals("PCI", "PCI++1'", pCI.ToString(new CACharSet()));
			marks.Add("2");
			marks.Add("3");
			marks.Add("4");
			marks.Add("5");
			marks.Add("6");
			marks.Add("7");
			marks.Add("8");
			marks.Add("9");
			D00AMessageUtilities.PopulatePCI(pCI, marks);
			AssertEquals("PCI", "PCI++1:2:3:4:5:6:7:8:9'", pCI.ToString(new CACharSet()));
		}

		public void TestPopulateCSTList()
		{
			var cST = new CSTSegment();
			var tariffs = new List<ZString>();
			tariffs.Add("1");
			D00AMessageUtilities.PopulateCSTList(cST, tariffs);
			AssertEquals("CST", "CST++1'", cST.ToString(new CACharSet()));
			tariffs.Add("2");
			tariffs.Add("3");
			tariffs.Add("4");
			tariffs.Add("5");
			D00AMessageUtilities.PopulateCSTList(cST, tariffs);
			AssertEquals("CST", "CST++1+2+3+4+5'", cST.ToString(new CACharSet()));
		}

		public void TestPopulateAUT()
		{
			var aUT = new AUTSegment();
			D00AMessageUtilities.PopulateAUT(aUT, "123456789");
			AssertEquals("AUT", "AUT+123456789'", aUT.ToString(new CACharSet()));
		}

		public void TestPopulateDTM203()
		{
			var dTM = new DTMSegment();
			D00AMessageUtilities.PopulateDTM203(dTM, DateOrTimeOrPeriodFunctionCodeQualifierList.ExportationDate, new ZDateTime(2008, 12, 31, 23, 59, 0));
			AssertEquals("DTM", "DTM+129:200812312359:203'", dTM.ToString(new CACharSet()));
		}

		public void TestPopulateNAD()
		{
			var nAD = new NADSegment();
			D00AMessageUtilities.PopulateNAD(nAD, PartyFunctionCodeQualifierList.AgentRepresentative, "21311", CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise);
			AssertEquals("NAD", "NAD+AG+21311::96'", nAD.ToString(new CACharSet()));
		}

		public void TestPopulateMOA1()
		{
			var mOA = new MOASegment();
			D00AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount, 1234.5m, "USD", 2);
			AssertEquals("MOA", "MOA+39:1234.50:USD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateMOA2()
		{
			var mOA = new MOASegment();
			D00AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeCodeQualifierList.InvoiceTotalAmount, 1234.5m, "CAD", 0);
			AssertEquals("MOA", "MOA+39:1235:CAD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateUNT()
		{
			var uNT = new UNTSegment();
			D00AMessageUtilities.PopulateUNT(uNT, "10", "20");
			AssertEquals("UNT", "UNT+10+20'", uNT.ToString(new CACharSet()));
		}
	}
}
