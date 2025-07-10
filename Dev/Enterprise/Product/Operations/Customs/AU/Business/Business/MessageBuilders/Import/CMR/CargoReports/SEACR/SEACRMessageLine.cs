using System;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEACRMessageLine : UniqueIdentifierMessageLine
	{
		public SEACRMessageLine(ISeaCargoReportLine reportLine)
		{
			this.reportLine = reportLine;
		}

		public override string UniqueIdentifier
		{
			get { return reportLine.ContainerMode + reportLine.ContainerNumber; }
		}

		public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
		{
			SegmentGroup7 group7 = (SegmentGroup7)segmentGroup;
			MessageUtilities.PopulateCNI(group7.CNI[0], null, lineActionCode);
			PopulateReferences(group7);
			PopulateIndicators(group7);
			SegmentGroup14 group14 = null;
			foreach (SegmentGroup8 group8 in group7.Group8)
			{
				group14 = group8.Group14[0];
				CargoReportHelper.DoICSRelease(() =>
				{
					var vendor = reportLine.ConsignorVendor;
					if (!vendor.IsEmpty)
					{
						var group11 = group8.Group11[0];
						MessageUtilities.PopulateNAD(group11.NAD.InstantiateAChildAndAddItToChildrenCollection(), PartyFunctionCodeQualifierList.Vendor, vendor, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService, null, null);
					}
				});
				MessageUtilities.PopulateGID(group14.GID[0], "1");
			}
			if (group14 != null)
			{
				PopulatePackingDetails(group14);
				PopulateDescription(group14);
				PopulateMeasurements(group14);
				PopulateMarksAndNumbers(group14);
			}
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			return ((CUSCARMessage)message).Group7.InstantiateAChildAndAddItToChildrenCollection();
		}

		protected internal override Type SegmentGroupType => typeof(SegmentGroup7);

		#region References

		protected void PopulateReferences(SegmentGroup7 group7)
		{
			if (!reportLine.ContainerNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, reportLine.ContainerNumber, null);
			}
			if (!reportLine.ContainerSize.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.ShippingUnitIdentification, reportLine.ContainerSize, null);
			}
			if (!reportLine.SealNumber.IsEmpty)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.SealNumber, reportLine.SealNumber, null);
			}
			if (group7.Group8.Count == 0)
			{
				MessageUtilities.PopulateRFF(group7.Group8.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.MutuallyDefinedReferenceNumber, "1", null);
			}
		}

		#endregion

		#region Indicators

		protected void PopulateIndicators(SegmentGroup7 group7)
		{
			if (reportLine.FumigationCertificateIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("FUM"));
			}

			if (reportLine.HazardousGoodsIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("HAZ"));
			}

			if (reportLine.PerishableGoodsIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("PSH"));
			}

			if (reportLine.PersonalEffectsIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("PER"));
			}

			if (reportLine.DocumentsIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("DOC"));
			}

			if (reportLine.SACIndication)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("SAC"));
			}

			if (reportLine.TimberIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("TMB"));
			}

			if (reportLine.ShippingOwnedContainerIndicator)
			{
				PopulateIndicator(group7, ProcessingIndicatorDescriptionCodeList.GetFromString("SOC"));
			}
		}

		protected void PopulateIndicator(SegmentGroup7 group7, ProcessingIndicatorDescriptionCodeList code)
		{
			int group8Count = group7.Group8.Count;
			SegmentGroup8 lastGroup8 = group7.Group8[group8Count == 0 ? 0 : group8Count - 1];
			MessageUtilities.PopulateGIS(lastGroup8.GIS[lastGroup8.GIS.Count], code, CodeListIdentificationCodeList.CustomsIndicator, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
		}

		#endregion

		#region Packing

		protected void PopulatePackingDetails(SegmentGroup14 group14)
		{
			if (!reportLine.PackageCount.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.PackageCount);
			}
			if (!reportLine.ContainerMode.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.ContainerMode, CodeListIdentificationCodeList.TypeOfPackage, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (!reportLine.ContainerType.IsEmpty)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.ContainerType, CodeListIdentificationCodeList.ShipmentDescription, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
			if (!reportLine.PackageType.IsEmpty && reportLine.ContainerMode != CMRImportCargoTypes.Codes.Bulk)
			{
				MessageUtilities.PopulatePAC(group14.PAC.InstantiateAChildAndAddItToChildrenCollection(), reportLine.PackageType, CodeListIdentificationCodeList.ItemType, CodeListResponsibleAgencyCodeList.AuAustralianCustomsService);
			}
		}

		#endregion

		#region Description

		protected void PopulateDescription(SegmentGroup14 group14)
		{
			if (!reportLine.GoodsDescription.IsEmpty)
			{
				MessageUtilities.PopulateFTX(group14.FTX[0], TextSubjectCodeQualifierList.GoodsDescription, reportLine.GoodsDescription);
			}
		}

		#endregion

		#region Measurements

		protected void PopulateMeasurements(SegmentGroup14 group14)
		{
			if (!reportLine.NetWeight.IsEmpty && !reportLine.WeightUQ.IsEmpty)
			{
				MessageUtilities.PopulateMEA(group14.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementAttributeCodeList.Measurement, "AAL", reportLine.WeightUQ, reportLine.NetWeight.ToString(2));
			}

			if (!reportLine.Weight.IsEmpty && !reportLine.WeightUQ.IsEmpty)
			{
				MessageUtilities.PopulateMEA(group14.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementAttributeCodeList.Measurement, "G", reportLine.WeightUQ, reportLine.Weight.ToString(2));
			}

			if (!reportLine.Volume.IsEmpty)
			{
				MessageUtilities.PopulateMEA(group14.MEA.InstantiateAChildAndAddItToChildrenCollection(), MeasurementAttributeCodeList.Measurement, "ABJ", "CU", reportLine.Volume.ToString(2));
			}
		}

		#endregion

		#region Marks And Numbers

		protected void PopulateMarksAndNumbers(SegmentGroup14 group14)
		{
			// TODO: Remove when fixed on customs' side.
			if (reportLine.ContainerMode == CMRImportCargoTypes.Codes.Bulk)
			{
				MessageUtilities.PopulatePCI(group14.PCI[0], MarkingInstructionsCodedList.MarkFreeText, "N/A");
			}
			else
			{
				if (!reportLine.MarksAndNumbers.IsEmpty)
				{
					MessageUtilities.PopulatePCI(group14.PCI[0], MarkingInstructionsCodedList.MarkFreeText, reportLine.MarksAndNumbers);
				}
			}
		}

		#endregion

		readonly ISeaCargoReportLine reportLine;
	}
}
