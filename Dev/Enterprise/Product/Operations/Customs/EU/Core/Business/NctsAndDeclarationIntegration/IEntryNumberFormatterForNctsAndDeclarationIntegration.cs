using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration
{
	public interface IEntryNumberFormatterForNctsAndDeclarationIntegration
	{
		(ZString Class, ZString Type, ZString Reference, ZInt? EntryLineNumber) FormatEntryNumber(CusEntryNumber sourceEntryNumber, ZString jobType);
	}
}
