using CargoWise.Types;
using Enterprise.Edifact.Utilities;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public static class CommonExportMessageTextBuilder
	{
		public static SegmentGroup6 AddNewNADWithEmailInSG6Group(SegmentGroup6MessageSection sg6Section, CodeListResponsibleAgencyCodedList agencyCodedList, IExportDeclarantPartyIdProvider addressDetails)
		{
			SegmentGroup6 result = null;
			if (addressDetails != null)
			{
				var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
				var nadSegment = sg6.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nadSegment.PartyQualifier = PartyQualifierList.GetFromString(addressDetails.PartyQualifier);
				nadSegment.PartyIdentificationDetails.PartyIdIdentification = addressDetails.Id;
				if (agencyCodedList != null)
				{
					nadSegment.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = agencyCodedList;
				}
				var textSplitter = new TextSplitter(CommonMessageTextBuilder.NameAndAddressLineLength)
				{
					Text = addressDetails.EmailAddress
				};
				nadSegment.NameAndAddress.NameAndAddressLine1 = textSplitter[0];
				nadSegment.NameAndAddress.NameAndAddressLine2 = textSplitter[1];
				nadSegment.PartyName.PartyName1 = addressDetails.Name.Left(35);
				if (!addressDetails.NameCode.IsEmpty)
				{
					nadSegment.PartyName.PartyNameFormatCoded = PartyNameFormatCodedList.GetFromString(addressDetails.NameCode);
				}
			}
			return result;
		}

		public static void AddNewSG7Group(SegmentGroup7MessageSection group7, ZString termsOfDeliveryCode, ZString deliveryLocation, ZString locationId)
		{
			if (!termsOfDeliveryCode.IsEmpty)
			{
				var sg7 = group7.InstantiateAChildAndAddItToChildrenCollection();
				var todSegment = sg7.TOD[0];
				todSegment.TermsOfDelivery.TermsOfDeliveryCoded = termsOfDeliveryCode;
				todSegment.TermsOfDelivery.CodeListQualifier = CodeListQualifierList.Incoterms1980;
				if (!deliveryLocation.IsEmpty)
				{
					var locSegment = sg7.LOC[0];
					locSegment.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDelivery;
					locSegment.LocationIdentification.PlaceLocation = deliveryLocation;
				}
				if (!locationId.IsEmpty)
				{
					var locSegment = sg7.LOC[1];
					locSegment.PlaceLocationQualifier = PlaceLocationQualifierList.RegionOfDelivery;
					locSegment.LocationIdentification.PlaceLocationIdentification = locationId;
					locSegment.LocationIdentification.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1;
				}
			}
		}

		public static void AddNewSG37(SegmentGroup37MessageSection sg37Section, ZString name, ZString number, ZString source, ZDateTime dateOfIssue, ZDateTime dateOfExpiry)
		{
			var sg37 = sg37Section.InstantiateAChildAndAddItToChildrenCollection();
			CommonMessageTextBuilder.AddNewDOCSegment(sg37.DOC, name, number, source, ZString.Empty);
			if (!dateOfExpiry.IsEmpty)
			{
				CommonMessageTextBuilder.AddNewDTMSegment(sg37.DTM, dateOfExpiry.ToShortCustomsFormatDateString(), DateTimePeriodQualifierList.ExpiryDate, DateTimePeriodFormatQualifierList.Yymmdd);
			}
			else
			{
				CommonMessageTextBuilder.AddNewDTMSegment(sg37.DTM, dateOfIssue.ToShortCustomsFormatDateString(), DateTimePeriodQualifierList.DocumentMessageDateTime, DateTimePeriodFormatQualifierList.Yymmdd);
			}
		}
	}
}
