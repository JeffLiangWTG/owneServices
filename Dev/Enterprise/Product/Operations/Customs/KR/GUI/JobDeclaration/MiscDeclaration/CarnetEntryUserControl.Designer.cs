namespace Enterprise.Customs.KR.GUI
{
	partial class CarnetEntryUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.CarnetEntryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CarnetEntryGrid)).BeginInit();
			this.CarnetEntryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// CarnetEntryGrid
			// 
			this.CarnetEntryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CarnetEntryGrid, "CustomsEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CarnetCertificateNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryHeaderStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
			this.CarnetEntryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("64B093FD-49AA-4EA0-8597-55BC9208BAD9", "Carnet Certificate No.");
			zTextBoxColumnStyleInfo1.ColumnName = "CarnetCertificateNo";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("f06b7feb-5461-47ed-bfb2-d7f4e9719f96", "Message Type");
			zDropEditColumnStyleInfo1.ColumnName = "CH_MessageType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9c8cc2aa-f60b-44ad-a832-b0755bdb910f", "Message Type Desc.");
			zTextBoxColumnStyleInfo2.ColumnName = "MessageTypeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d014a7a3-4638-4572-9881-2a3ef45b141f", "Msg. Status");
			zDropEditColumnStyleInfo2.ColumnName = "CH_Status";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9c1d4655-dc6e-4d88-8854-ab08eba25df3", "Msg. Status Desc.");
			zTextBoxColumnStyleInfo3.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("22449734-17d7-4fd0-8ef5-5355d5d78f93", "Ent. Status");
			zDropEditColumnStyleInfo3.ColumnName = "CH_EntryStatus";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e710a05b-1fbd-4f2f-8846-f1d556009b3e", "Ent. Status Desc.");
			zTextBoxColumnStyleInfo4.ColumnName = "EntryHeaderStatusDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("c967971f-725b-4c09-8ce5-372a33866357", "Entry Submitted Date");
			zDateEditColumnStyleInfo1.ColumnName = "CH_EntrySubmittedDate";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.CarnetEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CarnetEntryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CarnetEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CarnetEntryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CarnetEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CarnetEntryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CarnetEntryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CarnetEntryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CarnetEntryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CarnetEntryGrid.GridId = "dcacfb5c-1018-4b9c-84e2-1cfbed1d5f87";
			this.CarnetEntryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CarnetEntryGrid.LayoutKey = "CarnetEntryGrid";
			this.CarnetEntryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CarnetEntryGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.CarnetEntryGrid.Name = "CarnetEntryGrid";
			this.CarnetEntryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 146, true);
			this.CarnetEntryGrid.TabIndex = 0;
			// 
			// CarnetEntryUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CarnetEntryGrid);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Name = "CarnetEntryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 146, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CarnetEntryGrid)).EndInit();
			this.CarnetEntryGrid.ResumeLayout(false);
			this.CarnetEntryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CarnetEntryGrid;
	}
}
