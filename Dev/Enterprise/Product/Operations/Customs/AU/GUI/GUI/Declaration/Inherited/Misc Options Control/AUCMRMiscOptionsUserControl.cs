using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCMRMiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public AUCMRMiscOptionsUserControl()
		{
			InitializeComponent();
			UpdateComponentProperty();
		}

		void UpdateComponentProperty()
		{
			var premisesIdColumn = this.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
			premisesIdColumn.ModuleID = CMRReferenceDataHelper.UseReferenceData ? Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList : Enterprise.ZArchitecture.Modules.ModuleIDs.Premises;
		}

		bool ImplementUPE
		{
			get
			{
				return (JobDeclaration != null) && ((JobDeclaration)JobDeclaration).ImplementUPE;
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			if (JobDeclaration != null)
			{
				((JobDeclaration)JobDeclaration).AddInfo.ZA_UPEIndicator_HiddenInfo.ValueChanged += UPEIndicator_ValueChanged;
			}

			bool notExWarehouseActiveAndNotNonTransportDeclarationType = !JobDeclaration.IsExWarehouse && !JobDeclaration.IsNonTransportDeclarationType;
			bool notNonTransportDeclarationType = !JobDeclaration.IsNonTransportDeclarationType;

			CustomsReceiptForGoodsIdTextBox.Visible = ((JobDeclaration)JobDeclaration).IsTransportModeOther;

			AQISInspectionLocationGroupBox.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;
			AQISConcernTypes.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;
			AQISInformationGroupBox.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;
			AQISDocumentGroupBox.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;
			AQISPremisesIdAndPackagesGroupBox.Visible = notExWarehouseActiveAndNotNonTransportDeclarationType;

			LandedCostingDefaultsGroupBox.Visible = notNonTransportDeclarationType;
			FirstPaidUnderProtestID.Visible = notNonTransportDeclarationType;
			zGroupBox3.Visible = notNonTransportDeclarationType;
			DeclarationIndicators.Visible = notNonTransportDeclarationType;
			ExternalDeclaration.Visible = notNonTransportDeclarationType;
			UnaccompaniedPersonalEffectsID.Visible = notNonTransportDeclarationType;
			ForceManualTILVCheckBox.Visible = notNonTransportDeclarationType;
			SOFACheckBox.Visible = ((JobDeclaration)JobDeclaration).IsSOFAVisible;

			HeaderAmberReasonTypeDropEdit.Size = HeaderAmberReasonTypeCodeFindBox.Size;
			HeaderAmberReasonTypeDropEdit.Location = HeaderAmberReasonTypeCodeFindBox.Location;
			HeaderAmberReasonTypeDropEdit.Visible = !notNonTransportDeclarationType;

			HeaderAmberReasonTypeCodeFindBox.Visible = notNonTransportDeclarationType;

			UnaccompaniedPersonalEffectsID.Visible = !ImplementUPE;
			UnaccompaniedPersonalEffectsCheckBox.Visible = ImplementUPE;
			uPETabPage.TabVisible = SetUPETabVisibility();

			PeriodicSettlementGroupBox.Visible = JobDeclaration.IsExWarehouse;
		}

		void UPEIndicator_ValueChanged(object sender, EventArgs e)
		{
			uPETabPage.TabVisible = SetUPETabVisibility();
		}

		bool SetUPETabVisibility()
		{
			return ImplementUPE && ((JobDeclaration)JobDeclaration).IsUPEDeclaration;
		}

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (JobDeclaration != null)
				{
					((JobDeclaration)JobDeclaration).AddInfo.ZA_UPEIndicator_HiddenInfo.ValueChanged -= UPEIndicator_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
