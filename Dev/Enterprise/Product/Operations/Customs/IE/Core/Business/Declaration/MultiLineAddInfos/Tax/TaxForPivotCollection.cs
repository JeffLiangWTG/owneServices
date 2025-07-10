using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
	{
		public TaxForPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new TaxOnlyForPivot this[int index] => (TaxOnlyForPivot)Elements[index];

		public new TaxOnlyForPivot AddNew() => (TaxOnlyForPivot)base.AddNew();

		protected new TaxOnlyForPivot AddNew(Type type) => (TaxOnlyForPivot)base.AddNew(type);

		protected override BusinessObject AddNewCore() => AddNew(typeof(TaxOnlyForPivot));
	}
}
