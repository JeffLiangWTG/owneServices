using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.Module
{
	public partial class ViewComponentChangeLogFilterControl : ZFilterStripControl
	{
		public ViewComponentChangeLogFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			((ZDisplayGrid)Grid).ForceShowExportToExcelMenuItem = true;
		}
	}
}
