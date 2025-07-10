namespace Enterprise.Customs.GB.Module
{
	partial class CcsukGenralMessageFilterStripControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("40aea3e8-2a3b-43e9-85bf-21a6b4e146d3", "PIMA");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("f9f2fd61-80b8-4aa2-a3ce-0bf096ddb072", "Direction");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("c7e6ba2f-62b7-42b6-a2c8-c67954360b6b", "Message number");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("2c3aa109-4fe8-4985-b11d-5f88cdf35db0", "Type");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("dfc170cc-9366-4ffd-add1-d3e402b2fc11", "Sub Type");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("0c1827dc-5ad4-4062-89c5-aeb744519dc2", "Local Profile");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_MessageOwner";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("41d7fe1f-fd76-4a7a-a12e-d36db766d783", "Status");
			zTextBoxColumnStyleInfo7.ColumnName = "EM_Status";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("4d736988-f54b-4d25-83c2-ce22980e59fe", "Created date");
			zDateEditColumnStyleInfo1.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("9b6cf8f1-cab8-471b-a42e-8206964d31de", "Has related message?");
			zCheckBoxColumnStyleInfo1.ColumnName = "HasLinkedMessage";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage);
			// 
			// CcsukGenralMessageFilterStripControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "CcsukGenralMessageFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
