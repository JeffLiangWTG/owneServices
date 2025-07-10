using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUOrgSupplierPartFormCustomsControl : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public AUOrgSupplierPartFormCustomsControl()
		{
			InitializeComponent();
			DisableUnneededPivotColumns();
		}

		#region properties

		CusClassPartPivot CurrentPivot
		{
			get
			{
				return currentPartPivot != null && !currentPartPivot.IsDeleted
					? currentPartPivot as CusClassPartPivot
					: null;
			}
		}

		internal bool IsImport => CurrentPivot?.IsImport ?? false;
		internal bool IsExport => CurrentPivot?.IsExport ?? false;
		#endregion

		#region overrides

		protected override string TariffColumnNameCore => Business.CusClassPartPivot.Schema.CI_TariffNum;
		protected override string UniversalTariffType => IsImport ? Universal.Constants.TariffTypes.Import : Universal.Constants.TariffTypes.Export;
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Australia;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Australia;

		protected override ZBaseFindBoxColumnStyleInfo CreateTariffColumn()
		{
			if (AUCAHECCWrapper.EnableCWRefForAHECC)
			{
				var tariffColumn = (Universal.GUI.TariffColumnStyleInfo)base.CreateTariffColumn();
				tariffColumn.GetTariffType = () => UniversalTariffType;
				return tariffColumn;
			}
			else
			{
				return new AHECCTariffColumnStyleInfo(() => IsImport)
				{
					CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUOrgSupplierPartFormCustomsControl|662ed62c-169f-4c52-9864-4e01a17b5c19", "Tariff", "Tariff Code"),
					ColumnName = TariffColumnName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				};
			}
		}

		#endregion

		#region event handlers

		void CMRDefaultCPDecAnswersButton_Click(object sender, EventArgs e)
		{
			var pivot = CurrentPivot;
			if (pivot?.IsImport ?? false)
			{
				pivot.GenerateQuestion();
				ZFormModaliser.ShowDialogAndDispose(new CPQAProductForm(pivot));
			}
		}

		#endregion

		protected override void ChangeControlsVisibility()
		{
			var isImport = IsImport;
			var enableCWRefForImportTariff = AUCClassWrapper.UseCustomsReferenceData;
			var enableCWRefForExportTariff = AUCAHECCWrapper.EnableCWRefForAHECC;

			CMRDefaultCPDecAnswersButton.Visible = isImport;
			ImportTariffFindBox.Visible = isImport && enableCWRefForImportTariff;
			ImportTariffFindBoxAUCClass.Visible = isImport && !enableCWRefForImportTariff;
			ExportTariffFindBox.Visible = !isImport && enableCWRefForExportTariff;
			ExportTariffFindBoxAHECC.Visible = !isImport && !enableCWRefForExportTariff;

			ClassificationFindBox.ModuleID = isImport ? ZArchitecture.Modules.ModuleIDs.ImportClassification : ZArchitecture.Modules.ModuleIDs.ExportClassification;

			var ciccFindBox = (PivotGrid?.Columns[CusClassPartPivot.Schema.CI_CC]?.ColumnStyle as ZGuidFindBoxColumnStyle)?.EditControl as ZGridFindBox;
			if (ciccFindBox != null)
			{
				ciccFindBox.ModuleID = ClassificationFindBox.ModuleID;
			}

			using (PivotGrid?.SuspendCancelOfNonEditedRowOnLeaving())
			{
				HTIQuarantineTabPage.TabVisible = isImport;
				HTIPermitsTabPage.TabVisible = isImport;
				HTEQuarantineTabPage.TabVisible = IsExport;
			}
		}
	}
}
