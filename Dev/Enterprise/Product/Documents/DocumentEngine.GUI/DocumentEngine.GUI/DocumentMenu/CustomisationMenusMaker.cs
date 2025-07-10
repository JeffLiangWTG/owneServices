using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.BuildTools;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#if DEBUG
using Enterprise.Builder.DataUpgradeSetup;
#endif

namespace Enterprise.DocumentEngine.GUI
{
	/// <summary>
	/// Creates Check In, Check Out, Undo Check Out and Customise menus for a given parent menu item.
	/// </summary>
	public abstract class CustomisationMenusMaker<T> : IDocumentCustomisationMenusMaker where T : class
	{
		protected CustomisationMenusMaker(Form parentForm, ISecurityCheckpoint securityCheckpoint, ZDocumentsMenuItemHelper<T> helper)
		{
			ParentForm = parentForm;
			customisationSecurityCheckpoint = securityCheckpoint;
			MenuItemHelper = helper;
		}
		readonly ISecurityCheckpoint customisationSecurityCheckpoint;

		protected virtual MultilingualString CustomizeMenuText => ResString.GetMultilingualString("169FCA9E-C41F-4d8e-870B-5239A52D2051", "Customize");

		#region Helper

		protected ZDocumentsMenuItemHelper<T> MenuItemHelper { get; }

		#endregion

		#region ParentForm

		protected Form ParentForm { get; }

		#endregion

		protected abstract IMenuCustomisationForm GetCustomisationForm();

		protected virtual void AddSpecificMenus(List<T> menuItems)
		{ }

		public event EventHandler MenuItemsChanged;

		void DoMenuItemsChanged() => MenuItemsChanged?.Invoke(this, EventArgs.Empty);

		#region RaiseMenuItemsChangedForTesting
#if DEBUG
		public void RaiseMenuItemsChangedForTesting() => DoMenuItemsChanged();
#endif
		#endregion

		public void Make(IList menuItemsCollection)
		{
			MenuItemHelper.Make(menuItemsCollection, Make());
#if DEBUG && !WINZOR
			UpdateCheckedOut();
#endif
		}

		public T[] Make()
		{
			var menuItems = new List<T>();
			AddCustomiseMenuItem(menuItems);
			AddSpecificMenus(menuItems);
#if DEBUG
			if (DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.DocumentsSourceControlIsEnabled.Value)
			{
				AddDebugOnlyMenuItems(menuItems);
			}
#endif
			return menuItems.ToArray();
		}

		protected virtual void AddCustomiseMenuItem(List<T> menuItems)
		{
			menuItems.Add(MenuItemHelper.GetSeparator());
			menuItems.Add(MenuItemHelper.GetNewMenuItem((NoResString)"Customize", CustomizeMenuText, new EventHandler(OnCustomise))); // Menu item name, not menu item text, should not be localized
		}

		void OnCustomise(object sender, EventArgs e)
		{
			ShowCustomisationForm(customisationSecurityCheckpoint, GetCustomisationForm);
		}

		protected void SetCustomisationFormEditingMode(IMenuCustomisationForm form)
		{
			SetMenuEditableEditingMode(form.EditableBusinessEntity);
		}

		protected void SetMenuEditableEditingMode(IMenuEditable menuEditable)
		{
#if DEBUG && !WINZOR
			if (GetIsCheckedOutByMe())
			{
				menuEditable.EditingMode = string.IsNullOrEmpty(CurrentCheckedOutClientName)
					? MenuEditingMode.AllowEditingOfSystemDefinedOnly
					: MenuEditingMode.AllowEditingOfClientSpecificOnly;
			}
			else
#endif
			{
				menuEditable.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			}
		}

		protected void ShowCustomisationForm(ISecurityCheckpoint securityCheckpoint, Func<IMenuCustomisationForm> getMenuCustomisationForm)
		{
			if (securityCheckpoint.IsAllowed)
			{
				var customisationForm = getMenuCustomisationForm();
				ShowCustomisationForm(customisationForm);
			}
			else
			{
				securityCheckpoint.ShowError();
			}
		}

		void ShowCustomisationForm(IMenuCustomisationForm customisationForm)
		{
			SetCustomisationFormEditingMode(customisationForm);
			ShowCustomisationForm((ZForm)customisationForm);
		}

		protected void ShowCustomisationForm(ZForm form)
		{
			if (ParentForm == null)
			{
				_ = ZFormModaliser.ShowDialogAndDispose(form);
				DoMenuItemsChanged();
			}
			else
			{
				form.Disposed += delegate
				{ DoMenuItemsChanged(); };
				ZFormModaliser.Show(form, ParentForm);
			}
		}

#if DEBUG
		#region Check In/Out

		protected virtual string CustomisationMenusDescription { get; }

		internal T MarkAllEditableMenu { get; private set; }
		string MarkAllEditableMenuCaption => $"{DocumentMenuCaptions.MarkAll} {CustomisationMenusDescription} {DocumentMenuCaptions.Editable}";

		internal T MarkEditableForClientMenu { get; private set; }
		string MarkEditableForClientMenuCaption => $"{DocumentMenuCaptions.Mark} {CustomisationMenusDescription} {DocumentMenuCaptions.EditableForClient}";

		internal T SaveConfigChangesMenu { get; private set; }
		string SaveConfigChangesMenuCaption => $"{DocumentMenuCaptions.Save} {CustomisationMenusDescription} {DocumentMenuCaptions.ConfigChanges}";

		internal T UndoAllChangesMenu { get; private set; }

		readonly string UndoAllChangesMenuCaption = DocumentMenuCaptions.UndoAllChanges;

		internal T RegenerateAllClientXMLMenu { get; private set; }
		readonly string RegenerateAllClientDocumentsXMLMenuCaption = DocumentMenuCaptions.RegenerateAllClientsDocumentsXML;

		[ThreadStatic]
		static bool shouldAddDebugOnlyMenuItemsForTesting;

		bool GetIsCheckedOutByMe() => DSC.IsCheckedOutByMe;

		DocumentsSetupController dsc;
		DocumentsSetupController DSC => dsc = dsc ?? GetNewDocumentsSetupController();

		string CurrentCheckedOutClientName
		{
			get => Env.Registry.CurrentCheckedOutClientName;
			set => Env.Registry.CurrentCheckedOutClientName = value;
		}

		public static bool ShouldAddDebugOnlyMenuItemsForTesting
		{
			get => shouldAddDebugOnlyMenuItemsForTesting;
			set => shouldAddDebugOnlyMenuItemsForTesting = value;
		}

		protected virtual void AddAdditionalDebugOnlyMenuItems(List<T> menuItems)
		{ }

		protected virtual bool IsSupportingClientDocuments => true;

		protected virtual void AddDebugOnlyMenuItems(List<T> menuItems)
		{
			if ((!Globals.IsTest || ShouldAddDebugOnlyMenuItemsForTesting) && BuildConstants.LocalSourcePathAvailable)
			{
				menuItems.Add(MenuItemHelper.GetSeparator());

				MarkAllEditableMenu = MenuItemHelper.GetNewMenuItem("MarkAllEditable", (NoResString)MarkAllEditableMenuCaption, new EventHandler(OnMarkAllDocumentsEditable));
				MarkEditableForClientMenu = MenuItemHelper.GetNewMenuItem("MarkEditableForClient", (NoResString)MarkEditableForClientMenuCaption, null);
				SaveConfigChangesMenu = MenuItemHelper.GetNewMenuItem("SaveConfigChanges", (NoResString)SaveConfigChangesMenuCaption, new EventHandler(OnSaveDocumentsConfigChanges));
				UndoAllChangesMenu = MenuItemHelper.GetNewMenuItem("UndoAllChanges", (NoResString)UndoAllChangesMenuCaption, new EventHandler(OnUndoAllChanges));
				RegenerateAllClientXMLMenu = MenuItemHelper.GetNewMenuItem("RegenerateAllClientDocumentsXML", (NoResString)RegenerateAllClientDocumentsXMLMenuCaption, new EventHandler(OnRegenerateAllClientDocumentsXML));

				menuItems.Add(MarkAllEditableMenu);
				if (IsSupportingClientDocuments)
				{
					menuItems.Add(MarkEditableForClientMenu);
				}

				menuItems.Add(SaveConfigChangesMenu);
				menuItems.Add(UndoAllChangesMenu);
				if (IsSupportingClientDocuments)
				{
					menuItems.Add(RegenerateAllClientXMLMenu);
				}

				AddAdditionalDebugOnlyMenuItems(menuItems);

				if (IsSupportingClientDocuments)
				{
					ReloadClientMenus();
				}

				UpdateCheckedOut();
			}
		}

		protected void UpdateCheckedOut()
		{
			var editable = GetIsCheckedOutByMe();
			SetMenuItemEnabledIfStillExists(MarkAllEditableMenu, !editable);
			SetMenuItemEnabledIfStillExists(MarkEditableForClientMenu, !editable);
			SetMenuItemEnabledIfStillExists(SaveConfigChangesMenu, editable);
			SetMenuItemEnabledIfStillExists(UndoAllChangesMenu, editable);
		}

		void SetMenuItemEnabledIfStillExists(T menuItem, bool enabled)
		{
			if (menuItem != null)
			{
				MenuItemHelper.SetEnabled(menuItem, enabled);
			}
		}

		DocumentsSetupController GetApplicableDocumentsSetupController()
		{
			if (string.IsNullOrEmpty(CurrentCheckedOutClientName))
			{
				return GetNewDocumentsSetupController();
			}
			else
			{
				var result = GetNewClientSpecificDocumentsSetupControllerWithDocCompleteTask(CurrentCheckedOutClientName);
				result.Initialise();
				return result;
			}
		}

		protected virtual ClientSpecificDocumentsSetupController GetNewClientSpecificDocumentsSetupControllerWithDocCompleteTask(string clientName) => new ClientSpecificDocumentsSetupController(clientName);

		protected ClientSpecificDocumentsSetupController GetNewClientSpecificDocumentsSetupControllerWithoutDocCompleteTask(string clientName) => new ClientSpecificDocumentsSetupController(clientName, false);

		protected virtual DocumentsSetupController GetNewDocumentsSetupController() => new DocumentsSetupController();

		void InvokeWithWaitCursor(string actionDescription, WaitableActionInvoker.WaitableAction action) => WaitableActionInvoker.Invoke<SourceControlException>(actionDescription + " Error", ParentForm, action);

		void PerformCheckout(ISetupController controller)
		{
			controller.FullCheckOut();
			MenuItemHelper.SetEnabled(SaveConfigChangesMenu, true);
			MenuItemHelper.SetEnabled(UndoAllChangesMenu, true);
			MenuItemHelper.SetEnabled(MarkAllEditableMenu, false);
			MenuItemHelper.SetEnabled(MarkEditableForClientMenu, false);
		}

		protected virtual Action GetCustomisationDebugOnlyAction(Action action) => action;

		void OnSaveDocumentsConfigChanges(object sender, EventArgs e) =>
			InvokeWithWaitCursor(
				SaveConfigChangesMenuCaption,
				new WaitableActionInvoker.WaitableAction(GetCustomisationDebugOnlyAction(() => GetApplicableDocumentsSetupController().FullSave())));

		void OnMarkAllDocumentsEditable(object sender, EventArgs e) =>
			InvokeWithWaitCursor(
				MarkAllEditableMenuCaption,
				new WaitableActionInvoker.WaitableAction(GetCustomisationDebugOnlyAction(() => PerformCheckout(GetNewDocumentsSetupController()))));

		void OnUndoAllChanges(object sender, EventArgs e) =>
			InvokeWithWaitCursor(
				UndoAllChangesMenuCaption,
				new WaitableActionInvoker.WaitableAction(GetCustomisationDebugOnlyAction(() =>
				{
					GetApplicableDocumentsSetupController().FullUndoCheckOut();
					CurrentCheckedOutClientName = string.Empty;
					MenuItemHelper.SetEnabled(UndoAllChangesMenu, false);
					MenuItemHelper.SetEnabled(MarkAllEditableMenu, true);
					MenuItemHelper.SetEnabled(MarkEditableForClientMenu, true);
					MenuItemHelper.SetEnabled(SaveConfigChangesMenu, false);
				})));

		void OnCheckOutClient(object sender, EventArgs e)
		{
			var clientMenu = (T)sender;
			CurrentCheckedOutClientName = MenuItemHelper.GetName(clientMenu);

			var shouldContinue = true;
			var changeController = GetNewClientSpecificDocumentsSetupControllerWithDocCompleteTask(CurrentCheckedOutClientName);
			if (!changeController.ClientDocDirExists)
			{
				if (Globals.Message.Show("This client has no customized documents yet. Do you want to create them?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					shouldContinue = false;
				}
			}

			if (shouldContinue)
			{
				changeController.Initialise();
				InvokeWithWaitCursor(MarkEditableForClientMenuCaption, new WaitableActionInvoker.WaitableAction(GetCustomisationDebugOnlyAction(() => PerformCheckout(changeController))));
			}
		}

		void OnRegenerateAllClientDocumentsXML(object sender, EventArgs e)
		{
			if (Globals.Message.Show("This will re-generate ALL clients' specific Documents.xml files. Use with CAUTION." + System.Environment.NewLine + "This process can be quite long. Do you really want to re-generate them?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				InvokeWithWaitCursor(RegenerateAllClientDocumentsXMLMenuCaption, new WaitableActionInvoker.WaitableAction(GetCustomisationDebugOnlyAction(() =>
				{
					var clientNames = ClientSpecificDocumentsSetupController.GetClientList(BuildConstants.LocalEnterprisePath);
					ClientSpecificDocumentsSetupController changeController;

					var createDocCompleteTask = true;

					foreach (var clientName in clientNames)
					{
						changeController = createDocCompleteTask
							? GetNewClientSpecificDocumentsSetupControllerWithDocCompleteTask(clientName)
							: GetNewClientSpecificDocumentsSetupControllerWithoutDocCompleteTask(clientName);

						if (changeController.ClientDocDirExists)
						{
							changeController.Initialise();
							changeController.FullCheckOut();
							changeController.FullSave();
							createDocCompleteTask = false;
						}
					}
				})));
			}
		}

		void ReloadClientMenus()
		{
			MenuItemHelper.GetItems(MarkEditableForClientMenu).Clear();
			try
			{
				var clientNames = ClientSpecificDocumentsSetupController.GetClientList(BuildConstants.LocalEnterprisePath);
				var lookup = new SortedDictionary<char, List<string>>();

				foreach (var clientName in clientNames)
				{
					if (string.IsNullOrEmpty(clientName))
					{
						continue;
					}

					var c = clientName[0];

					if (!lookup.TryGetValue(c, out var list))
					{
						list = new List<string>();
						lookup.Add(c, list);
					}

					list.Add(clientName);
				}

				foreach (var pair in lookup)
				{
					T parent;

					if (pair.Value.Count > 1)
					{
						MultilingualString text = (NoResString)char.ToUpper(pair.Key).ToString();
						parent = MenuItemHelper.GetNewMenuItem(text, text, null);
						_ = MenuItemHelper.GetItems(MarkEditableForClientMenu).Add(parent);
					}
					else
					{
						parent = MarkEditableForClientMenu;
					}

					foreach (NoResString clientName in pair.Value)
					{
						var item = MenuItemHelper.GetNewMenuItem(clientName, clientName, new EventHandler(OnCheckOutClient));
						_ = MenuItemHelper.GetItems(parent).Add(item);
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				_ = MenuItemHelper
					.GetItems(MarkEditableForClientMenu)
					.Add(MenuItemHelper.GetNewMenuItem(ex.Message, (NoResString)ex.Message, null));
			}
		}
		#endregion
#endif
	}
}
