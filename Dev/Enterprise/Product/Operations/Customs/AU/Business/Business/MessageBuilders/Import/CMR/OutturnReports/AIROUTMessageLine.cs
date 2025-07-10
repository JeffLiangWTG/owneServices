using System;
using CargoWise.Types;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AIROUTMessageLine : UniqueIdentifierMessageLine
	{
		public AIROUTMessageLine(IAirOutturnReportLineInformation reportLine)
		{
			this.reportLine = reportLine;
		}

		public override string UniqueIdentifier
		{
			get { return "MAWB=" + reportLine.MasterAirWaybillNumber + "HAWB=" + reportLine.HouseAirWaybillNumber; }
		}

		public override ZDateTime LastMessageDate
		{
			get { return reportLine.LastMessageDate; }
		}

		public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
		{
			SegmentGroup7 group7 = (SegmentGroup7)segmentGroup;
			MessageUtilities.PopulateCNI(group7.CNI[0], null, lineActionCode);
			PopulateRFF(group7);
			SegmentGroup8 group8 = group7.Group8.Count > 0 ? group7.Group8[group7.Group8.Count - 1] : null;
			if (group8 != null)
			{
				PopulateGIS(group8);
				PopulateGID(group7);
				SegmentGroup14 group14 = group8.Group14.Count > 0 ? group8.Group14[group8.Group14.Count - 1] : null;
				if (group14 != null)
				{
					PopulatePAC(group14);
					PopulateFTX(group14);
				}
			}
		}

		void PopulateRFF(SegmentGroup7 group7)
		{
			if (!reportLine.OutturnResultType.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.LossEventNumber, reportLine.OutturnResultType, null);
			}
			if (!reportLine.MasterAirWaybillNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.MasterAirWaybillNumber, reportLine.MasterAirWaybillNumber.KeepAlphanumericCharacters(), null);
			}
			if (!reportLine.HouseAirWaybillNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.HouseWaybillNumber, reportLine.HouseAirWaybillNumber, null);
			}
		}

		void PopulateGIS(SegmentGroup8 group8)
		{
			if (reportLine.DamageIndicator)
			{
				MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("DAM"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (reportLine.PillageIndicator)
			{
				MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString("PIL"), CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		void PopulateGID(SegmentGroup7 group7)
		{
			foreach (SegmentGroup8 group8 in group7.Group8)
			{
				MessageUtilities.PopulateGID(group8.Group14[0].GID[0], "1");
			}
		}

		void PopulatePAC(SegmentGroup14 group14)
		{
			MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.NumberOfPackages);
		}

		void PopulateFTX(SegmentGroup14 group14)
		{
			if (!reportLine.GoodsDescription.IsEmpty && (reportLine.OutturnResultType == CMROutturnResultType.Codes.SurplusConsignment || reportLine.OutturnResultType == CMROutturnResultType.Codes.SurplusPackages))
			{
				MessageUtilities.PopulateFTX(group14.FTX[0], TextSubjectCodeQualifierList.GoodsDescription, reportLine.GoodsDescription);
			}
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			return ((CUSCARMessage)message).Group7.InstantiateAChildAndAddItToChildrenCollection();
		}

		protected internal override Type SegmentGroupType => typeof(SegmentGroup7);

		readonly IAirOutturnReportLineInformation reportLine;
	}
}
