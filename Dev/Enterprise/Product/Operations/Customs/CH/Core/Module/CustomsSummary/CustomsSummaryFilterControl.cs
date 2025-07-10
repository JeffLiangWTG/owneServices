using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.Module;

public sealed partial class CustomsSummaryFilterControl : ZFilterStripControl
{
	public CustomsSummaryFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject) : base(gridCollection, filterStripBusinessObject)
	{
		InitializeComponent();
	}
}
