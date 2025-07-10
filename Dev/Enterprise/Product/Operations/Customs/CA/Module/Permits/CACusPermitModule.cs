using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module
{
	public class CACusPermitModule : CusPermitModule
	{
		public CACusPermitModule()
		{
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CACusPermitFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override CusPermitFilterStripBusinessObject GetFilterStripBusinessObject()
		{
			return new CACusPermitFilterStripBusinessObject();
		}
	}
}
