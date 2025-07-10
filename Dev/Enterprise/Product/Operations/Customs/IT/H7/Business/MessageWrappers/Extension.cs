using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.H7.Business;

public static class Extension
{
	public static IAddress GetShipperAddress(this AsycudaBill bill)
	{
		return new AddressWrapper(
			name: bill.ABL_ShipperName,
			streetAndNumber: bill.ABL_ShipperStreet1 + " " + bill.ABL_ShipperStreet2,
			country: bill.ABL_RN_NKShipperCountry,
			zipCode: bill.ABL_ShipperPostcode,
			city: bill.ABL_ShipperCity
		);
	}

	public static IAddress GetConsigneeAddress(this AsycudaBill bill)
	{
		return new AddressWrapper(
			name: bill.ABL_ConsigneeName,
			streetAndNumber: bill.ABL_ConsigneeStreet1 + " " + bill.ABL_ConsigneeStreet2,
			country: bill.ABL_RN_NKConsigneeCountry,
			zipCode: bill.ABL_ConsigneePostcode,
			city: bill.ABL_ConsigneeCity
		);
	}
}
