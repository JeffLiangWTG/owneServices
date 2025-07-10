namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class ContingencyDataEmailAddressesControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContingencyDataEmailAddressesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContingencyDataEmailAddressesGrid)).BeginInit();
			this.ContingencyDataEmailAddressesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ContingencyDataEmailAddressCollection);
			// 
			// ContingencyDataEmailAddressesGrid
			// 
			this.ContingencyDataEmailAddressesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContingencyDataEmailAddressesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ContingencyDataEmailAddress)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ContingencyDataEmailAddress)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ContingencyDataEmailAddress)(null)).EnglishDescription)));
			this.ContingencyDataEmailAddressesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ContingencyDataEmailAddressesControl|f0f72113-273b-4fac-905c-268277b18b01", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ContingencyDataEmailAddressesControl|99db2ece-0ee0-4ff4-af11-6fe064b15b4f", "Email Address");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ContingencyDataEmailAddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContingencyDataEmailAddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContingencyDataEmailAddressesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContingencyDataEmailAddressesGrid.GridId = "4c9a667f-2498-4ac5-a7fe-a71e430407b4";
			this.ContingencyDataEmailAddressesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContingencyDataEmailAddressesGrid.LayoutKey = "zGrid1";
			this.ContingencyDataEmailAddressesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContingencyDataEmailAddressesGrid.Name = "ContingencyDataEmailAddressesGrid";
			this.ContingencyDataEmailAddressesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 344, true);
			this.ContingencyDataEmailAddressesGrid.TabIndex = 0;
			// 
			// ContingencyDataEmailAddressesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContingencyDataEmailAddressesGrid);
			this.Name = "ContingencyDataEmailAddressesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContingencyDataEmailAddressesGrid)).EndInit();
			this.ContingencyDataEmailAddressesGrid.ResumeLayout(false);
			this.ContingencyDataEmailAddressesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ContingencyDataEmailAddressesGrid;
	}
}
