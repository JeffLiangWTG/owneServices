using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IPreviousDocumentUniversalTariffProvider
{
	ZString TariffCode { get; }
	ZString FormattedTariff { get; }
	ZString UniversalTariffType { get; }
	ZString Procedure { get; }
	ZPropertyInfo Quantity2Info { get; }
}
