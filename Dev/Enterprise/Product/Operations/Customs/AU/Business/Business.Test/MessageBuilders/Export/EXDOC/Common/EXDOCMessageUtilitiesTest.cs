using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCMessageUtilitiesTest : TestCaseWithFactory
	{
		public void TestPopulateUNH()
		{
			EXDOCMessageUtilities.PopulateUNH(sancrtMessage.UNH.InstantiateAChildAndAddItToChildrenCollection(),
					MessageTypeList.AvailabilityResponseInteractiveMessage,
					MessageVersionNumberList.StandardVersion,
					MessageReleaseNumberList.Release1997B,
					ControllingAgencyList.Limnet,
					"MARDIGRA");
			AssertEquals("UNH population", "UNH+<<MSGNO PLACEHOLDER>>+AVLRSP:S:97B:LI:MARDIGRA'", MessageAsString);
		}

		public void TestPopulateBGM()
		{
			EXDOCMessageUtilities.PopulateBGM(sancrtMessage.BGM.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.CreditNoteRelatedToFinancialAdjustments,
					CodeListResponsibleAgencyCodedList.PlPolishStateRailway,
					"BGMTEST",
					"666",
					MessageFunctionCodedList.Forecast,
					ResponseTypeCodedList.Pending);
			AssertEquals("BGM Population", "BGM+83::32:BGMTEST+666+25+AJ'", MessageAsString);
		}

		public void TestPopulateDTM()
		{
			EXDOCMessageUtilities.PopulateDTM(sancrtMessage.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.AccountingYear,
					"23042007",
					DateTimePeriodFormatQualifierList.FourMonthsPeriod);
			AssertEquals("DTM Population", "DTM+246:23042007:809'", MessageAsString);
		}

		public void TestPopulateLOC3Parameters()
		{
			EXDOCMessageUtilities.PopulateLOC(sancrtMessage.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.BorderCrossingPlace,
					"LOCATION");
			AssertEquals("3 Parameter LOC Population", "LOC+17+LOCATION'", MessageAsString);
		}

		public void TestPopulateLOC4Parameters()
		{
			EXDOCMessageUtilities.PopulateLOC(sancrtMessage.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.CountryOfDespatch,
					"LOCATION",
					"RELATIVE");
			AssertEquals("4 Parameter LOC Population", "LOC+113+LOCATION+RELATIVE'", MessageAsString);
		}

		public void TestPopulateRFF()
		{
			EXDOCMessageUtilities.PopulateRFF(sancrtMessage.RFF.InstantiateAChildAndAddItToChildrenCollection(),
					ReferenceQualifierList.AgerdAerospaceGroundEquipmentRequirementDataNumber,
					"REF123");
			AssertEquals("RFF Population", "RFF+ALU:REF123'", MessageAsString);
		}

		public void TestPopulateATT()
		{
			var group11 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.LabelApprovalIndicatorQualifier);
			AssertEquals("ATT Population", "ATT+10++Y:LAI:AQ'", MessageAsString);
		}

		public void TestPopulateATTWithTrue()
		{
			var group11 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.LabelApprovalIndicatorQualifier, true);
			AssertEquals("ATT Population", "ATT+10++Y:LAI:AQ'", MessageAsString);
		}

		public void TestPopulateATTWithFalse()
		{
			var group11 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.LabelApprovalIndicatorQualifier, false);
			AssertEquals("ATT Population", "ATT+10++N:LAI:AQ'", MessageAsString);
		}

		public void TestPopulateFTX()
		{
			EXDOCMessageUtilities.PopulateFTX(sancrtMessage,
					TextSubjectQualifierList.BillOfLadingClause,
					65,
					18,
					"JUST SOME TEST FREE TEXT THAT ALLOWS ME TO SE IF THE LINES ARE SP");
			AssertEquals("FTX Population", "FTX+BLC+++JUST SOME TEST FRE:E TEXT THAT ALLOWS: ME TO SE IF THE L:INES ARE SP'", MessageAsString);
		}

		public void TestPopulateFTXGroup11()
		{
			EXDOCMessageUtilities.PopulateFTX(sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection(),
					TextSubjectQualifierList.CodeListName,
					63,
					10,
					"SEGMENT GROUP 11 FREE TEXT FOR TESTING THE FTX SPLITS CORRECTLY U");
			AssertEquals("FTX Population Group 11", "FTX+ADF+++SEGMENT GR:OUP 11 FRE:E TEXT FOR: TESTING T:HE FTX SPL'FTX+ADF+++ITS CORREC:TLY'", MessageAsString);
		}

		public void TestPopulateFTXGroup11Type2()
		{
			EXDOCMessageUtilities.PopulateFTX(sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection(),
					TextSubjectQualifierList.BankToBankInformation,
					"1234",
					"5678",
					"9012",
					"3456",
					"7890");
			AssertEquals("FTX Population Group 11 Type 2", "FTX+AEQ+++1234:5678:9012:3456:7890'", MessageAsString);
		}

		public void TestPopulateFTXGroup16()
		{
			var group11 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateFTX(group11.Group16.InstantiateAChildAndAddItToChildrenCollection(),
					TextSubjectQualifierList.ControlledAtmosphere,
					90,
					35,
					"SEGMENT GROUP 11 FREE TEXT FOR TESTING THE FTX SPLITS CORRECTLY EVEN WITH WHITE SPACE");
			AssertEquals("FTX Population Group 16", "FTX+AEJ+++SEGMENT GROUP 11 FREE TEXT FOR TEST:ING THE FTX SPLITS CORRECTLY EVEN W:ITH WHITE SPACE'", MessageAsString);
		}

		public void TestPopulateMEA1()
		{
			EXDOCMessageUtilities.PopulateMEA(sancrtMessage.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.AverageReading,
					PropertyMeasuredCodedList.Cobalt,
					"465");
			AssertEquals("MEA Population 1", "MEA+ABN+ZCO+:465'", MessageAsString);
		}

		public void TestPopulateMEA2()
		{
			EXDOCMessageUtilities.PopulateMEA(sancrtMessage.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Footage,
					"KG",
					"98");
			AssertEquals("MEA Population 2", "MEA+FO++KG:98'", MessageAsString);
		}

		public void TestPopulateMEA3()
		{
			EXDOCMessageUtilities.PopulateMEA(sancrtMessage.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.InterpolatedValue,
					PropertyMeasuredCodedList.DistanceBetweenPoints,
					"M3",
					"3.456");
			AssertEquals("MEA Population 3", "MEA+IV+DS+M3:3.456'", MessageAsString);
		}

		public void TestPopulateMEA4()
		{
			EXDOCMessageUtilities.PopulateMEA(sancrtMessage.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.LicenceQuantityDeducted,
					PropertyMeasuredCodedList.AngleOfBend,
					MeasurementSignificanceCodedList.NotEqualTo,
					"KG",
					"456");
			AssertEquals("MEA Population 4", "MEA+AAK+AF:10+KG:456'", MessageAsString);
		}

		public void TestPopulateMEA5()
		{
			EXDOCMessageUtilities.PopulateMEA(sancrtMessage.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.LineItemMeasurement,
					PropertyMeasuredCodedList.AverageGrossWeight,
					"KG",
					"150.3",
					"140",
					"160");
			AssertEquals("MEA Population 5", "MEA+AAA+AEP+KG:150.3:140:160'", MessageAsString);
		}

		public void TestPopulateMEA6()
		{
			EXDOCMessageUtilities.PopulateMEA(sancrtMessage.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.WeightAscertained,
					PropertyMeasuredCodedList.Zirconium,
					MeasurementSignificanceCodedList.Trace,
					"KG",
					"1234",
					"145",
					"180");
			AssertEquals("MEA Population 6", "MEA+ASW+ZZR:11+KG:1234:145:180'", MessageAsString);
		}

		public void TestPopulateMOA()
		{
			EXDOCMessageUtilities.PopulateMOA(sancrtMessage.MOA.InstantiateAChildAndAddItToChildrenCollection(),
					MonetaryAmountTypeQualifierList.AgentCommissionAmount,
					"AUD");
			AssertEquals("MOA Population", "MOA+209::AUD'", MessageAsString);
		}

		public void TestPopulateMOA2()
		{
			EXDOCMessageUtilities.PopulateMOA(sancrtMessage.MOA.InstantiateAChildAndAddItToChildrenCollection(),
					MonetaryAmountTypeQualifierList.AgentCommissionAmount,
					"USD");
			AssertEquals("MOA Population", "MOA+209::US'", MessageAsString);
		}

		public void TestPopulateMOA3()
		{
			EXDOCMessageUtilities.PopulateMOA(sancrtMessage.MOA.InstantiateAChildAndAddItToChildrenCollection(),
					MonetaryAmountTypeQualifierList.AgentCommissionAmount,
					"EUR");
			AssertEquals("MOA Population", "MOA+209::EUR'", MessageAsString);
		}

		public void TestPopulateMOA4()
		{
			EXDOCMessageUtilities.PopulateMOA(sancrtMessage.MOA.InstantiateAChildAndAddItToChildrenCollection(),
					MonetaryAmountTypeQualifierList.AgentCommissionAmount,
					"JPY");
			AssertEquals("MOA Population", "MOA+209::JP'", MessageAsString);
		}

		public void TestPopulateGIS()
		{
			EXDOCMessageUtilities.PopulateGIS(sancrtMessage,
					"Y",
					"PF");
			AssertEquals("GIS Population", "GIS+Y::AQ:PF'", MessageAsString);
		}

		public void TestPopulateDOC1()
		{
			var group1 = sancrtMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDOC(group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.CrewsEffectsDeclaration,
					"DFG8934");
			AssertEquals("DOC Population 1", "DOC+744+DFG8934'", MessageAsString);
		}

		public void TestPopulateDOC2()
		{
			var group1 = sancrtMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDOC(group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.DeliveryOrder,
					"E7",
					"SNKL3498",
					DocumentMessageStatusCodedList.ToArriveBySeparateEdiMessage);
			AssertEquals("DOC Population 2", "DOC+640:::E7+SNKL3498:4'", MessageAsString);
		}

		public void TestPopulateDOC3()
		{
			var group1 = sancrtMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDOC(group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.NoticeOfCircumstancesPreventingTransportGoods,
					"UYNAD87934",
					"CUSTOMS HOUSE");
			AssertEquals("DOC Population 3", "DOC+783+UYNAD87934::CUSTOMS HOUSE'", MessageAsString);
		}

		public void TestPopulateDOC4()
		{
			SegmentGroup1 group1 = sancrtMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateDOC(group1.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.WrittenInstructionsInConformanceWithAdrArticleNumber10385,
					"TST123",
					"KJH237",
					DocumentMessageStatusCodedList.ShippedOnBoard,
					"AQIS");
			AssertEquals("DOC Population 4", "DOC+48:::TST123+KJH237:21:AQIS'", MessageAsString);
		}

		public void TestPopulateGroup2FTX()
		{
			var group2 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateGroup2FTX(group2.FTX.InstantiateAChildAndAddItToChildrenCollection(), TextSubjectQualifierList.MutuallyDefined, "TRACESApprovalID");
			AssertEquals("FTX Traces Approval ID", "FTX+ZZZ+4+TRACESCONSIGNEEID:160:AQ+TRACESAPPROVALID'", MessageAsString);
		}

		public void TestPopulatePNA1()
		{
			var group2 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.BuildingSiteForwarder,
					"456781");
			AssertEquals("PNA Population 1", "PNA+BZ+456781'", MessageAsString);
		}

		public void TestPopulatePNA2()
		{
			var group2 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePNA(group2.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.Exchanger,
					"4579872",
					NameComponentQualifierList.Surname,
					"WRIGHT");
			AssertEquals("PNA Population 2", "PNA+EC+4579872++++1:WRIGHT'", MessageAsString);
		}

		public void TestPopulateCTA()
		{
			var group3 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection().Group3.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateCTA(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.ChangedBy,
					"VOTE CLINTY");
			AssertEquals("CTA Population", "CTA+CB+VOTE CLINTY'", MessageAsString);
		}

		public void TestPopulateCTARepName()
		{
			var group3 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection().Group3.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateCTARepName(group3.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.ChangedBy,
					"AKIRA KURUSAWA");
			AssertEquals("CTA Population", "CTA+CB+:AKIRA KURUSAWA'", MessageAsString);
		}

		public void TestPopulateCOM()
		{
			var group3 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection().Group3.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateCOM(group3.COM.InstantiateAChildAndAddItToChildrenCollection(), CommunicationChannelQualifierList.Telephone, "+61 (2) 8001-1234");
			AssertEquals("COM Population", "COM+61280011234:TE'", MessageAsString);
		}

		public void TestPopulateADR()
		{
			var group2 = sancrtMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateADR(group2.ADR.InstantiateAChildAndAddItToChildrenCollection(),
					AddressFormatCodedList.UnstructuredAddress,
					"184 Bourke Rd",
					"Alexandria",
					"Sydney",
					"2056",
					"AU",
					"NSW");
			AssertEquals("ADR Population", "ADR++5:184 BOURKE RD:ALEXANDRIA+SYDNEY+2056+AU+:::NSW'", MessageAsString);
		}

		public void TestPopulateTDT()
		{
			var group4 = sancrtMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateTDT(group4.TDT.InstantiateAChildAndAddItToChildrenCollection(),
					TransportStageQualifierList.SecondPreCarriageTransport,
					"123",
					"S",
					"PERKINS",
					"345678");
			AssertEquals("TDT Population", "TDT+26+123+S++:::PERKINS+++:::345678'", MessageAsString);
		}

		public void TestPopulateEQD1()
		{
			var group6 = sancrtMessage.Group6.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateEQD(group6.EQD.InstantiateAChildAndAddItToChildrenCollection());
			AssertEquals("EQD Population 1", "EQD+VH'", MessageAsString);
		}

		public void TestPopulateEQD2()
		{
			var group6 = sancrtMessage.Group6.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateEQD(group6.EQD.InstantiateAChildAndAddItToChildrenCollection(),
					EquipmentQualifierList.ForkedSupport,
					"3243214");
			AssertEquals("EQD Population 2", "EQD+FSU+3243214'", MessageAsString);
		}

		public void TestPopulateSEL1()
		{
			var group7 = sancrtMessage.Group6.InstantiateAChildAndAddItToChildrenCollection().Group7.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateSEL(group7.SEL.InstantiateAChildAndAddItToChildrenCollection(),
					"123498", "111", "222");
			AssertEquals("SEL Population 1", "SEL+123498+111+222'", MessageAsString);
		}

		public void TestPopulateSEL2()
		{
			var group7 = sancrtMessage.Group6.InstantiateAChildAndAddItToChildrenCollection().Group7.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateSEL(group7.SEL.InstantiateAChildAndAddItToChildrenCollection(),
					"123454",
					"123456");
			AssertEquals("SEL Population 2", "SEL+123454+123456'", MessageAsString);
		}

		public void TestPopulatePRC()
		{
			var group8 = sancrtMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePRC(group8.PRC.InstantiateAChildAndAddItToChildrenCollection(),
					ProcessTypeIdentificationList.WoodPreparation);
			AssertEquals("PRC Population", "PRC+1:PP:AQ'", MessageAsString);
		}

		public void TestPopulateIMD1()
		{
			var group8 = sancrtMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateIMD(group8.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					"BFK",
					"FICTIONAL BOOKS");
			AssertEquals("IMD Population 1", "IMD+++BFK:::FICTIONAL BOOKS'", MessageAsString);
		}

		public void TestPopulateIMD2()
		{
			var group8 = sancrtMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateIMD(group8.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					ItemCharacteristicCodedList.ControlItem,
					"LIT",
					"FLOODLIGHTS");
			AssertEquals("IMD Population 2", "IMD++24+LIT:::FLOODLIGHTS'", MessageAsString);
		}

		public void TestPopulateIMD3()
		{
			var group8 = sancrtMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateIMD(group8.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					"BFK",
					"DESCRIPTION LINE 1---------------->" +
					"DESCRIPTION LINE 2---------------->" +
					"DESCRIPTION LINE 3---------------->" +
					"DESCRIPTION LINE 4---------------->" +
					"DESCRIPTION LINE 5---------------->" +
					"DESCRIPTION LINE 6---------------->" +
					"DESCRIPTION LINE 7---------------->" +
					"DESCRIPTION LINE 8---------------->");
			AssertEquals("IMD Population 1", "IMD+++BFK:::DESCRIPTION LINE 1---------------->:DESCRIPTION LINE 2---------------->:DESCRIPTION LINE 3---------------->:" +
			"DESCRIPTION LINE 4---------------->:DESCRIPTION LINE 5---------------->:DESCRIPTION LINE 6---------------->:DESCRIPTION LINE 7---------------->:DESCRIPTION LINE 8---------------->'", MessageAsString);
		}

		public void TestPopulateLIN()
		{
			var group11 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateLIN(group11.LIN.InstantiateAChildAndAddItToChildrenCollection(),
					"23");
			AssertEquals("LIN Population", "LIN+23'", MessageAsString);
		}

		public void TestPopulatePIA()
		{
			var group11 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
					ProductIdFunctionQualifierList.SubstitutedBy,
					"AFG",
					ItemNumberTypeCodedList.Exhibit);
			AssertEquals("PIA Population", "PIA+3+AFG:AW'", MessageAsString);
		}

		public void TestPopulatePAC()
		{
			var group15 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection().Group15.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(),
					"34",
					PackagingLevelCodedList.Intermediate,
					"BOX");
			AssertEquals("PAC Population", "PAC+34+2+BOX::AQ'", MessageAsString);
		}

		public void TestPopulatePCI()
		{
			var group15 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection().Group15.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulatePCI(group15.PCI.InstantiateAChildAndAddItToChildrenCollection(),
					"NOT TO SURE HOW MANY THE SPECIFICATION SAID THERE COULD BE FOR SHIPPING MARKS SO I PUT A LOT");
			AssertEquals("PCI Population", "PCI++NOT TO SURE HOW MANY THE SPECIFICATION SAID THERE COULD BE FOR SHIPPING MARKS SO I PUT A LOT'", MessageAsString);
		}

		public void TestPopulateGIN()
		{
			var group16 = sancrtMessage.Group11.InstantiateAChildAndAddItToChildrenCollection().Group16.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateGIN(group16.GIN.InstantiateAChildAndAddItToChildrenCollection(),
					IdentityNumberQualifierList.PositionNumberInPackage,
					"213");
			AssertEquals("GIN Population", "GIN+AO+213'", MessageAsString);
		}

		public void TestPopulateUNT()
		{
			var group21 = sancrtMessage.Group21.InstantiateAChildAndAddItToChildrenCollection();
			EXDOCMessageUtilities.PopulateUNT(group21.UNT.InstantiateAChildAndAddItToChildrenCollection(),
					"56");
			AssertEquals("UNT Population", "UNT+56+<<MSGNO PLACEHOLDER>>'", MessageAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			sancrtMessage = new SANCRTMessage();
		}

		ZString MessageAsString => sancrtMessage.ToString(new Edifact.UNOACharacterSet());

		SANCRTMessage sancrtMessage;
	}
}
