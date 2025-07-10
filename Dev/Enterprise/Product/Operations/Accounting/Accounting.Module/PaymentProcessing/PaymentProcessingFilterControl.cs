using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for Transaction.
	/// </summary>
	public partial class PaymentProcessingFilterControl : ZFilterStripControl
	{
		public PaymentProcessingFilterControl()
		{
			InitializeComponent();
		}

		public PaymentProcessingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetEPaymentColumnsVisibility();
		}

		void SetEPaymentColumnsVisibility()
		{
			var showEPaymentColumns = AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider;
			if (showEPaymentColumns)
			{
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
				this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
				this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			}
			zTextBoxColumnStyleInfo13.IsVisible = showEPaymentColumns;
			zDateEditColumnStyleInfo3.IsVisible = showEPaymentColumns;
			zDateEditColumnStyleInfo4.IsVisible = showEPaymentColumns;
			zTextBoxColumnStyleInfo14.IsVisible = showEPaymentColumns;
			zTextBoxColumnStyleInfo15.IsVisible = showEPaymentColumns;
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new TransactionModuleFilterStrip();
		}

		ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13;
		ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3;
		ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4;
		ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14;
		ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15;
	}
}

