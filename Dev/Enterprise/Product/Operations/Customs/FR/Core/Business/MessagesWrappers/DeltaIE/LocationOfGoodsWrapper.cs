using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using ICustomsOffice = CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.ICustomsOffice;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class LocationOfGoodsWrapper : ILocationOfGoods
	{
		LocationOfGoodsWrapper(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
			this.goodsLocationAddress = this.goodsLocation.Address;
		}
		readonly CusGoodsLocation goodsLocation;

		readonly CusGoodsLocationAddress goodsLocationAddress;

		public string AdditionalIdentifier => additionalIdentifier ?? (additionalIdentifier = goodsLocation.CGL_AdditionalIdentifier);
		string additionalIdentifier;

		public IAddress Address => address ?? (address = CusGoodsLocationAddressWrapper.New(goodsLocationAddress));
		IAddress address;

		public string AuthorisationNumber => authorisationNumber ?? (authorisationNumber = goodsLocationAddress?.AuthorisationNumber);
		string authorisationNumber;

		public ICustomsOffice CustomsOffice => customsOffice ?? (customsOffice = CustomsOfficeWrapper.New(goodsLocation));
		ICustomsOffice customsOffice;

		public IEconomicOperator EconomicOperator => economicOperator ?? (economicOperator = EconomicOperatorWrapper.New(goodsLocationAddress));
		IEconomicOperator economicOperator;

		public IGps GNSS => gNSS ?? (gNSS = GpsWrapper.New(goodsLocationAddress));
		IGps gNSS;

		public ILocationOfGoodsAddress PostcodeAddress => postcodeAddress ?? (postcodeAddress = LocationOfGoodsPostcodeAddressWrapper.New(goodsLocationAddress));
		ILocationOfGoodsAddress postcodeAddress;

		public string QualifierOfIdentification => qualifierOfIdentification ?? (qualifierOfIdentification = goodsLocation.CGL_Qualifier);
		string qualifierOfIdentification;

		public string TypeOfLocation => typeOfLocation ?? (typeOfLocation = goodsLocation.CGL_Type);
		string typeOfLocation;

		public string UNLOCODE => uNLOCODE ?? (uNLOCODE = goodsLocation.CGL_AdditionalIdentifier);
		string uNLOCODE;

		public static LocationOfGoodsWrapper New(CusGoodsLocation goodsLocation) => goodsLocation == null ? null : new LocationOfGoodsWrapper(goodsLocation);
	}
}
