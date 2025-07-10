using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	class UkHmrcVATOrganizationEventProcessor : OrganizationEventProcessorBase<UkHmrcVATResponse>
	{
		protected override string DataProvider => "UkHmrcVAT";

		protected override bool ShouldProcessResponse(string responseType, UkHmrcVATResponse response)
		{
			return base.ShouldProcessResponse(responseType, response) ||
				(responseType == AutoEvents.MessageRejectedCode && response.Code == "NOT_FOUND" && response.Message == "targetVrn does not match a registered company");
		}

		protected override DateTime GetVerifiedDateTime(UkHmrcVATResponse response) => response.ProcessingDate;

		protected override bool ResponseIsPositive(string responseType, UkHmrcVATResponse response) => responseType == AutoEvents.ServiceRequestedCode;
	}
}
