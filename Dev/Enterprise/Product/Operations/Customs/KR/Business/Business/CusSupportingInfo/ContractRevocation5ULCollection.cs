using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ContractRevocation5ULCollection : CusSupportingInfoCollection<ContractRevocation5UL>
	{
		public ContractRevocation5ULCollection(CusReconEntryLine parent)
			: base(parent, CusSupportingInfoTypeList.Codes.ContractRevocation5UL)
		{
		}
	}
}
