using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.BR.Business
{
	public class DocCusEntryLineFeeCollection : DocBaseCusEntryLineFeeCollection
	{
		public DocCusEntryLineFeeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocCusEntryLineFee this[int index] => (DocCusEntryLineFee)Elements[index];

		public new DocCusEntryLineFee this[string chargeType] => this.Cast<DocCusEntryLineFee>().FirstOrDefault(x => x.ChargeType == chargeType);
	}
}
