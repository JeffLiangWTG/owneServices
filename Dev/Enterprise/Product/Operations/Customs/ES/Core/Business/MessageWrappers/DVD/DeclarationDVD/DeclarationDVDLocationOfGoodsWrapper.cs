using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDLocationOfGoodsWrapper : IDeclarationDVDLocationOfGoods
	{
		public DeclarationDVDLocationOfGoodsWrapper(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			goodsLocation = this.entryInstruction.GoodsLocation;
		}
		readonly CusEntryInstruction entryInstruction;
		readonly EU.Business.CusGoodsLocation goodsLocation;

		const string CountryForTypeB = "ES";

		public ZString LocationCountry => goodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace
																? (ZString)CountryForTypeB
																: goodsLocation.Address.E2_RN_NKCountryCode;

		public ZString LocationType => goodsLocation.CGL_Type;

		public ZString LocationQualifier => goodsLocation.CGL_Qualifier;

		public ZString LocationId => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
														? goodsLocation.Address.AuthorisationNumber
														: ZString.Empty;

		public ZString LocationAdditionalId => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
																? goodsLocation.CGL_AdditionalIdentifier
																: ZString.Empty;

		public ZString LocationAddress => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address
																? goodsLocation.Address.E2_Address1
																: ZString.Empty;

		public ZString LocationCity => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address
																? goodsLocation.Address.E2_City
																: ZString.Empty;

		public ZString LocationPostCode => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address
																? goodsLocation.Address.E2_Postcode
																: ZString.Empty;
	}
}
