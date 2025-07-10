using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryLineFeeUserEnteredStashSource : IUserEnteredStashSource
	{
		public CusEntryLineFeeUserEnteredStashSource(CusEntryLineFee entryLineFee)
		{
			this.entryLineFee = Argument.NotNull(entryLineFee, nameof(entryLineFee));
		}

		public IDictionary<ZPropertyInfo, ZBool> StashedPropertiesAndConditions => GetStashedPropertiesAndConditionsCore();

		public ZBool ShouldStash()
		{
			return !entryLineFee.CF_MethodOfPayment.IsEmpty;
		}

		public ZString GetStashKey()
		{
			return FormattableString.Invariant($"{entryLineFee.CF_ChargeType}_{entryLineFee.CF_MethodOfCalculation}_{entryLineFee.EntryLine?.PK}");
		}

		protected IDictionary<ZPropertyInfo, ZBool> GetStashedPropertiesAndConditionsCore()
		{
			return new Dictionary<ZPropertyInfo, ZBool>
			{
				{ entryLineFee.CF_MethodOfPaymentInfo, true },
			};
		}

		readonly CusEntryLineFee entryLineFee;
	}
}
