using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class AgreementCancelledCommissionsControl : ZUserControl
	{
		public AgreementCancelledCommissionsControl(CancelledLinesCollection cancelledLinesCollection, OrgCommissionAgreement agreement)
		{
			this.CancelledLines = cancelledLinesCollection;

			InitializeComponent();

			this.CancelledLinesIDGroupBox.Text = agreement.AgreementId;
			this.OpportunityCancelledLinesGrid.SetDataBinding(CancelledLines, "");

			TypeDescriptor.AddAttributes(CancelledLinesIDGroupBox, new SuppressFormsLocalizedTestAttribute());
		}

		public IEnumerable<ViewCommissionLine> GetCommissionLinesToNotReinstate()
		{
			return CancelledLines.OfType<ExcludeViewCommissionLine>().Where(line => line.IsExcluded).Select(line => line.DataItem);
		}

		public CancelledLinesCollection CancelledLines;
	}
}
