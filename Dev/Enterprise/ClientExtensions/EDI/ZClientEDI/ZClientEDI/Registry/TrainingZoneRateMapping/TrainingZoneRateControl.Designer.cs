

namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class TrainingZoneRateControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.gridRates = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridRates)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.TrainingZoneRateCollection);
			// 
			// gridRates
			// 
			this.gridRates.AllowNavigation = false;
			this.gridRates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.gridRates, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TrainingZoneRate)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.TrainingZoneRate)(null)).ZonePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TrainingZoneRate)(null)).Lookups.Zones)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.TrainingZoneRate)(null)).RateAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.TrainingZoneRate)(null)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.TrainingZoneRate)(null)).Lookups.Currencies)));
			this.gridRates.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+Zones";
			zGuidFindBoxColumnStyleInfo1.Caption = "Zone Code";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ZonePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.InternationalZone;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Rate Amount";
			zCalcEditColumnStyleInfo1.ColumnName = "RateAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+Currencies";
			zCodeFindBoxColumnStyleInfo1.Caption = "Currency";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CurrencyCode";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.gridRates.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.gridRates.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.gridRates.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.gridRates.GridId = "c769f6f5-0fed-4003-9e58-50d6385c4b0a";
			this.gridRates.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridRates.LayoutKey = "gridRates";
			this.gridRates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 8, true);
			this.gridRates.Name = "gridRates";
			this.gridRates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 315, true);
			this.gridRates.TabIndex = 0;
			// 
			// TrainingZoneRateControl
			// 
			this.Controls.Add(this.gridRates);
			this.Name = "TrainingZoneRateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridRates)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid gridRates;
	}
}
