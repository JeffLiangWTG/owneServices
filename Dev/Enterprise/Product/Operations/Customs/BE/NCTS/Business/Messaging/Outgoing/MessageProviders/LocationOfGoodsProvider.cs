using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class LocationOfGoodsProvider : ILocationOfGoods
	{
		readonly NctsHeader header;
		readonly EU.NCTS.Business.CusGoodsLocation cusGoodsLocation;

		public LocationOfGoodsProvider(NctsHeader nctsHeader)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			cusGoodsLocation = header.IsArrivalMovement ? header.ArrivalMovementHeader.GoodsLocation : header.MovementHeader.GoodsLocation;
		}

		public string TypeOfLocation => cusGoodsLocation.CGL_Type;

		public string AuthorisationNumber => null;

		public string AdditionalIdentifier => null;

		public string CustomsOfficeReferenceNumber => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? cusGoodsLocation.CGL_CustomsOffice : null;

		public string EconomicOperatorIdentificationNumber => null;

		public IContactPerson ContactPerson => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? contactPerson ?? (contactPerson = ContactPersonProvider.NewOrNull(cusGoodsLocation.Address)) : null;
		IContactPerson contactPerson;

		public IPostCodeAddress PostCodeAddress => null;

		public string QualifierOfIdentification => cusGoodsLocation.CGL_Qualifier;

		public string UNLocode => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? cusGoodsLocation.CGL_AdditionalIdentifier : null;

		public string GNSSLatitute => null;

		public string GNSSLongitude => null;

		public CargoWise.Customs.BE.MessageContracts.Interfaces.IAddress Address => null;
	}
}
