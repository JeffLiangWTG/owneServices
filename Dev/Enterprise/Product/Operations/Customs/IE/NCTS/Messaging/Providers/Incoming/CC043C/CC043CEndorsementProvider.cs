using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CEndorsementProvider
	{
		public CC043CEndorsementProvider(EndorsementType03 endorsement)
		{
			endorsementType = Argument.NotNull(endorsement, nameof(endorsement));
		}
		readonly EndorsementType03 endorsementType;

		public ZString Authority => endorsementType.Authority ?? ZString.Empty;

		public ZDate Date => endorsementType.Date.ConvertToZDate();

		public ZString Place => endorsementType.Place ?? ZString.Empty;

		public ZString Country => endorsementType.Country ?? ZString.Empty;
	}
}
