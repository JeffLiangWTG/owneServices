using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ExportXmlMenuItemHelper
	{
		public static MultilingualString VerboseMenuItemText { get { return ResString.GetMultilingualString("7a2aa50a-e25a-40e5-917d-1184e62fe1c5", "Export to XML (Verbose)"); } }
		public static MultilingualString LightWeightMenuItemText { get { return ResString.GetMultilingualString("66ff8b3b-df3a-4b01-b867-ea1e1849dd11", "Export to XML (Light-Weight)"); } }
		public static MultilingualString NativeMenuItemText { get { return ResString.GetMultilingualString("ff3465bb-b3cc-4ad4-a6e3-835148a73708", "Export as Native XML"); } }

		public const string VerboseMenuItemName = "ExportToXmlVerbose";
		public const string LightWeightMenuItemName = "ExportToXmlLightWeight";
		public const string NativeExportMenuItemName = "ExportToXmlNative";
		public const string NativeImportMenuItemName = "ImportToXmlNative";
		public const string NativeSchemaMenuItemName = "NativeSchema";
		public const string DataTransferMenuItemName = "ActionDataTransferMenuItem";
	}

	class ZFormNativeXMLActionMenuStrategy
	{
		public IExportService Exporter { get; set; }
		public IExportValidator ExportValidator { get; set; }

		public void AddAdornments(Form form)
		{
			var zForm = form as ZForm;
			if (zForm == null)
			{
				return;
			}

			if (!zForm.AllowExportNativeXml)
			{
				return;
			}

			var menuItemsProvider = (IFileMenuItemsProvider)form;
			var menuItems = menuItemsProvider.ActionsMenuItem.MenuItems;

			if (menuItems[ExportXmlMenuItemHelper.NativeExportMenuItemName] != null)
			{
				return;
			}

			var bizObj = zForm.BusinessEntity;
			var controllerID = zForm.ControllerID;

			var exportMenuItem = CreateExportMenuItem(bizObj, controllerID);

			AddMenuItem(menuItems, exportMenuItem);
		}

		public MenuItem CreateExportMenuItem(IBusiness bizObj, ControllerID controllerID)
		{
			if (bizObj == null)
			{
				return null;
			}

			if (!ExportValidator.CanBeExported(bizObj.GetType()))
			{
				return null;
			}

			EventHandler handler = (o, e) => ExportCore(new[] { bizObj });

			if (controllerID != null)
			{
				var controller = ZControllerFactory.Create(controllerID);

				if (controller != null)
				{
					var currentModule = ZCurrentModules.Instance.GetCurrentModule(controller.ModuleID);
					var disposeModule = false;
					try
					{
						if (currentModule == null)
						{
							currentModule = ZModuleFactory.Instance.Create(controller.ModuleID);
							disposeModule = true;
						}
						if (currentModule != null)
						{
							var checkpoint = currentModule.SecurityCheckpoint;
							var security = EnvProxy.Instance.Security;
							if (checkpoint != security.None)
							{
								checkpoint = security.FindOrCreateExportNativeXmlCheckPoint(currentModule.SecurityCheckpoint) as SecurityCheckpoint;
							}
							handler = GetSecurityCheckerCore(new[] { bizObj }, checkpoint).OnClick;
						}
					}
					finally
					{
						if (disposeModule)
						{
							currentModule?.Dispose();
						}
					}
				}
			}

			var exportMenuItem = new ZMenuItem(ExportXmlMenuItemHelper.NativeMenuItemText, handler, Shortcut.None);
			exportMenuItem.Name = ExportXmlMenuItemHelper.NativeExportMenuItemName;

#if DEBUG
			NativeXmlMenuItems.Add(ExportXmlMenuItemHelper.NativeMenuItemText, handler);
#endif

			return exportMenuItem;
		}

		protected virtual void ExportCore(IEnumerable<IBusiness> bizos)
		{
			Exporter.Export(bizos);
		}

		protected virtual ImportSecurityChecker GetSecurityCheckerCore(IEnumerable<IBusiness> bizos, SecurityCheckpoint checkpoint)
		{
			return new ImportSecurityChecker((o, e) => ExportCore(bizos), checkpoint);
		}

#if DEBUG
		public FilterModuleMenuItemDescriptorCollection NativeXmlMenuItems
		{
			get
			{
				if (nativeXmlMenuItems == null)
				{
					nativeXmlMenuItems = new FilterModuleMenuItemDescriptorCollection();
				}
				return nativeXmlMenuItems;
			}
		}
		FilterModuleMenuItemDescriptorCollection nativeXmlMenuItems;
#endif

		public void AddMenuItem(Menu.MenuItemCollection menuItems, MenuItem exportMenuItem)
		{
			if (exportMenuItem == null)
			{
				return;
			}

			var prev = menuItems[ExportXmlMenuItemHelper.LightWeightMenuItemName];
			if (prev != null)
			{
				var prevIndex = menuItems.IndexOf(prev);

				menuItems.Add(prevIndex + 1, exportMenuItem);
			}
			else
			{
				menuItems.Add(exportMenuItem);
			}
		}
	}
}
