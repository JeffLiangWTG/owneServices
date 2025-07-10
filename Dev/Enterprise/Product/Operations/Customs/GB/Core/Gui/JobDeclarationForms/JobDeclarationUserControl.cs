using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class JobDeclarationUserControl : EU.GUI.EUJobDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			OwnersReferenceTextBox.CaptionResourceString = null;
			TotalNoOfPacksCalcDropEdit.CaptionResourceString = null;
			PortOfLoadingFindBox.CaptionResourceString = null;
			IncoTermDropEdit.CaptionResourceString = null;
			GoodsLocationDropEdit.CaptionResourceString = null;
			JE_ShipmentIncoTermPlaceTextBox.CaptionResourceString = null;
			ExportDeclarationNumberBoundTextBox.CaptionResourceString = null;
			JE_IATALoadPortCodeFindBox.CaptionResourceString = null;
			PortOfDischargeFindBox.CaptionResourceString = null;
			JE_ExportDateBoundDateEdit.CaptionResourceString = null;
			StatusTextBox.CaptionResourceString = null;

			CustomsOfficesUserControl.AllowOverlap(ShipmentDetailsGroupBox);
			UseClientEoriForDucrCheckBox.AllowOutsideOfParent();
		}

		protected override Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!MessageResponseSemaphoreTimer.Enabled && string.IsNullOrEmpty(dataMember) && dataSource is JobDeclaration jobDeclaration &&
				MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(jobDeclaration, out var semaphoreInfo))
			{
				if (!HandleMessageResponseTimeout(semaphoreInfo, out var _))
				{
					Globals.Message.Show($"This declaration was locked at {semaphoreInfo.CreateTimeUtc} (UTC) to wait for a response from customs");
					StartWaitForMessageResponseSemaphore(this);
				}
			}
		}

		public void StartWaitForMessageResponseSemaphore(System.Windows.Forms.Control formToReload)
		{
			formToReloadAfterMessageResponse = formToReload as ZForm ?? formToReload.GetParent<ZForm>();
			if (formToReloadAfterMessageResponse != null)
			{
				formToReloadAfterMessageResponse.SetReadOnlyIncludingChildren(removeReadonlyBinding: true, new List<string> { "CancelButton" });
				MessageResponseSemaphoreTimer.Enabled = true;
			}
			else
			{
				Globals.Message.Show("The parent form could not be determined, which means it will not be possible to unlock this declaration automatically. Please close and reopen this form to try again.");
			}
		}

		void MessageResponseSemaphoreTimer_Tick(object sender, EventArgs e)
		{
			var reload = false;
			string message = null;
			if (!MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration((JobDeclaration)JobDeclaration, out var semaphoreInfo))
			{
				reload = true;
				message = "Response message has been received";
			}
			else if (HandleMessageResponseTimeout(semaphoreInfo, out message))
			{
				reload = true;
			}
			if (reload)
			{
				MessageResponseSemaphoreTimer.Enabled = false;
				Globals.Message.Show(message);
				ReloadedForm = formToReloadAfterMessageResponse?.ReloadForm();
			}
		}

		protected ZForm ReloadedForm { get; private set; }
		ZForm formToReloadAfterMessageResponse;

		bool HandleMessageResponseTimeout(ISemaphoreInfo semaphoreInfo, out string message)
		{
			if (semaphoreInfo.CreateTimeUtc < ZDateTime.UtcNow.AddMinutes(-30))
			{
				message = "Wait for customs response has timed out after 30 minutes";
				MessageResponseSemaphoreHelper.RemoveSemaphoreForDeclaration((JobDeclaration)JobDeclaration);
				return true;
			}
			message = null;
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (JobDeclaration != null)
				{
					((JobDeclaration)JobDeclaration).OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DeclarationTypeDropEdit.PreBoundMaxLength = DeclarationTypeMaxLength;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			var declaration = (JobDeclaration)JobDeclaration;

			SpecificCircumstanceDropEdit.Visible = declaration.IsExport;
			IrcDropDown.Visible = declaration.IsInventoryControlledImport;
			StyleOfEntrySOEDropDown.Visible = !IrcDropDown.Visible;
			Box18TransportIDTextBox.Visible = GetBox18TransportIDTextBoxVisibility();
			Box18TrNationalityFindBox.Visible = false;
			DepartureTransportIDTextBox.Visible = GetBox21TransportInfoVisibility();
			HouseSplitReferenceTextBox.Visible = declaration.IsInventoryControlledAirImport;
			BadgeCodeDropEdit.Visible = true;
			GoodsLocationDropEdit.Visible = false;
			SubLocationDropEdit.Visible = false;
			EntrySubStyleDropEdit.Visible = false;
			LocationOfGoodsUserControl.Visible = true;
			DeclarationTypeDropEdit.Visible = false;
			BondedWarehouseDocAddressControl.Visible = false;

			declaration.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
			declaration.OnApplicationCodeChanged += Dec_OnAppCodeChanged;
			Dec_OnAppCodeChanged(declaration, null);

			if (!JE_RS_NKServiceLevelBoundFindBox.Visible)
			{
				this.CTStatusIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 148, true);
				this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 170, true);
			}
			else
			{
				this.CTStatusIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 170, true);
				this.JE_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 191, true);
			}

			if (declaration.IsAir)
			{
				this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 40, true);
				this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 16, true);
				this.OverrideValuesCheckBox.TabIndex = 1;
				this.InlandModeOfTransportDropEdit.TabIndex = 2;
				this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			}
			else if (declaration.IsSea)
			{
				this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 17, true);
				this.JE_MasterBillForSeaBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 16, true);
				this.JE_MasterBillForSeaBoundTextBox.TabIndex = 0;
				this.OverrideValuesCheckBox.TabIndex = 1;
				this.InlandModeOfTransportDropEdit.TabIndex = 2;
				this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
				this.VesselFindBox.TabIndex = 3;
				this.JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
				this.JE_VoyageFlightNoBoundTextBox.TabIndex = 4;
			}
			else
			{
				this.Box18TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 17, true);
				this.Box18TransportIDTextBox.TabIndex = 0;
				this.OverrideValuesCheckBox.TabIndex = 1;
				this.Box18TrNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 17, true);
				this.Box18TrNationalityFindBox.TabIndex = 2;
				this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 40, true);
				this.InlandModeOfTransportDropEdit.TabIndex = 3;
			}
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 157, true);
			this.ShipmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 270, true);

			IsHighValueOvrdCheckBox.Visible = declaration.IsImport && declaration.DV1DetailsSupport;
		}

		protected override Type GetOrganizationImportUserControlType() => typeof(ImportOrganizationUserControl);

		protected override Type GetOrganizationExportUserControlType() => typeof(ExportOrganizationUserControl);

		void Dec_OnAppCodeChanged(object sender, EventArgs e)
		{
			var jobDeclaration = sender as JobDeclaration;

			if (jobDeclaration != null)
			{
				Box18TransportIDTextBox.Visible = GetBox18TransportIDTextBoxVisibility();
				DepartureTransportIDTextBox.Visible = GetBox21TransportInfoVisibility();

				this.RefreshControlCaptions();
				DepartureTransportIDTextBox.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.DepartureTransportIDCaption;
				BondedWarehouseDocAddressControl.CaptionResourceString = jobDeclaration.CaptionResourceStringForProperty(nameof(jobDeclaration.WarehouseDocAddress));
				ImporterDocAddress.CaptionResourceString = jobDeclaration.CaptionResourceStringForProperty(nameof(jobDeclaration.ImporterDocumentaryAddress));
				SupplierDocAddress.CaptionResourceString = jobDeclaration.CaptionResourceStringForProperty(nameof(jobDeclaration.SupplierDocumentaryAddress));
				((ImportOrganizationUserControl)OrganizationImportUserControl.HostedControl)?.SetCaptions();
				((ExportOrganizationUserControl)OrganizationExportUserControl.HostedControl)?.SetCaptions();

				this.JE_MessageSubTypeBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 57, true);
				this.JE_MessageSubTypeBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
				this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 113, true);
				this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
				this.TotalNoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 139, true);
				this.TotalNoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
				this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 161, true);
				this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
				this.JE_ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 184, true);
				this.JE_ShipmentIncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
				this.ICSDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 67, true);
				this.ICSDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
				this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 40, true);
				this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 64, true);
				this.GoodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 40, true);
				this.GoodsDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 64, true);
				if (((JobDeclaration)JobDeclaration).JE_TransportMode == Enterprise.Customs.Business.TransportTypeList.Codes.Air || ((JobDeclaration)JobDeclaration).JE_TransportMode == Enterprise.Customs.Business.TransportTypeList.Codes.Sea)
				{
					this.TransportNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 64, true);
				}
				else
				{
					this.TransportNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 64, true);
				}

				RefreshDocAddressCaptions();
			}
		}

		void RefreshDocAddressCaptions()
		{
			ImporterDocAddress.SetLabelCaptionVisible(false);
			ImporterDocAddress.SetLabelCaptionVisible(true);
			SupplierDocAddress.SetLabelCaptionVisible(false);
			SupplierDocAddress.SetLabelCaptionVisible(true);
		}

		void GoodsLocationDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			SubLocationDropEdit.Refresh();
		}

		bool GetBox18TransportIDTextBoxVisibility()
		{
			var declaration = (JobDeclaration)JobDeclaration;
			var transportMode = declaration.JE_TransportMode;
			return !codesUcc.Contains(transportMode);
		}

		readonly HashSet<string> codesUcc = new HashSet<string> { Customs.Business.TransportTypeList.Codes.Air, Customs.Business.TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Codes.FixedTransportInstallations, Customs.Business.TransportTypeList.Codes.Mail };

		bool GetBox21TransportInfoVisibility() => false;

		protected override int DeclarationTypeMaxLength => 3;
	}
}
