using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class RefundSessionalDataCollection : CusSupportingInfoCollection<RefundSessionalData>
	{
		public RefundSessionalDataCollection(CusEntryInstruction parent, ZGuid cusSupportingInfoPK)
			: base(parent, CusSupportingInfoTypeList.Codes._5UL, cusSupportingInfoPK)
		{
		}
		public RefundSessionalDataCollection(CusEntryInstruction parent)
			: base(parent, CusSupportingInfoTypeList.Codes._5UL)
		{
		}
	}
}
