using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	sealed class MainFormHotKeyManager : Disposable
	{
		readonly MainForm mainForm;

		public MainFormHotKeyManager(MainForm mainForm)
		{
			this.mainForm = Argument.NotNull(mainForm, nameof(mainForm));
		}

		#region Globals

		public void RegisterGlobalHotKeys(HotkeyRegister globalHotkeys)
		{
			globalHotkeys.RegisterHotKey(Keys.Alt | Keys.F12, ShowHotkeysForm, Res.GetString("D6D4427F-5FFE-43CB-802C-788C855D8A78", "Show this form"));

			globalHotkeys.RegisterHotKey(Keys.F1, ProcessNewServiceRequestHotKey, ServiceRequestMenuItem.ServiceRequestMenuName);
			globalHotkeys.RegisterHotKey(Keys.F6, ProcessNewCommunicationFormHotkey, Res.GetString("9CBB777A-2A9C-4084-A9DB-81918FF96ADE", "New Communication Form"));
			globalHotkeys.RegisterHotKey(Keys.F7, (sender, key) => ProcessOpenOrShowModuleFormHotkey(ModuleIDs.Organisation), Res.GetString("3F8266E5-2956-4B4A-91F6-60ED7E6C697D", "Open Organization Module"));
			globalHotkeys.RegisterHotKey(Keys.F8, (sender, key) => ProcessOpenOrShowModuleFormHotkey(ModuleIDs.SalesEnquiry), Res.GetString("9CB94A1F-1364-4169-9E17-BA8D54AA61EE", "Open Sales Enquiry Module"));

			globalHotkeys.RegisterHotKeyRange(Keys.Control | Keys.F1, Keys.Control | Keys.F12, ProcessFavoriteShortcut, Res.GetString("14F0F06D-799B-4AB6-A771-3AB17F8F62DF", "Open a favorite"));
			globalHotkeys.RegisterHotKeyRange(Keys.Control | Keys.Shift | Keys.F1, Keys.Control | Keys.Shift | Keys.F12, ProcessFavoriteShortcut, Res.GetString("78C0F23D-E101-4811-8873-877818BAFDFA", "Open a favorite in a new window"));
		}

		bool ShowHotkeysForm(object sender, Keys args)
		{
			var control = ((Control)sender)?.GetFrontMostActiveControl();
			if (control != null)
			{
				control.BeginInvokeSafe(() =>
				{
					var hotkeysForm = new AvailableHotkeysForm(control);
					hotkeysForm.Show();
				});

				return true;
			}

			return false;
		}

		bool ProcessNewServiceRequestHotKey(object sender, Keys keyPressed)
		{
			if (GlbCompany.CurrentCompany != null)
			{
				if (sender is Form form)
				{
					ServiceRequestHandler.Launch(form);
					return true;
				}
			}

			return false;
		}

		bool ProcessNewCommunicationFormHotkey(object sender, Keys keyPressed)
		{
			if (GlbCompany.CurrentCompany != null)
			{
				mainForm.BeginInvokeSafe(ShowNewCommunicationForm);
				return true;
			}

			return false;
		}

		void ShowNewCommunicationForm()
		{
			if (Env.Security.CommunicationManager.IsAllowed && Env.Security.CommunicationManagerNew.IsAllowed)
			{
				if (!mainForm.HasUnReadItemsMandatoryToRead())
				{
					var controller = ZControllerFactory.Create(ControllerIDs.Communication);
					((ICommunicationController)controller).CreateNewWithParentFormBizObjDefaults = true;
					controller.ShowNewForm();
				}
			}
			else
			{
				Env.Security.CommunicationManagerNew.ShowError();
			}
		}

		bool ProcessOpenOrShowModuleFormHotkey(ModuleIdentifier moduleId)
		{
			mainForm.BeginInvokeSafe(() =>
			{
				Form openedForm;
				if (!OpenedModuleForms.TryGetValue(moduleId, out openedForm) || openedForm.IsDisposed)
				{
					openedForm = OpenModuleInNewWindow(moduleId);
					if (openedForm != null)
					{
						OpenedModuleForms[moduleId] = openedForm;
						openedForm.Disposed += delegate
						{
							OpenedModuleForms.Remove(moduleId);
						};
					}
					else
					{
						OpenedModuleForms.Remove(moduleId);
					}
				}
				else
				{
					if (openedForm.WindowState == FormWindowState.Minimized)
					{
						openedForm.WindowState = FormWindowState.Normal;
					}
					openedForm.Show();
					openedForm.Focus();
				}
			});

			return true;
		}

		bool ProcessFavoriteShortcut(object sender, Keys keysPressed)
		{
			if (Env.CurrentUser != null)
			{
				var openInNewWindow = keysPressed.HasFlag(Keys.Shift);

				var controlKeysToStrip = Keys.Control | Keys.Shift;
				var index = (int)(keysPressed & ~controlKeysToStrip) - (int)Keys.F1;
				if (index >= 0 && index < 12)
				{
					return mainForm.OpenFavorite(index, openInNewWindow);
				}
			}

			return false;
		}

		#endregion

		#region MainForm

		public void RegisterMainFormHotkeys(HotkeyRegister formHotkeys)
		{
			formHotkeys.RegisterHotKey(Keys.Control | Keys.F, ProcessSearchShortcut, Res.GetString("F0109075-073B-4729-B95B-322B8F0A48D6", "Search for a module"));
			formHotkeys.RegisterHotKey(Keys.Control | Keys.D1, ProcessCategoryShortcut(Category.Jump), Res.GetString("82CD7E7A-1673-4A81-BA2F-47EAEE667E99", "Open the Jump category"));
			formHotkeys.RegisterHotKey(Keys.Control | Keys.D2, ProcessCategoryShortcut(Category.Operations), Res.GetString("756483FA-AF30-43BA-AA1C-884759584569", "Open the Operations category"));
			formHotkeys.RegisterHotKey(Keys.Control | Keys.D3, ProcessCategoryShortcut(Category.Manage), Res.GetString("839ABD340-261E-4092-ACD1-732A8222AD2", "Open the Manage category"));
			formHotkeys.RegisterHotKey(Keys.Control | Keys.D4, ProcessCategoryShortcut(Category.Admin), Res.GetString("33FB4713-C86D-4752-AEC5-8EC9E03037A3", "Open the Admin category"));

			formHotkeys.RegisterHotKeyRange(Keys.A, Keys.Z, ProcessModuleOrQuickSearchShortcut, Res.GetString("8C6E37FF-2FE8-4F06-A40F-385316321748", "Show corresponding menu or, if no menus, begin a search"));
			formHotkeys.RegisterHotKeyRange(Keys.D0, Keys.D9, ProcessModuleOrQuickSearchShortcut, Res.GetString("8C6E37FF-2FE8-4F06-A40F-385316321748", "Show corresponding menu or, if no menus, begin a search"));
		}

		bool ProcessSearchShortcut(object sender, Keys keysPressed)
		{
			if (mainForm.NavigationBar != null && mainForm.NavigationBar.IsShown && mainForm.CurrentModule == null)
			{
				return mainForm.NavigationBar.SelectFindbox();
			}
			return false;
		}

		bool ProcessModuleOrQuickSearchShortcut(object sender, Keys key)
		{
			if (mainForm?.NavigationBar?.SelectedCategory == null
				|| !mainForm.NavigationBar.IsShown
				|| mainForm.CurrentModule != null
				|| !mainForm.ContainsFocus
				|| mainForm.SearchBoxContainsFocus)
			{
				return false;
			}

			//ensures that even if the alphabet and number ranges were shifted in the Keys enum, this would still work
			int letterRange = key - Keys.A;
			char letter = (letterRange >= 0 && letterRange <= 25) ? (char)('A' + (char)(letterRange)) : (char)('0' + (char)(key - Keys.D0));
			return (mainForm.NavigationBar.SelectedCategory.Name == Category.Jump.Name && !mainForm.NavigationBar.IsInSearchMode)
				? mainForm.NavigationBar.SelectFindbox(letter.ToString())
				: mainForm.NavigationBar.SelectModuleFromFirstLetter(letter);
		}

		HotKeyPressedProcessor ProcessCategoryShortcut(Entry category)
		{
			return (sender, key) =>
			{
				SwitchCategory(ModuleTree.Tree.Categories[category.Name]);
				return true;
			};
		}

		void SwitchCategory(ModuleCategory category)
		{
			mainForm.SwitchCategory(category);

			if (!mainForm.ContainsFocus)
			{
				mainForm.Focus();
			}
		}

		#endregion

		#region Form management

		readonly IDictionary<ModuleIdentifier, Form> OpenedModuleForms = new Dictionary<ModuleIdentifier, Form>();

		Form OpenModuleInNewWindow(ModuleIdentifier moduleId)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("OpenModuleInNewWindow", moduleId.Name))
			{
				Form result = null;
				MainFormModule module = (MainFormModule)ModuleTree.Tree.FindByID(moduleId.ToString());

				if (mainForm.CheckLicenceAndSecurityPermissions(module))
				{
					var originalCursor = Cursor.Current;
					Cursor.Current = Cursors.WaitCursor;

					try
					{
						result = mainForm.NavigationBar?.OpenModuleInNewWindowCore(module, null);
					}
					finally
					{
						Cursor.Current = originalCursor;
					}
				}
				return result;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			foreach (var openedForm in OpenedModuleForms.Values.ToArray())
			{
				openedForm.Dispose();
			}
		}

		#endregion
	}
}
