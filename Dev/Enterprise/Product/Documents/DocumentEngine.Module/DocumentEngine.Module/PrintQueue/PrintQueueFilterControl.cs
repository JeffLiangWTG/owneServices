using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module
{
	public partial class PrintQueueFilterControl : ZFilterStripControl
	{
		public PrintQueueFilterControl(IBusinessObjectCollection gridCollection, PrintQueueFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
