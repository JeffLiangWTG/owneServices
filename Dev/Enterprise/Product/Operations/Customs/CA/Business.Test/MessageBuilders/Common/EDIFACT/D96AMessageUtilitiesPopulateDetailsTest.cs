using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Edifact.D96A.Segments;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class D96AMessageUtilitiesPopulateDetailsTest : TestCaseWithFactory
	{
		public void TestPopulateUNH()
		{
			var uNH = new UNHSegment();
			D96AMessageUtilities.PopulateUNH(uNH, "123456", Enterprise.Edifact.D96A.Elements.MessageTypeList.CustomsDeclarationMessage, "D", "96A", ControllingAgencyList.UnEceTradeWp4UnitedNationsStandardMessagesUnsm);
			AssertEquals("UNH", "UNH+123456+CUSDEC:D:96A:UN'", uNH.ToString(new CACharSet()));
		}

		public void TestPopulateBGM()
		{
			var bGM = new BGMSegment();
			D96AMessageUtilities.PopulateBGM(bGM, "DOCNAME", "ABCD1234", MessageFunctionCodedList.Original, DocumentMessageNameCodedList.ForwardersWarehouseReceipt);
			AssertEquals("BGM", "BGM+631:::DOCNAME+ABCD1234+9'", bGM.ToString(new CACharSet()));
			bGM = new BGMSegment();
			D96AMessageUtilities.PopulateBGM(bGM, "", "ABCD1234", null, null);
			AssertEquals("BGM", "BGM++ABCD1234'", bGM.ToString(new CACharSet()));
		}

		public void TestPopulateCST()
		{
			var cST = new CSTSegment();
			D96AMessageUtilities.PopulateCST(cST, "257", CodeListQualifierList.CustomsDeclarationType, "1", CodeListQualifierList.CustomsProcedure, "987654321RM001", CodeListQualifierList.BusinessAccountNumber,
				"FIRST", CodeListQualifierList.PartyIdentification, "1", CodeListQualifierList.CustomsSpecialCodes);
			AssertEquals("CST", "CST++257:105+1:117+987654321RM001:58+FIRST:160+1:110'", cST.ToString(new CACharSet()));
		}

		public void TestPopulateLOC1()
		{
			var lOC = new LOCSegment();
			D96AMessageUtilities.PopulateLOC(lOC, PlaceLocationQualifierList.CustomsOfficeOfClearance, "0497", null, string.Empty, "14", "BONDED WAREHOUSE");
			AssertEquals("LOC without Sub-Location Code", "LOC+22+0497+14:::BONDED WAREHOUSE'", lOC.ToString(new CACharSet()));

			D96AMessageUtilities.PopulateLOC(lOC, PlaceLocationQualifierList.CustomsOfficeOfClearance, "0497", CodeListQualifierList.CustomsWarehouse, "3252", "14", "BONDED WAREHOUSE");
			AssertEquals("LOC with Sub-Location Code", "LOC+22+0497:129::3252+14:::BONDED WAREHOUSE'", lOC.ToString(new CACharSet()));
		}

		public void TestPopulateLOC2()
		{
			var lOC = new LOCSegment();
			D96AMessageUtilities.PopulateLOC(lOC, "VAR", "UNY", "MX");
			AssertEquals("LOC", "LOC+27+VAR+UNY+MX'", lOC.ToString(new CACharSet()));
		}

		public void TestPopulateDTM203()
		{
			var dTM = new DTMSegment();
			D96AMessageUtilities.PopulateDTM203(dTM, DateTimePeriodQualifierList.DepartureDateTimeEstimated, new ZDateTime(2008, 1, 21, 23, 35, 1));
			AssertEquals("DTM", "DTM+133:200801212335:203'", dTM.ToString(new CACharSet()));
		}

		public void TestPopulateDTM102()
		{
			var dTM = new DTMSegment();
			D96AMessageUtilities.PopulateDTM102(dTM, DateTimePeriodQualifierList.InvoiceDateTime, new ZDate(2009, 5, 21));
			AssertEquals("DTM", "DTM+3:20090521:102'", dTM.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeight1()
		{
			var mEA = new MEASegment();
			D96AMessageUtilities.PopulateMEAWeight(mEA, MeasurementDimensionCodedList.TotalNetWeight, 12345678.9123m, "KG");
			AssertEquals("MEA", "MEA+WT+AAC+KGM:12345679'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeight2()
		{
			var mEA = new MEASegment();
			D96AMessageUtilities.PopulateMEAWeight(mEA, MeasurementDimensionCodedList.TotalGrossWeight, 1m, Constants.Weight.LongTons);
			AssertEquals("MEA", "MEA+WT+AAD+KGM:1016'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateMEAWeight3()
		{
			var mEA = new MEASegment();
			D96AMessageUtilities.PopulateMEAWeight(mEA, MeasurementDimensionCodedList.TotalNetWeight, 1m, "XX");
			AssertEquals("MEA", "MEA+WT+AAC+XX:1'", mEA.ToString(new CACharSet()));
		}

		public void TestPopulateEQDContainer4()
		{
			var eQD = new EQDSegment();
			D96AMessageUtilities.PopulateEQDContainer(eQD, "ABCD1234567");
			AssertEquals("EQD", "EQD+CN+ABCD1234567'", eQD.ToString(new CACharSet()));
		}

		public void TestPopulateRFF()
		{
			var rFF = new RFFSegment();
			D96AMessageUtilities.PopulateRFF(rFF, ReferenceQualifierList.CarriersReferenceNumber, "CC N");
			AssertEquals("RFF", "RFF+CN:CCN'", rFF.ToString(new CACharSet()));
		}

		public void TestPopulatePAC()
		{
			var pAC = new PACSegment();
			D96AMessageUtilities.PopulatePAC(pAC, 200, "BOX");
			AssertEquals("PAC", "PAC+200++:::BOX'", pAC.ToString(new CACharSet()));
		}

		public void TestPopulateTOD1()
		{
			var tOD = new TODSegment();
			D96AMessageUtilities.PopulateTOD(tOD, TermsOfDeliveryOrTransportFunctionCodedList.TransportCondition, "", "");
			AssertEquals("TOD", "TOD+5'", tOD.ToString(new CACharSet()));
		}

		public void TestPopulateTOD2()
		{
			var tOD = new TODSegment();
			D96AMessageUtilities.PopulateTOD(tOD, null, "DELIVERY INSTRUCTIONS 1", "DELIVERY INSTRUCTIONS 2");
			AssertEquals("TOD", "TOD+++:::DELIVERY INSTRUCTIONS 1:DELIVERY INSTRUCTIONS 2'", tOD.ToString(new CACharSet()));
		}

		public void TestPopulateUNS()
		{
			var uNS = new UNSSegment();
			D96AMessageUtilities.PopulateUNS(uNS, SectionIdentificationList.HeaderDetailSectionSeparation);
			AssertEquals("UNS", "UNS+D'", uNS.ToString(new CACharSet()));
		}

		public void TestPopulateMOA1()
		{
			var mOA = new MOASegment();
			D96AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, 1234.5m, "USD", 2);
			AssertEquals("MOA", "MOA+39:1234.50:USD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateMOA2()
		{
			var mOA = new MOASegment();
			D96AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, 1234.5m, "CAD", 0);
			AssertEquals("MOA", "MOA+39:1235:CAD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateMOA3()
		{
			var mOA = new MOASegment();
			D96AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeQualifierList.InvoiceTotalAmount, 1234.5m, "", 0);
			AssertEquals("MOA", "MOA+39:1235'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateMOA4()
		{
			var mOA = new MOASegment();
			D96AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeQualifierList.UnitPrice, 1234.5m, "CAD", 4);
			AssertEquals("MOA", "MOA+146:1234.50:CAD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateMOA5()
		{
			var mOA = new MOASegment();
			D96AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeQualifierList.UnitPrice, 1234.56789m, "CAD", 4);
			AssertEquals("MOA", "MOA+146:1234.5679:CAD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateMOA6()
		{
			var mOA = new MOASegment();
			D96AMessageUtilities.PopulateMOA(mOA, MonetaryAmountTypeQualifierList.UnitPrice, 1234m, "CAD", 4);
			AssertEquals("MOA", "MOA+146:1234.00:CAD'", mOA.ToString(new CACharSet()));
		}

		public void TestPopulateDMS()
		{
			var dMS = new DMSSegment();
			D96AMessageUtilities.PopulateDMS(dMS, "INV987654321");
			AssertEquals("DMS", "DMS+INV987654321'", dMS.ToString(new CACharSet()));
		}

		public void TestPopulateDOC1()
		{
			var dOC = new DOCSegment();
			D96AMessageUtilities.PopulateDOC(dOC, "", "");
			AssertEquals("DOC", "DOC+862'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateDOC2()
		{
			var dOC = new DOCSegment();
			D96AMessageUtilities.PopulateDOC(dOC, "RULING98765", "NEW YORK CITY, NY, USA");
			AssertEquals("DOC", "DOC+862:::RULING98765+::NEW YORK CITY, NY, USA'", dOC.ToString(new CACharSet()));
		}

		public void TestPopulateLIN()
		{
			var lIN = new LINSegment();
			D96AMessageUtilities.PopulateLIN(lIN, 1, "0303030000", 2);
			AssertEquals("LIN", "LIN+1++0303030000+:2'", lIN.ToString(new CACharSet()));
		}

		public void TestPopulateQTY()
		{
			var qTY = new QTYSegment();
			D96AMessageUtilities.PopulateQTY(qTY, 100m, "PCE");
			AssertEquals("QTY", "QTY+PCE:100'", qTY.ToString(new CACharSet()));
			D96AMessageUtilities.PopulateQTY(qTY, 1.5m, "KGM");
			AssertEquals("QTY", "QTY+KGM:1.5'", qTY.ToString(new CACharSet()));
			D96AMessageUtilities.PopulateQTY(qTY, 1.23456, "LGM");
			AssertEquals("QTY", "QTY+LGM:1.2346'", qTY.ToString(new CACharSet()));
		}

		public void TestPopulateIMD1()
		{
			var iMD = new IMDSegment();
			D96AMessageUtilities.PopulateIMD(iMD, "TRDESC", "COMMODITY 3 DESCRIPTION WHICH SHOULD BE SPLIT OVER 2 LINES");
			AssertEquals("IMD", "IMD+++TRDESC:::COMMODITY 3 DESCRIPTION WHICH SHOUL:D BE SPLIT OVER 2 LINES'", iMD.ToString(new CACharSet()));
		}

		public void TestPopulateIMD2()
		{
			var iMD = new IMDSegment();
			D96AMessageUtilities.PopulateIMD(iMD, "TRDESC", "COMMODITY DESCRIPTION");
			AssertEquals("IMD", "IMD+++TRDESC:::COMMODITY DESCRIPTION'", iMD.ToString(new CACharSet()));
		}

		public void TestPopulateFTX()
		{
			var fTX = new FTXSegment();
			D96AMessageUtilities.PopulateFTX(fTX, TextSubjectQualifierList.GoodsDescription, "EXTRA DETAILS");
			AssertEquals("FTX", "FTX+AAA+++EXTRA DETAILS'", fTX.ToString(new CACharSet()));
		}

		public void TestPopulateUNT()
		{
			var uNT = new UNTSegment();
			D96AMessageUtilities.PopulateUNT(uNT, "10", "20");
			AssertEquals("UNT", "UNT+10+20'", uNT.ToString(new CACharSet()));
		}

		public void TestPopulateCOM()
		{
			var com = new COMSegment();
			D96AMessageUtilities.PopulateCOM(com, CommunicationChannelQualifierList.Telefax, "(101) 234-567-89");
			AssertEquals("COM", "COM+0123456789:FX'", com.ToString(new CACharSet()));
		}
	}
}
