using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CWSINFProvider : ICWSINF
	{
		public CWSINFProvider(LCWSIE message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}
		readonly LCWSIE message;

		public string MessageIdentifier => message.MetaData?.MessageIdentifier;

		public string CurrentProcedure => message.Header?.CustomsAuthorisation?.CurrentProcedure ?? string.Empty;
	}
}
