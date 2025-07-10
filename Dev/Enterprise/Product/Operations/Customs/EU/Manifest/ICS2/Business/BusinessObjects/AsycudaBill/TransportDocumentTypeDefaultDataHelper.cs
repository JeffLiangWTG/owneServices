using System.Linq;
using CargoWise.Types;
using TransportDocumentTypes = Enterprise.Customs.ASYCUDA.Business.TransportDocumentTypes;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public static class TransportDocumentTypeDefaultDataHelper
	{
		public static void DefaultTransportDocumentType(this AsycudaBill bill, ZString transportMode, ZString specificCircumstanceIndicator, ZString agentType)
		{
			var defaultTransportDocumentType = string.Empty;
			if (!transportMode.IsEmpty && !specificCircumstanceIndicator.IsEmpty)
			{
				var defaultDataMatchKey = transportMode + specificCircumstanceIndicator.SubstringSafe(0, 2) + (bill.IsChildMasterBill ? masterLevelBill : houseLevelBill);
				switch (defaultDataMatchKey)
				{
					case Core.Constants.TransportModes.Air + airSpecificCircumstanceIndicatorPrefix + masterLevelBill:
						if (agentType == Core.Constants.AgentType.Direct)
						{
							defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N740;
						}
						else
						{
							defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N741;
						}
						break;
					case Core.Constants.TransportModes.Air + airSpecificCircumstanceIndicatorPrefix + houseLevelBill:
						defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N703;
						break;
					case Core.Constants.TransportModes.Sea + seaSpecificCircumstanceIndicatorPrefix + masterLevelBill:
						if (IsN705DocType(specificCircumstanceIndicator))
						{
							defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N705;
						}
						else if(IsN704DocType(specificCircumstanceIndicator))
						{
							defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N704;
						}
						break;
					case Core.Constants.TransportModes.Sea + seaSpecificCircumstanceIndicatorPrefix + houseLevelBill:
						defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N714;
						break;
					case Core.Constants.TransportModes.Road + roadSpecificCircumstanceIndicatorPrefix + masterLevelBill:
						defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N722;
						break;
					case Core.Constants.TransportModes.Road + roadSpecificCircumstanceIndicatorPrefix + houseLevelBill:
						defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N730;
						break;
					case Core.Constants.TransportModes.InlandWaterwayTransport + seaSpecificCircumstanceIndicatorPrefix + masterLevelBill:
						defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_C625;
						break;
					case Core.Constants.TransportModes.Mail + mailSpecificCircumstanceIndicatorPrefix + houseLevelBill:
						defaultTransportDocumentType = TransportDocumentTypes.Codes.CL754_N750;
						break;
				}
			}
			bill.TransportDocumentType = defaultTransportDocumentType;
		}

		static bool IsN704DocType(ZString specificCircumstanceIndicator) => new[]
			{
				EUICS2SpecificCircumstanceList.Codes.F11, EUICS2SpecificCircumstanceList.Codes.F12, EUICS2SpecificCircumstanceList.Codes.F14,
				EUICS2SpecificCircumstanceList.Codes.F15, EUICS2SpecificCircumstanceList.Codes.F16,
			}.Contains((string)specificCircumstanceIndicator);

		static bool IsN705DocType(ZString specificCircumstanceIndicator) => new []
				{
					EUICS2SpecificCircumstanceList.Codes.F10, EUICS2SpecificCircumstanceList.Codes.F13, EUICS2SpecificCircumstanceList.Codes.F17
				}.Contains((string)specificCircumstanceIndicator);

		const string masterLevelBill = "MasterLevelBill";
		const string houseLevelBill = "HouseLevelBill";
		const string seaSpecificCircumstanceIndicatorPrefix = "F1";
		const string airSpecificCircumstanceIndicatorPrefix = "F2";
		const string mailSpecificCircumstanceIndicatorPrefix = "F4";
		const string roadSpecificCircumstanceIndicatorPrefix = "F5";
	}
}
