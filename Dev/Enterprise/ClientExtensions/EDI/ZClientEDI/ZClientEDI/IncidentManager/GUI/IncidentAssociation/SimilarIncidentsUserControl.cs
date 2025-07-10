using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.GUI.TileBar;
using CargoWise.Main.Navigation;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Core.Modules;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Newtonsoft.Json;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class SimilarIncidentsUserControl : ZUserControl
	{
		SimilarIncidentsWrapper similarIncidentsWrapper;
		RecentItemsControl incidentsListControl;
		(string DateRange, bool AllCustomers, string IncidentStatus) activeOptionTuple;

		public SimilarIncidentsUserControl()
		{
			InitializeComponent();
			SetupSimilarIncidentsList();
		}

		void SetupSimilarIncidentsList()
		{
			incidentsListControl = new RecentItemsControl
			{
				PanelBackgroundColor = SystemDataRegistry.Instance.ColorTheme.NavBarRecentPanelBackground,
				PanelBorderColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1
			};
			hostControl.Child = incidentsListControl;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource == null)
			{
				base.SetDataBinding(null, "");
				similarIncidentsWrapper = null;
				return;
			}

			var supportIncident = dataSource as SupportIncident ?? throw new ArgumentException("Can bind only to SupportIncident parent object, but was " + dataSource.GetType().FullName, nameof(dataSource));

			similarIncidentsWrapper = new SimilarIncidentsWrapper(supportIncident);
			base.SetDataBinding(similarIncidentsWrapper, "");

			dateRangeControl.SetDataBinding(similarIncidentsWrapper.Filter.DateFilter, "");
			if (EDIDataRegistry.Instance.EnableIncidentSimilarityPreload.Value)
			{
				RefreshSimilarIncidentsList();
			}
		}

		void ButtonFind_Click(object sender, EventArgs e)
		{
				RefreshSimilarIncidentsList();
		}

		void RefreshSimilarIncidentsList()
		{
			DisposeOfRecentItems();

			if (similarIncidentsWrapper == null)
			{
				return;
			}

			var caption = ResString.GetMultilingualString("61c73523-6fb6-497b-8cbe-01230ac898db", "Similar Incidents");
			var itemsViewModel = new MenuSection(caption.ToString(Res.CurrentLanguage), "Similar Incidents", caption, SectionType.RecentItem);

			var options = similarIncidentsWrapper.Filter.PrepareSearchOptions();
			activeOptionTuple = similarIncidentsWrapper.Filter.SearchOptionsToTuple();

			LoadMoreMenuItemsAsync(itemsViewModel, options);

			incidentsListControl.DataContext = itemsViewModel;
		}

		void DisposeOfRecentItems()
		{
			if (incidentsListControl != null)
			{
				var viewModel = incidentsListControl.DataContext as MenuSection;
				viewModel?.Dispose();

				incidentsListControl.DataContext = null;
			}
		}

		bool isLoading;

		void LoadMoreMenuItemsAsync(MenuSection itemsViewModel, SimilarIncidentSearchOptions options)
		{
			if (similarIncidentsWrapper == null)
			{
				return;
			}

			if (isLoading)
			{
				return;
			}

			isLoading = true;
			buttonFind.Enabled = false;

			RemoveLoadMoreMenuItem(itemsViewModel);
			var currentItemCount = itemsViewModel.Items.Count;

			AddLoadingMenuItem(itemsViewModel);

			options.StartIndex = currentItemCount;
			var pk = similarIncidentsWrapper.ParentIncident.PK;

			Task
				.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var similarities = ObjectFactory.Get<ISimilarIncidentRepository>().SearchStoredIncidentSimilarities(options).ToList();
						similarities.FirstOrDefault()?.GetOtherIncident(pk)?.Factory?.RelinquishThreadOwnership(); // Unlock access to data factory from different threads
						return similarities;
					}
				})
				.ContinueWith(
					dataTask =>
					{
						if (!dataTask.IsCompleted || dataTask.IsFaulted || dataTask.IsCanceled)
						{
							// To Avoid:  A Task's excepteon(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.
							// https://stackoverflow.com/questions/7883052/a-tasks-exceptions-were-not-observed-either-by-waiting-on-the-task-or-accessi
							if (dataTask.IsFaulted)
							{
								_ = dataTask.Exception;
							}
							return;
						}

						var similarities = dataTask.Result;
						similarities.FirstOrDefault()?.GetOtherIncident(pk)?.Factory?.TakeThreadOwnership(); // Set current thread to access data factory

						RemoveLoadMoreMenuItem(itemsViewModel); // Remove "Loading..." stub menu item
						var hasNewItems = FillMenuItems(itemsViewModel, similarities);
						if (hasNewItems)
						{
							AddLoadMoreMenuItem(itemsViewModel, options);
						}
					},
					TaskScheduler.FromCurrentSynchronizationContext() // Run this task on UI thread
				)
				.ContinueWith(
					displayTask =>
					{
						isLoading = false;
						buttonFind.Enabled = true;
						if (displayTask.IsFaulted || displayTask.IsCanceled)
						{
							// To Avoid:  A Task's excepteon(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.
							// https://stackoverflow.com/questions/7883052/a-tasks-exceptions-were-not-observed-either-by-waiting-on-the-task-or-accessi
							if (displayTask.IsFaulted)
							{
								_ = displayTask.Exception;
							}

							RemoveLoadMoreMenuItem(itemsViewModel);

							// To Avoid:  A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.
							// https://stackoverflow.com/questions/7883052/a-tasks-exceptions-were-not-observed-either-by-waiting-on-the-task-or-accessi
							if (displayTask.IsFaulted)
							{
								_ = displayTask.Exception;
							}
						}
					},
					TaskScheduler.FromCurrentSynchronizationContext() // Run this task on UI thread
				)
				.ContinueWith(
					// To Avoid:  A Task's excepteon(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.
					// https://stackoverflow.com/questions/7883052/a-tasks-exceptions-were-not-observed-either-by-waiting-on-the-task-or-accessi
					cleanupTask => cleanupTask.IsFaulted ? cleanupTask.Exception : null,
					TaskScheduler.FromCurrentSynchronizationContext() // Run this task on UI thread
				);
		}

		bool FillMenuItems(MenuSection itemsViewModel, IEnumerable<IncidentSimilarityMatrix> similarities)
		{
			var parentForm = FindForm() as ZForm;
			if (parentForm == null)
			{
				return false;
			}

			var hasNewItems = false;

			foreach (var item in similarities.Select((similarityMatrix, index) => (SimilarityMatrix: similarityMatrix,  Index: index)))
			{
				var index = item.Index;
				var similarityMatrix = item.SimilarityMatrix;
				var relatedIncident = similarityMatrix.GetOtherIncident(similarIncidentsWrapper.ParentIncident.PK);
				var shortcut = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(parentForm, relatedIncident);

				if (shortcut != null)
				{
					var menuItem = new MenuItem(
						shortcut.UniqueKey,
						(NoResString)shortcut.RecordDescription,
						() => OpenRelatedIncidentForm(index, parentForm, shortcut),
						null,
						null
					);

					menuItem.SetFavoriteAction(() =>
					{
						var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
						if (favoriteProvider != null)
						{
							if (RecentItemManager.Instance.IsInFavoriteModules(shortcut))
							{
								favoriteProvider.DeleteFromFavorites(shortcut);
								menuItem.IsInFavorites = false;
							}
							else
							{
								if (favoriteProvider.AddToFavorites(shortcut))
								{
									menuItem.IsInFavorites = true;
								}
							}
						}
					});

					menuItem.IsInFavorites = RecentItemManager.Instance.IsInFavoriteModules(shortcut);

					itemsViewModel.Items.Add(menuItem);
					hasNewItems = true;
				}
			}

			return hasNewItems;
		}

		void OpenRelatedIncidentForm(int index, ZForm parentForm, LinkWrapper shortcut)
		{
			var dict = new Dictionary<string, object>
			{
				{ "Index", index },
				{ "DateRange", activeOptionTuple.DateRange },
				{ "AllCustomers", activeOptionTuple.AllCustomers },
				{ "IncidentStatus", activeOptionTuple.IncidentStatus },
			};

			using (PerformanceStatisticsCollector.StartMonitoring(
				string.Format(CultureInfo.InvariantCulture, "Similar Incident Clicked"),
				JsonConvert.SerializeObject(dict)))
			{
				var factory = similarIncidentsWrapper.ParentIncident.Factory
					.CreateNewFactory(); // Do not load bizos into this form's factory - create new one
				if (factory != null)
				{
					var bizO = factory.Load<SupportIncident>(shortcut.RecordKey);

					if (bizO != null)
					{
						var controller = ZControllerFactory.Create(parentForm.ControllerID);
						controller?.ShowEditForm(bizO);
					}
				}
			}
		}

		void AddLoadMoreMenuItem(MenuSection itemsViewModel, SimilarIncidentSearchOptions options)
		{
			RemoveLoadMoreMenuItem(itemsViewModel);

			var menuItem = new MenuItem(
				LoadMoreMenuItemKey,
				ResString.GetMultilingualString("f6eb1846-bdcd-48ba-8b5b-aaf49422f58e", "Load more..."),
				() =>
				{
					LoadMoreMenuItemsAsync(itemsViewModel, options);
				},
				null,
				null
			);

			itemsViewModel.Items.Add(menuItem);
		}

		void AddLoadingMenuItem(MenuSection itemsViewModel)
		{
			RemoveLoadMoreMenuItem(itemsViewModel);

			var menuItem = new	MenuItem(
				LoadMoreMenuItemKey,
				ResString.GetMultilingualString("a4d25d91-99db-4679-a607-729281da1672", "Loading..."),
				() =>
				{
					System.Media.SystemSounds.Beep.Play();
				},
				null,
				null
			);

			itemsViewModel.Items.Add(menuItem);
		}

		void RemoveLoadMoreMenuItem(MenuSection itemsViewModel)
		{
			var existingItem = itemsViewModel.Items.FirstOrDefault(item => item.Key.Equals(LoadMoreMenuItemKey, StringComparison.Ordinal));
			if (existingItem != null)
			{
				itemsViewModel.Items.Remove(existingItem);
			}
		}

		const string LoadMoreMenuItemKey = "LOAD_MORE_KEY";
	}
}
