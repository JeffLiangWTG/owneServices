using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM099MessageInterpreter : InboundMessageInterpreter<IIM099Provider>
	{
		public IM099MessageInterpreter(AISInboundEDIMessage message, IM099Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM099Provider messageProvider;

		protected override string Summary => CommonResStrings.GetIM099MessageInterpreterSummary(relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.DateLimitOfResponse, messageProvider.DateLimitOfResponse.ToISO8601ShortDateString());
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
			yield return (CommonResStrings.CustomsOfficeOfPresentation, messageProvider.CustomsOfficeOfPresentation);
			yield return (CommonResStrings.SupervisingCustomsOffice, messageProvider.SupervisingCustomsOffice);
			yield return (CommonResStrings.CustomsOfficeLodgement, messageProvider.CustomsOfficeLodgement);
		}
	}
}
