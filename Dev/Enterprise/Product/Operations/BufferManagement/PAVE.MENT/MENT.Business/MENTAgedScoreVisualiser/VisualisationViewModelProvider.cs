using Enterprise.BufferManagement.Integration;

namespace Enterprise.PAVE.MENT.Business
{
	public class VisualisationViewModelProvider
	{
		public VisualisationViewModelProvider(MENTAgedScoreExtraction extraction, MENTAgedScoreVisualisation visualisation)
		{
			this.extraction = extraction;
			this.visualisation = visualisation;
		}

		readonly MENTAgedScoreExtraction extraction;
		readonly MENTAgedScoreVisualisation visualisation;

		public VisualisationViewModel GenerateViewModel(IVisualisationFactoryProvider provider)
		{
			var viewModelFactory = provider.GetNewEditFactory("VisualisationViewModelFactory");
			var loadedExtraction = viewModelFactory.Load<MENTAgedScoreExtraction>(extraction.PK);
			var loadedVisualisation = viewModelFactory.Load<MENTAgedScoreVisualisation>(visualisation.PK);

			return new VisualisationViewModel(loadedExtraction, loadedVisualisation, provider);
		}
	}
}
