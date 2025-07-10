using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AlternateChartofAccountsForm : ZTemplateForm
	{
		public AlternateChartofAccountsForm(AccAlternateChart chart) : base(chart)
		{
		}

		public new AccAlternateChart BusinessEntity
		{
			get { return (AccAlternateChart)base.BusinessEntity; }
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		protected override bool ShowAuditTab => true;

		protected override void OnShown(System.EventArgs e)
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.AccountingReportingBookFeature);

			if (featureData != null && featureData.TryDeserializeParameterAsJson<ReportingBookFeatureControlData>(out var reportingBookFeatureControlData))
			{
				if (reportingBookFeatureControlData.EnableCurrencyTranslation)
				{
					this.CurrencyTranslationTabPage.TabVisible = true;
				}
			}
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AlternateChartGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zChartCode = new Enterprise.ZArchitecture.ZTextBox();
			this.zChartName = new Enterprise.ZArchitecture.ZTextBox();
			this.zIsGlobal = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zFixedLength = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zBalanceSheetStyle = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AccountingFormatTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AccountingFormatPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AccountingFormatTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CurrencyTranslationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CurrencyTranslationsControl = new AlternateChartCurrencyTranslationControl();

			this.MainTabPage.SuspendLayout();
			this.AlternateChartGroupBox.SuspendLayout();
			this.zBalanceSheetStyle.SuspendLayout();
			this.AccountingFormatTabPage.SuspendLayout();
			this.CurrencyTranslationTabPage.SuspendLayout();
			this.AccountingFormatTabControl.SuspendLayout();
			this.AccountingFormatPanel.SuspendLayout();
			this.MainTabPage.Controls.Add(this.AlternateChartGroupBox);
			this.MainTabPage.Controls.Add(this.AccountingFormatPanel);
			AlternateChartGroupBox.AllowOverlap(AccountingFormatPanel);
			// 
			// AlternateChartGroupBox
			// 
			this.AlternateChartGroupBox.AutoSize = true;
			this.AlternateChartGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AlternateChartGroupBox.Controls.Add(this.zChartCode);
			this.AlternateChartGroupBox.Controls.Add(this.zChartName);
			this.AlternateChartGroupBox.Controls.Add(this.zIsGlobal);
			this.AlternateChartGroupBox.Controls.Add(this.zFixedLength);
			this.AlternateChartGroupBox.Controls.Add(this.zBalanceSheetStyle);
			this.AlternateChartGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AlternateChartGroupBox, false);
			this.AlternateChartGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AlternateChartGroupBox.Name = "AlternateChartGroupBox";
			this.AlternateChartGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 123, true);
			this.AlternateChartGroupBox.TabIndex = 0;
			this.AlternateChartGroupBox.TabStop = false;
			// 
			// zChartCode
			// 
			this.BindingSource.SetBindingMember(this.zChartCode, "AAC_Code");
			this.zChartCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 25, true);
			this.zChartCode.Name = "zChartCode";
			this.zChartCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zChartCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.zChartCode.TabIndex = 1;
			this.zChartCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ChartCodeTextBox_KeyPress);
			// 
			// zChartName
			// 
			this.BindingSource.SetBindingMember(this.zChartName, "AAC_Description");
			this.zChartName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 53, true);
			this.zChartName.Name = "zChartName";
			this.zChartName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 20, true);
			this.zChartName.TabIndex = 4;
			// 
			// zIsGlobal
			// 
			this.BindingSource.SetBindingMember(this.zIsGlobal, "AAC_IsGlobal");
			this.zIsGlobal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 22, true);
			this.zIsGlobal.Name = "zIsGlobal";
			this.zIsGlobal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 24, true);
			this.zIsGlobal.TabIndex = 2;
			this.zIsGlobal.UseVisualStyleBackColor = true;
			// 
			// zFixedLength
			// 
			this.BindingSource.SetBindingMember(this.zFixedLength, "AAC_IsFixedLength");
			this.zFixedLength.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 22, true);
			this.zFixedLength.Name = "zFixedLength";
			this.zFixedLength.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 24, true);
			this.zFixedLength.TabIndex = 3;
			this.zFixedLength.UseVisualStyleBackColor = true;
			// 
			// zBalanceSheetStyle
			// 
			this.zBalanceSheetStyle.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zBalanceSheetStyle, "AAC_BalanceSheetStyle");
			this.zBalanceSheetStyle.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AlternateChartofAccountsForm|806428F4-A776-452C-B34F-66FE5181BECA", "Balance Sheet Style");
			this.zBalanceSheetStyle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 84, true);
			this.zBalanceSheetStyle.Name = "zBalanceSheetStyle";
			this.zBalanceSheetStyle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.zBalanceSheetStyle.TabIndex = 7;
			this.zBalanceSheetStyle.CodeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BalanceSheetStyle_KeyPress);
			// 
			// AccountingFormatTabControl
			// 
			this.AccountingFormatTabControl.Controls.Add(this.AccountingFormatTabPage);
			this.AccountingFormatTabControl.Controls.Add(this.CurrencyTranslationTabPage);
			this.AccountingFormatTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 126, true);
			this.AccountingFormatTabControl.Name = "AccountingFormatTabControl";
			this.AccountingFormatTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 192, true);
			this.AccountingFormatTabControl.TabIndex = 8;
			// 
			// AccountingFormatPanel
			// 
			this.AccountingFormatPanel.Controls.Add(this.AccountingFormatTabControl);
			this.AccountingFormatPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountingFormatPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 126, true);
			this.AccountingFormatPanel.Name = "AccountingFormatPanel";
			this.AccountingFormatPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 336, true);
			this.AccountingFormatPanel.TabIndex = 1;
			// 
			// AccountingFormatTabPage
			// 
			this.AccountingFormatTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AlternateChartofAccountsForm|626A63EE-3D9E-4787-9E9A-6AE598CD9FCC", "Account Format");
			this.AccountingFormatTabPage.Controls.Add(this.AlternateChartFormatsControl);
			this.AccountingFormatTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.AccountingFormatTabPage.Name = "AccountingFormatTabPage";
			this.AccountingFormatTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 174, true);
			this.AccountingFormatTabPage.TabIndex = 5;
			this.AccountingFormatTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AccountingFormatTabPage_InitializeTab));
			// 
			// CurrencyTranslationTabPage
			// 
			this.CurrencyTranslationTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AlternateChartofAccountsForm|78AE6158-B2FF-45DB-9F99-F671F924920D", "Currency Translation");
			this.CurrencyTranslationTabPage.Controls.Add(this.CurrencyTranslationsControl);
			this.CurrencyTranslationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.CurrencyTranslationTabPage.Name = "CurrencyTranslationTabPage";
			this.CurrencyTranslationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 174, true);
			this.CurrencyTranslationTabPage.TabIndex = 6;
			this.CurrencyTranslationTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CurrencyTranslationTabPage_InitializeTab));
			this.CurrencyTranslationTabPage.TabVisible = false;
			this.MainTabPage.PerformLayout();
			this.AlternateChartGroupBox.ResumeLayout(false);
			this.AlternateChartGroupBox.PerformLayout();
			this.zBalanceSheetStyle.ResumeLayout(true);
			this.zBalanceSheetStyle.PerformLayout();
			this.AccountingFormatTabControl.ResumeLayout(false);
			this.AccountingFormatTabControl.PerformLayout();
			this.AccountingFormatPanel.ResumeLayout(false);
			this.AccountingFormatPanel.PerformLayout();
			this.AccountingFormatTabPage.ResumeLayout(false);
			this.AccountingFormatTabPage.PerformLayout();
			this.CurrencyTranslationTabPage.ResumeLayout(false);
			this.CurrencyTranslationTabPage.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		public ZGroupBox GetAlternateChartGroupBox_ForTest()
		{
			return this.AlternateChartGroupBox;
		}

		void CurrencyTranslationTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			this.CurrencyTranslationsControl.SuspendLayout();
			// 
			// CurrencyTranslationsControl
			// 
			this.CurrencyTranslationsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyTranslationsControl, "AccAlternateChartCurrencyTranslations");
			this.CurrencyTranslationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrencyTranslationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CurrencyTranslationsControl.Name = "CurrencyTranslationsControl";
			this.CurrencyTranslationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 174, true);
			this.CurrencyTranslationsControl.TabIndex = 6;
			this.CurrencyTranslationsControl.ResumeLayout(true);
			this.CurrencyTranslationsControl.PerformLayout();
		}
	}
}
