using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE819HeaderProvider : HeaderProvider, IIE819Header
	{
		public IE819HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IAlertOrReject alertOrReject) : base(emcsJobDeclaration)
		{
			this.alertOrReject = Argument.NotNull(alertOrReject, nameof(alertOrReject));
			helper = new Message819HeaderProviderHelper(emcsJobDeclaration, alertOrReject);
		}
		readonly IAlertOrReject alertOrReject;
		readonly Message819HeaderProviderHelper helper;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => int.TryParse(emcsJobDeclaration.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCache;

		public string DestinationOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.OfficeOfDestination);

		public bool RejectedFlag => helper.RejectedFlag;

		public DateTime? DateOfAlertOrRejection => helper.DateOfAlertOrRejection;

		public DateTime? DateAndTimeOfValidationOfAlertRejection => null;

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress));
		IEMCSPartyConsignee consigneeTrader;

		public IReadOnlyCollection<IEMCSReason> AlertOrRejectionReasons => alertOrRejectionReasons ?? (alertOrRejectionReasons = alertOrReject.AlertOrRejectionReasons.Select(x => new IE819ReasonProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSReason> alertOrRejectionReasons;
	}
}
