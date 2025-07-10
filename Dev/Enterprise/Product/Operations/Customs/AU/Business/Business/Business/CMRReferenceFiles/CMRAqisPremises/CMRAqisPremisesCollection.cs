using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisPremisesCollection : BusinessObjectCollection<CMRAqisPremises>
	{
		public CMRAqisPremisesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override void Load()
		{
			base.Load();
			Sort(CMRAqisPremisesSchema.Constants.QP_AQISPremisesName, ListSortDirection.Ascending);
		}
	}
}
