using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocVesselCollection : DocumentWrapperCollection
	{
		public DocVesselCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocVessel this[int index]
		{
			get { return (DocVessel)base[index]; }
		}
	}
}

