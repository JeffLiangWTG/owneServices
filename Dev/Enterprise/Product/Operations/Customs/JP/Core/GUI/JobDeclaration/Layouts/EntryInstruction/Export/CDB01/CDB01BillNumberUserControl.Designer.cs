namespace Enterprise.Customs.JP.GUI
{
	partial class CDB01BillNumberUserControl
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
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
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
			this.CDB01BillNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillNumberTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CDB01BillNumberPanel.SuspendLayout();
			this.BillNumberTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// CDB01BillNumberPanel
			// 
			this.CDB01BillNumberPanel.Controls.Add(this.BillNumberTextBox);
			this.CDB01BillNumberPanel.Controls.Add(this.BillNumberTypeDropEdit);
			this.CDB01BillNumberPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CDB01BillNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CDB01BillNumberPanel.Name = "CDB01BillNumberPanel";
			this.CDB01BillNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 26, true);
			this.CDB01BillNumberPanel.TabIndex = 1;
			// 
			// BillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillNumberTextBox, "CEI_BillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_BillNumber)));
			this.BillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 3, true);
			this.BillNumberTextBox.Name = "BillNumberTextBox";
			this.BillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.BillNumberTextBox.TabIndex = 2;
			// 
			// BillNumberTypeDropEdit
			// 
			this.BillNumberTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillNumberTypeDropEdit, "CEI_BillNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_BillNumberType)));
			this.BillNumberTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillNumberTypeDropEdit.Name = "BillNumberTypeDropEdit";
			this.BillNumberTypeDropEdit.PreBoundMaxLength = 1;
			this.BillNumberTypeDropEdit.ShowDescriptionBox = false;
			this.BillNumberTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.BillNumberTypeDropEdit.TabIndex = 1;
			// 
			// CDB01BillNumberUserControl
			// 
			this.Controls.Add(this.CDB01BillNumberPanel);
			this.Name = "CDB01BillNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CDB01BillNumberPanel.ResumeLayout(false);
			this.CDB01BillNumberPanel.PerformLayout();
			this.BillNumberTypeDropEdit.ResumeLayout(true);
			this.BillNumberTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel CDB01BillNumberPanel;
		private ZArchitecture.GUI.ZDropEdit BillNumberTypeDropEdit;
		private ZArchitecture.ZTextBox BillNumberTextBox;
	}
}
