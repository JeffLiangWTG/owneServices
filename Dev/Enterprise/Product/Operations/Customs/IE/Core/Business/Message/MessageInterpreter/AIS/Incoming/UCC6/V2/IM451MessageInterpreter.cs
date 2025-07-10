using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM451MessageInterpreter : InboundMessageInterpreter<IIM451Provider>
	{
		public IM451MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM451Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM451Provider messageProvider;

		protected override string Summary => Res.GetString("74C2B52A-5EB1-4C08-8FA2-A96573578661", "A No Release (IM451) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.LocalReferenceNumber, messageProvider.LocalReferenceNumber);
			yield return (CommonResStrings.DeclarationType, messageProvider.DeclarationType);
			yield return (CommonResStrings.AdditionalDeclarationType, messageProvider.AdditionalDeclarationType);
			yield return (Res.GetString("ECC7EB83-49E3-4DDF-A998-7E05559161E8", "Decision Date"), messageProvider.DecisionDate.ToShortDateString());
			yield return (CommonResStrings.DecisionReason, messageProvider.DecisionReason);
			yield return (CommonResStrings.PreferredPaymentMethod, messageProvider.PreferredPaymentMethod);
			yield return (CommonResStrings.Remarks, messageProvider.Remarks);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			foreach (var controlResult in messageProvider.ControlResults)
			{
				yield return (string.Empty, controlResult.GetMessageDetails(
					(riskAreaCode) => GetDescriptionFromCode(riskAreaCode, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL740, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN),
					(controlType) => GetDescriptionFromCode(controlType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType)));
			}
		}
	}
}
