using Enterprise.Core.Forms;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	partial class IntercompanyCostsApportionmentForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.subSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.costsControl = new Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment.CostsControl();
			this.apportionmentDetails = new Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment.ApportionmentControl();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.subSplitContainer.Panel1.SuspendLayout();
			this.subSplitContainer.Panel2.SuspendLayout();
			this.subSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 442, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.mainSplitContainer.IsSplitterFixed = true;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.subSplitContainer);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.PostingButtonsUserControl);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 466, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(431);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// subSplitContainer
			// 
			this.subSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.subSplitContainer.Name = "subSplitContainer";
			this.subSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// subSplitContainer.Panel1
			// 
			this.subSplitContainer.Panel1.Controls.Add(this.costsControl);
			// 
			// subSplitContainer.Panel2
			// 
			this.subSplitContainer.Panel2.Controls.Add(this.apportionmentDetails);
			this.subSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 431, true);
			this.subSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(286);
			this.subSplitContainer.TabIndex = 1;
			// 
			// costsControl
			// 
			this.BindingSource.SetBindingMember(this.costsControl, ".");
			this.costsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.costsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.costsControl.Name = "costsControl";
			this.costsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 286, true);
			this.costsControl.TabIndex = 0;
			// 
			// apportionmentDetails
			// 
			this.BindingSource.SetBindingMember(this.apportionmentDetails, ".");
			this.apportionmentDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.apportionmentDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.apportionmentDetails.Name = "apportionmentDetails";
			this.apportionmentDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 141, true);
			this.apportionmentDetails.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 0, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// IntercompanyCostsApportionmentForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("IntercompanyCostsApportionmentForm|1987d51a-0314-4165-965c-26ce47eb56a7", "Overhead Costs Apportionment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 466, true);
			this.Controls.Add(this.mainSplitContainer);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 470, true);
			this.Name = "IntercompanyCostsApportionmentForm";
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			this.mainSplitContainer.ResumeLayout(false);
			this.subSplitContainer.Panel1.ResumeLayout(false);
			this.subSplitContainer.Panel2.ResumeLayout(false);
			this.subSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer subSplitContainer;
		private ZPostingButtonsUserControl PostingButtonsUserControl;
		private ApportionmentControl apportionmentDetails;
		private CostsControl costsControl;

	}
}
