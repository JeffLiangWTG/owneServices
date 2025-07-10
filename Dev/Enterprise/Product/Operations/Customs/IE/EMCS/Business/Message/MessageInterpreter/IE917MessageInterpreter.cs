using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class IE917MessageInterpreter : InboundMessageInterpreter<IIE917>
	{
		public IE917MessageInterpreter(EMCSInboundEDIMessage message, IIE917 provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("BA61940F-7455-4259-8CC3-15FF6A141015", "A Negative Acknowledgement of XML Receipt message has been received.");

		protected override IEnumerable<string> GetAdvancedSummaries()
		{
			yield return Res.GetString("1C85E191-D959-40B6-AD53-73DCFC0FCD8D", "Your submission has been rejected for the following reasons:");
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			var index = 1;
			foreach (var error in provider.Errors)
			{
				yield return (Res.GetString("17DDA3F0-AE27-4CA8-80F0-79BE109895D9", "Error {0}:", index), new (string, string)[]
				{
					(CommonResStrings.ErrorLineNumber, error.ErrorLineNumber),
					(CommonResStrings.ErrorColumnNumber, error.ErrorColumnNumber),
					(CommonResStrings.ErrorReason, error.ErrorReason),
					(Res.GetString("6F6E6658-8CC5-44AE-9FC9-504DB261DC2B", "Error Location"), error.ErrorLocation),
					(CommonResStrings.OriginalAttributeValue, error.OriginalAttributeValue),
				});
				index++;
			}
		}

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails() => Enumerable.Empty<(string, string)>();
	}
}
