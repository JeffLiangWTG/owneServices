using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class ReceptacleForm
	{
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.closeButton = new ZButton();
			this.oKButton = new ZButton();
			this.ReceptaclesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReceptaclesGrid)).BeginInit();
			this.ReceptaclesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 234, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader);
			// 
			// CloseButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("ReceptacleForm|D43E8B4D-0FDF-4DF3-9651-299B2D0440A9", "Cancel");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.closeButton.Name = "CloseButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.Click += new EventHandler(this.OnCloseButton_Click);
			// 
			// OKButton
			// 
			this.oKButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.oKButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("ReceptacleForm|7F070A4F-46DA-4FB2-BF94-EEACFCEBCA1C", "OK");
			this.oKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 211, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.oKButton.TabIndex = 2;
			this.oKButton.Click += new EventHandler(this.OnOKButton_Click);
			// 
			// ReceptaclesGrid
			// 
			this.ReceptaclesGrid.AllowNavigation = false;
			this.ReceptaclesGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			                                  | System.Windows.Forms.AnchorStyles.Left)
			                                 | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.ReceptaclesGrid, "Receptacles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).Receptacles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.Receptacle)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).Receptacles)).SyncRoot)).CY_Data)));
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ReceptaclesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReceptaclesGrid.CaptionVisible = false;
			this.ReceptaclesGrid.GridId = "C88EBD9D-4464-4786-B492-84B8C187E4A2";
			this.ReceptaclesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceptaclesGrid.LayoutKey = "ReceptaclesGrid";
			this.ReceptaclesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceptaclesGrid.Name = "ReceptaclesGrid";
			this.ReceptaclesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 206, true);
			this.ReceptaclesGrid.TabIndex = 1;
			this.ReceptaclesGrid.AllowSorting = false;
			// 
			// ReceptacleForm
			// 
			this.AcceptButton = this.oKButton;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 257, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 257, true);
			this.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("E5D5BED3-CE0C-4983-BD5D-8395003070ED", "Receptacle ID(s)");
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.ReceptaclesGrid);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader);
			this.Name = "ReceptacleForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.ReceptaclesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReceptaclesGrid)).EndInit();
			this.ReceptaclesGrid.ResumeLayout(false);
			this.ReceptaclesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZButton closeButton;
		ZButton oKButton;
		private ZArchitecture.ZGrid ReceptaclesGrid;
	}
}
