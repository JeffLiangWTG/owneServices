using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class CusHAWBAutoQueueMovementControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid QueueMovementGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.QueueMovementGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.QueueMovementGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// QueueMovementGrid
			// 
			this.QueueMovementGrid.AllowNavigation = false;
			this.QueueMovementGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))));
			this.QueueMovementGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Segment Name";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "FreeTextSegmentName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.Caption = "Segment Value";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "FreeTextSegmentValue";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "Queue+Lookups+QueueList";
			zDropEditColumnStyleInfo1.Caption = "Queue Name";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Queue+QueueName";
			zDropEditColumnStyleInfo2.BindToList = "Queue+Lookups+StatusList";
			zDropEditColumnStyleInfo2.Caption = "Reason Code";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "Queue+Status";
			zDropEditColumnStyleInfo3.BindToList = "Queue+Lookups+SubStatusList";
			zDropEditColumnStyleInfo3.Caption = "Status Code";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Queue+SubStatus";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Priority";
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "Priority";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			this.QueueMovementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QueueMovementGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QueueMovementGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QueueMovementGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.QueueMovementGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.QueueMovementGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QueueMovementGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueueMovementGrid.EnableToolTips = false;
			this.QueueMovementGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QueueMovementGrid.LayoutKey = "zGrid1";
			this.QueueMovementGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QueueMovementGrid.Name = "QueueMovementGrid";
			this.QueueMovementGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 384, true);
			this.QueueMovementGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).FreeTextSegmentNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).FreeTextSegmentName)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).FreeTextSegmentValueInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).FreeTextSegmentValue)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.QueueNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.QueueName)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.Lookups.QueueList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.StatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.Status)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.Lookups.StatusList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.SubStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.SubStatus)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovement)(((object)(((Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection)(null)))))).Queue.Lookups.SubStatusList)));
			// 
			// CusHAWBAutoQueueMovementControl
			// 
			this.Controls.Add(this.QueueMovementGrid);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Registry.Business.CusHAWBAutoQueueMovementCollection";
			this.Name = "CusHAWBAutoQueueMovementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 384, true);
			((System.ComponentModel.ISupportInitialize)(this.QueueMovementGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
