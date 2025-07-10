using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCClassCollection : DependentBusinessObjectCollection<AUCClass, BusinessObject>
	{
		public AUCClassCollection(BusinessObject parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(AUCClass.Schema.UJ_Code, ListSortDirection.Ascending);
		}
	}
}
