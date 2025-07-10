using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public static class BusinessObjectModulePicker
	{
		/// <summary>
		/// Show module popup screen for user to select rows.
		/// </summary>
		/// <param name="collection">collection to select from</param>
		/// <param name="moduleID">module</param>
		/// <param name="filterLayoutToSelect"></param>
		/// <param name="shouldLoadLayoutEvenWhenUnsaved"></param>
		/// <param name="allowMultiSelect"></param>
		/// <param name="okButtonCaption"></param>
		/// <returns>array of selected rows, length zero if nothing selected</returns>
		public static BusinessObject[] PickFromModuleScreen(
			IBusinessObjectCollection collection,
			ModuleIdentifier moduleID,
			StmModuleFilter filterLayoutToSelect = null,
			bool shouldLoadLayoutEvenWhenUnsaved = false,
			bool allowMultiSelect = true,
			string okButtonCaption = null)
		{
			var findboxListProvider = collection as IFindBoxListProvider ?? throw new InvalidOperationException("Collection does not implement required interface IFindBoxListProvider.");

			var findBox = new ZSimpleFindBox(findboxListProvider);
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleID))
			{
				var provider = allowMultiSelect
					? new PopupModuleDecisionProviderWithMultipleSelect(findBox)
					: new PopupModuleDecisionProvider(findBox);

				module.OverrideModuleDecisionProvider(provider);
				BusinessObject[] selected = null;

				using (var modulePopup = new EmbeddedModulePopup(module, filterLayoutToSelect, shouldLoadLayoutEvenWhenUnsaved, okButtonCaption))
				{
					findBox.PopupForm = modulePopup;
					modulePopup.Selected += new EmbeddedModulePopup.SelectedEventHandler((s, e) => selected = e.SelectedBusinessObjects);
					ZFormModaliser.ShowDialogAndDispose(modulePopup);
				}

				return selected ?? Array.Empty<BusinessObject>();
			}
		}

		public static T[] PickFromModuleScreen<T>(
			IBusinessObjectCollection collection,
			ModuleIdentifier moduleID,
			StmModuleFilter filterLayoutToSelect = null,
			bool shouldLoadLayoutEvenWhenUnsaved = false,
			bool allowMultiSelect = true,
			string okButtonCaption = null)
			where T : BusinessObject
		{
			var selected = PickFromModuleScreen(collection, moduleID, filterLayoutToSelect, shouldLoadLayoutEvenWhenUnsaved, allowMultiSelect, okButtonCaption);
			return selected.Cast<T>().ToArray();
		}

		public static T PickOneRecordFromModuleScreen<T>(IBusinessObjectCollection collection, ModuleIdentifier moduleID, StmModuleFilter filterLayoutToSelect = null, bool shouldLoadLayoutEvenWhenUnsaved = false, string okButtonCaption = null)
			where T : BusinessObject
		{
			var selected = PickFromModuleScreen<T>(collection, moduleID, filterLayoutToSelect, shouldLoadLayoutEvenWhenUnsaved, false, okButtonCaption);
			return selected.Length > 0 ? selected[0] : null;
		}

		public static void ShowModuleScreen(ModuleIdentifier moduleID, Action<ZFilterModule> moduleInitialiser, FilterLayoutStrategy filterLayoutStrategy, bool isReadOnly = true, bool isSelectionMandatory = true)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleID))
			{
				moduleInitialiser.Invoke(module);
				((IZModuleInternals)module).SetReadOnly(isReadOnly);

				var layout = filterLayoutStrategy.FilterGetter?.Invoke();

				if (filterLayoutStrategy.DeleteFilterAfterUse)
				{
					layout.Factory.Save();
					module.LoadActiveCollection = filterLayoutStrategy.LoadActiveCollection;
				}

				try
				{
					var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;

					filterControl.IsFilterReadonly = isReadOnly;
					filterControl.RunSearchOnEnteringAModuleOverride = true;
					filterControl.CaptionRenderingEnabled = false;

					ZFormModaliser.ShowDialogAndDispose(new EmbeddedModulePopup(module, layout)
					{
						IsSelectionMandatory = isSelectionMandatory,
						RequireAtLeastOneItemToBeSelected = false,
					});
				}
				finally
				{
					if (filterLayoutStrategy.DeleteFilterAfterUse)
					{
						layout.Delete();
						layout.Factory.Save();
					}
				}
			}
		}

		public class FilterLayoutStrategy
		{
			public static FilterLayoutStrategy TransientLayoutStrategy(Func<StmModuleFilter> filterGetter, bool loadActiveCollection = false)
			{
				return new FilterLayoutStrategy(filterGetter, deleteFilterAfterUse: true, loadActiveCollection: loadActiveCollection);
			}

			public static FilterLayoutStrategy EmptyLayoutStrategy
			{
				get { return new FilterLayoutStrategy(() => new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(FilterLayoutStrategy) }.New<StmModuleFilter>(), deleteFilterAfterUse: false); }
			}

			public static FilterLayoutStrategy Empty => new FilterLayoutStrategy(null, false);

			FilterLayoutStrategy(Func<StmModuleFilter> filterGetter, bool deleteFilterAfterUse, bool loadActiveCollection = false)
			{
				FilterGetter = filterGetter;
				DeleteFilterAfterUse = deleteFilterAfterUse;
				LoadActiveCollection = loadActiveCollection;
			}

			internal Func<StmModuleFilter> FilterGetter { get; }
			internal bool DeleteFilterAfterUse { get; }
			internal bool LoadActiveCollection { get; }
		}
	}
}
