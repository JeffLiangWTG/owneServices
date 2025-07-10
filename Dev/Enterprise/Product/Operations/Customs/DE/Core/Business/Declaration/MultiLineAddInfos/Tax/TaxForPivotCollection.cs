using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.DE.Business
{
	public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
	{
		public TaxForPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new DETax_OnlyForPivot this[int index] => (DETax_OnlyForPivot)Elements[index];

		public new DETax_OnlyForPivot AddNew() => (DETax_OnlyForPivot)base.AddNew();

		protected new DETax_OnlyForPivot AddNew(Type type) => (DETax_OnlyForPivot)base.AddNew(type);

		protected override BusinessObject AddNewCore() => AddNew(typeof(DETax_OnlyForPivot));
	}
}
