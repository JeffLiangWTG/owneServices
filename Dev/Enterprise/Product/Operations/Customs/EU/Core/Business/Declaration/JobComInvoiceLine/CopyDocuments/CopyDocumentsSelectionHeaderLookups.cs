using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration;

public class CopyDocumentsSelectionHeaderLookups(CopyDocumentsSelectionHeader parent) : ZLookups(parent)
{
	public new CopyDocumentsSelectionHeader Parent => (CopyDocumentsSelectionHeader)base.Parent;

	public CodeDescriptionPairList Invoices
	{
		get
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(AllCode, AllDescription);
			list.DefaultCode = AllCode;
			list.AddRange(Parent.Declaration.SortedInvoiceList);
			return list;
		}
	}

	public string AllCode => Res.GetString("177dc2d4-d943-416e-8f35-322098f3cd86", "ALL");

	public string AllDescription => Res.GetString("2152a6b9-424f-4c65-9967-2cc0cc7646c7", "All Invoices");
}
