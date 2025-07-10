using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.BE.Business.Declaration;

public class BETaxOnlyForPivot : CusAddInfo<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
{
	public BETaxOnlyForPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new Tax_OnlyForPivot Data => (Tax_OnlyForPivot)base.Data;

	protected override Type AddInfoType => typeof(Tax_OnlyForPivot);
}
