using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class LocationOfGoodsProvider : IGoodsLocation
	{
		public static LocationOfGoodsProvider New(CusGoodsLocation goodsLocation) => new LocationOfGoodsProvider(goodsLocation);

		LocationOfGoodsProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}
		readonly CusGoodsLocation goodsLocation;

		public string TypeOfLocation => goodsLocation.CGL_Type;

		public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

		public string AuthorisationNumber => goodsLocation.CGL_AdditionalIdentifier;

		public string AdditionalIdentifier => goodsLocation.CGL_AdditionalIdentifier;

		public string UNLOCODE => goodsLocation.CGL_AdditionalIdentifier;

		public string CustomsOffice => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? goodsLocation.CGL_CustomsOffice : null;

		public IGps GNSS => CachedValueHelper.GetValue(ref gnss, () => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.GnssCoordinates ? GPSProvider.New(goodsLocation.Address.E2_Latitude.ToString(), goodsLocation.Address.E2_Longitude.ToString()) : null);
		CachedValue<IGps> gnss;

		public string EconomicOperator => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.EoriNumber ? goodsLocation.Address.E2_GovRegNum : null;

		public IAddress Address => CachedValueHelper.GetValue(ref address, () => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.Address ? AddressProvider.New(goodsLocation.Address) : null);
		CachedValue<IAddress> address;

		public IPostcodeAddress PostcodeAddress => CachedValueHelper.GetValue(ref postcodeAddress, () => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress ? PostcodeAddressProvider.New(goodsLocation) : null);
		CachedValue<IPostcodeAddress> postcodeAddress;
	}
}
