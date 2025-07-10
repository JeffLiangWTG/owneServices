using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM447MessageInterpreter : InboundMessageInterpreter<IM447Provider>
	{
		public IM447MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM447Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("499578CC-C5A4-47D8-9817-381B005E1225", "A Documentary Control Result (IM447) message has been received for Job {0}.", relatedJob.JobNumber);

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
				(Res.GetString("5041F67B-0081-440B-B7EB-9F4CCE86518E", "Supporting Documents Provided"), provider.SupportingDocumentsProvided),
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
