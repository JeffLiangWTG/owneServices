namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentAdditionalDocumentsDetailsUserControl
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
			this.SequenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.KindDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SequenceNumberTextBox.SuspendLayout();
			this.KindDropEdit.SuspendLayout();
			this.DocTypeCodeFindBox.SuspendLayout();
			this.ReferenceNumberTextBox.SuspendLayout();
			this.TextTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument);
			// 
			// SequenceNumberTextBox
			// 
			this.SequenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SequenceNumberTextBox, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_LineNo)));
			this.SequenceNumberTextBox.Name = "SequenceNumberTextBox";
			this.SequenceNumberTextBox.TabIndex = 0;
			this.SequenceNumberTextBox.ReadOnly = true;
			// 
			// StatusLabel
			// 
			this.StatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusLabel, false);
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 180, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
			this.StatusLabel.TabIndex = 11;
			// 
			// KindDropEdit
			// 
			this.KindDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.KindDropEdit, "CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_SubType)));
			this.KindDropEdit.Name = "KindDropEdit";
			this.KindDropEdit.TabIndex = 1;
			this.KindDropEdit.ReadOnly = true;
			// 
			// DocTypeCodeFindBox
			// 
			this.DocTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocTypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_Code)));
			this.DocTypeCodeFindBox.Name = "DocTypeCodeFindBox";
			this.DocTypeCodeFindBox.TabIndex = 2;
			// 
			// ReferenceNumberTextBox
			// 
			this.ReferenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.TabIndex = 3;
			// 
			// TextTextBox
			// 
			this.TextTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TextTextBox, "CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_Description)));
			this.TextTextBox.Name = "TextTextBox";
			this.TextTextBox.TabIndex = 4;
			this.TextTextBox.Multiline = true;
			this.TextTextBox.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// UnloadingDetailsUserControl
			//
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.SequenceNumberTextBox);
			this.Controls.Add(this.KindDropEdit);
			this.Controls.Add(this.DocTypeCodeFindBox);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.TextTextBox);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SequenceNumberTextBox);
			this.Controls.SetChildIndex(this.SequenceNumberTextBox, 0);
			this.Controls.Add(this.KindDropEdit);
			this.Controls.SetChildIndex(this.KindDropEdit, 0);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.SetChildIndex(this.ReferenceNumberTextBox, 0);
			this.Controls.Add(this.TextTextBox);
			this.Controls.SetChildIndex(this.TextTextBox, 0);
			this.Name = "HouseConsignmentAdditionalDocumentsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SequenceNumberTextBox.ResumeLayout(false);
			this.SequenceNumberTextBox.PerformLayout();
			this.KindDropEdit.ResumeLayout(false);
			this.KindDropEdit.PerformLayout();
			this.DocTypeCodeFindBox.ResumeLayout(false);
			this.DocTypeCodeFindBox.PerformLayout();
			this.ReferenceNumberTextBox.ResumeLayout(false);
			this.ReferenceNumberTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.ZTextBox SequenceNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit KindDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox DocTypeCodeFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.ZTextBox TextTextBox;
		internal ZArchitecture.ZLabel StatusLabel;

		#endregion
	}
}
