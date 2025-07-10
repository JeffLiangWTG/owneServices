using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public class SADHEntryLinePacksMarksAndNumbersBuilder : EntryLinePacksMarksAndNumbersBuilder
	{
		public SADHEntryLinePacksMarksAndNumbersBuilder(CusEntryLine entryLine) : base(entryLine)
		{
		}

		protected override string PackagesSeparator => DocumentWrapperConstants.Delimiters.SemiColonAndspace;

		protected override string FormatPackageDetailFields(string mark, string number, string type) => ResString.GetMultilingualString("4D2369E5-4DCD-4098-AD61-51CD554722D4", "Marks & Number={0} Number={1} Package type={2}", mark, number, type);
	}
}
