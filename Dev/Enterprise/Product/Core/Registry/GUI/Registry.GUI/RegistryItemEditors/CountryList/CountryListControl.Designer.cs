using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class CountryListControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CountryListGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryListGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CountryListCollection);
			// 
			// CountryListGrid
			// 
			this.CountryListGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryListGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CountryListElement)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.CountryListElement)(null)).CountryPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CountryListElement)(null)).CountryCollection)));
			this.CountryListGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "CountryCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CountryListControl|e5691c11-697a-425e-b1b6-57221f67ca51", "Country/Region");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CountryPK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			//
			//zTextBoxColumnStyleInfo1
			//
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CountryListControl|ba346110-8cd9-4aa9-bb21-5896a235d656", "Country/Region Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CountryName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			this.CountryListGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CountryListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CountryListGrid.GridId = "651f176e-4d0b-4143-894f-84b1165b8843";
			this.CountryListGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryListGrid.LayoutKey = "CountryListGrid";
			this.CountryListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CountryListGrid.Name = "CountryListGrid";
			this.CountryListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 152, true);
			this.CountryListGrid.TabIndex = 0;
			// 
			// CountryListControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CountryListGrid);
			this.Name = "CountryListControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryListGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid CountryListGrid;
	}
}
