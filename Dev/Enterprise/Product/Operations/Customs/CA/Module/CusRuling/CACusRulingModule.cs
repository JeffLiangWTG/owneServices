using Enterprise.Customs.Universal.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CACusRulingModule : ZZRefCusRulingModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.CACusRuling;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CACusRulingFilterStripBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new CACusRulingFilterStripControl(GridCollection, FilterBusinessObject);
		}
	}
}
