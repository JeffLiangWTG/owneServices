using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISEntityIdForm
	{
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.aQISEntityIdGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.aQISEntityIdGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			// 
			// OKButton
			// 
			this.oKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 248, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.TabIndex = 20;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AQISEntityIdGroupBox
			// 
			this.aQISEntityIdGroupBox.Controls.Add(this.zGrid1);
			this.aQISEntityIdGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.aQISEntityIdGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISEntityIdGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISEntityIdGroupBox.Name = "AQISEntityIdGroupBox";
			this.aQISEntityIdGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 244, true);
			this.aQISEntityIdGroupBox.TabIndex = 22;
			this.aQISEntityIdGroupBox.TabStop = false;
			this.aQISEntityIdGroupBox.Text = "Quarantine Entity Id";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "AQISEntityIds";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISEntityIds)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+AQISEntityIdList";
			zDropEditColumnStyleInfo1.Caption = "Code";
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 225, true);
			this.zGrid1.TabIndex = 0;
			this.FormToolTip.SetToolTip(this.zGrid1, @"The Quarantine Entity Identifier is a Quarantine field on an Import Declaration. It is optional and is an identifier of some business objects for significance within the Quarantine system. This field is currently used to identify overseas accredited fumigation parties, but may in future be used for any party registered by Quarantine. This field is optional and allows the Importer to quote additional information that may interest Quarantine.  Example: Overseas Treatment Provider Number. This element is not allowed when the Line Nature Type is N30. Up to 10 Entity Identifiers may be provided on each tariff line.");
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.AQISEntityId)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISEntityIds)))).CodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISEntityId)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISEntityIds)))).Code)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISEntityId)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).AQISEntityIds)))).Lookups.AQISEntityIdList)));
			// 
			// AQISEntityIdForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 300, true);
			this.Controls.Add(this.aQISEntityIdGroupBox);
			this.Controls.Add(this.oKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AQISEntityIdForm";
			this.Text = "Entity ID Form";
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.aQISEntityIdGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.aQISEntityIdGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		private ZButton oKButton;
		private ZGroupBox aQISEntityIdGroupBox;
		internal ZArchitecture.ZGrid zGrid1;
	}
}
