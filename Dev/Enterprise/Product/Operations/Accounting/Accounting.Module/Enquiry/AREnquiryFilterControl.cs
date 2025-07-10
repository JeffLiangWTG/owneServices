using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.Module
{
	public partial class AREnquiryFilterControl : EnquiryFilterControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public AREnquiryFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject filterBizO)
				: base(collection, filterBizO)
		{
			InitializeComponent();
			this.FilteredGrid.ColumnLayoutContext = nameof(FilteredGridColumnLayoutContext.AR);
			MissingResourceStringChecker.ExcludeFromTest(this.SettlementGroupInfoLabel);

			AverageDaysToFullyPayGroupBox.AllowOverlap(AccountBalanceGroupBox);
			GlobalAccountCreditStatusGroupBox.AllowOverlap(AccountBalanceGroupBox);
		}

		#region Virtual Display Overrides

		protected override bool HasDisbursementFields
		{
			get { return true; }
		}

		#endregion

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control == CurrentTotalCalcEdit && previousControl == OnePeriodTotalCalcEdit)
				|| (control == OnePeriodTotalCalcEdit && previousControl == TwoPeriodTotalCalcEdit)
				|| (control == TwoPeriodTotalCalcEdit && previousControl == ThreePeriodTotalCalcEdit)
				|| (control == CurrentCalcEdit && previousControl == TotalOutstandingAmountCalcEdit);
		}
	}
}
