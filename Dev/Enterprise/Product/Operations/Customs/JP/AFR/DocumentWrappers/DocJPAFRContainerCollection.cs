using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers
{
	public class DocJPAFRContainerCollection : DocumentWrapperCollection
	{
		public DocJPAFRContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJPAFRContainerCollection(DocJPAFRContainerCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJPAFRContainer this[int index]
		{
			get { return (DocJPAFRContainer)Elements[index]; }
		}
	}
}
