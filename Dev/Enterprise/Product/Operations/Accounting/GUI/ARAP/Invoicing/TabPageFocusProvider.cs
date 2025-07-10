using CargoWise.Common;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	// Internal GUI helper classes for tab page auto-switch.
	internal abstract class TabPageFocusProvider
	{
		public TabPageFocusProvider(ZTabPage tabPageToFocus)
		{
			TabPageToFocus = tabPageToFocus;
		}

		public abstract bool IsNeedFocus(string focusedCellName);

		public ZTabPage TabPageToFocus { get; private set; }
	}

	internal class SubAccountsTabPageFocusProvider : TabPageFocusProvider
	{
		public SubAccountsTabPageFocusProvider(ZTabPage tabPageToFocus) : base(tabPageToFocus)
		{
		}

		public override bool IsNeedFocus(string focusedCellName)
		{
			return focusedCellName.In(
				DependentTransactionLine.Schema.AL_Calc_SecondSubClassParent,
				DependentTransactionLine.Schema.AL_Calc_FirstSubClassParent,
				DependentTransactionLine.Schema.AL_Calc_SecondSubClassParentId,
				DependentTransactionLine.Schema.AL_Calc_FirstSubClassParentId);
		}
	}

	internal class PeriodApportionmentTabPageFocusProvider : TabPageFocusProvider
	{
		public PeriodApportionmentTabPageFocusProvider(ZTabPage tabPageToFocus) : base(tabPageToFocus)
		{
		}

		public override bool IsNeedFocus(string focusedCellName)
		{
			return focusedCellName.In(
				nameof(InvoicingLineBase.PeriodApportionmentMethod),
				nameof(InvoicingLineBase.PeriodStartDate),
				nameof(InvoicingLineBase.PeriodEndDate),
				nameof(InvoicingLineBase.PeriodClearingGLAccountPK));
		}
	}
}
