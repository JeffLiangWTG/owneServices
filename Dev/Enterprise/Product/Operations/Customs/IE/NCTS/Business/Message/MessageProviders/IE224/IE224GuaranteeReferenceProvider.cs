using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE224GuaranteeReferenceProvider : IIE224GuaranteeReference
	{
		public IE224GuaranteeReferenceProvider(GuaranteeVoucherSoldSendingAction sendingAction)
		{
			this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
			header = Argument.NotNull(sendingAction.Header, nameof(sendingAction.Header));
		}
		readonly GuaranteeVoucherSoldSendingAction sendingAction;
		readonly CusGuaranteeHeader header;

		public string Grn => header.CPH_Number;

		public string Currency => Core.Constants.CurrencyCodes.EuropeanUnion;

		public DateTime IssueDate => header.CPH_StartDate.IsValid ? header.CPH_StartDate.ToDateTime() : DateTime.MinValue;

		public string AccessCode => header.MainAccessCode;

		public bool TIRCarnet => sendingAction.TIRCarnet;

		public DateTime ExpiryDate => header.CPH_EndDate.IsValid ? header.CPH_EndDate.ToDateTime() : DateTime.MinValue;

		public bool CopyGiven => true;

		public decimal VoucherAmount => sendingAction.VoucherAmount;
	}
}
