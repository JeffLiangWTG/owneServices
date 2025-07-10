using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerDocumentDataLoader : IVisualizerDocumentDataLoader
	{
		IEnumerable<Integration.IVisualizerDocumentData> IVisualizerDocumentDataLoader.Load(BusinessObject parent)
		{
			return parent.LoadDocumentData();
		}

		Integration.IVisualizerDocumentData IVisualizerDocumentDataLoader.Load(BusinessObject parent, string dataStoreName)
		{
			IVisualizerDocumentData result = null;

			if (!string.IsNullOrWhiteSpace(dataStoreName))
			{
				result = parent.LoadDocumentData(dataStoreName);
			}

			return result;
		}
	}
}
