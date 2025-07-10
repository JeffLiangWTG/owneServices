using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Tools
{
	partial class CommandsInfoListGrid
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
			Enterprise.ZArchitecture.ZSqlProfilerTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZSqlProfilerTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZSqlProfilerDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZSqlProfilerDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZSqlProfilerCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZSqlProfilerCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZSqlProfilerTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZSqlProfilerTextBoxColumnStyleInfo();
			this.sp = new CargoWise.Windows.UI.KSplitContainer();
			this.sp1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zToolStip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.tsButtonStart = new System.Windows.Forms.ToolStripButton(); // SuppressCodeSmell Reason = Developer Only Tool
			this.tsButtonPause = new System.Windows.Forms.ToolStripButton(); // SuppressCodeSmell Reason = Developer Only Tool
			this.tsButtonStop = new System.Windows.Forms.ToolStripButton(); // SuppressCodeSmell Reason = Developer Only Tool
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator(); // SuppressCodeSmell Reason = Developer Only Tool
			this.tsButtonClear = new System.Windows.Forms.ToolStripButton(); // SuppressCodeSmell Reason = Developer Only Tool
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.txtCommandText = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sp)).BeginInit();
			this.sp.Panel1.SuspendLayout();
			this.sp.Panel2.SuspendLayout();
			this.sp.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sp1)).BeginInit();
			this.sp1.Panel1.SuspendLayout();
			this.sp1.Panel2.SuspendLayout();
			this.sp1.SuspendLayout();
			this.zToolStip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Tools.SqlCommandsManager);
			// 
			// sp
			// 
			this.sp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sp.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sp.Name = "sp";
			this.sp.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// sp.Panel1
			// 
			this.sp.Panel1.Controls.Add(this.sp1);
			// 
			// sp.Panel2
			// 
			this.sp.Panel2.Controls.Add(this.txtCommandText);
			this.sp.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 521, true);
			this.sp.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(244);
			this.sp.TabIndex = 0;
			// 
			// sp1
			// 
			this.sp1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sp1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.sp1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sp1.Name = "sp1";
			this.sp1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// sp1.Panel1
			// 
			this.sp1.Panel1.Controls.Add(this.zToolStip);
			// 
			// sp1.Panel2
			// 
			this.sp1.Panel2.Controls.Add(this.zGrid1);
			this.sp1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 244, true);
			this.sp1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			this.sp1.TabIndex = 0;
			// 
			// zToolStip
			// 
			this.zToolStip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zToolStip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsButtonStart,
            this.tsButtonPause,
            this.tsButtonStop,
            this.toolStripSeparator1,
            this.tsButtonClear});
			this.zToolStip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zToolStip.Name = "zToolStip";
			this.zToolStip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 25, true);
			this.zToolStip.TabIndex = 0;
			this.zToolStip.Text = "zToolStrip1";
			// 
			// tsButtonStart
			// 
			this.tsButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsButtonStart.Image = global::Enterprise.ZArchitecture.GUI.Properties.Resources.Start;
			this.tsButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsButtonStart.Name = "tsButtonStart";
			this.tsButtonStart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 22, true);
			this.tsButtonStart.Text = "Start";
			this.tsButtonStart.Click += new System.EventHandler(this.tsButtonStart_Click);
			// 
			// tsButtonPause
			// 
			this.tsButtonPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsButtonPause.Image = global::Enterprise.ZArchitecture.GUI.Properties.Resources.Pause;
			this.tsButtonPause.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsButtonPause.Name = "tsButtonPause";
			this.tsButtonPause.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 22, true);
			this.tsButtonPause.Text = "Pause";
			this.tsButtonPause.Click += new System.EventHandler(this.tsButtonPause_Click);
			// 
			// tsButtonStop
			// 
			this.tsButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsButtonStop.Image = global::Enterprise.ZArchitecture.GUI.Properties.Resources.Stop;
			this.tsButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsButtonStop.Name = "tsButtonStop";
			this.tsButtonStop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 22, true);
			this.tsButtonStop.Text = "Stop";
			this.tsButtonStop.Click += new System.EventHandler(this.tsButtonStop_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 25, true);
			// 
			// tsButtonClear
			// 
			this.tsButtonClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsButtonClear.Image = global::Enterprise.ZArchitecture.GUI.Properties.Resources.Clear;
			this.tsButtonClear.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsButtonClear.Name = "tsButtonClear";
			this.tsButtonClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 22, true);
			this.tsButtonClear.Text = "Clear";
			this.tsButtonClear.Click += new System.EventHandler(this.tsButtonClear_Click);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.AlternatingBackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zGrid1, "CommandList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Tools.SqlCommandsManager)(null)).CommandList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Tools.DbCommandWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.Tools.SqlCommandsManager)(null)).CommandList)).SyncRoot)).SqlText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ZArchitecture.Tools.DbCommandWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.Tools.SqlCommandsManager)(null)).CommandList)).SyncRoot)).Time)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Tools.DbCommandWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.Tools.SqlCommandsManager)(null)).CommandList)).SyncRoot)).IsFailed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Tools.DbCommandWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.Tools.SqlCommandsManager)(null)).CommandList)).SyncRoot)).ExceptionMessage)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CommandsInfoListGrid|b2552bb1-778a-4ed3-9161-eab85014ba3c", "Text", "This represents the SQL command text.");
			zTextBoxColumnStyleInfo1.ColumnName = "SqlText";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CommandsInfoListGrid|98e3fae6-a6f7-45af-9e12-2ad5599db790", "Time", "This represents the SQL command created time.");
			zDateEditColumnStyleInfo1.ColumnName = "Time";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CommandsInfoListGrid|96fc099b-a6d6-4113-835c-37fd455fd53c", "Failed", "This represents whether the SQL command is failed.");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsFailed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("CommandsInfoListGrid|42f6d389-f278-48d1-b7b5-bb8134906c7d", "Exception Message", "This represents the SQL command exception.");
			zTextBoxColumnStyleInfo2.ColumnName = "ExceptionMessage";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "41755209-d0a4-429d-8d34-30848fd19d99";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.ReadOnly = true;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 215, true);
			this.zGrid1.TabIndex = 1;
			// 
			// txtCommandText
			// 
			this.txtCommandText.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.txtCommandText, "CommandList.SqlTextWithFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Tools.DbCommandWrapper)(((System.Collections.IList)(((Enterprise.ZArchitecture.Tools.SqlCommandsManager)(null)).CommandList)).SyncRoot)).SqlTextWithFormat)));
			this.txtCommandText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtCommandText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtCommandText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.txtCommandText.Multiline = true;
			this.txtCommandText.Name = "txtCommandText";
			this.txtCommandText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtCommandText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 273, true);
			this.txtCommandText.TabIndex = 0;
			// 
			// CommandsInfoListGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.sp);
			this.Name = "CommandsInfoListGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 521, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sp.Panel1.ResumeLayout(false);
			this.sp.Panel2.ResumeLayout(false);
			this.sp.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sp)).EndInit();
			this.sp.ResumeLayout(false);
			this.sp1.Panel1.ResumeLayout(false);
			this.sp1.Panel1.PerformLayout();
			this.sp1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.sp1)).EndInit();
			this.sp1.ResumeLayout(false);
			this.zToolStip.ResumeLayout(false);
			this.zToolStip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer sp;
		private ZTextBox txtCommandText;
		private ZGrid zGrid1;
		private CargoWise.Windows.UI.KSplitContainer sp1;
		private GUI.ZToolStrip zToolStip;
		private System.Windows.Forms.ToolStripButton tsButtonStart;
		private System.Windows.Forms.ToolStripButton tsButtonPause;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsButtonStop;
		private System.Windows.Forms.ToolStripButton tsButtonClear;
	}
}
