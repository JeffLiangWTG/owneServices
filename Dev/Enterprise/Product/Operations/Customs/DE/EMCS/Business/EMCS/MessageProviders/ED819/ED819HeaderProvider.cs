using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED819HeaderProvider : HeaderProvider, IED819Header
	{
		public ED819HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IAlertOrReject alertOrReject)
		: base(emcsJobDeclaration)
		{
			this.alertOrReject = Argument.NotNull(alertOrReject, nameof(alertOrReject));
			helper = new Message819HeaderProviderHelper(emcsJobDeclaration, alertOrReject);
		}
		readonly IAlertOrReject alertOrReject;
		readonly Message819HeaderProviderHelper helper;

		int IED819Header.SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => int.TryParse(emcsJobDeclaration.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCache;

		public string DestinationOfficeReferenceNumber => helper.DestinationOfficeReferenceNumber;

		public bool RejectedFlag => helper.RejectedFlag;

		public DateTime? DateOfAlertOrRejection => helper.DateOfAlertOrRejection;

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress));
		IEMCSPartyConsignee consigneeTrader;

		public IReadOnlyCollection<IEMCSReason> AlertOrRejectionReasons => alertOrRejectionReasons ?? (alertOrRejectionReasons = alertOrReject.AlertOrRejectionReasons.Select(x => new ED819ReasonProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSReason> alertOrRejectionReasons;
	}
}
