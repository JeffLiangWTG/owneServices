using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class CustomsWareIntegrationOutOfLine : CustomsWareIntegration
	{
		public new bool SubmitSucceeded(XElement submissionResult) => base.SubmitSucceeded(submissionResult);

		public new ZString GetSubmitFailedReasons(XElement submissionResult) => base.GetSubmitFailedReasons(submissionResult);

		protected override XElement Submit(ZString submittedData)
		{
			return new XElement("QueueForSubmission",
				new XElement("StatusCode", "0")
			);
		}

		protected override ZString EntryStatus => CustomsWareEntryStatusList.Codes.Queued;

		protected override bool ShouldLogCustomsCommenced => false;

		protected override ZString SubmitMessageStatus => EDIMessage.Status.Queued;

		protected override bool ShouldCreateDataExportEventAfterSubmitMessageCreation => false;

		protected override string SubmitSucceededMessage => Res.GetString("1876c7df-2361-4ea8-93fe-0b3007abe956", "Queue for Submission Succeeded.");
	}
}
