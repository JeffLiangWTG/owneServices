using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemDifferencesDetailsColumnUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsArrivalCargoDesc), userControl.BindingSource.DataSourceType);
		}

		[RequiresSTA]
		public void TestDeclaredValueLabel()
		{
			var control = userControl.DeclaredValueLabel;
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource caption", nameof(NctsArrivalCargoDesc.DeclaredNewLabel), control.BindTo);
				AssertEquals("Visible", true, control.Visible);
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(control));
			});
		}

		[RequiresSTA]
		public void TestDeclaredCommodityCode()
		{
			var header = Factory.New<NctsHeaderForTest>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = header.UnloadingAllowedOrCompleteStatusListExposed[0];
			movementHeader.BM_ValuationDate = new ZDateTime(2023, 1, 1);
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();

			using (var form = new Phase5ArrivalMovementForm(goodsItem.Header))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var unloadingRemarksTabPage = form.UnloadingRemarksTabPage;
				mainTabControl.SelectTab(unloadingRemarksTabPage);

				var unloadingRemarksTabUserControl = form.UnloadingRemarksTabUserControl;
				var houseConsignmentDifferencesTabPage = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage;
				unloadingRemarksTabUserControl.UnloadingRemarksTabControl.SelectTab(houseConsignmentDifferencesTabPage);

				var houseConsignmentDifferencesTabUserControl = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabUserControl;
				var goodsItemsTabPage = houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage;
				houseConsignmentDifferencesTabUserControl.HouseConsignmentDifferencesTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemDifferencesTabUserControl = houseConsignmentDifferencesTabUserControl.Phase5GoodsItemDifferencesTabUserControl;
				var itemDetailsTabPage = phase5GoodsItemDifferencesTabUserControl.ItemDetailsTabPage;
				phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesTabControl.SelectTab(itemDetailsTabPage);

				var declaredCommodityCodeCodeFindBox = phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.FindSingle<TariffFindBox>(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredCommodityCodeCodeFindBox));

				CombineAssertions(() =>
				{
					AssertType<TariffFindBox>(declaredCommodityCodeCodeFindBox);
					AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.BY_HarmonisedTariff), declaredCommodityCodeCodeFindBox.BindTo);
					AssertEquals("Visible", true, declaredCommodityCodeCodeFindBox.Visible);
					AssertEquals("TariffType", null, declaredCommodityCodeCodeFindBox.TariffType);
					AssertEquals("PreBoundMaxLength", 8, declaredCommodityCodeCodeFindBox.PreBoundMaxLength);
					AssertEquals("GetTariffType()", Constants.TariffTypes.Export, declaredCommodityCodeCodeFindBox.GetTariffType?.Invoke());
					AssertEquals("GetEffectiveDate()", header.ArrivalMovementHeader.BM_ValuationDate, declaredCommodityCodeCodeFindBox.GetEffectiveDate());
					AssertEquals("EffectiveTariffCountry", goodsItem.Header.CountryCode, declaredCommodityCodeCodeFindBox.EffectiveTariffCountry);
					AssertEquals("EffectiveDataGrouping", goodsItem.Header.DefaultDataGroupingCode, declaredCommodityCodeCodeFindBox.EffectiveDataGrouping);
					AssertEquals("BindToForDescription", "UniversalTariffDescription", declaredCommodityCodeCodeFindBox.BindToForDescription);
				});
			}
		}

		[RequiresSTA]
		public void TestDeclaredCusCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.BY_CusC4Number), userControl.DeclaredCusCodeCodeFindBox.BindTo);
				AssertEquals("Visible", true, userControl.DeclaredCusCodeCodeFindBox.Visible);
			});
		}

		public void TestDeclaredDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.BY_Description), userControl.DeclaredDescriptionTextBox.BindTo);
				AssertEquals("MultiLine", true, userControl.DeclaredDescriptionTextBox.Multiline);
				AssertEquals("Visible", true, userControl.DeclaredDescriptionTextBox.Visible);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, userControl.DeclaredDescriptionTextBox.CharacterCasing);
			});
		}

		public void TestDeclaredGrossWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource Amount", nameof(NctsArrivalCargoDesc.BY_GrossWeight), userControl.DeclaredGrossWeightDropEdit.BindToAmount);
				AssertEquals("BindingSource Unit", nameof(NctsArrivalCargoDesc.BY_GrossWeightUnit), userControl.DeclaredGrossWeightDropEdit.BindToUnit);
				AssertEquals("Visible", true, userControl.DeclaredGrossWeightDropEdit.Visible);
			});
		}

		[RequiresSTA]
		public void TestDeclaredNetWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.BY_NetWeight), userControl.DeclaredNetWeightDropEdit.BindToAmount);
				AssertEquals("BindingSource Unit", nameof(NctsArrivalCargoDesc.BY_NetWeightUnit), userControl.DeclaredNetWeightDropEdit.BindToUnit);
				AssertEquals("Visible", true, userControl.DeclaredNetWeightDropEdit.Visible);
			});
		}

		public void TestUnloadedValueLabel()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Unloaded Value", userControl.UnloadedValueLabel.CaptionResourceString.Caption);
				AssertEquals("Visible", true, userControl.UnloadedValueLabel.Visible);
			});
		}

		[RequiresSTA]
		public void TestUnloadedCommodityCode()
		{
			var header = Factory.New<NctsHeaderForTest>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = header.UnloadingAllowedOrCompleteStatusListExposed[0];
			movementHeader.BM_ValuationDate = new ZDateTime(2023, 1, 1);
			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

			using (var form = new Phase5ArrivalMovementForm(goodsItem.Header))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var unloadingRemarksTabPage = form.UnloadingRemarksTabPage;
				mainTabControl.SelectTab(unloadingRemarksTabPage);

				var unloadingRemarksTabUserControl = form.UnloadingRemarksTabUserControl;
				var houseConsignmentDifferencesTabPage = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabPage;
				unloadingRemarksTabUserControl.UnloadingRemarksTabControl.SelectTab(houseConsignmentDifferencesTabPage);

				var houseConsignmentDifferencesTabUserControl = unloadingRemarksTabUserControl.HouseConsignmentDifferencesTabUserControl;
				var goodsItemsTabPage = houseConsignmentDifferencesTabUserControl.GoodsItemsTabPage;
				houseConsignmentDifferencesTabUserControl.HouseConsignmentDifferencesTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemDifferencesTabUserControl = houseConsignmentDifferencesTabUserControl.Phase5GoodsItemDifferencesTabUserControl;
				var itemDetailsTabPage = phase5GoodsItemDifferencesTabUserControl.ItemDetailsTabPage;
				phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesTabControl.SelectTab(itemDetailsTabPage);

				var unloadedCommodityCodeCodeFindBox = phase5GoodsItemDifferencesTabUserControl.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.FindSingle<TariffFindBox>(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedCommodityCodeCodeFindBox));

				CombineAssertions(() =>
				{
					AssertType<TariffFindBox>(unloadedCommodityCodeCodeFindBox);
					AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_HarmonisedTariff), unloadedCommodityCodeCodeFindBox.BindTo);
					AssertEquals("Visible", true, unloadedCommodityCodeCodeFindBox.Visible);
					AssertEquals("TariffType", null, unloadedCommodityCodeCodeFindBox.TariffType);
					AssertEquals("PreBoundMaxLength", 8, unloadedCommodityCodeCodeFindBox.PreBoundMaxLength);
					AssertEquals("GetTariffType()", Constants.TariffTypes.Export, unloadedCommodityCodeCodeFindBox.GetTariffType?.Invoke());
					AssertEquals("GetEffectiveDate()", header.ArrivalMovementHeader.BM_ValuationDate, unloadedCommodityCodeCodeFindBox.GetEffectiveDate());
					AssertEquals("EffectiveTariffCountry", header.CountryCode, unloadedCommodityCodeCodeFindBox.EffectiveTariffCountry);
					AssertEquals("EffectiveDataGrouping", header.DefaultDataGroupingCode, unloadedCommodityCodeCodeFindBox.EffectiveDataGrouping);
					AssertEquals("BindToForDescription", "UnloadedGoodsItem.UniversalTariffDescription", unloadedCommodityCodeCodeFindBox.BindToForDescription);
				});
			}
		}

		public void TestUnloadedCusCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_CusC4Number), userControl.UnloadedCusCodeCodeFindBox.BindTo);
				AssertEquals("Visible", true, userControl.UnloadedCusCodeCodeFindBox.Visible);
			});
		}

		[RequiresSTA]
		public void TestUnloadedDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_Description), userControl.UnloadedDescriptionTextBox.BindTo);
				AssertEquals("MultiLine", true, userControl.UnloadedDescriptionTextBox.Multiline);
				AssertEquals("Visible", true, userControl.UnloadedDescriptionTextBox.Visible);
				AssertEquals("Character casing", System.Windows.Forms.CharacterCasing.Normal, userControl.UnloadedDescriptionTextBox.CharacterCasing);
			});
		}

		[RequiresSTA]
		public void TestUnloadedGrossWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource Amount", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_GrossWeight), userControl.UnloadedGrossWeightDropEdit.BindToAmount);
				AssertEquals("BindingSource Unit", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_GrossWeightUnit), userControl.UnloadedGrossWeightDropEdit.BindToUnit);
				AssertEquals("Visible", true, userControl.UnloadedGrossWeightDropEdit.Visible);
			});
		}

		[RequiresSTA]
		public void TestUnloadedNetWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_NetWeight), userControl.UnloadedNetWeightDropEdit.BindToAmount);
				AssertEquals("BindingSource Unit", nameof(NctsArrivalCargoDesc.UnloadedGoodsItem) + "." + nameof(NctsUnloadedCargoDesc.BY_NetWeightUnit), userControl.UnloadedNetWeightDropEdit.BindToUnit);
				AssertEquals("Visible", true, userControl.UnloadedNetWeightDropEdit.Visible);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemDifferencesDetailsColumnUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		Phase5GoodsItemDifferencesDetailsColumnUserControl userControl;
	}

	sealed class NctsHeaderForTest : NctsHeader
	{
		public NctsHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IReadOnlyList<ZString> UnloadingAllowedOrCompleteStatusListExposed => UnloadingAllowedOrCompleteStatusList;
	}
}
