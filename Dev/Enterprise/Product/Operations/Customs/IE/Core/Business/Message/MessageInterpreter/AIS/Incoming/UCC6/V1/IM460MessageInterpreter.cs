using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using IM460Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM460Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM460MessageInterpreter : InboundMessageInterpreter<IIM460Provider>
	{
		public IM460MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM460Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM460Provider messageProvider;

		protected override string Summary => Res.GetString("5d057858-60ad-4bce-9e32-e65ae622b24a", "A Control Notice (IM460) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (Res.GetString("80cc15c9-9f04-4b70-8149-2060d93791e8", "Notification Date"), messageProvider.NotificationDate.ToShortDateString());
			yield return (Res.GetString("ec364372-8c8e-43c7-aa3a-bdaa92ddd1fe", "Time Limit For Control"), messageProvider.TimeLimitForControl.ToLongTimeString());
			yield return (Res.GetString("8d010810-77f8-4563-b99c-45558ccd40b2", "Overall Control Type Code"), messageProvider.OverallControlTypeCode);
			yield return (Res.GetString("c3670154-560a-4603-8c1e-0e1276af1fbf", "Overall Control Type Description"), OverallControlTypeList.GetDescriptionFromCode(messageProvider.OverallControlTypeCode));
		}

		public OverallControlTypeList OverallControlTypeList => CachedValueHelper.GetValue(ref overallControlTypeList, () => new OverallControlTypeList());
		CachedValue<OverallControlTypeList> overallControlTypeList;
	}
}
