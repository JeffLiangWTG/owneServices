using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ITaxAndDocsProvider : ISupportingDocumentsProvider
	{
		ZString CountryCode { get; }
		BusinessObjectCollection Taxes { get; }
		ZString PreferenceCode { set; }
		ZString SupplementaryCode { set; }
		ZString Tariff { get; }
		ZString CountryOfOriginCode { get; }
		BusinessObjectFactory Factory { get; }

		ZDateTime DateOfValuation { get; }
	}
}
