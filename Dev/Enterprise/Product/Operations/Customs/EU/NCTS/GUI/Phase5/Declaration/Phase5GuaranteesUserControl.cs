using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5GuaranteesUserControl : EU.GUI.GuaranteesUserControl
	{
		public Phase5GuaranteesUserControl()
		{
			InitializeComponent();
		}

		protected override void GridColumnStyleDecider()
		{
			var bondTypeInfo = GuaranteesGrid.GetColumnStyle(AutoCusBondDetail.Schema.PW_BondType);
			bondTypeInfo.CaptionResourceString = Res.GetData("2B92F1CF-30D1-46DE-AB26-601E41C9527A", "Type", "Type", "Type of Guarantee", "Type");
			bondTypeInfo.Width = 87;

			var bondNumberInfo = GuaranteesGrid.GetColumnStyle(AutoCusBondDetail.Schema.PW_BondNumber);
			bondNumberInfo.CaptionResourceString = Res.GetData("1E7D355A-34A0-41E3-8451-60A5BFBEB866", "GRN", "Guarantee Ref. No.", "Guarantee Reference Number", "GRN");
			bondNumberInfo.Width = 226;

			var passwordInfo = GuaranteesGrid.GetColumnStyle(AutoCusBondDetail.Schema.PW_Password);
			passwordInfo.CaptionResourceString = Res.GetData("15ED8496-949A-498F-8E5B-7E1C7243E864", "GAC", "Access Code", "Guarantee Access Code", "GAC");
			passwordInfo.Width = 144;

			var bondAmountInfo = GuaranteesGrid.GetColumnStyle(AutoCusBondDetail.Schema.PW_BondAmount);
			bondAmountInfo.CaptionResourceString = Res.GetData("4CDEE329-34C4-4026-A746-8865AE890D26", "Amount", "Amount", "Liability Amount", "Amount");
			bondAmountInfo.Width = 202;

			var bondNumber2Info = GuaranteesGrid.GetColumnStyle(AutoCusBondDetail.Schema.PW_BondNumber2);
			bondNumber2Info.CaptionResourceString = Res.GetData("768CE87A-3FCC-4F73-94A4-CB7F1C795D22", "Other Guarantee Ref.", "Other Guarantee Ref. No.", "Other Guarantee Reference Number", "Other Guarantee Ref.");
			bondNumber2Info.Width = 247;
		}

		protected override void AddAndRemoveColumns()
		{
			RemoveColumnStyle(AutoCusBondDetail.Schema.PW_BondFiledPort);
			RemoveColumnStyle(AutoCusBondDetail.Schema.PW_ValidityLimitation);
			RemoveColumnStyle(AutoCusBondDetail.Schema.PW_HolderIdentification);
			RemoveColumnStyle(GuaranteeForDeclaration.Schema.EntryInstructionID);

			var liabilityFractionColumnStyleInfo = new ZDropEditColumnStyleInfo();
			liabilityFractionColumnStyleInfo.ColumnName = AutoCusBondDetail.Schema.PW_SuretyCode;
			liabilityFractionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			GuaranteesGrid.ColumnStyles.Add(liabilityFractionColumnStyleInfo);

			var liabilityAmountOverrideColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			liabilityAmountOverrideColumnStyleInfo.ColumnName = AutoCusBondDetail.Schema.PW_Override;
			liabilityAmountOverrideColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			GuaranteesGrid.ColumnStyles.Add(liabilityAmountOverrideColumnStyleInfo);
		}

		protected void RemoveColumnStyle(string columnName)
		{
			var columnInfo = GuaranteesGrid.GetColumnStyle(columnName);
			if (columnInfo != null)
			{
				GuaranteesGrid.ColumnStyles.Remove(columnInfo);
			}
		}

		void GuaranteesGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = GuaranteesGrid.ListManager;
			HandleShowLiabilityCalculationPopUp(listManager);
			listManager.CurrentChanged -= ListManager_CurrentChanged;
			listManager.CurrentChanged += ListManager_CurrentChanged;
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (sender is CurrencyManager listManager)
			{
				HandleShowLiabilityCalculationPopUp(listManager);
			}
		}

		void HandleShowLiabilityCalculationPopUp(CurrencyManager listManager)
		{
			if (listManager?.GetCurrent() is NctsGuarantee currentSelectedGuarantee)
			{
				if (guaranteeWithEvent != null)
				{
					guaranteeWithEvent.PW_OverrideInfo.ValueChanged -= ShowLiabilityCalculationPopUp;
				}
				currentSelectedGuarantee.PW_OverrideInfo.ValueChanged += ShowLiabilityCalculationPopUp;
				guaranteeWithEvent = currentSelectedGuarantee;
			}
		}

		void ShowLiabilityCalculationPopUp(object sender, EventArgs e)
		{
			if (sender is NctsGuarantee guarantee && guarantee.PW_Override)
			{
				using (var guaranteeCalculationLiabilityAmountForm = new Phase5GuaranteeCalculationLiabilityAmountForm(new CalculateLiabilityBizObj(guarantee.Factory, guarantee)))
				{
					ZFormModaliser.ShowDialogWithoutDispose(guaranteeCalculationLiabilityAmountForm);
				}
				GuaranteesGrid.Focus();
			}
		}

		NctsGuarantee guaranteeWithEvent;
	}
}
