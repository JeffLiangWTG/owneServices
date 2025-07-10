using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Module
{
	public partial class CSARevenueSummaryFormStatementFilterControl : StatementFilterControl
	{
		public CSARevenueSummaryFormStatementFilterControl()
		{
			InitializeComponent();
		}

		public CSARevenueSummaryFormStatementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
