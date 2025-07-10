using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuTemplatePivot : StmMenuTemplatePivotBase
	{
		public VisualizerMenuTemplatePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SI_PrintCopyType = nameof(PrintCopyType.ALL);
			SI_Index = 0;
		}

		public new VisualizerMenuItem MenuItem => Factory.Load<VisualizerMenuItem>(SI_SU);

		public new VisualizerTemplate Template => Factory.Load<VisualizerTemplate>(SI_SO);

		protected override StmMenuTemplatePivotValidation GetNewValidation() => new VisualizerMenuTemplatePivotValidation(this);
	}
}