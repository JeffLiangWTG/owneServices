using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocJobSupplierBookingLineCollection : DocumentWrapperCollection
	{
		public DocJobSupplierBookingLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocJobSupplierBookingLine this[int index]
		{
			get { return (DocJobSupplierBookingLine)base[index]; }
		}
	}
}
