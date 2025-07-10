using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCFSShipmentCollection : DocumentWrapperCollection
	{
		public DocCFSShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocCFSShipment this[int index]
		{
			get { return (DocCFSShipment)base[index]; }
		}
	}
}
