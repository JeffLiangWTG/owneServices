using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSReassessmentMessage
	{
		public string originalLrn { get; set; }

		public string brokerBranchId { get; set; }

		public string reassessmentReason { get; set; }

		public string contactName { get; set; }

		public string contactPhone { get; set; }

		public string contactEmail { get; set; }

		public string thirdPartyNotificationEmail { get; set; }

		public string additionalComments { get; set; }

		public string generalDeclaration { get; set; }

		public List<COLSDirectionRequests> directionRequests { get; set; }

		public bool documentationRequired { get; set; }
	}
}
