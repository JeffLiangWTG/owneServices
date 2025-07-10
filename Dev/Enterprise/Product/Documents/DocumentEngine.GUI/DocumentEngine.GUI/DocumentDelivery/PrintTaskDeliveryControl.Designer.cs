namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	partial class PrintTaskDeliveryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.deliveryDestinationsControl1 = new Enterprise.DocumentEngine.GUI.DocumentDelivery.PrintTaskDeliveryDestinationControl();
			this.deliveryInstructionsInfoControl1 = new Enterprise.DocumentEngine.GUI.DocumentDelivery.DeliveryInstructionsInfoControl();
			this.RecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.PrintTaskSettings);
			// 
			// DocumentsGroupBox
			// 
			this.DocumentsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryControl|d2d06d73-0dd5-4320-b455-ed39f59d9efb", "Document Packs");
			this.DocumentsGroupBox.Controls.Add(this.deliveryDestinationsControl1);
			this.DocumentsGroupBox.Controls.Add(this.deliveryInstructionsInfoControl1);
			this.DocumentsGroupBox.Controls.Add(this.RecipientsGrid);
			this.DocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentsGroupBox.Name = "DocumentsGroupBox";
			this.DocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 349, true);
			this.DocumentsGroupBox.TabIndex = 4;
			this.DocumentsGroupBox.TabStop = false;
			// 
			// deliveryDestinationsControl1
			// 
			this.deliveryDestinationsControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.deliveryDestinationsControl1, ".");
			this.deliveryDestinationsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 288, true);
			this.deliveryDestinationsControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 55, true);
			this.deliveryDestinationsControl1.Name = "deliveryDestinationsControl1";
			this.deliveryDestinationsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 55, true);
			this.deliveryDestinationsControl1.TabIndex = 4;
			// 
			// deliveryInstructionsInfoControl1
			// 
			this.deliveryInstructionsInfoControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.deliveryInstructionsInfoControl1, ".");
			this.deliveryInstructionsInfoControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 138, true);
			this.deliveryInstructionsInfoControl1.Name = "deliveryInstructionsInfoControl1";
			this.deliveryInstructionsInfoControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 143, true);
			this.deliveryInstructionsInfoControl1.TabIndex = 3;
			// 
			// RecipientsGrid
			// 
			this.RecipientsGrid.AllowBeginDrag = false;
			this.RecipientsGrid.AllowDragDropWithChanges = false;
			this.RecipientsGrid.AllowNavigation = false;
			this.RecipientsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RecipientsGrid, "DocPacksDeliveryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DeliveryInstructions)(((System.Collections.IList)(((Enterprise.DocumentEngine.PrintTaskSettings)(null)).DocPacksDeliveryInstructions)).SyncRoot)).DocumentPackTitle)));
			this.RecipientsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryControl|50553478-e289-43d0-bd59-0f32bac78d2d", "Document Pack Title");
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentPackTitle";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecipientsGrid.GridId = "4a11f6ee-aa18-4104-b8fb-272f1af4787d";
			this.RecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientsGrid.IsWholeRowSelectedOnClick = true;
			this.RecipientsGrid.LayoutKey = "zGrid1";
			this.RecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.RecipientsGrid.Name = "RecipientsGrid";
			this.RecipientsGrid.ReadOnly = true;
			this.RecipientsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 112, true);
			this.RecipientsGrid.TabIndex = 2;
			// 
			// PrintTaskDeliveryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DocumentsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 349, true);
			this.Name = "PrintTaskDeliveryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 349, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox DocumentsGroupBox;
		private Enterprise.ZArchitecture.ZGrid RecipientsGrid;
		private DeliveryInstructionsInfoControl deliveryInstructionsInfoControl1;
		private PrintTaskDeliveryDestinationControl deliveryDestinationsControl1;
	}
}
