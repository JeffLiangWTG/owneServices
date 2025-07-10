using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM460MessageInterpreter : InboundMessageInterpreter<IM460Provider>
	{
		public IM460MessageInterpreter(AISUCC5InboundEDIMessage message, IM460Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("846BC1ED-F14C-46F5-BBE9-EB089B55C363", "A Control (IM460) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ControlNotificationDateAndTime, provider.ControlNotificationDate.ToLongTimeString());
			yield return (Res.GetString("0CD54F2A-BACB-4B1A-B540-59A395692AB3", "Time Limit for Control"), provider.TimeLimitForControl.ToLongTimeString());
			yield return (CommonResStrings.CustomsOfficeLodgement, provider.CustomsOfficeLodgement);
			yield return (Res.GetString("3880EB01-214C-43CC-AA3C-34E59F48267A", "Overall Control Type Coded"), provider.OverallControlTypeCoded);
		}
		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var controlType in provider.ControlTypes)
			{
				yield return (CommonResStrings.ControlType, new (string, string)[]
				{
					(Res.GetString("C33528AC-13C5-4FE3-AD47-0A737B13F74A", "Control Type Coded"),controlType.ControlTypeCoded),
					(Res.GetString("D9AC9802-61BB-4187-849E-AB82031E49C1", "Control Type Agency"), controlType.ControlTypeAgency),
				});
			}
		}
	}
}
