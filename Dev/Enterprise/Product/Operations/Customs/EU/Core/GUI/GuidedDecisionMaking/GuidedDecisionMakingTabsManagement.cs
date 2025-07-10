using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class GuidedDecisionMakingTabsManagement
	{
		protected readonly ZTabControl tabControl;
		protected readonly List<GuidedDecisionMakingTab> tabs;

		public GuidedDecisionMakingTabsManagement(ZTabControl tabControl)
		{
			this.tabControl = tabControl;
			tabs = new List<GuidedDecisionMakingTab>();
		}

		public void AddTab(GuidedDecisionMakingTab guidedDecisionMakingTab)
		{
			tabs.Add(guidedDecisionMakingTab);
		}

		public void InvalidateTabControl()
		{
			if (RecalculateTabStatuses())
			{
				tabControl.Refresh();
			}
		}

		bool RecalculateTabStatuses()
		{
			var lastStatuses = tabs.Select(x => x.Status).ToList();

			foreach (var gdmTab in tabs)
			{
				var tab = gdmTab.Tab;
				var isApplicable = gdmTab.IsApplicable;
				if (tabControl.SelectedTab == tab)
				{
					gdmTab.Status = GuidedDecisionMakingTabStatus.Current;
				}
				else if (isApplicable())
				{
					if (tab.TabIndex < SelectedIndex)
					{
						if (gdmTab.GetUnsatisfiedGroupDescriptions().IsEmpty)
						{
							gdmTab.Status = GuidedDecisionMakingTabStatus.Completed;
						}
						else
						{
							gdmTab.Status = GuidedDecisionMakingTabStatus.CompletedWithWarning;
						}
					}
					else
					{
						gdmTab.Status = GuidedDecisionMakingTabStatus.Incomplete;
					}
				}
				else
				{
					gdmTab.Status = GuidedDecisionMakingTabStatus.NotApplicable;
				}
#if WINZOR
				tab.Text = gdmTab.FullCaption;
				tab.CaptionBackgroundColor = gdmTab.CaptionBackground;
				tab.IconIndex = gdmTab.IconIndex;
#endif
			}

			var currentStatuses = tabs.Select(x => x.Status).ToList();
			return !currentStatuses.SequenceEqual(lastStatuses);
		}

		public void NavigateToNext()
		{
			if (SelectedGuidedDecisionMakingTab.CanNavigateToNext())
			{
				if (NextApplicableTab is GuidedDecisionMakingTab tab)
				{
					tabControl.SelectedIndex = tab.Tab.TabIndex;
					InvalidateTabControl();
				}
			}
		}

		public void NavigateToPrevious()
		{
			if (PreviousApplicableTab is GuidedDecisionMakingTab tab)
			{
				tabControl.SelectedIndex = tab.Tab.TabIndex;
				InvalidateTabControl();
			}
		}

		public GuidedDecisionMakingTab FindGuidedDecisionMakingTab(ZTabPage tab)
		{
			return tabs.Find(x => x.Tab == tab);
		}

		GuidedDecisionMakingTab SelectedGuidedDecisionMakingTab => tabs.Find(x => x.Tab == tabControl.SelectedTab);

		int SelectedIndex => tabs.IndexOf(SelectedGuidedDecisionMakingTab);

		List<GuidedDecisionMakingTab> PreviousApplicableTabs => tabs.Take(SelectedIndex).Where(x => x.IsApplicable()).ToList();

		public bool HasPreviousTabs => PreviousApplicableTabs.Any();

		public GuidedDecisionMakingTab PreviousApplicableTab => PreviousApplicableTabs.LastOrDefault();

		public string PreviousButtonText => PreviousApplicableTab == null ? Res.GetString("8F991127-F274-41DC-B90F-202227426ED8", "Previous") : Res.GetString("F98D748A-BBCF-4F23-B1A8-892AC7DBAAD3", "Previous ({0})", PreviousApplicableTab.Caption);

		List<GuidedDecisionMakingTab> NextApplicableTabs => tabs.Skip(SelectedIndex + 1).Where(x => x.IsApplicable()).ToList();

		public GuidedDecisionMakingTab NextApplicableTab => NextApplicableTabs.FirstOrDefault();

		public bool HasNextTabs => NextApplicableTabs.Any();

		public string NextButtonText => NextApplicableTab == null ? Res.GetString("AEA57CFB-7394-42A2-A2D1-CB92261CA1BF", "Next") : Res.GetString("CB134A50-B234-4B34-BFA2-1467B0412657", "Next ({0})", NextApplicableTab.Caption);
	}
}
