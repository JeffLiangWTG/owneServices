using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperDocumentFreight : IDocumentFreight
	{
		internal BLSendChileWrapperDocumentFreight(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly AsycudaBill bill;

		string IDocumentFreight.Type => bill.ABL_PrepaidCollect == Core.Constants.PaymentType.Prepaid ? BLSendChileConstants.PaymentType.Prepaid : BLSendChileConstants.PaymentType.Collect;
	}
}
