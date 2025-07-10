using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public partial class MessageTypes
	{
		public static bool ShouldCreateInterchangeHeaderText(ZString messageType) => messageType == Codes.CHA || messageType == Codes.CHB || messageType == Codes.CHC;
	}
}
