using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSRES_2_912;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	public class CUSRESInboundParser
	{
		public CUSRESInboundParser(EDIMessage ediMessageIn)
		{
			inboundEdiMessage = ediMessageIn;
			inputText = CUSCARGeneratorBase.RestoreFakeSegmentNames(ediMessageIn.CharacterSet, ediMessageIn.EM_MessageText);  // turns 'BGM+blah into  'BGM-CCSUK+blah, so that we can parse the fake segments			
		}

		public CUSRESResponseData ParseCUSRESForListOfUpdatedFields()
		{
			source = new CUSRESMessage();
			source.Parse(inboundEdiMessage.CharacterSet, inputText);

			var cUSRESResponseData = new CUSRESResponseData();

			var unh = source.UNH[0];
			cUSRESResponseData.CommonAccessReference = unh.CommonAccessReference;

			var bgm = source.BGM[0];
			cUSRESResponseData.DocumentNameCode = bgm.DocumentMessageName.DocumentNameCode.ToString();
			cUSRESResponseData.AirWaybillPrefixAndNumber = bgm.DocumentMessageIdentification.DocumentIdentifier;
			if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier == "HWB")
			{
				cUSRESResponseData.HouseWaybillNumber = bgm.ReferenceC506.ReferenceIdentifier;
				cUSRESResponseData.SplitReference = bgm.ReferenceC506.DocumentLineIdentifier;
			}
			else if (bgm.ReferenceC506.ReferenceFunctionCodeQualifier == "ACD")
			{
				cUSRESResponseData.SplitReference = bgm.ReferenceC506.DocumentLineIdentifier;
			}

			foreach (NADSegment nad in source.NAD)
			{
				cUSRESResponseData.AgentCode = nad.PartyIdentificationDetails.PartyIdentifier;
				cUSRESResponseData.AgentName = nad.NameAndAddress.NameAndAddressDescription1;
				cUSRESResponseData.AgentsTelephoneNumber = nad.NameAndAddress.NameAndAddressDescription2;
				break;
			}

			foreach (LOCSegmentWithLotsOfLocations loc in source.LOC)
			{
				foreach (LocationAndRelationsAsASingleElement c517 in new List<LocationAndRelationsAsASingleElement>() { loc.Location1, loc.Location2, loc.Location3 })
				{
					if (c517.PlaceLocationQualifier3227 == "11")
					{
						cUSRESResponseData.AirportOfReceipt = c517.PlaceLocationIdentification3225;
						cUSRESResponseData.ShedId = c517.SubLocationIdentification3439;
					}
					else if (c517.PlaceLocationQualifier3227 == "27")
					{
						cUSRESResponseData.CountryOfOrigin = c517.PlaceLocationIdentification3225;
					}
					else if (c517.PlaceLocationQualifier3227 == "84")
					{
						cUSRESResponseData.AirportOfOrigin = c517.PlaceLocationIdentification3225;
					}
				}
			}

			foreach (CUSRESSegmentGroup2 grp2 in source.Group2)
			{
				cUSRESResponseData.NoOfPackagesExpected = int.Parse(grp2.PAC[0].PackageQuantity);
				break;
			}

			foreach (CUSRESSegmentGroup4 grp4 in source.Group4)
			{
				if (grp4.RFF[0].Reference.ReferenceFunctionCodeQualifier == "TN")
				{
					cUSRESResponseData.EntryNumber = grp4.RFF[0].Reference.ReferenceIdentifier;
					var dateAndTime = new ZDateTime();
					if (!string.IsNullOrEmpty(grp4.RFF[0].DateTimeC507.DateOrTimeOrPeriodValue))
					{
						ZDateTime.TryParseExact(grp4.RFF[0].DateTimeC507.DateOrTimeOrPeriodValue, out dateAndTime, "yyyyMMdd");
					}
					cUSRESResponseData.EntryDate = dateAndTime.Date;
				}
				else if (grp4.RFF[0].Reference.ReferenceFunctionCodeQualifier == "ABE")
				{
					cUSRESResponseData.AgentsReferenceNumber = grp4.RFF[0].Reference.ReferenceIdentifier;
				}
			}

			foreach (GISSegment gis in source.GIS)
			{
				if (gis.ProcessingIndicator_X.CodeListIdentificationCode == "120")
				{
					cUSRESResponseData.CustomsActionCode_StatusOfRequest = gis.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode.ToString();
				}
			}
			foreach (FTXSegment ftx in source.FTX)
			{
				if (ftx.TextSubjectCodeQualifier == "CAT")
				{
					cUSRESResponseData.CustomsActionText = ftx.TextLiteral.FreeTextValue1;
				}
				else if (ftx.TextSubjectCodeQualifier == "AAA")
				{
					cUSRESResponseData.DescriptionOfGoods = ftx.TextLiteral.FreeTextValue1;
				}
			}

			return cUSRESResponseData;
		}

		CUSRESMessage source;
		readonly string inputText;
		readonly EDIMessage inboundEdiMessage;
	}
}
