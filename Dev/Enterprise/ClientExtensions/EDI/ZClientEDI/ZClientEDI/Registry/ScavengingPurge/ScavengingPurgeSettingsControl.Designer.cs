namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ScavengingPurgeSettingsControl
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
        void InitializeComponent()
        {
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.gridScavengingItems = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridScavengingItems)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.ScavengingPurgeItem);
			// 
			// gridScavengingItems
			// 
			this.gridScavengingItems.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridScavengingItems, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ScavengingPurgeItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ScavengingPurgeItem)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.ScavengingPurgeItem)(null)).PurgeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.ScavengingPurgeItem)(null)).PurgeTimeUnit)));
			this.gridScavengingItems.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("0ff95cd7-5214-49f6-aff5-e738fb6619a9", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("ffed832d-6e83-4316-a6ad-cbee54063d5d", "Time");
			zCalcEditColumnStyleInfo1.ColumnName = "PurgeTime";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("cfb0e5a2-953d-4e27-9266-d314a7af0cb9", "Time Unit");
			zGuidDropEditColumnStyleInfo1.ColumnName = "PurgeTimeUnit";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.gridScavengingItems.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridScavengingItems.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.gridScavengingItems.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.gridScavengingItems.CopySelectedRowsAllowed = true;
			this.gridScavengingItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridScavengingItems.GridId = "c769f6f5-0fed-4003-9e58-50d6385c4b0a";
			this.gridScavengingItems.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridScavengingItems.LayoutKey = "gridRates";
			this.gridScavengingItems.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gridScavengingItems.Name = "gridScavengingItems";
			this.gridScavengingItems.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 462, true);
			this.gridScavengingItems.TabIndex = 1;
			// 
			// ScavengingPurgeSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.gridScavengingItems);
			this.Name = "ScavengingPurgeSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 462, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridScavengingItems)).EndInit();

        }

        #endregion

        public Enterprise.ZArchitecture.ZGrid gridScavengingItems;
    }
}
