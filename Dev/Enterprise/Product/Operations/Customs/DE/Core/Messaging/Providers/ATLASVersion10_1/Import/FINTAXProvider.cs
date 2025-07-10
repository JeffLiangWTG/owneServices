using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class FINTAXProvider : IFINTAX
	{
		public FINTAXProvider(FFTAXE message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly FFTAXE message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public string ReferenceNumber => message.Header?.ReferenceNumber;

		public string MRN => message.Header?.MRN;

		public string LocalReferenceNumber => message.Header?.LRN;
	}
}
