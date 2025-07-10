using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsBillConsigneeJobDocAddressValidation : ConsigneeJobDocAddressValidation
	{
		public NctsBillConsigneeJobDocAddressValidation(AutoJobDocAddress parent, NctsHeader nctsHeader)
			: base(parent, nctsHeader)
		{
		}

		protected override bool IsRelevantConsigneeEmpty() => NctsHeader.Consignee.IsEmpty;
	}
}
