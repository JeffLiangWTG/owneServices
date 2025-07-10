using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaPackUserControlTest : TestCaseWithFactory
	{
		public void TestSetVisibility()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_TransportMode = "AIR";

			var bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");
				var packItemStandAloneCountryProperty = nameof(AsycudaPack.PackedItem) + "+";
				var tariffColumnStyle = packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_FormattedTariff) as Universal.GUI.TariffColumnStyleInfo;
				var tariffColumn = packsGrid.GetColumnStyle(AsycudaPackedItem.Schema.API_FormattedTariff) as Universal.GUI.TariffColumnStyleInfo;
				Assert("API_FormattedTariff", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_FormattedTariff).IsUnavailable);
				Assert("APA_VINNumber Visible", packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_VINNumber).IsUnavailable);
				Assert("API_GoodsDescription Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_GoodsDescription).IsUnavailable);
				Assert("API_CustomsQty Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsQty).IsUnavailable);
				Assert("API_CustomsUQ Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsUQ).IsUnavailable);
				Assert("API_CustomsValue Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsValue).IsUnavailable);
				Assert("API_DutyAmount Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_DutyAmount).IsUnavailable);
				Assert("API_TaxAmount Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_TaxAmount).IsUnavailable);
				Assert("API_RN_NKGoodsOrigin Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin).IsUnavailable);
				Assert("API_MessageStatus Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_MessageStatus).IsUnavailable);
				Assert("API_PackStatus Visible", !packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_PackStatus).IsUnavailable);

				AssertEquals(false, packsGrid.GetColumnStyle(AsycudaPack.Schema.LinePrice).IsVisible);
				AssertEquals(false, packsGrid.GetColumnStyle(AsycudaPack.Schema.LinePriceCurrency).IsVisible);
			}

			manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifest.AMA_TransportMode = "AIR";
			manifest.AMA_ManifestType = "ALH";

			bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packCountrySplitContainer = asycudaPackUserControl.FindSingle<CargoWise.Windows.UI.KSplitContainer>(control => control.Name == "PackCountrySplitContainer");
				AssertEquals(0, packCountrySplitContainer.Panel2.Controls.Count);
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");
				var packItemStandAloneCountryProperty = nameof(AsycudaPack.PackedItem) + "+";
				Assert("APA_VINNumber Visible", packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_VINNumber).IsUnavailable);
				AssertNull("API_GoodsDescription", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_GoodsDescription));
				AssertNull("API_CustomsQty", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsQty));
				AssertNull("API_CustomsUQ", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsUQ));
				AssertNull("API_CustomsValue", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsValue));
				AssertNull("API_DutyAmount", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_DutyAmount));
				AssertNull("API_TaxAmount", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_TaxAmount));
				AssertNull("API_RN_NKGoodsOrigin", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin));
				AssertNull("API_MessageStatus", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_MessageStatus));
				AssertNull("API_PackStatus", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_PackStatus));

				manifest.AMA_TransportMode = "ROA";
				manifest.AMA_ManifestType = "RFM";
				Assert("APA_VINNumber Visible", !packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_VINNumber).IsUnavailable);
				AssertNull("API_GoodsDescription", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_GoodsDescription));
				AssertNull("API_CustomsQty", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsQty));
				AssertNull("API_CustomsUQ", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsUQ));
				AssertNull("API_CustomsValue", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsValue));
				AssertNull("API_DutyAmount", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_DutyAmount));
				AssertNull("API_TaxAmount", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_TaxAmount));
				AssertNull("API_RN_NKGoodsOrigin", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin));
				AssertNull("API_MessageStatus", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_MessageStatus));
				AssertNull("API_PackStatus", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_PackStatus));

				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_ManifestType = "ECL";
				Assert("APA_VINNumber Visible", packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_VINNumber).IsUnavailable);
				AssertNull("API_GoodsDescription", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_GoodsDescription));
				AssertNull("API_CustomsQty", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsQty));
				AssertNull("API_CustomsUQ", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsUQ));
				AssertNull("API_CustomsValue", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_CustomsValue));
				AssertNull("API_DutyAmount", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_DutyAmount));
				AssertNull("API_TaxAmount", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_TaxAmount));
				AssertNull("API_RN_NKGoodsOrigin", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin));
				AssertNull("API_MessageStatus", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_MessageStatus));
				AssertNull("API_PackStatus", packsGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_PackStatus));
			}
		}

		public void TestSetPacksGridColumnsMandatory()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_TransportMode = "AIR";

			var bill = manifest.Bills.AddNew();
			bill.Packs.AddNew();

			CombineAssertions(() =>
			{
				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
					billsAndPacksTabControl.SelectedTab = packsTabPage;
					var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
					var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

					AssertEquals("For SG, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, APA_CommodityCode.IsMandatory", false, packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_CommodityCode).IsMandatory);
					AssertEquals("For SG, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, LinePrice.IsMandatory", false, packsGrid.GetColumnStyle(AsycudaPack.Schema.LinePrice).IsMandatory);
					AssertEquals("For SG, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, LinePrice.IsMandatory", false, packsGrid.GetColumnStyle(AsycudaPack.Schema.LinePriceCurrency).IsMandatory);
				}

				manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Italy;
				manifest.AMA_ManifestType = "MAN";

				bill = manifest.Bills.AddNew();
				bill.Packs.AddNew();

				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
					billsAndPacksTabControl.SelectedTab = packsTabPage;
					var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
					var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

					AssertEquals("For IT, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, APA_CommodityCode.IsMandatory", false, packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_CommodityCode).IsMandatory);
					AssertEquals("For IT, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, APA_LineNo.IsMandatory", false, packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_LineNo).IsMandatory);
					AssertEquals("For IT, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, UNDGSubstanceManagerValue.IsMandatory", false, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue).IsMandatory);
					AssertEquals("For IT, when GetPacksGridMandatoryOrderedColumns is default (null) columns mandatory attribute is default, UNDGClassManagerValue.IsMandatory", false, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue).IsMandatory);
				}

				manifest = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
				manifest.AMA_TransportMode = "AIR";
				manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
				manifest.AMA_ManifestType = "ICS";
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

				bill = manifest.Bills.AddNew();
				bill.Packs.AddNew();

				using (var form = new ManifestForm(manifest))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
					billsAndPacksTabControl.SelectedTab = packsTabPage;
					var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
					var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

					AssertEquals("For ES, when GetPacksGridMandatoryOrderedColumns is changed (not null) columns mandatory attribute is changed to true, APA_CommodityCode.IsMandatory", true, packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_CommodityCode).IsMandatory);
					AssertEquals("For ES, when GetPacksGridMandatoryOrderedColumns is changed (not null) columns mandatory attribute is changed to true, APA_LineNo.IsMandatory", true, packsGrid.GetColumnStyle(AsycudaPack.Schema.APA_LineNo).IsMandatory);
					AssertEquals("For ES, when GetPacksGridMandatoryOrderedColumns is changed (not null) columns mandatory attribute is changed to true, UNDGSubstanceManagerValue.IsMandatory", true, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue).IsMandatory);
					AssertEquals("For ES, when GetPacksGridMandatoryOrderedColumns is changed (not null) columns mandatory attribute is changed to true, UNDGClassManagerValue.IsMandatory", true, packsGrid.GetColumnStyle(UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue).IsMandatory);
				}
			});
		}

		public void TestIAdditionalTabPage()
		{
			CombineAssertions(() =>
			{
				AssertPacksTabPageUserControlVisible(Core.Constants.CountryCodes.Turkey, true, "ATAIHR");

				AssertPacksTabPageUserControlVisible(Core.Constants.CountryCodes.Singapore, true, "MGI");
				AssertPacksTabPageUserControlVisible(Core.Constants.CountryCodes.SouthAfrica, true, "ALH");
				AssertPacksTabPageUserControlVisible(Core.Constants.CountryCodes.Fiji, true, "ASY");
				AssertPacksTabPageUserControlVisible(Core.Constants.CountryCodes.UnitedStates, false, "IAM", assertMessage: "invisible for US AMS");
			});
		}

		public void TestIAdditionalTabPage_NZ()
		{
			AssertPacksTabPageUserControlVisible(Core.Constants.CountryCodes.NewZealand, true, "OCR", ApplicationCodeTypeList.Codes.ShippingLine);
		}

		void AssertPacksTabPageUserControlVisible(string countryCode, bool tabVisible, string manifestType, string applicationCode = ApplicationCodeTypeList.Codes.Consolidator, string assertMessage = "")
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ManifestType = manifestType;
				header.AMA_ApplicationCode = applicationCode;

				var bill = header.Bills.AddNew();

				using (var form = new ManifestForm(header))
				{
					form.Show();
					var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

					var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
					var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
					mainTabControl.SelectedTab = billsAndPacksTabPage;
					var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
					var additionalTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
					AssertEquals(countryCode + ":" + assertMessage, tabVisible, additionalTabPage?.TabVisible ?? false);
				}
			}
		}

		public void TestVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.India;
			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingle<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				CombineAssertions(() =>
				{
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.ContainerPK, false, false, true, CharacterCasing.Normal, "Container", 80);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_PackQty, false, true, true, CharacterCasing.Normal, "Quantity (on Pack)", 117);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_PackUQ, false, false, true, CharacterCasing.Normal, "Pack Unit", 68);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_CommodityCode, false, false, true, CharacterCasing.Upper, "Commodity Code", 106);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_GoodsDescription, false, false, true, CharacterCasing.Upper, "Goods' Description (on Pack)", 163);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_MarksAndNumbers, false, false, true, CharacterCasing.Upper, "Marks and Numbers (on Pack)", 170);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_Weight, false, false, true, CharacterCasing.Normal, "Weight (on Pack)", 110);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_WeightUQ, false, false, true, CharacterCasing.Upper, "Weight Unit", 80);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_Volume, false, false, true, CharacterCasing.Normal, "Volume (on Pack)", 112);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_VolumeUQ, false, false, true, CharacterCasing.Upper, "Volume Unit", 82);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_VINNumber, true, false, true, CharacterCasing.Upper, "VIN Number", 80);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.LinePrice, false, false, false, CharacterCasing.Normal, "Line Price", 80);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.LinePriceCurrency, false, false, false, CharacterCasing.Upper, "Currency", 80);
					AssertGridColumnInfo(packsGrid, AsycudaPack.Schema.APA_LineNo, false, false, false, CharacterCasing.Normal, "Seq. No.", 80);
				});
			}
		}

		void AssertGridColumnInfo(ZGrid containersGrid, string columnName, bool isUnavailable, bool isMandatory, bool isVisible, CharacterCasing characterCasing, string headerText, int width)
		{
			var gridColumnInfo = containersGrid.GetColumnStyle(columnName);

			AssertNotNull(columnName + " column template", gridColumnInfo);

			AssertEquals(columnName + " IsUnavailable", isUnavailable, gridColumnInfo.IsUnavailable);
			AssertEquals(columnName + " IsMandatory", isMandatory, gridColumnInfo.IsMandatory);
			AssertEquals(columnName + " IsVisible", isVisible, gridColumnInfo.IsVisible);
			AssertEquals(columnName + " CharacterCasing", characterCasing, gridColumnInfo.CharacterCasing);
			AssertEquals(columnName + " Width", width, gridColumnInfo.Width);

			var columnInstance = containersGrid.Columns.OfType<ZGridColumn>().SingleOrDefault(x => x.ColumnName == columnName);
			if (isUnavailable)
			{
				AssertNull(columnName + " column instance", columnInstance);
			}
			else
			{
				AssertNotNull(columnName + " column instance", columnInstance);

				if (headerText != null)
				{
					AssertEquals(columnName + " Column.Caption", headerText, columnInstance.ColumnStyle.HeaderText);
				}
			}
		}
	}
}
