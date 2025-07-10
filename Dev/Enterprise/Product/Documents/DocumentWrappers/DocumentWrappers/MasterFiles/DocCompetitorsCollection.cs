using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCompetitorCollection : DocumentWrapperCollection<DocCompetitor>
	{
		public DocCompetitorCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
