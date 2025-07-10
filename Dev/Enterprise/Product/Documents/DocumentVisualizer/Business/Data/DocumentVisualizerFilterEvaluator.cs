using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class DocumentVisualizerFilterEvaluator : IDocumentVisualizerFilterEvaluator
	{
		public bool IsApplicable(BusinessObject bizObj, ZString filter, params BusinessObject[] otherDataSources) => bizObj.IsApplicable(filter);
	}
}
