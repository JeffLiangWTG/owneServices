using CargoWise.Types;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUASPEntryHeaderIAccInvoiceDataProvider : AUEntryHeaderIAccInvoiceDataProvider
{
	public AUASPEntryHeaderIAccInvoiceDataProvider(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	public static ZString GetUniqueNumber(ZString entryNumber) => $"ASP/{entryNumber}";

	public override ZString UniqueNumber => GetUniqueNumber(base.UniqueNumber);

	public override EntryChargeTypeList EntryChargeTypeList => Factory.GetCachedValue<AUASPEntryChargeTypeList>();

	public override bool APInvoiceNumberAlwaysIncludeChargeCode => true;
}
