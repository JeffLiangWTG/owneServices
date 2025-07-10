using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	partial class MapTreeRightPanel
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
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			selectedMemberInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			selectedMemberInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			topLevelDataSourceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			topLevelDataSourceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			selectedMemberInfoGroupBox.SuspendLayout();
			topLevelDataSourceGroupBox.SuspendLayout();

			this.Controls.Add(selectedMemberInfoGroupBox);
			this.Controls.Add(topLevelDataSourceGroupBox);
			this.Dock = System.Windows.Forms.DockStyle.Right;
			this.CaptionRenderingEnabled = true;
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 3, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 200, true);
			this.Name = "rightPanel";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 328, true);
			this.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper);
			// 
			// selectedMemberInfoGroupBox
			// 
			selectedMemberInfoGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|3943d435-278f-4b2d-8b5e-b026b4ae7bbe", "Selected Property Information", "Shows additional information about the Property you have selected in the Tree View.");
			selectedMemberInfoGroupBox.Controls.Add(selectedMemberInfoTextBox);
			selectedMemberInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			selectedMemberInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			selectedMemberInfoGroupBox.Name = "selectedMemberInfoGroupBox";
			selectedMemberInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 222, true);
			selectedMemberInfoGroupBox.TabIndex = 1;
			selectedMemberInfoGroupBox.TabStop = false;
			// 
			// selectedMemberInfoTextBox
			// 
			this.BindingSource.SetBindingMember(selectedMemberInfoTextBox, "SelectedMemberInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper)(null)).SelectedMemberInformation)));
			selectedMemberInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			selectedMemberInfoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(selectedMemberInfoTextBox, false);
			selectedMemberInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 8, true);
			selectedMemberInfoTextBox.Multiline = true;
			selectedMemberInfoTextBox.Name = "selectedMemberInfoTextBox";
			selectedMemberInfoTextBox.ReadOnly = true;
			selectedMemberInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 213, true);
			selectedMemberInfoTextBox.TabIndex = 0;
			// 
			// topLevelDataSourceGroupBox
			// 
			topLevelDataSourceGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|1285cdbe-52c0-40e0-b84b-cfb33744bb2d", "Top Level Data Source Information", "Contains all information about the Data Source available.");
			topLevelDataSourceGroupBox.Controls.Add(topLevelDataSourceTextBox);
			topLevelDataSourceGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			topLevelDataSourceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			topLevelDataSourceGroupBox.Name = "topLevelDataSourceGroupBox";
			topLevelDataSourceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 100, true);
			topLevelDataSourceGroupBox.TabIndex = 0;
			topLevelDataSourceGroupBox.TabStop = false;
			// 
			// topLevelDataSourceTextBox
			// 
			this.BindingSource.SetBindingMember(topLevelDataSourceTextBox, "TopLevelDataSourceInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper)(null)).TopLevelDataSourceInformation)));
			topLevelDataSourceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			topLevelDataSourceTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(topLevelDataSourceTextBox, false);
			topLevelDataSourceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 8, true);
			topLevelDataSourceTextBox.Multiline = true;
			topLevelDataSourceTextBox.Name = "topLevelDataSourceTextBox";
			topLevelDataSourceTextBox.ReadOnly = true;
			topLevelDataSourceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 90, true);
			topLevelDataSourceTextBox.TabIndex = 0;

			selectedMemberInfoGroupBox.ResumeLayout(false);
			selectedMemberInfoGroupBox.PerformLayout();
			topLevelDataSourceGroupBox.ResumeLayout(false);
			topLevelDataSourceGroupBox.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox selectedMemberInfoGroupBox;
		internal Enterprise.ZArchitecture.ZTextBox selectedMemberInfoTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox topLevelDataSourceGroupBox;
		internal Enterprise.ZArchitecture.ZTextBox topLevelDataSourceTextBox;
	}
}
