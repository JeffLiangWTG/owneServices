using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICCF15ADeclaration : IDeclaration
	{
		ZString DeclarantTIN { get; }
		ZString ValidationDate { get; }
		ZString AuthorisedLocationOfGoodsCode { get; }
		ZString AgreedLocationOfGoods { get; }
		ZString AgreedLocationOfGoodsLanguage { get; }
		ZString IdentityOfMeansOfTransportAtDeparture { get; }
		ZString IdentityOfMeansOfTransportAtDepartureLanguage { get; }
		ZString NationalityOfMeansOfTransportAtDeparture { get; }
		ZString ControlResultDateLimit { get; }
		ZString PrincipalTIN { get; }
	}
}
