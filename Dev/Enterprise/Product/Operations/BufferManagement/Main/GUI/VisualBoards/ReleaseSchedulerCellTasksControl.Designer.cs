using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ReleaseSchedulerCellTasksControl
	{
		#region Windows Form Designer generated code
		ZArchitecture.GUI.ZButton SaveButton;
		void InitializeComponent()
		{
			this.SaveButton = new ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ChannelDayHeaderLabel
			// 
			this.ChannelDayHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6af36e74-dab1-4c2e-90c8-fb45990f9815", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 0, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 25, true);
			this.SaveButton.TabIndex = 4;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new EventHandler(this.SaveButton_Click);
			// 
			// ReleaseSchedulerCellTasksControl
			// 
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.Controls.Add(this.SaveButton);
			this.Name = "ReleaseSchedulerCellTasksControl";
			this.Controls.SetChildIndex(this.TaskCardsPanel, 0);
			this.Controls.SetChildIndex(this.ChannelDayHeaderLabel, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
