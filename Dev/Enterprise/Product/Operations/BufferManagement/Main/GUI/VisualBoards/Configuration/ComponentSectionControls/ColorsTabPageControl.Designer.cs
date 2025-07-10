namespace Enterprise.BufferManagement.GUI
{
	partial class ColorsTabPageControl
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
			this.StartableTaskStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StartableTaskColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TargetTaskStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TargetTaskColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverdueForeColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverdueBackColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Zone0ColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Zone1ColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Zone2ColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Zone3ColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ColorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ColorsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// StartableTaskStyleDropEdit
			// 
			this.StartableTaskStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StartableTaskStyleDropEdit, "CountdownStartableBorderStyle");
			this.StartableTaskStyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StartableTaskStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 219, true);
			this.StartableTaskStyleDropEdit.Name = "StartableTaskStyleDropEdit";
			this.StartableTaskStyleDropEdit.ShowDescriptionBox = false;
			this.StartableTaskStyleDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.StartableTaskStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.StartableTaskStyleDropEdit.TabIndex = 34;
			// 
			// StartableTaskColorDropEdit
			// 
			this.StartableTaskColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StartableTaskColorDropEdit, "CountdownStartableBorderColor");
			this.StartableTaskColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StartableTaskColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 193, true);
			this.StartableTaskColorDropEdit.Name = "StartableTaskColorDropEdit";
			this.StartableTaskColorDropEdit.ShowDescriptionBox = false;
			this.StartableTaskColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.StartableTaskColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.StartableTaskColorDropEdit.TabIndex = 33;
			// 
			// TargetTaskStyleDropEdit
			// 
			this.TargetTaskStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TargetTaskStyleDropEdit, "CountdownTargetBorderStyle");
			this.TargetTaskStyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TargetTaskStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 159, true);
			this.TargetTaskStyleDropEdit.Name = "TargetTaskStyleDropEdit";
			this.TargetTaskStyleDropEdit.ShowDescriptionBox = false;
			this.TargetTaskStyleDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.TargetTaskStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TargetTaskStyleDropEdit.TabIndex = 32;
			// 
			// TargetTaskColorDropEdit
			// 
			this.TargetTaskColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TargetTaskColorDropEdit, "CountdownTargetBorderColor");
			this.TargetTaskColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TargetTaskColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 133, true);
			this.TargetTaskColorDropEdit.Name = "TargetTaskColorDropEdit";
			this.TargetTaskColorDropEdit.ShowDescriptionBox = false;
			this.TargetTaskColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.TargetTaskColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TargetTaskColorDropEdit.TabIndex = 31;
			// 
			// OverdueForeColorDropEdit
			// 
			this.OverdueForeColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverdueForeColorDropEdit, "OverdueForegroundColor");
			this.OverdueForeColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverdueForeColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 100, true);
			this.OverdueForeColorDropEdit.Name = "OverdueForeColorDropEdit";
			this.OverdueForeColorDropEdit.ShowDescriptionBox = false;
			this.OverdueForeColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OverdueForeColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OverdueForeColorDropEdit.TabIndex = 30;
			// 
			// OverdueBackColorDropEdit
			// 
			this.OverdueBackColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverdueBackColorDropEdit, "OverdueBackgroundColor");
			this.OverdueBackColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverdueBackColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 74, true);
			this.OverdueBackColorDropEdit.Name = "OverdueBackColorDropEdit";
			this.OverdueBackColorDropEdit.ShowDescriptionBox = false;
			this.OverdueBackColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OverdueBackColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.OverdueBackColorDropEdit.TabIndex = 29;
			// 
			// Zone0ColorDropEdit
			// 
			this.Zone0ColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Zone0ColorDropEdit, "BufferZone0Color");
			this.Zone0ColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Zone0ColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 45, true);
			this.Zone0ColorDropEdit.Name = "Zone0ColorDropEdit";
			this.Zone0ColorDropEdit.ShowDescriptionBox = false;
			this.Zone0ColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.Zone0ColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.Zone0ColorDropEdit.TabIndex = 28;
			// 
			// Zone1ColorDropEdit
			// 
			this.Zone1ColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Zone1ColorDropEdit, "BufferZone1Color");
			this.Zone1ColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Zone1ColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 45, true);
			this.Zone1ColorDropEdit.Name = "Zone1ColorDropEdit";
			this.Zone1ColorDropEdit.ShowDescriptionBox = false;
			this.Zone1ColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.Zone1ColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.Zone1ColorDropEdit.TabIndex = 27;
			// 
			// Zone2ColorDropEdit
			// 
			this.Zone2ColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Zone2ColorDropEdit, "BufferZone2Color");
			this.Zone2ColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Zone2ColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 19, true);
			this.Zone2ColorDropEdit.Name = "Zone2ColorDropEdit";
			this.Zone2ColorDropEdit.ShowDescriptionBox = false;
			this.Zone2ColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.Zone2ColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.Zone2ColorDropEdit.TabIndex = 26;
			// 
			// Zone3ColorDropEdit
			// 
			this.Zone3ColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Zone3ColorDropEdit, "BufferZone3Color");
			this.Zone3ColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Zone3ColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 19, true);
			this.Zone3ColorDropEdit.Name = "Zone3ColorDropEdit";
			this.Zone3ColorDropEdit.ShowDescriptionBox = false;
			this.Zone3ColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.Zone3ColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.Zone3ColorDropEdit.TabIndex = 25;
			// 
			// ColorsGroupBox
			// 
			this.ColorsGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.ColorsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("9b1c8082-2951-4749-b37b-2582806ac511", "Colors");
			this.ColorsGroupBox.Controls.Add(this.StartableTaskStyleDropEdit);
			this.ColorsGroupBox.Controls.Add(this.Zone3ColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.StartableTaskColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.Zone2ColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.TargetTaskStyleDropEdit);
			this.ColorsGroupBox.Controls.Add(this.Zone1ColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.TargetTaskColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.Zone0ColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.OverdueForeColorDropEdit);
			this.ColorsGroupBox.Controls.Add(this.OverdueBackColorDropEdit);
			this.ColorsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ColorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ColorsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 104, true);
			this.ColorsGroupBox.Name = "ColorsGroupBox";
			this.ColorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 408, true);
			this.ColorsGroupBox.TabIndex = 35;
			this.ColorsGroupBox.TabStop = false;
			// 
			// ColorsTabPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ColorsGroupBox);
			this.Name = "ColorsTabPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ColorsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit StartableTaskStyleDropEdit;
		private ZArchitecture.GUI.ZDropEdit StartableTaskColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit TargetTaskStyleDropEdit;
		private ZArchitecture.GUI.ZDropEdit TargetTaskColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit OverdueForeColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit OverdueBackColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit Zone0ColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit Zone1ColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit Zone2ColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit Zone3ColorDropEdit;
		private ZArchitecture.GUI.ZGroupBox ColorsGroupBox;
	}
}
