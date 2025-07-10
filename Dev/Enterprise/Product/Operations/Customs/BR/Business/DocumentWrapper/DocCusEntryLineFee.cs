using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.BR.Business
{
	public class DocCusEntryLineFee : DocBaseCusEntryLineFee
	{
		DocCusEntryLineFee(CusEntryLineFee cusEntryLineFee, BusinessObjectFactory factory)
			: base(cusEntryLineFee, factory)
		{
		}

		public static DocCusEntryLineFee New(CusEntryLineFee cusEntryLineFee, BusinessObjectFactory factory)
		{
			return cusEntryLineFee == null ? null : new DocCusEntryLineFee(cusEntryLineFee, factory);
		}

		CusEntryLineFee CusEntryLineFee => (CusEntryLineFee)WrappedObject;

		public ZDecimal BaseValue => CusEntryLineFee.CF_BaseValue;

		public ZDecimal Rate => CusEntryLineFee.CF_Rate;

		public ZString MethodOfCalculation => CusEntryLineFee.CF_MethodOfCalculation;
	}
}
