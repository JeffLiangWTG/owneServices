using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ECCCVehicleUserControl : ZUserControl
	{
		public ECCCVehicleUserControl()
		{
			InitializeComponent();
		}

		public ECCCVehicleUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			if (!DesignMode)
			{
				InitializeLazyCreate(isOnInvoiceLine);
			}
		}

		void InitLPCOControlStyle()
		{
			var list = new List<string>(ECCCPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_IsHolderOverridden);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactEmail);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactName);
			list.Remove(CusCALPCO.Schema.CLP_HolderContactPhone);
			list.Remove(CusCALPCO.Schema.CLP_EndDate);
			list.Remove(CusCALPCO.Schema.CLP_HolderType);
			list.Remove(CusCALPCO.Schema.LPCOHolderOrgPK);
			list.Remove(CusCALPCO.Schema.CLP_HolderName);
			list.Remove(CusCALPCO.Schema.CLP_OA_Holder);
			list.Remove(CusCALPCO.Schema.CLP_IssueDate);
			list.Remove(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity);
			list.Remove(CusCALPCO.Schema.CLP_AlternativeQuotaUQ);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			InitLPCOControlStyle();
			if (!isOnInvoiceLine)
			{
				VINTextBox.Parent.Controls.Remove(VINTextBox);
				VehicleModelYearDropEdit.Parent.Controls.Remove(VehicleModelYearDropEdit);
			}
		}

		void CA_ProcessCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (DataSource is ECCCPGAHeader eccc)
			{
				LPCOGroupBox.Visible = eccc.XE02SpecificControlsVisible;
				EngineLocationAddressControl.Visible = eccc.XE02SpecificControlsVisible;
				EvidenceOfConformityLocationAddressControl.Visible = eccc.XE02SpecificControlsVisible;

				if (eccc.XE02SpecificControlsVisible)
				{
					DetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(924, 160, true);
					EngineGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(924, 155, true);
				}
				else
				{
					DetailsGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(924, 100, true);
					EngineGroupBox.Size = ControlDpiScalingHelper.NewScaledSize(924, 125, true);
				}
			}
		}

		public const string IsVisibleForBindingString = "IsVisibleForBinding";
		public const string ShowOrganisationForBinding = "ShowOrganisationForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			EngineGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			VehicleGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			MachineGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			LPCOGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			TestGroupTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			PowerCalcDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			BulkReportingApprovalCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			EvaporativeFamilyTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			MachineModelYearDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			TransitionCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			IncompleteCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			NonCommercialImportCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ReplacementEnginesCheckBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			EngineComplianceStatementGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AOSConformityDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AOSReplacementDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AOSEvidenceDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AOSRetentionDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AlternativeStandardOfEngineClassDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				EngineGroupBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "EngineGroupBoxVisible", false, DataSourceUpdateMode.Never));
				VehicleGroupBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "VehicleGroupBoxVisible", false, DataSourceUpdateMode.Never));
				LPCOGroupBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
				MachineGroupBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "MachineGroupBoxVisible", false, DataSourceUpdateMode.Never));
				TestGroupTextBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "TestGroupTextBoxVisible", false, DataSourceUpdateMode.Never));
				PowerCalcDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "PowerCalcDropEditVisible", false, DataSourceUpdateMode.Never));
				EvaporativeFamilyTextBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "EvaporativeFamilyTextBoxVisible", false, DataSourceUpdateMode.Never));
				MachineModelYearDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "MachineModelYearDropEditVisible", false, DataSourceUpdateMode.Never));
				TransitionCheckBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "TransitionCheckBoxIncompleteCheckBoxVisible", false, DataSourceUpdateMode.Never));
				IncompleteCheckBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "TransitionCheckBoxIncompleteCheckBoxVisible", false, DataSourceUpdateMode.Never));
				ReplacementEnginesCheckBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
				EngineComplianceStatementGroupBox.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "EngineComplianceStatementGroupBoxVisible", false, DataSourceUpdateMode.Never));
				AOSConformityDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
				AOSReplacementDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
				AOSEvidenceDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
				AOSRetentionDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
				AlternativeStandardOfEngineClassDropEdit.DataBindings.Add(
					new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "XE02SpecificControlsVisible", false, DataSourceUpdateMode.Never));
			}

			if (dataSource is ECCCPGAHeader eccc)
			{
				eccc.CA_ProcessCodeInfo.ValueChanged -= CA_ProcessCodeInfo_ValueChanged;
				eccc.CA_ProcessCodeInfo.ValueChanged += CA_ProcessCodeInfo_ValueChanged;
				CA_ProcessCodeInfo_ValueChanged(null, null);
			}
		}
	}
}
