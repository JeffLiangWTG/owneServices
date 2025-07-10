using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryHeaderCharges : Base.DocBaseCusEntryHeaderCharges
	{
		DocCusEntryHeaderCharges(CusEntryHeaderCharges cusEntryHeaderCharges, BusinessObjectFactory factory)
			: base(cusEntryHeaderCharges, factory)
		{
		}

		public static DocCusEntryHeaderCharges New(CusEntryHeaderCharges cusEntryHeaderCharges, BusinessObjectFactory factory)
		{
			return (cusEntryHeaderCharges == null) ? null : new DocCusEntryHeaderCharges(cusEntryHeaderCharges, factory);
		}

		CusEntryHeaderCharges CusEntryHeaderCharges
		{
			get { return (CusEntryHeaderCharges)WrappedObject; }
		}

		public ZBool IsPayableToCustomsForHeader
		{
			get { return CusEntryHeaderCharges.IsPayableToCustomsForHeader; }
		}
	}
}
