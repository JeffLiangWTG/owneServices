using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class CusPermitModule : Customs.Module.CusPermitModule
	{
		public CusPermitModule()
		{
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusPermitFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override Customs.Module.CusPermitFilterStripBusinessObject GetFilterStripBusinessObject()
		{
			return new CusPermitFilterStripBusinessObject();
		}
	}
}
