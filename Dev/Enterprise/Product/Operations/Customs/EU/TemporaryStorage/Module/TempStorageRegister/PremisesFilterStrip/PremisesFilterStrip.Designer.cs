using System.Windows.Forms;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	partial class PremisesFilterStrip
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
			this.PremisesTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PremisesCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PremisesCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PremisesLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PremisesTypeDropEdit.SuspendLayout();
			this.PremisesCodeDropEdit.SuspendLayout();
			this.PremisesCodeTextBox.SuspendLayout();
			this.PremisesLocationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PremisesModuleFilter);
			// 
			// PremisesTypeDropEdit
			// 
			this.PremisesTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PremisesTypeDropEdit, "PremisesType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PremisesModuleFilter)(null)).PremisesType)));
			this.PremisesTypeDropEdit.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.Module.Res.GetData("E83E329E-89F9-4704-9E0A-C736CAD04AD7", "Premises Type");
			this.PremisesTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 1, true);
			this.PremisesTypeDropEdit.Name = "PremisesTypeDropEdit";
			this.PremisesTypeDropEdit.PreBoundMaxLength = 2;
			this.PremisesTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PremisesTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PremisesTypeDropEdit.TabIndex = 1;
			// 
			// PremisesCodeDropEdit
			// 
			this.PremisesCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PremisesCodeDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PremisesModuleFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PremisesModuleFilter)(null)).ComparisonOperator_List)));
			this.PremisesCodeDropEdit.BindToList = "ComparisonOperator_List";
			this.PremisesCodeDropEdit.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.Module.Res.GetData("FE42CB62-1F6F-45F1-BE70-996315C1B470", "Premises Code");
			this.PremisesCodeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.PremisesCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 23, true);
			this.PremisesCodeDropEdit.Name = "PremisesCodeDropEdit";
			this.PremisesCodeDropEdit.ShouldResizeByMaxLength = true;
			this.PremisesCodeDropEdit.ShowDescriptionBox = false;
			this.PremisesCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PremisesCodeDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			this.PremisesCodeDropEdit.TabIndex = 2;
			// 
			// PremisesCodeTextBox
			// 
			this.PremisesCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PremisesCodeTextBox, "PremisesCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PremisesModuleFilter)(null)).PremisesCode)));
			this.PremisesCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.Module.Res.GetData("54018EEB-46D1-494B-AD75-DF3ADD5919CC", "Premises Code");
			this.PremisesCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 23, true);
			this.PremisesCodeTextBox.Name = "PremisesCodeTextBox";
			this.PremisesCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PremisesCodeTextBox.TabIndex = 3;
			// 
			// PremisesLocationCodeFindBox
			// 
			this.PremisesLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PremisesLocationCodeFindBox, "PremisesLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PremisesModuleFilter)(null)).PremisesLocation)));
			this.PremisesLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.Module.Res.GetData("719A21D9-B088-4B18-8C1C-026976320340", "Premises Location");
			this.PremisesLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 45, true);
			this.PremisesLocationCodeFindBox.Name = "PremisesLocationCodeFindBox";
			this.PremisesLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.PremisesLocationCodeFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.PremisesLocationCodeFindBox.TabIndex = 4;
			// 
			// PremisesFilterStrip
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PremisesTypeDropEdit);
			this.Controls.Add(this.PremisesCodeDropEdit);
			this.Controls.Add(this.PremisesCodeTextBox);
			this.Controls.Add(this.PremisesLocationCodeFindBox);
			this.Name = "PremisesFilterStrip";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 66, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PremisesTypeDropEdit.ResumeLayout(true);
			this.PremisesTypeDropEdit.PerformLayout();
			this.PremisesCodeDropEdit.ResumeLayout(true);
			this.PremisesCodeDropEdit.PerformLayout();
			this.PremisesCodeTextBox.ResumeLayout(true);
			this.PremisesCodeTextBox.PerformLayout();
			this.PremisesLocationCodeFindBox.ResumeLayout(true);
			this.PremisesLocationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit PremisesTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PremisesCodeDropEdit;
		private ZArchitecture.ZTextBox PremisesCodeTextBox;
		private ZArchitecture.GUI.ZCodeFindBox PremisesLocationCodeFindBox;
	}
}
