using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public interface INotesTabPage
	{
		bool HasRelatedNotes { get; }
		bool HasNotes { get; }
		int ImageIndex { get; set; }
		IBusiness BusinessEntity { get; }
	}

	public static class INotesTabPageExtensionMethods
	{
		public static void UpdateNoteImage(this INotesTabPage tabPage)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("UpdateNoteImage"))
			{
				if (tabPage.BusinessEntity != null)
				{
					if (tabPage.HasRelatedNotes)
					{
						if (tabPage.ImageIndex == -1 || tabPage.ImageIndex == Icons.GetImageIndex(IconTypes.StmNote))
						{
							tabPage.ImageIndex = Icons.GetImageIndex(IconTypes.StmNoteRelated);
						}
					}
					else if (tabPage.HasNotes)
					{
						if (tabPage.ImageIndex == -1 || tabPage.ImageIndex == Icons.GetImageIndex(IconTypes.StmNoteRelated))
						{
							tabPage.ImageIndex = Icons.GetImageIndex(IconTypes.StmNote);
						}
					}
					else if (tabPage.ImageIndex == Icons.GetImageIndex(IconTypes.StmNoteRelated) || tabPage.ImageIndex == Icons.GetImageIndex(IconTypes.StmNote))
					{
						tabPage.ImageIndex = -1;
					}
				}
			}
		}
	}
}
