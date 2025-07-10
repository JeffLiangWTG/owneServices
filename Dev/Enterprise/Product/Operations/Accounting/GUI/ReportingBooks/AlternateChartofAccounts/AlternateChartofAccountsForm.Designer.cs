using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	partial class AlternateChartofAccountsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (AlternateChartFormatsControl != null)
				{
					AlternateChartFormatsControl.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
		ZPanel AccountingFormatPanel;
		ZTabControl AccountingFormatTabControl;
		ZTabPage AccountingFormatTabPage;
		AlternateChartFormatsControl AlternateChartFormatsControl;
		private ZGroupBox AlternateChartGroupBox;
		ZArchitecture.ZTextBox zChartCode;
		ZArchitecture.ZTextBox zChartName;
		ZCheckBox zIsGlobal;
		ZCheckBox zFixedLength;
		ZDropEdit zBalanceSheetStyle;
		ZTabPage CurrencyTranslationTabPage;
		AlternateChartCurrencyTranslationControl CurrencyTranslationsControl;

		protected override void InitializeComponent()
		{
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 435, true);
			this.components = new System.ComponentModel.Container();
			this.AlternateChartFormatsControl = new Enterprise.Accounting.GUI.AlternateChartFormatsControl();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(892, 349, true);
			this.MainTabControl.TabIndex = 8;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AlternateChartofAccountsForm|1E99FCA6-79C7-4DFE-9147-8A4268CE8193", "Chart Details");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 325, true);
			this.MainTabPage.TabIndex = 1;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 325, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(892, 349, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(591, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(892, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccAlternateChart);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_IsGlobal)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_IsFixedLength)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AAC_BalanceSheetStyle)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(((Enterprise.MasterFiles.Business.AccAlternateChartFormat)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AlternateChartFormats)).SyncRoot)))));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(((Enterprise.MasterFiles.Business.AccAlternateChartCurrencyTranslation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccAlternateChart)(null)).AccAlternateChartCurrencyTranslations)).SyncRoot)))));
			// 
			// AlternateChartofAccountsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AlternateChartofAccountsForm|DC9AC685-2E4E-4520-B68A-DADF05A6462A", "Alternate Chart of Accounts");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(892, 405, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccAlternateChart);
			this.Name = "AlternateChartofAccountsForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void AccountingFormatTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AlternateChartFormatsControl.SuspendLayout();
			// 
			// AlternateChartFormatsControl
			// 
			this.AlternateChartFormatsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlternateChartFormatsControl, "AlternateChartFormats");
			this.AlternateChartFormatsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternateChartFormatsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AlternateChartFormatsControl.Name = "AlternateChartFormatsControl";
			this.AlternateChartFormatsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 174, true);
			this.AlternateChartFormatsControl.TabIndex = 6;
			this.AlternateChartFormatsControl.ResumeLayout(true);
			this.AlternateChartFormatsControl.PerformLayout();
		}

		void ChartCodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = !char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
		}

		void BalanceSheetStyle_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				((CargoWise.Windows.UI.KTextBox)sender).Text = "";
				e.Handled = true;
			}
		}
	}
}
