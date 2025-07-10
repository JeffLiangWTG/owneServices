using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE224MessageProvider : MessageProvider, IIE224Header
	{
		public IE224MessageProvider(GuaranteeVoucherSoldSendingAction sendingAction)
		{
			this.sendingAction = Argument.NotNull(sendingAction, nameof(sendingAction));
			CusGuaranteeHeader = Argument.NotNull(sendingAction.Header, nameof(sendingAction.Header));
		}

		public readonly CusGuaranteeHeader CusGuaranteeHeader;
		readonly GuaranteeVoucherSoldSendingAction sendingAction;
		OrgHeader orgHeader => CusGuaranteeHeader.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, sendingAction.HolderOfTransitProcedure));

		public IHolder HolderOfTheTransitProcedure => CachedValueHelper.GetValue(ref holderOfTheTransitProcedure, () => HolderOfTransitProcedureProvider.New(orgHeader.MainAddress, NctsPhase5DeclarationTypeList.Codes.TIR));
		CachedValue<IHolder> holderOfTheTransitProcedure;

		public CargoWise.Customs.IE.MessageContracts.Interfaces.IParty Guarantor => CachedValueHelper.GetValue(ref guarantor, () => IE.Business.AES.PartyProvider.New(CusGuaranteeHeader.PermitHolder.MainAddress));
		CachedValue<CargoWise.Customs.IE.MessageContracts.Interfaces.IParty> guarantor;

		public string CustomsOfficeOfGuaranteeReferenceNumber => sendingAction.CustomsOfficeOfGuarantee;

		public IReadOnlyCollection<IIE224GuaranteeReference> GuaranteeReference => guaranteeReference ?? (guaranteeReference = new List<IIE224GuaranteeReference>() { new IE224GuaranteeReferenceProvider(sendingAction) });
		IReadOnlyCollection<IIE224GuaranteeReference> guaranteeReference;
	}
}
