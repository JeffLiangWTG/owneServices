using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public interface IAdditionalInformationFormatter
	{
		ZString Format(AdditionalInfo additionalInfo);
	}
}
