using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.TIP.DocWrappers
{
	public class DocTIPHeaderLineTransactionCollection : DocumentWrapperCollection
	{
		public DocTIPHeaderLineTransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocTIPHeaderLineTransaction this[int index]
		{
			get { return (DocTIPHeaderLineTransaction)base[index]; }
		}
	}
}
