using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC029CMessageInterpreter : InboundMessageInterpreter<CC029CProvider>
	{
		public CC029CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC029CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("32C5C7E6-4BEB-4C27-A195-3196A7647C2C", "A Release of Transit (IE029) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.ReleaseDate, provider.ReleaseDate.ToShortDateString());
			yield return (Res.GetString("0BFC9D0F-B6ED-4165-B4C3-CC4A709A5A9E", "Additional Declaration Type"), provider.AdditionalDeclarationType);
			yield return (Res.GetString("27DA8E06-6586-4725-ABAF-D952757EBBA5", "Control Result Code"), provider.ControlResultCode);
			yield return (Res.GetString("D99E6B3D-6048-434E-84D3-B2814576F177", "Control Result Date"), provider.ControlResultDate.ToShortDateString());
			yield return (Res.GetString("9F9715B3-3DB0-44E8-BB35-FF6FBAF8F562", "Control Result Controller"), provider.ControlResultController);
			yield return (Res.GetString("24877D63-C58F-40A0-9B83-C58553FCBE90", "Control Result Text"), provider.ControlResultText);
		}
	}
}
