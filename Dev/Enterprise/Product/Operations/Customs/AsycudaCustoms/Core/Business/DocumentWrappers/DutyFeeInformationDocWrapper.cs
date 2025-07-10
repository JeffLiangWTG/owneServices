using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers
{
	public class DutyFeeInformationDocWrapper : NonPersistentBusinessObject
	{
		public DutyFeeInformationDocWrapper(CusEntryLineFee input)
		{
			if (input != null)
			{
				Type = input.CF_ChargeType;
				Amount = input.CF_ChargeAmount.Round(2);
			}
		}

		public ZString Type { get; }

		public ZDecimal Amount { get; }
	}
}
