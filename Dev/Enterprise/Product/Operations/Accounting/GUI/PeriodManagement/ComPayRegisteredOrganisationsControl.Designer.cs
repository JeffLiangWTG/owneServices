using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ComPayRegisteredOrganisationsControl
	{
void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			this.DescriptionGroupBox = new ZGroupBox();
			this.FindButton = new ZButton();
			this.ComPayGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComPayGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ComPayRegisteredOrganisationDataSource);
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionGroupBox.CaptionResourceString = null;
			this.DescriptionGroupBox.Controls.Add(this.FindButton);
			this.DescriptionGroupBox.Controls.Add(this.ComPayGrid);
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(646, 688, true);
			this.DescriptionGroupBox.TabIndex = 3;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComPayRegisteredOrganisationsControl|d6938663-da96-4cf0-84cf-fa9d8988cb4e", "Find");
			this.FindButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FindButton.TabIndex = 1;
			this.FindButton.Click += new EventHandler(this.FindButton_Click);
			// 
			// ComPayGrid
			// 
			this.ComPayGrid.AllowNavigation = false;
			this.ComPayGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ComPayGrid, "ComPayRegisteredOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).ClientName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).ABN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).ECN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).RelatedOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).RegistrationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Suburb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(((System.Collections.IList)(((ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).TerminalCode)));
			this.ComPayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "ABN";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ECN";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "RelatedOrganisations";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.ColumnName = "RegistrationDate";
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.ColumnName = "Address1";
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.ColumnName = "Address2";
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = null;
			zTextBoxColumnStyleInfo6.ColumnName = "Suburb";
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = null;
			zTextBoxColumnStyleInfo7.ColumnName = "State";
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = null;
			zTextBoxColumnStyleInfo8.ColumnName = "Postcode";
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = null;
			zTextBoxColumnStyleInfo9.ColumnName = "Country";
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = null;
			zTextBoxColumnStyleInfo10.ColumnName = "Phone";
			zTextBoxColumnStyleInfo11.Caption = null;
			zTextBoxColumnStyleInfo11.CaptionResourceString = null;
			zTextBoxColumnStyleInfo11.ColumnName = "Fax";
			zTextBoxColumnStyleInfo12.Caption = null;
			zTextBoxColumnStyleInfo12.CaptionResourceString = null;
			zTextBoxColumnStyleInfo12.ColumnName = "TerminalCode";
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ComPayGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ComPayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ComPayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ComPayGrid.CopySelectedRowsAllowed = true;
			this.ComPayGrid.GridId = "7E4F3800-7A65-4CC0-B4C6-FDD9F9F22850";
			this.ComPayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComPayGrid.IsWholeRowSelectedOnClick = true;
			this.ComPayGrid.LayoutKey = "zGrid1";
			this.ComPayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 48, true);
			this.ComPayGrid.Name = "ComPayGrid";
			this.ComPayGrid.ReadOnly = true;
			this.ComPayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 632, true);
			this.ComPayGrid.TabIndex = 0;
			this.ComPayGrid.DoubleClick += new EventHandler(this.ComPayGrid_DoubleClick);
			// 
			// ComPayRegisteredOrganisationsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionGroupBox);
			this.Name = "ComPayRegisteredOrganisationsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 694, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ComPayGrid)).EndInit();
			this.ResumeLayout(false);
		}

	}
}