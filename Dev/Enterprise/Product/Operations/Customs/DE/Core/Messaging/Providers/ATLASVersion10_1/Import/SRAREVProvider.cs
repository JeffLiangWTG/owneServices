using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class SRAREVProvider : ISRAREV
	{
		public SRAREVProvider(NSREVC message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly NSREVC message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier?.Replace("-", string.Empty);

		public string CancelledReferenceNumber => message.Header?.CancelledReferenceNumber;

		public string CancelledMRN => message.Header?.CancelledMRN;

		public string InterchangeRecipientEBS => message.MetaData?.InterchangeRecipient?.Identification?.SubsidiaryNumber;
	}
}
