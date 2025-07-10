using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class CuscarInboundParser
	{
		public CuscarInboundParser(CUSCARMessage source)
		{
			this.source = source;
		}

		internal CuscarWithFlagsToShowWhatsSet ParseCuscarForListOfUpdatedFields()
		{
			var flagableCuscar = new CuscarWithFlagsToShowWhatsSet();
			var bgm = source.BGM[0];
			ZString fullMawb = bgm.DocumentMessageIdentification.DocumentIdentifier;
			flagableCuscar.AirlinePrefix = fullMawb.Left(3);
			flagableCuscar.AirlinePrefixSet = true;
			flagableCuscar.AirWaybillSerialNumber = fullMawb.Right(8);
			flagableCuscar.AirWaybillSerialNumberSet = true;
			if (bgm.ReferenceC506 != null)
			{
				if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier != null)
				{
					if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier.ToString() == "HWB")
					{
						flagableCuscar.HouseAirWaybillNumber = bgm.ReferenceC506.ReferenceIdentifier;
						flagableCuscar.HouseAirWaybillNumberSet = true;
						flagableCuscar.SplitReference = bgm.ReferenceC506.DocumentLineIdentifier;
						flagableCuscar.SplitReferenceSet = !flagableCuscar.SplitReference.IsEmpty;
					}
					if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier.ToString() == "ACD")
					{
						flagableCuscar.SplitReference = bgm.ReferenceC506.DocumentLineIdentifier;
						flagableCuscar.SplitReferenceSet = true;
					}
				}
			}

			if (bgm.DateTimeC507_1 != null || bgm.DateTimeC507_2 != null)
			{
				var creationDate = FsaParser.GetDate("97", bgm.DateTimeC507_1, bgm.DateTimeC507_2);
				if (!creationDate.IsEmpty)
				{
					flagableCuscar.DateTimeOfRecordCreation = creationDate;
					flagableCuscar.DateTimeOfRecordCreationSet = true;
				}
				var status1Date = FsaParser.GetDate("50", bgm.DateTimeC507_1, bgm.DateTimeC507_2);
				if (!status1Date.IsEmpty)
				{
					flagableCuscar.Status1Date = status1Date;
					flagableCuscar.Status1DateSet = true;
				}
			}

			foreach (GISSegment gis in source.GIS)
			{
				if (gis.ProcessingIndicator_X != null)
				{
					var code = gis.ProcessingIndicator_X.CodeListIdentificationCode;
					var description = gis.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode;
					if (string.IsNullOrEmpty(code) && description != null && description.ToString().StartsWith("S2"))
					{
						flagableCuscar.Status2Indicator = description.ToString().EndsWith("Y");
						flagableCuscar.Status2IndicatorSet = true;
					}
					else if (code == "121" && !string.IsNullOrEmpty(description))
					{
						flagableCuscar.ShipmentDescriptionCode = description.ToString();
						flagableCuscar.ShipmentDescriptionCodeSet = true;
					}
					else if (code == "131" && !string.IsNullOrEmpty(description))
					{
						flagableCuscar.CommunityHandlingCodes.Add(description.ToString());
					}
				}
			}
			foreach (CUSCARSegmentGroup2 grp2 in source.Group2)
			{
				foreach (TDTSegmentWithDatetime tdt in grp2.TDT)
				{
					if (tdt.Carrier != null && tdt.Carrier.CodeListIdentificationCode == "172")
					{
						flagableCuscar.CarrierCode = tdt.Carrier.CarrierIdentifier;
						flagableCuscar.CarrierCodeSet = true;
					}
					if (tdt.ConveyanceReferenceNumber != null)
					{
						flagableCuscar.FlightNumber = tdt.ConveyanceReferenceNumber;
						flagableCuscar.FlightNumberSet = true;
					}
					if (tdt.DateTimeC507 != null && tdt.DateTimeC507.DateOrTimeOrPeriodFunctionCodeQualifier == "178")
					{
						flagableCuscar.DateOfFlightArrival = FsaParser.GetDate(tdt.DateTimeC507.DateOrTimeOrPeriodValue, tdt.DateTimeC507.DateOrTimeOrPeriodFormatCode);
						flagableCuscar.DateOfFlightArrivalSet = true;
					}
				}

				foreach (LOCSegmentWithLotsOfLocations loc in grp2.LOC)
				{
					// This is really cool....
					var locations = new LocationsCollection();
					foreach (LocationAndRelationsAsASingleElement c517 in new List<LocationAndRelationsAsASingleElement>() { loc.Location1, loc.Location2, loc.Location3, loc.Location4, loc.Location5, loc.Location6, loc.Location7, loc.Location8, loc.Location9 })
					{
						var freightLocation = new FreightLocation();
						freightLocation.LocationCode = c517.PlaceLocationIdentification3225;
						freightLocation.Function = c517.PlaceLocationQualifier3227;
						freightLocation.ShedOperator = c517.SubLocationIdentification3439;
						freightLocation.ShedPhysicalIdentity = c517.SubLocation3438;
						locations.Add(freightLocation);
					}
					if (locations.AirportOfDestination != null)
					{
						if (!locations.AirportOfDestination.ShedOperator.IsEmpty)
						{
							flagableCuscar.CargoTerminalOperator = locations.AirportOfDestination.ShedOperator;
							flagableCuscar.CargoTerminalOperatorSet = true;
						}
						if (!locations.AirportOfDestination.LocationCode.IsEmpty)
						{
							flagableCuscar.AirportOfDestination = locations.AirportOfDestination.LocationCode;
							flagableCuscar.AirportOfDestinationSet = true;
						}
					}
					if (locations.AirportOfOrigin != null && !locations.AirportOfOrigin.LocationCode.IsEmpty)
					{
						flagableCuscar.AirportOfOrigin = locations.AirportOfOrigin.LocationCode;
						flagableCuscar.AirportOfOriginSet = true;
					}
					if (locations.AirportOfArrival != null)
					{
						if (!locations.AirportOfArrival.LocationCode.IsEmpty)
						{
							flagableCuscar.AirportOfArrival = locations.AirportOfArrival.LocationCode;
							flagableCuscar.AirportOfArrivalSet = true;
						}
						if (!locations.AirportOfArrival.ShedOperator.IsEmpty)
						{
							flagableCuscar.CargoTerminalOperator = locations.AirportOfArrival.ShedOperator;
							flagableCuscar.CargoTerminalOperatorSet = true;
						}
					}
				}
			}

			foreach (CUSCARSegmentGroup3 grp3 in source.Group3)
			{
				foreach (NADSegment nad in grp3.NAD)
				{
					if (nad != null && nad.PartyFunctionCodeQualifier == "CB")
					{
						flagableCuscar.AgentBrokerConsolidatorCode = nad.PartyIdentificationDetails.PartyIdentifier;
						flagableCuscar.AgentBrokerConsolidatorCodeSet = true;
					}
				}
			}

			foreach (CUSCARSegmentGroup5 grp5 in source.Group5)
			{
				var line = ProcessGroup5LineItemOrItems(grp5);
				flagableCuscar.LinesOnlyForFcsResponses.Add(line);
			}

			if (flagableCuscar.LinesOnlyForFcsResponses.Count > 0)
			{
				var line = (CuscarLineWithFlagsToShowWhatsSet)flagableCuscar.LinesOnlyForFcsResponses[0];
				CopyLineData(flagableCuscar, line);
			}
			return flagableCuscar;
		}

		CuscarLineWithFlagsToShowWhatsSet ProcessGroup5LineItemOrItems(CUSCARSegmentGroup5 grp5)
		{
			var cuscarLine = new CuscarLineWithFlagsToShowWhatsSet();

			foreach (GIDSegment gid in grp5.GID)
			{
				cuscarLine.LineOrSplitNumber = gid.GoodsItemNumber;
			}

			foreach (MEASegment mea in grp5.MEA)
			{
				if (mea.MeasurementAttributeCode == "WT")
				{
					cuscarLine.Weight = ZDecimal.Parse(mea.ValueRange.MeasurementValue);
					cuscarLine.WeightSet = true;
					cuscarLine.WeightCode = mea.ValueRange.MeasurementUnitCode == "KGM" ? Core.Constants.Weight.Kilograms : mea.ValueRange.MeasurementUnitCode.Substring(0, 2);
					cuscarLine.WeightCodeSet = true;
				}
			}

			foreach (FTXSegment ftx in grp5.FTX)
			{
				if (ftx.TextSubjectCodeQualifier == "AAA")
				{
					cuscarLine.DescriptionOfGoods = ftx.TextLiteral.FreeTextValue1;
					cuscarLine.DescriptionOfGoodsSet = true;
				}
			}

			foreach (CUSCARSegmentGroup6 grp6 in grp5.Group6)
			{
				foreach (QTYSegment qty in grp6.QTY)
				{
					ZShort quantity = ZShort.ParseSafe(qty.QuantityDetails.Quantity, 0);
					if (qty.QuantityDetails.QuantityTypeCodeQualifier == "48")
					{
						cuscarLine.NumberOfPiecesReceived = quantity;
						cuscarLine.NumberOfPiecesReceivedSet = true;
					}
					else if (qty.QuantityDetails.QuantityTypeCodeQualifier == "118")
					{
						cuscarLine.NumberOfPiecesExpected = quantity;
						cuscarLine.NumberOfPiecesSet = true;
					}
				}
			}
			return cuscarLine;
		}

		static void CopyLineData(CuscarWithFlagsToShowWhatsSet cuscarTarget, CuscarLineWithFlagsToShowWhatsSet cuscuarLineSource)
		{
			cuscarTarget.NumberOfPiecesExpected = cuscuarLineSource.NumberOfPiecesExpected;
			cuscarTarget.NumberOfPiecesReceived = cuscuarLineSource.NumberOfPiecesReceived;
			cuscarTarget.HarmonisedCommodityCode = cuscuarLineSource.HarmonisedCommodityCode;
			cuscarTarget.DescriptionOfGoods = cuscuarLineSource.DescriptionOfGoods;
			cuscarTarget.Weight = cuscuarLineSource.Weight;
			cuscarTarget.WeightCode = cuscuarLineSource.WeightCode;
			cuscarTarget.NumberOfPiecesExpectedSet = cuscuarLineSource.NumberOfPiecesSet;
			cuscarTarget.NumberOfPiecesReceivedSet = cuscuarLineSource.NumberOfPiecesReceivedSet;
			cuscarTarget.HarmonisedCommodityCodeSet = cuscuarLineSource.HarmonisedCommodityCodeSet;
			cuscarTarget.DescriptionOfGoodsSet = cuscuarLineSource.DescriptionOfGoodsSet;
			cuscarTarget.WeightSet = cuscuarLineSource.WeightSet;
			cuscarTarget.WeightCodeSet = cuscuarLineSource.WeightCodeSet;
			cuscarTarget.LineOrSplitNumber = cuscuarLineSource.LineOrSplitNumber;
		}

		readonly CUSCARMessage source;
	}
}
