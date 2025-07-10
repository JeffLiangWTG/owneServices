using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUImportTariffBulkChangeForm))]
	sealed class AUImportTariffBulkChangeFormTest : ZFormBasherTest
	{
		public void TestUpdateProductsWithTheNewLookupsFromNewTariff()
		{
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PART1";
			var classification1 = part1.ClassificationsForBinding.AddNew();
			classification1.CC_TariffNum = "0105.92.00 01";
			classification1.CC_ClassificationType = Common.ClassificationType.IMP;
			classification1.CC_LookupCode = "L1";
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PART2";
			var classification2 = part2.ClassificationsForBinding.AddNew();
			classification2.CC_TariffNum = "0105.92.00 01";
			classification2.CC_ClassificationType = Common.ClassificationType.IMP;
			classification2.CC_LookupCode = "L2";
			Factory.Save();
			var tariffBulkChange = new AUImportTariffBulkChange(Factory);
			var oldTariff = tariffBulkChange.TariffBulkChangeOldTariffs.AddNew();
			oldTariff.OldTariffNum = "0105.92.00 01";
			var newTariff = oldTariff.TariffBulkChangeNewTariffs.AddNew();
			newTariff.NewTariffNum = "0105.92.00 00";
			var newLookup = oldTariff.NewClassifications.AddNew();
			newLookup.CC_TariffNum = newTariff.NewTariffNum;
			newLookup.CC_LookupCode = "NEW LOOKUP";
			using (var form = new AUImportTariffBulkChangeForm(tariffBulkChange))
			{
				form.Show();
				var oldTariffsZGrid = (ZGrid)form.Controls.Find("OldTariffsZGrid", true).Single();
				var newClassificationsZGrid = (ZGrid)form.Controls.Find("NewClassificationsZGrid", true).Single();
				var partsZGrid = (ZGrid)form.Controls.Find("PartsZGrid", true).Single();
				var updateProductsZButton = (CargoWise.Windows.UI.KButton)form.Controls.Find("UpdateProductsZButton", true).Single();
				CombineAssertions(() =>
				{
					AssertEquals("PreCondition: oldTariffsZGrid has 1 tariff", 1, oldTariffsZGrid.List.Count);
					oldTariffsZGrid.Select(0);
					AssertEquals("PreCondition: PartsZGrid has two products", 2, partsZGrid.List.Count);
					partsZGrid.Select(0);
					partsZGrid.Select(1);
					var selectedClassPartPivot1 = partsZGrid.SelectedElements.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => x.OldClassificationCode == "L1");
					var selectedClassPartPivot2 = partsZGrid.SelectedElements.Cast<BaseCusClassPartPivot>().FirstOrDefault(x => x.OldClassificationCode == "L2");
					AssertProductRow(selectedClassPartPivot1, "PART1", "0105.92.00 01", "L1", "", "");
					AssertProductRow(selectedClassPartPivot2, "PART2", "0105.92.00 01", "L2", "", "");
					AssertEquals("PreCondition: NewClassificationsZGrid has 1 new classification", 1, newClassificationsZGrid.List.Count);
					newClassificationsZGrid.Select(0);
					var selectedNewLookup = newClassificationsZGrid.SelectedElements[0] as TariffBulkChange.TBCClassification;
					AssertEquals("PreCondition: selected new Lookups from new Tariff - Orig. Lookup", "NEW LOOKUP", selectedNewLookup.CC_LookupCode);
					AssertEquals("PreCondition: selected new Lookups from new Tariff - Orig. Tariff", "0105.92.00 00", selectedNewLookup.CC_TariffNum);
					AssertEquals("PreCondition: selected new Lookups from new Tariff - New Lookup", "", selectedNewLookup.NewLookupCode);
					AssertEquals("PreCondition: selected new Lookups from new Tariff - New Tariff", "", selectedNewLookup.NewTariffNum);
					updateProductsZButton.PerformClick();
					AssertProductRow(selectedClassPartPivot1, "PART1", "0105.92.00 01", "L1", "0105.92.00 00", "NEW LOOKUP");
					AssertProductRow(selectedClassPartPivot2, "PART2", "0105.92.00 01", "L2", "0105.92.00 00", "NEW LOOKUP");
				});
			}
		}

		protected override Form GetFormToBashCore() => new AUImportTariffBulkChangeForm(new AUImportTariffBulkChange(Factory));

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		void AssertProductRow(BaseCusClassPartPivot selectedClassPartPivot, string partNum, string oldTariffCode, string oldClassificationCode, string newTariffNum, string newLookUpCode)
		{
			AssertEquals("Products for selected Old Tariff - Part Num", partNum, selectedClassPartPivot.Part.OP_PartNum);
			AssertEquals("Products for selected Old Tariff - Orig.Import Lookup", oldTariffCode, selectedClassPartPivot.OldTariffCode);
			AssertEquals("Products for selected Old Tariff - Orig.Import TariffNum", oldClassificationCode, selectedClassPartPivot.OldClassificationCode);
			AssertEquals("Products for selected Old Tariff - New LookUp Code", newLookUpCode, selectedClassPartPivot.NewLookUpCode);
			AssertEquals("Products for selected Old Tariff - New Tariff Num", newTariffNum, selectedClassPartPivot.NewTariffNum);
		}
	}
}
