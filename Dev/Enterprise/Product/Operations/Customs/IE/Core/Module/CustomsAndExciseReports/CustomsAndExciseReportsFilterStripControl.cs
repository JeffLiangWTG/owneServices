using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.Module
{
	public partial class CustomsAndExciseReportsFilterStripControl : ZFilterStripControl
	{
		public CustomsAndExciseReportsFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
