using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class SealWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.ISeal
	{
		SealWrapper(string sealNumber)
		{
			this.sealNumber = Argument.NotNull(sealNumber, nameof(sealNumber));
		}

		public string Identifier => identifier ?? (identifier = sealNumber);

		string identifier;
		readonly string sealNumber;

		public static SealWrapper New(ZString sealNumber) => new SealWrapper(sealNumber);
	}
}
