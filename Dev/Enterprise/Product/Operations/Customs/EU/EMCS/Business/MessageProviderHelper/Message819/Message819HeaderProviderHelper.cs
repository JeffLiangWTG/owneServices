using System;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message819HeaderProviderHelper : HeaderProviderHelper
	{
		public Message819HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration, IAlertOrReject alertOrReject) : base(emcsJobDeclaration)
		{
			this.alertOrReject = alertOrReject;
		}
		readonly IAlertOrReject alertOrReject;

		public string DestinationOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);

		public bool RejectedFlag => alertOrReject.RejectedFlag;

		public DateTime? DateOfAlertOrRejection => alertOrReject.DateOfAlertOrRejection.ToNullableDateTime();
	}
}
