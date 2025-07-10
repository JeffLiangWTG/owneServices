using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class OrgSupplierPartFormCustomsControlGlobalTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestTariffFindBoxAndColumn()
		{
			var part = Factory.New<Business.OrgSupplierPart>();
			part.PivotsForBinding.AddNew();
			using var form = new ZForm(part);
			using var control = new OrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			var codeFindBox = control.FindSingle<Universal.GUI.TariffFindBox>("TariffCodeFindBox");
			AssertNotNull("Should have the box", codeFindBox);
			AssertEquals("Tariff box country code", Core.Constants.CountryCodes.China, codeFindBox.GetCountryCode());
			AssertEquals("Tariff box data grouping", Core.Constants.CountryCodes.China, codeFindBox.GetDataGrouping());
			AssertEquals("Tariff box tariff type", Universal.Constants.TariffTypes.HarmonizedSystem, codeFindBox.TariffType);
			Assert("TariffFindBox is DescriptionMinLength is 2:", codeFindBox.PartialDescriptionMinLengthForSearch == 2);
			var codeFindColumn = control.FindSingle<ZGrid>("PivotGrid").GetColumnStyle(CusClassPartPivot.Schema.CI_TariffNum) as Universal.GUI.TariffColumnStyleInfo;
			AssertNotNull("Should have the column", codeFindColumn);
			AssertEquals("Tariff box country code", Core.Constants.CountryCodes.China, codeFindColumn.GetCountryCode());
			AssertEquals("Tariff box data grouping", Core.Constants.CountryCodes.China, codeFindColumn.GetDataGrouping());
			AssertEquals("Tariff box tariff type", Universal.Constants.TariffTypes.HarmonizedSystem, codeFindColumn.TariffType);
			AssertEquals("Min desc length: ", 2, codeFindColumn.PartialDescriptionMinLengthForSearch);
		}

		public void TestChangeControlsVisibility()
		{
			var part = Factory.New<Business.OrgSupplierPart>();
			var partPivot = part.PivotsForBinding.AddNew();
			using var form = new ZForm(part);
			using var control = new OrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			form.Show();
			control.SetDataBinding(part, "");
			var originDistrictCodeFindBox = control.FindSingle<ZCodeFindBox>("CNC_OriginDistrictCodeFindBox");
			var originRegionCodeFindBox = control.FindSingle<ZCodeFindBox>("CNC_OriginRegionCodeFindBox");
			var ciqOriginStateCodeFindBox = control.FindSingle<ZCodeFindBox>("CNC_OriginStateCodeFindBox");
			var destinationDistrictCodeFindBox = control.FindSingle<ZCodeFindBox>("CNC_DestinationDistrictCodeFindBox");
			var destinationRegionCodeFindBox = control.FindSingle<ZCodeFindBox>("CNC_DestinationRegionCodeFindBox");
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("CNC_OriginDistrictCodeFindBox should be hidden", !originDistrictCodeFindBox.Visible);
			Assert("CNC_OriginRegionCodeFindBox should be hidden", !originRegionCodeFindBox.Visible);
			Assert("CNC_OriginStateCodeFindBox should be shown", ciqOriginStateCodeFindBox.Visible);
			Assert("CNC_DestinationDistrictCodeFindBox should be shown", destinationDistrictCodeFindBox.Visible);
			Assert("CNC_DestinationRegionCodeFindBox should be shown", destinationRegionCodeFindBox.Visible);
			partPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert("CNC_OriginDistrictCodeFindBox should be shown", originDistrictCodeFindBox.Visible);
			Assert("CNC_OriginRegionCodeFindBox should be shown", originRegionCodeFindBox.Visible);
			Assert("CNC_OriginStateCodeFindBox should be hidden", !ciqOriginStateCodeFindBox.Visible);
			Assert("CNC_DestinationDistrictCodeFindBox should be hidden", !destinationDistrictCodeFindBox.Visible);
			Assert("CNC_DestinationRegionCodeFindBox should be hidden", !destinationRegionCodeFindBox.Visible);
			var detailTabControl = control.FindSingle<ZTabControl>("DetailTabControl");
			var ciqTabPage = control.FindSingle<ZTabPage>("CIQTabPage");
			detailTabControl.SelectedTab = ciqTabPage;
			var nonDangerousChemicalFlagCheckBox = control.FindSingle<ZCheckBox>("CNC_NonDangerousChemicalFlagCheckBox");
			AssertEquals("CNC_NonDangerousChemicalFlagCheckBox should be invisible by default.", false, nonDangerousChemicalFlagCheckBox.Visible);
			var undg = part.UNDGs.AddNew();
			undg.DI_IMOClass = "tt";
			partPivot.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("CNC_NonDangerousChemicalFlagCheckBox should be visible for UNDG has value.", true, nonDangerousChemicalFlagCheckBox.Visible);
			partPivot.DangerousGoodsDGSubs = ZGuid.Empty;
			var testCollection = partPivot.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._31);
			partPivot.DangerousGoodsDGSubs = ZGuid.Empty;
			AssertEquals("CNC_NonDangerousChemicalFlagCheckBox should be visible for CargoAttribute 31 selected.", true, nonDangerousChemicalFlagCheckBox.Visible);
		}

		public void TestControlExistances()
		{
			CombineAssertions(() =>
			{
				using var control = new OrgSupplierPartFormCustomsControl();
				TestUtility.AssertControlExistance(control, "CNC_UNPackageMarkingDropEdit", "PivotsForBinding.CNC_UNPackageMarking");
				TestUtility.AssertControlExistance(control, "CNC_QualityGuaranteePeriodCalcEdit", "PivotsForBinding.CNC_QualityGuaranteePeriod");
				TestUtility.AssertControlExistance(control, "CNC_NonDangerousChemicalFlagCheckBox", "PivotsForBinding.CNC_NonDangerousChemicalFlag");
				TestUtility.AssertControlExistance(control, "CNC_TradeQuantityDropEdit", "PivotsForBinding.CNC_TradeUnitQty");
				TestUtility.AssertControlExistance(control, "DangerousGoodsGuidFindBox", "PivotsForBinding.DangerousGoodsDGSubs");
			});
		}

		public void TestCargoAttributesButton_Click()
		{
			var part = Factory.New<Business.OrgSupplierPart>();
			var partPivot = part.PivotsForBinding.AddNew();
			using var form = new ZForm(part);
			using var control = new OrgSupplierPartFormCustomsControl();
			form.Controls.Add(control);
			control.SetDataBinding(part, "");
			form.Show();
			control.FindSingle<ZGrid>("PivotGrid").Select(0);
			var detailTabControl = control.FindSingle<ZTabControl>("DetailTabControl");
			var ciqTabPage = control.FindSingle<ZTabPage>("CIQTabPage");
			detailTabControl.SelectedTab = ciqTabPage;

			var cargoAttributesUserControl = ciqTabPage.FindSingle<CodeDescriptionSelectionUserControl>("CNC_CargoAttributesTextBox");
			AssertEquals("PivotsForBinding.CargoAttributesAsString", cargoAttributesUserControl.TextBox.GetBindingMember());
			cargoAttributesUserControl.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertSame(partPivot.CargoAttributes, (ZFormModaliser.LastIBusinessShownOnDialogForTest as CodeDescriptionOptionCollectionParent).OptionCollection.Storage);
		}

		protected override ZUserControl GetUserControl() => new OrgSupplierPartFormCustomsControl();

		protected override string UserControlName => "OrgSupplierPartFormCustomsControl";

		protected override string ExpectedTariffColumnName => CusClassPartPivot.Schema.CI_TariffNum;

		protected override string ExpectedUniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override string ExpectedCustomsCountryCode => Core.Constants.CountryCodes.China;

		protected override string ExpectedDataGrouping => Core.Constants.CountryCodes.China;
	}
}
