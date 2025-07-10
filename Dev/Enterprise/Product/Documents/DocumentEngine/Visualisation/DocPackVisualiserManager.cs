using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Visualisation
{
	public sealed class DocPackVisualiserManager : IVisualizerManager
	{
		public DocPackVisualiserManager(DocumentPack packUsedToGetDataSet, DeliverableCollectionView delivarableCollectionUsedToGetListOfReportsToShowBasedOnRecipients)
		{
			Pack = packUsedToGetDataSet;
			DelivarableCollection = delivarableCollectionUsedToGetListOfReportsToShowBasedOnRecipients;
		}

		readonly DocumentPack Pack;
		readonly DeliverableCollectionView DelivarableCollection;

		List<Report> reports;
		public IEnumerable<Report> Reports
		{
			get
			{
				if (reports == null)
				{
					reports = new List<Report>();

					foreach (IDeliverable deliverable in DelivarableCollection)
					{
						var report = deliverable as Report;
						if (report != null)
						{
							reports.Add(report);
						}
					}
				}

				return reports;
			}
		}

		public void RebuildDocPackIfLanguageChanged(DeliveryInstructions instructions)
		{
			Pack.RebuildIfLanguageChanged(instructions);
		}

		public void SaveData()
		{
			Pack.SaveVisualizerContentNote();
		}

		public void ClearData()
		{
			Pack.DeleteVisualizerContentNote();
		}

		public void RevertData()
		{
			Pack.RevertVisualizerNote();
		}
	}
}
