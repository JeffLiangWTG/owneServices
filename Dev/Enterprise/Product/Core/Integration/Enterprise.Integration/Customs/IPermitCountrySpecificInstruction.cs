using CargoWise.Integration;
using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IPermitCountrySpecificInstruction
		{
			ICodeDescriptionPairList GetTypeList();
			ICodeDescriptionPairList GetSubTypeList(ZString typeCode);
		}
	}
}
