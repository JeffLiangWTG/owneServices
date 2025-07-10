using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public static class MENTAgedScoreExtractionFormFactory
	{
		static MENTAgedScoreQueryForm GetForm(MENTAgedScoreExtraction mentAgedScoreExtraction)
		{
			return new MENTAgedScoreQueryForm(mentAgedScoreExtraction.RelatedQuery);
		}

		public static IZForm ShowForm(IBusiness bizo)
		{
			var mentAgedScoreExtraction = bizo as MENTAgedScoreExtraction;

			if (mentAgedScoreExtraction != null)
			{
				return GetForm(mentAgedScoreExtraction);
			}

			throw new ArgumentException("BusinessObject type " + bizo.GetType() + " is not supported.");
		}

		public static void NavigateToExtractionItem(IZForm form, MENTAgedScoreExtraction extraction)
		{
			var tabPage = GetExtractionsTab(form);

			if (tabPage != null)
			{
				((Control)form).BeginInvoke(new Action(() => tabPage.NavigateToItem(extraction)));
			}
		}

		static MENTNavigationTabPage GetExtractionsTab(IZForm form)
		{
			var tab = ((Control)form).FindAll<MENTNavigationTabPage>(o => o.Name == "VisualisationConfigurationTabPage");
			return tab.FirstOrDefault();
		}
	}
}
