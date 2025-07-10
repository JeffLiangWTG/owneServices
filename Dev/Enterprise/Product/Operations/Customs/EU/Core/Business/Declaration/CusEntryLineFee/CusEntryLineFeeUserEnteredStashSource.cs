using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryLineFeeUserEnteredStashSource : IUserEnteredStashSource
	{
		public CusEntryLineFeeUserEnteredStashSource(CusEntryLineFee entryLineFee)
		{
			this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
		}
		protected CusEntryLineFee entryLineFee;

		IDictionary<ZPropertyInfo, ZBool> IUserEnteredStashSource.StashedPropertiesAndConditions => GetStashedPropertiesAndConditionsCore();

		protected virtual IDictionary<ZPropertyInfo, ZBool> GetStashedPropertiesAndConditionsCore()
		{
			return new Dictionary<ZPropertyInfo, ZBool>
			{
				{ entryLineFee.CF_MethodOfPaymentInfo, true },
				{ entryLineFee.NationalFeeTypeCodeInfo, true }
			};
		}

		ZBool IUserEnteredStashSource.ShouldStash()
		{
			return !entryLineFee.CF_MethodOfPayment.IsEmpty || !entryLineFee.NationalFeeTypeCode.IsEmpty;
		}

		ZString IUserEnteredStashSource.GetStashKey()
		{
			return FormattableString.Invariant($"{entryLineFee.CF_ChargeType}_{entryLineFee.CF_MethodOfCalculation}_{entryLineFee.EntryLine?.PK}");
		}
	}
}
