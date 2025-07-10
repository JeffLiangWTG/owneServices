using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class CurrencySummaryForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.zGrid1 = new ZDisplayGrid();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 283, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CurrencySummary);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "SummaryRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).AverageExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).AdjustmentNoteTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).ContraTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).CreditNoteTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).DiscountAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).ExchangeDifferenceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).InvoiceTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).JournalTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).OverpaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).PaymentTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).ReceiptTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((CurrencySummary)(null)).SummaryRows)).SyncRoot)).TransferTotal)));
			this.zGrid1.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|7ef70726-8b94-4bee-8985-8c6f6837f694", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Currency";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|cfc0ada9-2f07-4667-98e4-1c2e25ae9ed7", "Total Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|1da8c7a3-4291-4ae4-9741-df6079a33d44", "Average Ex Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "AverageExRate";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|0055801d-667e-4a9a-88d9-b71b93ffd7a8", "Local Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|c1d3be59-1e20-4939-813e-aedcf60da3e3", "Adjustment Note");
			zCalcEditColumnStyleInfo4.ColumnName = "AdjustmentNoteTotal";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|c3b23be0-56f3-430f-9d67-73130575aca7", "Contra");
			zCalcEditColumnStyleInfo5.ColumnName = "ContraTotal";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|f5a0bff7-37dd-4e11-8a4a-c6d76d268982", "Credit Note");
			zCalcEditColumnStyleInfo6.ColumnName = "CreditNoteTotal";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|efd221a8-fe9c-4423-bd16-ed790a4b7629", "Discount");
			zCalcEditColumnStyleInfo7.ColumnName = "DiscountAmount";
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|937d73e4-1c0f-45d3-98a6-965e283e7454", "Exchange Difference");
			zCalcEditColumnStyleInfo8.ColumnName = "ExchangeDifferenceAmount";
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|ccb54dfe-e162-48f4-a495-bd6ca09f04fd", "Invoice");
			zCalcEditColumnStyleInfo9.ColumnName = "InvoiceTotal";
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|7cb23265-1774-4ff8-9e0b-d31e50fb7c88", "Journal");
			zCalcEditColumnStyleInfo10.ColumnName = "JournalTotal";
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|912e3253-ecc7-4f66-94df-7200c860c622", "Overpayment");
			zCalcEditColumnStyleInfo11.ColumnName = "OverpaymentAmount";
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|f4a701dc-9121-4a08-b7b2-345729fd8e19", "Payment");
			zCalcEditColumnStyleInfo12.ColumnName = "PaymentTotal";
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|3464b015-d176-4bac-80f8-9ebbc5912022", "Receipt");
			zCalcEditColumnStyleInfo13.ColumnName = "ReceiptTotal";
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|545dc20e-398e-4676-a5cc-4f97a53beafa", "Transfer");
			zCalcEditColumnStyleInfo14.ColumnName = "TransferTotal";
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.IsWholeRowSelectedOnClick = true;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.ReadOnly = true;
			this.zGrid1.ShouldSetErrorsOnTabPage = false;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 245, true);
			this.zGrid1.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|dad80e1b-e7ad-4f90-93b4-c60ef4d85a81", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 260, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 2;
			// 
			// CurrencySummaryForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 305, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CurrencySummaryForm|44174500-f83f-444a-b8ff-56cf281ef52a", "Summary by Currency");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.zGrid1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(CurrencySummary);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.Matching.CurrencySummary";
			this.Name = "CurrencySummaryForm";
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		private ZDisplayGrid zGrid1;
		private Core.Forms.ZPostOrCancelButton CloseButton;

		private readonly System.ComponentModel.Container components = null;

		#endregion
	}
}
