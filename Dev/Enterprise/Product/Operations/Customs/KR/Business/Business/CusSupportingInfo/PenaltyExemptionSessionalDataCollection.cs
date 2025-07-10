using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionSessionalDataCollection : CusSupportingInfoCollection<PenaltyExemptionSessionalData>
	{
		public PenaltyExemptionSessionalDataCollection(CusEntryInstruction parent, ZGuid cusSupportingInfoPK)
			: base(parent, CusSupportingInfoTypeList.Codes._5UA, cusSupportingInfoPK)
		{
		}
	}
}
