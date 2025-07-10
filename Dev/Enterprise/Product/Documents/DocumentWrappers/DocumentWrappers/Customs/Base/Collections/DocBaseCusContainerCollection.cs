using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseCusContainerCollection : DocumentWrapperCollection
	{
		protected DocBaseCusContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected DocBaseCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseCusContainer this[int index]
		{
			get { return (DocBaseCusContainer)base[index]; }
		}

		public IDocSimpleContainerCollection ToIDocSimpleContainerCollection()
		{
			IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
			foreach (DocBaseCusContainer container in this)
			{
				result.Add(container);
			}
			return result;
		}
	}
}
