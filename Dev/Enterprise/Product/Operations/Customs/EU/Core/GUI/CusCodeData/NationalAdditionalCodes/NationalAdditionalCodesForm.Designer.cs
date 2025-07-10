using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class NationalAdditionalCodesForm
	{
		ZArchitecture.ZGrid nationalAdditionalCodesGrid;
		ZButton closeButton;
		ZButton oKButton;

		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.nationalAdditionalCodesGrid = new ZArchitecture.ZGrid();
			this.closeButton = new ZButton();
			this.oKButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.nationalAdditionalCodesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 234, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(NationalAdditionalCodeCollection);
			// 
			// OrderItemsGrid
			// 
			this.nationalAdditionalCodesGrid.AllowNavigation = false;
			this.nationalAdditionalCodesGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.nationalAdditionalCodesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NationalAdditionalCode)(null)).CY_Code);
			this.nationalAdditionalCodesGrid.CaptionVisible = false;
			this.nationalAdditionalCodesGrid.GridId = "0FE708CA-EE1F-4FB3-961A-9CB29F3F2B85";
			this.nationalAdditionalCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.nationalAdditionalCodesGrid.LayoutKey = "NationalAdditionalCodesGrid";
			this.nationalAdditionalCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.nationalAdditionalCodesGrid.Name = "NationalAdditionalCodesGrid";
			this.nationalAdditionalCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 206, true);
			this.nationalAdditionalCodesGrid.TabIndex = 1;
			this.nationalAdditionalCodesGrid.AllowSorting = false;
			// 
			// CloseButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("NationalAdditionalCodesForm|D8FA764B-D0DB-4028-B4CE-582C0F5EAD1F", "Cancel");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.closeButton.Name = "CloseButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.Click += new EventHandler(this.OnCloseButton_Click);
			// 
			// OKButton
			// 
			this.oKButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.oKButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("NationalAdditionalCodesForm|C4F6E4F4-FA55-4038-A388-D32D501CF927", "OK");
			this.oKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 211, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.oKButton.TabIndex = 2;
			this.oKButton.Click += new EventHandler(this.OnOKButton_Click);
			// 
			// NationalAdditionalCodesForm
			// 
			this.AcceptButton = this.oKButton;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("NationalAdditionalCodesForm|CCD43913-13DA-4235-BE7D-11828E8ECF45", "National Additional Codes");
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.nationalAdditionalCodesGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.EU.Business";
			this.DataSourceType = typeof(NationalAdditionalCodeCollection);
			this.DataSourceTypeName = "Enterprise.Customs.EU.Business.NationalAdditionalCodeCollection";
			this.Name = "NationalAdditionalCodesForm";
			this.Controls.SetChildIndex(this.nationalAdditionalCodesGrid, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.nationalAdditionalCodesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		readonly Container components;
		
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
