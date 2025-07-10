using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class AdditionalTariffsUserControl
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
			Enterprise.Customs.BR.GUI.ExTariffColumnStyleInfo exTariffColumnStyleInfo1 = new Enterprise.Customs.BR.GUI.ExTariffColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalTariffsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalTariffsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalTariffsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTariffsGrid)).BeginInit();
			this.AdditionalTariffsGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.AdditionalTariffCollection);
			//
			// AdditionalTariffsGroupBox
			//
			this.AdditionalTariffsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("BB4817ED-A646-41BA-A7FF-2D0839FD361C", "Additional Tariffs");
			this.AdditionalTariffsGroupBox.Controls.Add(this.AdditionalTariffsGrid);
			this.AdditionalTariffsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalTariffsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalTariffsGroupBox.Name = "AdditionalTariffsGroupBox";
			this.AdditionalTariffsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 133, true);
			this.AdditionalTariffsGroupBox.TabIndex = 0;
			this.AdditionalTariffsGroupBox.TabStop = false;
			//
			// AdditionalTariffsGrid
			//
			this.AdditionalTariffsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalTariffsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).ExNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).TariffType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).LegalActSubjectDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).LegalActType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).LegalActIssuingBody)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).LegalActNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalTariff)(null)).LegalActYear)));
			this.AdditionalTariffsGrid.CaptionVisible = false;
			exTariffColumnStyleInfo1.ColumnName = "ExNumber";
			exTariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			exTariffColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			exTariffColumnStyleInfo1.TariffCodeProperty = "TariffCode";
			exTariffColumnStyleInfo1.TariffTypeProperty = "TariffType";
			exTariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "TariffType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.ColumnName = "LegalActSubjectDescription";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo3.ColumnName = "LegalActType";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo4.ColumnName = "LegalActIssuingBody";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "LegalActNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "LegalActYear";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalTariffsGrid.ColumnStyles.Add(exTariffColumnStyleInfo1);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalTariffsGrid.DisableImportDataMenuItem = true;
			this.AdditionalTariffsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalTariffsGrid.GridId = "29B90BAF-CC89-4707-9D3C-354802B5975A";
			this.AdditionalTariffsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalTariffsGrid.LayoutKey = null;
			this.AdditionalTariffsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalTariffsGrid.Name = "AdditionalTariffsGrid";
			this.AdditionalTariffsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 114, true);
			this.AdditionalTariffsGrid.TabIndex = 1;
			//
			// AdditionalTariffsUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalTariffsGroupBox);
			this.Name = "AdditionalTariffsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 133, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalTariffsGroupBox.ResumeLayout(false);
			this.AdditionalTariffsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTariffsGrid)).EndInit();
			this.AdditionalTariffsGrid.ResumeLayout(false);
			this.AdditionalTariffsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox AdditionalTariffsGroupBox;
		internal ZArchitecture.ZGrid AdditionalTariffsGrid;
	}
}
