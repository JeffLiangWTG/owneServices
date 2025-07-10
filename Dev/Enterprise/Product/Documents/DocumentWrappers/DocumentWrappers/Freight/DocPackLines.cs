using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocPackLines : DocBaseWrapper, IDocPackageDetails
	{
		#region Construction

		protected DocPackLines(PackLine packLine, BusinessObjectFactory factoryToWrap)
			: this(packLine, null, factoryToWrap)
		{
		}

		protected DocPackLines(PackLine packLine, CommonShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(packLine, factoryToWrap)
		{
			shipmentBizObj = shipment;
		}

		public static DocPackLines New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<PackLine>(pK), factory);
		}

		public static DocPackLines New(PackLine packLine, BusinessObjectFactory factory)
		{
			return New(packLine, null, factory);
		}

		public static DocPackLines New(PackLine packLine, CommonShipment shipment, BusinessObjectFactory factory)
		{
			DocPackLines result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(packLine, factory);
			}
			else if (packLine != null)
			{
				result = new DocPackLines(packLine, shipment, factory);
			}

			return result;
		}

		protected delegate DocPackLines NewDelegate(PackLine packLine, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}

		PackLine PackLine
		{
			get { return (PackLine)WrappedObject; }
		}

		#endregion

		#region Other Fields

		public ZString ContainerCode
		{
			get { return Container != null ? Container.ContainerNumberOrTypeCount : ZString.Empty; }
		}

		public ZString ConsignorAndConsigneeForLoadList { get; set; }
		public ZString CustomsBrokerForLoadList { get; set; }

		public ZString CargoLocationAndPacks
		{
			get
			{
				ZString result = "";
				foreach (PackLocation location1 in PackLine.PackLocations)
				{
					if (location1.JQ_NoPackages > 0)
					{
						if (result.Length > 0)
						{
							result += "\n";
						}

						if (Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value)
						{
							WhsLocation location = Factory.Load<WhsLocation>(location1.JQ_WL);

							if (location != null)
							{
								result += location.Row.WR_Name;
							}
						}
						else
						{
							result += location1.JQ_WarehouseLocation;
						}

						result += (NoResString)",  Packs: " + location1.JQ_NoPackages + (NoResString)" " + PackType;
					}
				}
				return result;
			}
		}

		public ZString CargoLocationAndPacksWithShipmentLocationOfGoods
		{
			get
			{
				ZString result = CargoLocationAndPacksLine;

				if ((Shipment != null) && (PackLine.PackLocations.TotalPackages != PackageCount))
				{
					if (result.Length > 0)
					{
						result += "   ";
					}

					result += Shipment.LocationOfGoodsExcludeName + (NoResString)",  Packs: " + (PackageCount - PackLine.PackLocations.TotalPackages) + (NoResString)" " + PackType;
				}

				return result;
			}
		}

		public ZString CargoLocationAndPacksLine
		{
			get { return CargoLocationAndPacks.Replace("\n", "   "); }
		}

		public ZDecimal ContainerManifestWeight { get; set; }
		public ZString ContainerManifestWeightUnit { get; set; }
		public ZDecimal ContainerTotalManifestWeight { get; set; }
		public ZString ContainerTotalManifestWeightUnit { get; set; }
		public ZDecimal ContainerManifestVolume { get; set; }
		public ZString ContainerManifestVolumeUnit { get; set; }
		public ZDecimal ContainerTotalManifestVolume { get; set; }
		public ZString ContainerTotalManifestVolumeUnit { get; set; }
		public ZInt ContainerManifestPackage { get; set; }
		public ZString ContainerManifestPackType { get; set; }
		public ZString ContainerManifestMarksAndNumbers { get; set; }
		public ZString PackageDetails { get; set; }

		#region Dimensions and Outturned Dimensions

		public ZString FormattedWeightAndUnit
		{
			get { return (ActualWeight.IsEmpty) ? "" : FormatNumber(ActualWeight) + " " + ActualWeightUQ; }
		}

		public ZString FormattedVolumeAndUnit
		{
			get { return (ActualVolume.IsEmpty) ? "" : FormatNumber(ActualVolume) + " " + ActualVolumeUQ; }
		}

		public ZString FormattedLength
		{
			get { return (Length.IsEmpty) ? ZString.Empty : FormatNumber(Length); }
		}

		public ZString FormattedWidth
		{
			get { return (Width.IsEmpty) ? ZString.Empty : FormatNumber(Width); }
		}

		public ZString FormattedHeight
		{
			get { return (Height.IsEmpty) ? ZString.Empty : FormatNumber(Height); }
		}

		public ZString DimensionUnit
		{
			get { return (!Height.IsEmpty || !Length.IsEmpty || !Width.IsEmpty) ? UnitOfDimension : ZString.Empty; }
		}

		public ZString Remarks
		{
			get { return (OutturnedOrActualDimensions + System.Environment.NewLine + OutturnComments).Trim(); }
		}

		public ZString OutturnedOrActualDimensions
		{
			get { return (PackLine.JL_Outturn > 0 ? OutturnedDimensions : Dimensions); }
		}

		public ZString OutturnedDimensions
		{
			get { return GetFormattedDimensions(OutturnedLength, OutturnedWidth, OutturnedHeight); }
		}

		public ZString DimensionsInIN
		{
			get
			{
				ZDecimal lengthIN = ZArchitecture.Core.Utilities.Round(Core.Constants.Length.ConvertSafe(Length, LengthUnit, Core.Constants.Length.Inches), 0);
				ZDecimal widthIN = ZArchitecture.Core.Utilities.Round(Core.Constants.Length.ConvertSafe(Width, WidthUnit, Core.Constants.Length.Inches), 0);
				ZDecimal heightIN = ZArchitecture.Core.Utilities.Round(Core.Constants.Length.ConvertSafe(Height, HeightUnit, Core.Constants.Length.Inches), 0);

				ZString result = "";

				result += FormatNumber(lengthIN);
				result += (NoResString)"x" + FormatNumber(widthIN);
				result += (NoResString)"x" + FormatNumber(heightIN);

				return result;
			}
		}

		public ZString Dimensions
		{
			get { return GetFormattedDimensions(Length, Width, Height); }
		}

		ZString GetFormattedDimensions(ZDecimal length, ZDecimal width, ZDecimal height)
		{
			ZString result = "";

			if (!length.IsEmpty || !width.IsEmpty || !height.IsEmpty)
			{
				result += "(L):" + " " + FormatNumber(length) + "  ";
				result += "(W):" + " " + FormatNumber(width) + "  ";
				result += "(H):" + " " + FormatNumber(height) + " " + UnitOfDimension;
			}

			return result;
		}

		#endregion

		internal bool IsHazardous
		{
			get { return (Commodity != null && Commodity.Code == Core.Constants.CargoTypes.Hazardous) || UNDGs.Length > 0; }
		}

		public ZString HazardousDescription
		{
			get { return HazardousDescriptionWithoutWeight + " - " + ActualWeight.ToStringTrimZeros() + " " + ActualWeightUQ; }
		}

		public ZString HazardousDescriptionWithoutWeight
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();

				if (UNDGs.Length == 0)
				{
					builder.Append(GetHazardousDescriptionWithoutWeight(null));
				}
				else
				{
					foreach (UNDGSubstanceWrapper undg in UNDGs)
					{
						builder.Append(GetHazardousDescriptionWithoutWeight(undg));
					}

					var helper = new UNDGSubstanceWrapperHelper();
					var summary = helper.GetUNDGPackagesSummary(UNDGs);
					builder.AppendIfNotEmpty(summary);
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		internal ZString GetHazardousDescriptionWithoutWeight(UNDGSubstanceWrapper dG)
		{
			List<ZString> resultList = new List<ZString>();
			resultList.Add((Commodity != null) ? Commodity.Code : ZString.Empty);
			if (dG != null)
			{
				resultList.Add(dG.Summary);
			}

			return ListToString(resultList, " - ");
		}

		ZString ListToString(List<ZString> list, ZString delimiter)
		{
			ZString result = "";
			foreach (ZString part in list)
			{
				if (!result.Trim().IsEmpty && !part.Trim().IsEmpty)
				{
					result += delimiter;
				}

				result += part.Trim();
			}
			return result;
		}

		public ZString HandlingInstructions
		{
			get
			{
				ZString result = IsGroupPackLine ? PackageDetails : ZString.Empty;

				if (Shipment != null && !Shipment.HandlingInstructions.IsEmpty)
				{
					result += result.IsEmpty ? "" : "\n";
					result += Shipment.HandlingInstructions;
				}
				return result;
			}
		}

		public ZString PackageDetailsAndHandlingInstructionNote
		{
			get
			{
				ZString result = IsGroupPackLine ? PackageDetails : Dimensions;

				if (Shipment != null && !Shipment.HandlingInstructions.IsEmpty)
				{
					result += result.IsEmpty ? "" : "\n";
					result += Shipment.HandlingInstructions;
				}
				return result;
			}
		}

		public ZBool IsGroupPackLine { get; set; }

		public ZString PacksAndLocationLine
		{
			get
			{
				ZString result = "";
				foreach (PackLocation location in PackLine.PackLocations)
				{
					result += location.JQ_NoPackages + " " + PackType + " " + location.JQ_WarehouseLocation;
					result += ",   ";
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString ContainerNumberForLoadList { get; set; }

		public ZString WeightVolumeAndPacks
		{
			get
			{
				return FormatNumber(ActualWeight) + " " + ActualWeightUQ + "\n" +
					FormatNumber(ActualVolume) + " " + ActualVolumeUQ + "\n" +
					PackageCount.ToString() + " " + PackTypeDescription;
			}
		}

		public ZString ContainerManifestWeightVolumeAndPacks
		{
			get
			{
				return ZString.Format("{0} {1}\n{2} {3}\n{4} {5}",
					FormatNumber(ContainerManifestWeight), ContainerManifestWeightUnit,
					FormatNumber(ContainerManifestVolume), ContainerManifestVolumeUnit,
					ContainerManifestPackage, ContainerManifestPackType);
			}
		}

		#endregion

		#region ZString Fields

		public ZString LengthUnit
		{
			get { return PackLine.JL_UnitOfDimension; }
		}

		public ZString HeightUnit
		{
			get { return PackLine.JL_Calc_HeightUnit; }
		}

		public ZString WidthUnit
		{
			get { return PackLine.JL_Calc_WidthUnit; }
		}

		public ZString ActualVolumeUQ
		{
			get { return PackLine.JL_ActualVolumeUQ; }
		}

		public ZString ActualWeightUQ
		{
			get { return PackLine.JL_ActualWeightUQ; }
		}

		public ZString Description
		{
			get { return PackLine.JL_Description; }
		}

		public ZString DetailedDescription
		{
			get { return PackLine.JL_DetailedDescription; }
		}

		public ZString FreightMode
		{
			get { return PackLine.JL_FreightMode; }
		}

		public ZString PackType
		{
			get { return PackLine.JL_F3_NKPackType; }
		}

		public ZString PackTypeDescription
		{
			get { return PackLine.JL_F3_NKPackType_List.GetDescriptionFromCode(PackType); }
		}

		public ZString PackTypeDescriptionPlural
		{
			get
			{
				var type = PackLine.JL_F3_NKPackType_List.GetDescriptionFromCode(PackType);
				return (type != null) ? type.ToString() + Res.GetString("c0af9134-1b37-4ef2-8feb-bfeaed4fe8f2", "(s)") : "";
			}
		}

		public ZString Consignee
		{
			get { return (PackLine.CalcConsignee != null) ? PackLine.CalcConsignee.OH_FullName : ZString.Empty; }
		}

		public ZString Consignor
		{
			get { return (PackLine.CalcConsignor != null) ? PackLine.CalcConsignor.OH_FullName : ZString.Empty; }
		}

		public ZString ShipmentNumber
		{
			get { return PackLine.JL_Calc_JS_UniqueConsignRef; }
		}

		public ZString HouseBill
		{
			get { return PackLine.JL_JS_HouseBill; }
		}

		public ZString GoodsDescription
		{
			get { return PackLine.JL_Calc_JS_GoodsDescription; }
		}

		public ZString MarksAndNumbers
		{
			get
			{
				ZString marksAndNumbers = PackLine.JL_MarksAndNumbers;

				if (marksAndNumbers.IsEmpty)
				{
					marksAndNumbers = PackLine.JL_Calc_JS_MarksAndNumbers;
				}

				return marksAndNumbers;
			}
		}

		public ZString Destination
		{
			get { return PackLine.JL_Calc_JS_Destination; }
		}

		public ZString OutturnComments
		{
			get { return PackLine.JL_OutturnComment.Trim(); }
		}

		public ZString RefNumber
		{
			get { return PackLine.JL_RefNumber; }
		}
		public ZString UnitOfDimension
		{
			get { return PackLine.JL_UnitOfDimension; }
		}

		public ZString WeightUQ
		{
			get { return ActualWeightUQ; }
		}

		public ZString VolumeUQ
		{
			get { return ActualVolumeUQ; }
		}

		public ZString CustomAttribute1
		{
			get { return PackLine.JL_CustomAttrib1; }
		}

		public ZString CustomAttribute2
		{
			get { return PackLine.JL_CustomAttrib2; }
		}

		public ZString CustomAttribute3
		{
			get { return PackLine.JL_CustomAttrib3; }
		}

		public ZString CustomAttribute4
		{
			get { return PackLine.JL_CustomAttrib4; }
		}

		public ZString VehicleColour
		{
			get { return ZString.Empty; }
		}

		public ZString VehicleMake
		{
			get { return ZString.Empty; }
		}

		public ZString VehicleModel
		{
			get { return ZString.Empty; }
		}

		public ZString VehicleTransmission
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal LengthInIN
		{
			get
			{
				return Core.Constants.Length.ConvertSafe(Length, LengthUnit, Core.Constants.Length.Inches);
			}
		}

		public ZDecimal WidthInIN
		{
			get
			{
				return Core.Constants.Length.ConvertSafe(Width, WidthUnit, Core.Constants.Length.Inches);
			}
		}

		public ZDecimal HeightInIN
		{
			get
			{
				return Core.Constants.Length.ConvertSafe(Height, HeightUnit, Core.Constants.Length.Inches);
			}
		}

		public ZDecimal ActualWeightLB
		{
			get
			{
				return ZArchitecture.Core.Utilities.Round(Core.Constants.Weight.ConvertSafe(ActualWeight, ActualWeightUQ, "LB"), 2);
			}
		}

		public ZDecimal ChargeableWeightLB
		{
			get { return ActualWeightLB > ActualVolumeWeightLB ? ActualWeightLB : ActualVolumeWeightLB; }
		}

		public ZDecimal ActualVolumeWeightLB
		{
			get
			{
				return ZArchitecture.Core.Utilities.Round(Core.Constants.Volume.ConvertSafe(ActualVolume, ActualVolumeUQ, Core.Constants.Volume.CubicInches) / 194, 2);
			}
		}

		public ZDecimal ActualVolume
		{
			get { return PackLine.JL_ActualVolume; }
		}

		public ZDecimal ActualWeight
		{
			get { return PackLine.JL_ActualWeight; }
		}

		public ZDecimal Height
		{
			get { return PackLine.JL_Height; }
		}

		public ZDecimal OutturnedHeight
		{
			get { return PackLine.JL_OutturnedHeight; }
		}

		public ZDecimal OutturnedWeight
		{
			get { return PackLine.JL_OutturnedWeight; }
		}
		public ZDecimal OutturnedVolume
		{
			get { return PackLine.JL_OutturnedVolume; }
		}

		public ZDecimal Length
		{
			get { return PackLine.JL_Length; }
		}

		public ZDecimal Width
		{
			get { return PackLine.JL_Width; }
		}

		public ZDecimal OutturnedLength
		{
			get { return PackLine.JL_OutturnedLength; }
		}

		public ZDecimal OutturnedWidth
		{
			get { return PackLine.JL_OutturnedWidth; }
		}

		#endregion

		#region ZInt Fields

		public ZInt Damaged
		{
			get { return PackLine.JL_Damaged; }
		}

		public ZInt Outturn
		{
			get { return PackLine.JL_Outturn; }
		}

		public ZInt PackageCount
		{
			get { return PackLine.JL_PackageCount; }
		}

		public ZInt Pillaged
		{
			get { return PackLine.JL_Pillaged; }
		}

		public ZInt PackagesDelivered
		{
			get { return PackLine.PackagesConfirmed_DeliveredToConsignee; }
		}

		public ZInt Surplus
		{
			get { return PackLine.JL_Calc_Surplus; }
		}

		public ZInt Short
		{
			get { return PackLine.JL_Calc_Shortlanded; }
		}

		public ZInt ContainerPackingOrder
		{
			get
			{
				return PackLine.JL_ContainerPackingOrder;
			}
		}

		#endregion

		#region ZShort Fields

		public ZShort EndItemNo
		{
			get { return PackLine.JL_EndItemNo; }
		}

		public ZShort ItemNo
		{
			get { return PackLine.JL_ItemNo; }
		}

		public ZShort VehicleYear
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region ZByte Fields

		public ZByte VehicleNumberOfDoors
		{
			get { return ZByte.Zero; }
		}

		#endregion

		#region Related Objects

		public DocShipment Shipment
		{
			get { return (shipmentBizObj ?? PackLine.Shipment) != null ? DocShipment.New((shipmentBizObj ?? PackLine.Shipment), PackLine.Factory) : null; }
		}
		readonly CommonShipment shipmentBizObj;

		public DocContainer GetContainerOnConsol(DocBaseConsol consol)
		{
			if (consol != null)
			{
				ForwardingConsol consolBizo = Factory.Load<ForwardingConsol>(consol.ConsolPK);
				CommonContainer container = PackLine.GetContainer(consolBizo);
				return DocContainer.New(container, PackLine.Shipment, PackLine.Factory);
			}

			return null;
		}

		public DocCommodity Commodity
		{
			get { return DocCommodity.New(PackLine.Factory, PackLine.JL_RH_NKCommodityCode); }
		}

		/// <summary>
		/// Obsolete - use UNDGs instead
		/// </summary>
		public UNDGSubstanceWrapper UNDG
		{
			get { return UNDGs.Length > 0 ? UNDGs[0] : null; }
		}

		/// <summary>
		/// Obsolete - use UNDGs instead
		/// </summary>
		public ZString UNDGNumber
		{
			get { return UNDG != null ? UNDG.UNNumber : ZString.Empty; }
		}

		internal UNDGSubstanceWrapper[] UNDGs
		{
			get
			{
				List<UNDGSubstanceWrapper> result = new List<UNDGSubstanceWrapper>();
				foreach (UNDGDataItem dg in PackLine.UNDGs)
				{
					UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(dg, Factory);
					result.Add(wrapper);
				}

				return result.ToArray();
			}
		}

		public DocContainer ShipmentConsolContainer
		{
			get
			{
				if (Shipment != null && Shipment.Consol != null)
				{
					var consol = Factory.Load<ForwardingConsol>(Shipment.Consol.ConsolPK);
					return DocContainer.New(PackLine.GetContainer(consol), PackLine.Shipment, PackLine.Factory);
				}
				return null;
			}
		}

		public DocContainer Container
		{
			get
			{
				DocContainer result = null;

				if (Shipment != null)
				{
					CommonConsol consol;

					if (CurrentConsol != null)
					{
						consol = (CommonConsol)CurrentConsol.WrappedObject;
						result = DocContainer.New(PackLine.GetContainer(consol), PackLine.Shipment, PackLine.Factory);
					}
					else
					{
						result = DocContainer.New((CommonContainer)PackLine.Containers.FindByPK(PackLine.JL_JC), PackLine.Shipment, Factory);
					}
				}

				return result;
			}
		}

		public DocBaseConsol CurrentConsol { get; set; }

		public DocPackLocationCollection PackLocationCollection
		{
			get { return new DocPackLocationCollection(PackLine.PackLocations, Factory); }
		}

		internal PackLocationCollection PackLocations
		{
			get { return PackLine.PackLocations; }
		}

		#endregion

		#region Calculated Fields

		public ZString ContainerNumber
		{
			get { return (Container != null) ? Container.ContainerNumber : ZString.Empty; }
		}

		public ZString SealNumber
		{
			get { return (Container != null) ? Container.SealNumber : ZString.Empty; }
		}

		public ZString ContainerType
		{
			get { return (Container != null) ? Container.ContainerType : ZString.Empty; }
		}

		public ZString ContainerNum
		{
			get { return PackLine.JL_Calc_FirstImportContainerNum; }
		}

		public DocContainer FirstImportContainer
		{
			get
			{
				ZGuid containerPK = PackLine.JL_Calc_FirstImportContainer != null ? PackLine.JL_Calc_FirstImportContainer.PK : ZGuid.Empty;
				return DocContainer.New(Factory, containerPK);
			}
		}

		public DocContainer FirstExportContainer
		{
			get
			{
				ZGuid containerPK = PackLine.JL_Calc_FirstExportContainer != null ? PackLine.JL_Calc_FirstExportContainer.PK : ZGuid.Empty;
				return DocContainer.New(Factory, containerPK);
			}
		}

		public DocContainer ContainerForLoadList { get; set; }

		public ZString FirstExportContainerNumber
		{
			get { return (FirstExportContainer != null) ? FirstExportContainer.ContainerNumber : ZString.Empty; }
		}

		public ZDecimal ConvertedWeight
		{
			get
			{
				if (!ActualWeightUQ.IsEmpty && ActualWeightUQ != Env.Registry.FreightWeightUnit)
				{
					return Core.Constants.Weight.ConvertSafe(ActualWeight, ActualWeightUQ, Env.Registry.FreightWeightUnit);
				}
				else
				{
					return ActualWeight;
				}
			}
		}

		public ZDecimal ConvertedVolume
		{
			get
			{
				if (!ActualVolumeUQ.IsEmpty && ActualVolumeUQ != Env.Registry.FreightVolumeUnit)
				{
					return Core.Constants.Volume.ConvertSafe(ActualVolume, ActualVolumeUQ, Env.Registry.FreightVolumeUnit);
				}
				else
				{
					return ActualVolume;
				}
			}
		}

		#endregion

		#region IDocPackageDetails Members

		ZString IDocPackageDetails.ReferenceNumber
		{
			get { return RefNumber; }
		}

		ZInt IDocPackageDetails.Count
		{
			get { return PackageCount; }
		}

		ZString IDocPackageDetails.PackType
		{
			get { return PackType; }
		}

		ZString IDocPackageDetails.Description
		{
			get { return Description; }
		}

		ZString IDocPackageDetails.DetailedDescription
		{
			get { return DetailedDescription; }
		}

		ZString IDocPackageDetails.MarksAndNumbers
		{
			get { return MarksAndNumbers; }
		}

		ZDecimal IDocPackageDetails.Weight
		{
			get { return ActualWeight; }
		}

		ZString IDocPackageDetails.WeightUnit
		{
			get { return ActualWeightUQ; }
		}

		ZDecimal IDocPackageDetails.Volume
		{
			get { return ActualVolume; }
		}

		ZString IDocPackageDetails.VolumeUnit
		{
			get { return ActualVolumeUQ; }
		}

		ZDecimal IDocPackageDetails.Length
		{
			get { return Length; }
		}

		ZDecimal IDocPackageDetails.Height
		{
			get { return Height; }
		}

		ZDecimal IDocPackageDetails.Width
		{
			get { return Width; }
		}

		ZString IDocPackageDetails.DimensionUnit
		{
			get { return UnitOfDimension; }
		}

		DocCommodity IDocPackageDetails.Commodity
		{
			get { return Commodity; }
		}

		ZString IDocPackageDetails.HarmonisedCode
		{
			get { return PackLine.JL_HarmonisedCode; }
		}

		UNDGSubstanceWrapper[] IDocPackageDetails.UNDGs
		{
			get { return UNDGs; }
		}

		#endregion
	}
}
