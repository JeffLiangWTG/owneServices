using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRContainerUtilities
	{
		public ZString GetContainerSizeCode(ZDecimal height, ZDecimal width, ZDecimal length)
		{
			ZString result = CMRContainerSizes.Codes.Other;

			if (width == 8)
			{
				if (length == 20)
				{
					if (height == 9.5m)
					{
						result = CMRContainerSizes.Codes._20X8X95;
					}
					else
					{
						result = CMRContainerSizes.Codes._20X8X8;
					}
				}
				else if (length == 40 && height == 8)
				{
					result = CMRContainerSizes.Codes._40X8X8;
				}
			}

			return result;
		}

		public ZString GetContainerSizeCodeFromISOCode(ZString iSOCode)
		{
			ContainerISOType iSOType = new ContainerISOType();
			iSOType.ISOCode = iSOCode;
			return GetContainerSizeCode(ZDecimal.ParseSafe(iSOType.HeightInFeet, 0), ZDecimal.ParseSafe(iSOType.WidthInFeet, 0), ZDecimal.ParseSafe(iSOType.LengthInFeet, 0));
		}

		public ZString GetContainerSizeCode(RefContainer container)
		{
			ZString result = CMRContainerSizes.Codes.Other;

			if (container != null)
			{
				result = GetContainerSizeCode(container.RC_Height, container.RC_Width, container.RC_Length);
			}

			return result;
		}

		public ZString GetContainerTypeCodeFromISOCode(ZString iSOCode)
		{
			ContainerISOType iSOType = new ContainerISOType();
			iSOType.ISOCode = iSOCode;

			ZString result = CMRContainerTypes.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo;

			switch (iSOType.GroupCode)
			{
				case "GP":
				case "BU":
				case "BH":
				case "BK":
				case "SN":
				case "HI":
					result = CMRContainerTypes.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo;
					break;
				case "VN":
					result = CMRContainerTypes.Codes.GeneralPurposeVentedAVentilatedContainerUsedToTransportCargo;
					break;
				case "RS":
				case "HR":
				case "RE":
				case "RT":
					result = CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo;
					break;
				case "UT":
					result = CMRContainerTypes.Codes.OpenTopAContainerWithNoHardTopUsedToTransportCargoThatWouldNotNormallyFitInsideAConventionalContainer;
					break;
				case "PL":
				case "PF":
				case "PC":
				case "PS":
					result = CMRContainerTypes.Codes.FlatRackCargoSecuredOntoAFlatBaseForEaseOfLoadingAndDischarge;
					break;
				case "TN":
				case "TD":
				case "TG":
					result = CMRContainerTypes.Codes.TankATypeOfVesselUsedToTransportLiquidCargo;
					break;
			}

			return result;
		}

		public ZString GetContainerTypeCode(RefContainer containerType)
		{
			ZString result = ZString.Empty;
			if (containerType != null)
			{
				switch (containerType.RC_ContainerType)
				{
					case Core.Constants.ContainerTypes.Refrigerated:
						result = CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo;
						break;
					case Core.Constants.ContainerTypes.OpenTop:
						result = CMRContainerTypes.Codes.OpenTopAContainerWithNoHardTopUsedToTransportCargoThatWouldNotNormallyFitInsideAConventionalContainer;
						break;
					case Core.Constants.ContainerTypes.FlatRack:
						result = CMRContainerTypes.Codes.FlatRackCargoSecuredOntoAFlatBaseForEaseOfLoadingAndDischarge;
						break;
					case Core.Constants.ContainerTypes.Tank:
						result = CMRContainerTypes.Codes.TankATypeOfVesselUsedToTransportLiquidCargo;
						break;
					case Core.Constants.ContainerTypes.MAFI:
						result = CMRContainerTypes.Codes.MafiATypeOfWheeledTrailerOntoWhichCargoIsStrappedForTransportOnAVessel;
						break;
					case Core.Constants.ContainerTypes.Bolster:
					case Core.Constants.ContainerTypes.DryStorage:
					case Core.Constants.ContainerTypes.Other:
						if (containerType.RC_HasVents)
						{
							result = CMRContainerTypes.Codes.GeneralPurposeVentedAVentilatedContainerUsedToTransportCargo;
						}
						else
						{
							result = CMRContainerTypes.Codes.GeneralPurposeNonVentedANonVentilatedContainerUsedToTransportCargo;
						}

						break;
				}
			}
			return result;
		}

		public ZString GetContainerTypeMappingOrOriginal(ZString code)
		{
			CMRContainerTypesMapping mapping = new CMRContainerTypesMapping();
			string mapped = mapping.GetDescriptionFromCode(code);
			return (mapped ?? (string)code);
		}

		public ZString GetContainerTypeMappingCodeOrEmpty(ZString description)
		{
			CMRContainerTypesMapping mapping = new CMRContainerTypesMapping();
			return new ZString(mapping.GetCodeFromDescription(description));
		}
	}
}
