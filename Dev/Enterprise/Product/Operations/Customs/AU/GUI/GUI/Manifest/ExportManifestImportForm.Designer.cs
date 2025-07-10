using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class ExportManifestImportForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ManifestsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancellButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder);
			// 
			// ManifestsGrid
			// 
			this.ManifestsGrid.AllowNavigation = false;
			this.ManifestsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ManifestsGrid, "Manifests");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).ShouldSave)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).CalcExportManifest.Departure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).CalcExportManifest.DischargeCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).CalcExportManifest.DepartureDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).CalcExportManifest.ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).CalcExportManifest.EmptyContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifest)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder)(null)).Manifests)).SyncRoot)).CalcExportManifest.PackageCount)));
			this.ManifestsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = "Save?";
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSave";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.Caption = "Departure Port";
			zTextBoxColumnStyleInfo1.ColumnName = "CalcExportManifest+Departure";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.Caption = "Destination Ctry/Rgn.";
			zTextBoxColumnStyleInfo2.ColumnName = "CalcExportManifest+DischargeCountry";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo1.Caption = "Departure Date";
			zDateEditColumnStyleInfo1.ColumnName = "CalcExportManifest+DepartureDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Containers";
			zCalcEditColumnStyleInfo1.ColumnName = "CalcExportManifest+ContainerCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Empty Containers";
			zCalcEditColumnStyleInfo2.ColumnName = "CalcExportManifest+EmptyContainerCount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Packs";
			zCalcEditColumnStyleInfo3.ColumnName = "CalcExportManifest+PackageCount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ManifestsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ManifestsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ManifestsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ManifestsGrid.GridId = "96f8e38d-36ae-462e-a687-c3afe235baa9";
			this.ManifestsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ManifestsGrid.LayoutKey = "ManifestsGrid";
			this.ManifestsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ManifestsGrid.Name = "ManifestsGrid";
			this.ManifestsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 289, true);
			this.ManifestsGrid.TabIndex = 1;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 307, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Text = "Export";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// CancellButton
			// 
			this.CancellButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancellButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(577, 307, true);
			this.CancellButton.Name = "CancellButton";
			this.CancellButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancellButton.TabIndex = 3;
			this.CancellButton.Text = "Cancel";
			this.CancellButton.UseVisualStyleBackColor = true;
			this.CancellButton.Click += new System.EventHandler(this.CancellButton_Click);
			// 
			// ExportManifestImportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 360, true);
			this.Controls.Add(this.CancellButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.ManifestsGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.TemporaryManifestHolder";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 300, true);
			this.Name = "ExportManifestImportForm";
			this.Text = "Create Export Manifests";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ManifestsGrid, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CancellButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ManifestsGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CancellButton;
	}
}
