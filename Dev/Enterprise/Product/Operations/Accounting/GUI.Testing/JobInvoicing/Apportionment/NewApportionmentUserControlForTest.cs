using System;
using Enterprise.Accounting.Business.ConsolCosting;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	public class NewApportionmentUserControlForTest : NewApportionmentUserControl
	{
		public NewApportionmentUserControlForTest(ApportionmentListing apportionments)
			: base(apportionments)
		{
		}

		public ZArchitecture.GUI.ZPanel ExtraTaxPanel_Exposed
		{
			get { return base.ExtraTaxPanel; }
		}

		public ZArchitecture.ZGrid CostSummaryGrid_Exposed
		{
			get { return base.CostSummaryGrid; }
		}

		public void OnLoad_Exposed(EventArgs e)
		{
			base.OnLoad(e);
		}
	}
}
