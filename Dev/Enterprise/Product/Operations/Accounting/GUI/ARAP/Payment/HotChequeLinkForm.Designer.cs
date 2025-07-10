using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class HotChequeLinkForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			this.HotChequesGrid = new ZDisplayGrid();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.SelectButton = new Core.Forms.ZPostOrCancelButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HotChequesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 274, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 23, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(184);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(HotChequeLink);
			// 
			// HotChequesGrid
			// 
			this.HotChequesGrid.AllowNavigation = false;
			this.HotChequesGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HotChequesGrid, "HotCheques");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).AQ_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).AQ_ChequePayee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).AQ_AK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).AccChequeBooks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).AQ_ChequeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).AQ_Calc_RX_NK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccHotCheque)(((System.Collections.IList)(((HotChequeLink)(null)).HotCheques)).SyncRoot)).Currencies)));
			this.HotChequesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AQ_Amount";
			zTextBoxColumnStyleInfo1.ColumnName = "AQ_ChequePayee";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("HotChequeLinkForm|33f98e94-8934-4df5-865d-2063a292e243", "Check");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AQ_AK";
			zTextBoxColumnStyleInfo2.ColumnName = "AQ_ChequeNumber";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("HotChequeLinkForm|f8121644-5087-4914-bd5e-1ea4934e4ec5", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AQ_Calc_RX_NK";
			this.HotChequesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.HotChequesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HotChequesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.HotChequesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HotChequesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.HotChequesGrid.GridId = "a227475e-b0c7-4dc5-9de7-f0f8a5a999b5";
			this.HotChequesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HotChequesGrid.IsWholeRowSelectedOnClick = true;
			this.HotChequesGrid.LayoutKey = "zGrid1";
			this.HotChequesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.HotChequesGrid.Name = "HotChequesGrid";
			this.HotChequesGrid.ReadOnly = true;
			this.HotChequesGrid.ShouldSetErrorsOnTabPage = false;
			this.HotChequesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 236, true);
			this.HotChequesGrid.TabIndex = 0;
			this.HotChequesGrid.DoubleClick += new System.EventHandler(this.HandleDoubleClick);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("HotChequeLinkForm|1c49acfd-4d68-45f3-9656-6f4169329c22", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 251, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 22, true);
			this.CloseButton.TabIndex = 2;
			// 
			// SelectButton
			// 
			this.SelectButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 251, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.SelectButton.TabIndex = 1;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// HotChequeLinkForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 297, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("HotChequeLinkForm|903d8d71-7f82-413c-a693-00e2cfdc0390", "Hot Check Link");
			this.Controls.Add(this.SelectButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.HotChequesGrid);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(HotChequeLink);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.ReceiptPayment.HotChequeLink";
			this.Name = "HotChequeLinkForm";
			this.KeyDown += new KeyEventHandler(this.HotChequeLinkForm_KeyDown);
			this.Controls.SetChildIndex(this.HotChequesGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.SelectButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HotChequesGrid)).EndInit();
			this.ResumeLayout(false);
		}
		private Core.Forms.ZPostOrCancelButton CloseButton;
		protected ZDisplayGrid HotChequesGrid;
		private Core.Forms.ZPostOrCancelButton SelectButton;

		private System.ComponentModel.Container components = null;

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#endregion
	}
}
