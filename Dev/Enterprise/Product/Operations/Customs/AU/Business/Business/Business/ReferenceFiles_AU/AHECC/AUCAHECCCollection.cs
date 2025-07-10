using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCAHECCCollection : DependentBusinessObjectCollection<AUCAHECC, BusinessObject>
	{
		public AUCAHECCCollection(BusinessObject parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(AUCAHECC.Schema.UA_Index, ListSortDirection.Ascending);
		}
	}
}
