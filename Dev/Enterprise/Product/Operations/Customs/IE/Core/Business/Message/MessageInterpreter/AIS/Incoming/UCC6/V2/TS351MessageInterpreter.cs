using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS351MessageInterpreter : InboundMessageInterpreter<TS351Provider>
	{
		public TS351MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS351Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("4DDE28FB-F4F4-4B2E-9B80-027A31AA159D", "A [G4 | G4+G3 | Manifest] Refusal (TS351) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("699BABBB-B526-45B0-B128-88CC1B11769F", "Specific Circumstance Indicator"), provider.SpecificCircumstanceIndicator);

			var pogDates = provider.DateAndTimesOfPresentationOfTheGoods;
			if (pogDates != null)
			{
				var isFirstRow = true;
				foreach (var pogDate in pogDates)
				{
					var pogDateString = pogDate.ToShortDateString();
					if (isFirstRow)
					{
						yield return (CommonResStrings.DateAndTimeOfPresentationOfTheGoods, pogDateString);
						isFirstRow = false;
					}
					else
					{
						yield return (string.Empty, pogDateString);
					}
				}
			}

			yield return (CommonResStrings.ControlResultCode, provider.ControlResultCode);
			yield return (CommonResStrings.ControlResultDate, provider.ControlResultDate.ToShortDateString());
			yield return (CommonResStrings.ControlResultRemarks, provider.ControlResultRemarks);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}
	}
}
