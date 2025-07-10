using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class ExportInvoiceLineTemplate
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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.TariffFindBox.SuspendLayout();
			this.SuspendLayout();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			this.ResumeLayout(false);
			this.PerformLayout();
			// 
			// TariffFindBox
			//
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(null)).JI_FormattedTariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.PreBoundMaxLength = 30;
			this.TariffFindBox.ShouldResize = true;
			this.TariffFindBox.ShowDescriptionBox = true;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 15, true);
			this.TariffFindBox.TabIndex = 3;
			// 
			// ExportInvoiceLineTemplate
			// 
			this.Controls.Add(this.TariffFindBox);
			this.Name = "ExportInvoiceLineTemplate";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public Enterprise.Customs.Universal.GUI.TariffFindBox TariffFindBox;
	}
}
