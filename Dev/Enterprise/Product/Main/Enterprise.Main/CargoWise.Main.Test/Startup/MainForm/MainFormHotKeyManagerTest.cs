using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Interop;
using CargoWise.Main.Navigation;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Main.ModuleTreeLoader;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant;
using MainFormForTest = Enterprise.Startup.Testing.MainFormTestCase.TestMainForm;

namespace Enterprise.Startup.Testing
{
	sealed class MainFormHotKeyManagerTest : TransactionedTestCase
	{
		class MainFormWithDeniedRights : MainFormForTest
		{
			internal override bool HasLicenceAndSecurityPermissions(MainFormModule module)
			{
				return false;
			}
		}

		class MainFormWithUnReadItems : MainFormForTest
		{
			internal override bool HasUnReadItemsMandatoryToRead()
			{
				return true;
			}
		}

		class NullUserUserContext : UserContext, IUserContext
		{
			public NullUserUserContext(IUserContext currentUserContext)
			{
				this.currentUserContext = currentUserContext;
			}
			readonly IUserContext currentUserContext;

			IBranch IUserContext.Branch
			{
				get { return currentUserContext.Branch; }
			}

			IDepartment IUserContext.Department
			{
				get { return currentUserContext.Department; }
			}

			IUser IUserContext.User
			{
				get { return null; }
			}

			ICompany IUserContext.Company
			{
				get { return currentUserContext.Company; }
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestPreFilterMessageWithDeniedRights()
		{
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();
			using (var mainForm = new MainFormWithDeniedRights())
			{
				AssertNoExceptionThrown(() => PressHotkey(mainForm, Keys.F7));
			}
		}

		[RequiresSTA]
		public void TestPreFilterMessageWithNullUser()
		{
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();
			using (var mainForm = new MainFormForTest())
			using (Env.Instance.SetTemporaryUserContext(new NullUserUserContext(Env.Instance.CurrentUserContext)))
			{
				AssertNull("Precondition:", Env.CurrentUser);
				Assert(!PressHotkey(mainForm, Keys.Control | Keys.F7));
			}
		}

		[RequiresSTA]
		public void TestGlobalHotkeyServiceRequestWithNoUserLogin()
		{
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();

			using (var mainForm = new MainFormForTest())
			{
				mainForm.Show();
				Application.DoEvents();
				Exception exceptionThrown = null;

				using (Env.SetTemporaryUserContext(null))
				{
					try
					{
						AssertNull("Precondition: No user logged in", GlbCompany.CurrentCompany);
						Assert(!PressHotkey(mainForm, Keys.F1));
					}
					catch (Exception ex)
					{
						exceptionThrown = ex;
					}
				}

				AssertNull("Exception should not have been thrown", exceptionThrown);
			}
		}

		[RequiresSTA]
		public void TestGlobalHotkeyOnAnotherThread_ServiceRequest()
		{
			AssertGlobalHotkeysOnOtherThread(Keys.F1, true, (mainForm) => { });
		}

		[RequiresSTA]
		public void TestGlobalHotkeyOnAnotherThread_Communication()
		{
			AssertGlobalHotkeyThatOpensForm(Keys.F6, true, "New Communication");
		}
		[RequiresSTA]
		public void TestGlobalHotkeyOnAnotherThread_Organization()
		{
			AssertGlobalHotkeyThatOpensForm(Keys.F7, true, "Organization");
		}
		[RequiresSTA]
		public void TestGlobalHotkeyOnAnotherThread_Inquiry()
		{
			AssertGlobalHotkeyThatOpensForm(Keys.F8, true, "Inquiry Manager");
		}

		void AssertGlobalHotkeyThatOpensForm(Keys hotkey, bool shouldBeProcessed, string expectedFormText)
		{
			AssertGlobalHotkeysOnOtherThread(hotkey, shouldBeProcessed, (mainForm) =>
			{
				var latestForm = Application.OpenForms[Application.OpenForms.Count - 1];
				AssertEquals("Should be the form we asked for", expectedFormText, latestForm.Text);

				Assert("Should have been created on the tests main form", !latestForm.InvokeRequired);

				latestForm.Close();
			});
		}

		void AssertGlobalHotkeysOnOtherThread(Keys hotkey, bool shouldBeProcessed, Action<MainForm> assertions)
		{
			using (var mainForm = new MainFormForTest())
			{
				mainForm.Show();
				Application.DoEvents();
				Assert("PRE: Handle must have been created to use BeginInvoke", mainForm.IsHandleCreated);

				bool wasProcessed = false;
				Exception exceptionThrownOnOtherThread = null;
				var thread = new Thread(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						using (var someOtherForm = new ZForm())
						{
							someOtherForm.Show();
							wasProcessed = PressHotkey(someOtherForm, hotkey);
						}
					}
					catch (Exception ex)
					{
						exceptionThrownOnOtherThread = ex;
					}
				});
				thread.Start();

				Assert("Deadlocked", thread.Join(TimeSpan.FromSeconds(10)));
				AssertNull("Exception shouldnt have been thrown", exceptionThrownOnOtherThread);
				AssertEquals("Should return that the key will was/wasnt processed", shouldBeProcessed, wasProcessed);

				Application.DoEvents();

				assertions(mainForm);
			}
		}

		[RequiresSTA]
		public void TestPreFilterMessageForNavigationShortcuts()
		{
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();

			using (var mainForm = new MainFormForTest())
			{
				mainForm.Show();

				PressHotkey(mainForm, Keys.Control | Keys.D1);
				AssertEquals(Category.Jump.Name, mainForm.NavigationBar.SelectedCategory.Name);

				PressHotkey(mainForm, Keys.Control | Keys.D2);
				AssertEquals(Category.Operations.Name, mainForm.NavigationBar.SelectedCategory.Name);

				PressHotkey(mainForm, Keys.W);
				AssertEquals("W", ((MenuSection)mainForm.NavigationBar.navigationViewModel.SelectedCategory.SelectedItem).Letter);

				PressHotkey(mainForm, Keys.S);
				AssertEquals("S", ((MenuSection)mainForm.NavigationBar.navigationViewModel.SelectedCategory.SelectedItem).Letter);

				PressHotkey(mainForm, Keys.Control | Keys.D3);
				AssertEquals(Category.Manage.Name, mainForm.NavigationBar.SelectedCategory.Name);

				PressHotkey(mainForm, Keys.Control | Keys.D4);
				AssertEquals(Category.Admin.Name, mainForm.NavigationBar.SelectedCategory.Name);

				Assert("Not in search mode", !mainForm.NavigationBar.navigationViewModel.IsInSearchMode);

				PressHotkey(mainForm, Keys.Control | Keys.F);
				Assert("Not in search mode - not on Jump", !mainForm.NavigationBar.navigationViewModel.IsInSearchMode);

				PressHotkey(mainForm, Keys.Control | Keys.D1);
				AssertEquals(Category.Jump.Name, mainForm.NavigationBar.SelectedCategory.Name);

				PressHotkey(mainForm, Keys.Control | Keys.F);
				Assert("In search mode", mainForm.NavigationBar.navigationViewModel.IsInSearchMode);

				PressHotkey(mainForm, Keys.F7);
				AssertEquals("Latest Form", "Organization", Application.OpenForms[Application.OpenForms.Count - 1].Text);
			}
		}

		bool PressHotkey(Form form, Keys keyToSend)
		{
			var lParam = (IntPtr)(keyToSend & ~(Keys.Control | Keys.Shift | Keys.Alt));
			var msg = new Message { Msg = WindowsMessage.WM_KEYDOWN, LParam = lParam };

			var method = form.GetType().GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic);
			return (bool)method.Invoke(form, new object[] { msg, keyToSend });
		}

		[RequiresSTA]
		public void TestPreFilterMessageWhenHasNoUnReadItemsMandatoryToRead()
		{
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();

			using (var mainForm = new MainFormForTest())
			using (var textBox = new TextBox())
			{
				mainForm.Controls.Add(textBox);
				mainForm.Show();
				mainForm.ByPassHasUnReadItemsCheck = true;
				textBox.Focus();
				Application.DoEvents();

				Assert("Not a registered hotkey, so nothing should happen", !PressHotkey(mainForm, Keys.Control | Keys.A));
				Application.DoEvents();
				AssertEquals("Latest Form Type", typeof(MainFormForTest), Application.OpenForms[Application.OpenForms.Count - 1].GetType());

				PressHotkey(mainForm, Keys.F6);
				Application.DoEvents();
				AssertEquals("textBox.Focused", false, textBox.Focused);
				AssertEquals("Latest Form", ControllerIDs.Communication, ((ZForm)Application.OpenForms[Application.OpenForms.Count - 1]).ControllerID);

				using (Env.SetTemporaryUserContext(null))
				{
					Assert("No user logged in, cant open form", !PressHotkey(mainForm, Keys.F6));
					AssertNoExceptionThrown(() => Application.DoEvents());
				}

				Assert("Pressing F7 should open the main form", PressHotkey(mainForm, Keys.F7));
				Application.DoEvents();
				AssertEquals("textBox.Focused", false, textBox.Focused);
				AssertEquals("Latest Form", "Organization", Application.OpenForms[Application.OpenForms.Count - 1].Text);

				var openFormCount = Application.OpenForms.Count;
				Assert("Should still consume the keypress", PressHotkey(mainForm, Keys.F7));
				Application.DoEvents();
				AssertEquals("No new windows should be opened.", openFormCount, Application.OpenForms.Count);
				AssertNotEquals("Only one organizations module window should be open.", "Organization", Application.OpenForms[Application.OpenForms.Count - 2].Text);

				Assert(PressHotkey(mainForm, Keys.F8));
				Application.DoEvents();
				AssertEquals("textBox.Focused", false, textBox.Focused);
				AssertEquals("Latest Form", "Inquiry Manager", Application.OpenForms[Application.OpenForms.Count - 1].Text);

				openFormCount = Application.OpenForms.Count;
				Assert("Still should consume the keypress", PressHotkey(mainForm, Keys.F8));
				Application.DoEvents();
				AssertEquals("No new windows should be opened.", openFormCount, Application.OpenForms.Count);
				AssertNotEquals("Only one inquiry module window should be open.", "Inquiry Manager", Application.OpenForms[Application.OpenForms.Count - 2].Text);
			}

			loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();
		}

		[RequiresSTA]
		public void TestOpenModuleByHotKeyWhenHasUnReadItemsMandatoryToRead()
		{
			var loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();

			using (var mainForm = new MainFormWithUnReadItems())
			{
				mainForm.Show();
				mainForm.ByPassHasUnReadItemsCheck = false;

				PressHotkey(mainForm, Keys.F6);
				Application.DoEvents();
				AssertEquals("New Form should not be opened", typeof(MainFormWithUnReadItems), Application.OpenForms[Application.OpenForms.Count - 1].GetType());

				PressHotkey(mainForm, Keys.F7);
				Application.DoEvents();
				AssertEquals("New Form should not be opened", typeof(MainFormWithUnReadItems), Application.OpenForms[Application.OpenForms.Count - 1].GetType());

				PressHotkey(mainForm, Keys.F8);
				Application.DoEvents();
				AssertEquals("New Form should not be opened", typeof(MainFormWithUnReadItems), Application.OpenForms[Application.OpenForms.Count - 1].GetType());
			}

			loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();
		}

		[RequiresSTA]
		public void TestPreFilterMessageGlobalShortcut()
		{
			ModuleTreeLoader loader = new ModuleTreeLoader();
			loader.Initialise(ModuleTree.Tree, Env.Security);
			loader.LoadModules();

			using (var mainForm = new MainFormForTest())
			{
				using (var textBox = new TextBox())
				{
					mainForm.Controls.Add(textBox);
					mainForm.Show();
					mainForm.Focus();
					textBox.Focus();

					PressHotkey(mainForm, Keys.Control | Keys.D3);
					var foundShortcut = PressHotkey(mainForm, Keys.Control | Keys.Shift | Keys.B);

					AssertEquals("Must not find a local shortcut", false, foundShortcut);
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			foreach (var form in Application.OpenForms.OfType<ZForm>().ToArray())
			{
				if (form.ControllerID == ControllerIDs.Communication)
				{
					form.Dispose();
				}
			}
		}
	}
}
