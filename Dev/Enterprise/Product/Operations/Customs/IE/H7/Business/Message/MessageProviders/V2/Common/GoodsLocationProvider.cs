using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;

namespace Enterprise.Customs.IE.H7.Business
{
	public class GoodsLocationProvider : IGoodsLocation, CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1.IGoodsLocation
	{
		public GoodsLocationProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}

		readonly CusGoodsLocation goodsLocation;

		public string TypeOfLocation => goodsLocation.CGL_Type;

		public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

		public string AuthorisationNumber => null;

		public string AdditionalIdentifier => goodsLocation.CGL_AdditionalIdentifier;

		public string UNLOCODE => goodsLocation.Unlocode;

		public string CustomsOffice => goodsLocation.Parent.Header.AMA_CustomsOffice;

		public IGps GNSS => CachedValueHelper.GetValue(ref gnssCached, () => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.GnssCoordinates && goodsLocation.Address is CusGoodsLocationAddress address ? GpsProvider.New(address.E2_Latitude.ToString(), address.E2_Longitude.ToString()) : null);
		CachedValue<IGps> gnssCached;

		public string EconomicOperator => economicOperator ?? (economicOperator = goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.EoriNumber ? goodsLocation.Address.E2_GovRegNum.ToString() : string.Empty);
		string economicOperator;

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.Address ? AddressProvider.New(goodsLocation.Address) : null);
		CachedValue<IAddress> addressCached;

		public IPostcodeAddress PostcodeAddress => CachedValueHelper.GetValue(ref postcodeAddressCached, () =>
		{
			if (goodsLocation.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress)
			{
				var address = goodsLocation.Address;
				return PostcodeAddressProvider.New(goodsLocation.CGL_AdditionalIdentifier, address.E2_Postcode, address.E2_RN_NKCountryCode);
			}
			return null;
		});
		CachedValue<IPostcodeAddress> postcodeAddressCached;
	}
}
