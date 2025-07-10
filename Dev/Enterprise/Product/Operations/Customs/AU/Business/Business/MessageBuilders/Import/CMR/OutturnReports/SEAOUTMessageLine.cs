using System;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAOUTMessageLine : UniqueIdentifierMessageLine
	{
		public SEAOUTMessageLine(ISeaOutturnReportLineInformation reportLine)
		{
			this.reportLine = reportLine;
		}

		public override string UniqueIdentifier
		{
			get { return GetUniqueIdentifier(reportLine.ContainerNumber, reportLine.OceanBillOfLading, reportLine.HouseBillOfLading); }
		}

		public static string GetUniqueIdentifier(string containerNo, string oceanBill, string houseBill)
		{
			return ("CONTAINERNUMBER=" + containerNo + "OCEANBILL=" + oceanBill + "HOUSEBILL=" + houseBill).ToUpper();
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
				PopulateDTM(group8);
				PopulateQTY(group8);
				PopulateGID(group7);
				SegmentGroup14 group14 = group8.Group14.Count > 0 ? group8.Group14[group8.Group14.Count - 1] : null;
				if (group14 != null)
				{
					PopulatePAC(group14);
					PopulateFTX(group14);
					PopulatePCI(group14);
				}
			}
		}

		void PopulateRFF(SegmentGroup7 group7)
		{
			if (!reportLine.ContainerNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, reportLine.ContainerNumber, null);
			}

			if (!reportLine.OutturnResultType.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.LossEventNumber, reportLine.OutturnResultType, null);
			}

			if (!reportLine.HouseBillOfLading.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber, reportLine.HouseBillOfLading, null);
			}

			if (!reportLine.OceanBillOfLading.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber, reportLine.OceanBillOfLading, null);
			}

			if (!reportLine.SealNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.SealNumber, reportLine.SealNumber.KeepAlphanumericCharacters().SubstringSafe(0, 10), null);
			}
		}

		void PopulateGIS(SegmentGroup8 group8)
		{
			MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString(reportLine.SealIntactIndicator ? "Y" : "N"), CodeListIdentificationCodeList.RequirementsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString(reportLine.VesselDischargeUnderbondIndicator ? "V" : "U"), CodeListIdentificationCodeList.HandlingAction, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString(reportLine.UnpackIndicator ? "U" : "R"), CodeListIdentificationCodeList.NatureOfTransaction, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString(reportLine.DamageIndicator ? "Y" : "N"), CodeListIdentificationCodeList.ProductSupplyCondition, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			MessageUtilities.PopulateGIS(group8.GIS.InstantiateAChildAndAddItToChildrenCollection(), ProcessingIndicatorDescriptionCodeList.GetFromString(reportLine.PillageIndicator ? "Y" : "N"), CodeListIdentificationCodeList.ArticleStatus, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
		}

		void PopulateDTM(SegmentGroup8 group8)
		{
			if (!reportLine.DateTimeOfCargoReceiptUnload.IsEmpty)
			{
				MessageUtilities.PopulateDTM(group8.Group9[0].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.UnloadedDateAndTime, reportLine.DateTimeOfCargoReceiptUnload.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
				MessageUtilities.PopulateDTM(group8.Group9[0].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.UnloadedDateAndTime, reportLine.DateTimeOfCargoReceiptUnload.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
			}

			if (!reportLine.DateTimeOfOutturn.IsEmpty)
			{
				MessageUtilities.PopulateDTM(group8.Group9[0].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ReportedDate, reportLine.DateTimeOfOutturn.ToString("yyyyMMdd"), DateTimePeriodFormatCodeList.Ccyymmdd);
				MessageUtilities.PopulateDTM(group8.Group9[0].DTM.InstantiateAChildAndAddItToChildrenCollection(), DateTimePeriodFunctionCodeQualifierList.ReportedDate, reportLine.DateTimeOfOutturn.ToString("HHmm"), DateTimePeriodFormatCodeList.Hhmm);
			}
			if (group8.Group9.Count > 0)
			{
				group8.Group9[0].TDT[0].TransportStageCodeQualifier = TransportStageCodeQualifierList.GetFromString("1");
			}
		}

		void PopulateQTY(SegmentGroup8 group8)
		{
			if (!reportLine.Quantity.IsEmpty && !reportLine.QuantityUnit.IsEmpty)
			{
				group8.Group13[0].QTY[0].QuantityDetails.QuantityTypeCodeQualifier = QuantityTypeCodeQualifierList.LostGoods;
				group8.Group13[0].QTY[0].QuantityDetails.Quantity = reportLine.Quantity.ToString();
				group8.Group13[0].QTY[0].QuantityDetails.MeasurementUnitCode = reportLine.QuantityUnit;
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
			if (reportLine.NumberOfPackages > 0 && !reportLine.PackageType.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.PackageType, CodeListIdentificationCodeList.ItemType, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}

			if (!reportLine.ImportCargoType.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.ImportCargoType, CodeListIdentificationCodeList.TypeOfPackage, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		void PopulateFTX(SegmentGroup14 group14)
		{
			if (!reportLine.GoodsDescription.IsEmpty)
			{
				MessageUtilities.PopulateFTX(group14.FTX[0], TextSubjectCodeQualifierList.GoodsDescription, reportLine.GoodsDescription);
			}
		}

		void PopulatePCI(SegmentGroup14 group14)
		{
			if (!reportLine.MarksAndNumbers.IsEmpty)
			{
				MessageUtilities.PopulatePCI(group14.PCI[0], MarkingInstructionsCodedList.MarkFreeText, reportLine.MarksAndNumbers);
			}
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			return ((CUSCARMessage)message).Group7.InstantiateAChildAndAddItToChildrenCollection();
		}

		protected internal override Type SegmentGroupType
		{
			get
			{
				return typeof(SegmentGroup7);
			}
		}

		readonly ISeaOutturnReportLineInformation reportLine;
	}
}
