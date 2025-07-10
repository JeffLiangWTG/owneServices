using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryLineConfirmedFeeWrapper : NonPersistentBusinessObject
	{
		public CusEntryLineConfirmedFeeWrapper(CusEntryLineFee cusEntryLineFee)
		{
			this.cusEntryLineFee = Argument.NotNull(cusEntryLineFee, nameof(cusEntryLineFee));
		}

		protected override ZGuid GetPK() => cusEntryLineFee.PK;
		public ZString CF_ChargeType => cusEntryLineFee.CF_ChargeType;
		public ZString ChargeTypeDescription => cusEntryLineFee.ChargeTypeDescription;
		public ZString CF_RateOverrideReasonCode => cusEntryLineFee.CF_RateOverrideReasonCode;
		public ZDecimal CF_BaseValue => cusEntryLineFee.CF_BaseValue;
		public ZString CF_MethodOfCalculation => cusEntryLineFee.CF_MethodOfCalculation;
		public ZDecimal CF_Rate => cusEntryLineFee.CF_Rate;
		public ZDecimal CF_ChargeAmount => cusEntryLineFee.CF_ChargeAmount;
		public ZString CF_MethodOfPayment => cusEntryLineFee.CF_MethodOfPayment;

		readonly CusEntryLineFee cusEntryLineFee;
	}
}
