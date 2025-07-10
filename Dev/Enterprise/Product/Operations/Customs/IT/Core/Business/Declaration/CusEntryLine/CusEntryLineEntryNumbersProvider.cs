using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineEntryNumbersProvider : CusEntryNumbersProvider
{
	public CusEntryLineEntryNumbersProvider(CusEntryLine entryLine) : base(entryLine?.Factory, entryLine, entryLine?.CountryCode ?? ZString.Empty)
	{
	}
}
