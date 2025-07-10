using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStoragePackedItemWithGridLayout))]
	public class G5V1TemporaryStoragePackedItemWithGridLayoutTest : LayoutsAbstractTest
	{
		public void TestMissingCheckBoxCanVisibility()
		{
			var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			using (var form = new ZForm(tempStorageHeader))
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var tempStoragePackedItem = control.FindSingleOrDefault<ZCheckBox>("MissingCheckBox");
					tempStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
					AssertEquals("MissingCheckBox is not visible when declaration is not G5P", false, tempStoragePackedItem.Visible);

					tempStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
					AssertEquals("MissingCheckBox is visible when declaration is G5P", true, tempStoragePackedItem.Visible);
				});
			}
		}

		[RequiresSTA]
		public void TestDataLiabilityAmountCanVisibility()
		{
			var tempStorageHeader = Factory.New<TemporaryStorageHeader>();
			using (var form = new ZForm(tempStorageHeader))
			using (var control = new G5V1TemporaryStoragePackedItemControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var countryOfOriginDropEdit = control.FindSingleOrDefault<ZDropEdit>("CountryOfOriginDropEdit");
					var customsValueCalcDropEdit = control.FindSingleOrDefault<ZCalcDropEdit>("CustomsValueCalcDropEdit");
					var supplementaryUnitsCalcDropEdit = control.FindSingleOrDefault<ZCalcDropEdit>("SupplementaryUnitsCalcDropEdit");
					var customsSecondQuantityDropEdit = control.FindSingleOrDefault<ZCalcDropEdit>("CustomsSecondQuantityDropEdit");
					var customsThirdQuantityDropEdit = control.FindSingleOrDefault<ZCalcDropEdit>("CustomsThirdQuantityDropEdit");
					var additionalSupplementaryCodesUserControl = control.FindSingleOrDefault<AdditionalSupplementaryCodesUserControl>("AdditionalSupplementaryCodesUserControl");
					var dutiesAndTaxesLabel = control.FindSingleOrDefault<ZLabel>("DutiesAndTaxesLabel");
					var dutiesAndTaxesGrid = control.FindSingleOrDefault<ZGrid>("DutiesAndTaxesGrid");
					tempStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
					AssertEquals("CountryOfOriginDropEdit is not visible when declaration is LAM", false, countryOfOriginDropEdit.Visible);
					AssertEquals("CustomsValueCalcDropEdit is not visible when declaration is LAM", false, customsValueCalcDropEdit.Visible);
					AssertEquals("SupplementaryUnitsCalcDropEdit is not visible when declaration is LAM", false, supplementaryUnitsCalcDropEdit.Visible);
					AssertEquals("CustomsSecondQuantityDropEdit is not visible when declaration is LAM", false, customsSecondQuantityDropEdit.Visible);
					AssertEquals("CustomsThirdQuantityDropEdit is not visible when declaration is LAM", false, customsThirdQuantityDropEdit.Visible);
					AssertEquals("AdditionalSupplementaryCodesUserControl is not visible when declaration is LAM", false, additionalSupplementaryCodesUserControl.Visible);
					AssertEquals("DutiesAndTaxesLabel is not visible when declaration is LAM", false, dutiesAndTaxesLabel.Visible);
					AssertEquals("DutiesAndTaxesGrid is not visible when declaration is LAM", false, dutiesAndTaxesGrid.Visible);

					tempStorageHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
					AssertEquals("CountryOfOriginDropEdit is not visible when declaration is G5P", true, countryOfOriginDropEdit.Visible);
					AssertEquals("CustomsValueCalcDropEdit is not visible when declaration is G5P", true, customsValueCalcDropEdit.Visible);
					AssertEquals("SupplementaryUnitsCalcDropEdit is not visible when declaration is G5P", true, supplementaryUnitsCalcDropEdit.Visible);
					AssertEquals("CustomsSecondQuantityDropEdit is not visible when declaration is G5P", true, customsSecondQuantityDropEdit.Visible);
					AssertEquals("CustomsThirdQuantityDropEdit is not visible when declaration is G5P", true, customsThirdQuantityDropEdit.Visible);
					AssertEquals("AdditionalSupplementaryCodesUserControl is not visible when declaration is G5P", true, additionalSupplementaryCodesUserControl.Visible);
					AssertEquals("DutiesAndTaxesLabel is not visible when declaration is G5P", true, dutiesAndTaxesLabel.Visible);
					AssertEquals("DutiesAndTaxesGrid is not visible when declaration is G5P", true, dutiesAndTaxesGrid.Visible);
				});
			}
		}

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumn;
				yield return SecondColumn;
				yield return ThirdColumn;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
		{
			get
			{
				yield return (EUControlBagInstance.SeqTextBox, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.TariffCodeFindBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.GoodDescriptionTextBox, ControlWidthClass.Long);
				yield return (EUControlBagInstance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (EUControlBagInstance.CusCodeCodeFindBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.UCRTextBox, ControlWidthClass.Long);
				yield return (ESControlBagInstance.PresentationDateEdit, ControlWidthClass.Medium);
				yield return (ESControlBagInstance.MissingCheckBox, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumn
		{
			get
			{
				yield return (EUControlBagInstance.CountryOfOriginDropEdit, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.CustomsValueCalcDropEdit, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.CustomsSecondQuantityDropEdit, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.CustomsThirdQuantityDropEdit, ControlWidthClass.Medium);
				yield return (EUControlBagInstance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumn
		{
			get
			{
				yield return (EUControlBagInstance.DutiesAndTaxesLabel, ControlWidthClass.LongNoCaption);
				yield return (EUControlBagInstance.DutiesAndTaxesGrid, ControlWidthClass.LongNoCaption);
			}
		}

		UCC6TemporaryStoragePackedItemDetailsControlBag EUControlBagInstance => EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance;

		G5V1TemporaryStoragePackedItemDetailsControlBag ESControlBagInstance => G5V1TemporaryStoragePackedItemDetailsControlBag.Instance;

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();
		protected override Type ExpectedGridUserControlType => typeof(G5V1TemporaryStoragePackedItemGridControl);
	}
}
