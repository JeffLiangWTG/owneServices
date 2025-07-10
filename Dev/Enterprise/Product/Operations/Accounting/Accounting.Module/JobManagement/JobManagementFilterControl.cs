using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementFilterControl : JobManagementFilterControlBase
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new JobManagementFilterStrip();
		}

		public JobManagementFilterControl()
		{
			InitializeComponent();
		}

		public JobManagementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
