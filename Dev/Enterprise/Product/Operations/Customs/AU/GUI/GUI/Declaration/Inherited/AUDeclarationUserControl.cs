using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUDeclarationUserControl /*Base control specifies the DataSourceTypeName*/ : BaseCustomsDeclarationUserControl
	{
		public AUDeclarationUserControl()
		{
			InitializeComponent();

			custShipNoTextBox.AllowOverlap(VesselFindBox);
			jE_ToOrderCheckBox.AllowOverlap(ImporterOrganisationControl);
		}

		#region Expose Controls For Test

		public ZTextBox JE_HouseBillParcelPostTextEdit
		{
			get { return HouseBillParcelPostTextEdit; }
		}

		public ZTabControl TabControlOnDeclarationControl
		{
			get { return RightTabControl; }
		}

		#endregion

		#region Control Visibility

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisplayConsolidatedDeclarationAdviceIfNeeded();
		}

		#region SetConsolidatedDeclarationAdviceIfNeeded

		void DisplayConsolidatedDeclarationAdviceIfNeeded()
		{
			ConsolidatedDeclarationAdviceLabel.Visible = false;

			if (ConsolidatedEntriesEnabled && JobDeclaration != null && ConsolidatedDeclaration.IsConsolidated(JobDeclaration))
			{
				declarationMessageAndWHSTransactionStatusPanel.Size = ControlDpiScalingHelper.NewScaledSize(740, 20, true);
				ExportDeclarationNumberBoundTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(100, 25, true);
				StatusTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(358, 25, true);
				ConsolidatedDeclarationAdviceLabel.Visible = true;
			}
		}

		bool ConsolidatedEntriesEnabled => RawDataRegistry.Instance.EnableConsolidatedEntries.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		#endregion

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		protected override void HookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			declaration.OnBondedWarehouseRelatedFieldChanged += OnBondedWarehouseRelatedFieldChanged;
		}

		protected override void UnHookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			declaration.OnBondedWarehouseRelatedFieldChanged -= OnBondedWarehouseRelatedFieldChanged;
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		void OnBondedWarehouseRelatedFieldChanged(object sender, EventArgs e)
		{
			var declaration = JobDeclaration;
			wHSTransactionStatusPanel.Visible = declaration != null && declaration.IsWHSUniversalXMLActive && declaration.IsImport && declaration.SupportsBondedWarehousing;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			SetHouseBillFields();
			SetMarksAndNumbersFields();
			SetUpExWarehouseControls();
			SetTransportDetails();//Grouped under a group box 'Transport Details'
			SetShipmentDetails();//Grouped under a group box 'Shipment Details'

			JE_ExportGoodsTypeBoundDropDownEdit.Visible = JobDeclaration.IsExport;
			messageStatusDescriptionTextBox.Visible = !JobDeclaration.IsEXPDeclaration;
			detailsButton.Visible = !JobDeclaration.IsEXPDeclaration;
			jE_ToOrderCheckBox.Visible = JobDeclaration.IsQuarantine;
			importerToOrderCityLabel.Visible = JobDeclaration.IsQuarantine && JobDeclaration.JE_ToOrder;
			importerToOrderCityTextBox.Visible = JobDeclaration.IsQuarantine && JobDeclaration.JE_ToOrder;
			cargoStatusGroupBox.Visible = JobDeclaration.JE_MessageType == JobMessageTypeList.Codes.Import;

			JE_TransportModeBoundDropDownEdit.Visible = !JobDeclaration.IsNonTransportDeclarationType;

			this.ScreeningStatusDropEdit.Visible = !JobDeclaration.IsDrawback && JobDeclaration.Shipment == null && !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;
			this.ScreenButton.Visible = !JobDeclaration.IsDrawback && JobDeclaration.Shipment == null && !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;
			this.JE_MessageSubTypeBoundDropDownEdit.Visible = !JobDeclaration.IsDrawback;
			this.ImporterOrganisationControl.Text = JobDeclaration.IsDrawback ? "Drawback Claimant" : "Importer";

			OnBondedWarehouseRelatedFieldChanged(this, EventArgs.Empty);

			tSS_SeparatorTextUserControl.Visible = deliveryDocAddressControl.Visible = !(JobDeclaration != null && JobDeclaration.IsExportOrNonTransport);
			deliveryDocAddressControl.Text = string.Empty;

			customsProcessingUserControl.Visible = JobDeclaration.IsQueuedEntryLodgementsEnabled;
		}

		protected override bool ContainerCountOrInnerPacksLabelVisible
		{
			get { return base.ContainerCountOrInnerPacksLabelVisible && !JobDeclaration.IsExWarehouse; }
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible
		{
			get { return base.JE_ContainerModeBoundDropDownEditVisible && !JobDeclaration.IsExWarehouse && !JobDeclaration.IsPost; }
		}

		void SetHouseBillFields()
		{
			if (JobDeclaration.IsTransportModeOther || (JobDeclaration.IsImportCMR && JobDeclaration.IsExWarehouse) || JobDeclaration.IsNonTransportDeclarationType)
			{
				HouseBillParcelPostTextEdit.Visible = false;
			}
			else
			{
				HouseBillParcelPostTextEdit.Visible = true;
			}
		}

		void SetMarksAndNumbersFields()
		{
			bool controlVisibility = JobDeclaration.IsImportCMR ? !JobDeclaration.IsExWarehouse && !JobDeclaration.IsTransportModeOther : (bool)JobDeclaration.IsExWarehouse || JobDeclaration.IsNonTransportDeclarationType;
			SetControlState(marksAndNumbersTextBox, controlVisibility);
			SetControlState(marksAndNumberButton, controlVisibility);
		}

		void SetShipmentDetails()
		{
			bool isSAC = JobDeclaration.IsSAC; //JobDeclaration.IsSACWithoutLines || JobDeclaration.IsSACWithLines;
			bool notExWarehouseActiveAndNotNonTransportDeclarationType = !JobDeclaration.IsExWarehouse && !JobDeclaration.IsNonTransportDeclarationType;
			bool notNonTransportDeclarationType = !JobDeclaration.IsNonTransportDeclarationType;

			OriginFindBox.Visible = !isSAC && notExWarehouseActiveAndNotNonTransportDeclarationType;

			JE_ExportDateBoundDateEdit2.Visible = !isSAC && notExWarehouseActiveAndNotNonTransportDeclarationType;

			FinalDestinationFindBox.Visible = !isSAC && notNonTransportDeclarationType;

			JE_DateOfArrivalBoundDateEdit2.Visible = !isSAC && notExWarehouseActiveAndNotNonTransportDeclarationType;

			WeightzCalcDropEdit.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;

			VolumeCalcDropEdit.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;

			JE_TotalNoOfPiecesBoundCalcEdit.Visible = JobDeclaration.IsImport && !isSAC && notExWarehouseActiveAndNotNonTransportDeclarationType;

			IncoTermDropEdit.Visible = !isSAC && notExWarehouseActiveAndNotNonTransportDeclarationType;
			IncoTermExplainButton.Visible = !isSAC && notExWarehouseActiveAndNotNonTransportDeclarationType;

			TotalNoOfPacksCalcDropEdit.Visible = notNonTransportDeclarationType;

			drawbackAssessmentMethodDropEdit.Size = HouseBillParcelPostTextEdit.Size;
			drawbackAssessmentMethodDropEdit.Location = OriginFindBox.Location;
			drawbackAssessmentMethodDropEdit.TabIndex = OriginFindBox.TabIndex;
			drawbackAssessmentMethodDropEdit.Visible = !notNonTransportDeclarationType;

			drawbackEDNControl.Location = FinalDestinationFindBox.Location;
			drawbackEDNControl.TabIndex = FinalDestinationFindBox.TabIndex;
			drawbackEDNControl.Visible = !notNonTransportDeclarationType;

			PartShipConsignRefTextBox.Visible = JobDeclaration.IsPartShipConsignmentReferenceRelevant && isSAC;
			if (marksAndNumbersTextBox.Visible)
			{
				PartShipConsignRefAlt1TextBox.Visible = false;
				PartShipConsignRefAlt2TextBox.Visible = JobDeclaration.IsConsignmentReferenceActive && !isSAC;
			}
			else
			{
				PartShipConsignRefAlt1TextBox.Visible = JobDeclaration.IsConsignmentReferenceActive && !isSAC;
				PartShipConsignRefAlt2TextBox.Visible = false;
			}
		}

		#region ExWarehouseControls

		void SetTransportDetails()
		{
			if (JobDeclaration.IsNonTransportDeclarationType)
			{
				TransportDetailsGroupBox.Visible = false;
			}
			else
			{
				TransportDetailsGroupBox.Visible = true;
				bool isSAC = JobDeclaration.IsSAC;
				bool isExWarehouse = JobDeclaration.IsExWarehouse;
				bool isExport = JobDeclaration.IsExport;
				bool isQuarantine = JobDeclaration.IsQuarantine;

				custShipNoOverrideCheckBox.Visible = !isExWarehouse && JobDeclaration.IsSea;
				custShipNoTextBox.Visible = !isExWarehouse && JobDeclaration.IsSea && JobDeclaration.ZA_CustShipNoOverride_Hidden;

				JE_VoyageFlightNoBoundTextBox.Visible = !isExWarehouse && (JobDeclaration.IsSea || JobDeclaration.IsAir);
				FolioNumberTextBox.Visible = JobDeclaration.IsAir && JE_VoyageFlightNoBoundTextBox.Visible;

				JE_PortOfFirstArrivalCodeFindBox.Visible = (!isSAC && !isExWarehouse && !isExport) || isQuarantine;
				DateOfFirstArrivalDateEdit.Visible = (!isSAC && !isExWarehouse && !isExport) || isQuarantine;

				PortOfDischargeFindBox.GetExtension<ILabelCaptionRenderer>().Caption = isExport && !isQuarantine ? "First Discharge" : "Discharge";
			}
		}

		void SetUpExWarehouseControls()
		{
			bool exWarehouseActiveOrNonTransportDeclarationType = JobDeclaration.IsExWarehouse || JobDeclaration.IsNonTransportDeclarationType;

			SetControlState(SupplierOrganisationControl, exWarehouseActiveOrNonTransportDeclarationType);
			SetControlState(VesselFindBox, exWarehouseActiveOrNonTransportDeclarationType || !JobDeclaration.IsSea);
			SetControlState(PortOfLoadingFindBox, exWarehouseActiveOrNonTransportDeclarationType);
			SetControlState(JE_ExportDateBoundDateEdit, exWarehouseActiveOrNonTransportDeclarationType);
			SetControlState(PortOfDischargeFindBox, exWarehouseActiveOrNonTransportDeclarationType);
			SetControlState(JE_DateOfArrivalBoundDateEdit, exWarehouseActiveOrNonTransportDeclarationType);
		}

		#endregion

		#region Set Control State Methods

		void SetControlState(ZButton control, bool exWarehouseActive)
		{
			control.Visible = !exWarehouseActive;
		}

		void SetControlState(ZTextBox control, bool exWarehouseActive)
		{
			control.Visible = !exWarehouseActive;
		}

		void SetControlState(UserControl control, bool exWarehouseActive)
		{
			control.Visible = !exWarehouseActive;
		}

		#endregion

		#endregion

		void MarksAndNumberButton_NoteHasChangesChanged()
		{
			JobDeclaration.JE_MarksAndNumbersShortInfo.RefreshBinding();
		}

		void DetailsButton_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(JobDeclaration.MessageStatusExtraDetails);
		}

		void StatusDetailsButton_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(JobDeclaration.CombinedConsolidatedCargoStatusDetails);
		}

		void MessageStatusDescriptionTextBox_TextChanged(object sender, EventArgs e)
		{
			if (JobDeclaration != null)
			{
				var backgroundColor = JobDeclaration.HasOutstandingAmendment || JobDeclaration.HasConsolidatedEntryChanges ? Color.LightSalmon : SystemColors.Control;
				messageStatusDescriptionTextBox.ColorChanger.ForceBackColor(backgroundColor);
			}
		}

		void ConsolidatedCargoStatusTextBox_TextChanged(object sender, EventArgs e)
		{
			if (JobDeclaration != null)
			{
				if (JobDeclaration.isCargoStatusAvailableAndCargoClear)
				{
					consolidatedCargoStatusTextBox.ColorChanger.ForceBackColor(Color.LightGreen);
				}
				else if (JobDeclaration.isCargoStatusAvailableAndCargoNotClear)
				{
					consolidatedCargoStatusTextBox.ColorChanger.ForceBackColor(Color.LightSalmon);
				}
				else
				{
					consolidatedCargoStatusTextBox.ColorChanger.ForceBackColor(SystemColors.Control);
				}
			}
		}

		protected override void SetRightTabControlSelectTab()
		{
			RightTabControl.SelectedTab = OrganisationsTabPage;
		}
	}
}
