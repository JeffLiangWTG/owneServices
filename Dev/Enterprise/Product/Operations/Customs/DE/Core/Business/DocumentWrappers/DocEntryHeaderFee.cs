using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocEntryHeaderFee : DocBaseWrapper
	{
		DocEntryHeaderFee(EntryFee cusEntryLineFee, BusinessObjectFactory factory)
			: base(cusEntryLineFee, factory)
		{
		}

		public static DocEntryHeaderFee New(EntryFee cusEntryLineFee, BusinessObjectFactory factory) => cusEntryLineFee == null ? null : new DocEntryHeaderFee(cusEntryLineFee, factory);

		public ZString Description => Fee.Description;

		public ZString ChargeType => Fee.ChargeType;

		public ZString ChargeAmountAsString => Fee.ChargeAmount.ToStringRounded(2);

		public ZString BaseValue => Fee.BaseValue.ToStringRounded(2);

		public ZString MethodOfCalculation => Fee.MethodOfCalculation;

		public ZString Rate => Fee.Rate.ToStringRounded(6);

		EntryFee Fee => fee ??= (EntryFee)WrappedObject;
		EntryFee fee;
	}
}
