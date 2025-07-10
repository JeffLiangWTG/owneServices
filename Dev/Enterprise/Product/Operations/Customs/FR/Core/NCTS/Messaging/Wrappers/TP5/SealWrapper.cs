using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class SealWrapper : ISeal
	{
		SealWrapper(string sealNumber, string unloadingState)
		{
			this.sealNumber = Argument.NotNull(sealNumber, nameof(sealNumber));
			this.unloadingState = unloadingState;
		}

		readonly string sealNumber;
		readonly string unloadingState;

		public static SealWrapper New(ZString sealNumber, string unloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW) => new SealWrapper(sealNumber, unloadingState);

		public string Identifier => identifier ?? (identifier = unloadingState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW ? sealNumber : null);
		string identifier;
	}
}
