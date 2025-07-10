using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class DocumentDataCache
	{
		public DocumentDataCache(BusinessObject parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
		}

		readonly BusinessObject parent;

		readonly IDictionary<string, IVisualizerDocumentData> cache = new Dictionary<string, IVisualizerDocumentData>();

		public IVisualizerDocumentData Get(string name)
		{
			IVisualizerDocumentData result = null;

			if (!string.IsNullOrEmpty(name)
				&& !cache.TryGetValue(name, out result))
			{
				result = parent.LoadOrCreateDocumentData(name);
				cache[name] = result;
			}

			return result;
		}
	}
}