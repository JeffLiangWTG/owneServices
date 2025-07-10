using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CCF15ADeclarationWrapper : DeclarationWrapper, ICCF15ADeclaration
	{
		public CCF15ADeclarationWrapper(NctsHeader nctsHeader) : base(nctsHeader)
		{ }

		public ZString DeclarantTIN => CachedValueHelper.GetValue(ref declarantTIN, () => nctsHeader.DepartureDeclarantTIN());
		CachedValue<ZString> declarantTIN;

		public ZString ValidationDate => WrapperHelper.GetLongDate(ZDateTime.Now);

		public ZString PrincipalTIN => CachedValueHelper.GetValue(ref principalTIN, () => nctsHeader.Principal.Address.GetEORI().Left(17));
		CachedValue<ZString> principalTIN;

		public ZString AuthorisedLocationOfGoodsCode => nctsHeader.MovementHeader.BM_LocationOfGoodsCode;

		public ZString AgreedLocationOfGoods => nctsHeader.MovementHeader.BM_LocationOfGoods;

		public ZString AgreedLocationOfGoodsLanguage => ZString.Empty;

		public ZString IdentityOfMeansOfTransportAtDeparture => nctsHeader.MovementHeader.BM_TransportAtDeparture;

		public ZString IdentityOfMeansOfTransportAtDepartureLanguage => ZString.Empty;

		public ZString NationalityOfMeansOfTransportAtDeparture => nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry;

		public ZString ControlResultDateLimit => WrapperHelper.GetLongDate(nctsHeader.MovementHeader.BM_ExportDate);
	}
}
