using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class MiscDisbursementChargeCodeList
	{
		public static string GetChargeTypeCode(ZString chargeType)
		{
			var result = ZString.Empty;
			switch (chargeType)
			{
				case ChargeTypeList.Codes.DIF:
					result = Codes._1;
					break;
				case ChargeTypeList.Codes.PAF:
					result = Codes._2;
					break;
				case ChargeTypeList.Codes.TOF:
					result = Codes._3;
					break;
			}
			return result;
		}
	}
}
