using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class ESH7BillModule : EUH7BillModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ESH7BillFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new ESH7BillFilterStripControl(GridCollection, base.FilterBusinessObject as ESH7BillFilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new EUH7BillModuleCollection<AsycudaBill>(Factory);
	}
}
