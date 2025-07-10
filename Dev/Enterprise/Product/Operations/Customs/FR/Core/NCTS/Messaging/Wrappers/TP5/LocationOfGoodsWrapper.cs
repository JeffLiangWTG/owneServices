using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class LocationOfGoodsWrapper : ILocationOfGoods
	{
		LocationOfGoodsWrapper(EU.NCTS.Business.CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}
		readonly EU.NCTS.Business.CusGoodsLocation goodsLocation;

		public static LocationOfGoodsWrapper New(EU.NCTS.Business.CusGoodsLocation goodsLocation) => goodsLocation == null ? null : new LocationOfGoodsWrapper(goodsLocation);

		public string AdditionalIdentifier => goodsLocation.CGL_AdditionalIdentifier;

		public string AuthorisationNumber => authorisationNumber ?? (authorisationNumber = goodsLocation.Address?.AuthorisationNumber);
		string authorisationNumber;

		public ICustomsOffice CustomsOffice => customsOffice ?? (customsOffice = CustomsOfficeWrapper.New(goodsLocation.CGL_CustomsOffice));
		ICustomsOffice customsOffice;

		public IEconomicOperator EconomicOperator => economicOperator ?? (economicOperator = EconomicOperatorWrapper.New(goodsLocation.Address.E2_GovRegNum));
		IEconomicOperator economicOperator;

		public IGnss GNSS => gnss ?? (gnss = GnssWrapper.New(goodsLocation.Address));
		IGnss gnss;

		public string QualifierofIdentification => qualifierOfIdentification ?? (qualifierOfIdentification = goodsLocation.CGL_Qualifier);
		string qualifierOfIdentification;

		public string TypeOfLocation => typeOfLocation ?? (typeOfLocation = goodsLocation.CGL_Type);
		string typeOfLocation;

		public string UNLoCode => unLoCode ?? (unLoCode = goodsLocation.Unlocode);
		string unLoCode;

		public IPostCodeAddress PostCodeAddress => postCodeAddress ?? (postCodeAddress = PostCodeAddressWrapper.New(goodsLocation.Address));
		IPostCodeAddress postCodeAddress;

		public IContactPerson ContactPerson => null;

		public IAddress Address => address ?? (address = CusGoodsLocationAddressWrapper.New(goodsLocation.Address));
		IAddress address;
	}
}
