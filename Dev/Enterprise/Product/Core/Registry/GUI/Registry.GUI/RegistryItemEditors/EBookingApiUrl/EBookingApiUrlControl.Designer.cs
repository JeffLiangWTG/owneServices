namespace Enterprise.Registry.GUI
{
	partial class EBookingApiUrlControl
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
			this.PresetEBookingApiUrlDropEdit = new ZArchitecture.GUI.ZDropEdit(); //SuppressCodeSmell Reason = Descriptions are URLs, not translatable
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PresetEBookingApiUrlDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EBookingApiUrls);
			// 
			// PresetEBookingApiUrlDropEdit
			// 
			this.PresetEBookingApiUrlDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresetEBookingApiUrlDropEdit, "SelectedEBookingApiUrlCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.EBookingApiUrls)(null)).SelectedEBookingApiUrlCode)));
			this.PresetEBookingApiUrlDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("46ecd422-b52b-46fe-a298-f113f63afce5", "Default");
			this.PresetEBookingApiUrlDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PresetEBookingApiUrlDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 3, true);
			this.PresetEBookingApiUrlDropEdit.Name = "PresetEBookingApiUrlDropEdit";
			this.PresetEBookingApiUrlDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 20, true);
			this.PresetEBookingApiUrlDropEdit.TabIndex = 0;
			// 
			// EBookingApiUrlControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PresetEBookingApiUrlDropEdit);
			this.Name = "EBookingApiUrlControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PresetEBookingApiUrlDropEdit.ResumeLayout(true);
			this.PresetEBookingApiUrlDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZDropEdit PresetEBookingApiUrlDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PresetEBookingApiUrlDropEditInternal => PresetEBookingApiUrlDropEdit;

		#endregion
	}
}
