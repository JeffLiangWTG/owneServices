using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuTemplatePivotCollection : StmMenuTemplatePivotBaseCollection
	{
		public VisualizerMenuTemplatePivotCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public new VisualizerMenuTemplatePivot this[int index]
		{
			get { return (VisualizerMenuTemplatePivot)Elements[index]; }
		}

		public new VisualizerMenuTemplatePivot AddNew()
		{
			return (VisualizerMenuTemplatePivot)base.AddNew();
		}
	}
}