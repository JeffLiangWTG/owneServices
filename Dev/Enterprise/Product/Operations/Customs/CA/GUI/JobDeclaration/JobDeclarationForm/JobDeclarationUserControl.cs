using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAJobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public CAJobDeclarationUserControl()
		{
			InitializeComponent();
			ImporterDocumentaryAddress.Visible = CACustomsDataRegistry.Instance.MakeSomeFieldsJobDocAddress.Value;
			JE_ApplicationCodeBoundDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(69, 122);
			JE_ApplicationCodeBoundDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(176, 20);
		}

		#region Implementation

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem is JobDeclaration declaration)
			{
				JobDeclaration.JE_TransportModeInfo.ValueChanged += JE_TransportMode_ValueChanged;
			}

			JE_TransportMode_ValueChanged(this, null);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (JobDeclaration is JobDeclaration declaration)
			{
				JobDeclaration.JE_TransportModeInfo.ValueChanged -= JE_TransportMode_ValueChanged;
			}
		}

		protected override void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			base.JE_MessageTypeInfo_ValueChanged(sender, e);
			var isExport = JobDeclaration != null && JobDeclaration.IsExport;
			var isDisplayed = CACustomsDataRegistry.Instance.DisplayCargoControlNumberSeparately.Value;
			groupBoxManualSubmissionInfo.Visible = !isExport;
			CargoControlNumberNoBoundTextBox.Visible = !isExport && !isDisplayed;
			CargoControlNumberPrefixNoBoundTextBox.Visible = !isExport && isDisplayed;
			CargoControlNumberSuffixNoBoundTextBox.Visible = !isExport && isDisplayed;
			DefaultHWBButton.Visible = !isExport && JobDeclaration != null && !JobDeclaration.IsAir;
		}

		void JE_TransportMode_ValueChanged(object sender, EventArgs e)
		{
			DefaultHWBButton.Visible = JobDeclaration != null && !JobDeclaration.IsExport && !JobDeclaration.IsAir;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			JobDeclaration.JE_OH_ImporterInfo.ValueChanged += JE_OH_ImporterInfo_ValueChanged;
			JobDeclaration.ImporterOfRecordAddress.OrganisationPKIncludesMiscOrgInfo.ValueChanged += ImporterOfRecordAddress_OrganisationChanged;
			if (!JobDeclaration.IsInDatabase && (!JobDeclaration.JE_OH_Importer.IsEmpty || JobDeclaration.ImporterOfRecordAddress.HasRealOrganisation))
			{
				ImporterOfRecordAddress_OrganisationChanged(this, null);
				if (JobDeclaration.DisplaySequentialOfTransactionNumberSeparately)
				{
					JobDeclaration.TransactionNumber.SetAccountSecurityNo();
				}
			}
			JobDeclaration.JE_EntryAuthorisationDateInfo.ValueChanged += JE_EntryAuthorisationDateInfo_ValueChanged;
			JobDeclaration.CA_DeclarationExceptionInfo.ValueChanged += CA_DeclarationExceptionInfo_ValueChanged;
			CargoControlNumberNoBoundTextBox.ReadOnlyChanged += CargoControlNumberNoBoundTextBox_ReadOnlyChanged;
			CargoControlNumberSuffixNoBoundTextBox.ReadOnlyChanged += CargoControlNumberNoBoundTextBox_ReadOnlyChanged;
			CargoControlNumberNoBoundTextBox_ReadOnlyChanged(null, null);
			JobDeclaration.SuppressShipmentRelatedFieldsInfo.ValueChanged += SuppressShipmentRelatedFieldsInfo_ValueChanged;
			JE_TransportMode_ValueChanged(this, null);
		}

		void JE_OH_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.TransactionNumber.CanChange && !JobDeclaration.ImporterOfRecordAddress.HasRealOrganisation
				&& !(JobDeclaration.IsInDatabase && JobDeclaration.HasWHSTransaction))
			{
				var importerAddInfo = JobDeclaration.ImporterAddInfo;
				if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
				{
					JobDeclaration.CA_UseImporterAccountSecurityNumber = CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.Value ||
						Globals.Message.Show(
						Res.GetString("89185DC5-F471-4C0B-9503-B01EE0700669", "Do you want to use the Importer's Account Security Number instead of yours?"),
						Res.GetString("892FE048-2AC0-4FDD-9947-160E81FCDA18", "Importer Account Security Number"),
						MessageBoxButtons.YesNo,
						DialogResult.Yes) == DialogResult.Yes;
				}
			}
		}

		void CA_DeclarationExceptionInfo_ValueChanged(object sender, EventArgs e)
		{
			ExceptionDescriptionLabel.Visible = JobDeclaration != null && !JobDeclaration.CA_DeclarationException.IsEmpty;
		}

		void ImporterOfRecordAddress_OrganisationChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.TransactionNumber.CanChange && !(JobDeclaration.IsInDatabase && JobDeclaration.HasWHSTransaction))
			{
				if (JobDeclaration.ImporterOfRecordAddress.HasRealOrganisation)
				{
					var importerAddInfo = JobDeclaration.ImporterOfRecordAddInfo;
					if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
					{
						JobDeclaration.CA_UseImporterAccountSecurityNumber = CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.Value ||
							Globals.Message.Show(
							Res.GetString("5F6DE8A0-DCF9-456C-AEA2-F8927EE3241B", "Do you want to use the Importer of Record's Account Security Number instead of yours?"),
							Res.GetString("892FE048-2AC0-4FDD-9947-160E81FCDA18", "Importer Account Security Number"),
							MessageBoxButtons.YesNo,
							DialogResult.Yes) == DialogResult.Yes;
					}
				}
				else
				{
					JE_OH_ImporterInfo_ValueChanged(this, null);
				}
			}
		}

		void JE_EntryAuthorisationDateInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.JE_EntryAuthorisationDate.IsEmpty)
			{
				Globals.Message.ShowWarning(Res.GetString("d858ecd6-9d26-4bc6-8456-97b84ae743af", "You have cleared the Actual Release Date. As a result any automatic Entry will not be sent, late Entry warnings will not occur, and the Age in Days will show as zero. If the job has actually been released then this may result in a penalty for not reporting the Entry."));
			}
		}

		void SuppressShipmentRelatedFieldsInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = JobDeclaration;
			var showShipmentRelatedFields = declaration.ShowShipmentRelatedFields;
			var showShipmentRelatedFieldsOrSea = declaration.ShowShipmentRelatedFieldsOrSea;

			PortOfLoadingFindBox.Visible = showShipmentRelatedFields;
			JE_ExportDateBoundDateEdit.Visible = showShipmentRelatedFields;

			PortOfDischargeFindBox.Visible = showShipmentRelatedFieldsOrSea;
			JE_DateOfArrivalBoundDateEdit.Visible = showShipmentRelatedFieldsOrSea;
			PortOfUnladingCodeFindBox.Visible = showShipmentRelatedFieldsOrSea;
			WarehouseReleaseDateDateEdit.Visible = showShipmentRelatedFieldsOrSea;

			OriginFindBox.Visible = showShipmentRelatedFields;
			JE_ExportDateBoundDateEdit2.Visible = showShipmentRelatedFields;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = JobDeclaration.IsImport && !JobDeclaration.SuppressShipmentRelatedFields;
			VolumeCalcDropEdit.Visible = showShipmentRelatedFields;
			NetWeightCalcDropEdit.Visible = showShipmentRelatedFields;
			ScreeningStatusDropEdit.Visible = showShipmentRelatedFields && JobDeclaration.Shipment == null && !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;
			ScreenButton.Visible = showShipmentRelatedFields && JobDeclaration.Shipment == null && !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;
			JE_DateOfArrivalBoundDateEdit.AutoValidate = AutoValidate.Disable;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			this.SuspendDrawing();

			ChangeControlsVisibility();
			ChangeB3ScheduleControlVisibility();
			ChangeTransactionNumberControlVisibility();
			this.ResumeDrawing();
		}

		void DefaultHWBButton_Click(object sender, EventArgs e)
		{
			JobDeclaration?.DefaultHouseBillFromEffectiveCCN();
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible
		{
			get
			{
				return base.JE_ContainerModeBoundDropDownEditVisible
					|| (JobDeclaration != null
					&& (JobDeclaration.IsRoad || JobDeclaration.IsRail)
					&& !JobDeclaration.IsNonTransportDeclarationType
					&& !JobDeclaration.IsLVS);
			}
		}

		void ChangeB3ScheduleControlVisibility()
		{
			var declaration = JobDeclaration;
			var scheduledB3SendingDate = declaration.ScheduledB3SendingDate;
			var shouldDisplayB3ScheduleControl = scheduledB3SendingDate.IsValid;
			ScheduledB3DateEdit.Visible = shouldDisplayB3ScheduleControl;
			if (!shouldDisplayB3ScheduleControl)
			{
				this.B3EntryStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 17, true);
			}
			else
			{
				var isCADEnabled = declaration.IsCADEnabled;
				this.B3EntryStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
				if (!declaration.HasScheduledB3Message || declaration.ScheduledB3AutoSendingDate == scheduledB3SendingDate)
				{
					if (isCADEnabled)
					{
						ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bc349293-5c9e-42d9-ba89-106f5c53d758", "CAD Auto-Send Time", "Scheduled Auto-Sending Time For CAD Message", "");
					}
					else
					{
						ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("50f9674f-ae5b-47ec-b944-9e83aaae81d3", "Entry Auto-Send Time", "Scheduled Auto-Sending Time For Entry Message", "");
					}
					ScheduledB3DateEdit.DateTextBox.ColorChanger.ForceBackColor(SystemColors.Control);
				}
				else
				{
					if (isCADEnabled)
					{
						ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4604ddf-1c12-4acc-80c2-f81300c3230a", "CAD Scheduled Time", "Scheduled Sending Time For CAD Message", "");
					}
					else
					{
						ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("555E907B-BC8E-4834-A112-AA6AC9D6153E", "Entry Scheduled Time", "Scheduled Sending Time For Entry Message", "");
					}
					ScheduledB3DateEdit.DateTextBox.ColorChanger.ForceBackColor(Color.Yellow);
				}
			}
		}

		void ChangeTransactionNumberControlVisibility()
		{
			if (JobDeclaration != null && JobDeclaration.IsImportIncludingB2)
			{
				var isFormattedControlVisible = !JobDeclaration.DisplaySequentialOfTransactionNumberSeparately;
				FormattedTransactionNumberTextBox.Visible = isFormattedControlVisible;
				SecurityCodeTextBox.Visible = !isFormattedControlVisible;
				SequentialNumberTextBox.Visible = !isFormattedControlVisible;
				CheckDigitTextBox.Visible = !isFormattedControlVisible;
			}
		}

		void CargoControlNumberNoBoundTextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			DefaultHWBButton.Visible = JobDeclaration != null && JobDeclaration.JE_JS.IsEmpty && !JobDeclaration.MultipleCCN && !JobDeclaration.IsAir;
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var isExport = JobDeclaration != null && JobDeclaration.IsExport;
			var isDLMExport = JobDeclaration != null && JobDeclaration.IsDataLoadingModule;
			var isG7Export = JobDeclaration != null && JobDeclaration.IsExport && JobDeclaration.IsG7ExportDeclaration;
			var isImport = JobDeclaration != null && JobDeclaration.IsImport;
			var isLVS = JobDeclaration != null && JobDeclaration.IsLVS;
			var isIM2 = JobDeclaration != null && JobDeclaration.IsIM2;
			var isCAD = JobDeclaration?.IsCADEnabled ?? false;

			ExportPortPanel.Visible = isExport;
			ImportPortPanel.Visible = isImport;

			SupplierOrganisationControl.Visible = !isImport && !isExport;
			SupplierDocumentAddress.Visible = isImport;
			ImportExtraDetailsZPanel.Visible = isImport;
			ControlDpiScalingHelper.SetHeight(TransportDetailsGroupBox, isImport ? 395 : 270, true);
			ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, isImport ? 470 : 342, true);
			ExportExtraDetailPanel.Visible = isExport;
			DeliveryInstructionsLongTextControl.Location = ControlDpiScalingHelper.NewScaledPoint(113, isImport ? 366 : 243);

			ShipmentDetailsGroupBox.Size = isImport ? ControlDpiScalingHelper.NewScaledSize(456, 176) : CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height) - 350);
			ShipmentDetailsGroupBox.Anchor =
				isImport ? AnchorStyles.Top | AnchorStyles.Left
					: AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

			JE_ExportDateBoundDateEdit.DateTimeFormat = isG7Export ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short;

			if (isG7Export)
			{
				B3EntryStatusDescriptionTextBox.CaptionResourceString = Res.GetData("32cbf4d2-e257-44de-8e1d-7dc0cd334c3d", "G7 Status");
			}
			else if (isCAD)
			{
				B3EntryStatusDescriptionTextBox.CaptionResourceString = Res.GetData("D6BF0456-938F-4838-B0D9-93A87A0C4A8D", "CAD Status");
				B3EntryStatusDescriptionTextBox.CharacterCasing = CharacterCasing.Normal;
				B3CSubmittedDate.CaptionResourceString = Res.GetData("C7F7090E-F195-4419-87E6-897EE5DEC414", "CAD Submitted Time");
			}
			else
			{
				B3EntryStatusDescriptionTextBox.CaptionResourceString = Res.GetData("94f70add-093f-41a4-8656-c4b3379823e2", "Entry Status");
			}

			PermitsGroupBox.Visible = isExport;
			EDIReleaseOptionsGroupBox.Visible = isImport;

			SupplierOrganisationControl.Text =
				isExport ? Res.GetString("8da969aa-d5c4-4f6e-ad57-b4f4ec69babd", "Exporter")
					: Res.GetString("24e60521-3d52-486b-9b99-f316d928ec02", "Main Vendor");

			ImporterOrganisationControl.Text =
				isExport ? Res.GetString("d73be376-0331-4010-9a81-97d7c725480c", "Consignee")
					: Res.GetString("ac6e70c3-9e36-4044-a998-9307d00d645b", "Importer");

			ExportDeclarationNumberBoundTextBox.Visible = !isImport;
			ExportDeclarationNumberBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption =
				isDLMExport ? Res.GetString("492ac03c-64fc-458f-8f29-7368896b7352", "Form Key")
					: Res.GetString("f8cc55b9-7fce-40f1-a45f-1ed6a0069ff7", "Transaction #");

			TransactionNumberPanel.Visible = isImport;
			LowValueShipmentLabel.Visible = isImport && JobDeclaration.IsLowValueNormalReleaseJob;
			StatusTextBox.Visible = !isExport;

			PortOfFirstArrivalCodeFindBox.Visible = isImport;
			DateOfFirstArrivalDateEdit.Visible = isImport;

			JE_TransportModeBoundDropDownEdit.Visible = !isLVS;
			JE_MessageSubTypeBoundDropDownEdit.Visible = isImport;
			JE_MessageSubTypeBoundDropDownEdit.Location = isLVS ? JE_TransportModeBoundDropDownEdit.Location : CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 79);
			JE_MessageSubTypeBoundDropDownEdit.CaptionResourceString =
				isLVS ? Res.GetData("15eeed8e-09e1-4c20-9357-f2373d38ac57", "LVS Type")
					: Res.GetData("CAJobDeclarationUserControl|ced4cc7b-729f-4158-8859-96ddd686ef4f", "Entry T", "Entry Type", "");
			ImporterOrganisationControl.Visible = !isExport;
			ImporterOrgAddressControl.Visible = isExport;
			SupplierOrgAddressControl.Visible = isExport;

			EnableACROSSCheckBox.Visible = !isExport && !isIM2;
			EnableB3CheckBox.Visible = !isExport;

			ResetOrganizationTab(isImport, isExport);
			CA_DeclarationExceptionInfo_ValueChanged(this, null);
			SuppressShipmentRelatedFieldsInfo_ValueChanged(this, null);
		}

		void ResetOrganizationTab(bool isImport, bool isExport)
		{
			ForwarderOrganisationControl.GetExtension<ILabelCaptionRenderer>().Caption =
				isExport ? Res.GetString("79df8e75-f2f2-4c27-84b1-3c5fc22ccb0f", "Service Provider")
					: Res.GetString("3983754a-020c-4e1b-9590-7bea0aad2566", "Forwarder");

			ImporterOfRecordDocAddressControl.Visible = isImport;
			VendorAddressControl.Visible = isExport;
			CommercialInvoiceOriginatorDocAddressControl.Visible = isImport;
		}

		protected override bool IsVoyageFlightNoVisible
		{
			get { return JobDeclaration.Lookups.TransportTypeList.ContainsCode(JobDeclaration.JE_TransportMode); }
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (JobDeclaration is JobDeclaration declaration)
				{
					declaration.JE_OH_ImporterInfo.ValueChanged -= JE_OH_ImporterInfo_ValueChanged;
					declaration.ImporterOfRecordAddress.OrganisationPKIncludesMiscOrgInfo.ValueChanged -= ImporterOfRecordAddress_OrganisationChanged;
					declaration.JE_TransportModeInfo.ValueChanged -= JE_TransportMode_ValueChanged;
					DefaultHWBButton.Click -= DefaultHWBButton_Click;
					declaration.JE_EntryAuthorisationDateInfo.ValueChanged -= JE_EntryAuthorisationDateInfo_ValueChanged;
					declaration.CA_DeclarationExceptionInfo.ValueChanged -= CA_DeclarationExceptionInfo_ValueChanged;
					declaration.SuppressShipmentRelatedFieldsInfo.ValueChanged -= SuppressShipmentRelatedFieldsInfo_ValueChanged;
				}
				ImporterOfRecordDocAddressControl.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion Implementation
	}
}
