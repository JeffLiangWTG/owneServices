namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class G5V1TemporaryStoragePackedItemDetailsControl
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
			this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PresentationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MissingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UCRTextBox.SuspendLayout();
			this.PresentationDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// UCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRTextBox, "Bills.PackedItems.UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).UCR)));
			this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 142, true);
			this.UCRTextBox.Name = "UCRTextBox";
			this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.UCRTextBox.TabIndex = 2;
			// 
			// PresentationDateEdit
			// 
			this.PresentationDateEdit.AllowDrop = true;
			this.PresentationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationDateEdit, "Bills.PackedItems.PresentationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).PresentationDate)));
			this.PresentationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 168, true);
			this.PresentationDateEdit.Name = "PresentationDateEdit";
			this.PresentationDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PresentationDateEdit.TabIndex = 2;
			// 
			// MissingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.MissingCheckBox, "Bills.PackedItems.IsMissing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStoragePackedItem)(null)).IsMissing)));
			this.MissingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 194, true);
			this.MissingCheckBox.Name = "MissingCheckBox";
			this.MissingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.MissingCheckBox.TabIndex = 3;
			// 
			// G5V1TemporaryStoragePackedItemDetailsControl
			// 
			this.Controls.Add(this.UCRTextBox);
			this.Controls.Add(this.PresentationDateEdit);
			this.Controls.Add(this.MissingCheckBox);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "G5V1TemporaryStoragePackedItemDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 381, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UCRTextBox.ResumeLayout(true);
			this.UCRTextBox.PerformLayout();
			this.PresentationDateEdit.ResumeLayout(true);
			this.PresentationDateEdit.PerformLayout();
			this.MissingCheckBox.ResumeLayout(true);
			this.MissingCheckBox.PerformLayout();

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZTextBox UCRTextBox;
		internal ZArchitecture.GUI.ZDateEdit PresentationDateEdit;
		internal ZArchitecture.GUI.ZCheckBox MissingCheckBox;
	}
}
