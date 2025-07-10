using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class GuaranteesModule : Customs.Module.GuaranteesModule
	{
		public GuaranteesModule()
		{
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GuaranteesFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GuaranteesFilterStripBusinessObject();
	}
}
