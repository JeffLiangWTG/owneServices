using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTests : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(JPBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(JPBillPartiesLayout);

		public void TestBillsGridExtraColumns()
		{
			var header = CreateNewManifest();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");

				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");
				var customsWeightColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.CustomsWeight));
				var customsWeightUQColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.CustomsWeightUQ));
				var weightUQColumn = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GrossWeightUQ);
				var specialCargoCodeColumn = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SpecialCargoCode);
				var specialCargoCodeDescriptionColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.SpecialCargoCodeDescription));
				var messageStatusColumn = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MessageStatus);
				var messageStatusDescriptionColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.MessageStatusDescription));
				var customsStatusDescriptionColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_BillStatusDescription));
				var temporaryLandingStatusDescriptioncolumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.TemporaryLandingStatusDescription));
				var dischargePortColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_RL_NKPortOfDischarge));
				var tariffColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_Tariff));
				var tariffDescriptionColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.TariffDescription));
				var netWeightColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_NetWeight));
				var netWeightUQColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_NetWeightUQ));
				var customsNetWeightColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.CustomsNetWeight));
				var customsNetWeightUQColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.CustomsNetWeightUQ));
				var customsVolumeColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.CustomsVolume));
				var customsVolumeUQColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.CustomsVolumeUQ));
				var sequenceNumberColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_SequenceNumber));
				var customsStatusColumn = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillStatus);
				var billNumberColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_BillNumber));
				var finalDestinationColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_RL_NKFinalDestination));
				var quantityColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.ABL_ManifestQty));
				var weightColumn = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GrossWeight);
				var goodsDescriptionColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ABL_GoodsDescription));
				var cargoTypeColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ABL_CargoType));
				var goodsLocationColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ABL_GoodsLocation));
				var shipperOrgColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ShipperOrgPK));
				var shipperAddressColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ABL_OA_Shipper));
				var consigneeOrgColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ConsigneeOrgPK));
				var consigneeAddressColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.Schema.ABL_OA_Consignee));
				var temporaryLandingNumberColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.TemporaryLandingNumber));
				var temporaryLandingStatusColumn = billsGrid.GetColumnStyle(nameof(AsycudaBill.TemporaryLandingStatus));

				CombineAssertions(() =>
				{
					AssertEquals("Customs Weight", customsWeightColumn.GroupName.Caption);
					AssertEquals("Customs Weight", customsWeightUQColumn.GroupName.Caption);
					AssertEquals("Gross Weight", weightColumn.GroupName.Caption);
					AssertEquals("Gross Weight", weightUQColumn.GroupName.Caption);
					AssertEquals("Net Weight", netWeightColumn.GroupName.Caption);
					AssertEquals("Net Weight", netWeightUQColumn.GroupName.Caption);
					AssertEquals("Customs Net Weight", customsNetWeightColumn.GroupName.Caption);
					AssertEquals("Customs Net Weight", customsNetWeightUQColumn.GroupName.Caption);
					AssertEquals("Customs Volume", customsVolumeColumn.GroupName.Caption);
					AssertEquals("Customs Volume", customsVolumeUQColumn.GroupName.Caption);
					AssertEquals("Tariff", tariffColumn.GroupName.Caption);
					AssertEquals("Tariff", tariffDescriptionColumn.GroupName.Caption);
					AssertEquals("Temporary Landing", temporaryLandingNumberColumn.GroupName.Caption);
					AssertEquals("Temporary Landing", temporaryLandingStatusColumn.GroupName.Caption);
					AssertEquals("ABL_Calc_WeightInKG", true, customsWeightColumn.IsVisible);
					AssertEquals("ABL_Calc_WeightInKG", true, customsWeightUQColumn.IsVisible);
					AssertEquals("SpecialCargoCodeColumn should be visible", true, specialCargoCodeColumn.IsVisible);
					AssertEquals("SpecialCargoCodeDescriptionColumn not be visible by default", false, specialCargoCodeDescriptionColumn.IsVisible);
					AssertEquals("MessageStatusColumn should not be visible", false, messageStatusColumn.IsVisible);
					AssertEquals("MessageStatusDescriptionColumn not be visible by default", false, messageStatusDescriptionColumn.IsVisible);
					AssertEquals("Customs Status Description column should not be visible.", false, customsStatusDescriptionColumn.IsVisible);
					AssertEquals("Temporary Landing Status description column should not be visible.", false, temporaryLandingStatusDescriptioncolumn.IsVisible);
					AssertEquals("ABL_RL_NKPortOfDischarge should not be visible", false, dischargePortColumn.IsVisible);
					AssertEquals("ABL_Tariff should not be visible", false, tariffColumn.IsVisible);
					AssertEquals("TariffDescription should not be visible", false, tariffDescriptionColumn.IsVisible);
					AssertEquals("ABL_NetWeight should not be visible", false, netWeightColumn.IsVisible);
					AssertEquals("ABL_NetWeightUQ should not be visible", false, netWeightUQColumn.IsVisible);
					AssertEquals("Sequence Number column should be visible.", true, sequenceNumberColumn.IsVisible);
					AssertEquals("Customs Status column should be visible.", true, customsStatusColumn.IsVisible);
					AssertEquals("Bill Number column should be visible.", true, billNumberColumn.IsVisible);
					AssertEquals("Final Destination column should be visible.", true, finalDestinationColumn.IsVisible);
					AssertEquals("Quantity column should be visible.", true, quantityColumn.IsVisible);
					AssertEquals("Weight column should be visible.", true, weightColumn.IsVisible);
					AssertEquals("Goods Description column should be visible.", true, goodsDescriptionColumn.IsVisible);
					AssertEquals("Cargo Type column should not be visible.", false, cargoTypeColumn.IsVisible);
					AssertEquals("Goods Location Column should be visible.", true, goodsLocationColumn.IsVisible);
					AssertEquals("Shipper Org column should not be visible.", false, shipperOrgColumn.IsVisible);
					AssertEquals("Shipper Address column should not be visible.", false, shipperAddressColumn.IsVisible);
					AssertEquals("Consignee Org column should not be visible.", false, consigneeOrgColumn.IsVisible);
					AssertEquals("Consignee Address column should not be visible.", false, consigneeAddressColumn.IsVisible);
				});
			}
		}

		public void TestBillsGridColumnAvailability()
		{
			var header = CreateNewManifest();
			header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
				AssertBillsGridColumnAvailability();
				header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
				AssertBillsGridColumnAvailability();
				header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
				AssertBillsGridColumnAvailability();

				void AssertBillsGridColumnAvailability()
				{
					CombineAssertions("Column Visibilities", () =>
					{
						billsGridColumnsVisibility.ForEach(column =>
						{
							var manifestType = header.AMA_ManifestType;
							var columnName = column.Key;
							if (column.Value.Contains(manifestType))
							{
								Assert($"{columnName} IsVisible for {manifestType}", billsGrid.GetColumnStyle(columnName).IsVisible);
							}
							else
							{
								Assert($"{columnName} IsUnavailable for {manifestType}", billsGrid.GetColumnStyle(columnName).IsUnavailable);
							}
						});
					});
				}
			}
		}

		protected override void AssertGetBillPartiesTabPageVisibility()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Assert("Bill Parties Tab Page should not be visible when HDF01", !provider.GetBillPartiesTabPageVisibility(header));

			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Assert("Bill Parties Tab Page should be visible when not HDF01", provider.GetBillPartiesTabPageVisibility(header));
		}

		protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			AssertEquals(41, columnsVisiblility.Count);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] { "CustomsWeight", "CustomsWeightUQ", "ABL_SpecialCargoCode", "SpecialCargoCodeDescription", "ABL_CargoType", "ABL_MessageStatus", "MessageStatusDescription", "ABL_BillStatus", "ABL_BillStatusDescription", "ABL_RL_NKPortOfDischarge", "ABL_Tariff", "TariffDescription", "ABL_NetWeight", "ABL_NetWeightUQ", "CustomsNetWeight", "CustomsNetWeightUQ", "CustomsVolume", "CustomsVolumeUQ", "ABL_GoodsLocation", "TemporaryLandingNumber", "TemporaryLandingStatus", "ABL_CountryOfOrigin", "TemporaryLandingStatusDescription" }, columnInfos.Select(s => s.ColumnName));
		}

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInAnyOrder(new[] { AsycudaContainer.Schema.NACCSContainerType, AsycudaContainer.Schema.NACCSContainerSize, AsycudaContainer.Schema.CustomsTareWeight, AsycudaContainer.Schema.CustomsWeightUQ, AsycudaContainer.Schema.ACN_MoveOutDate, AsycudaContainer.Schema.VanningLocationCode }, columnInfos.Select(x => x.ColumnName));
		}

		protected override void AssertGetContainersGridColumnsOrder(string[] columnsOrder)
		{
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaContainer.Schema.ACN_ContainerNumber,
					AsycudaContainer.Schema.ACN_EmptyFullIndicator,
					AsycudaContainer.Schema.ACN_RC_ContainerType,
					AsycudaContainer.Schema.NACCSContainerSize,
					AsycudaContainer.Schema.NACCSContainerType,
					AsycudaContainer.Schema.ACN_Seal1,
					AsycudaContainer.Schema.ACN_SealType1,
					AsycudaContainer.Schema.ACN_SealingPartyType,
					AsycudaContainer.Schema.ACN_SealingPartyName,
					AsycudaContainer.Schema.ACN_NumberOfPackages,
					AsycudaContainer.Schema.ACN_CommodityCode,
					AsycudaContainer.Schema.ACN_GoodsWeight,
					AsycudaContainer.Schema.ACN_GoodsWeightUQ,
					AsycudaContainer.Schema.ACN_StowageLocation,
					AsycudaContainer.Schema.CustomsTareWeight,
					AsycudaContainer.Schema.CustomsWeightUQ,
					AsycudaContainer.Schema.ACN_MoveOutDate,
				},
				columnsOrder);
		}

		protected override Type ExpectedContainerUserControlType => typeof(JPAsycudaContainerUserControl);

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => [typeof(JPTemporaryLandingUserControl)];

		protected override void AssertGetMessagesGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			CombineAssertions(() =>
			{
				AssertEquals(4, columnInfos.Length);
				AssertEquals("EM_Calc_ProcedureCode", Common.EDIMessage.Schema.EM_Calc_ProcedureCode, columnInfos[0].ColumnName);
				AssertEquals("EM_Calc_ProcedureName", Common.EDIMessage.Schema.EM_Calc_ProcedureName, columnInfos[1].ColumnName);
				AssertEquals("EM_Calc_OutputInformationCode", Common.EDIMessage.Schema.EM_Calc_OutputInformationCode, columnInfos[2].ColumnName);
				AssertEquals("EM_Calc_OutputInformation", Common.EDIMessage.Schema.EM_Calc_OutputInformation, columnInfos[3].ColumnName);
			});
		}

		protected override void AssertGetMessagesGridOrder(string[] columns)
		{
			CombineAssertions(() =>
			{
				AssertEquals(11, columns.Length);
				AssertEquals("EM_Calc_ProcedureCode", Common.EDIMessage.Schema.EM_Calc_ProcedureCode, columns[0]);
				AssertEquals("EM_Calc_ProcedureName", Common.EDIMessage.Schema.EM_Calc_ProcedureName, columns[1]);
				AssertEquals("EM_Calc_OutputInformationCode", Common.EDIMessage.Schema.EM_Calc_OutputInformationCode, columns[2]);
				AssertEquals("EM_Calc_OutputInformation", Common.EDIMessage.Schema.EM_Calc_OutputInformation, columns[3]);
				AssertEquals("EM_MessageNum", Common.EDIMessage.Schema.EM_MessageNum, columns[4]);
				AssertEquals("EM_Status", Common.EDIMessage.Schema.EM_Status, columns[5]);
				AssertEquals("EM_MessageDateTime", Common.EDIMessage.Schema.EM_MessageDateTime, columns[6]);
				AssertEquals("EM_SystemCreateTimeUtc", Common.EDIMessage.Schema.EM_SystemCreateTimeUtc, columns[7]);
				AssertEquals("EM_SystemCreateUser", Common.EDIMessage.Schema.EM_SystemCreateUser, columns[8]);
				AssertEquals("EM_ReceiveTransmit", Common.EDIMessage.Schema.EM_ReceiveTransmit, columns[9]);
				AssertEquals("EM_MessageType", Common.EDIMessage.Schema.EM_MessageType, columns[10]);
			});
		}

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header;
		}

		protected override int MaxColumnsOfManifestLayout => 3;

		public void TestIsInEnforceOnlyValidTransportMode()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			Assert(!provider.IsInEnforceOnlyValidTransportMode());
		}

		readonly Dictionary<string, ImmutableHashSet<string>> billsGridColumnsVisibility = new Dictionary<string, ImmutableHashSet<string>>()
		{
			{
				AsycudaBill.Schema.ABL_BolType, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_RL_NKOrigin, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.ABL_Volume, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.CustomsVolume), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_MarksAndNumbers, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_Remarks, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.VAN
				)
			},
			{
				AsycudaBill.Schema.ABL_UCRNumber, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.CustomsJobNumber, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.VAN
				)
			},
			{
				nameof(AsycudaBill.ABL_CountryOfOrigin), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.VAN,
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.Schema.ABL_GoodsDescription), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.VAN,
					JPManifestTypeCodeList.Codes.HCH,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				AsycudaBill.Schema.ABL_SpecialCargoCode, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				AsycudaBill.Schema.ABL_RL_NKPortOfDischarge, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.ABL_Tariff), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_NetWeight, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.CustomsNetWeight), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_VolumeUQ, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.CustomsVolumeUQ), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.TariffDescription), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_NetWeightUQ, ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.CustomsNetWeightUQ), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.ABL_CargoType), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				nameof(AsycudaBill.ABL_BillStatusDescription), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.VAN,
					JPManifestTypeCodeList.Codes.HDF
				)
			},
			{
				nameof(AsycudaBill.TemporaryLandingStatusDescription), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.ABL_ManifestUQ), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.VAN,
					JPManifestTypeCodeList.Codes.HDF,
					JPManifestTypeCodeList.Codes.NVC,
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				nameof(AsycudaBill.ABL_GoodsLocation), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.HCH
				)
			},
			{
				nameof(AsycudaBill.ShipperOrgPK), ImmutableHashSet.Create<string>()
			},
			{
				nameof(AsycudaBill.ABL_OA_Shipper), ImmutableHashSet.Create<string>()
			},
			{
				nameof(AsycudaBill.ConsigneeOrgPK), ImmutableHashSet.Create<string>()
			},
			{
				nameof(AsycudaBill.ABL_OA_Consignee), ImmutableHashSet.Create<string>()
			},
			{
				nameof(AsycudaBill.TemporaryLandingNumber), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				nameof(AsycudaBill.TemporaryLandingStatus), ImmutableHashSet.Create(
					JPManifestTypeCodeList.Codes.NVC
				)
			},
			{
				AsycudaBill.Schema.ABL_CarrierReference, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.ABL_PrepaidCollect, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.ABL_InsuranceValue, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.DiscountValue, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.DiscountValueCurrency, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.OtherChargesValue, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.OtherChargesValueCurrency, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.ABL_CustomsValue, ImmutableHashSet.Create<string>()
			},
			{
				AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency, ImmutableHashSet.Create<string>()
			},
		};
	}
}
