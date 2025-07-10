using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class BusinessHeaderDocumentWrapper : IBusinessHeaderDocument
	{
		internal BusinessHeaderDocumentWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			header = bill.Header;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;

		string IBusinessHeaderDocument.ID => bill.ABL_BillNumber;

		string IBusinessHeaderDocument.ConsignorAuthenticationSignatory => header.ShippingAgentName;

		ICarrierAuthentication IBusinessHeaderDocument.CarrierAuthentication => carrierAuthentication ?? (carrierAuthentication = new CarrierAuthenticationWrapper(header));
		ICarrierAuthentication carrierAuthentication;
	}
}
