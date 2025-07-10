using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePackedItemDetailsControl
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
            this.RegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ReleaseDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader);
            // 
            // RegistrationNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.RegistrationNoTextBox, "Bills.PackedItems.RegistrationNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).RegistrationNo)));
            this.RegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 302, true);
            this.RegistrationNoTextBox.Name = "RegistrationNoTextBox";
			this.RegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
            this.RegistrationNoTextBox.TabIndex = 1;
            // 
            // ReleaseDateEdit
            // 
            this.ReleaseDateEdit.AllowDrop = true;
            this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "Bills.PackedItems.ReleaseDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).ReleaseDate)));
            this.ReleaseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 332, true);
            this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
            this.ReleaseDateEdit.TabIndex = 2;
            // 
            // UCC6TemporaryStoragePackedItemDetailsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.RegistrationNoTextBox);
            this.Controls.Add(this.ReleaseDateEdit);
            this.Name = "UCC6TemporaryStoragePackedItemDetailsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 426, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ReleaseDateEdit.ResumeLayout(true);
            this.ReleaseDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox RegistrationNoTextBox;
		internal ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
	}
}
