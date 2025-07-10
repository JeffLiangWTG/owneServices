namespace Enterprise.Customs.GB.Module.DLU
{
	partial class DLUMessageFilterStripControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("18c35595-b011-4d7f-b4c2-5bf4d786aa99", "License");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("53c5e8d7-4d73-4fa2-ad7a-c480a6d286e2", "Has response?");
			zCheckBoxColumnStyleInfo1.ColumnName = "HasLinkedMessage";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("3cc361bc-125d-48fc-9059-2f95a4a6af10", "Badge");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageOwner";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("dbe99a0a-8877-4113-9d75-5b74e5bfe7c3", "Message number");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("d3d3a9d6-6eeb-462a-9090-11939c6f1248", "Type");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("b7334729-7ffe-4294-b3f9-8464fb6ca654", "Status");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("b10066f2-bdb1-4575-893e-7889c4bd6028", "Created date");
			zDateEditColumnStyleInfo1.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage);
			// 
			// DLUMessageFilterStripControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "DLUMessageFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
