using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class LocationOfGoodsProvider : IE.Messaging.MessageProvider, ILocationOfGoods
	{
		public LocationOfGoodsProvider(ICusGoodsLocationProvider cusGoodsLocationProvider)
		{
			GoodsLocation = Argument.NotNull(cusGoodsLocationProvider.GoodsLocation, nameof(cusGoodsLocationProvider.GoodsLocation));
			CusGoodsLocationAddress = Argument.NotNull(GoodsLocation.Address, nameof(GoodsLocation.Address));
		}
		public readonly EU.Business.CusGoodsLocation GoodsLocation;
		public readonly EU.Business.CusGoodsLocationAddress CusGoodsLocationAddress;

		public string QualifierOfIdentification => GoodsLocation.CGL_Qualifier;

		public string AuthorisationNumber => GoodsLocation.Address.AuthorisationNumber;

		public string AdditionalIdentifier => GoodsLocation.CGL_AdditionalIdentifier;

		public string LocationCodeType => GoodsLocation.CGL_Type;

		public string UNLocode => GoodsLocation.Unlocode;

		public string CustomsOffice => GoodsLocation.CGL_CustomsOffice;

		public IGNSS GNSS => CachedValueHelper.GetValue(ref gnss, () => new GNSSProvider(ZGeography.CreatePoint((double)CusGoodsLocationAddress.E2_Longitude, (double)CusGoodsLocationAddress.E2_Latitude)));
		CachedValue<IGNSS> gnss;

		public string EconomicOperator => CachedValueHelper.GetValue(ref economicOperator, () => { return QualifierOfIdentification.Equals(Customs.Business.CusGoodsLocationQualifierList.Codes.EoriNumber, System.StringComparison.InvariantCultureIgnoreCase) ? GetEconomicOperator() : null; });
		CachedValue<string> economicOperator;

		string GetEconomicOperator()
			=> CusGoodsLocationAddress.E2_GovRegNum.IsEmpty
			? MessageProviderHelper.GetRegCodeFromCustomsCodes(CusGoodsLocationAddress.Organisation)
			: (string)CusGoodsLocationAddress.E2_GovRegNum;

		public IAddress Address => CachedValueHelper.GetValue(ref address, () => AddressProvider.New(CusGoodsLocationAddress));
		CachedValue<IAddress> address;

		public IAddress PostcodeAddress => CachedValueHelper.GetValue(ref postcodeAddress, () => AddressProvider.New(CusGoodsLocationAddress));
		CachedValue<IAddress> postcodeAddress;

		public IContact ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => IE.Business.ContactProvider.New(CusGoodsLocationAddress));
		CachedValue<IContact> contactPerson;
	}
}
