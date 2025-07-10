using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
#if !WINZOR
	using Enterprise.Messaging.Integration;
	using Enterprise.ZArchitecture.Schema;

	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		public void TestGetHeaderAdditionalTabPageUserControls_CarrierManifest()
		{
			var header = CreateNewManifest();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			AssertHeaderAdditionalTabPageUserControls(header, ExpectedGetHeaderAdditionalTabPageUserControlsTypes_CarrierManifest);
		}

		[RequiresSTA]
		public void TestAdditionalSupplyChainActorTabVisibility()
		{
			TestBillsAndPacksTabControlTabVisibility("CusSupplyChainActorReferenceUserControl", ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Air, EUICS2SpecificCircumstanceList.Codes.F24);
		}

		public void TestSupplementaryDeclarantTabVisibility()
		{
			TestBillsAndPacksTabControlTabVisibility("SupplementaryDeclarantUserControl", ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Road, EUICS2SpecificCircumstanceList.Codes.F50);
			TestBillsAndPacksTabControlTabVisibility("SupplementaryDeclarantUserControl", ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Air, EUICS2SpecificCircumstanceList.Codes.F24);
		}

		public void TestAdditionalInformationTabVisibility()
		{
			TestBillsAndPacksTabControlTabVisibility("AdditionalInfoUserControl", ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine);
			TestBillsAndPacksTabControlTabVisibility("AdditionalInfoUserControl", ManifestBase.ApplicationCodeTypeList.Codes.Consolidator);
		}

		public void TestHRCMTabVisibility()
		{
			TestBillsAndPacksTabControlTabVisibility("HRCMScreeningResultUserControl", ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Road, EUICS2SpecificCircumstanceList.Codes.F50);
		}

		public void TestAdditionalFiscalReferenceTabVisibility()
		{
			TestBillsAndPacksTabControlTabVisibility("AdditionalFiscalReferenceUserControl", ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Rail, EUICS2SpecificCircumstanceList.Codes.F44);
			TestBillsAndPacksTabControlTabVisibility("AdditionalFiscalReferenceUserControl", ManifestBase.ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Road, EUICS2SpecificCircumstanceList.Codes.F44);
		}

		void TestBillsAndPacksTabControlTabVisibility(string tabName, string applicationCode = null, string transportMode = null, params string[] notVisibleForSpecificCircumstanceIndicatorValues)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = applicationCode;
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.EuropeanUnion;
			header.AMA_TransportMode = transportMode ?? header.AMA_TransportMode;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var tabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == $"billsAndPacksTabControl_TabPage_{tabName}");

				foreach (var item in header.Lookups.SpecificCircumstanceList.ToArray())
				{
					header.SpecificCircumstanceIndicator = item.Code;

					if (notVisibleForSpecificCircumstanceIndicatorValues.Contains(item.Code))
					{
						Assert($"Expected {tabName} to be invisible for SpecificCircumstanceIndicator = {item.Code}", !tabPage.TabVisible);
					}
					else
					{
						Assert($"Expected {tabName} to be visible for SpecificCircumstanceIndicator = {item.Code}", tabPage.TabVisible);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestReceptacleIdColumnVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.EuropeanUnion;
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				billsAndPacksTabPage.Show();
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				var columnStyle = billsGrid.GetColumnStyle("ReceptacleId");

				AssertNotNull(columnStyle);
				Assert("SpecificCircumstanceIndicator: F44, ReceptacleId is visible", columnStyle.IsVisible);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				Assert("SpecificCircumstanceIndicator: F43, ReceptacleId is not visible", !columnStyle.IsVisible);
			}
		}

		public void TestTariffColumnOnPacksGrid()
		{
			var header = CreateNewManifest();

			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var packsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_AsycudaPackUserControl");
				billsAndPacksTabControl.SelectedTab = packsTabPage;
				var asycudaPackUserControl = packsTabPage.FindSingleOrDefault<AsycudaPackUserControl>(c => c.Name == "AsycudaPackUserControl");
				var packsGrid = asycudaPackUserControl.FindSingle<ZGrid>(c => c.Name == "PacksGrid");

				var columnStyle = packsGrid.GetColumnStyle("PackedItem+API_FormattedTariff");
				CombineAssertions("Tariff column is visible and not mandatory", () =>
				{
					AssertNotNull(columnStyle);
					Assert(columnStyle.IsVisible);
					Assert(!columnStyle.IsMandatory);
					AssertEquals("Tariff", columnStyle.Caption);
				});
			}
		}

		public void TestAdditionalColumnsOnBillsGrid()
		{
			var header = CreateNewManifest();
			header.Bills.AddNew();
			Factory.Save();

			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				billsAndPacksTabPage.Show();
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				foreach (var (columnName, columnCaption) in BillsGridAdditionalColumnCaptions)
				{
					var columnStyle = billsGrid.GetColumnStyle(columnName);
					billsGrid.SetColumnVisible(true, columnName);
					AssertNotNull(columnStyle);
					Assert(!columnStyle.IsMandatory);

					var caption = billsGrid.Columns[columnName].ColumnStyle.HeaderText;

					AssertEquals(columnCaption, caption);
				}
			}
		}

		IEnumerable<(string, string)> BillsGridAdditionalColumnCaptions => new List<(string, string)>()
		{
			("BuyerOrgPK", "Buyer"),
			("ABL_OA_Buyer", "Buyer Address"),
			("ABL_BuyerName", "Buyer Name"),
			("ABL_BuyerStreet1", "Buyer Street 1"),
			("ABL_BuyerStreet2", "Buyer Street 2"),
			("ABL_BuyerCity", "Buyer City"),
			("ABL_BuyerState", "Buyer State"),
			("ABL_BuyerPhone", "Buyer Phone"),
			("ABL_BuyerPostcode", "Buyer Postcode"),
			("ABL_RN_NKBuyerCountry", "Buyer Country / Region"),

			("SellerOrgPK", "Seller"),
			("ABL_OA_Seller", "Seller Address"),
			("ABL_SellerName", "Seller Name"),
			("ABL_SellerStreet1", "Seller Street 1"),
			("ABL_SellerStreet2", "Seller Street 2"),
			("ABL_SellerCity", "Seller City"),
			("ABL_SellerState", "Seller State"),
			("ABL_SellerPhone", "Seller Phone"),
			("ABL_SellerPostcode", "Seller Postcode"),
			("ABL_RN_NKSellerCountry", "Seller Country / Region"),
		};

		[RequiresSTA]
		public void TestSendICS2TestMessage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			var codeType = "IC2MS";
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "DE", "Germany", startDate, endDate);
			Factory.Save();

			var header = CreateNewManifest();
			header.AddressedMemberState = Core.Constants.CountryCodes.Germany;
			Factory.Save();
			using var form = new ManifestForm(header);
			form.Show();

			CombineAssertions(() =>
			{
				var testMessageMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("ICS2 Test Message");
				AssertNotNull("ICS2 Test Message menu item exists", testMessageMenuItem);
				AssertNotNull("ICS2 Test Message menu item visible", testMessageMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); // Acknowledge "could not send" error
				testMessageMenuItem.PerformClick();

				AssertNull("No message was generated", Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK)));

				var savedInterchange = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, GlbBranch.CurrentBranch.PK));
				AssertEquals("One interchange was generated", 0, savedInterchange.Length);

				AssertEquals("No Declarant in Manifest or the Declarant does not have an EORI. Could not send.", UnitTestUserNotification.Instance.LastMessage.Text);

				header.Declarant.Header.CustomsCodes.AddNew("EOR", "234233", "DE");
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save and proceed
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, proceed with errors
				testMessageMenuItem.PerformClick();

				savedInterchange = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, GlbBranch.CurrentBranch.PK));
				AssertEquals("One interchange was saved", true, savedInterchange[0].IsInDatabase);
				AssertEquals("Interchange type is TST", Enterprise.Messaging.Integration.EDIInterchangeTypeList.Codes.TST, savedInterchange[0].EI_InterchangeType);
				AssertEquals("Interchange direction is TRX", ReceiveTransmitList.Codes.Transmit, savedInterchange[0].EI_ReceiveTransmit);
				AssertEquals("Interchange direction is ready to be sent", EDIInterchangeStatusList.Codes.Queued, savedInterchange[0].EI_Status);
			});
		}

		public void TestIsInEnforceOnlyValidTransportMode()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(true, provider.IsInEnforceOnlyValidTransportMode());
		}

		public void TestIsInEnforceOnlyValidSpecificCircumstanceIndicator()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(true, provider.IsInEnforceOnlyValidSpecificCircumstanceIndicator());
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[]
				{
					"BuyerOrgPK",
					"ABL_OA_Buyer",
					"ABL_BuyerName",
					"ABL_BuyerStreet1",
					"ABL_BuyerStreet2",
					"ABL_BuyerCity",
					"ABL_BuyerState",
					"ABL_BuyerPhone",
					"ABL_BuyerPostcode",
					"ABL_RN_NKBuyerCountry",
					"SellerOrgPK",
					"ABL_OA_Seller",
					"ABL_SellerName",
					"ABL_SellerStreet1",
					"ABL_SellerStreet2",
					"ABL_SellerCity",
					"ABL_SellerState",
					"ABL_SellerPhone",
					"ABL_SellerPostcode",
					"ABL_RN_NKSellerCountry",
					"ReceptacleId",
				}, columnInfos.Select(s => s.ColumnName));
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[]
		{
			typeof(AsycudaPackUserControl),
			typeof(SupportingDocumentsUserControl),
			typeof(AdditionalInfoUserControl),
			typeof(HRCMScreeningResultUserControl),
			typeof(CusSupplyChainActorReferenceUserControl),
			typeof(AdditionalFiscalReferenceUserControl),
			typeof(AsycudaTransportMeansUserControl),
			typeof(SupplementaryDeclarantUserControl)
		};

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(EUICS2BillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(EUICS2BillPartiesLayouts);

		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new[]
		{
			typeof(ItineraryForManifestHeaderUserControl),
			typeof(ReferralRequestUserControl)
		};

		Type[] ExpectedGetHeaderAdditionalTabPageUserControlsTypes_CarrierManifest =>
		[
			typeof(ItineraryForManifestHeaderUserControl),
			typeof(ReferralRequestUserControl),
			typeof(HeaderPartiesUserControl),
		];

		protected override Type[] ExpectedPackAdditionalTabPageUserControls => new[]
		{
			typeof(SupportingDocumentsUserControl),
			typeof(AdditionalInfoUserControl),
			typeof(CusSupplyChainActorReferenceUserControl),
			typeof(AsycudaTransportMeansUserControl)
		};

		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var eUICS2ManifestControlBag = EUICS2ManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[]
			{
				common.VoyageFlightTextBox,
				common.VesselCodeFindBox,
				eUICS2ManifestControlBag.MOTIdentifierTypeDropEdit,
				eUICS2ManifestControlBag.MOTIdentifierTextBox,
				common.LloydsNumberTextBox,
			});
			groups.Add("Road Transport", new[]
			{
				common.VehicleRegistrationTextBox,
				eUICS2ManifestControlBag.VehicleRegistrationAndNationalityUserControl,
				common.Trailer1RegNoTextBox,
				common.Trailer2RegNoTextBox,
				common.Trailer1RegCountryCodeFindBox,
				common.Trailer2RegCountryCodeFindBox
			});
			return groups;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AssertGetContainersGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
		{
			AssertEquals("Count", 1, columnAvailability.Count);

			var unavailableColumns = columnAvailability[key: false];
			var expectedUnavailableColumns = new[]
			{
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealType1,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealingPartyType,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealingPartyName,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealType2,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealingPartyType2,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealType3,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_SealingPartyType3,
				ManifestBase.AutoAsycudaContainer.Schema.ACN_StowageLocation,
			};

			AssertContainsExactElementsInAnyOrder("Unavailable Columns", expectedUnavailableColumns, unavailableColumns);
		}

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			var isShipperOwnedColumn = columnInfos.Single(i => i.ColumnName == ManifestBase.AutoAsycudaContainer.Schema.ACN_IsShipperOwned);

			AssertEquals(140, isShipperOwnedColumn.Width);
			AssertEquals(false, isShipperOwnedColumn.IsUnavailable);
		}

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.EuropeanUnion;
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.AMA_TransportMode = ZString.Empty;
			return header;
		}

		protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			(string ColumnName, int Width)[] expectedColumns = [
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_FormattedTariff)}", 90),
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_ChemicalSubstanceCode)}", 90),
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_GoodsValue)}", 90),
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency)}", 70),
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_TypeOfGoods)}", 90),
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_MessageStatus)}", 90),
				($"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_PackStatus)}", 90),
			];

			AssertContainsExactElementsInExactOrder(expectedColumns.Select(e => e.ColumnName), columnInfos.Select(s => s.ColumnName));

			foreach ((string columnName, int width)  in expectedColumns)
			{
				var columnInfo = columnInfos.Single(e => e.ColumnName == columnName);
				AssertEquals($"Width of {columnName}", width, columnInfo.Width);
			}
		}

		protected override void AssertGetPacksGridColumnAvailability(IReadOnlyDictionary<bool, string[]> result, AsycudaManifestHeader header)
		{
			AssertEquals("Count", 1, result.Count);

			var expectedColumns = new[]
			{
				$"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_GoodsValue)}",
				$"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency)}",
				$"{nameof(AsycudaPack.PackedItem)}+{nameof(AsycudaPackedItem.API_TypeOfGoods)}",
			};

			var unavailableColumns = result[key: false];
			AssertEquals("PreReq Unavailable", expected: false, header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled);
			AssertContainsExactElementsInAnyOrder("Unavailable columns", expectedColumns, unavailableColumns);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
			var availableColumns = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetPacksGridColumnAvailability(header)[key: true];
			AssertEquals("PreReq Available", expected: true, header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled);
			AssertContainsExactElementsInAnyOrder("Available columns", expectedColumns, availableColumns);
		}

		protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			AssertEquals(1, columnsVisiblility.Count);
			AssertContainsExactElementsInExactOrder(new[] { AsycudaBill.Schema.ReceptacleId }, columnsVisiblility.Keys);
		}
	}
#endif
}
