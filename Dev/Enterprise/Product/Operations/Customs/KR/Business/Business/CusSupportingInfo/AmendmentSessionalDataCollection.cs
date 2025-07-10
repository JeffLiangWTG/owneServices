using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class AmendmentSessionalDataCollection : CusSupportingInfoCollection<AmendmentSessionalData>
	{
		public AmendmentSessionalDataCollection(CusEntryInstruction parent)
			: base(parent, CusSupportingInfoTypeList.Codes._5FE)
		{
		}
	}
}
