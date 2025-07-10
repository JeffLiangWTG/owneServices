using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Testing
{
	[SuppressFormDesignerAnalysis]
	public class FilterStripControlForTest : ZFilterStripControl
	{
		public FilterStripControlForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			if (gridCollection != null)
			{
				Grid.DataSource = gridCollection;
			}
		}
	}
}
