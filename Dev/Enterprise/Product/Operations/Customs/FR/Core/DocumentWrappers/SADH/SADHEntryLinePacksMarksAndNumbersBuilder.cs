using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents;
using Enterprise.DocumentWrappers.Customs.EU;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH;

class SADHEntryLinePacksMarksAndNumbersBuilder : EntryLinePacksMarksAndNumbersBuilder
{
	public SADHEntryLinePacksMarksAndNumbersBuilder(CusEntryLine entryLine) : base(entryLine)
	{
	}

	protected override string FormatPackageDetailFields(string mark, string number, string type) => FormattableString.Invariant($"Nb et nature des colis : {number} - {type}{DocumentWrapperConstants.Delimiters.CarriageReturn}Marques et numéros : {mark}");

	protected override string PackagesSeparator => DocumentWrapperConstants.Delimiters.CarriageReturn;
}
