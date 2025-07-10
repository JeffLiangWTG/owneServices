using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocAccQueryClaimCollection : DocumentWrapperCollection
	{
		public DocAccQueryClaimCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocAccQueryClaim this[int index]
		{
			get { return (DocAccQueryClaim)base[index]; }
		}
	}
}
