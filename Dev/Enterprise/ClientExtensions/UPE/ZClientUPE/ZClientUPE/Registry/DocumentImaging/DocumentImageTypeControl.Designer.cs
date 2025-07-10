using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class DocumentImageTypeControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid Grid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "UPS Code";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "UPSCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo1.BindToList = "DocTypeCode_List";
			zDropEditColumnStyleInfo1.Caption = "Doc Type";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "DocTypeCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo2.Caption = "Description";
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.Caption = "Notify";
			zCheckBoxColumnStyleInfo1.ColumnName = "NotifyOnImport";
			zCheckBoxColumnStyleInfo1.ToolTip = "Notify the user by an email when he image has been imported";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.Caption = "Move Job to Working";
			zCheckBoxColumnStyleInfo2.ColumnName = "MoveJobToClassOnImport";
			zCheckBoxColumnStyleInfo2.ToolTip = "Move the job to the \'Classification\' queue if it\'s status is \'Missing Invoice\', \'" +
				"Documents insufficient\' or \'Poor image\'";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.EnableToolTips = false;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "zGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 384, true);
			this.Grid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).UPSCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).UPSCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).DocTypeCode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).DocTypeCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).DocTypeCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).Description)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).NotifyOnImport)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).NotifyOnImportInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).MoveJobToClassOnImport)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.DocumentImageType)(((object)(((Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection)(null)))))).MoveJobToClassOnImportInfo)));
			// 
			// DocumentImageTypeControl
			// 
			this.Controls.Add(this.Grid);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Registry.Business.DocumentImageTypeCollection";
			this.Name = "DocumentImageTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 384, true);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
