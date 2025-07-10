using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class WarehouseCommonWrapper : IWarehouseCommon
	{
		public WarehouseCommonWrapper(CusAuthorizationUsage authorization)
		{
			this.authorization = Argument.NotNull(authorization, nameof(authorization));
		}
		readonly CusAuthorizationUsage authorization;

		public ZString Type => GetMappedType(authorization.AGC_Code);

		public ZString Identifier => authorization.AGC_Number;

		ZString GetMappedType(ZString type)
		{
			switch (type)
			{
				case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1:
					return AESWarehouseCodes.WarehouseTypeR;
				case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2:
					return AESWarehouseCodes.WarehouseTypeS;
				case CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP:
					return AESWarehouseCodes.WarehouseTypeU;
				case CusAuthorizationHeaderTypeList.Codes.TemporaryStorage:
					return AESWarehouseCodes.WarehouseTypeV;
				case ESCusAuthorisationHeaderTypeList.Codes.InAPrivateOtherThanCustomsWarehouse:
				case ESCusAuthorisationHeaderTypeList.Codes.InAPublicOtherThanCustomsWarehouseTypeI:
				case ESCusAuthorisationHeaderTypeList.Codes.InAPublicOtherThanCustomsWarehouseTypeIi:
				case ESCusAuthorisationHeaderTypeList.Codes.InAPublicRefWarehouseOnlyCanaryIslandAdministration:
					return AESWarehouseCodes.WarehouseTypeY;
				default:
					return ZString.Empty;
			}
		}
	}
}
