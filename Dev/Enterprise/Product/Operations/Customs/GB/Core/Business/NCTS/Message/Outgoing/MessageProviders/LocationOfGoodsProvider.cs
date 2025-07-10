using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using IAddress = CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.IAddress;
using Qualifier = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class LocationOfGoodsProvider : ILocationOfGoods
	{
		public LocationOfGoodsProvider(NctsHeader nctsHeader)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			goodsLocation = header.IsArrivalMovement ? header.ArrivalMovementHeader.GoodsLocation : header.MovementHeader.GoodsLocation;
		}

		public string TypeOfLocation => goodsLocation.CGL_Type;

		public string AuthorisationNumber => IsQualifierEquals(Qualifier.AuthorizationNumber) ? goodsLocation.Address.E2_GovRegNum.ToString() : null;

		public string AdditionalIdentifier => IsQualifierEquals(Qualifier.AuthorizationNumber) || IsQualifierEquals(Qualifier.EoriNumber) ? goodsLocation.CGL_AdditionalIdentifier.ToString() : null;

		public string CustomsOfficeReferenceNumber => IsQualifierEquals(Qualifier.CustomsOfficeIdentifier) ? goodsLocation.CGL_CustomsOffice.ToString() : null;

		public string EconomicOperatorIdentificationNumber => IsQualifierEquals(Qualifier.EoriNumber) ? goodsLocation.Address.E2_GovRegNum.ToString() : null;

		public IContactPerson ContactPerson => !IsQualifierEquals(Qualifier.CustomsOfficeIdentifier) ? contactPerson ?? (contactPerson = ContactPersonProvider.NewOrNull(goodsLocation.Address)) : null;
		IContactPerson contactPerson;

		public IPostCodeAddress PostCodeAddress => IsQualifierEquals(Qualifier.PostcodeAddress) ? postCodeAddress ?? (postCodeAddress = new PostCodeAddressProvider(goodsLocation)) : null;
		IPostCodeAddress postCodeAddress;

		public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

		public string UNLocode => IsQualifierEquals(Qualifier.UnLocode) ? goodsLocation.CGL_AdditionalIdentifier.ToString() : null;

		public string GNSSLatitude => IsQualifierEquals(Qualifier.GnssCoordinates) ? goodsLocation.Address.E2_Latitude.ToString() : null;

		public string GNSSLongitude => IsQualifierEquals(Qualifier.GnssCoordinates) ? goodsLocation.Address.E2_Longitude.ToString() : null;

		public IAddress Address => IsQualifierEquals(Qualifier.Address) ? address ?? (address = new AddressProvider(goodsLocation.Address, header.IsInPhase5TransitionPeriod)) : null;
		IAddress address;

		bool IsQualifierEquals(string value) => QualifierOfIdentification == value;

		readonly NctsHeader header;
		readonly EU.NCTS.Business.CusGoodsLocation goodsLocation;
	}
}
