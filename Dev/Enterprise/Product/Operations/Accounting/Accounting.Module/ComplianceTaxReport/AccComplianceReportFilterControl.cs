using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for GLJournal.
	/// </summary>
	public partial class AccComplianceReportFilterControl : ZFilterStripControl
	{
		public AccComplianceReportFilterControl()
		{
			InitializeComponent();
		}

		public AccComplianceReportFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
