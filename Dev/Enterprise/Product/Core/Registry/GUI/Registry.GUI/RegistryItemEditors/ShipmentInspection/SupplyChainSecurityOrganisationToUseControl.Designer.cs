using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class SupplyChainSecurityOrganisationToUseControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.OrganisationsToUseGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationsToUseGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SupplyChainSecurityOrganisationToUseCollection);
			// 
			// OrganisationsToUseGrid
			// 
			this.OrganisationsToUseGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrganisationsToUseGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SupplyChainSecurityOrganisationToUse)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SupplyChainSecurityOrganisationToUse)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SupplyChainSecurityOrganisationToUse)(null)).ValidationCode)));
			this.OrganisationsToUseGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bb37fee9-351a-45c6-99c6-14848d091422", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.Width = 300;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f73f7082-7c64-4988-8ca7-667a95170d68", "Validation");
			zDropEditColumnStyleInfo1.ColumnName = "ValidationCode";
			this.OrganisationsToUseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationsToUseGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrganisationsToUseGrid.CopySelectedRowsAllowed = true;
			this.OrganisationsToUseGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationsToUseGrid.GridId = "6cb2d34f-9f8b-40b4-8b7b-ec1fc86b1246";
			this.OrganisationsToUseGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationsToUseGrid.LayoutKey = "OrganisationsToUseGrid";
			this.OrganisationsToUseGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationsToUseGrid.Name = "OrganisationsToUseGrid";
			this.OrganisationsToUseGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.OrganisationsToUseGrid.TabIndex = 0;
			// 
			// SupplyChainSecurityOrganisationToUseControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrganisationsToUseGrid);
			this.Name = "SupplyChainSecurityOrganisationToUseControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationsToUseGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid OrganisationsToUseGrid;

	}
}
