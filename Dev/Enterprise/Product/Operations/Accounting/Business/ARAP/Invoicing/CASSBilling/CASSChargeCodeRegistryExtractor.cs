using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class CASSChargeCodeRegistryExtractor
	{
		public static CodeDescriptionPairList CASSExportCostComponents
		{
			get
			{
				return CASSChargeCodeLookups.CASSExportLineComponentList;
			}
		}

		public static CodeDescriptionPairList CASSImportCostComponents
		{
			get
			{
				return CASSChargeCodeLookups.CASSImportLineComponentList;
			}
		}

		public static ZGuid[] GetChargeCodesByCASSCostComponent(string cassType, string cassComponent)
		{
			ZGuid[]  chargeCodePKs = null;

			bool isValidComponent = (cassType == CASSChargeCodeLookups.CASSTypes.Export.Code && CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(cassComponent))
						  || (cassType == CASSChargeCodeLookups.CASSTypes.Import.Code && CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(cassComponent));

			if (isValidComponent)
			{
				chargeCodePKs = AccountingConfigurationRegistry.Instance.CASSChargeCodes.Value.Cast<CASSChargeCode>().Where(x => (x.CASSType.Equals(cassType) || x.CASSType.Equals(CASSChargeCodeLookups.ALL)) && (x.CASSComponentCode.Equals(cassComponent) || x.CASSComponentCode.Equals(CASSChargeCodeLookups.ALL))).Select(x => x.ChargeCodePK).Distinct().ToArray();
			}

			return chargeCodePKs;
		}

		public static ZString[] GetCASSCompnentsByChargeCode(string cassType, ZGuid chargeCodePK)
		{
			var regValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.Value;
			var cassComponents = regValue.Cast<CASSChargeCode>().Where(x => (x.CASSType.Equals(cassType) || x.CASSType.Equals(CASSChargeCodeLookups.ALL)) && x.ChargeCodePK.Equals(chargeCodePK)).Select(x => x.CASSComponentCode).Distinct().ToList();

			if (cassComponents.Contains(CASSChargeCodeLookups.ALL))
			{
				cassComponents.Remove(CASSChargeCodeLookups.ALL);

				if (cassType == CASSChargeCodeLookups.CASSTypes.Export.Code)
				{
					cassComponents.AddRange(CASSChargeCodeLookups.CASSExportLineComponentList.Cast<CodeDescriptionPair>().Select(x => new ZString(x.Code)));
				}
				else if (cassType == CASSChargeCodeLookups.CASSTypes.Import.Code)
				{
					cassComponents.AddRange(CASSChargeCodeLookups.CASSImportLineComponentList.Cast<CodeDescriptionPair>().Select(x => new ZString(x.Code)));
				}
			}

			return cassComponents.Distinct().ToArray();
		}

		public static ZGuid[] GetAllChargeCodePksByCASSType(string cassType)
		{
			var regValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.Value;
			var chargeCodePKs = regValue.Cast<CASSChargeCode>().Where(x => x.CASSType.Equals(cassType) || x.CASSType.Equals(CASSChargeCodeLookups.ALL)).Select(x => x.ChargeCodePK).Distinct().ToArray();
			return chargeCodePKs;
		}
	}
}
