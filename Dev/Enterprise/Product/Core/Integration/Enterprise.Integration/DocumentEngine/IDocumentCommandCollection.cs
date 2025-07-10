using System.Collections;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IDocumentCommandCollection : IList
	{
		void Load();
	}
}
