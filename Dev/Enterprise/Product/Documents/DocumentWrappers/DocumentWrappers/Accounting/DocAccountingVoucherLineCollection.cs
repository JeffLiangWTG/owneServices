using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingVoucherLineCollection : DocumentWrapperCollection
	{
		public DocAccountingVoucherLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocAccountingVoucherLine this[int index]
		{
			get { return (DocAccountingVoucherLine)base[index]; }
		}
	}
}
