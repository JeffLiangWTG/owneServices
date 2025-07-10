using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE026MessageProvider : NctsDepartureHeaderMessageProvider, IIE026Header
	{
		public IE026MessageProvider(GuaranteeAccessCodesSendingAction sendingAction) : base(sendingAction?.CusGuaranteeHeader)
		{
			this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
			cusGuaranteeHeader = Argument.NotNull(sendingAction.CusGuaranteeHeader, nameof(sendingAction.CusGuaranteeHeader));
		}
		readonly GuaranteeAccessCodesSendingAction sendingAction;
		readonly CusGuaranteeHeader cusGuaranteeHeader;

		public IParty HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedureCached, () => HolderOfTransitProcedureProvider.New(cusGuaranteeHeader.PermitHolder.MainAddress, cusGuaranteeHeader.CPH_SubType));
		CachedValue<IHolder> holderOfTheTransitProcedureCached;

		public string CustomsOfficeOfGuaranteeReferenceNumber => sendingAction.OfficeOfGuarantee;

		public IIE026GuaranteeReference GuaranteeReference => guaranteeReference ?? (guaranteeReference = new IE026GuaranteeReferenceProvider(sendingAction));
		IIE026GuaranteeReference guaranteeReference;
	}
}
