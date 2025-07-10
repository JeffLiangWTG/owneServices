using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5GenericLocationWrapper : IGenericLocation
	{
		public G5GenericLocationWrapper(CusGoodsLocation location)
		{
			this.location = Argument.NotNull(location, nameof(location));
			qualifierIsZ = location.CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.Address;
		}
		readonly CusGoodsLocation location;
		readonly ZBool qualifierIsZ;

		public ZString Type => location.CGL_Type;

		public ZString Qualifier => location.CGL_Qualifier;

		public ICodedGenericLocation Coded => coded ??= qualifierIsZ ? null : new G5CodedGenericLocationWrapper(location);
		G5CodedGenericLocationWrapper coded;

		public IPartyAddressProvider Address => address ??= !qualifierIsZ ? null : PartyAddressWrapper.New(location.Address.E2_Address1,
																											location.Address.E2_City,
																											location.Address.E2_Postcode,
																											location.Address.E2_RN_NKCountryCode);
		PartyAddressWrapper address;
	}
}
