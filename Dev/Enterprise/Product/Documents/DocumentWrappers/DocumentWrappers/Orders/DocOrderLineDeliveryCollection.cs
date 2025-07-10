using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocOrderLineDeliveryCollection : DocumentWrapperCollection
	{
		public DocOrderLineDeliveryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocOrderLineDelivery this[int index]
		{
			get { return (DocOrderLineDelivery)base[index]; }
		}
	}
}

