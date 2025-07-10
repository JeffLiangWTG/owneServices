
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

#region DocForwardingConsolCollection

namespace Enterprise.DocumentWrappers.Freight.Forwarding
{
	public class DocForwardingConsolCollection : DocBaseConsolCollection
	{
		public DocForwardingConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocForwardingConsolCollection(ConsolCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocForwardingConsol this[int index]
		{
			get { return (DocForwardingConsol)Elements[index]; }
		}
	}
}

#endregion

#region DocBaseConsolCollection

namespace Enterprise.DocumentWrappers.Freight
{
	public class DocBaseConsolCollection : DocumentWrapperCollection
	{
		public DocBaseConsolCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocBaseConsolCollection(ConsolCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseConsol this[int index]
		{
			get { return (DocBaseConsol)base[index]; }
		}
	}
}

#endregion
