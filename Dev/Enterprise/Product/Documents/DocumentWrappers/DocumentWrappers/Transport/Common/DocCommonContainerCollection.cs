using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCommonContainerCollection : DocumentWrapperCollection<DocCommonContainer>
	{
		public DocCommonContainerCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public DocCommonContainerCollection(CommonCartage cartage)
			: base(cartage.Factory)
		{
			foreach (CommonContainer container in cartage.Containers)
			{
				Add(DocCommonContainer.New(container, cartage, cartage.Factory));
			}
		}

		public IDocSimpleContainerCollection ToIDocSimpleContainerCollection()
		{
			IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
			foreach (DocCommonContainer container in this)
			{
				result.Add(container);
			}
			return result;
		}
	}
}
