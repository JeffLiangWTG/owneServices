using System;
using System.ComponentModel;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class JobProfitLossTotalsControl : ZUserControl
	{
		public JobProfitLossTotalsControl()
		{
			InitializeComponent();

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(MarginProfitCost);
			MissingResourceStringChecker.ExcludeFromTest(MarginProfitCostNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(MarginProfitCostRecognized);
			MissingResourceStringChecker.ExcludeFromTest(MarginProfitRev);
			MissingResourceStringChecker.ExcludeFromTest(MarginProfitRevNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(MarginProfitRevRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalRevenue);
			MissingResourceStringChecker.ExcludeFromTest(TotalRevenueNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalRevenueRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalWIP);
			MissingResourceStringChecker.ExcludeFromTest(TotalWIPNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalWIPRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalCost);
			MissingResourceStringChecker.ExcludeFromTest(TotalCostNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalCostRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalAccrual);
			MissingResourceStringChecker.ExcludeFromTest(TotalAccrualNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalAccrualRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalLineAmount);
			MissingResourceStringChecker.ExcludeFromTest(TotalLineAmountNotRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalLineAmountRecognized);
			MissingResourceStringChecker.ExcludeFromTest(TotalTaxExpenseRevenue);
			MissingResourceStringChecker.ExcludeFromTest(TotalTaxExpenseCost);
#endif
		}

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<JobProfitLossTotalsControl>().Result;
		}

		#endregion

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var profitLossTotals = CurrentDataItem as JobProfitLoss;
			if (profitLossTotals != null && !profitLossTotals.IsTaxExpenseSupported)
			{
				TaxExpensePanel.Visible = false;
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			var profitLoss = CurrentDataItem as JobProfitLoss;
			if (profitLoss != null && !profitLoss.RefreshBindingSuspender.IsSuspended)
			{
				base.OnBindingContextChanged(e);
			}
		}

		#region Binding

		string fBindPrepend;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (string.IsNullOrEmpty(fBindPrepend))
			{
				fBindPrepend = dataMember + ".";

				TotalRevenue.BindToDecimalPlaces = fBindPrepend + TotalRevenue.BindToDecimalPlaces;
				TotalRevenueRecognized.BindToDecimalPlaces = fBindPrepend + TotalRevenueRecognized.BindToDecimalPlaces;
				TotalRevenueNotRecognized.BindToDecimalPlaces = fBindPrepend + TotalRevenueNotRecognized.BindToDecimalPlaces;
				TotalWIP.BindToDecimalPlaces = fBindPrepend + TotalWIP.BindToDecimalPlaces;
				TotalWIPRecognized.BindToDecimalPlaces = fBindPrepend + TotalWIPRecognized.BindToDecimalPlaces;
				TotalWIPNotRecognized.BindToDecimalPlaces = fBindPrepend + TotalWIPNotRecognized.BindToDecimalPlaces;
				TotalAccrual.BindToDecimalPlaces = fBindPrepend + TotalAccrual.BindToDecimalPlaces;
				TotalAccrualRecognized.BindToDecimalPlaces = fBindPrepend + TotalAccrualRecognized.BindToDecimalPlaces;
				TotalAccrualNotRecognized.BindToDecimalPlaces = fBindPrepend + TotalAccrualNotRecognized.BindToDecimalPlaces;
				TotalCost.BindToDecimalPlaces = fBindPrepend + TotalCost.BindToDecimalPlaces;
				TotalCostRecognized.BindToDecimalPlaces = fBindPrepend + TotalCostRecognized.BindToDecimalPlaces;
				TotalCostNotRecognized.BindToDecimalPlaces = fBindPrepend + TotalCostNotRecognized.BindToDecimalPlaces;
				TotalLineAmount.BindToDecimalPlaces = fBindPrepend + TotalLineAmount.BindToDecimalPlaces;
				TotalLineAmountRecognized.BindToDecimalPlaces = fBindPrepend + TotalLineAmountRecognized.BindToDecimalPlaces;
				TotalLineAmountNotRecognized.BindToDecimalPlaces = fBindPrepend + TotalLineAmountNotRecognized.BindToDecimalPlaces;
				TotalTaxExpenseRevenue.BindToDecimalPlaces = fBindPrepend + TotalTaxExpenseRevenue.BindToDecimalPlaces;
				TotalTaxExpenseCost.BindToDecimalPlaces = fBindPrepend + TotalTaxExpenseCost.BindToDecimalPlaces;
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		#endregion
	}
}
