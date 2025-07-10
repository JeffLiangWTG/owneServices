using System;
using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMApplicationGUIProvider))]
sealed class CGMApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<CGMApplicationGUIProvider, CGMAsycudaManifestHeader>
{
	public void TestManifestLayoutType()
	{
		var provider = GetProvider();
		AssertType<CGMManifestLayout>("Type", provider.GetManifestLayout());
	}

	protected override void AssertGetContainersGridColumnAvailability(IReadOnlyDictionary<bool, string[]> columnAvailability)
	{
		AssertEquals("Count", 1, columnAvailability.Count);

		var unavailableColumns = columnAvailability[key: false];
		var expectedUnavailableColumns = new[]
		{
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealType1,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealingPartyType,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealingPartyName,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_CommodityCode,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_StowageLocation,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal2,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealingPartyType2,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealType2,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_Seal3,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealingPartyType3,
			ASYCUDA.Business.AsycudaContainer.Schema.ACN_SealType3
		};

		AssertContainsExactElementsInAnyOrder("Unavailable Columns", expectedUnavailableColumns, unavailableColumns);
	}

	public void TestGetBillsGridColumnAvailabilityAir()
	{
		var header = CreateNewManifest();
		header.AMA_ManifestType = INManifestTypes.Codes.CGM;
		header.AMA_TransportMode = TransportTypeList.Codes.Air;
		var columnAvailability = CGMApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridColumnAvailability(header);
		AssertEquals("Count", 1, columnAvailability.Count);
		AssertEquals("False key check", expected: true, columnAvailability.ContainsKey(false));

		var unavailableColumns = columnAvailability[key: false];
		var expectedUnavailableColumns = new[]
		{
			CGMAsycudaBill.Schema.ABL_BolType,
			CGMAsycudaBill.Schema.NotifyPartyOrgPK,
			CGMAsycudaBill.Schema.ABL_OA_NotifyParty,
			CGMAsycudaBill.Schema.ABL_NotifyPartyName,
			CGMAsycudaBill.Schema.ABL_NotifyPartyStreet1,
			CGMAsycudaBill.Schema.ABL_NotifyPartyStreet2,
			CGMAsycudaBill.Schema.ABL_NotifyPartyCity,
			CGMAsycudaBill.Schema.ABL_NotifyPartyState,
			CGMAsycudaBill.Schema.ABL_NotifyPartyPostcode,
			CGMAsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry,
			CGMAsycudaBill.Schema.ABL_NotifyPartyPhone,
			CGMAsycudaBill.Schema.ABL_UCRNumber,
			CGMAsycudaBill.Schema.CustomsJobNumber,
			CGMAsycudaBill.Schema.ABL_CustomsValue,
			CGMAsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
			CGMAsycudaBill.Schema.DiscountValue,
			CGMAsycudaBill.Schema.DiscountValueCurrency,
			CGMAsycudaBill.Schema.ABL_TransportValue,
			CGMAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
			CGMAsycudaBill.Schema.ABL_FreightValue,
			CGMAsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
			CGMAsycudaBill.Schema.ABL_InsuranceValue,
			CGMAsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
			CGMAsycudaBill.Schema.OtherChargesValue,
			CGMAsycudaBill.Schema.OtherChargesValueCurrency,
			CGMAsycudaBill.Schema.ABL_CargoStatus,
		};

		AssertContainsExactElementsInAnyOrder("Unavailable Columns", expectedUnavailableColumns, unavailableColumns);
	}

	public void TestGetBillsGridColumnAvailabilitySea()
	{
		var header = CreateNewManifest();
		header.AMA_ManifestType = INManifestTypes.Codes.CGM;
		header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		var columnAvailability = CGMApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillsGridColumnAvailability(header);
		AssertEquals("Count", 2, columnAvailability.Count);

		var unavailableColumns = columnAvailability[key: false];
		var availableColumns = columnAvailability[key: true];

		var expectedAvailableColumns = new[]
		{
			CGMAsycudaBill.Schema.ABL_CargoStatus
		};

		var expectedUnavailableColumns = new[]
		{
			CGMAsycudaBill.Schema.ABL_BolType,
			CGMAsycudaBill.Schema.NotifyPartyOrgPK,
			CGMAsycudaBill.Schema.ABL_OA_NotifyParty,
			CGMAsycudaBill.Schema.ABL_NotifyPartyName,
			CGMAsycudaBill.Schema.ABL_NotifyPartyStreet1,
			CGMAsycudaBill.Schema.ABL_NotifyPartyStreet2,
			CGMAsycudaBill.Schema.ABL_NotifyPartyCity,
			CGMAsycudaBill.Schema.ABL_NotifyPartyState,
			CGMAsycudaBill.Schema.ABL_NotifyPartyPostcode,
			CGMAsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry,
			CGMAsycudaBill.Schema.ABL_NotifyPartyPhone,
			CGMAsycudaBill.Schema.ABL_UCRNumber,
			CGMAsycudaBill.Schema.CustomsJobNumber,
			CGMAsycudaBill.Schema.ABL_CustomsValue,
			CGMAsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
			CGMAsycudaBill.Schema.DiscountValue,
			CGMAsycudaBill.Schema.DiscountValueCurrency,
			CGMAsycudaBill.Schema.ABL_TransportValue,
			CGMAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
			CGMAsycudaBill.Schema.ABL_FreightValue,
			CGMAsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
			CGMAsycudaBill.Schema.ABL_InsuranceValue,
			CGMAsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
			CGMAsycudaBill.Schema.OtherChargesValue,
			CGMAsycudaBill.Schema.OtherChargesValueCurrency
		};

		AssertContainsExactElementsInAnyOrder("Unavailable Columns", expectedUnavailableColumns, unavailableColumns);
		AssertContainsExactElementsInAnyOrder("Available Columns", expectedAvailableColumns, availableColumns);
	}

	protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		CombineAssertions(() =>
		{
			AssertGridColumnInfo<ZCheckBoxColumnStyleInfo>(columnInfos, CGMAsycudaContainer.Schema.ACN_IsShipperOwned, 100, isVisible: true, additionalAsserts: column => AssertEquals("IsUnavailable", false, column.IsUnavailable));

			AssertGridColumnInfo<ZOrganisationFindBoxColumnStyleInfo>(columnInfos, CGMAsycudaContainer.Schema.ContainerAgentCodeOrgPK, 120, isVisible: true, additionalAsserts: column => AssertEquals("GroupName", "Container Agent Code", column.GroupName.Caption));

			AssertGridColumnInfo<ZGuidDropEditColumnStyleInfo>(columnInfos, CGMAsycudaContainer.Schema.ContainerAgentCode, 162, isVisible: true, additionalAsserts: column =>
			{
				AssertEquals("GroupName", "Container Agent Code", column.GroupName.Caption);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, column.CharacterCasing);
			});

			AssertGridColumnInfo<ZDropEditColumnStyleInfo>(columnInfos, CGMAsycudaContainer.Schema.ISOCode, 120, isVisible: true);
		});
	}

	public void TestGetBillsGridColumnsOrder()
	{
		var columnsOrder = GetProvider().GetBillsGridColumnsOrder();
		var expectedColumnsOrder = GetBillsGridExpectedColumnsOrder();

		AssertContainsExactElementsInExactOrder(expectedColumnsOrder, columnsOrder);
	}

	public void TestBillsGridRefreshColumnHeaders()
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

			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("ABL_SpecialCargoCode Caption for Sea", "Item Type", billsGrid.Columns[CGMAsycudaBill.Schema.ABL_SpecialCargoCode].ColumnStyle.HeaderText);

			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("ABL_SpecialCargoCode Caption for Sea", "Shipment Type", billsGrid.Columns[CGMAsycudaBill.Schema.ABL_SpecialCargoCode].ColumnStyle.HeaderText);
		}
	}

	public void TestSetBillsGridColumnCaptions()
	{
		var header = CreateNewManifest();
		using var form = new ManifestForm(header);
		form.Show();
		var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
		var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
		var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
		mainTabControl.SelectedTab = billsAndPacksTabPage;
		var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

		AssertColumnCaptions(CGMAsycudaBill.Schema.ABL_ManifestQty, CGMAsycudaBill.Schema.ABL_ManifestUQ, "Packages", "Packages", "Pack.");
		AssertColumnCaptions(CGMAsycudaBill.Schema.ABL_GrossWeight, CGMAsycudaBill.Schema.ABL_GrossWeightUQ, "Gross Weight", "Gross Wt.", "Gross Wt.");
		AssertColumnCaptions(CGMAsycudaBill.Schema.ABL_Volume, CGMAsycudaBill.Schema.ABL_VolumeUQ, "Volume", "Volume", "Vol.");

		void AssertColumnCaptions(string columnName, string groupColumnName, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			var columnInfo = billsGrid.GetColumnStyle(columnName);
			AssertCaptionData(columnInfo?.CaptionResourceString, expectedCaption, expectedMediumCaption, expectedShortCaption);
			AssertCaptionData(columnInfo?.GroupName, expectedCaption, expectedMediumCaption, expectedShortCaption);

			var groupColumnInfo = billsGrid.GetColumnStyle(groupColumnName);
			AssertCaptionData(groupColumnInfo?.GroupName, expectedCaption, expectedMediumCaption, expectedShortCaption);
		}

		void AssertCaptionData(ResourceStringData resString, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			AssertEquals("Caption", expectedCaption, resString?.Caption);
			AssertEquals("MediumCaption", expectedMediumCaption, resString?.MediumCaption);
			AssertEquals("ShortCaption", expectedShortCaption, resString?.ShortCaption);
		}
	}

	protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		var expectedBillsGridExtraColumns = new[]
		{
			CGMAsycudaBill.Schema.ABL_BillIssueDate,
			CGMAsycudaBill.Schema.ABL_SpecialCargoCode,
			CGMAsycudaBill.Schema.BuyerOrgPK,
			CGMAsycudaBill.Schema.ABL_OA_Buyer,
			CGMAsycudaBill.Schema.ABL_BuyerName,
			CGMAsycudaBill.Schema.ABL_BuyerStreet1,
			CGMAsycudaBill.Schema.ABL_BuyerStreet2,
			CGMAsycudaBill.Schema.ABL_BuyerCity,
			CGMAsycudaBill.Schema.ABL_BuyerState,
			CGMAsycudaBill.Schema.ABL_BuyerPhone,
			CGMAsycudaBill.Schema.ABL_BuyerPostcode,
			CGMAsycudaBill.Schema.ABL_RN_NKBuyerCountry,
			CGMAsycudaBill.Schema.ABL_BillStatus,
			CGMAsycudaBill.Schema.MessageStatusDescription,
		};

		AssertContainsExactElementsInExactOrder(expectedBillsGridExtraColumns, columnInfos.Select(s => s.ColumnName));

		CombineAssertions(() =>
		{
			AssertBillIssueDateColumnInfo(columnInfos);

			AssertGridColumnInfo<ZDropEditColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_SpecialCargoCode), 100, isVisible: true);

			AssertGridColumnInfo<ZOrganisationFindBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.BuyerOrgPK), 80, additionalAsserts: column =>
			{
				AssertEquals("GroupName", "Importer", column.GroupName.Caption);
				AssertEquals("Caption", "Importer", column.CaptionResourceString.Caption);
			});
			AssertGridColumnInfo<ZGuidDropEditColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_OA_Buyer), 90, additionalAsserts: column =>
			{
				AssertEquals("GroupName", "Importer", column.GroupName.Caption);
				AssertEquals("Caption", "Importer Address", column.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Address", column.CaptionResourceString.ShortCaption);
				AssertEquals("FullDescription", "", column.CaptionResourceString.FullDescription);
			});
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerName), 90);
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerStreet1), 100);
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerStreet2), 100);
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerCity), 80);
			AssertGridColumnInfo<ZDropEditColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerState), 90);
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerPhone), 90);
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BuyerPostcode), 90);
			AssertGridColumnInfo<ZCodeFindBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_RN_NKBuyerCountry), 100);
			AssertGridColumnInfo<ZDropEditColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.ABL_BillStatus), 90, true, additionalAsserts: column =>
			{
				AssertEquals("IsMandatory", true, column.IsMandatory);
			});
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, nameof(CGMAsycudaBill.MessageStatusDescription), 90, true, additionalAsserts: column =>
			{
				AssertEquals("IsMandatory", true, column.IsMandatory);
			});
		});
	}

	protected override void AssertGetPacksGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
	{
		CombineAssertions(() =>
		{
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, CGMAsycudaPack.Schema.ContainerStatus, 90, isVisible: true, additionalAsserts: column =>
			{
				AssertEquals("ShortCaption", "Cont. St.", column.CaptionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Cont. Status", column.CaptionResourceString.MediumCaption);
				AssertEquals("Caption", "Container Status", column.CaptionResourceString.Caption);
				AssertEquals("Is Read Only", expected: true, column.IsReadOnly);
			});
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, CGMAsycudaPack.Schema.SealNo, 90, isVisible: true, additionalAsserts: column =>
			{
				AssertEquals("Caption", "Seal No", column.CaptionResourceString.Caption);
				AssertEquals("Is Read Only", expected: true, column.IsReadOnly);
			});
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, CGMAsycudaPack.Schema.ISOCode, 90, isVisible: true, additionalAsserts: column =>
			{
				AssertEquals("Caption", "ISO Code", column.CaptionResourceString.Caption);
				AssertEquals("Is Read Only", expected: true, column.IsReadOnly);
			});
			AssertGridColumnInfo<ZCheckBoxColumnStyleInfo>(columnInfos, CGMAsycudaPack.Schema.SOCFlag, 90, isVisible: true, additionalAsserts: column =>
			{
				AssertEquals("Caption", "SOC Flag", column.CaptionResourceString.Caption);
				AssertEquals("Is Read Only", expected: true, column.IsReadOnly);
			});
			AssertGridColumnInfo<ZTextBoxColumnStyleInfo>(columnInfos, CGMAsycudaPack.Schema.ContainerAgentPAN, 90, isVisible: true, additionalAsserts: column =>
			{
				AssertEquals("Caption", "Container Agent Code", column.CaptionResourceString.Caption);
				AssertEquals("Is Read Only", expected: true, column.IsReadOnly);
			});
		});
	}

	protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

	protected override Type ExpectedBillLayoutType => typeof(CGMBillLayout);

	protected override Type ExpectedBillPartiesLayoutType => typeof(CGMBillPartiesLayout);

	protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl) };

	IEnumerable<string> GetBillsGridExpectedColumnsOrder()
	{
		yield return CGMAsycudaBill.Schema.ABL_SequenceNumber;
		yield return CGMAsycudaBill.Schema.ABL_BillNumber;
		yield return CGMAsycudaBill.Schema.ABL_BillIssueDate;
		yield return CGMAsycudaBill.Schema.ABL_RL_NKOrigin;
		yield return CGMAsycudaBill.Schema.ABL_RL_NKFinalDestination;
		yield return CGMAsycudaBill.Schema.ABL_GoodsDescription;
		yield return CGMAsycudaBill.Schema.ABL_ManifestQty;
		yield return CGMAsycudaBill.Schema.ABL_ManifestUQ;
		yield return CGMAsycudaBill.Schema.ABL_GrossWeight;
		yield return CGMAsycudaBill.Schema.ABL_GrossWeightUQ;
		yield return CGMAsycudaBill.Schema.ABL_Volume;
		yield return CGMAsycudaBill.Schema.ABL_VolumeUQ;
		yield return CGMAsycudaBill.Schema.ABL_SpecialCargoCode;
		yield return CGMAsycudaBill.Schema.ABL_MarksAndNumbers;
		yield return CGMAsycudaBill.Schema.ABL_Remarks;
	}

	ApplicationGUIProvider GetProvider() => CGMApplicationGUIProvider.GetApplicationGuiProvider(CreateNewManifest());

	void AssertGridColumnInfo<T>(ZGridColumnInfo[] columnInfos, string columnName, int expectedColumnWidth, bool isVisible = false, Action<T> additionalAsserts = null)
		where T : ZGridColumnInfo
	{
		var machedColumn = columnInfos.FirstOrDefault(x => x.ColumnName == columnName) as T;
		AssertNotNull(columnName, machedColumn);
		AssertEquals(columnName + " IsVisible", isVisible, machedColumn?.IsVisible);
		AssertEquals("Width", expectedColumnWidth, machedColumn?.Width);
		additionalAsserts?.Invoke(machedColumn);
	}

	void AssertBillIssueDateColumnInfo(ZGridColumnInfo[] columnInfos)
	{
		var column = columnInfos.FirstOrDefault(x => x.ColumnName == nameof(CGMAsycudaBill.Schema.ABL_BillIssueDate));
		AssertNotNull(nameof(CGMAsycudaBill.Schema.ABL_BillIssueDate), column);
		AssertEquals("Mandatory", expected: true, column.IsMandatory);
	}

	protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
	{
		var expectedColumnsOrder = new[]
		{
			CGMAsycudaPack.Schema.ContainerPK,
			CGMAsycudaPack.Schema.ContainerStatus,
			CGMAsycudaPack.Schema.APA_PackQty,
			CGMAsycudaPack.Schema.APA_Weight,
			CGMAsycudaPack.Schema.APA_WeightUQ,
			CGMAsycudaPack.Schema.SealNo,
			CGMAsycudaPack.Schema.ISOCode,
			CGMAsycudaPack.Schema.SOCFlag,
			CGMAsycudaPack.Schema.ContainerAgentPAN,
			CGMAsycudaPack.Schema.APA_CommodityCode,
			CGMAsycudaPack.Schema.APA_GoodsDescription,
			CGMAsycudaPack.Schema.APA_MarksAndNumbers,
			CGMAsycudaPack.Schema.APA_Volume,
			CGMAsycudaPack.Schema.APA_VolumeUQ,
			CGMAsycudaPack.Schema.APA_VINNumber,
			CGMAsycudaPack.Schema.LinePrice,
			CGMAsycudaPack.Schema.LinePriceCurrency,
			CGMAsycudaPack.Schema.APA_LineNo,
			UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
			UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
		};
		AssertContainsExactElementsInExactOrder(expectedColumnsOrder, columnsOrder);
	}

	protected override void AssertGetPacksGridColumnVisibility(IReadOnlyDictionary<bool, string[]> columnVisibility)
	{
		AssertContainsExactElementsInAnyOrder(new[] { false }, columnVisibility.Keys);
		AssertContainsExactElementsInAnyOrder(
			new[]
			{
					CGMAsycudaPack.Schema.APA_CommodityCode,
					CGMAsycudaPack.Schema.APA_GoodsDescription,
					CGMAsycudaPack.Schema.APA_MarksAndNumbers,
					CGMAsycudaPack.Schema.APA_Volume,
					CGMAsycudaPack.Schema.APA_VolumeUQ,
					CGMAsycudaPack.Schema.APA_VINNumber,
					CGMAsycudaPack.Schema.LinePrice,
					CGMAsycudaPack.Schema.LinePriceCurrency,
					CGMAsycudaPack.Schema.APA_LineNo,
					UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
					UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
			},
			columnVisibility.Single().Value);
	}
}
