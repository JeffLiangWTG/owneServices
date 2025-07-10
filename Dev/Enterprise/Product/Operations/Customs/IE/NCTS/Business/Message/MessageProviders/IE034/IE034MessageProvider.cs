using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE034MessageProvider : NctsDepartureHeaderMessageProvider, IIE034Header
	{
		public IE034MessageProvider(QueryOnGuaranteeSendingAction sendingAction) : base(sendingAction.Header)
		{
			this.sendingAction = sendingAction;
		}
		readonly QueryOnGuaranteeSendingAction sendingAction;

		public string RequesterIdentificationNumber => MessageProviderHelper.GetRegCodeFromCustomsCodes(NctsHeader.Principal.Organisation);

		public IReadOnlyCollection<IIE034GuaranteeReference> GuaranteeReferences => guaranteeReferences ?? (guaranteeReferences = sendingAction.GetSelectedGuarantees.Select(p => new IE034GuaranteeReferenceProvider(sendingAction, p)).ToArray());
		IReadOnlyCollection<IIE034GuaranteeReference> guaranteeReferences;
	}
}
