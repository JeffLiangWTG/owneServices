using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public interface ISearcheableCusEntryNumber
	{
		ZString CE_Category { get; }
		ZString CE_EntryType { get; }
		ZString CE_EntryNum { get; }
		ZString CE_RN_NKCountryCode { get; }
	}
}
