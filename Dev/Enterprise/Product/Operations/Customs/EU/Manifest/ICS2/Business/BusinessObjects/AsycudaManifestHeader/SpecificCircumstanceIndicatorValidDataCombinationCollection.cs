using System.Collections.Generic;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	class SpecificCircumstanceIndicatorValidDataCombinationCollection
	{
		public static Dictionary<string, SpecificCircumstanceIndicatorValidDataCombination> GetValidSpecificCircumstanceIndicatorDataDictionary()
		{
			return new Dictionary<string, SpecificCircumstanceIndicatorValidDataCombination>()
			{
				{ EUICS2SpecificCircumstanceList.Codes.F10, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F11, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F12, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F13, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F14, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F15, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F16, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F17, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Sea,TransportModes.InlandWaterwayTransport }) },
				{ EUICS2SpecificCircumstanceList.Codes.F20, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F21, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F22, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F23, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F24, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F25, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F26, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F27, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F28, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F29, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F30, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F31, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F32, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F33, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F34, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Road }) },
				{ EUICS2SpecificCircumstanceList.Codes.F40, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Mail, TransportModes.Road }) },
				{ EUICS2SpecificCircumstanceList.Codes.F41, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Mail, TransportModes.Rail }) },
				{ EUICS2SpecificCircumstanceList.Codes.F42, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Mail }) },
				{ EUICS2SpecificCircumstanceList.Codes.F43, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Rail, TransportModes.Road, TransportModes.Sea, TransportModes.InlandWaterwayTransport, TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F44, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Rail, TransportModes.Road, TransportModes.Sea, TransportModes.InlandWaterwayTransport, TransportModes.Air }) },
				{ EUICS2SpecificCircumstanceList.Codes.F45, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.Consolidator, new string[] { TransportModes.Mail }) },
				{ EUICS2SpecificCircumstanceList.Codes.F50, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Road }) },
				{ EUICS2SpecificCircumstanceList.Codes.F51, new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Rail }) }
			};
		}
	}
}
