using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class SealIDWrapper : ISealID
	{
		SealIDWrapper(string seal)
		{
			value = seal;
		}

		public static SealIDWrapper NewOrNull(string seal) => seal.IsNullOrEmpty() ? null : new SealIDWrapper(seal);

		public string Value => value;
		readonly string value;
	}
}
