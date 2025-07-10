using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class COLSLodgementMessage
	{
		public string entryNumber { get; set; }

		public string branchId { get; set; }

		public string biconReference { get; set; }

		public string importPermitNumber { get; set; }

		public string contactName { get; set; }

		public string phoneNumber { get; set; }

		public string email { get; set; }

		public bool thirdPartyInd { get; set; }

		public string thirdPartyEmail { get; set; }

		public string aaRefNum { get; set; }

		public string lateLodgementReason { get; set; }

		public string lateLodgementDetails { get; set; }

		public List<COLSDirectionRequests> directionRequests { get; set; }

		public string deliveryClassification { get; set; }

		public string unpackAddress { get; set; }

		public string additionalComment { get; set; }

		public string generalDeclaration { get; set; }
	}
}
