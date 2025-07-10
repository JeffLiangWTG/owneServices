using Enterprise.Customs.EU.Business.Documents;
using Enterprise.DocumentWrappers.Customs.EU;
using FRCusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit;

public class T2LEntryLinePacksMarksAndNumbersBuilder : EntryLinePacksMarksAndNumbersBuilder
{
	public T2LEntryLinePacksMarksAndNumbersBuilder(FRCusEntryLine entryLine) : base(entryLine)
	{
	}

	protected override string PackagesSeparator => DocumentWrapperConstants.Delimiters.CarriageReturn;

	protected override string FormatPackageDetailFields(string mark, string number, string type) => $"Nombre et nature :    {number}  {type}{DocumentWrapperConstants.Delimiters.CarriageReturn}Marques et numéro :    {mark}";
}
