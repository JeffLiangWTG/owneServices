using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS328MessageInterpreter : InboundMessageInterpreter<TS328Provider>
	{
		public TS328MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS328Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("B147D1C6-C3A9-4268-9720-A1BC5FD64820", "A [G4 | G4+G3 | Manifest] Declaration Acceptance (TS328) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("008B20A6-377D-4636-813E-AC6EC37BFF86", "Date of Acceptance"), provider.DateOfAcceptance.ToShortDateString());
			yield return (Res.GetString("C57B18B3-ACD4-42EE-94FD-7E7B9E5B1D64", "Response Date Limit"), provider.ResponseDateLimit.ToShortDateString());
			yield return (CommonResStrings.DateAndTimeOfPresentationOfTheGoods, provider.DateAndTimeOfPresentationOfGoods.ToLongTimeString());
			yield return (Res.GetString("B0384DB7-5608-4F34-B48B-42E2FA618525", "Remarks"), provider.Remarks);
		}
	}
}
