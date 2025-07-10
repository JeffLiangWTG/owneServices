using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using CusGoodsLocation = Enterprise.Customs.ES.Business.CusGoodsLocation;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5CodedGenericLocationWrapper : ICodedGenericLocation
	{
		public G5CodedGenericLocationWrapper(CusGoodsLocation location)
		{
			this.location = Argument.NotNull(location, nameof(location));
		}
		readonly CusGoodsLocation location;

		public ZString UNLOCOCode => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode ? location.Unlocode : ZString.Empty;

		public ZString CustomsOffice => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? location.CGL_CustomsOffice : ZString.Empty;

		public ICommonGNSS GPS => gps ??= location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates
																	? new CommonGNSSWrapper(location.Address.E2_Latitude, location.Address.E2_Longitude)
																	: null;
		CommonGNSSWrapper gps;

		public ZString EconomicOperator => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber ? location.Address.E2_GovRegNum : ZString.Empty;

		public ZString AuthorisationNumber => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
																	? location.Address?.AuthorisationNumber ?? ZString.Empty
																	: ZString.Empty;

		public ZString AdditionalId => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber
												|| location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
											? location.CGL_AdditionalIdentifier
											: ZString.Empty;
	}
}
