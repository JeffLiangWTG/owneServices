using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.UniversalCopy;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class ModuleUniversalCopyManager : UniversalCopyManager, IModuleUniversalCopyManager
	{
		public ModuleUniversalCopyManager(ZFilterGridModule filterGridModule)
			: base(GetElementType(filterGridModule), filterGridModule)
		{
			FilterGridModule = filterGridModule;
		}

		static Type GetElementType(ZFilterGridModule filterGridModule)
		{
			return filterGridModule.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>()?.InstanceType ??
						filterGridModule.GetElementType();
		}

		internal ZFilterGridModule FilterGridModule { get; }

		#region Stored configurations

		UniversalCopyFactory CopyFactory
		{
			get => factory ??= new UniversalCopyFactory(ElementType, ModuleId);
		}

		UniversalCopyFactory factory;

		#endregion

		public void AddMenuItems(ZMenuItem newMenuItem, EventHandler onNewClick)
		{
			if (AllowsUniversalCopy)
			{
				const string universalCopyMenuItemName = "UniversalCopyMenuItem";
				var universalCopyMenuItemExists = false;
				var universalCopyItem = newMenuItem.MenuItems.Find(universalCopyMenuItemName, false).FirstOrDefault();
				if (universalCopyItem == null)
				{
					universalCopyItem = new ZMenuItem(ResString.GetMultilingualString("2FE24FBF-D3A7-48BE-A993-B3920862355C", "Universal Copy"));
					universalCopyItem.Name = universalCopyMenuItemName;
				}
				else
				{
					universalCopyMenuItemExists = true;
					universalCopyItem.MenuItems.Clear();
				}

				foreach (var copyTemplate in GetStoredCopyConfiguration())
				{
					copyTemplate.CopyTemplateTree = null; // Reset old cached template

					if (copyTemplate.CopyTemplateTree != null && copyTemplate.IsActive &&
						(
							(copyTemplate.CopyTemplateTree.ConfigurationSource == ConfigurationSourceCodes.FilteredRecord && copyTemplate.CopyTemplateTree.EntityFilter != null) ||
							copyTemplate.CopyTemplateTree.ConfigurationSource == ConfigurationSourceCodes.NominatedRecord
						))
					{
						AddCopyConfigurationMenu(copyTemplate, universalCopyItem, copyTemplate.CopyTemplateTree.ConfigurationSource);
					}
				}

				if (universalCopyItem.MenuItems.Count > 0)
				{
					if (newMenuItem.MenuItems.Count == 0)
					{
						var newItem = new ZMenuItem(newMenuItem.CaptionResourceString, onNewClick, newMenuItem.ActiveIcon, newMenuItem.RestIcon);
						newMenuItem.MenuItems.Add(newItem);
					}

					if (!universalCopyMenuItemExists)
					{
						newMenuItem.MenuItems.Add(universalCopyItem);
					}
				}
			}
		}

		void AddCopyConfigurationMenu(UniversalCopyTemplate copyTemplate, MenuItem parentMenuItemCollection, string configurationSource)
		{
			EventHandler copyMenuHandler;
			var menuPath = copyTemplate.CopyTemplateTree.ConfigurationName.ToString().Split('\\');
			var menuText = menuPath[menuPath.Length - 1];

			if (configurationSource == ConfigurationSourceCodes.FilteredRecord)
			{
				copyMenuHandler = Security.CanRun ? ExecuteOnCorrectThread(CopyFilteredMenuClicked) : ExecuteOnCorrectThread(ShowDisabledRunSecurityMessageOnMenuHandler);
				menuText += " " + Res.GetString("866eb46e-697c-486d-9e8f-5606c531f7e2", "(filter)");
			}
			else
			{
				copyMenuHandler = Security.CanRun ? ExecuteOnCorrectThread(CopyNominatedMenuClicked) : ExecuteOnCorrectThread(ShowDisabledRunSecurityMessageOnMenuHandler);
				menuText += " " + Res.GetString("E51A2BAB-C059-44FB-A737-B214AEC919CF", "(nominated)");
			}

			var newMenuItem = new ZMenuItem(menuText, copyMenuHandler) { Tag = copyTemplate };
			parentMenuItemCollection.MenuItems.Add(newMenuItem);
		}

		void CopyFilteredMenuClicked(object sender, EventArgs e)
		{
			RunCopyTemplate(sender, (copyTemplate) =>
			{
				var filter = GetFilter(copyTemplate.CopyTemplateTree.CopyTemplateNode, Enumerable.Empty<string>());
				var query = new ZQuery(filter != null ? filter.Filter : ZQuery.NoResultQuery) { OrderBy = copyTemplate.CopyTemplateTree.OrderBy };
				return CopyFactory.Factory.LoadTop1(FilterGridModule.GetElementType(), query);
			});
		}

		void CopyNominatedMenuClicked(object sender, EventArgs e)
		{
			RunCopyTemplate(sender, LoadNominatedRecord);
		}

		internal BusinessObject LoadNominatedRecord(UniversalCopyTemplate copyTemplate)
		{
			var source = CopyFactory.Factory.Load(FilterGridModule.GetElementType(), copyTemplate.CopyTemplateTree.NominatedRecordPk);
			if (source == null && ((IZFilterGridModule)FilterGridModule).AllowTemplateRecords)
			{
				source = FilterGridModule.LoadFromTemplateRecordPk(CopyFactory.Factory, copyTemplate.CopyTemplateTree.NominatedRecordPk);
			}
			return source;
		}

		public bool IsTryingToCopyFromCancelledTemplateRecord(UniversalCopyTemplate copyTemplate)
		{
			var nominatedRecord = LoadNominatedRecord(copyTemplate);

			return nominatedRecord is ITemplateRecordProvider templateRecordProvider
				&& templateRecordProvider.IsTemplateRecord
				&& templateRecordProvider.TemplateRecord is ICancellable templateRecord
				&& templateRecord.IsCancelled;
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		void RunCopyTemplate(object sender, Func<UniversalCopyTemplate, BusinessObject> selectedBizo)
		{
			var menuItem = (sender is IConvertedFromMenuItem) ? ((IConvertedFromMenuItem)sender).SourceMenuItem : sender as MenuItem;
			var copyTemplate = menuItem != null ? menuItem.Tag as UniversalCopyTemplate : null;
			if (copyTemplate != null && ElementType != null)
			{
				if (IsTryingToCopyFromCancelledTemplateRecord(copyTemplate))
				{
					Globals.Message.ShowError(
						Res.GetString(
							"9954efa0-4709-4afb-bcb4-cf3177dae3bd",
							"This template is inactive. Please re-activate the nominated record to use this functionality."
						)
					);
				}
				else
				{
					copyTemplate.CopyTemplateTree = null;
					copyTemplate.ReloadSafe();

					// Extend is needed for correct filtering, otherwise filter layout may not load parts for columns not added to ColumnNamesToInclude list, and to restore non-serialized reflection data.
					CopyFactory.ExtendTemplate(copyTemplate, InterfaceType);

					var bizo = selectedBizo(copyTemplate);
					if (bizo != null)
					{
						CopyMenuClicked(copyTemplate.CopyTemplateTree.CopyTemplateNode, new[] { bizo });
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("fb93019e-2084-4596-9c8c-3116b4e157ad", "Nothing has been found to copy with specified filter in template '{0}'.", menuItem.Text));
					}
				}
			}
			else if (copyTemplate == null)
			{
				ErrorReporter.ReportOnce("UniversalCopyManager_CopyMenuClicked_NullConfiguration", "CopyTemplate [stored in MenuItem.Tag] is null, menu item: " + (menuItem != null ? menuItem.Text : "<NULL>"));
			}
			else if (ElementType == null)
			{
				ErrorReporter.ReportOnce("UniversalCopyManager_CopyMenuClicked_NullElementType", "ElementType is null, module ID: " + (ModuleId != null ? ModuleId.Name : "<NULL>"));
			}
		}

		#region Implementation

		protected override BusinessObject CopyMenuClicked_GetSourceElement(BusinessObject selectedElement)
		{
			var controller = FilterGridModule.GetNewController(selectedElement);
			var elementType =
				selectedElement.GetType().GetCustomAttribute<UniversalCopyInstanceTypeAttribute>()?.InstanceType ??
				(controller != null ? controller.TypeOfTopLevelBusinessObject : selectedElement.GetType());
			var newFactory = controller != null ? controller.Factory : selectedElement.Factory.CreateNewFactory();
			return ImportIntoAnotherFactory(selectedElement, elementType, newFactory);
		}

		protected override bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets)
		{
			throw new NotImplementedException();
		}

		protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
		{
			if (newElement != null)
			{
				var formShowingArgs = new FormShowingForElementArgs(newElement);
				OnFormShowingForNewElement(formShowingArgs);
				if (!formShowingArgs.Cancelled)
				{
					var controller = FilterGridModule.GetNewController(newElement);
					controller.ShowFormForNewEntity(newElement);
				}
			}
			else
			{
				ShowElementWasNotCopiedInformation();
			}
		}

		protected override bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			throw new NotImplementedException();
		}

		protected override bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
