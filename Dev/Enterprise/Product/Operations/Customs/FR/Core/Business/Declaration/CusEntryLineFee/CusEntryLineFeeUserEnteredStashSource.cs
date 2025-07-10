using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineFeeUserEnteredStashSource : EU.Business.Declaration.CusEntryLineFeeUserEnteredStashSource
	{
		public CusEntryLineFeeUserEnteredStashSource(CusEntryLineFee entryLineFee) : base(entryLineFee)
		{
		}

		protected new CusEntryLineFee entryLineFee => (CusEntryLineFee)base.entryLineFee;

		protected override IDictionary<ZPropertyInfo, ZBool> GetStashedPropertiesAndConditionsCore()
		{
			var result = base.GetStashedPropertiesAndConditionsCore();
			result.Add(entryLineFee.CF_ChargeAmountInfo, entryLineFee.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Precalcule);
			return result;
		}
	}
}
