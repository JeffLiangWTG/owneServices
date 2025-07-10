using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	internal static class ArrivalAtExitExportMessageTextBuilder
	{
		internal static void PopulateCUSDECMessage(CUSDECMessage message, IArrivalAtExitExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.PopulateUNHSegment(message.UNH[0], MessageReleaseNumberList.TrialRelease1992, "ECSR02");
			CommonMessageTextBuilder.PopulateBGMSegment(message.BGM[0], DocumentMessageNameCodedList.GetFromString("EAL"), source.LocalReferenceNumber, null);
			PopulateCSTSegment(message.CST[0], source);
			PopulateLOCSegments(message.LOC, source);
			PopulateDTMSegments(message.DTM, source);
			PopulateSG6Groups(message.Group6, source);
			CommonMessageTextBuilder.PopulateUNTSegment(message.UNT[0], message.CountIncludingUNT);
		}

		#region Fields Population

		static void PopulateCSTSegment(CSTSegment cstSegment, IArrivalAtExitExportMessageDataProvider source)
		{
			cstSegment.CustomsIdentityCodes5.CustomsCodeIdentification = source.CustomsProcedureCategory5;
		}

		static void PopulateLOCSegments(LOCSegmentMessageSection locSection, IArrivalAtExitExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.CustomsOfficeofExitCountryCode, PlaceLocationQualifierList.CustomsOfficeOfExit, source.CustomsOfficeofExit, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1); // LOC+42
			CommonMessageTextBuilder.AddNewLOCSegment(locSection, source.LocationOfGoodsExamCustomsOffice, PlaceLocationQualifierList.PlaceOfCustomsExamination, source.LocationOfGoodsExam, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, CodeListResponsibleAgencyCodedList.EsSpanishCustoms); // LOC+43
		}

		static void PopulateDTMSegments(DTMSegmentMessageSection dtmSection, IArrivalAtExitExportMessageDataProvider source)
		{
			CommonMessageTextBuilder.AddNewDTMSegment(dtmSection, source.DateOfArrival.ToCustomsFormatDateString(), DateTimePeriodQualifierList.DeliveryDateTimeLast, DateTimePeriodFormatQualifierList.Ccyymmdd);
		}

		#region SG6 Population

		static void PopulateSG6Groups(SegmentGroup6MessageSection sg6Section, IArrivalAtExitExportMessageDataProvider source)
		{
			CommonExportMessageTextBuilder.AddNewNADWithEmailInSG6Group(sg6Section, CodeListResponsibleAgencyCodedList.EsSpanishCustoms, source.Declarant); //NAD+Declarant
		}

		#endregion

		#endregion
	}
}
