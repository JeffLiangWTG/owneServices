using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE819HeaderProvider : HeaderProvider, IIE819Header
	{
		public IE819HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IAlertOrReject alertOrReject) : base(emcsJobDeclaration)
		{
			this.alertOrReject = Argument.NotNull(alertOrReject, nameof(alertOrReject));
		}
		readonly IAlertOrReject alertOrReject;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => emcsJobDeclaration.SequenceNumber == "0" ? DefaultSequenceNumber : ZInt.ParseSafe(emcsJobDeclaration.SequenceNumber, DefaultSequenceNumber));
		CachedValue<int> sequenceNumberCache;

		public string DestinationOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);

		public bool RejectedFlag => alertOrReject.RejectedFlag;

		public DateTime? DateOfAlertOrRejection => alertOrReject.DateOfAlertOrRejection.ToNullableDateTime();

		public DateTime? DateAndTimeOfValidationOfAlertRejection => this.IsValidationAttributeAllowed ? ZDateTime.Now.ToDateTime().ToUnspecifiedKindWithSecondsPrecision() : null;

		public IEMCSPartyConsignee ConsigneeTrader => consigneeTrader ?? (consigneeTrader = PartyConsigneeProvider.NewOrNull(emcsJobDeclaration.ImporterDocumentaryAddress));
		IEMCSPartyConsignee consigneeTrader;

		public IReadOnlyCollection<IEMCSReason> AlertOrRejectionReasons => alertOrRejectionReasons ?? (alertOrRejectionReasons = alertOrReject.AlertOrRejectionReasons.Select(x => new IE819ReasonProvider(x)).ToArray());
		IReadOnlyCollection<IEMCSReason> alertOrRejectionReasons;
	}
}
