using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.CH.Business;

public class DocEdecBusinessDataWrapper : DocBaseWrapper
{
	public static DocEdecBusinessDataWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
	=> new DocEdecBusinessDataWrapper(entryHeader, factoryToWrap);

	DocEdecBusinessDataWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
	{
		dataProvider = EdecIMPBusinessDataProvider.New(entryHeader);
	}
	readonly EdecIMPBusinessDataProvider dataProvider;

	public ZString CustomsAccount => ParseAccountError(dataProvider.CustomsAccount);

	public ZString VATAccount => ParseAccountError(dataProvider.VATAccount);

	ZString ParseAccountError(string account)
	{
		return account == "0" || account == "-1" ? ZString.Empty : account;
	}
}
