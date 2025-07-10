using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZCodeFindBoxPopup
	{

		#region Designer Generated Code

		protected Enterprise.ZArchitecture.GUI.ZButton CancelBtn;
		protected Enterprise.ZArchitecture.GUI.ZButton OKBtn;
		protected Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		protected Enterprise.ZArchitecture.ZTextBox CodeTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton FindBtn;
		protected ZDisplayGrid Grid;

		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo CodeColumnStyle = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo DescriptionColumnStyle = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

		protected override void InitializeComponent()
		{
			this.Grid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 22, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// Grid
			// 
			this.Grid.AllowBeginDrag = false;
			this.Grid.AllowNavigation = false;
			this.Grid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.Grid.CaptionVisible = false;
			this.Grid.GridId = "40bcf514-ff5d-472f-9db4-cc02ac4d4f2d";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.IsWholeRowSelectedOnClick = true;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.Grid.Name = "Grid";
			this.Grid.ReadOnly = true;
			this.Grid.ShouldSetErrorsOnTabPage = false;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 225, true);
			this.Grid.TabIndex = 5;
			this.Grid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Grid_KeyDown);
			this.Grid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Grid_MouseDown);
			this.CodeColumnStyle.ColumnName = "Code";
			this.CodeColumnStyle.IsMandatory = true;
			this.DescriptionColumnStyle.ColumnName = "Description";
			this.DescriptionColumnStyle.IsMandatory = true;
			this.DescriptionColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.Grid.ColumnStyles.Add(CodeColumnStyle);
			this.Grid.ColumnStyles.Add(DescriptionColumnStyle);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelBtn.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZCodeFindBoxPopup|0dab52a9-c41b-4cbf-b00f-387a4eb9385c", "Cancel");
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 284, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelBtn.TabIndex = 7;
			this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.DescriptionTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZCodeFindBoxPopup|93df6a60-0b36-4c37-a399-e4b166096470", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 30, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// CodeTextBox
			// 
			this.CodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.CodeTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZCodeFindBoxPopup|8d4234a2-ba94-4e6f-8cde-1a1d566adfd6", "Code");
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 7, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CodeTextBox.TabIndex = 1;
			// 
			// OKBtn
			// 
			this.OKBtn.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.OKBtn.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZCodeFindBoxPopup|cb429f80-56e0-4663-8080-2e354a9715d7", "OK");
			this.OKBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 284, true);
			this.OKBtn.Name = "OKBtn";
			this.OKBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKBtn.TabIndex = 6;
			this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
			// 
			// FindBtn
			// 
			this.FindBtn.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.FindBtn.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZCodeFindBoxPopup|6060c668-39f7-48ac-a833-310ac97cbfb2", "Filter");
			this.FindBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 7, true);
			this.FindBtn.Name = "FindBtn";
			this.FindBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.FindBtn.TabIndex = 4;
			this.FindBtn.Click += new System.EventHandler(this.FindBtn_Click);
			// 
			// ZCodeFindBoxPopup
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 334, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZCodeFindBoxPopup|bb8a6441-cb29-413c-ae45-8745e729f9b5", "Find");
			this.Controls.Add(this.FindBtn);
			this.Controls.Add(this.CodeTextBox);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.OKBtn);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.Grid);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 342, true);
			this.Name = "ZCodeFindBoxPopup";
			this.Controls.SetChildIndex(this.Grid, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.OKBtn, 0);
			this.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.CodeTextBox, 0);
			this.Controls.SetChildIndex(this.FindBtn, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
