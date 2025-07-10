using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocAgencyDetentionAdviceContainerCollection : DocBaseWrapperCollection<DocAgencyDetentionAdviceLine>
	{
		public DocAgencyDetentionAdviceContainerCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
