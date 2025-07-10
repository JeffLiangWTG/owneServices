namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSLateLodgementReasonUserControl
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
			this.ReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// ReasonDropEdit
			// 
			this.ReasonDropEdit.AllowDrop = true;
			this.ReasonDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReasonDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReasonDropEdit.Name = "ReasonDropEdit";
			this.ReasonDropEdit.PreBoundMaxLength = 47;
			this.ReasonDropEdit.ShouldResizeByMaxLength = false;
			this.ReasonDropEdit.ShowDescriptionBox = false;
			this.ReasonDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ReasonDropEdit.TabIndex = 0;
			// 
			// AUCOLSLateLodgementReasonUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Controls.Add(this.ReasonDropEdit);
			this.Name = "AUCOLSLateLodgementReasonUserControl";
			this.Controls.SetChildIndex(this.ReasonDropEdit, 0);
			this.Controls.SetChildIndex(this.MoreButton, 0);
			this.Controls.SetChildIndex(this.LongTextTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReasonDropEdit.ResumeLayout(true);
			this.ReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit ReasonDropEdit;
	}
}
