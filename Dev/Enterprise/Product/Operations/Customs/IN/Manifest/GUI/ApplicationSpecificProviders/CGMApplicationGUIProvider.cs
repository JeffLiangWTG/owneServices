using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using ResourceStringData = CargoWiseOne.ResourceStrings.ResourceStringData;

namespace Enterprise.Customs.IN.Manifest.GUI;

public sealed class CGMApplicationGUIProvider : ApplicationGUIProvider
{
	public override Type ApplicationBusinessProviderType => typeof(CGMApplicationBusinessProvider);

	public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

	protected override IPanelLayoutProvider GetManifestLayoutCore() => new CGMManifestLayout();

	protected override IPanelLayoutProvider GetBillLayoutCore() => new CGMBillLayout();

	protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new CGMBillPartiesLayout();

	protected override IReadOnlyDictionary<bool, string[]> GetContainersGridColumnAvailabilityCore()
	{
		return new Dictionary<bool, string[]>
		{
			{
				false,
				new[]
				{
					CGMAsycudaContainer.Schema.ACN_SealType1,
					CGMAsycudaContainer.Schema.ACN_SealingPartyType,
					CGMAsycudaContainer.Schema.ACN_SealingPartyName,
					CGMAsycudaContainer.Schema.ACN_CommodityCode,
					CGMAsycudaContainer.Schema.ACN_StowageLocation,
					CGMAsycudaContainer.Schema.ACN_Seal2,
					CGMAsycudaContainer.Schema.ACN_SealingPartyType2,
					CGMAsycudaContainer.Schema.ACN_SealType2,
					CGMAsycudaContainer.Schema.ACN_Seal3,
					CGMAsycudaContainer.Schema.ACN_SealingPartyType3,
					CGMAsycudaContainer.Schema.ACN_SealType3,
				}
			}
		};
	}

	protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
	{
		return new (bool Availability, string ColumnName)[]
		{
			(false, CGMAsycudaBill.Schema.ABL_BolType),
			(false, CGMAsycudaBill.Schema.NotifyPartyOrgPK),
			(false, CGMAsycudaBill.Schema.ABL_OA_NotifyParty),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyName),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyStreet1),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyStreet2),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyCity),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyState),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyPostcode),
			(false, CGMAsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry),
			(false, CGMAsycudaBill.Schema.ABL_NotifyPartyPhone),
			(false, CGMAsycudaBill.Schema.ABL_UCRNumber),
			(false, CGMAsycudaBill.Schema.CustomsJobNumber),
			(false, CGMAsycudaBill.Schema.ABL_CustomsValue),
			(false, CGMAsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency),
			(false, CGMAsycudaBill.Schema.DiscountValue),
			(false, CGMAsycudaBill.Schema.DiscountValueCurrency),
			(false, CGMAsycudaBill.Schema.ABL_TransportValue),
			(false, CGMAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency),
			(false, CGMAsycudaBill.Schema.ABL_FreightValue),
			(false, CGMAsycudaBill.Schema.ABL_RX_NKFreightValueCurrency),
			(false, CGMAsycudaBill.Schema.ABL_InsuranceValue),
			(false, CGMAsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency),
			(false, CGMAsycudaBill.Schema.OtherChargesValue),
			(false, CGMAsycudaBill.Schema.OtherChargesValueCurrency),
			(header.IsSea, CGMAsycudaBill.Schema.ABL_CargoStatus),
		}
		.GroupBy(x => x.Availability).ToDictionary(x => x.Key, x => x.Select(n => n.ColumnName).ToArray());
	}

	protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
	{
		yield return new ZDateEditColumnStyleInfo()
		{
			ColumnName = CGMAsycudaBill.Schema.ABL_BillIssueDate,
			DateTimeFormat = ZDateTimePickerFormat.Short,
			IsMandatory = true,
		};

		yield return new ZDropEditColumnStyleInfo()
		{
			ColumnName = CGMAsycudaBill.Schema.ABL_SpecialCargoCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
		};

		foreach (var field in GetBillsGridImporterColumnInfos())
		{
			yield return field;
		}

		foreach (var field in base.GetBillsGridExtraColumnInfosCore())
		{
			yield return field;
		}
	}

	protected override IEnumerable<string> GetBillsGridColumnsOrderCore()
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

	protected override void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid)
	{
		base.CustomizeBillsGridCore(billsGrid);

		SetBillsGridColumnCaptions(billsGrid);
	}

	protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
	{
		if (header is CGMAsycudaManifestHeader inHeader)
		{
			yield return new ZCheckBoxColumnStyleInfo()
			{
				ColumnName = CGMAsycudaContainer.Schema.ACN_IsShipperOwned,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				IsUnavailable = false,
			};
		}

		foreach (var field in base.GetContainersGridExtraColumnInfosCore(header))
		{
			yield return field;
		}

		var containerAgentCodeGroup = Res.GetData("46704660-A35A-4405-802A-C6B4F2FB6460", "Container Agent Code");

		yield return new ZOrganisationFindBoxColumnStyleInfo
		{
			ColumnName = CGMAsycudaContainer.Schema.ContainerAgentCodeOrgPK,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			GroupName = containerAgentCodeGroup
		};

		yield return new ZGuidDropEditColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			ColumnName = CGMAsycudaContainer.Schema.ContainerAgentCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(162),
			GroupName = containerAgentCodeGroup
		};

		yield return new ZDropEditColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			ColumnName = CGMAsycudaContainer.Schema.ISOCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
		};
	}

	protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
	{
		yield return new AsycudaPackUserControl();
	}

	protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
	{
		var containerStatusTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		containerStatusTextBoxColumnStyle.ColumnName = CGMAsycudaPack.Schema.ContainerStatus;
		containerStatusTextBoxColumnStyle.IsReadOnly = true;
		containerStatusTextBoxColumnStyle.CaptionResourceString = Res.GetData("EEC8BC05-63B2-4913-B31A-455CA05D284C", "Cont. St.", "Cont. Status", "Container Status", "");
		containerStatusTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return containerStatusTextBoxColumnStyle;

		var sealNoTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		sealNoTextBoxColumnStyle.ColumnName = CGMAsycudaPack.Schema.SealNo;
		sealNoTextBoxColumnStyle.IsReadOnly = true;
		sealNoTextBoxColumnStyle.CaptionResourceString = Res.GetData("4FCA9584-5EBE-4C95-B7FE-4614A282F264", "Seal No");
		sealNoTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return sealNoTextBoxColumnStyle;

		var isoCodeTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		isoCodeTextBoxColumnStyle.ColumnName = CGMAsycudaPack.Schema.ISOCode;
		isoCodeTextBoxColumnStyle.IsReadOnly = true;
		isoCodeTextBoxColumnStyle.CaptionResourceString = Res.GetData("40C09610-AC29-4DBA-9364-0ADBEF75DEC9", "ISO Code");
		isoCodeTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return isoCodeTextBoxColumnStyle;

		var socFlagCheckBoxColumnStyle = new ZCheckBoxColumnStyleInfo();
		socFlagCheckBoxColumnStyle.ColumnName = CGMAsycudaPack.Schema.SOCFlag;
		socFlagCheckBoxColumnStyle.IsReadOnly = true;
		socFlagCheckBoxColumnStyle.CaptionResourceString = Res.GetData("5D6A8029-53DC-489D-BA47-1995C2FE623E", "SOC Flag");
		socFlagCheckBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return socFlagCheckBoxColumnStyle;

		var containerAgentCodeTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		containerAgentCodeTextBoxColumnStyle.ColumnName = CGMAsycudaPack.Schema.ContainerAgentPAN;
		containerAgentCodeTextBoxColumnStyle.IsReadOnly = true;
		containerAgentCodeTextBoxColumnStyle.CaptionResourceString = Res.GetData("C1E19C9C-C333-4936-8777-BD3BB0310E86", "Container Agent Code");
		containerAgentCodeTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return containerAgentCodeTextBoxColumnStyle;

		foreach (var field in base.GetPacksGridExtraColumnInfosCore())
		{
			yield return field;
		}
	}

	protected override IReadOnlyDictionary<bool, string[]> GetPacksGridColumnVisibilityCore()
	{
		return new Dictionary<bool, string[]>
		{
			{
				false,
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
				}
			}
		};
	}

	protected override string[] GetPacksGridColumnsOrderCore()
	{
		return new string[] {
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
	}

	IEnumerable<ZGridColumnInfo> GetBillsGridImporterColumnInfos()
	{
		var importerName = Res.GetData("2B70FACC-1D31-49C4-A2AC-7B28258FE25A", "Importer");

		var buyerOrgFindBoxColumnStyle = new ZOrganisationFindBoxColumnStyleInfo();
		buyerOrgFindBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.BuyerOrgPK);
		buyerOrgFindBoxColumnStyle.CaptionResourceString = importerName;
		buyerOrgFindBoxColumnStyle.GroupName = importerName;
		buyerOrgFindBoxColumnStyle.IsVisible = false;
		buyerOrgFindBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		yield return buyerOrgFindBoxColumnStyle;

		var buyerAddressDropEditColumnStyle = new ZGuidDropEditColumnStyleInfo();
		buyerAddressDropEditColumnStyle.CaptionResourceString = Res.GetData("27D92564-5397-4D96-9991-139A267B7788", "Address", "Importer Address", "");
		buyerAddressDropEditColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_OA_Buyer);
		buyerAddressDropEditColumnStyle.GroupName = importerName;
		buyerAddressDropEditColumnStyle.IsVisible = false;
		buyerAddressDropEditColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return buyerAddressDropEditColumnStyle;

		var buyerNameTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		buyerNameTextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerName);
		buyerNameTextBoxColumnStyle.IsVisible = false;
		buyerNameTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return buyerNameTextBoxColumnStyle;

		var buyerStreet1TextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		buyerStreet1TextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerStreet1);
		buyerStreet1TextBoxColumnStyle.IsVisible = false;
		buyerStreet1TextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		yield return buyerStreet1TextBoxColumnStyle;

		var buyerStreet2TextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		buyerStreet2TextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerStreet2);
		buyerStreet2TextBoxColumnStyle.IsVisible = false;
		buyerStreet2TextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		yield return buyerStreet2TextBoxColumnStyle;

		var buyerCityTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		buyerCityTextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerCity);
		buyerCityTextBoxColumnStyle.IsVisible = false;
		buyerCityTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		yield return buyerCityTextBoxColumnStyle;

		var buyerStateDropEditColumnStyle = new ZDropEditColumnStyleInfo();
		buyerStateDropEditColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerState);
		buyerStateDropEditColumnStyle.IsVisible = false;
		buyerStateDropEditColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return buyerStateDropEditColumnStyle;

		var buyerPhoneTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		buyerPhoneTextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerPhone);
		buyerPhoneTextBoxColumnStyle.IsVisible = false;
		buyerPhoneTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return buyerPhoneTextBoxColumnStyle;

		var buyerPostcodeTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		buyerPostcodeTextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BuyerPostcode);
		buyerPostcodeTextBoxColumnStyle.IsVisible = false;
		buyerPostcodeTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		yield return buyerPostcodeTextBoxColumnStyle;

		var buyerCountryCodeFindBoxColumnStyle = new ZCodeFindBoxColumnStyleInfo();
		buyerCountryCodeFindBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_RN_NKBuyerCountry);
		buyerCountryCodeFindBoxColumnStyle.IsVisible = false;
		buyerCountryCodeFindBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		yield return buyerCountryCodeFindBoxColumnStyle;

		var billStatusDropEditColumnStyle = new ZDropEditColumnStyleInfo();
		billStatusDropEditColumnStyle.ColumnName = nameof(CGMAsycudaBill.ABL_BillStatus);
		billStatusDropEditColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		billStatusDropEditColumnStyle.IsMandatory = true;
		yield return billStatusDropEditColumnStyle;

		var messageStatusTextBoxColumnStyle = new ZTextBoxColumnStyleInfo();
		messageStatusTextBoxColumnStyle.ColumnName = nameof(CGMAsycudaBill.MessageStatusDescription);
		messageStatusTextBoxColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
		messageStatusTextBoxColumnStyle.IsMandatory = true;
		yield return messageStatusTextBoxColumnStyle;
	}

	void SetBillsGridColumnCaptions(ZGridWithDynamicColumnHandler billsGrid)
	{
		SetGridColumnCaption(billsGrid, CGMAsycudaBill.Schema.ABL_ManifestQty, Res.GetData("13E725F4-E749-405A-9C52-260AF1A5B010", "Pack.", "Packages", "Packages", ""), CGMAsycudaBill.Schema.ABL_ManifestUQ);
		SetGridColumnCaption(billsGrid, CGMAsycudaBill.Schema.ABL_GrossWeight, Res.GetData("C9007B4B-0214-44E6-90D5-97F20F488926", "Gross Wt.", "Gross Wt.", "Gross Weight", ""), CGMAsycudaBill.Schema.ABL_GrossWeightUQ);
		SetGridColumnCaption(billsGrid, CGMAsycudaBill.Schema.ABL_Volume, Res.GetData("2C2970FC-4486-43C2-9A7E-9E21C0CF0E11", "Vol.", "Volume", "Volume", ""), CGMAsycudaBill.Schema.ABL_VolumeUQ);
	}

	void SetGridColumnCaption(ZGridWithDynamicColumnHandler billsGrid, string columnName, ResourceStringData resStringData, params string[] groupColumns)
	{
		var columnInfo = billsGrid.GetColumnStyle(columnName);
		if (columnInfo is not null)
		{
			columnInfo.CaptionResourceString = resStringData;
			columnInfo.GroupName = resStringData;
		}

		foreach (var groupColumn in groupColumns)
		{
			var groupColumnInfo = billsGrid.GetColumnStyle(groupColumn);
			if (groupColumnInfo is not null)
			{
				groupColumnInfo.GroupName = resStringData;
			}
		}
	}
}
