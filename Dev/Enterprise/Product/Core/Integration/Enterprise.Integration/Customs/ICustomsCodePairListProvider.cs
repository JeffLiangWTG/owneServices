using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICustomsCodePairListProvider
		{
			ICodeDescriptionPairList GetContainerModeList(ZString transportMode);
		}
	}
}
