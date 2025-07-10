using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	public partial class MonthlyClosingFilterStripControl : ZFilterStripControl
	{
		public MonthlyClosingFilterStripControl(IBusinessObjectCollection gridCollection, MonthlyClosingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
