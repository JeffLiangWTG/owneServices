namespace Enterprise.Customs.DE.ExitControl.GUI
{
	partial class ConsignmentsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ConsignmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConsignmentsGrid)).BeginInit();
			this.ConsignmentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentCollection<Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment>);
			// 
			// ConsignmentsGrid
			// 
			this.ConsignmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsignmentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)).CXC_MovementReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)).CXC_LocalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)).CXC_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)).CXC_UniqueConsignmentReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.CusExitConsignment)(null)).CXC_ReferenceNumber)));
			this.ConsignmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CXC_MovementReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CXC_LocalReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CXC_Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(440);
			zTextBoxColumnStyleInfo4.ColumnName = "CXC_UniqueConsignmentReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo5.ColumnName = "CXC_ReferenceNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.ConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConsignmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ConsignmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentsGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.ConsignmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsignmentsGrid.LayoutKey = "ConsignmentsGrid";
			this.ConsignmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentsGrid.Name = "ConsignmentsGrid";
			this.ConsignmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1295, 150, true);
			this.ConsignmentsGrid.TabIndex = 0;
			// 
			// ConsignmentsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsignmentsGrid);
			this.Name = "ConsignmentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1295, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ConsignmentsGrid)).EndInit();
			this.ConsignmentsGrid.ResumeLayout(false);
			this.ConsignmentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ConsignmentsGrid;
	}
}

