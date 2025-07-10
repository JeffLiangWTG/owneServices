using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.Module
{
	public partial class RefDocSourceFilterControl : ZFilterStripControl
	{
		public RefDocSourceFilterControl()
		{
			InitializeComponent();
		}

		public RefDocSourceFilterControl(IBusinessObjectCollection gridCollection, RefDocSourceFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
