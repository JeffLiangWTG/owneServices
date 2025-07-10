using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Core
{
	public interface IDocumentInfoPack
	{
		string Name { get; }
		IEnumerable<IDocumentInfo> DocumentInfos { get; }
	}
}
