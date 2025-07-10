using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class H7ExporterNonOrganizationWrapper : IPartyProvider
{
	public H7ExporterNonOrganizationWrapper(AsycudaBill bill)
	{
		this.bill = bill;
	}

	public ZString Id => ZString.Empty;
	public ZString Name => bill.ABL_ShipperName;
	public ZString Address => bill.ABL_ShipperStreet1 + " " + bill.ABL_ShipperStreet2;
	public ZString City => bill.ABL_ShipperCity;
	public ZString PostCode => bill.ABL_ShipperPostcode;
	public ZString Country => bill.ABL_RN_NKShipperCountry;

	readonly AsycudaBill bill;
}
