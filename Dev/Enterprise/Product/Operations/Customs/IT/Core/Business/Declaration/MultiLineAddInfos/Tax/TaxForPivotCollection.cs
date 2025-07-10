using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.Declaration;

public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
{
	public TaxForPivotCollection(BusinessObject master) : base(master)
	{
	}

	public new ITTaxOnlyForPivot this[int index] => (ITTaxOnlyForPivot)Elements[index];

	public new ITTaxOnlyForPivot AddNew() => (ITTaxOnlyForPivot)base.AddNew();

	protected new ITTaxOnlyForPivot AddNew(Type type) => (ITTaxOnlyForPivot)base.AddNew(type);

	protected override BusinessObject AddNewCore() => AddNew(typeof(ITTaxOnlyForPivot));
}
