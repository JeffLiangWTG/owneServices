using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUCChapterAHECCCollection : BusinessObjectCollection<AUCAHECC>
{
	public AUCChapterAHECCCollection(BusinessObjectFactory factory, ZQuery sQLFilter)
		: base(factory, sQLFilter)
	{
	}

	public override void Load()
	{
		base.Load();
		Sort(AUCAHECC.Schema.UA_Index, ListSortDirection.Ascending);
	}
}
