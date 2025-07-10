using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Environment
{
	public partial class UserResponseDialog
	{
		#region Windows Form Designer generated code

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		void InitializeComponent()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
		{
			this.UserResponseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserResponseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// Button1
			//
			this.Button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 64, true);
			//
			// Button2
			//
			this.Button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 64, true);
			//
			// Button3
			//
			this.Button3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 64, true);
			//
			// TextBox
			//
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			//
			// UserResponseTextBox
			//
			this.UserResponseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.UserResponseTextBox.CaptionResourceString = null;
			this.UserResponseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 37, true);
			this.UserResponseTextBox.Name = "UserResponseTextBox";
			this.UserResponseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.UserResponseTextBox.TabIndex = 6;
			this.UserResponseTextBox.Visible = false;
			this.UserResponseTextBox.TextChanged += new System.EventHandler(this.UserResponseTextBox_TextChanged);
			//
			// UserResponseDropEdit
			//
			this.UserResponseDropEdit.AllowDrop = true;
			this.UserResponseDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.UserResponseDropEdit.CaptionResourceString = null;
			this.UserResponseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 37, true);
			this.UserResponseDropEdit.Name = "UserResponseDropEdit";
			this.UserResponseDropEdit.PreBoundMaxLength = 3;
			this.UserResponseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.UserResponseDropEdit.TabIndex = 7;
			this.UserResponseDropEdit.Visible = false;
			this.UserResponseDropEdit.SelectedIndexChanged += new System.EventHandler(this.UserResponseDropEdit_SelectedIndexChanged);
			//
			// UserResponseDialog
			//

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 98, true);
			this.Controls.Add(this.UserResponseTextBox);
			this.Controls.Add(this.UserResponseDropEdit);
			this.Name = "UserResponseDialog";
			this.Controls.SetChildIndex(this.UserResponseDropEdit, 0);
			this.Controls.SetChildIndex(this.UserResponseTextBox, 0);
			this.Controls.SetChildIndex(this.PictureBox, 0);
			this.Controls.SetChildIndex(this.Button1, 0);
			this.Controls.SetChildIndex(this.Button2, 0);
			this.Controls.SetChildIndex(this.Button3, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Control ResponseControl;
		internal Enterprise.ZArchitecture.ZTextBox UserResponseTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit UserResponseDropEdit;

		#endregion
	}
}
