using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class LocationOfGoodsWrapper : ILocationOfGoods
	{
		LocationOfGoodsWrapper(EU.Business.CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}
		readonly EU.Business.CusGoodsLocation goodsLocation;

		public string AdditionalIdentifier => additionalIdentifier ?? (additionalIdentifier = goodsLocation.AdditionalIdentifier);
		string additionalIdentifier;

		public IAddress Address => address ?? (address = AddressWrapper.New(goodsLocation.Address));
		IAddress address;

		public string AuthorisationNumber => authorisationNumber ?? (authorisationNumber = goodsLocation.Address?.AuthorisationNumber ?? ZString.Empty);
		string authorisationNumber;

		public ICustomsOffice CustomsOffice => customsOffice ?? (customsOffice = CustomsOfficeWrapper.New(goodsLocation.CGL_CustomsOffice));
		ICustomsOffice customsOffice;

		public IEconomicOperator EconomicOperator => economicOperator ?? (economicOperator = EconomicOperatorWrapper.New(goodsLocation.Address));
		IEconomicOperator economicOperator;

		public IGnss Gnss => gnss ?? (gnss = GnssWrapper.New(goodsLocation.Address));
		IGnss gnss;

		public string QualifierOfIdentification => qualifierOfIdentification ?? (qualifierOfIdentification = goodsLocation.CGL_Qualifier);
		string qualifierOfIdentification;

		public string TypeOfLocation => typeOfLocation ?? (typeOfLocation = goodsLocation.CGL_Type);
		string typeOfLocation;

		public string UnLoCode => unLoCode ?? (unLoCode = goodsLocation.CGL_AdditionalIdentifier);
		string unLoCode;

		public static LocationOfGoodsWrapper New(EU.Business.CusGoodsLocation goodsLocation) => goodsLocation == null ? null : new LocationOfGoodsWrapper(goodsLocation);
	}
}
