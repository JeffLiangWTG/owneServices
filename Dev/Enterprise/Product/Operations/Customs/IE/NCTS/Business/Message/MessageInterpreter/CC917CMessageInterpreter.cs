using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC917CMessageInterpreter : InboundMessageInterpreter<CC917CProvider>
	{
		public CC917CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC917CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"64B09369-5F82-4DF3-ACF5-E015463FE878",
			"An Syntax Error Notification (IE917) message has been received for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);

			var errors = provider?.XMLErrors;
			if (!errors.IsNullOrEmpty())
			{
				foreach (var error in errors)
				{
					yield return (CommonResStrings.ErrorLineNumber, error.ErrorLineNumber);
					yield return (CommonResStrings.ErrorColumnNumber, error.ErrorColumnNumber);
					yield return (CommonResStrings.ErrorPointer, error.ErrorPointer);
					yield return (CommonResStrings.ErrorCode, GetCodeAndDescription(error.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180));
					yield return (CommonResStrings.ErrorText, error.ErrorText);
				}
			}
		}
	}
}
