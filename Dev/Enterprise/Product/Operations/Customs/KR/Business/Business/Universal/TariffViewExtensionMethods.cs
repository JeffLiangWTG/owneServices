using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class TariffViewExtensionMethods
	{
		public static ZString GetPostClearanceProcedureCondition(this TariffView tariffView, ZString primaryPreference)
		{
			return tariffView?.FilteredConditions.FirstOrDefault(x => x.PreferenceCode == primaryPreference)?.ConditionValues.FirstOrDefault(x => x.ConditionValueType.ZX4_ValueType == Constants.ZZ.RefCusConditionType.PostClearanceProcedure)?.ZX3_Value ?? ZString.Empty;
		}
	}
}
