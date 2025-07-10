using System;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	class UkHmrcEORIOrganizationEventProcessor : OrganizationEventProcessorBase<UkHmrcEORIResponse>
	{
		protected override string DataProvider => "UkHmrcEORI";

		protected override DateTime GetVerifiedDateTime(UkHmrcEORIResponse response) => response.ProcessingDate;

		protected override bool ResponseIsPositive(string responseType, UkHmrcEORIResponse response) => response.Valid;
	}
}
