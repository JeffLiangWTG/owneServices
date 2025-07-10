using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public partial class StlCollectorForm(StlCollectorForDisplayCollection collectorForDisplay) : ZChildForm(collectorForDisplay)
	{
		public override string FormCaption
		{
			get { return Res.GetString("Enterprise.Billing.StlCollector.Retriever.StlCollectorForDisplayCollection", "STL Collectors"); }
		}
	}
}
