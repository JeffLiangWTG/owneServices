using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestFeesGrid()
		{
			using var testItem = new ItemDetailsUserControl();
			AssertNotNull(testItem.FindSingleOrDefault<ZGrid>("FeesGrid"));
		}

		public void TestFeesGridDecimals()
		{
			using var testItem = new ItemDetailsUserControl();
			var feesGrid = testItem.FindSingleOrDefault<ZGrid>("FeesGrid");
			AssertNotNull("Pre-requisite: FeesGrid", feesGrid);

			var columnBaseValue = feesGrid.GetColumnStyle("BFE_BaseValue") as ZCalcEditColumnStyleInfo;
			var columnChargeAmount = feesGrid.GetColumnStyle("BFE_ChargeAmount") as ZCalcEditColumnStyleInfo;
			CombineAssertions(() =>
			{
				AssertNotNull("Column BFE_BaseValue", columnBaseValue);
				AssertNotNull("Column BFE_ChargeAmount", columnChargeAmount);
			});
			CombineAssertions(() =>
			{
				AssertEquals("BFE_BaseValue.Decimals", 2, columnBaseValue.Decimals);
				AssertEquals("BFE_ChargeAmount.Decimals", 2, columnChargeAmount.Decimals);
			});
		}

		public void TestAdditionalSupplementaryCodesTextBox()
		{
			using (var testItem = new ItemDetailsUserControl())
			{
				AssertNotNull(testItem.FindSingle<ZTextBox>("AdditionalSupplementaryCodesTextBox"));
			}
		}

		public void TestAdditionalSupplementaryCodesEditButton()
		{
			using (var testItem = new ItemDetailsUserControl())
			{
				var button = testItem.FindSingle<ZButton>("AdditionalSupplementaryCodesEditButton");
				AssertEquals("Additional codes...", button.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertNoExceptionThrown(() => button.PerformClick());
			}
		}

		public void TestCommodityCodeTariffFindBox()
		{
			using (var testItem = new ItemDetailsUserControl())
			{
				var commodityCodeTariffFindBox = testItem.FindSingle<Universal.GUI.TariffFindBox>("CommodityCodeTariffFindBox");
				AssertEquals("CommodityCode is bound to the formatted tariff", NctsCommonCargoDesc.Schema.BY_FormattedHarmonisedTariff, commodityCodeTariffFindBox.GetBindingMember());
			}
		}

		public void TestCustomsValueDropEditCaption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.EuropeanUnion))
			using (var control = new ItemDetailsUserControl())
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				control.SetDataBinding(header, ZString.Empty);
				var customsValueCaption = control.FindSingle<ZCalcDropEdit>("CustomsValueDropEdit").GetExtension<ILabelCaptionRenderer>().Caption;
				AssertEquals("Value in EUR", customsValueCaption);
			}
		}

		public void TestDescriptionOfGoodsTextBoxAllowsNormalCase()
		{
			using (var control = new ItemDetailsUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("DescriptionOfGoodsTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestCustomsFirstQtyDropEdit()
		{
			using (var control = new ItemDetailsUserControl())
			{
				var customsFirstQtyDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsFirstQtyDropEdit");
				AssertEquals("Should be visible", true, customsFirstQtyDropEdit.Visible);
			}
		}

		public void TestCustomsThirdQtyDropEdit()
		{
			using (var control = new ItemDetailsUserControl())
			{
				var customsThirdQtyDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsThirdQtyDropEdit");
				AssertEquals("Should not be visible for EU", false, customsThirdQtyDropEdit.Visible);
			}
		}
	}
}
