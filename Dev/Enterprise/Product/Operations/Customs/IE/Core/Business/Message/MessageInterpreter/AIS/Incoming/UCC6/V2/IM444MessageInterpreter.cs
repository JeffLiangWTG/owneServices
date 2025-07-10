using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM444MessageInterpreter : InboundMessageInterpreter<IM444Provider>
	{
		public IM444MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM444Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("F374288E-FDEF-40CD-95D8-8250BC3415AF", "A Control Results (IM444) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			yield return (CommonResStrings.ControlResult, new (string, string)[]
			{
				(CommonResStrings.Code, provider.Code),
				(CommonResStrings.Date, provider.Date.ToShortDateString()),
				(CommonResStrings.Remarks, provider.Remarks),
				(Res.GetString("7E548419-86DE-4126-B059-D9D3A7346F5E", "Pending Sampling Results"), provider.PendingSamplingResults),
			});

			foreach (var controlResult in provider.ControlResults)
			{
				yield return (string.Empty, controlResult.GetMessageDetails(
					(riskAreaCode) => GetDescriptionFromCode(riskAreaCode, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL740, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN),
					(controlType) => GetDescriptionFromCode(controlType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType)));
			}
		}
	}
}
