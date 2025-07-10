using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA
{
	public class FsaParser
	{
		public FsaParser(EDIMessage ediMessageIn)
		{
			inputText = CUSCARGeneratorBase.RestoreFakeSegmentNames(ediMessageIn.CharacterSet, ediMessageIn.EM_MessageText);  // turns 'BGM+blah into  'BGM-CCSUK+blah, so that we can parse the fake segments			
		}

		public FsaResponseMessage Parse()
		{
			if (source == null)
			{
				try
				{
					source = new CUKFSAMessage();
					source.Parse(new UkCharSet(), inputText);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					string message = "PARSE ERROR.  It's possible - likely - that the FSA from CCSUK contained segments or composite elements where the D00A definition differs from the 91-2 in a way that cases base to fail. To fix this we must make a new segment or element and change the definition of CUKFSAMessage to utilise that. The inner exception will tell you the segment.";
					var ex2 = new Exception(message, ex);
					ErrorReporter.ReportOnce("BGB-CUK-FSA-ParseError", ex2);
					throw ex2;
				}
			}

			result = new FsaResponseMessage();
			result.CargoWise_CommonAccessReference = source.UNH[0].CommonAccessReference;

			ParseBgm();
			ParseHeaderFtx();
			ParseGroup1();
			CopyTemporaryStorageDateFromOldConsignmentToNewConsignmentBecauseCcsukDontSendItInTheNewHalf();
			return result;
		}

		void CopyTemporaryStorageDateFromOldConsignmentToNewConsignmentBecauseCcsukDontSendItInTheNewHalf()
		{
			// CCSUK should send the DTM+164 is boththe old and the new consignments. Because we usually use only the new consignment to create a record when processing a P5 (because it's the new half that talks about the shed to will receive the ISR or IAR goods), we need to ensure that the new consigment has the old consignment's TS date.  Copy it.  This is valid, CCSUK have admitted their defect (early 2018), and have confirmed that the dates in the old and new halves should be identical. 
			var oldConsignment = result.ChildConsignments.FirstOrDefault(c => c.OldOrNewDataIndicator == "OLD");
			var newConsignment = result.ChildConsignments.FirstOrDefault(c => c.OldOrNewDataIndicator == "NEW");
			if (oldConsignment != null && newConsignment != null && !oldConsignment.TemporaryStorageEndDate.IsEmpty && newConsignment.TemporaryStorageEndDate.IsEmpty)
			{
				newConsignment.TemporaryStorageEndDate = oldConsignment.TemporaryStorageEndDate;
			}
		}

		void ParseBgm()
		{
			var bgm = source.BGM[0];
			result.Header_ReportType = bgm.DocumentMessageName.DocumentName;
			result.Header_AirwaybillPrefixAndAirwaybillNumber = bgm.DocumentMessageIdentification.DocumentIdentifier;
			if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier == "HWB")
			{
				result.Header_HouseWaybillNumber = bgm.ReferenceC506.ReferenceIdentifier;
				result.Header_SplitReference = bgm.ReferenceC506.DocumentLineIdentifier;
			}
			else if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier == "ACD")
			{
				result.Header_SplitReference = bgm.ReferenceC506.DocumentLineIdentifier;
			}
		}

		void ParseHeaderFtx()
		{
			var ftx = source.FTX[0];
			if (ftx != null && ftx.TextSubjectCodeQualifier == "AAA")
			{
				result.Header_ReportText = CondenseFtxIntoSingleString(ftx);
			}
		}

		void ParseGroup1()
		{
			var childConsignments = new List<FsaChildConsignment>();

			foreach (CUKFSASegmentGroup1 grp1ChildConsignment in source.Group1)
			{
				var childConsignment = new FsaChildConsignment();
				childConsignments.Add(childConsignment);
				ParseGroup1Doc(grp1ChildConsignment, childConsignment);
			}
			result.ChildConsignments = childConsignments;
		}

		void ParseGroup1Doc(CUKFSASegmentGroup1 grp1ChildConsignment, FsaChildConsignment childConsignment)
		{
			// Group 1 - one for each child consignment
			childConsignment.ConsignmentReferenceNumber = grp1ChildConsignment.DOC[0].DocumentMessageDetails.DocumentIdentifier;  // awb or split number
			childConsignment.ConsignmentReferenceNumberType = grp1ChildConsignment.DOC[0].DocumentMessageName.DocumentNameCode != null
																	? grp1ChildConsignment.DOC[0].DocumentMessageName.DocumentNameCode.ToString()
																	: "";  // 740, 741, 703, SRF. CCSUK has a bug (Dec 2012) whereby this segment is blank when no record is found. 
			childConsignment.OldOrNewDataIndicator = grp1ChildConsignment.DOC[0].Element7;  // OLD or NEW
			foreach (CUKFSASegmentGroup2 grp2 in grp1ChildConsignment.Group2)
			{
				ParseGroup2(grp2, childConsignment);
			}
			foreach (CUKFSASegmentGroup3 grp3 in grp1ChildConsignment.Group3)
			{
				ParseGroup3(childConsignment, grp3);
			}
			foreach (CUKFSASegmentGroup4 grp4 in grp1ChildConsignment.Group4)
			{
				ParseGroup4(grp4, childConsignment);
			}
			foreach (CUKFSASegmentGroup5 grp5 in grp1ChildConsignment.Group5)
			{
				ParseGroup5(grp5, childConsignment);
			}
			foreach (CUKFSASegmentGroup6 grp6 in grp1ChildConsignment.Group6)
			{
				ParseGroup6(grp6, childConsignment);
			}
		}

		void ParseGroup2(CUKFSASegmentGroup2 grp2, FsaChildConsignment childConsignment)
		{
			// group 2 - status flags and error text
			foreach (GISSegment gis in grp2.GIS)
			{
				SetStatusFlagsFromGroup2Gis(gis, childConsignment);
			}
			foreach (FTXSegment ftx in grp2.FTX)
			{
				childConsignment.IndicatorErrorText = CondenseFtxIntoSingleString(ftx);
			}
		}

		void ParseGroup3(FsaChildConsignment childConsignment, CUKFSASegmentGroup3 grp3)
		{
			var flight = CreateLegFromGroup3(grp3);
			if (flight.LegType == ConsignmentLegFlight.LegTypes.Inward)
			{
				childConsignment.InwardLeg = flight;
			}
			else if (flight.LegType == ConsignmentLegFlight.LegTypes.Onward)
			{
				childConsignment.OnwardLeg = flight;
			}
		}

		void ParseGroup4(CUKFSASegmentGroup4 grp4, FsaChildConsignment childConsignment)
		{
			foreach (NADSegment nad in grp4.NAD)
			{
				if (nad.PartyFunctionCodeQualifier.ToString() == "CB")
				{
					childConsignment.AgentCode = nad.PartyIdentificationDetails.PartyIdentifier;
					childConsignment.AgentName = nad.NameAndAddress.NameAndAddressDescription1;
				}
				if (nad.PartyFunctionCodeQualifier.ToString() == "CM")
				{
					childConsignment.CustomsUserID = nad.PartyIdentificationDetails.PartyIdentifier;
				}
			}
			int printerNumber = 0;
			foreach (COMSegment com in grp4.COM)
			{
				printerNumber++;
				if (com.CommunicationContact.CommunicationNumberCodeQualifier == "CA")
				{
					if (printerNumber == 1)
					{ childConsignment.PrintLocation1 = com.CommunicationContact.CommunicationNumber; }
					if (printerNumber == 2)
					{ childConsignment.PrintLocation2 = com.CommunicationContact.CommunicationNumber; }
				}
			}
		}

		void ParseGroup5(CUKFSASegmentGroup5 grp5, FsaChildConsignment childConsignment)
		{
			// Goods item
			foreach (QTYSegment qty in grp5.QTY)
			{
				ZInt quantity = ZInt.ParseEmptyAsZero(qty.QuantityDetails.Quantity);
				if (qty.QuantityDetails.QuantityTypeCodeQualifier == "48")
				{
					childConsignment.NPR = quantity;
				}
				else if (qty.QuantityDetails.QuantityTypeCodeQualifier == "118")
				{
					childConsignment.NPX = quantity;
				}
			}
			foreach (MEASegment mea in grp5.MEA)
			{
				if (mea.MeasurementAttributeCode == "WT")
				{
					childConsignment.Weight = ZDecimal.Parse(mea.ValueRange.MeasurementValue);
					childConsignment.WeightCode = mea.ValueRange.MeasurementUnitCode;
				}
			}

			DTMSegment dtm = grp5.DTM[0];
			if (dtm != null)
			{
				var date = GetDate(dtm.DateTimePeriod.DateOrTimeOrPeriodValue, dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode);
				if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == "7") // status 1
				{
					childConsignment.Status1Date = date;
				}
				else if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == "164") // Temporary Storage End Date
				{
					childConsignment.TemporaryStorageEndDate = date;
				}
			}

			foreach (FTXSegment ftx in grp5.FTX)
			{
				if (ftx.TextSubjectCodeQualifier.ToString() == "AAA")
				{
					childConsignment.DescriptionOfGoods = ftx.TextLiteral.FreeTextValue1;
					break; //expect only one
				}
			}

			int rffIndex = 0;
			foreach (RFFSegmentWithDatetime rff in grp5.RFF)
			{
				rffIndex++;
				if (rff.Reference.ReferenceFunctionCodeQualifier == "HS")
				{
					if (rffIndex == 1)
					{
						childConsignment.HarmonisedCommodityCode1 = rff.Reference.ReferenceIdentifier;
					}

					if (rffIndex == 2)
					{
						childConsignment.HarmonisedCommodityCode2 = rff.Reference.ReferenceIdentifier;
					}
				}
			}

			foreach (MOASegment moa in grp5.MOA)
			{
				if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == "40")
				{
					childConsignment.ValueOfGoods = ZDecimal.Parse(moa.MonetaryAmount.MonetaryAmount);
					childConsignment.CurrencyCode = moa.MonetaryAmount.CurrencyIdentificationCode;
				}
			}

			// NB - CCSUK have a bug (July 2018) whereby they send the DTM+164 only in the "old" half, i.e. in DTM, but not in the "new" half (i.e. in DTM2).  Ensure do not clobber the date if it was captured frm the old half but missing from the new half.  When sent in both places it will be equal. But don't clobber the 'old' value with the blank 'new' value. 
			DTMSegment dtm2 = grp5.DTM2[0];
			if (dtm2 != null && dtm2.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == "164") // Temporary Storage End Date
			{
				var tsEndDateFromNewHalf = GetDate(dtm2.DateTimePeriod.DateOrTimeOrPeriodValue, dtm2.DateTimePeriod.DateOrTimeOrPeriodFormatCode);
				if (!tsEndDateFromNewHalf.IsEmpty)
				{
					childConsignment.TemporaryStorageEndDate = tsEndDateFromNewHalf;
				}
			}
		}

		void ParseGroup6(CUKFSASegmentGroup6 grp6, FsaChildConsignment childConsignment)
		{
			CSTSegment cst = grp6.CST[0];
			if (cst != null)
			{
				foreach (CustomsIdentityCodesElements c246 in new CustomsIdentityCodesElements[] { cst.CustomsIdentityCodes1, cst.CustomsIdentityCodes2, cst.CustomsIdentityCodes3, cst.CustomsIdentityCodes4, cst.CustomsIdentityCodes5 })
				{
					string value = c246.CustomsGoodsIdentifier;
					switch (c246.CodeListIdentificationCode)
					{
						case "117":
							childConsignment.CustomsActionCode = value;
							break;
						case "120":
							childConsignment.CustomsClearanceStatus = value;
							break;
						case "141":
							childConsignment.Route = value;
							break;
						case "110":
							childConsignment.InventoryReturnCodeIRC = value;
							break;
					}
				}
			}

			foreach (FTXSegment ftx in grp6.FTX)
			{
				switch (ftx.TextSubjectCodeQualifier)
				{
					case "CAT":
						childConsignment.CustomsActionText = ftx.TextLiteral.FreeTextValue1;
						break;
					case "IRT":
						childConsignment.IRCText = (ftx.TextLiteral.FreeTextValue1 + " " + ftx.TextLiteral.FreeTextValue2).Trim();
						break;
				}
			}

			DTMSegment dtm = grp6.DTM[0];
			if (dtm != null && dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == "176")
			{
				childConsignment.DateOfCustomsAction = GetDate(dtm.DateTimePeriod.DateOrTimeOrPeriodValue, dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode);
			}

			foreach (RFFSegmentWithDatetime rff in grp6.RFF)
			{
				var value = rff.Reference.ReferenceIdentifier;
				switch (rff.Reference.ReferenceFunctionCodeQualifier)
				{
					case "CKN":
						childConsignment.InventoryVersionNo = ZInt.ParseEmptyAsZero(value);
						break;
					case "ABE":
						childConsignment.AgentsReferenceNumber = value;
						break;
					case "ACF":
						childConsignment.EntryProcessingUnit = value;
						break;
					case "TN":
						childConsignment.EntryNumber = value;
						childConsignment.EntryDate = GetDate(rff.DateTimeC507.DateOrTimeOrPeriodValue, rff.DateTimeC507.DateOrTimeOrPeriodFormatCode);
						break;
				}
			}

			QTYSegment qty = grp6.QTY[0];
			if (qty != null && qty.QuantityDetails.QuantityTypeCodeQualifier == "66")
			{
				childConsignment.NumPackagesEntered = ZInt.ParseEmptyAsZero(qty.QuantityDetails.Quantity);
			}
		}

		void SetStatusFlagsFromGroup2Gis(GISSegment gis, FsaChildConsignment childConsignment)
		{
			string codeListQualifier_1131 = gis.ProcessingIndicator_X.CodeListIdentificationCode;
			string processingIndicator_7365 = gis.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode;
			switch (codeListQualifier_1131)
			{
				case "121":
					childConsignment.ShipmentDescriptionCode = processingIndicator_7365;
					break;

				case "117":
					childConsignment.ConsignmentType = processingIndicator_7365;
					break;

				case "148":
					childConsignment.DetainedTextIndicator = processingIndicator_7365 == "DTI";
					break;

				case "109":
					switch (processingIndicator_7365)
					{
						case "PAI":
							childConsignment.PreArrivalIndicator = true;
							break;
						case "CSI":
							childConsignment.CurrentStatusIndicator = true;
							break;
						case "VI1":
							childConsignment.VerificationIndicatorOne = true;
							break;
						case "VI2":
							childConsignment.VerificationIndicatorTwo = true;
							break;
						case "VI3":
							childConsignment.VerificationIndicatorThree = true;
							break;
						case "VI4":
							childConsignment.VerificationIndicatorFour = true;
							break;
						case "VI5":
							childConsignment.VerificationIndicatorFive = true;
							break;
						case "VI6":
							childConsignment.VerificationIndicatorSix = true;
							break;
						case "VI7":
							childConsignment.VerificationIndicatorSeven = true;
							break;
						case "VI8":
							childConsignment.VerificationIndicatorEight = true;
							break;
						case "VI9":
							childConsignment.VerificationIndicatorNine = true;
							break;
						case "LIC":
							childConsignment.IndicatorLicenceRestricted = true;
							break;
						case "SAI":
							childConsignment.SpecialActionIndicator = true;
							break;
					}
					break;

				case "131":
					childConsignment.CommunityHandlingCodes.Add(processingIndicator_7365);
					break;
			}
		}

		ConsignmentLegFlight CreateLegFromGroup3(CUKFSASegmentGroup3 grp3)
		{
			// Gr 3 appears twice, once for inward, once for outward movements
			var tdt = grp3.TDT[0];
			var carrierCode = tdt.Carrier.CarrierIdentifier;
			var dateOfArrival = tdt.DateTimeC507.DateOrTimeOrPeriodValue;
			var dateOfArrivalFormat = tdt.DateTimeC507.DateOrTimeOrPeriodFormatCode;

			var flight = new ConsignmentLegFlight();
			flight.LegType = (ConsignmentLegFlight.LegTypes)Enum.Parse(typeof(ConsignmentLegFlight.LegTypes), tdt.TransportStageQualifier);

			flight.Carrier = carrierCode;
			flight.FlightNumber = tdt.ConveyanceReferenceNumber;
			flight.Date = GetDate(dateOfArrival, dateOfArrivalFormat);
			flight.Mode = tdt.XModeOfTransport;

			var locations = new LocationsCollection();
			LocationTypes locType = new LocationTypes();
			foreach (LOCSegmentWithLotsOfLocations loc in grp3.LOC)  //Two LOC segments, but each has up to 9 C517 elements.  Pfff. 
			{
				foreach (LocationAndRelationsAsASingleElement c517 in new List<LocationAndRelationsAsASingleElement>() { loc.Location1, loc.Location2, loc.Location3, loc.Location4, loc.Location5, loc.Location6, loc.Location7, loc.Location8, loc.Location9 })
				{
					var freightLocation = new FreightLocation();
					freightLocation.LocationCode = c517.PlaceLocationIdentification3225;
					freightLocation.Function = c517.PlaceLocationQualifier3227;
					freightLocation.ShedOperator = c517.SubLocationIdentification3439;
					freightLocation.ShedPhysicalIdentity = c517.SubLocation3438;
					if (!freightLocation.IsEmpty)
					{
						locations.Add(freightLocation);
					}
				}
			}
			foreach (RFFSegmentWithDatetime rff in grp3.RFF)
			{
				if (rff.Reference.ReferenceFunctionCodeQualifier == "AWB")
				{
					flight.Awb = rff.Reference.ReferenceIdentifier;
				}
			}

			flight.Locations = locations;
			return flight;
		}

		string CondenseFtxIntoSingleString(FTXSegment ftx)
		{
			var sb = new ZStringBuilder();
			sb.AppendIfNotEmpty(ftx.TextLiteral.FreeTextValue1);
			sb.AppendIfNotEmpty(ftx.TextLiteral.FreeTextValue2);
			sb.AppendIfNotEmpty(ftx.TextLiteral.FreeTextValue3);
			sb.AppendIfNotEmpty(ftx.TextLiteral.FreeTextValue4);
			sb.AppendIfNotEmpty(ftx.TextLiteral.FreeTextValue5);
			return sb.ToStringWithDelimiterBetweenAppends(" ");
		}

		public static ZDateTime GetDate(string date, DateOrTimeOrPeriodFormatCodeList dateFormat)
		{
			ZDateTime dateTime;
			ZDateTime.TryParseExact(date, out dateTime, dateFormat == "101" ? "yyMMdd" : (dateFormat == "201" ? "yyMMddHHmm" : (dateFormat == "102" ? "yyyyMMdd" : "")));
			return dateTime;
		}

		public static ZDateTime GetDate(string desiredQualifierFilter, params DateTimePeriodElements[] dates)
		{
			var dateTime = ZDateTime.Empty;
			foreach (var dateElement in dates)
			{
				if (dateElement != null && dateElement.DateOrTimeOrPeriodFunctionCodeQualifier == desiredQualifierFilter)
				{
					var date = dateElement.DateOrTimeOrPeriodValue;
					var dateFormat = dateElement.DateOrTimeOrPeriodFormatCode;
					dateTime = GetDate(date, dateFormat);
					break;
				}
			}
			return dateTime;
		}

		readonly string inputText;
		CUKFSAMessage source;
		FsaResponseMessage result;
	}
}
