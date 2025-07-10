using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class HTSTariffBulkChangeForm : ZChildForm
	{
		public HTSTariffBulkChangeForm(CAHTSTariffBulkChange businessEntity, bool automaticConvert)
			: base(businessEntity)
		{
			this.automaticConvert = automaticConvert;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DisableNewAction();
		}
		readonly bool automaticConvert;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			BusinessEntity.HasChanges = true;
		}
		public new CAHTSTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as CAHTSTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("04de26af-5b86-4564-aef3-382191992edd", "Tariff Bulk Change"); }
		}

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
				Globals.Message.ShowError(Enterprise.Customs.GUI.TariffBulkChangeForm.UpdateOriginalSelectionErrorText, Enterprise.Customs.GUI.TariffBulkChangeForm.UpdateOriginalLookupsSelectionErrorText);
			}
			else
			{
				BusinessEntity.UpdateOriginalLookups(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
						NewTariffsZGrid.SelectedElements[0] as TariffBulkChange.TariffBulkChangeNewTariff,
						OldClassificationsZGrid.SelectedElements);
				RefreshGrids();
			}
		}

		public static string UpdateProductsSelectionErrorText
		{
			get { return Res.GetString("8e34c506-810e-41e2-b5d0-42ad9969f6d0", "Please select one or more Products (use Ctrl + Right Click to select a row) and one (only) New Lookup or one (only) New Tariff."); }
		}

		void UpdateProductsZButton_Click(object sender, EventArgs e)
		{
			if (PartsZGrid.SelectedElements.Length < 1 || (NewClassificationsZGrid.SelectedElements.Length + NewTariffsZGrid.SelectedElements.Length) != 1)
			{
				Globals.Message.ShowError(UpdateProductsSelectionErrorText, Enterprise.Customs.GUI.TariffBulkChangeForm.UpdateProductsGridErrorsText);
			}
			else
			{
				if (NewClassificationsZGrid.SelectedElements.Length > 0 && NewClassificationsZGrid.SelectedElements[0].HasErrors)
				{
					Globals.Message.ShowError(Enterprise.Customs.GUI.TariffBulkChangeForm.UpdateProductsGridErrorsText, Enterprise.Customs.GUI.TariffBulkChangeForm.UpdateProductsGridErrorsCaption);
				}
				else
				{
					if (NewTariffsZGrid.SelectedElements.Length == 1)
					{
						BusinessEntity.UpdateProducts(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
									null, NewTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeNewTariff,
									PartsZGrid.SelectedElements);
					}
					else
					{
						BusinessEntity.UpdateProducts(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
									NewClassificationsZGrid.SelectedElements[0] as BaseCusClassification, null,
									PartsZGrid.SelectedElements);
					}
					RefreshGrids();
				}
			}
		}

		void MakeNewClassZButton_Click(object sender, EventArgs e)
		{
			if (NewTariffsZGrid.SelectedElements.Length < 1)
			{
				Globals.Message.ShowError(Enterprise.Customs.GUI.TariffBulkChangeForm.MakeNewClassificationsErrorText, Enterprise.Customs.GUI.TariffBulkChangeForm.MakeNewClassificationsSelectionErrorText);
			}
			else
			{
				BusinessEntity.MakeNewClassifications(OldTariffsZGrid.ListManager.GetCurrent() as TariffBulkChange.TariffBulkChangeOldTariff,
							NewTariffsZGrid.SelectedElements);
				RefreshGrids();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = BusinessEntity.AdditionalContinueWithSave(automaticConvert);
			}
			return result;
		}
	}
}
