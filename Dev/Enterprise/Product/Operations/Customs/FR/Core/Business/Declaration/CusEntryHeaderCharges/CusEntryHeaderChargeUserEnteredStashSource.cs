using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderChargeUserEnteredStashSource : IUserEnteredStashSource
	{
		public CusEntryHeaderChargeUserEnteredStashSource(CusEntryHeaderCharges charge)
		{
			this.charge = Argument.NotNull(charge, nameof(charge));
		}

		readonly CusEntryHeaderCharges charge;

		IDictionary<ZPropertyInfo, ZBool> IUserEnteredStashSource.StashedPropertiesAndConditions => new Dictionary<ZPropertyInfo, ZBool>
			{
				{ charge.C1_MethodOfPaymentInfo, true }
			};

		ZBool IUserEnteredStashSource.ShouldStash()
		{
			return !charge.C1_MethodOfPayment.IsEmpty;
		}

		ZString IUserEnteredStashSource.GetStashKey()
		{
			return charge.C1_ChargeType;
		}
	}
}
