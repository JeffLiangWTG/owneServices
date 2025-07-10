using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocJobLineDetailCollection : DocumentWrapperCollection
	{
		public DocJobLineDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocJobLineDetail this[int index]
		{
			get { return (DocJobLineDetail)base[index]; }
		}
	}
}
