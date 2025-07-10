using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSREVProvider : ICUSREV
	{
		public CUSREVProvider(FCREVH message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public ZString CancelledReferenceNumber => message.Header?.CancelledReferenceNumber ?? ZString.Empty;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public ZString ReferenceNumber => message.Header?.ReferenceNumber ?? ZString.Empty;

		public ZString Reason => message.Header?.Reason ?? ZString.Empty;

		public string MRN => message.Header?.MRN;

		public string CancelledMRN => message.Header?.CancelledMRN;

		readonly FCREVH message;
	}
}
