using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using OxyPlot;

namespace Enterprise.PAVE.MENT.Business
{
	public class VisualisationViewModel : NonPersistentBusinessObject, INotifyPropertyChanged
	{
		public VisualisationViewModel(MENTAgedScoreExtraction extraction, MENTAgedScoreVisualisation visualisation, IVisualisationFactoryProvider provider)
		{
			this.extractor = new MENTAgedScoreExtractor(extraction, provider);
			this.visualiser = new MENTAgedScoreVisualiser(visualisation);
		}

		public VisualisationViewModel(MENTAgedScoreExtractor extractor, MENTAgedScoreVisualiser visualiser)
		{
			this.extractor = extractor;
			this.visualiser = visualiser;
		}

		readonly MENTAgedScoreVisualiser visualiser;
		readonly MENTAgedScoreExtractor extractor;

		public PlotModel PlotModel
		{
			get
			{
				if (plotModel == null)
				{
					plotModel = GenerateNewPlotModel();
				}

				return plotModel;
			}
		}

		PlotModel plotModel;

		public void RefreshModel()
		{
			plotModel = GenerateNewPlotModel();
		}

		PlotModel GenerateNewPlotModel()
		{
			return visualiser.Visualise(extractor.Extract());
		}

		#region INotifyPropertyChanged Members

		//this macro will suppress the compiler warning 'event never used' 
#pragma warning disable 0067
		//If you are not binding to a DependencyProperty or an object that implements INotifyPropertyChanged, 
		//then the binding can leak memory (and this is what happened when we open the board with MENT section graph - we have factories left in the memory).
		//please refer to the article below:
		//https://support.microsoft.com/en-au/kb/938416
		//If the object is not a DependencyProperty or does not implement INotifyPropertyChanged, then WPF uses the ValueChanged event.
		//This behavior involves calling the PropertyDescriptor.AddValueChanged method on the PropertyDescriptor object that corresponds to property P.
		//This action causes the common language runtime (CLR) to create a strong reference from this PropertyDescriptor object to object X.
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

		#endregion
	}
}
