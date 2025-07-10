
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRExportExemptionCodesList : CodeDescriptionPairList
	{
		public CMRExportExemptionCodesList()
		{
			Add(CMRExportExemptionCodes.EXDC);
			Add(CMRExportExemptionCodes.EXDD);
			Add(CMRExportExemptionCodes.EXLV);
			Add(CMRExportExemptionCodes.EXML);
			Add(CMRExportExemptionCodes.EXPE);
			Add(CMRExportExemptionCodes.EXSP);
			Add(CMRExportExemptionCodes.EXTI);
		}
	}
}
