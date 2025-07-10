using System;
using System.Linq;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public static class AdditionalInfoCollectionExtensions
	{
		public static bool HasAuthorisationForSpecialProcedure(this AdditionalInfoCollection collection) => collection.HasAdditionalInfo(IsAuthorisationForSpecialProcedure);

		public static bool IsAuthorisationForSpecialProcedure(this AdditionalInfo addInfo) =>
			addInfo.IsAnAdditionalInformation
			&& addInfo.CSI_Code == AdditionalInformationCodes._00100;

		public static bool HasAdditionalInfo(this AdditionalInfoCollection collection, Func<AdditionalInfo, bool> filter) => collection.Cast<AdditionalInfo>().Any(filter);
	}
}
