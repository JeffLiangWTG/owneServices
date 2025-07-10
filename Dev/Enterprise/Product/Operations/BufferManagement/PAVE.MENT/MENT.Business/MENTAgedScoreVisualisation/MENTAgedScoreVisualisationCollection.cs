using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreVisualisationCollection : ActiveBusinessObjectCollection<MENTAgedScoreVisualisation>
	{
		public MENTAgedScoreVisualisationCollection(MENTAgedScoreExtraction extraction)
			: base(extraction.Factory, extraction, new ZQuery(), MENTAgedScoreVisualisationSchema.MVI_MEX)
		{
			Argument.NotNull(extraction, nameof(extraction));
			this.agedScoreExtraction = extraction;
		}

		readonly MENTAgedScoreExtraction agedScoreExtraction;

		public MENTAgedScoreExtraction AgedScoreExtraction
		{
			get { return agedScoreExtraction; }
		}

		protected override void SetDefaultsForNewElementCore(MENTAgedScoreVisualisation newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.MVI_IsCustomised = true;
		}
	}
}
