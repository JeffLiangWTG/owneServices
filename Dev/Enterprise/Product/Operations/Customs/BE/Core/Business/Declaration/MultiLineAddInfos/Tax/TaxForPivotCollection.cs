using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.BE.Business.Declaration;

public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
{
	public TaxForPivotCollection(BusinessObject master) : base(master)
	{
	}

	public new BETaxOnlyForPivot this[int index] => (BETaxOnlyForPivot)Elements[index];

	public new BETaxOnlyForPivot AddNew()
	{
		return (BETaxOnlyForPivot)base.AddNew();
	}

	protected new BETaxOnlyForPivot AddNew(Type type)
	{
		return (BETaxOnlyForPivot)base.AddNew(type);
	}

	protected override BusinessObject AddNewCore()
	{
		return AddNew(typeof(BETaxOnlyForPivot));
	}
}
