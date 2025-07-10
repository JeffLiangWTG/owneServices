using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMSeaAsycudaBillDocWrapper : DocumentWrapper
{
	public CGMSeaAsycudaBillDocWrapper(CGMAsycudaBill bill)
	{
		Bill = Argument.NotNull(bill, nameof(bill));
		Header = Argument.NotNull(bill.Header, nameof(bill.Header));
	}

	public ZString SubLineNumber => Bill.ABL_CarrierReference;

	public ZString HBLNumber => Bill.ABL_BillNumber;

	public ZDate HBLDate => Bill.ABL_BillIssueDate;

	public ZString PortOfShipment => Header.AMA_RL_NKOrigin;

	public ZString PortOfDestination => Bill.ABL_RL_NKFinalDestination;

	public ZString ImporterName => Bill.Buyer?.CompanyName ?? Bill.ABL_BuyerName;

	public ZString ImporterAddress => GetImporterAddress();

	public ZString Consigneename => Bill.Consignee?.CompanyName ?? Bill.ABL_ConsigneeName;

	public ZString ConsigneeAddress => GetConsigneeAddress();

	public ZString NatureOfCargo => Bill.Lookups.NatureOfCargoList.GetDescriptionFromCode(Bill.ABL_ContainerMode);

	public ZString ItemType => Bill.Lookups.SpecialCargoCodeList.GetDescriptionFromCode(Bill.ABL_SpecialCargoCode);

	public ZString CargoMovement => Bill.Lookups.CargoStatusList.GetDescriptionFromCode(Bill.ABL_CargoStatus);

	public ZString DestinationCode => Bill.ABL_CustomsFinalDestinationPort;

	public ZInt NumberOfPackage => Bill.ABL_ManifestQty;

	public ZString NumberOfPackageUOM => Bill.ABL_ManifestUQ;

	public ZDecimal GrossWeight => Bill.ABL_GrossWeight;

	public ZString GrossWeightUOM => Bill.ABL_GrossWeightUQ;

	public ZDecimal GrossVolume => Bill.ABL_Volume;

	public ZString GrossVolumeUOM => Bill.ABL_VolumeUQ;

	public ZString MarksAndNumber => Bill.ABL_MarksAndNumbers;

	public ZString GoodsDescription => Bill.ABL_GoodsDescription;

	public ZString UNOCode => Bill.UnoCode;

	public ZString IMCOCode => Bill.ImcoCode;

	public ZString BondNumber => Bill.BondNumber;

	public ZString CarrierCode => "TBA";

	public ZString TransportMode => Bill.ABL_InlandTransportMode;

	public ZString MLOCode => "TBA";

	CGMAsycudaBill Bill { get; }

	CGMAsycudaManifestHeader Header { get; }

	ZString GetImporterAddress()
	{
		var bill = Bill;
		var buyer = bill.Buyer;

		var builder = new ZStringBuilder();
		if (buyer is null)
		{
			builder.AppendIfNotEmpty(bill.ABL_BuyerStreet1);
			builder.AppendIfNotEmpty(bill.ABL_BuyerStreet2);
			builder.AppendIfNotEmpty(bill.ABL_BuyerCity);
			builder.AppendIfNotEmpty(bill.ABL_BuyerState);
			builder.AppendIfNotEmpty(bill.ABL_BuyerPostcode);
			builder.AppendIfNotEmpty(bill.ABL_RN_NKBuyerCountry);
		}
		else
		{
			AppendOrgAddress(buyer, builder);
		}
		return builder.ToStringWithDelimiterBetweenAppends(AddressSeparator);
	}

	ZString GetConsigneeAddress()
	{
		var bill = Bill;
		var consignee = bill.Consignee;

		var builder = new ZStringBuilder();
		if (consignee is null)
		{
			builder.AppendIfNotEmpty(bill.ABL_ConsigneeStreet1);
			builder.AppendIfNotEmpty(bill.ABL_ConsigneeStreet2);
			builder.AppendIfNotEmpty(bill.ABL_ConsigneeCity);
			builder.AppendIfNotEmpty(bill.ABL_ConsigneeState);
			builder.AppendIfNotEmpty(bill.ABL_ConsigneePostcode);
			builder.AppendIfNotEmpty(bill.ABL_RN_NKConsigneeCountry);
		}
		else
		{
			AppendOrgAddress(consignee, builder);
		}
		return builder.ToStringWithDelimiterBetweenAppends(AddressSeparator);
	}

	static void AppendOrgAddress(OrgAddress orgAddress, ZStringBuilder builder)
	{
		builder.AppendIfNotEmpty(orgAddress.OA_Address1);
		builder.AppendIfNotEmpty(orgAddress.OA_Address2);
		builder.AppendIfNotEmpty(orgAddress.OA_City);
		builder.AppendIfNotEmpty(orgAddress.OA_State);
		builder.AppendIfNotEmpty(orgAddress.OA_PostCode);
		builder.AppendIfNotEmpty(orgAddress.OA_RN_NKCountryCode);
	}

	const string AddressSeparator = ", ";
}
