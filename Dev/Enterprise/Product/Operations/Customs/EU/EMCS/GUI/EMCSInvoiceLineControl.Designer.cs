
namespace Enterprise.Customs.EU.EMCS.GUI
{
	sealed partial class EMCSInvoiceLineControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>		
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.LineDetailTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.LineDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClassificationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClassificationDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ArrivalInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EMCSInvoiceLinePackagesUserControl = new Enterprise.Customs.EU.EMCS.GUI.EMCSInvoiceLinePackagesUserControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LineDetailTabControl.SuspendLayout();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.PackagesTabPage.SuspendLayout();
			this.EMCSInvoiceLinePackagesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLineViewCollection);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LineDetailTabControl.Controls.Add(this.LineDetailsTabPage);
			this.LineDetailTabControl.Controls.Add(this.ArrivalInformationTabPage);
			this.LineDetailTabControl.Controls.Add(this.PackagesTabPage);
			this.LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineDetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LineDetailTabControl.Name = "LineDetailTabControl";
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 397, true);
			this.LineDetailTabControl.TabIndex = 0;
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("EMCSInvoiceLineControl|e421a144-f253-4d22-aca6-7b4ce3d90215", "Line Details");
			this.LineDetailsTabPage.Controls.Add(this.ClassificationDetailsGroupBox);
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LineDetailsTabPage.Name = "LineDetailsTabPage";
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 370, true);
			this.LineDetailsTabPage.TabIndex = 0;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("7c5c2f28-6592-4ff0-98fa-6bdcafd07939", "Item");
			this.ClassificationDetailsGroupBox.Controls.Add(this.ClassificationDetailsDynamicLayoutPanel);
			this.ClassificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClassificationDetailsGroupBox, false);
			this.ClassificationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassificationDetailsGroupBox.Name = "ClassificationDetailsGroupBox";
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 370, true);
			this.ClassificationDetailsGroupBox.TabIndex = 1;
			this.ClassificationDetailsGroupBox.TabStop = false;
			// 
			// ClassificationDetailsDynamicLayoutPanel
			// 
			this.ClassificationDetailsDynamicLayoutPanel.AllowDrop = true;
			this.ClassificationDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClassificationDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ClassificationDetailsDynamicLayoutPanel.Name = "ClassificationDetailsDynamicLayoutPanel";
			this.ClassificationDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 351, true);
			this.ClassificationDetailsDynamicLayoutPanel.TabIndex = 6;
			// 
			// ArrivalInformationTabPage
			// 
			this.ArrivalInformationTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("a22d87d2-d1de-4f9e-b953-eb5baa34d13b", "Arrival Information");
			this.ArrivalInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ArrivalInformationTabPage.Name = "ArrivalInformationTabPage";
			this.ArrivalInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 370, true);
			this.ArrivalInformationTabPage.TabIndex = 1;
			// 
			// PackagesTabPage
			// 
			this.PackagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.PackagesTabPage.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("e8a8fc72-3bc3-4ccc-8479-a0fdb779229f", "Packages");
			this.PackagesTabPage.Controls.Add(this.EMCSInvoiceLinePackagesUserControl);
			this.PackagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackagesTabPage.Name = "PackagesTabPage";
			this.PackagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 370, true);
			this.PackagesTabPage.TabIndex = 2;
			// 
			// EMCSInvoiceLinePackagesUserControl
			// 
			this.EMCSInvoiceLinePackagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EMCSInvoiceLinePackagesUserControl, "EMCSPackagePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(((Enterprise.Customs.EU.EMCS.Business.NonPersistentPackagePivot)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobComInvoiceLine)(null)).EMCSPackagePivots)).SyncRoot)))));
			this.EMCSInvoiceLinePackagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EMCSInvoiceLinePackagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EMCSInvoiceLinePackagesUserControl.Name = "EMCSInvoiceLinePackagesUserControl";
			this.EMCSInvoiceLinePackagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 364, true);
			this.EMCSInvoiceLinePackagesUserControl.TabIndex = 21;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LineDetailTabControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(370);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(183);
			this.SplitContainer.TabIndex = 21;
			// 
			// EMCSInvoiceLineControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "EMCSInvoiceLineControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.LineDetailsTabPage.PerformLayout();
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.PackagesTabPage.ResumeLayout(false);
			this.PackagesTabPage.PerformLayout();
			this.EMCSInvoiceLinePackagesUserControl.ResumeLayout(true);
			this.EMCSInvoiceLinePackagesUserControl.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZTabPage ArrivalInformationTabPage;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl LineDetailTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage LineDetailsTabPage;
		private ZArchitecture.GUI.ZGroupBox ClassificationDetailsGroupBox;
		private ZArchitecture.GUI.ZTabPage PackagesTabPage;
		private EMCSInvoiceLinePackagesUserControl EMCSInvoiceLinePackagesUserControl;
		internal ZArchitecture.GUI.DynamicLayoutPanel ClassificationDetailsDynamicLayoutPanel;
	}
}
