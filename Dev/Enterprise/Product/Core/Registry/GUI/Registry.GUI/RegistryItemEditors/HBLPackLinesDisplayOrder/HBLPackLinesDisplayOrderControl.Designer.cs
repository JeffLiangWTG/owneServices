namespace Enterprise.Registry.GUI
{
	partial class HBLPackLinesDisplayOrderControl
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
            this.PresetPackLinesOrderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PresetPackLinesOrderDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.HBLPackLinesDisplayOrderDropEditBussinessObject);
            // 
            // PresetPackLinesOrderDropEdit
            // 
            this.PresetPackLinesOrderDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PresetPackLinesOrderDropEdit, "Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.GUI.HBLPackLinesDisplayOrderDropEditBussinessObject)(null)).Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.HBLPackLinesDisplayOrderDropEditBussinessObject)(null)).List)));
            this.PresetPackLinesOrderDropEdit.BindToList = "List";
            this.PresetPackLinesOrderDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8040ceb7-b2b9-4c19-92c1-d7430261bd3b", "Order Pack Lines by");
            this.PresetPackLinesOrderDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PresetPackLinesOrderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 16, true);
            this.PresetPackLinesOrderDropEdit.Name = "PresetPackLinesOrderDropEdit";
            this.PresetPackLinesOrderDropEdit.PreBoundMaxLength = 20;
            this.PresetPackLinesOrderDropEdit.ShowDescriptionBox = false;
            this.PresetPackLinesOrderDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
            this.PresetPackLinesOrderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 18, true);
            this.PresetPackLinesOrderDropEdit.TabIndex = 1;
            // 
            // HBLPackLinesDisplayOrderControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PresetPackLinesOrderDropEdit);
            this.Name = "HBLPackLinesDisplayOrderControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 68, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PresetPackLinesOrderDropEdit.ResumeLayout(true);
            this.PresetPackLinesOrderDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit PresetPackLinesOrderDropEdit;
	}
}
