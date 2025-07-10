using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTransactionLineCollection : DocumentWrapperCollection
	{
		public DocTransactionLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocTransactionLine this[int index]
		{
			get { return (DocTransactionLine)base[index]; }
		}
	}
}
