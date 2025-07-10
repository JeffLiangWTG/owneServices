using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderEntryNumbersProvider : CusEntryNumbersProvider
{
	public NctsHeaderEntryNumbersProvider(NctsHeader nctsHeader) : base(nctsHeader?.Factory, nctsHeader, nctsHeader?.CountryCode ?? ZString.Empty)
	{
	}
}
