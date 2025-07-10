using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUImportTariffBulkChangeForm : ZChildForm
	{
		public AUImportTariffBulkChangeForm(AUImportTariffBulkChange businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DisableNewAction();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			BusinessEntity.HasChanges = true;
		}

		public new AUImportTariffBulkChange BusinessEntity => base.BusinessEntity as AUImportTariffBulkChange;

		public override string FormCaption => "Import Tariff Bulk Change";

		void RefreshGrids()
		{
			int oldGridPosi = OldTariffsZGrid.ListManager.Position;
			int newGridPosi = NewTariffsZGrid.ListManager.Position;
			OldTariffsZGrid.ListManager.Position = oldGridPosi < 1 ? 1 : 0;
			OldTariffsZGrid.ListManager.Position = oldGridPosi;
			NewTariffsZGrid.ListManager.Position = newGridPosi;
		}

		void UpdateOriginalLookups(object sender, EventArgs e)
		{
			if (OldClassificationsZGrid.SelectedElements.Length < 1 || NewTariffsZGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(UpdateOriginalSelectionErrorText, "Update Original Lookups Selection Error");
			}
			else
			{
				#pragma warning disable IDE0001 // Prevent simplification to base class
				BusinessEntity.UpdateOriginalLookups(OldTariffsZGrid.ListManager.GetCurrent() as AUImportTariffBulkChange.TariffBulkChangeOldTariff,
						NewTariffsZGrid.SelectedElements[0] as AUImportTariffBulkChange.TariffBulkChangeNewTariff,
						OldClassificationsZGrid.SelectedElements);
				#pragma warning restore IDE0001 // Prevent simplification to base class
				RefreshGrids();
			}
		}

		void UpdateProductsZButton_Click(object sender, EventArgs e)
		{
			if (PartsZGrid.SelectedElements.Length < 1 || NewClassificationsZGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(UpdateProductsSelectionErrorText, "Update Products Selection Error");
			}
			else
			{
				if (NewClassificationsZGrid.SelectedElements[0].HasErrors)
				{
					Globals.Message.ShowError(UpdateProductsGridErrorsText, "Update Products Grid Error");
				}
				else
				{
					#pragma warning disable IDE0001 // Prevent simplification to base class
					BusinessEntity.UpdateProducts(OldTariffsZGrid.ListManager.GetCurrent() as AUImportTariffBulkChange.TariffBulkChangeOldTariff,
								NewClassificationsZGrid.SelectedElements[0] as TariffBulkChange.TBCClassification, null,
								PartsZGrid.SelectedElements);
					#pragma warning restore IDE0001 // Prevent simplification to base class
					RefreshGrids();
				}
			}
		}

		void MakeNewClassZButton_Click(object sender, EventArgs e)
		{
			if (NewTariffsZGrid.SelectedElements.Length < 1)
			{
				Globals.Message.ShowError(MakeNewClassificationsErrorText, "Make Classifications Selection Error");
			}
			else
			{
				#pragma warning disable IDE0001 // Prevent simplification to base class
				BusinessEntity.MakeNewClassifications(OldTariffsZGrid.ListManager.GetCurrent() as AUImportTariffBulkChange.TariffBulkChangeOldTariff,
							NewTariffsZGrid.SelectedElements);
				#pragma warning restore IDE0001 // Prevent simplification to base class
				RefreshGrids();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = BusinessEntity.ApplyAdditionalContinueWithSave();
			}
			return result;
		}

		const string UpdateOriginalSelectionErrorText = "Please select one or more Original Lookups (use Ctrl + Right Click to select a row) and one (only) New Tariff.";
		const string UpdateProductsSelectionErrorText = "Please select one or more Products (use Ctrl + Right Click to select a row) and one (only) New Lookup.";
		const string UpdateProductsGridErrorsText = "Please correct error on New Classifications.";
		const string MakeNewClassificationsErrorText = "Please select one or more New Tariffs (use Ctrl + Right Click to select a row).";
	}
}
