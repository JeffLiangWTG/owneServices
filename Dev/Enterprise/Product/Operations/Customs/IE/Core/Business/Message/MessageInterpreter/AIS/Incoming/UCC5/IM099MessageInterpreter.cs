using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM099MessageInterpreter : InboundMessageInterpreter<IM099Provider>
	{
		public IM099MessageInterpreter(AISUCC5InboundEDIMessage message, IM099Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Messaging.AISInterchangeTypeList.Descriptions.IM099;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.DateLimitOfResponse, provider.DateLimitOfResponse.ToISO8601ShortDateString());
			yield return (CommonResStrings.CustomsOfficeLodgement, provider.CustomsOfficeLodgement);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
