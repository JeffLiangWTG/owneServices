using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC917MessageInterpreter : InboundMessageInterpreter<CC917CProvider>
	{
		public CC917MessageInterpreter(AESInboundEDIMessage message, CC917CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"56ADA49B-B3B0-41DD-A216-092AA2366B89",
			"A Syntax Error Response message has been received from Customs for Job {0} through the IE917 message.",
			relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);

			var errors = provider.Errors;

			if (errors != null)
			{
				var xmlErrorCodes = factory.GetCachedValue<XmlErrorCodeList>();
				foreach (var error in errors)
				{
					yield return (CommonResStrings.ErrorLineNumber, error.ErrorLineNumber);
					yield return (CommonResStrings.ErrorColumnNumber, error.ErrorColumnNumber);
					if (error.ErrorPointer != string.Empty)
					{
						yield return (CommonResStrings.ErrorPointer, error.ErrorPointer);
					}
					if (error.ErrorCode != string.Empty)
					{
						yield return (CommonResStrings.ErrorCode, MessageInterpreterHelper.GetCodeAndDescription(error.ErrorCode, xmlErrorCodes));
					}
					yield return (CommonResStrings.ErrorText, error.ErrorText);
				}
			}
		}
	}
}
