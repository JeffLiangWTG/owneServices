using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderEntryNumbersProvider : CusEntryNumbersProvider
{
	public CusEntryHeaderEntryNumbersProvider(CusEntryHeader entryHeader) : base(entryHeader?.Factory, entryHeader, entryHeader?.CountryCode ?? ZString.Empty)
	{
	}
}
