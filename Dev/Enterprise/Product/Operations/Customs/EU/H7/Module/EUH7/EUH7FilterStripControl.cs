using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.Module
{
	public partial class EUH7FilterStripControl : ZFilterStripControl
	{
		public EUH7FilterStripControl(IBusinessObjectCollection gridCollection, EUH7FilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
