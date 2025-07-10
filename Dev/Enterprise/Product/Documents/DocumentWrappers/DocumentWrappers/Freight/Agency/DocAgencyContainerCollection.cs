using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocAgencyContainerCollection : DocumentWrapperCollection
	{
		public DocAgencyContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocAgencyContainer this[int index]
		{
			get { return (DocAgencyContainer)base[index]; }
		}
	}
}
