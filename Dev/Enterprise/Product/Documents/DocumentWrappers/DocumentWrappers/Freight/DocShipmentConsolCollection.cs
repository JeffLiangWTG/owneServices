using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocShipmentConsolCollection : DocumentWrapperCollection
	{
		public DocShipmentConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocShipmentConsol this[int index]
		{
			get { return (DocShipmentConsol)base[index]; }
		}
	}
}
