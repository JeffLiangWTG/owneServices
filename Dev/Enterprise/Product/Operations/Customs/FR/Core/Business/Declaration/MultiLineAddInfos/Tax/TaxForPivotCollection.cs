using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
	{
		public TaxForPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new FRTaxOnlyForPivot this[int index] => (FRTaxOnlyForPivot)Elements[index];

		public new FRTaxOnlyForPivot AddNew()
		{
			return (FRTaxOnlyForPivot)base.AddNew();
		}

		protected new FRTaxOnlyForPivot AddNew(Type type)
		{
			return (FRTaxOnlyForPivot)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(FRTaxOnlyForPivot));
		}
	}
}
