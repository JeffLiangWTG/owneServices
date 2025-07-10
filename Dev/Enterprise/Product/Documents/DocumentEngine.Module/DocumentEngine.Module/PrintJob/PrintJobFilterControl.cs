using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module
{
	public partial class PrintJobFilterControl : ZFilterStripControl
	{
		public PrintJobFilterControl(IBusinessObjectCollection gridCollection, PrintJobFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
