using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IVisualizerDocumentDataLoader
	{
		IEnumerable<IVisualizerDocumentData> Load(BusinessObject parent);
		IVisualizerDocumentData Load(BusinessObject parent, string dataStoreName);
	}
}
