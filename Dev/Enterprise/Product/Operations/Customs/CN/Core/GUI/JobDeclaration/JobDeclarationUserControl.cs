using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CNJobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		readonly bool twoStepDeclarationActive = CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.Value;
		public CNJobDeclarationUserControl()
		{
			InitializeComponent();
			RemoveObsoleteAddressControls();
			BindingSource.SetBindingMember(JE_MarksAndNumbersLongTextBox, "JE_MarksAndNumbers");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var declaration = Declaration;
			if (declaration != null)
			{
				JE_ClearanceModeDropEdit.Visible = twoStepDeclarationActive;
				XC_ClearanceModeInfo_ValueChanged(this, null);
				XC_TransitModeInfo_ValueChanged(this, null);
			}
		}

		void RemoveObsoleteAddressControls()
		{
			Controls.Remove(ImporterOrganisationControl);
			Controls.Remove(SupplierOrganisationControl);
		}

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		void OfficeOfDestination_ValueChanged(object sender, EventArgs e)
		{
			var currentCode = (CustomsOffice)sender;
			var allCodes = Declaration?.CustomsOffices?.Cast<CustomsOffice>();
			var instructions = Declaration.CustomsEntryInstructions.Cast<Business.CusEntryInstruction>().Where(x => x.IsCIQRequiresEditableAndFalse);

			if (!currentCode.CY_Data.IsEmpty
				&& allCodes != null && allCodes.Count(code => !code.CY_Data.IsEmpty) == 1
				&& instructions.Any()
				&& PromptMarkInstructionsRequireCIQ()
			)
			{
				instructions.ForEach(x => x.CEI_CIQRequires = true);
			}
		}

		bool PromptMarkInstructionsRequireCIQ()
		{
			return Globals.Message.Show(
				Res.GetString("DE10E921-57DE-4A7A-BCE3-8588E54F8A3F",
				"You are entering CIQ data, do you want to mark all of the Entry Instructions as Requires CIQ?"),
				ResContinue, MessageBoxButtons.OKCancel, DialogResult.Cancel
			) == DialogResult.OK;
		}

		static string ResContinue => Res.GetString("78C30EAA-0523-427F-A7A8-6197490D3283", "Continue?");

		protected override void SetContainerCountAndNoOfPieces()
		{
			base.SetContainerCountAndNoOfPieces();
			JE_ContainerCountCalcEdit.Visible = false;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			this.SuspendDrawing();
			base.HandleDeclarationControlVisibilityChangedCore();
			ChangeControlsVisibility();
			this.ResumeDrawing();
		}

		protected override void ChangeControlsVisibility()
		{
			var isImport = Declaration.IsImport;
			ReceiptNumberTextBox.Visible = Declaration.IsRail;
			JE_CNLastPortBeforeEntryCodeFindBox.Visible = isImport;
			PortOfStopoverCodeFindBox.Visible = isImport;

			var officeOfEntryExitCaption = Declaration.IsImport
				? Res.GetData("22AF2CD9-D2FF-4584-B59F-483F73F7EF6D", "Office of Entry")
				: Res.GetData("1C829C89-942D-4AC3-AAFA-1BCE2F69D299", "Office of Exit");
			if (!officeOfEntryExitCaption.ContentEquals(JE_OfficeOfEntryExitCodeFindBox.CaptionResourceString))
			{
				JE_OfficeOfEntryExitCodeFindBox.CaptionResourceString = officeOfEntryExitCaption;
				JE_OfficeOfEntryExitCodeFindBox.UpdateCaption();
			}

			var ciqOfficeCaption = Declaration.IsImport
				? Res.GetData("7C4FD92B-CB76-4407-846E-7273E2FC0CFC", "CIQ Office of Entry")
				: Res.GetData("D56B4988-734A-4829-B159-1756CB3D62DA", "CIQ Office of Exit");
			if (!ciqOfficeCaption.ContentEquals(JE_CIQOfficeOfEntryExitCodeFindBox.CaptionResourceString))
			{
				JE_CIQOfficeOfEntryExitCodeFindBox.CaptionResourceString = ciqOfficeCaption;
				JE_CIQOfficeOfEntryExitCodeFindBox.UpdateCaption();
			}

			BondedWarehouseDocAddressControl.Visible = JobDeclaration.BondedWarehouseEditable;

			ImporterDocAddressControl.UpdateControlsLayout();
			SupplierDocAddressControl.UpdateControlsLayout();
			ManufacturerDocAddressControl.UpdateControlsLayout();
			BuyerDocAddressControl.UpdateControlsLayout();

			ProcessTransportModeInlandControl();
		}

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible => (base.JE_ContainerModeBoundDropDownEditVisible && !(Declaration.IsAir || Declaration.IsPost)) || Declaration.IsRoad || Declaration.IsRail;

		protected override bool IsJE_MasterBillForSeaBoundTextBoxVisible
		{
			get { return base.IsJE_MasterBillForSeaBoundTextBoxVisible || Declaration.IsPost || Declaration.IsRoad || Declaration.IsRail; }
		}

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			if (declaration is JobDeclaration cnDeclaration)
			{
				cnDeclaration.JE_ClearanceModeInfo.ValueChanged += XC_ClearanceModeInfo_ValueChanged;
				cnDeclaration.JE_TransitModeInfo.ValueChanged += XC_TransitModeInfo_ValueChanged;
				cnDeclaration.JE_TransportModeInlandInfo.ValueChanged += JE_TransportModeInlandInfo_ValueChanged;
				cnDeclaration.MessageSubTypeChanging += Declaration_MessageSubTypeChanging;
				cnDeclaration.OfficeOfDestinationInfo.ValueChanged += OfficeOfDestination_ValueChanged;
			}
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.UnHookControlVisibilityChangeEvents(declaration);
			if (declaration is JobDeclaration cnDeclaration)
			{
				cnDeclaration.JE_ClearanceModeInfo.ValueChanged -= XC_ClearanceModeInfo_ValueChanged;
				cnDeclaration.JE_TransitModeInfo.ValueChanged -= XC_TransitModeInfo_ValueChanged;
				cnDeclaration.JE_TransportModeInlandInfo.ValueChanged -= JE_TransportModeInlandInfo_ValueChanged;
				cnDeclaration.MessageSubTypeChanging -= Declaration_MessageSubTypeChanging;
				cnDeclaration.CustomsOffices.Cast<CustomsOffice>().ForEach(x =>
				{
					x.CY_DataInfo.ValueChanged -= OfficeOfDestination_ValueChanged;
				});
			}
		}

		protected override void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			base.JE_MessageTypeInfo_ValueChanged(sender, e);
			JE_MessageSubTypeBoundDropDownEdit.SelectItem(Declaration.JE_MessageSubType);
		}

		void Declaration_MessageSubTypeChanging(object sender, CancelEventArgs e)
		{
			if (Declaration?.AnyEntryHasBeenLodgedOrIsWaitingForResponse ?? false)
			{
				e.Cancel = Globals.Message.Show(Res.GetString("52977D33-1AB9-4441-85E4-EE7788AE9026", "All existing entries may be discarded by changing Declaration Type. Some of them have been sent to Customs or set Declaration Unified Number. Are you sure to continue?"), ResContinue, MessageBoxButtons.OKCancel, DialogResult.Cancel) != DialogResult.OK;
			}
		}

		void XC_ClearanceModeInfo_ValueChanged(object sender, EventArgs e)
		{
			var showCheckBox = twoStepDeclarationActive && (Declaration?.IsTwoStepDeclaration ?? false);
			JE_LicenseInvolvedCheckBox.Visible = showCheckBox;
			JE_InspectionInvolvedCheckBox.Visible = showCheckBox;
			JE_TaxInvolvedCheckBox.Visible = showCheckBox;
			ClearanceModeAndCheckBoxesDescriptionLabel.Visible = showCheckBox;

			var showFullValidation = Declaration?.FullValidationReadOnly ?? true;
			FullValidationCheckBox.Visible = !showFullValidation;
		}

		void XC_TransitModeInfo_ValueChanged(object sender, EventArgs e)
		{
			var inlandControlVisible = Declaration.IsCustomsTransit;
			JE_VesselInlandTextBox.Visible = inlandControlVisible;
			JE_VoyageInlandTextBox.Visible = inlandControlVisible;
			SetInlandControlProperties();
			ProcessTransportModeInlandControl();
		}

		void ProcessTransportModeInlandControl()
		{
			JE_TransportModeInlandDropEdit.Visible = Declaration.IsCustomsTransit;
		}

		void JE_TransportModeInlandInfo_ValueChanged(object sender, EventArgs e)
		{
			SetInlandControlProperties();
		}

		void SetInlandControlProperties()
		{
			if (Declaration.IsCustomsTransit)
			{
				if (Declaration.IsInlandCarNumberApplicable)
				{
					JE_VoyageInlandTextBox.Visible = false;
					JE_VesselInlandTextBox.CaptionResourceString = Res.GetData("699ECD12-B664-4BAB-A51C-0EBD29F38054", "Car Number (Inland)");
				}
				else
				{
					JE_VoyageInlandTextBox.Visible = true;
					JE_VesselInlandTextBox.CaptionResourceString = Res.GetData("99B75551-0477-499A-BAA3-DBE6D9896E21", "Vessel/Voyage (Inland)");
				}
			}
		}
	}
}
