using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.BankStatement
{
	partial class StatementFileFormatForm
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
		new void InitializeComponent()
		{
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StatementFileFormatDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 94, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 24, true);
			this.MainStatusBar.TabIndex = 4;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.AccStatement.BankStatementFormat);
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementFileFormatForm|ca490eb2-c3f5-4768-b6aa-34a3577d2539", "Select file format to import");
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 13, true);
			this.MessageLabel.TabIndex = 0;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementFileFormatForm|d1e689bc-2d49-43c6-96b0-7b212ee992cf", "&OK");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 89, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 2;
			this.OkButton.UseVisualStyleBackColor = true;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementFileFormatForm|5b428c3c-ebdc-47c0-a45a-4ddb7467d25a", "&Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 89, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// StatementFileFormatDropEdit
			// 
			this.StatementFileFormatDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatementFileFormatDropEdit, "StatementFileFormatName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Base.AccStatement.BankStatementFormat)(null)).StatementFileFormatName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.AccStatement.BankStatementFormat)(null)).StatementFileFormats_List)));
			this.StatementFileFormatDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatementFileFormatDropEdit, false);
			this.StatementFileFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 35, true);
			this.StatementFileFormatDropEdit.Name = "StatementFileFormatDropEdit";
			this.StatementFileFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 20, true);
			this.StatementFileFormatDropEdit.TabIndex = 1;
			// 
			// StatementFileFormatForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 118, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementFileFormatForm|241a6267-9d16-4096-b91b-23958569fd92", "Statement File Format");
			this.Controls.Add(this.StatementFileFormatDropEdit);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.MessageLabel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.AccStatement.BankStatementFormat);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.AccStatement.BankStatementFormat";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "StatementFileFormatForm";
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StatementFileFormatForm_FormClosing);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.StatementFileFormatDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		private Enterprise.ZArchitecture.GUI.ZButton OkButton;
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatementFileFormatDropEdit;
	}
}
