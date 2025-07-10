using System;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	class UkHmrcNOPOrganizationEventProcessor : OrganizationEventProcessorBase<UkHmrcNOPResponse>
	{
		protected override string DataProvider => "UkHmrcNOP";

		protected override DateTime GetVerifiedDateTime(UkHmrcNOPResponse response) => response.Date;

		protected override bool ResponseIsPositive(string responseType, UkHmrcNOPResponse response)
		{
			return response.Eoris != null && response.Eoris.Count == 1 && response.Eoris[0].Authorised;
		}
	}
}
