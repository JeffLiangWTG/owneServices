using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public static class FuncsHelper
	{
		public enum ValidationOptions
		{
			UseSpecificDataGroupingOnly,
			UseCountryDefinitionFirstOtherwiseEUN
		}

		const string EunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		public static bool IsFunctionalityValid(
			string code,
			ZDateTime effectiveDate,
			string dataGroupingCode = null,
			ValidationOptions options = ValidationOptions.UseSpecificDataGroupingOnly,
			bool priorityToPilotFunctionality = false)
		{
			var result = false;

			if (!string.IsNullOrEmpty(code))
			{
				if (effectiveDate.IsEmpty)
				{
					effectiveDate = ZDateTime.Today;
				}

				switch (options)
				{
					case ValidationOptions.UseSpecificDataGroupingOnly:
						if (string.IsNullOrEmpty(dataGroupingCode))
						{
							dataGroupingCode = EunCode;
						}
						result = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(code, dataGroupingCode, effectiveDate, priorityToPilotFunctionality);
						break;
					case ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN:
						result = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(code, priorityToPilotFunctionality)
							? ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, effectiveDate, priorityToPilotFunctionality)
							: ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(code, EunCode, effectiveDate, priorityToPilotFunctionality);
						break;
					default:
						break;
				}
			}

			return result;
		}

		public static bool IsFunctionalityValid(string code, string dataGroupingCode = null, ValidationOptions options = ValidationOptions.UseSpecificDataGroupingOnly, bool priorityToPilotFunctionality = false)
		{
			return IsFunctionalityValid(code, ZDateTime.Today, dataGroupingCode, options, priorityToPilotFunctionality);
		}
	}
}
