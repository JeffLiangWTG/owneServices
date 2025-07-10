using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.GUI
{
	partial class ExperimentalSettingsForm
	{
		private ZLabel info;
		private ZGrid grid;
		private ZButton saveButton;
		private ZButton cancelButton;

		protected override void InitializeComponent()
		{
			CaptionRenderingEnabled = true;
			MinimumSize = ControlDpiScalingHelper.NewScaledSize(500, 400, true);
			MaximumSize = ControlDpiScalingHelper.NewScaledSize(500, 400, true);
			Size = ControlDpiScalingHelper.NewScaledSize(500, 400, true);
			Text = ResString.GetMultilingualString("D2CAA38C-EFF5-44DE-AE98-918A1E6BFA1E", "View / Edit Experimental Settings");

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			grid = new ZGrid();
			((ISupportInitialize)(grid)).BeginInit();
			Controls.Add(grid);

			info = new ZLabel();
			Controls.Add(info);
			saveButton = new ZButton();
			Controls.Add(saveButton);
			cancelButton = new ZButton();
			Controls.Add(cancelButton);

			// Info
			info.Text = ResString.GetMultilingualString("2EF31C38-4BD6-482E-947B-B186F3AFB91C", "PAVE experimental settings can be specified per individual BM System / Board.They control behavior which is still in pilot stage. Please contact the PAVE team for settings usage and definitions.");
			info.Location = ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			info.Size = ControlDpiScalingHelper.NewScaledSize(465, 45, true);

			// Grid
			grid.AllowNavigation = false;
			BindingSource.SetBindingMember(grid, "ExperimentalSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CompileTimeCheckBindingMember.Check(((IList)(((ExperimentalSettingsProvider)(null)).ExperimentalSettings)));
			CompileTimeCheckBindingMember.Check(((ZString)(((ExperimentalSetting)(((IList)(((ExperimentalSettingsProvider)(null)).ExperimentalSettings)).SyncRoot)).Key)));
			CompileTimeCheckBindingMember.Check(((ZString)(((ExperimentalSetting)(((IList)(((ExperimentalSettingsProvider)(null)).ExperimentalSettings)).SyncRoot)).Value)));
			grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("7726DF6E-490C-4974-871A-17B4C770F4E6", "Experimental Setting");
			zTextBoxColumnStyleInfo1.ColumnName = "Key";
			zTextBoxColumnStyleInfo1.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("3D9EC437-62FB-4EB1-BBC8-87F87E257435", "Value");
			zTextBoxColumnStyleInfo2.ColumnName = "Value";
			zTextBoxColumnStyleInfo2.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			grid.CopySelectedRowsAllowed = true;
			grid.GridId = "8C522F84-895E-45D8-9DF8-A8FA90B55585";
			grid.LayoutKey = "ExperimentalSettingsGrid";
			grid.Location = ControlDpiScalingHelper.NewScaledPoint(10, 60, true);
			grid.Name = "ExperimentalSettingsGrid";			
			grid.Size = ControlDpiScalingHelper.NewScaledSize(465, 225, true);
			grid.TabIndex = 1;

			// Save button
			saveButton.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			saveButton.Location = ControlDpiScalingHelper.NewScaledPoint(325, 300, true);
			saveButton.Name = "SaveExperimentalSettingsButton";
			saveButton.Size = ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			saveButton.TabIndex = 2;
			saveButton.Text = Res.GetString("8A9751A8-B07F-4D8B-9C59-FAAE88209EDC", "Save");
			saveButton.UseVisualStyleBackColor = false;
			saveButton.Click += new EventHandler(SaveButton_Click);

			// Cancel button
			cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			cancelButton.Location = ControlDpiScalingHelper.NewScaledPoint(405, 300, true);
			cancelButton.Name = "CloseExperimentalSettingsButton";
			cancelButton.Size = ControlDpiScalingHelper.NewScaledSize(70, 25, true);
			cancelButton.TabIndex = 3;
			cancelButton.Text = Res.GetString("D391CD3C-6D6D-4979-AC90-92D2374DC00A", "Cancel");
			cancelButton.UseVisualStyleBackColor = false;
			cancelButton.Click += new EventHandler(CancelButton_Click);

			((ISupportInitialize)(grid)).EndInit();
		}
	}
}
