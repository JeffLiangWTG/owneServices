using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class SCWINFProvider : ISCWINF
	{
		public SCWINFProvider(LSCWIF message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly LSCWIF message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public string MRN => message.Header?.MRN;

		public string ReferenceNumber => message.Header?.ReferenceNumber;

		public string CurrentProcedure => message.Header?.CustomsAuthorisation?.CurrentProcedure;
	}
}
