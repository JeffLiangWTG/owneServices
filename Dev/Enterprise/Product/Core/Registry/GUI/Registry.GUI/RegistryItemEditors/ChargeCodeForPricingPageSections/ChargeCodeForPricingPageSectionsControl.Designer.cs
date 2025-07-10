namespace Enterprise.Registry.GUI
{
	partial class ChargeCodeForPricingPageSectionsControl
	{
		internal Enterprise.ZArchitecture.ZGrid ConfigurationGrid;
		internal Enterprise.ZArchitecture.ZGrid ChargesGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ChargesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).BeginInit();
			this.ConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration);
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.ChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargesGrid, "Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.ChargeCodeGroup)(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).Charges)).SyncRoot)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeGroup)(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).Charges)).SyncRoot)).ChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeCodeGroup)(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).Charges)).SyncRoot)).ChargeCodeDescription)));
			this.ChargesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "ChargeCodeList";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeCodeForPricingPageSectionsControl|31e5d5c6-3955-403a-ae92-39f2f7aedecf", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeCodeForPricingPageSectionsControl|1966d62c-7a32-4405-916d-c8cabe6c5432", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.GridId = "2585f05c-af88-4b1a-84ba-38c7c477e7d6";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargeCodesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 213, true);
			this.ChargesGrid.TabIndex = 6;
			// 
			// ConfigurationGrid
			// 
			this.ConfigurationGrid.AllowNavigation = false;
			this.ConfigurationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).PricingPage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).PricingPageList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).Section)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeCodeForPricingPageSectionsConfiguration)(null)).SectionList)));
			this.ConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeCodeForPricingPageSectionsControl|dab1bb26-7396-4453-9b33-d9d3e185c329", "Pricing Page");
			zDropEditColumnStyleInfo1.ColumnName = "PricingPage";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeCodeForPricingPageSectionsControl|26debd91-0cdd-48d6-a126-f3884634eb11", "Section");
			zDropEditColumnStyleInfo2.ColumnName = "Section";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ConfigurationGrid.GridId = "128bfe3c-4140-4b58-8089-dbda3a2fbf24";
			this.ConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfigurationGrid.LayoutKey = "ConfigurationGrid";
			this.ConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationGrid.Name = "ConfigurationGrid";
			this.ConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 128, true);
			this.ConfigurationGrid.TabIndex = 5;
			// 
			// ChargeCodeForPricingPageSectionsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargesGrid);
			this.Controls.Add(this.ConfigurationGrid);
			this.Name = "ChargeCodeForPricingPageSectionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).EndInit();
			this.ConfigurationGrid.ResumeLayout(false);
			this.ConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
