using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CLocationProvider
	{
		public CC043CLocationProvider(LocationType02 location)
		{
			locationType = Argument.NotNull(location, nameof(location));
		}
		readonly LocationType02 locationType;

		public ZString QualifierOfIdentification => locationType.QualifierOfIdentification ?? ZString.Empty;

		public ZString UNLocode => locationType.UnLocode ?? ZString.Empty;

		public ZString Country => locationType.Country ?? ZString.Empty;
	}
}
