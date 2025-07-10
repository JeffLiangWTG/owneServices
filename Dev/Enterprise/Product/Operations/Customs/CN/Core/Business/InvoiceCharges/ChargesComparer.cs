using System.Collections;

namespace Enterprise.Customs.CN.Business
{
	public class ChargesComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			int result = 0;

			Customs.Business.BaseJobComInvHeaderCharge xCharge = (Customs.Business.BaseJobComInvHeaderCharge)x;
			Customs.Business.BaseJobComInvHeaderCharge yCharge = (Customs.Business.BaseJobComInvHeaderCharge)y;

			if (xCharge.ChargeCode != null && yCharge.ChargeCode != null)
			{
				if (xCharge.ChargeCode.Code == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance)
				{
					result = yCharge.ChargeCode.Code == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance ? 0 : 1;
				}
				else if (yCharge.ChargeCode.Code == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance)
				{
					result = xCharge.ChargeCode.Code == Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance ? 0 : -1;
				}
				else if (xCharge.ChargeCode.Code == Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight)
				{
					result = yCharge.ChargeCode.Code == Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight ? 0 : 1;
				}
			}

			return result;
		}
	}
}
