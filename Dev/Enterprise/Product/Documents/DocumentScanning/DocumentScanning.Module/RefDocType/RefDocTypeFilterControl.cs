using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.Module
{
	public partial class RefDocTypeFilterControl : ZFilterStripControl
	{
		public RefDocTypeFilterControl()
		{
			InitializeComponent();
		}

		public RefDocTypeFilterControl(IBusinessObjectCollection gridCollection, RefDocTypeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
