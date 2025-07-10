using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
	{
		public TaxForPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new ESTaxOnlyForPivot this[int index] => (ESTaxOnlyForPivot)Elements[index];

		public new ESTaxOnlyForPivot AddNew()
		{
			return (ESTaxOnlyForPivot)base.AddNew();
		}

		protected new ESTaxOnlyForPivot AddNew(Type type)
		{
			return (ESTaxOnlyForPivot)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(ESTaxOnlyForPivot));
		}
	}
}
