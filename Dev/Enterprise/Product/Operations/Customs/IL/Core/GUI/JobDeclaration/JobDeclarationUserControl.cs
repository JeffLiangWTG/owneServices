using System;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
			SupplierOrganisationControl.OrgAddressFormatter = (factory, orgAddress) => new ILSupplierAddressFormatter(factory, orgAddress);
			ImporterOrganisationControl.OrgAddressFormatter = (factory, orgAddress) => new ILImporterAddressFormatter(factory, orgAddress);
		}

		protected override void SetFolioNumberVisible()
		{
			FolioNumberTextBox.Visible = false;
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			SetTransportDetailsLayout();
			VesselFindBox.Visible = JobDeclaration != null && (JobDeclaration.IsSea || JobDeclaration.IsRoad);
		}

		protected override void SetContainerCountAndNoOfPieces()
		{
			JE_ContainerCountCalcEdit.Visible = false;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
		}

		protected override void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTransportDetailsLayout();
		}

		void SetTransportDetailsLayout()
		{
			if (JobDeclaration.TransportMode == Core.Constants.TransportModes.Air)
			{
				JE_ManifestNumberTextBox.Visible = false;
				JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40);
				PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64);
				PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88);
				JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 64);
				JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 88);
			}
			else
			{
				JE_ManifestNumberTextBox.Visible = true;
				JE_VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88);
				PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 112);
				PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 136);
				JE_ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 112);
				JE_DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 136);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ScreenButton.Visible = false;
			ScreeningStatusDropEdit.Visible = !((IComplianceItemRiskStatusProvider)JobDeclaration).IsEnabledComplianceWise;
		}

		protected override bool IsJE_MasterBillForSeaBoundTextBoxVisible => JobDeclaration != null && ((bool)JobDeclaration.IsSea || (bool)JobDeclaration.IsRoad);
	}
}
