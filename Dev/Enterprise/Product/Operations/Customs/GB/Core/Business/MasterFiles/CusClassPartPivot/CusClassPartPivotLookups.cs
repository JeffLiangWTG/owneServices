using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.MasterFiles
{
	public class CusClassPartPivotLookups : EU.Business.MasterFiles.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(EU.Business.MasterFiles.CusClassPartPivot parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GoodsCategoryListCore() => new GoodsCategoryList();
	}
}
