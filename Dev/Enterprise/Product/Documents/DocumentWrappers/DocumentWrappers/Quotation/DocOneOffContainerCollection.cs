
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOneOffContainerCollection : DocumentWrapperCollection
	{
		public DocOneOffContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocOneOffContainerCollection(RateOneOffContainersCollection oneOffContainersCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (RateOneOffContainers oneOffContainer in oneOffContainersCollection)
			{
				this.Add(DocOneOffContainer.New(oneOffContainer, factory));
			}
		}

		public new DocOneOffContainer this[int index]
		{
			get { return (DocOneOffContainer)base[index]; }
		}
	}
}
