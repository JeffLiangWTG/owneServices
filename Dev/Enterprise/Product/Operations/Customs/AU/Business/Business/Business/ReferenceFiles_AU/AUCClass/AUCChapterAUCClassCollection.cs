using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUCChapterAUCClassCollection : BusinessObjectCollection<AUCClass>
{
	public AUCChapterAUCClassCollection(BusinessObjectFactory factory, ZQuery sQLFilter)
		: base(factory, sQLFilter)
	{
	}

	public override void Load()
	{
		base.Load();
		Sort(AUCClass.Schema.UJ_Code, ListSortDirection.Ascending);
	}
}
