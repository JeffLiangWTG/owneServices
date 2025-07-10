namespace Enterprise.Customs.ES.GUI
{
	partial class ImportEntryLineTaxAndFeeUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.EntryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryLineDutyAndTaxGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
			this.EntryLineDutyAndTaxGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.AllCusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>);
			// 
			// EntryLineDutyAndTaxGroupBox
			// 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("8D71BB75-87D0-4627-AB03-02E8FA794D63", "Duty And Tax");
			this.EntryLineDutyAndTaxGroupBox.Controls.Add(this.EntryLineDutyAndTaxGrid);
			this.EntryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineDutyAndTaxGroupBox, true);
			this.EntryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLineDutyAndTaxGroupBox.Name = "EntryLineDutyAndTaxGroupBox";
			this.EntryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 314, true);
			this.EntryLineDutyAndTaxGroupBox.TabIndex = 5;
			this.EntryLineDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineDutyAndTaxGrid
			// 
			this.EntryLineDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineDutyAndTaxGrid, "Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).MaxMin)));
			this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("E0D091AF-D177-4B38-A751-EDF485405F6B", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.ES.GUI.Res.GetData("329CF68A-F0C3-4BE6-BB05-352F13B5CBF3", "Charge Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("2058D777-6C9B-4C40-8240-3706355ECF6A", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.ES.GUI.Res.GetData("329CF68A-F0C3-4BE6-BB05-352F13B5CBF3", "Charge Type");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("F26588AD-298A-4066-9B6B-2DB33D42757F", "Action");
			zDropEditColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("414E71A5-E014-4C27-B461-21A80C1ECB3D", "Base Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("9C358969-11E1-46FF-B4FE-53E5686A6590", "Method of Calculation");
			zDropEditColumnStyleInfo3.ColumnName = "CF_MethodOfCalculation";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("9EC47F8E-772E-4CB8-8E0A-4465B8918F7C", "Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("4FD2806A-F67D-4F4A-840E-6400D41A931A", "Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("BFA372C5-85B1-44E3-BAC5-A58F5E38EABD", "Method of Payment");
			zDropEditColumnStyleInfo4.ColumnName = "CF_MethodOfPayment";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("EFB8D8D9-5E4F-40B6-B720-7D0F562EF62C", "Max-Min");
			zTextBoxColumnStyleInfo2.ColumnName = "MaxMin";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineDutyAndTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
			this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 295, true);
			this.EntryLineDutyAndTaxGrid.TabIndex = 0;
			// 
			// ImportEntryLineTaxAndFeeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryLineDutyAndTaxGroupBox);
			this.Name = "ImportEntryLineTaxAndFeeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryLineDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineDutyAndTaxGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
			this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineDutyAndTaxGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox EntryLineDutyAndTaxGroupBox;
		internal ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
	}
}
