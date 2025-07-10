using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTRAProvider : ICUSTRA
	{
		public CUSTRAProvider(GCTRAG message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public ZString ReferenceNumber => message.Header?.ReferenceNumber ?? ZString.Empty;

		public string MRN => message.Header?.MRN;

		public ZDate ForwardedDate => (message.Header?.ForwardedDateSpecified ?? false) ? new ZDate(message.Header.ForwardedDate) : ZDate.Empty;

		public ZString Reason => message.Header?.Reason ?? ZString.Empty;

		readonly GCTRAG message;
	}
}
