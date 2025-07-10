using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public interface IAdditionalTariffParent : ICusLineTariffDetailParent
	{
		BusinessObjectFactory Factory { get; }
		LegalActInfoCollection LegalActInfos { get; }
		AdditionalTariffCollection AdditionalTariffs { get; }
	}
}
