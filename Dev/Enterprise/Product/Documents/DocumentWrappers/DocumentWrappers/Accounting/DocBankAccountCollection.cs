using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocBankAccountCollection : DocumentWrapperCollection
	{
		public DocBankAccountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocBankAccount this[int index]
		{
			get { return (DocBankAccount)base[index]; }
		}
	}
}

