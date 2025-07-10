using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperCharge : IDocCharges
	{
		internal AWBSendChileWrapperCharge(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly AsycudaBill bill;

		decimal IDocCharges.Amount => bill.ABL_FreightValue;

		string IDocCharges.Currency => bill.ABL_RX_NKFreightValueCurrency;

		string IDocCharges.PaymentCondition => bill.ABL_PrepaidCollect == Core.Constants.PaymentType.Prepaid ? AWBSendChileConstants.PaymentType.P : AWBSendChileConstants.PaymentType.C;
	}
}
