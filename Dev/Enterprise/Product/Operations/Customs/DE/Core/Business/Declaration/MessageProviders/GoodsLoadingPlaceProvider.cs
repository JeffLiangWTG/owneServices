using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class GoodsLoadingPlaceProvider : IGoodsLoadingPlace
	{
		public static GoodsLoadingPlaceProvider NewOrNull(CusEntryInstruction entryInstruction, OrgAddress pickUpAddress) => entryInstruction == null && pickUpAddress == null ? null : new GoodsLoadingPlaceProvider(entryInstruction, pickUpAddress);

		GoodsLoadingPlaceProvider(CusEntryInstruction entryInstruction, OrgAddress pickUpAddress)
		{
			this.entryInstruction = entryInstruction;
			this.pickUpAddress = pickUpAddress;
		}
		readonly CusEntryInstruction entryInstruction;
		readonly OrgAddress pickUpAddress;

		public string LoadingPlaceCode => entryInstruction?.GoodsLocation?.LoadingPlace ?? string.Empty;

		public string Address => pickUpAddress?.Address1 ?? string.Empty;

		public string City => pickUpAddress?.City ?? string.Empty;

		public string Postcode => pickUpAddress?.Postcode ?? string.Empty;

		public string Country => string.Empty;

		public string AdditionalAddressInfo => pickUpAddress?.OA_AdditionalAddressInformation ?? string.Empty;

		public string TypeOfLocation => string.Empty;

		public string QualifierOfIdentification => string.Empty;

		public string AuthorisationNumber => string.Empty;

		public string AdditionalIdentifier => string.Empty;

		public string UNLocode => string.Empty;

		public string CustomsOfficeReferenceNumber => string.Empty;

		public string GNSSLatitude => string.Empty;

		public string GNSSLongitude => string.Empty;

		public string EconomicOperatorIdentificationNumber => string.Empty;

		public string AddressComplementOfInformation => string.Empty;

		public string PostcodeAddressHouseNumber => string.Empty;

		public string PostcodeAddressPostcode => string.Empty;

		public string PostcodeAddressCountry => string.Empty;

		public IAESPartyContactPerson ContactPerson => null;
	}
}
