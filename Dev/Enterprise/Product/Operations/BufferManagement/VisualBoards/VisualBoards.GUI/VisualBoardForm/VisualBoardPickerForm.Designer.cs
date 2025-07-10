using System.Drawing;

namespace Enterprise.VisualBoards.GUI
{
	partial class VisualBoardPickerForm
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
		protected new void InitializeComponent()
		{
			this.VisualBoardDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.OpenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VisualBoardDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.VisualBoards.Business.BoardPickerViewModel);
			// 
			// VisualBoardDropEdit
			// 
			this.VisualBoardDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VisualBoardDropEdit, "BoardPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.VisualBoards.Business.BoardPickerViewModel)(null)).BoardPK)));
			this.VisualBoardDropEdit.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("880b49da-e322-4d18-a922-ac0a5752f8ce", "Find board by name:");
			this.VisualBoardDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.VisualBoardDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 10, true);
			this.VisualBoardDropEdit.Name = "VisualBoardDropEdit";
			this.VisualBoardDropEdit.PreBoundMaxLength = 20;
			this.VisualBoardDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 19, true);
			this.VisualBoardDropEdit.TabIndex = 0;
			// 
			// OpenButton
			// 
			this.OpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.OpenButton.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("36293871-cb90-47a0-9cf6-f80fb67ce061", "Open", "Open Selected Board");
			this.OpenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 34, true);
			this.OpenButton.Name = "OpenButton";
			this.OpenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 26, true);
			this.OpenButton.TabIndex = 4;
			this.OpenButton.UseVisualStyleBackColor = false;
			this.OpenButton.Click += new System.EventHandler(this.ShowBoardButton_Click);
			// 
			// VisualBoardPickerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 94, true);
			this.Controls.Add(this.VisualBoardDropEdit);
			this.Controls.Add(this.OpenButton);
			this.DataSourceType = typeof(Enterprise.VisualBoards.Business.BoardPickerViewModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 200, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 120, true);
			this.Name = "VisualBoardPickerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OpenButton, 0);
			this.Controls.SetChildIndex(this.VisualBoardDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VisualBoardDropEdit.ResumeLayout(true);
			this.VisualBoardDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGuidDropEdit VisualBoardDropEdit;
		internal ZArchitecture.GUI.ZButton OpenButton;
	}
}
