using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	partial class DeliveryModeRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DeliveryModeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryModeGrid)).BeginInit();
			this.DeliveryModeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DeliveryMode);
			// 
			// DeliveryModeGrid
			// 
			this.DeliveryModeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeliveryModeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DeliveryMode)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DeliveryMode)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DeliveryMode)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DeliveryMode)(null)).UserDefinedCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DeliveryMode)(null)).UserDefinedDescription)));
			this.DeliveryModeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DeliveryModeRegistryControl|4dfa5df4-cd0c-4ad3-a14a-ac140de5f2dc", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DeliveryModeRegistryControl|d9b8556a-c0ea-4b43-b100-35bac00a4690", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "UserDefinedCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DeliveryModeRegistryControl|5ea504b8-4232-4248-838f-cc0b9b5cff43", "User Defined Code");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "UserDefinedDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DeliveryModeRegistryControl|739e4b9c-6ff0-43bf-a9dc-9b5cc5c5fc12", "User Defined Description");
			this.DeliveryModeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeliveryModeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			if (FreightDataRegistry.Instance.ContainerDeliveryModeOverride.Value)
			{
				this.DeliveryModeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
				this.DeliveryModeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			}
			this.DeliveryModeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeliveryModeGrid.GridId = "1705533f-d811-4012-a72c-ff7d03a1b1a3";
			this.DeliveryModeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeliveryModeGrid.LayoutKey = "DeliveryModeGrid";
			this.DeliveryModeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeliveryModeGrid.Name = "DeliveryModeGrid";
			this.DeliveryModeGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DeliveryModeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.DeliveryModeGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.DeliveryModeGrid.TabIndex = 0;
			// 
			// DeliveryModeRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryModeGrid);
			this.Name = "DeliveryModeRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DeliveryModeGrid)).EndInit();
			this.DeliveryModeGrid.ResumeLayout(false);
			this.DeliveryModeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZGrid DeliveryModeGrid;
	}
}
