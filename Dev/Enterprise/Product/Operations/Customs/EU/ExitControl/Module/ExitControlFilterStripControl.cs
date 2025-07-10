using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public partial class ExitControlFilterStripControl : ZFilterStripControl
	{
		public ExitControlFilterStripControl(IBusinessObjectCollection gridCollection, ExitControlFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
