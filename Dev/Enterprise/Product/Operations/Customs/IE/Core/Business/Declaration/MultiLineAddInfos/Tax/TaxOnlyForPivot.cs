using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class TaxOnlyForPivot : CusAddInfo<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
	{
		public TaxOnlyForPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new Tax_OnlyForPivot Data => (Tax_OnlyForPivot)base.Data;

		protected override Type AddInfoType => typeof(Tax_OnlyForPivot);
	}
}
