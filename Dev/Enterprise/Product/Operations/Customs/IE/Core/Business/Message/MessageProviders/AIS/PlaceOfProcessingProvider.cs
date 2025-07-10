using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class PlaceOfProcessingProvider : IPlaceOfProcessing
	{
		public static PlaceOfProcessingProvider New(PlaceOfUseOrProcessing placeOfUseOrProcessing)
			=> placeOfUseOrProcessing is null ? null : new PlaceOfProcessingProvider(placeOfUseOrProcessing);

		public string IdentificationOfLocation => CachedValueHelper.GetValue(ref identificationOfLocationCached, () => GetIdentificationOfLocation());
		CachedValue<string> identificationOfLocationCached;

		public string QualifierIdentification => placeOfUseOrProcessing.CGL_Qualifier;

		public string AdditionalIdentifier => placeOfUseOrProcessing.AdditionalIdentifier;

		public string LocationTypeCode => placeOfUseOrProcessing.CGL_Type;

		public IAddress Address => CachedValueHelper.GetValue(ref addressCached, () => AddressProvider.New(placeOfUseOrProcessing.Address));
		CachedValue<IAddress> addressCached;

		string GetIdentificationOfLocation()
		{
			return placeOfUseOrProcessing.CGL_Qualifier.ToString() switch
			{
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode => placeOfUseOrProcessing.Unlocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier => placeOfUseOrProcessing.CGL_CustomsOffice,
				Customs.Business.CusGoodsLocationQualifierList.Codes.EoriNumber => placeOfUseOrProcessing.Address?.E2_GovRegNum,
				Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber => placeOfUseOrProcessing.Address?.AuthorisationNumber,
				_ => null
			};
		}

		PlaceOfProcessingProvider(PlaceOfUseOrProcessing placeOfUseOrProcessing)
		{
			this.placeOfUseOrProcessing = placeOfUseOrProcessing;
		}

		readonly PlaceOfUseOrProcessing placeOfUseOrProcessing;
	}
}
