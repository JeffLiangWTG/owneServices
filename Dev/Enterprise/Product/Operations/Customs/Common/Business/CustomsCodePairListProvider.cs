using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class CustomsCodePairListProvider : Integration.Customs.ICustomsCodePairListProvider
	{
		public ICodeDescriptionPairList GetContainerModeList(ZString transportMode)
		{
			var sgContainerModeList = new SG.CargoPackingTypeCodeList();
			sgContainerModeList.AddRange(new SG.CargoPackingCodeList());

			var result = new CodeDescriptionPairList();
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					result.AddPair(Core.Constants.ContainerModes.AIR, Core.Constants.ContainerModeDescriptions.AIR);
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(TW.ContainerModeList.Codes.Express, TW.ContainerModeList.Descriptions.Express);
					result.AddPair(TW.ContainerModeList.Codes.OwnPropulsion, TW.ContainerModeList.Descriptions.OwnPropulsion);
					result.AddPair(TW.ContainerModeList.Codes.Mail, TW.ContainerModeList.Descriptions.Mail);
					result.AddPair(TW.ContainerModeList.Codes.HandCarry, TW.ContainerModeList.Descriptions.HandCarry);
					result.AddPair(TW.ContainerModeList.Codes.FixedTransportInstallation, TW.ContainerModeList.Descriptions.FixedTransportInstallation);
					result.AddPair(TW.ContainerModeList.Codes.Pipeline, TW.ContainerModeList.Descriptions.Pipeline);
					result.AddPair(TW.ContainerModeList.Codes.PowerLine, TW.ContainerModeList.Descriptions.PowerLine);
					result.AddPair(TW.ContainerModeList.Codes.Other, TW.ContainerModeList.Descriptions.Other);
					result.AddRange(sgContainerModeList);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.FixedTransportInstallations:
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.InlandWaterwayTransport:
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.OwnPropulsion:
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.Mail:
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddRange(sgContainerModeList);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.Rail:
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddRange(sgContainerModeList);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.Road:
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddRange(sgContainerModeList);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;

				case Core.Constants.TransportModes.Sea:
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Combination, Core.Constants.ContainerModeDescriptions.Combination);
					result.AddPair(Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModeDescriptions.FCLMixedShipper);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(TW.ContainerModeList.Codes.Express, TW.ContainerModeList.Descriptions.Express);
					result.AddPair(TW.ContainerModeList.Codes.OwnPropulsion, TW.ContainerModeList.Descriptions.OwnPropulsion);
					result.AddPair(TW.ContainerModeList.Codes.Mail, TW.ContainerModeList.Descriptions.Mail);
					result.AddPair(TW.ContainerModeList.Codes.HandCarry, TW.ContainerModeList.Descriptions.HandCarry);
					result.AddPair(TW.ContainerModeList.Codes.FixedTransportInstallation, TW.ContainerModeList.Descriptions.FixedTransportInstallation);
					result.AddPair(TW.ContainerModeList.Codes.Pipeline, TW.ContainerModeList.Descriptions.Pipeline);
					result.AddPair(TW.ContainerModeList.Codes.PowerLine, TW.ContainerModeList.Descriptions.PowerLine);
					result.AddPair(TW.ContainerModeList.Codes.Other, TW.ContainerModeList.Descriptions.Other);
					result.AddRange(sgContainerModeList);
					result.AddPair(IN.INContainerModeList.Codes.ContainerisedAndPackaged, IN.INContainerModeList.Descriptions.ContainerisedAndPackaged);
					break;
				case Core.Constants.TransportModes.Unknown:
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.Combination, Core.Constants.ContainerModeDescriptions.Combination);
					result.AddPair(Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModeDescriptions.FCLMixedShipper);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					result.AddPair(TW.ContainerModeList.Codes.Express, TW.ContainerModeList.Descriptions.Express);
					result.AddPair(TW.ContainerModeList.Codes.OwnPropulsion, TW.ContainerModeList.Descriptions.OwnPropulsion);
					result.AddPair(TW.ContainerModeList.Codes.Mail, TW.ContainerModeList.Descriptions.Mail);
					result.AddPair(TW.ContainerModeList.Codes.HandCarry, TW.ContainerModeList.Descriptions.HandCarry);
					result.AddPair(TW.ContainerModeList.Codes.FixedTransportInstallation, TW.ContainerModeList.Descriptions.FixedTransportInstallation);
					result.AddPair(TW.ContainerModeList.Codes.Pipeline, TW.ContainerModeList.Descriptions.Pipeline);
					result.AddPair(TW.ContainerModeList.Codes.PowerLine, TW.ContainerModeList.Descriptions.PowerLine);
					result.AddPair(TW.ContainerModeList.Codes.Other, TW.ContainerModeList.Descriptions.Other);
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddRange(sgContainerModeList);
					break;

				case Core.Constants.TransportModes.Other:
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Core.Constants.ContainerModes.FCLMixedShipper, Core.Constants.ContainerModes.FCLMixedShipper);
					result.AddPair(Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddRange(sgContainerModeList);
					break;

				default:
					result.AddPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised);
					result.AddPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Core.Constants.ContainerModes.NonContainerised, Core.Constants.ContainerModeDescriptions.NonContainerised);
					break;
			}

			return result;
		}
	}
}
