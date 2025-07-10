using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEIMessageLine : GenericCMRDepotMessageLine<CMRSEIMessage>
	{
		public CMRSEIMessageLine(CMRSEIMessage message, SegmentGroup6 group6)
			: base(message)
		{
			Argument.NotNull(group6, "group6");
			this.LineGroup6 = group6;
		}
		public SegmentGroup6 LineGroup6 { private set; get; }

		ZString GetReference(ReferenceFunctionCodeQualifierList referenceCode)
		{
			foreach (RFFSegment rFF in LineGroup6.RFF)
			{
				if (rFF.Reference != null)
				{
					string refCode = rFF.Reference.ReferenceFunctionCodeQualifier;
					if (refCode == referenceCode.ToString())
					{
						return rFF.Reference.ReferenceIdentifier;
					}
				}
			}
			return ZString.Empty;
		}

		public CMRSEIMessage SEIMessage
		{
			get { return Message; }
		}

		public ZString OutturnResultType
		{
			get { return GetReference(ReferenceFunctionCodeQualifierList.LossEventNumber); }
		}

		public ZBool FreightForwarderIndicator
		{
			get
			{
				foreach (GISSegment gIS in LineGroup6.GIS)
				{
					if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode.ToString() == "FFO")
					{
						return true;
					}
				}
				return false;
			}
		}

		ReturnData GetMEAValue(MeasuredAttributeCodeList measurementCode)
		{
			ReturnData result = new ReturnData();
			foreach (MEASegment mEA in LineGroup6.MEA)
			{
				if (mEA.MeasurementDetails.MeasuredAttributeCode == measurementCode)
				{
					var weight = ZDecimal.ParseSafe(mEA.ValueRange.MeasurementValue, 0m);
					var weightUnit = (ZString)mEA.ValueRange.MeasurementUnitCode;
					new WeightConversionStrategy().ReScale(ref weight, ref weightUnit, 9, 3);
					result.ResultString = weightUnit;
					result.ResultDecimal = weight;
					break;
				}
			}
			return result;
		}

		public ReturnData NetWeight
		{
			get
			{
				if (netWeight == null)
				{
					netWeight = GetMEAValue(MeasuredAttributeCodeList.NetWeight);
				}

				return netWeight;
			}
		}
		ReturnData netWeight;

		public ZString NetWeightString
		{
			get
			{
				ZString units = NetWeight.ResultString;
				ZDecimal value = NetWeight.ResultDecimal;
				return !value.IsEmpty ? value.ToString("#.##") + " " + units : string.Empty;
			}
		}

		public ReturnData GrossWeight
		{
			get
			{
				if (grossWeight == null)
				{
					grossWeight = GetMEAValue(MeasuredAttributeCodeList.GrossWeight);
				}

				return grossWeight;
			}
		}
		ReturnData grossWeight;

		public ZString GrossWeightString
		{
			get
			{
				ZString units = GrossWeight.ResultString;
				ZDecimal value = GrossWeight.ResultDecimal;
				return !value.IsEmpty ? value.ToString("#.##") + " " + units : string.Empty;
			}
		}

		public ReturnData Volume
		{
			get
			{
				if (volume == null)
				{
					volume = GetMEAValue(MeasuredAttributeCodeList.Volume);
				}

				return volume;
			}
		}
		ReturnData volume;

		public ZString VolumeString
		{
			get
			{
				ZString units = Volume.ResultString;
				ZDecimal value = Volume.ResultDecimal;
				return !value.IsEmpty ? value.ToString("#.##") + " " + units : string.Empty;
			}
		}

		public ZString ConsigneeName
		{
			get
			{
				foreach (SegmentGroup7 group7 in LineGroup6.Group7)
				{
					foreach (NADSegment nAD in group7.NAD)
					{
						ZStringBuilder builder = new ZStringBuilder();
						builder.AppendIfNotEmpty(nAD.NameAndAddress.NameAndAddressLine1);
						builder.AppendIfNotEmpty(nAD.NameAndAddress.NameAndAddressLine2);
						builder.AppendIfNotEmpty(nAD.NameAndAddress.NameAndAddressLine3);
						builder.AppendIfNotEmpty(nAD.NameAndAddress.NameAndAddressLine4);
						builder.AppendIfNotEmpty(nAD.NameAndAddress.NameAndAddressLine5);
						return builder.ToStringWithNewLineBetweenAppends();
					}
				}
				return ZString.Empty;
			}
		}

		protected override ZString GetContainerNumberCore()
		{
			return GetReference(ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber);
		}

		protected override ZString GetGoodsDescriptionCore()
		{
			return LineGroup6.FTX.Count > 0 ? LineGroup6.FTX[0].TextLiteral.FreeTextValue1 : string.Empty;
		}

		protected override ZString GetHouseBillNumberCore()
		{
			CMRContainerModeChecker checker = new CMRContainerModeChecker(ContainerMode);
			return (checker.IsLCL || checker.IsBulk || checker.IsBreakBulk) ? GetReference(ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber) : ZString.Empty;
		}

		protected override ZString GetMarksAndNumbersCore()
		{
			ZStringBuilder marks = new ZStringBuilder();
			foreach (PCISegment pCI in LineGroup6.PCI)
			{
				if (pCI.MarkingInstructionsCoded == MarkingInstructionsCodedList.MarkFreeText)
				{
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks1);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks2);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks3);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks4);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks5);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks6);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks7);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks8);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks9);
					marks.AppendIfNotEmpty(pCI.MarksLabels.ShippingMarks10);
				}
			}
			return marks.ToStringWithNewLineBetweenAppends();
		}

		protected override ZInt GetNumberOfPackagesCore()
		{
			foreach (PACSegment pAC in LineGroup6.PAC)
			{
				if (!string.IsNullOrEmpty(pAC.NumberOfPackages))
				{
					ZInt result = 0;
					if (ZInt.TryParse(pAC.NumberOfPackages, out result))
					{
						return result;
					}
				}
			}
			return ZInt.Zero;
		}

		protected override ZString GetOceanBillNumberCore()
		{
			CMRContainerModeChecker checker = new CMRContainerModeChecker(ContainerMode);
			return (checker.IsLCL || checker.IsBulk || checker.IsBreakBulk) ? GetReference(ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber) : ZString.Empty;
		}

		protected override ZString GetPackageTypeCore()
		{
			foreach (PACSegment pAC in LineGroup6.PAC)
			{
				if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.ItemType)
				{
					return pAC.PackageType.PackageTypeDescriptionCode;
				}
			}
			return ZString.Empty;
		}

		protected override ZString GetContainerModeCore()
		{
			foreach (PACSegment pAC in LineGroup6.PAC)
			{
				if (pAC.PackageType.CodeListIdentificationCode == CodeListIdentificationCodeList.TypeOfPackage)
				{
					return pAC.PackageType.PackageTypeDescriptionCode;
				}
			}
			return ZString.Empty;
		}

		protected override ZDecimal GetActualWeightCore()
		{
			return GrossWeight.ResultDecimal;
		}

		protected override ZString GetWeightUnitsCore()
		{
			return GrossWeight.ResultString;
		}

		protected override ZDecimal GetActualVolumeCore()
		{
			return Volume.ResultDecimal;
		}

		protected override ZString GetVolumeUnitsCore()
		{
			return SeaCargoUtilities.ConvertCMRVolumeUnitToVolumeUnit(Volume.ResultString);
		}

		public override string ToString()
		{
			ZStringBuilder builder = new ZStringBuilder();
			builder.Append("Processing Date: " + Message.ProcessingDate.ToString("dd/MM/yyyy HH:mm:ss"));
			if (!HouseBillNumber.IsEmpty)
			{
				builder.Append("House Bill: " + HouseBillNumber);
			}

			if (!OceanBillNumber.IsEmpty)
			{
				builder.Append("Ocean Bill: " + OceanBillNumber);
			}

			if (!ContainerNumber.IsEmpty)
			{
				builder.Append("Container: " + ContainerNumber);
			}

			builder.Append("Container Mode: " + ContainerMode);
			builder.Append("Packages: " + NumberOfPackages.ToString() + " " + PackageType);
			if (!OutturnResultType.IsEmpty)
			{
				builder.Append("Outturn Result Type: " + OutturnResultType);
			}

			if (FreightForwarderIndicator)
			{
				builder.Append("Freight Forwarder Indicator ON (Master or Sub-Consol)");
			}

			if (!NetWeightString.IsEmpty)
			{
				builder.Append("Net Weight: " + NetWeightString);
			}

			if (!GrossWeightString.IsEmpty)
			{
				builder.Append("Gross Weight: " + GrossWeightString);
			}

			if (!VolumeString.IsEmpty)
			{
				builder.Append("Volume: " + VolumeString);
			}

			if (!ConsigneeName.IsEmpty)
			{
				builder.Append("Consignee: " + ConsigneeName);
			}

			builder.Append("Goods Description: " + GoodsDescription);
			if (!MarksAndNumbers.IsEmpty)
			{
				builder.Append("Marks: " + MarksAndNumbers);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}
	}
}
