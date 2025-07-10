using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class AdditionalFiscalReferenceProvider : IAdditionalReference
	{
		public AdditionalFiscalReferenceProvider(AsycudaBill bill)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public string Number => bill.ABL_SellerRegNo;

		public string Type => "FR5";
	}
}
