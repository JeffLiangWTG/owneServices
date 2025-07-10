using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

#if WINZOR
using CargoWise.Main.Navigation.ViewModels;
#endif

namespace CargoWise.Main.Navigation;

public class NewsViewModel : ViewModelBase
#if WINZOR
	, INewsViewModel
#endif
{
	static string SectionNameAll => Res.GetString("b892810a-8951-41c6-bbc7-0bec04927316", "All");

#if WINZOR
	public string NameAll
	{
		get
		{
			return SectionNameAll;
		}
	}
#endif
	public TopOrBottom SectionsToLoad { get; set; }

	public enum TopOrBottom
	{
		Top,
		Bottom
	}

	List<NewsItemViewModel> allNewsItems;

	ObservableCollection<NewsSectionViewModel> sections;

	public ObservableCollection<NewsSectionViewModel> Sections
	{
		get => sections;
		set
		{
			sections = value;
			OnPropertyChanged();
		}
	}

	NewsSectionViewModel selectedSection;
	public NewsSectionViewModel SelectedSection
	{
		get => selectedSection;
		set
		{
			if (selectedSection != value)
			{
				if (selectedSection != null)
				{
					selectedSection.IsSelected = false;
				}
				selectedSection = value;
				selectedSection.IsSelected = true;
				OnPropertyChanged();
				RefreshSelectedSection();
			}
		}
	}

#if WINZOR
	ObservableCollection<INewsItemViewModel> INewsViewModel.FilteredNewsItems
	{
		get => FilteredNewsItems is null ? null : new ObservableCollection<INewsItemViewModel>(FilteredNewsItems);
		set => throw new NotImplementedException();
	}
	INewsSectionViewModel INewsViewModel.SelectedSection
	{
		get => SelectedSection;
		set => SelectedSection = (NewsSectionViewModel)value;
	}
	ObservableCollection<INewsSectionViewModel> INewsViewModel.Sections
	{
		get => Sections is null ? null : new ObservableCollection<INewsSectionViewModel>(Sections);
		set => throw new NotImplementedException();
	}
#else
	public System.Windows.Visibility NoNewsPanelVisibility => allNewsItems?.Count > 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

	ICollectionView filteredNewsItemsView;
	public ICollectionView FilteredNewsItemsView
	{
		get => filteredNewsItemsView;
		set
		{
			filteredNewsItemsView = value;
			OnPropertyChanged();
		}
	}
#endif

	ObservableCollection<NewsItemViewModel> filteredNewsItems;
	public ObservableCollection<NewsItemViewModel> FilteredNewsItems
	{
		get => filteredNewsItems;
		set
		{
			filteredNewsItems = value;
			OnPropertyChanged();
#if !WINZOR
			if (filteredNewsItems != null)
			{
				FilteredNewsItemsView = System.Windows.Data.CollectionViewSource.GetDefaultView(filteredNewsItems);
				FilteredNewsItemsView.SortDescriptions.Add(
					new SortDescription(nameof(NewsItemViewModel.ReleaseDate), ListSortDirection.Descending));
			}
#endif
		}
	}

	#region Commands
	ICommand filterCommand;
	public ICommand FilterCommand
	{
		get => filterCommand;
		set
		{
			filterCommand = value;
			OnPropertyChanged();
		}
	}

	ICommand showItemCommand;
	public ICommand ShowItemCommand
	{
		get => showItemCommand;
		set
		{
			showItemCommand = value;
			OnPropertyChanged();
		}
	}

	void ExecuteFilterCommand(object parameter)
	{
		SelectedSection = parameter as NewsSectionViewModel;
	}

#if WINZOR
	public
#endif
	void ExecuteShowItemCommand(object parameter)
	{
		var newsItem = (parameter as NewsItemViewModel)?.NewsItem;
		if (newsItem != null)
		{
#if !WINZOR
			Enterprise.Startup.StartupOpenMainFormTask.MainFormInstance.Invoke(
				() => ShowItemCore(newsItem));
#else
			ShowItemCore(newsItem);
#endif
		}
	}
	#endregion

	void RefreshSelectedSection()
	{
		if (allNewsItems == null || FilteredNewsItems == null)
		{
			return;
		}

		foreach (var newsItem in allNewsItems)
		{
			var shouldShow = selectedSection.SectionName == SectionNameAll || newsItem.SectionName == selectedSection.SectionName;
			if (shouldShow && !FilteredNewsItems.Contains(newsItem))
			{
				FilteredNewsItems.Add(newsItem);
			}
			else if (!shouldShow && FilteredNewsItems.Contains(newsItem))
			{
				FilteredNewsItems.Remove(newsItem);
			}
		}

#if !WINZOR
		FilteredNewsItemsView?.Refresh();
#endif
	}

	internal bool AllowAutoLogin { get; set; } = true;

	bool ShouldAutoLogin => AllowAutoLogin && GlbStaff.CurrentUser != null && !GlbStaff.CurrentUser.GS_IsSystemAccount;

	async void ShowItemCore(GlbReleaseNoteCombined item)
	{
		try
		{
			using (new ZWaitCursorChanger())
			{
				if (ShouldAutoLogin && item.IsWiseTechGlobalItemViaTrustedMessaging)
				{
					if (!await ObjectFactory.Get<ISystemUserAccountCollectionTermChecker>().CheckTermAcknowledged())
					{
						return;
					}
				}
				WebUrlLauncher.Launch(ShouldAutoLogin ? item.GetDownloadURL() : item.GF_URL);
			}
		}
		catch (Win32Exception)
		{
			Globals.Message.Show(Res.GetString("98792D8A-18DD-488E-A7E4-3091D2921899", "The URL specified for this news item is invalid or not found."));
		}
		catch (FileNotFoundException)
		{
			Globals.Message.Show(Res.GetString("98792D8A-18DD-488E-A7E4-3091D2921899", "The URL specified for this news item is invalid or not found."));
		}
	}

	BusinessObjectFactory factory;
	public NewsViewModel()
	{
		if (IsDesignMode)
		{
			LoadDesignModeNewsItems();
			return;
		}

		FilterCommand = new RelayCommand(ExecuteFilterCommand);
		ShowItemCommand = new RelayCommand(ExecuteShowItemCommand);

		//Add dummy item to show control
		allNewsItems =
		[
			new NewsItemViewModel(summary: "-", releaseDate: new DateTime(2024, 10, 1))
		];
	}

	void LoadDesignModeNewsItems()
	{
#pragma warning disable CW1161 // Res.GetString Analyzer
		var newSections = new List<NewsSectionViewModel>
		{
			new ( sectionID: "1", sectionName: "WiseTech Global Pty Ltd News"),
			new ( sectionID: "2", sectionName: "FedEx News Section"),
			new ( sectionID: "3", sectionName: "FedEx Updates Section"),
		};
		Sections = new ObservableCollection<NewsSectionViewModel>(newSections);
		Sections.Insert(0, new NewsSectionViewModel(sectionID: "", sectionName: SectionNameAll));
		SelectedSection = Sections[0];
		allNewsItems =
		[
			new NewsItemViewModel(summary: "News Item 1, What ever that is important to users, important to users, important to users, important to users", releaseDate: new DateTime(2024,10,1)),
			new NewsItemViewModel(summary: "News Item 2, What ever that is important to users", releaseDate: new DateTime(2024,10,1)),
			new NewsItemViewModel(summary: "News Item 3, What ever that is important to users", releaseDate: new DateTime(2024,10,1)),
			new NewsItemViewModel(summary: "News Item 4, What ever that is important to users", releaseDate: new DateTime(2024,10,1)),
			new NewsItemViewModel(summary: "News Item 5, What ever that is important to users", releaseDate: new DateTime(2024,10,1)),
			new NewsItemViewModel(summary: "News Item 6, What ever that is important to users", releaseDate: new DateTime(2024,10,1)),
		];
		FilteredNewsItems = new ObservableCollection<NewsItemViewModel>(allNewsItems);
#pragma warning restore CW1161 // Res.GetString Analyzer
	}

	internal void LoadNewsItems()
	{
		factory = new BusinessObjectFactory { NameForDebugging = nameof(NewsViewModel) };

		var newsSections = NewsSections.ToList();
		Sections = new ObservableCollection<NewsSectionViewModel>();

		allNewsItems = new List<NewsItemViewModel>();
		foreach (var section in newsSections)
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			if (SectionsToLoad == TopOrBottom.Top
				&& !section.LayoutPanelID.StartsWith("Top", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			if (SectionsToLoad == TopOrBottom.Bottom
				&& !section.LayoutPanelID.StartsWith("Bottom", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
#pragma warning restore CW1161 // Res.GetString Analyzer

			var newsItems = GetNews(factory, section.SectionID, section.HideReadItems);
			if (newsItems.Length > 0)
			{
				Sections.Add(new NewsSectionViewModel(section));
				foreach (var newsItem in newsItems)
				{
					allNewsItems.Add(new NewsItemViewModel(newsItem, section));
				}
			}
		}

		Sections.Insert(0, new NewsSectionViewModel(sectionID: "", sectionName: Res.GetString("b892810a-8951-41c6-bbc7-0bec04927316", "All")));
		SelectedSection = Sections[0];

		FilteredNewsItems = new ObservableCollection<NewsItemViewModel>(allNewsItems);

#if !WINZOR
		OnPropertyChanged(nameof(NoNewsPanelVisibility));
#endif
	}

	public IEnumerable<NewsSection> NewsSections
	{
		get
		{
#if DEBUG
			if (ThrowExceptionInNewsSections)
			{
				throw new DummyDbExceptionForTest(
					"SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time.");
			}
#endif
			var panelConfigs = SystemDataRegistry.Instance.NewsSectionLayouts.Value;
			IEnumerable<NewsSection> newsSections = panelConfigs.Cast<NewsSection>().Where(x => !string.IsNullOrEmpty(x.SectionID));
			if (GlbStaff.CurrentUser == null)
			{
				newsSections = newsSections.Where(x => x.IsWiseTechSection);
			}
			return newsSections.ToArray();
		}
	}

	public static GlbReleaseNoteCombined[] GetNews(BusinessObjectFactory factory, string sectionID, bool hideReadItems)
	{
#if DEBUG
		if (ThrowExceptionInGetNews)
		{
			throw new Exception("Forced exception in GetNews");
		}
#endif

		var query = new ZDBOnlyQuery(typeof(GlbReleaseNoteCombined));

		if (hideReadItems && GlbStaff.CurrentUser != null)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(GlbReleaseNoteRead), GlbReleaseNoteReadSchema.GR_ReleaseNoteID, true);
			subQuery.AddToFilter(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
		}

		if (!string.IsNullOrWhiteSpace(sectionID))
		{
			var conditions = new ZQuery();
			conditions.AddToFilter(GlbReleaseNoteCombinedSchema.GF_Section, sectionID);

			if (sectionID.Equals(NewsSectionTypeList.Codes.ProductUpdates))
			{
				conditions.AddToFilter(JoinCondition.Or, GlbReleaseNoteCombinedSchema.GF_Section, NewsSectionTypeList.Codes.TechnicalAdvisoryNotes);
				conditions.AddToFilter(JoinCondition.Or, GlbReleaseNoteCombinedSchema.GF_Section, NewsSectionTypeList.Codes.BorderWise);
			}

			query.AddToFilter(conditions);
		}

		var countryCodes = new List<string> { GlbReleaseNoteCombinedLookups.AllCountriesCode };

		if (GlbCompany.CurrentCompany != null)
		{
			countryCodes.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		query.AddToFilter(GlbReleaseNoteCombinedSchema.GF_RN_NKCountryForReleaseNote, countryCodes);

		var sectionTypes = SystemDataRegistry.Instance.NewsSectionTypes.Value.OfType<NewsAnnouncementSectionType>();
		var sectionType = sectionTypes.FirstOrDefault(s => s.Code == sectionID);

		if (sectionType != null && sectionType.OrderItemsBy == NewsSectionSortTypeList.Codes.Alphabetically)
		{
			query.OrderBy = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} ASC", AutoGlbReleaseNoteCombined.Schema.GF_Summary);
		}
		else
		{
			query.OrderBy = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} DESC", AutoGlbReleaseNoteCombined.Schema.GF_ReleaseNoteDate);
		}

		query.MaximumRows = SystemDataRegistry.NewsMaximumRows;

		var rows = factory.Load<GlbReleaseNoteCombined>(query);

		if ((sectionID == NewsSectionTypeList.Codes.ProductUpdates || sectionID == NewsSectionTypeList.Codes.WiseLearningUpdates) && DataRegistry.Instance.ProductivityWiseModeEnabled)
		{
			var lookups = new GlbReleaseNoteCombinedLookups(null);
			return rows.Where(x => lookups.VisibleCategories.Contains(x.GF_Category)).ToArray();
		}

		return rows;
	}

#if DEBUG
	[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
	public static bool ThrowExceptionInGetNews = false;
	internal bool ThrowExceptionInNewsSections { get; set; }

	/// <summary>
	/// TODO: to be removed in WI00904057
	/// </summary>
	[Serializable]
	class DummyDbExceptionForTest : System.Data.Common.DbException
	{
		public DummyDbExceptionForTest(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected DummyDbExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
#endif
}
