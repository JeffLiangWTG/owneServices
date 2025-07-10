 
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class UnderbondUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.TsrTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IarTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IsrsTabsPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FallbackTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cusUnderbond_TSR1 = new Enterprise.Customs.GB.GUI.Ccsuk.CusUnderbond_TSR();
			this.cusUnderbond_IAR1 = new Enterprise.Customs.GB.GUI.Ccsuk.CusUnderbond_IAR();
			this.cusUnderbond_ISR1 = new Enterprise.Customs.GB.GUI.Ccsuk.CusUnderbond_ISR();
			this.cusUnderbond_FBK1 = new Enterprise.Customs.GB.GUI.Ccsuk.CusUnderbond_FBK();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.TsrTabPage.SuspendLayout();
			this.IarTabPage.SuspendLayout();
			this.IsrsTabsPage.SuspendLayout();
			this.FallbackTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB);
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.TsrTabPage);
			this.zTabControl1.Controls.Add(this.IarTabPage);
			this.zTabControl1.Controls.Add(this.IsrsTabsPage);
			this.zTabControl1.Controls.Add(this.FallbackTabPage);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 264, true);
			this.zTabControl1.TabIndex = 1;
			// 
			// TsrTabPage
			//
			this.TsrTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fd58cb9c-5e92-40fd-adcf-a7af1b109b0c", "Transhipments");
			this.TsrTabPage.Controls.Add(this.cusUnderbond_TSR1);
			this.TsrTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TsrTabPage.Name = "TsrTabPage";
			this.TsrTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TsrTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 237, true);
			this.TsrTabPage.TabIndex = 0;
			this.TsrTabPage.UseVisualStyleBackColor = true;
			// 
			// IarTabPage
			//
			this.IarTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7ef06e4f-cbf0-4d67-8b83-0d6606eac712", "Inter-Airport Removals");
			this.IarTabPage.Controls.Add(this.cusUnderbond_IAR1);
			this.IarTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IarTabPage.Name = "IarTabPage";
			this.IarTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IarTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 237, true);
			this.IarTabPage.TabIndex = 1;
			this.IarTabPage.UseVisualStyleBackColor = true;
			// 
			// IsrsTabsPage
			//
			this.IsrsTabsPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("491c08df-cde6-465d-b34b-01bc6e413a54", "Inter-Shed Removals");
			this.IsrsTabsPage.Controls.Add(this.cusUnderbond_ISR1);
			this.IsrsTabsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IsrsTabsPage.Name = "IsrsTabsPage";
			this.IsrsTabsPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IsrsTabsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 237, true);
			this.IsrsTabsPage.TabIndex = 2;
			this.IsrsTabsPage.UseVisualStyleBackColor = true;
			// 
			// FallbackTabPage
			//
			this.FallbackTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("16a83e2f-f322-43fe-882c-df3b826e5469", "Fallback");
			this.FallbackTabPage.Controls.Add(this.cusUnderbond_FBK1);
			this.FallbackTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FallbackTabPage.Name = "FallbackTabPage";
			this.FallbackTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FallbackTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 237, true);
			this.FallbackTabPage.TabIndex = 3;
			this.FallbackTabPage.UseVisualStyleBackColor = true;
			// 
			// cusUnderbond_TSR1
			// 
			this.cusUnderbond_TSR1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusUnderbond_TSR1, ".");
			this.cusUnderbond_TSR1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusUnderbond_TSR1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusUnderbond_TSR1.Name = "cusUnderbond_TSR1";
			this.cusUnderbond_TSR1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 231, true);
			this.cusUnderbond_TSR1.TabIndex = 0;
			// 
			// cusUnderbond_IAR1
			// 
			this.cusUnderbond_IAR1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusUnderbond_IAR1, ".");
			this.cusUnderbond_IAR1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusUnderbond_IAR1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusUnderbond_IAR1.Name = "cusUnderbond_IAR1";
			this.cusUnderbond_IAR1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 231, true);
			this.cusUnderbond_IAR1.TabIndex = 0;
			// 
			// cusUnderbond_ISR1
			// 
			this.cusUnderbond_ISR1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusUnderbond_ISR1, ".");
			this.cusUnderbond_ISR1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusUnderbond_ISR1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusUnderbond_ISR1.Name = "cusUnderbond_ISR1";
			this.cusUnderbond_ISR1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 231, true);
			this.cusUnderbond_ISR1.TabIndex = 0;
			// 
			// cusUnderbond_FBK1
			// 
			this.cusUnderbond_FBK1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusUnderbond_FBK1, ".");
			this.cusUnderbond_FBK1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusUnderbond_FBK1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusUnderbond_FBK1.Name = "cusUnderbond_FBK1";
			this.cusUnderbond_FBK1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 231, true);
			this.cusUnderbond_FBK1.TabIndex = 0;
			// 
			// UnderbondUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zTabControl1);
			this.Name = "UnderbondUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 264, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.TsrTabPage.ResumeLayout(false);
			this.IarTabPage.ResumeLayout(false);
			this.IsrsTabsPage.ResumeLayout(false);
			this.FallbackTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl zTabControl1;
		private ZArchitecture.GUI.ZTabPage TsrTabPage;
 

		private ZArchitecture.GUI.ZTabPage IarTabPage;
 

		private ZArchitecture.GUI.ZTabPage IsrsTabsPage;
 
		private ZArchitecture.GUI.ZTabPage FallbackTabPage;
		public CusUnderbond_TSR cusUnderbond_TSR1;
		public CusUnderbond_IAR cusUnderbond_IAR1;
		public CusUnderbond_ISR cusUnderbond_ISR1;
		private CusUnderbond_FBK cusUnderbond_FBK1;
	}
}
