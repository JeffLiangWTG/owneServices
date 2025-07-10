using System.Collections;

namespace Enterprise.DocumentEngine.Business
{
	public interface IDocumentConfigItemCollection : IList
	{
		void SortByPrintOrder();
	}
}
