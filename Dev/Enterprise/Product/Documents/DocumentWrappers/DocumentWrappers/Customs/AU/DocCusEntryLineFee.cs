using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryLineFee : Base.DocBaseCusEntryLineFee
	{
		DocCusEntryLineFee(CusEntryLineFee cusEntryLineFee, BusinessObjectFactory factory)
			: base(cusEntryLineFee, factory)
		{
		}

		public static DocCusEntryLineFee New(CusEntryLineFee cusEntryLineFee, BusinessObjectFactory factory)
		{
			return (cusEntryLineFee == null) ? null : new DocCusEntryLineFee(cusEntryLineFee, factory);
		}

		CusEntryLineFee CusEntryLineFee
		{
			get { return (CusEntryLineFee)WrappedObject; }
		}

		public ZBool IsPayableToCustomsForHeader
		{
			get { return CusEntryLineFee.IsPayableToCustomsForLine; }
		}
	}
}
