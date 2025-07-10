using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocJobPaymentBasisCollection : DocumentWrapperCollection
	{
		public DocJobPaymentBasisCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocJobPaymentBasis this[int index]
		{
			get { return (DocJobPaymentBasis)base[index]; }
		}
	}
}
