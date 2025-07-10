using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class DeletePeriodsFromForm
	{


		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.OKButton = new ZButton();
			this.zStartDateEdit = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zStartDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DeletePeriodsFromSetting);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DeletePeriodsRangeForm|81970523-078A-42D1-9605-BA1888EC8F1A", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 82, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.ToolTipCaption = null;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DeletePeriodsRangeForm|67766050-FDCC-42E9-9642-4993B7342543", "Continue");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 82, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// zStartDateEdit
			// 
			this.zStartDateEdit.AllowDrop = true;
			this.zStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zStartDateEdit, "DeletePeriodsFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DeletePeriodsFromSetting)(null)).DeletePeriodsFrom)));
			this.zStartDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DeletePeriodsRangeForm|FABEA0A4-E7F2-4A04-A7FA-7E6A8B0442C1", "Delete Periods From");
			this.zStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 34, true);
			this.zStartDateEdit.Name = "zStartDateEdit";
			this.zStartDateEdit.TabIndex = 1;
			// 
			// DeletePeriodsFromForm
			// 
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DeletePeriodsRangeForm|C983960A-F127-47E5-9A7E-7A0354350037", "Delete Period From");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 134, true);
			this.Controls.Add(this.zStartDateEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(DeletePeriodsFromSetting);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.DeletePeriodsRangeSetting";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DeletePeriodsFromForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zStartDateEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zStartDateEdit.ResumeLayout(true);
			this.zStartDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			deletePeriodsRangeSetting.RunPreSaveValidation();
			if (deletePeriodsRangeSetting.DeletePeriodsFromInfo.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("E4F7EE2F-059C-42F5-9DDD-69462F1A9FCA", "There are errors that need to be corrected."));
			}
			else
			{
				try
				{
					DialogResult result = Globals.Message.Show(Res.GetString("7F570780-5E71-4C7F-8190-EB99D6D8EC57", "Are you sure you want to delete all periods from the specified date in company [{0}]?", GlbCompany.CurrentCompany.GC_Name), Res.GetString("399F96BF-0116-4CF5-86BF-5465CCF4E7DA", "Delete Periods"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (result == DialogResult.Yes)
					{
						periodManager.DeletePeriodsFromStartDateInCurrentCompany(deletePeriodsRangeSetting.DeletePeriodsFrom);
						Globals.Message.Show(Res.GetString("DA78A101-4D42-462A-A0C4-065A9AC56A84", "All periods from specified date have been deleted"), Res.GetString("FD782102-D1DA-48A0-AF62-F9F0E6BCD07B", "Delete Periods"), MessageBoxButtons.OK, MessageBoxIcon.Information);
						Close();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		#endregion

	}
}