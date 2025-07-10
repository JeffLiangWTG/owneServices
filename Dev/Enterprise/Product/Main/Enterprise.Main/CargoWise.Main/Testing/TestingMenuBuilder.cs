#if DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Main.Diagnostics;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Types;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DbUpgrader.ReferenceDatabases;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Startup.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Startup
{
	sealed class TestingMenuBuilder
	{
		public TestingMenuBuilder(Form parentForm, NavigationViewModel viewModel)
		{
			this.parentForm = parentForm;
			this.navViewModel = viewModel;
		}

		readonly Form parentForm;
		readonly NavigationViewModel navViewModel;

		public ZToolStripMenuItem GetMenu()
		{
			UnitTestMenuItem = new ZToolStripMenuItem(navViewModel.TestingMenuTitle) { Name = "TestingToolStripMenuItem" };
			UnitTestMenuItem.Image = Icons.GetImage(IconTypes.EditButtonActive);
			TypeDescriptor.AddAttributes(UnitTestMenuItem, new SuppressFormsLocalizedTestAttribute());
			AddTestingDropDownItems();
			UnitTestMenuItem.DropDownOpening += (o, e) => { BuildTestingMenu(); };

			return UnitTestMenuItem;
		}

		void AddTestingDropDownItems()
		{
			UnitTestMenuItem.DropDownItems.Clear();

			AddNonUnitMenuItems();
			UnitTestMenuItem.DropDownItems.Add(new ToolStripSeparator());

			AddRepeatTestItems();
			UnitTestMenuItem.DropDownItems.Add(new ToolStripSeparator());

			AddNotRecentlyRunTestItems();
		}

		void AddNonUnitMenuItems()
		{
			AddDeveloperItems();
			AddLoadedClientAssmblyItems();
			AddDbUpgradeItems();
		}

		void AddDeveloperItems()
		{
			var devTestingMenu = new ZToolStripMenuItem("&Developer Functions");
			UnitTestMenuItem.DropDownItems.Add(devTestingMenu);

			var testRigMenuItem = new ZToolStripMenuItem("&TestRig", TestRigMenuItem_Click);
			testRigMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.T;
			testRigMenuItem.ShowShortcutKeys = true;
			devTestingMenu.DropDownItems.Add(testRigMenuItem);

			var licenceMenu = new ZToolStripMenuItem("&Licence");
			devTestingMenu.DropDownItems.Add(licenceMenu);
			licenceMenu.DropDownItems.Add(new ZToolStripMenuItem("Show Product Registration", OnShowProductRegistration));
			licenceMenu.DropDownItems.Add(new ToolStripSeparator());
			licenceMenu.DropDownItems.Add(new ZToolStripMenuItem("Show System Licence", OnShowSystemLicence));
			licenceMenu.DropDownItems.Add(new ZToolStripMenuItem("Edit System Licence", OnEditSystemLicence));
			licenceMenu.DropDownItems.Add(new ZToolStripMenuItem("Set Production System Licence", OnSetPRDSystemLicence));
			licenceMenu.DropDownItems.Add(new ZToolStripMenuItem("Set Test System Licence", OnSetTSTSystemLicence));
			licenceMenu.DropDownItems.Add(new ZToolStripMenuItem("Show System Expiry", OnShowSystemExpiry));
			var systemExpiryMenu = new ZToolStripMenuItem("Set System Expiry");
			systemExpiryMenu.DropDownItems.Add(new ZToolStripMenuItem("30 Days", OnSystemExpiry30Days));
			systemExpiryMenu.DropDownItems.Add(new ZToolStripMenuItem("5 Days", OnSystemExpiry5Days));
			systemExpiryMenu.DropDownItems.Add(new ZToolStripMenuItem("Today", OnSystemExpiryToday));
			systemExpiryMenu.DropDownItems.Add(new ZToolStripMenuItem("Yesterday", OnSystemExpiryYesterday));
			systemExpiryMenu.DropDownItems.Add(new ZToolStripMenuItem("5 Days ago", OnSystemExpiry5DaysAgo));
			licenceMenu.DropDownItems.Add(systemExpiryMenu);
		}

		void AddLoadedClientAssmblyItems()
		{
			ClientAssemblySelectionMenuItem = new ZToolStripMenuItem("&Loaded Client Assembly");
			ClientAssemblySelectionMenuItem.DropDownOpening += ClientAssemblySelectionMenuItem_Popup;
			ClientAssemblySelectionMenuItem.DropDownItems.Add(new ToolStripSeparator());
			UnitTestMenuItem.DropDownItems.Add(ClientAssemblySelectionMenuItem);
		}

		void AddDbUpgradeItems()
		{
			var dbUpgradeMenu = new ZToolStripMenuItem("DB &Upgrade");
			UnitTestMenuItem.DropDownItems.Add(dbUpgradeMenu);

			var runClientDBScriptsTestMenu = new ZToolStripMenuItem("Run Client-Specific DB Scripts", OnRunClientDBScripts);
			dbUpgradeMenu.DropDownItems.Add(runClientDBScriptsTestMenu);
			var runDbScriptUpgradeTestMenu = new ZToolStripMenuItem("Run DB Script Upgrade", OnRunDbScriptUpgrade);
			dbUpgradeMenu.DropDownItems.Add(runDbScriptUpgradeTestMenu);
			var runDbUpgradeTestMenu = new ZToolStripMenuItem("Run DB Upgrade", OnRunDbUpgrade);
			dbUpgradeMenu.DropDownItems.Add(runDbUpgradeTestMenu);
			var runDocumentsReloadMenuItem = new ZToolStripMenuItem("Reload DocumentsComplete.xml from file into DB", OnRunDocumentsCompleteXmlReload);
			dbUpgradeMenu.DropDownItems.Add(runDocumentsReloadMenuItem);
		}

		void AddRepeatTestItems()
		{
			AddResetSRDbForTestingItems();
			AddProductSpecificItems();
			AddRunTestsFromItems();
			AddRepeatUserSubmittedItems();
		}

		void AddResetSRDbForTestingItems()
		{
			var sRDbTestMenu = new ZToolStripMenuItem("&Reset Single Reference Database");
			UnitTestMenuItem.DropDownItems.Add(sRDbTestMenu);
			sRDbTestMenu.DropDownItems.Add(new ZToolStripMenuItem("&Use DAT SRDb Snapshot", SRDbTestsDATSnapshotResetMenuItem_Click));
			sRDbTestMenu.DropDownItems.Add(new ZToolStripMenuItem("&Use Reference Service", SRDbTestsReferenceServiceResetMenuItem_Click));
		}

		void AddProductSpecificItems()
		{
			var productTestMenu = new ZToolStripMenuItem("&Product Tests");
			UnitTestMenuItem.DropDownItems.Add(productTestMenu);
			productTestMenu.DropDownItems.Add(new ZToolStripMenuItem("&Customs", CustomsTestsMenuItem_Click));
			productTestMenu.DropDownItems.Add(new ZToolStripMenuItem("&Freight", FreightTestsDropDownItems_Click));
		}

		void AddRunTestsFromItems()
		{
			var runTestsFromMenu = new ZToolStripMenuItem("&Run Tests from...");
			UnitTestMenuItem.DropDownItems.Add(runTestsFromMenu);

			runCheckedOutFileTestsTestMenu = new ZToolStripMenuItem("... checked out files", OnRunCheckedOutFileTests);
			runCheckedOutFileTestsTestMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.X;
			runCheckedOutFileTestsTestMenu.ShowShortcutKeys = true;
			runTestsFromMenu.DropDownItems.Add(runCheckedOutFileTestsTestMenu);

			runCheckedOutFileTestsIncludingInheritingTestMenu = new ZToolStripMenuItem("... checked out files and inheriting classes", OnRunCheckedOutFileTestsInherit);
			runCheckedOutFileTestsIncludingInheritingTestMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
			runCheckedOutFileTestsIncludingInheritingTestMenu.ShowShortcutKeys = true;
			runTestsFromMenu.DropDownItems.Add(runCheckedOutFileTestsIncludingInheritingTestMenu);

			runCheckedOutProjecteTestsTestMenu = new ZToolStripMenuItem("... checked out projects", OnRunCheckedOutProjectTests);
			runCheckedOutProjecteTestsTestMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
			runCheckedOutProjecteTestsTestMenu.ShowShortcutKeys = true;
			runTestsFromMenu.DropDownItems.Add(runCheckedOutProjecteTestsTestMenu);

			uncommittedFileTestsMenu = new ZToolStripMenuItem("... files with uncommitted changes", (s, e) => UnitTestRunner.ShowTestsFromUncommittedFiles(false));
			uncommittedFileTestsMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.U;
			uncommittedFileTestsMenu.ShowShortcutKeys = true;
			runTestsFromMenu.DropDownItems.Add(uncommittedFileTestsMenu);

			uncommittedFileIncludingInheritingTestsMenu = new ZToolStripMenuItem("... files with uncommitted changes and inheriting classes", (s, e) => UnitTestRunner.ShowTestsFromUncommittedFiles(true));
			uncommittedFileIncludingInheritingTestsMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
			uncommittedFileIncludingInheritingTestsMenu.ShowShortcutKeys = true;
			runTestsFromMenu.DropDownItems.Add(uncommittedFileIncludingInheritingTestsMenu);

			uncommittedProjectsTestsMenu = new ZToolStripMenuItem("... projects with uncommitted changes", (s, e) => UnitTestRunner.ShowTestsFromProjectsWithUncommittedChanges());
			uncommittedProjectsTestsMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.M;
			uncommittedProjectsTestsMenu.ShowShortcutKeys = true;
			runTestsFromMenu.DropDownItems.Add(uncommittedProjectsTestsMenu);

			var runPreviousTestRunTestMenu = new ZToolStripMenuItem("... a Previous Run", OnRepeatTests)
			{
				ShortcutKeys = Keys.Control | Keys.Shift | Keys.Q,
				ShowShortcutKeys = true,
			};

			var runPreviousDATRunTestMenu = new ZToolStripMenuItem("... a Previous DAT Run", OnRunPreviousDatTests)
			{
				ShortcutKeys = Keys.Control | Keys.Shift | Keys.W,
				ShowShortcutKeys = true,
			};

			runTestsFromMenu.DropDownItems.Add(runPreviousTestRunTestMenu);
			runTestsFromMenu.DropDownItems.Add(runPreviousDATRunTestMenu);

			runTestsFromMenu.DropDownOpening += RunTestsFromMenu_Opening;
		}

		void RunTestsFromMenu_Opening(object sender, EventArgs e)
		{
			if (IsGit)
			{
				var currentBranchName = SourceControl.EnterpriseDatabase.GetCurrentBranchName();
				runCheckedOutFileTestsTestMenu.Text = $"... files with changes in {currentBranchName}";
				runCheckedOutFileTestsIncludingInheritingTestMenu.Text = $"... files with changes in {currentBranchName} and inherting classes";
				runCheckedOutProjecteTestsTestMenu.Text = $"... projects with changes in {currentBranchName}";
			}
			else
			{
				uncommittedFileTestsMenu.Visible = false;
				uncommittedFileIncludingInheritingTestsMenu.Visible = false;
				uncommittedProjectsTestsMenu.Visible = false;
			}
		}

		void AddRepeatUserSubmittedItems()
		{
			UserSubmittedTestFailuresMenuItem_EveryoneElse = new ZToolStripMenuItem("User Submitted Test &Failures");
			UserSubmittedTestFailuresMenuItem_EveryoneElse.DropDownItems.Add(new ZToolStripMenuItem("Placeholder"));
			UserSubmittedTestFailuresMenuItem_EveryoneElse.DropDownOpening += UserSubmittedTestFailuresMenuItem_Popup;
			UnitTestMenuItem.DropDownItems.Add(UserSubmittedTestFailuresMenuItem_EveryoneElse);

			UserSubmittedTestFailuresMenuItem_Mine = new ZToolStripMenuItem("My Te&st Failures");
			UserSubmittedTestFailuresMenuItem_Mine.DropDownItems.Add(new ZToolStripMenuItem("Placeholder"));
			UserSubmittedTestFailuresMenuItem_Mine.DropDownOpening += UserSubmittedTestFailuresMenuItemMine_Popup;
			UnitTestMenuItem.DropDownItems.Add(UserSubmittedTestFailuresMenuItem_Mine);

			ErrorGettingTestRunListMenuItem = new ZToolStripMenuItem("&Error Getting Test Run List");
			ErrorGettingTestRunListMenuItem.Visible = false;
			UserSubmittedTestFailuresMenuItem_EveryoneElse.DropDownItems.Add(ErrorGettingTestRunListMenuItem);
			UserSubmittedTestFailuresMenuItem_Mine.DropDownItems.Add(ErrorGettingTestRunListMenuItem);
		}

		void AddNotRecentlyRunTestItems()
		{
			ClientSpecificUnitTestsMenuItem = new ZToolStripMenuItem("Client Specific Tests I have not recently run");
			UnitTestMenuItem.DropDownItems.Add(ClientSpecificUnitTestsMenuItem);

			var clientSpecificUnitTests_All = new ZToolStripMenuItem("All", ClientTests_All_Click);
			ClientSpecificUnitTestsMenuItem.DropDownItems.Add(clientSpecificUnitTests_All);

			var clientSpecificUnitTests_A2M = new ZToolStripMenuItem("A-M", ClientTests_A2M_Click);
			ClientSpecificUnitTestsMenuItem.DropDownItems.Add(clientSpecificUnitTests_A2M);

			var clientSpecificUnitTests_N2Z = new ZToolStripMenuItem("N-Z", ClientTests_N2Z_Click);
			ClientSpecificUnitTestsMenuItem.DropDownItems.Add(clientSpecificUnitTests_N2Z);

			TestNotRecentlyRunMenuItem = new ZToolStripMenuItem("Tests I have not recently run");
			UnitTestMenuItem.DropDownItems.Add(TestNotRecentlyRunMenuItem);
		}

		ZToolStripMenuItem UnitTestMenuItem;
		ZToolStripMenuItem TestNotRecentlyRunMenuItem;
		ZToolStripMenuItem ClientSpecificUnitTestsMenuItem;
		ZToolStripMenuItem UserSubmittedTestFailuresMenuItem_Mine;
		ZToolStripMenuItem UserSubmittedTestFailuresMenuItem_EveryoneElse;
		ZToolStripMenuItem ErrorGettingTestRunListMenuItem;
		ZToolStripMenuItem ClientAssemblySelectionMenuItem;
		ZToolStripMenuItem runCheckedOutFileTestsTestMenu;
		ZToolStripMenuItem runCheckedOutFileTestsIncludingInheritingTestMenu;
		ZToolStripMenuItem runCheckedOutProjecteTestsTestMenu;
		ZToolStripMenuItem uncommittedFileTestsMenu;
		ZToolStripMenuItem uncommittedFileIncludingInheritingTestsMenu;
		ZToolStripMenuItem uncommittedProjectsTestsMenu;
		bool IsGit => lazyIsGit.Value;
		readonly Lazy<bool> lazyIsGit = new Lazy<bool>(() => SourceControl.EnterpriseDatabase.GetType().Name == "GitSourceControl");

		void BuildTestingMenu()
		{
			if (!hasCalculatedUnitTests)
			{
				hasCalculatedUnitTests = true;

				try
				{
					var buildxml = CargoWise.BuildTools.BuildXml.Instance;
					var solutionFileNames = buildxml.GetAllSolutionFileNames();
					buildxml.Release();

					for (var i = 0; i < solutionFileNames.Length; i++)
					{
						solutionFileNames[i] = Path.GetFileNameWithoutExtension(solutionFileNames[i]);
					}
					ArrayList.Adapter(solutionFileNames).Sort();

					var testsNotRecentlyRunDictionary = new Dictionary<ZString, List<ZToolStripMenuItem>>();
					var enterpriseTestsNotRecentlyRunDictionary = new Dictionary<ZString, List<ZToolStripMenuItem>>();
					var cargowiseTestsNotRecentlyRunDictionary = new Dictionary<ZString, List<ZToolStripMenuItem>>();
					var clientSpecificUnitTestsDictionary = new Dictionary<ZString, List<ZToolStripMenuItem>>();

					UnitTestMenuItem.DropDownItems.Add(new ToolStripSeparator());
					var xmlDocDirectory = UnitTestForm.GetOrCreateXmlDocDirectory();
					foreach (ZString solutionName in solutionFileNames)
					{
						var xmlFileName = Path.Combine(xmlDocDirectory, solutionName + "_Status.xml");
						var testRecentlyRun = File.Exists(xmlFileName) && (File.GetLastWriteTime(xmlFileName).AddDays(7) > ZDateTime.Now.ToDateTime());
						var toolStripMenuItem = new ZToolStripMenuItem(solutionName, UnitTestHandler);
						if (testRecentlyRun)
						{
							UnitTestMenuItem.DropDownItems.Add(toolStripMenuItem);
						}
						else
						{
							var isClientTest = solutionName.StartsWith("ZClient");
							Dictionary<ZString, List<ZToolStripMenuItem>> testsTableToAdd;
							var solutionNameKey = ZString.Empty;
							if (isClientTest)
							{
								testsTableToAdd = clientSpecificUnitTestsDictionary;
								solutionNameKey = solutionName.SubstringSafe(7, 1);
							}
							else
							{
								const string EnterprisePrefix = "Enterprise.";
								const string CargowisePrefix = "CargoWise.";

								if (solutionName.Length > EnterprisePrefix.Length && solutionName.ToString().StartsWith(EnterprisePrefix, StringComparison.OrdinalIgnoreCase))
								{
									solutionNameKey = solutionName.Left(EnterprisePrefix.Length + 1);
									testsTableToAdd = enterpriseTestsNotRecentlyRunDictionary;
								}
								else if (solutionName.Length > CargowisePrefix.Length && solutionName.ToString().StartsWith(CargowisePrefix, StringComparison.OrdinalIgnoreCase))
								{
									solutionNameKey = solutionName.Left(CargowisePrefix.Length - 1); // not enough CargoWise solutions to bother breaking it down further just yet.
									testsTableToAdd = cargowiseTestsNotRecentlyRunDictionary;
								}
								else
								{
									testsTableToAdd = testsNotRecentlyRunDictionary;
									solutionNameKey = solutionName.Left(1);
								}
							}

							var listToAddTestTo = GetListToAddTestTo(solutionNameKey, testsTableToAdd);
							listToAddTestTo.Add(toolStripMenuItem);
						}
					}

					if (testsNotRecentlyRunDictionary.Count > 0)
					{
						AddTestItems(testsNotRecentlyRunDictionary, TestNotRecentlyRunMenuItem);
						TestNotRecentlyRunMenuItem.DropDownItems.Add(new ToolStripSeparator());

						AddTestItems(cargowiseTestsNotRecentlyRunDictionary, TestNotRecentlyRunMenuItem);
						TestNotRecentlyRunMenuItem.DropDownItems.Add(new ToolStripSeparator());

						AddTestItems(enterpriseTestsNotRecentlyRunDictionary, TestNotRecentlyRunMenuItem);
					}

					if (clientSpecificUnitTestsDictionary.Count > 0)
					{
						AddTestItems(clientSpecificUnitTestsDictionary, ClientSpecificUnitTestsMenuItem);
					}

					RemoveErrorMenuItem();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AddErrorMenuItem();
				}
			}
		}

		#region Event Handlers

		void TestRigMenuItem_Click(object sender, EventArgs e)
		{
			new TestRigForm().Show();
		}

		void OnRepeatTests(object sender, EventArgs e)
		{
			var loader = new TestListLoader();
			var newForm = new LoadTestListForm(loader);
			if (newForm.ShowDialog(parentForm) == DialogResult.OK)
			{
				UnitTestRunner.ShowASpecificListOfUnitTests(loader.GetTestMethods(), loader.GroupTestsByAssembly);
			}
		}

		void OnRunPreviousDatTests(object sender, EventArgs e)
		{
			var newForm = new LoadPreviousDatTestForm();
			if (newForm.ShowDialog(parentForm) == DialogResult.OK)
			{
				UnitTestRunner.ShowUserSubmittedFailures(newForm.UserTestPK, sortTestsAlphabetically: false);
			}
		}

		void OnRunCheckedOutFileTests(object sender, EventArgs e)
		{
			if (IsGit)
			{
				UnitTestRunner.ShowTestsForBranchChanges(false);
			}
			else
			{
				UnitTestRunner.ShowTestsFromCheckedOutFiles(false);
			}
		}

		void OnRunCheckedOutFileTestsInherit(object sender, EventArgs e)
		{
			if (IsGit)
			{
				UnitTestRunner.ShowTestsForBranchChanges(true);
			}
			else
			{
				UnitTestRunner.ShowTestsFromCheckedOutFiles(true);
			}
		}

		void OnRunCheckedOutProjectTests(object sender, EventArgs e)
		{
			if (IsGit)
			{
				UnitTestRunner.ShowTestsFromProjectsWithBranchChanges();
			}
			else
			{
				UnitTestRunner.ShowTestsFromCheckedOutProjects();
			}
		}

		void OnRunClientDBScripts(object sender, EventArgs e)
		{
			if (ClientHookLoader.Instance.ClientHook == null)
			{
				Globals.Message.ShowError("No client DLL is loaded");
			}
			else
			{
				new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(true);
			}
		}

		void OnRunDbScriptUpgrade(object sender, EventArgs e)
		{
			if (Globals.Message.Show("All scripts will be upgraded.", "Database Script Upgrade", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
			{
				var upgraderForm = new UpgForm(new AlwaysRequiredScriptUpgradeManager());
				upgraderForm.ShowDialog();
			}
		}

		void OnRunDbUpgrade(object sender, EventArgs e)
		{
			if (Globals.Message.Show("DB upgrade will be run.", "Database Upgrade", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
			{
				ObjectFactory.Get<IDbUpgraderRunner>().FullUpgrade(new DbUpgraderVersionInfo(null), null, null, DbUpgraderDirector.RunUpgradeGUI);
			}
		}

		void OnRunDocumentsCompleteXmlReload(object sender, EventArgs e)
		{
			var answer = Globals.Message.Show(string.Format(@"This will load the content of DocumentsComplete.xml from disk into the database.
It will clobber any changes in your database if they're not in the file.
It will not change any version number except that contained in the XML data itself.
You should first BUILD EXCELTEMPLATES.sln to ensure that the content of Documents.xml and all .XLS files are embedded into DocumentsComplete.xml.
The default data file is:
	{0}
Press YES to reload from the default file.
Press NO to choose another file. 
Press CANCEL to abort.", DocumentsDataFileForTestingMenu.DocumentsCompleteXmlDefaultFilePath),
				"Reload DocumentsComplete.xml?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

			DocumentsDataFileForTestingMenu dataFile = null;
			if (answer == DialogResult.Yes)
			{
				dataFile = new DocumentsDataFileForTestingMenu(DocumentsDataFileForTestingMenu.DocumentsCompleteXmlDefaultFilePath);
			}
			else if (answer == DialogResult.No)
			{
				using (var fileOpen = new ZOpenFileDialog())
				{
					fileOpen.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
					fileOpen.RestoreDirectory = true;
					if (fileOpen.ShowDialog() == DialogResult.OK)
					{
						dataFile = new DocumentsDataFileForTestingMenu(fileOpen.UnmappedFileName);
					}
				}
			}

			if (dataFile != null)
			{
				new DbUpgrader.Data.DocumentsUpgradeTask(dataFile).Run();
				Globals.Message.Show("Finished reloading XML file");
			}
		}

		void UserSubmittedTestFailuresMenuItem_Popup(object sender, EventArgs e)
		{
			if (!hasCalculatedSubmittedTestRuns_EveryoneElse)
			{
				UserSubmittedTestFailuresMenuItem_EveryoneElse.DropDownItems.Clear();
			}

			new UserSubmittedTestRunsMenuBuilder().BuildMenu(UserSubmittedTestFailuresMenuItem_EveryoneElse, ErrorGettingTestRunListMenuItem, ref hasCalculatedSubmittedTestRuns_EveryoneElse);
		}

		void UserSubmittedTestFailuresMenuItemMine_Popup(object sender, EventArgs e)
		{
			if (!hasCalculatedSubmittedTestRuns_Mine)
			{
				UserSubmittedTestFailuresMenuItem_Mine.DropDownItems.Clear();
			}

			new UserSubmittedTestRunsMenuBuilder(true).BuildMenu(UserSubmittedTestFailuresMenuItem_Mine, ErrorGettingTestRunListMenuItem, ref hasCalculatedSubmittedTestRuns_Mine);
			UserSubmittedTestFailuresMenuItem_Mine.ShowShortcutKeys = true;

			var items = UserSubmittedTestFailuresMenuItem_Mine.DropDownItems;
			for (var i = 0; i < items.Count && i < 9; i++)
			{
				((ZToolStripMenuItem)items[i]).ShortcutKeys = Keys.Alt | (Keys.D1 + i);
			}

			if (items.Count >= 10)
			{
				((ZToolStripMenuItem)items[9]).ShortcutKeys = Keys.Alt | Keys.D0;
			}
		}

		bool hasCalculatedSubmittedTestRuns_EveryoneElse;
		bool hasCalculatedSubmittedTestRuns_Mine;

		void ClientTests_All_Click(object sender, EventArgs e)
		{
			RunTestsFromDllsMatchingPattern("All Client Specific", "ZClient[A-Z]{3}.dll");
		}

		void ClientTests_A2M_Click(object sender, EventArgs e)
		{
			RunTestsFromDllsMatchingPattern("A-M Client Specific", "ZClient[A-M][A-Z]{2}.dll");
		}

		void ClientTests_N2Z_Click(object sender, EventArgs e)
		{
			RunTestsFromDllsMatchingPattern("N-Z Client Specific", "ZClient[N-Z][A-Z]{2}.dll");
		}

		void FreightTestsDropDownItems_Click(object sender, EventArgs e)
		{
			RunTestsFromDllsMatchingPattern("All Freight", @"Enterprise\.Freight[A-Z.]*\.dll");
		}

		void RunTestsFromDllsMatchingPattern(string name, string pattern)
		{
			var selectedAssemblies = new List<string>();
			var patternRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

			foreach (var assembly in CargoWise.BuildTools.BuildXml.Instance.GetAllTestedAssemblies())
			{
				var match = patternRegex.Match(assembly);
				if (match != null && match.Success)
				{
					selectedAssemblies.Add(Path.ChangeExtension(assembly, null));
				}
			}

			UnitTestRunner.ShowUnitTestsFromAssemblies(selectedAssemblies, name, new UnitTestErrorDescriptionList());
		}

		void SRDbTestsDATSnapshotResetMenuItem_Click(object sender, EventArgs e)
		{
			if (RefDbTableNameResolver.GetSingleRefDatabaseName(Db.Connection) != RefDbTableNameResolver.DefaultSingleRefDbName)
			{
				Globals.Message.Show("Please restore default SRDb name in the system registry before Reset SRDb with DAT snapshot");
			}
			else if (Globals.Message.Show($"This is going to reset all schema and data in {RefDbTableNameResolver.DefaultSingleRefDbName}. It's recommended to rename/backup your current {RefDbTableNameResolver.DefaultSingleRefDbName} before proceeding. Are you sure you want to proceed?", "SRDb Reset Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				using (var connection = Db.NewAdminConnection())
				{
					OfflineSRDbHelper.ResetSRDbAndPopulateSchema(connection, null);
				}
				new RefDatabaseSynonymRecreator(Db.Connection).RecreateSynonym();
				Globals.Message.Show($"{RefDbTableNameResolver.DefaultSingleRefDbName} is upgraded from DAT SDRb snapshost succesfully (no data), synonym recreated", "SRDb Reset Status", MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
			}
		}

		void SRDbTestsReferenceServiceResetMenuItem_Click(object sender, EventArgs e)
		{
			if (RefDbTableNameResolver.GetSingleRefDatabaseName(Db.Connection) != RefDbTableNameResolver.DefaultSingleRefDbName)
			{
				Globals.Message.Show("Please restore default SRDb name in the system registry before Reset SRDb with reference service");
			}
			else if (Globals.Message.Show($"This is going to reset all schema and data in {RefDbTableNameResolver.DefaultSingleRefDbName} and might take few minutes, are you sure you want to proceed?", "SRDb Reset Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				using (var connection = Db.NewAdminConnection())
				{
					OfflineSRDbHelper.ResetSRDb(connection, null);
					new ReferenceDbUpgradeDirector(null, connection, new SharedRefDbUpgradeLogger()).UpgradeSRDbWithLiveService(connection, RefDbTableNameResolver.DefaultSingleRefDbName);
				}
				new RefDatabaseSynonymRecreator(Db.Connection).RecreateSynonym();
				Globals.Message.Show($"{RefDbTableNameResolver.DefaultSingleRefDbName} is upgraded from live service succesfully (no data), synonym recreated", "SRDb Reset Status", MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
			}
		}

		void CustomsTestsMenuItem_Click(object sender, EventArgs e)
		{
			RunTestsFromDllsMatchingPattern("All Customs", @"customs.*\.dll");
		}

		void UnitTestHandler(object sender, EventArgs e)
		{
			var toolStripMenuItem = (ZToolStripMenuItem)sender;
			if (toolStripMenuItem.OwnerItem != UnitTestMenuItem)
			{
				((ZToolStripMenuItem)toolStripMenuItem.OwnerItem).DropDownItems.Remove(toolStripMenuItem);
				UnitTestMenuItem.DropDownItems.Add(toolStripMenuItem);
			}
			ShowUnitTestsInSolution(toolStripMenuItem.Text);
		}

		void ShowUnitTestsInSolution(string solution)
		{
			var question = string.Format("Running on remote server: {0}" + System.Environment.NewLine + "Continue?", Db.ServerName);

			if (IsDbServerLocal() || Globals.Message.Show(question, "Unit Test Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				try
				{
					UnitTestRunner.ShowUnitTestsInSolution(solution, new UnitTestErrorDescriptionList());
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					NUnit.Framework.TestingState.TearDown();
					Globals.Message.ShowDeveloperException(e);
				}
			}
		}

		bool IsDbServerLocal()
		{
			return
				Db.ServerName == System.Environment.MachineName ||
				Db.ServerName.StartsWith(System.Environment.MachineName + "\\"); // named instance of sql server on local machine
		}

		void AddErrorMenuItem()
		{
			var menuItemErrorLoadingAssemblies = new ZToolStripMenuItem();
			menuItemErrorLoadingAssemblies.Name = "MenuItemErrorLoadingAssemblies";
			menuItemErrorLoadingAssemblies.Text = "(Error loading from Build.xml)";
			UnitTestMenuItem.DropDownItems.Add(menuItemErrorLoadingAssemblies);
		}

		void RemoveErrorMenuItem()
		{
			var errorMenuItem = UnitTestMenuItem.DropDownItems.Find("MenuItemErrorLoadingAssemblies", true).FirstOrDefault();
			if (errorMenuItem != null)
			{
				UnitTestMenuItem.DropDownItems.Remove(errorMenuItem);
				errorMenuItem.Dispose();
			}
		}

		List<ZToolStripMenuItem> GetListToAddTestTo(ZString solutionNameKey, Dictionary<ZString, List<ZToolStripMenuItem>> testsTableToAdd)
		{
			var checkName = solutionNameKey.ToUpper();
			List<ZToolStripMenuItem> result;
			if (!testsTableToAdd.TryGetValue(checkName, out result))
			{
				result = new List<ZToolStripMenuItem>();
				testsTableToAdd[checkName] = result;
			}

			return result;
		}

		void AddTestItems(Dictionary<ZString, List<ZToolStripMenuItem>> testsTableToAddFrom, ZToolStripMenuItem menuItemToAddTo)
		{
			var keys = new List<ZString>();
			foreach (var key in testsTableToAddFrom.Keys)
			{
				keys.Add(key);
			}

			keys.Sort();
			foreach (var firstCharOfSolutionName in keys)
			{
				var list = testsTableToAddFrom[firstCharOfSolutionName];
				if (list.Count >= 1)
				{
					var item = new ZToolStripMenuItem(firstCharOfSolutionName);
					item.DropDownItems.AddRange(list.ToArray());
					menuItemToAddTo.DropDownItems.Add(item);
				}
			}
		}

		bool hasCalculatedUnitTests;

		#endregion

		#region Client Assembly Loading Menu Item

		static readonly string NoClientAssembly = "None";

		void ClientAssemblySelectionMenuItem_Popup(object sender, EventArgs e)
		{
			ClientAssemblySelectionMenuItem.DropDownItems.Clear();
			ClientAssemblySelectionMenuItem.DropDownItems.Add(new ClientAssemblyLoadMenuItem(NoClientAssembly));
			foreach (var clientAssembly in ClientHookLoader.Instance.GetAllClientSpecificAssemblyFileNames())
			{
				ClientAssemblySelectionMenuItem.DropDownItems.Add(new ClientAssemblyLoadMenuItem(clientAssembly));
			}
			ClientAssemblySelectionMenuItem.DropDownOpening -= ClientAssemblySelectionMenuItem_Popup;
		}

		class ClientAssemblyLoadMenuItem : ZToolStripMenuItem
		{
			public ClientAssemblyLoadMenuItem(string assemblyFileName)
			{
				this.AssemblyFileName = assemblyFileName;
				Text = assemblyFileName;
				if (ClientHookLoader.Instance.ClientAssembly == null && assemblyFileName == NoClientAssembly)
				{
					Checked = true;
				}
				else if (ClientHookLoader.Instance.ClientAssembly != null && assemblyFileName.IndexOf(ClientHookLoader.Instance.ClientAssembly.GetName().Name) != -1)
				{
					Checked = true;
				}
			}

			public readonly string AssemblyFileName;

			protected override void OnClick(EventArgs e)
			{
				base.OnClick(e);

				if (AssemblyFileName == NoClientAssembly)
				{
					ClientHookLoader.Instance.OverrideClientAssemblyForTest(null);
				}
				else
				{
					ClientHookLoader.Instance.OverrideClientAssemblyForTest(ClientHookLoader.Instance.GetAssemblyFromFileName(AssemblyFileName));
				}

				foreach (ZToolStripMenuItem otherClientMenuItem in ((ZToolStripMenuItem)OwnerItem).DropDownItems)
				{
					otherClientMenuItem.Checked = false;
				}

				Checked = true;
			}
		}

		#endregion

		#region Licence

		void OnSystemExpiry30Days(object sender, EventArgs e)
		{
			SetSystemExpiry(EnvProxy.Instance.Time.CurrentLocalDateTime.Date.AddDays(30));
		}

		void OnSystemExpiry5Days(object sender, EventArgs e)
		{
			SetSystemExpiry(EnvProxy.Instance.Time.CurrentLocalDateTime.Date.AddDays(5));
		}

		void OnSystemExpiryToday(object sender, EventArgs e)
		{
			SetSystemExpiry(EnvProxy.Instance.Time.CurrentLocalDateTime.Date);
		}

		void OnSystemExpiryYesterday(object sender, EventArgs e)
		{
			SetSystemExpiry(EnvProxy.Instance.Time.CurrentLocalDateTime.Date.AddDays(-1));
		}

		void OnSystemExpiry5DaysAgo(object sender, EventArgs e)
		{
			SetSystemExpiry(EnvProxy.Instance.Time.CurrentLocalDateTime.Date.AddDays(-5));
		}

		void SetSystemExpiry(DateTime expiryDate)
		{
			var encryptedKey = DataRegistry.Instance.RawRegistry.LegacyEncryptedSystemRegistrationKey.Value;

			if (!string.IsNullOrEmpty(encryptedKey))
			{
				ISystemRegistrationKey key = Enterprise.ZArchitecture.Core.SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedKey);
				ISystemRegistrationKey newKey = new SystemRegistrationKey(expiryDate,
					key.ServerSid,
					key.DbInstanceName,
					key.DatabaseName,
					key.DatabaseType,
					key.DbSecurityMode,
					key.HostedLocation,
					key.ExpiredMessage,
					key.ExpiryWeekMessage,
					key.ExpiryMonthMessage,
					key.SystemId,
					key.CurrentBillingTimeZoneUtcOffset,
					key.NextBillingTimeZoneUtcOffset,
					key.NextUtcOffsetEffectiveTimeUtc,
					key.BillingModel);

				DataRegistry.Instance.RawRegistry.LegacyEncryptedSystemRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newKey.ToEncryptedKeyString());
			}

			ShowSystemExpiry();
		}

		void OnShowSystemExpiry(object sender, EventArgs e)
		{
			ShowSystemExpiry();
		}

		void ShowSystemExpiry()
		{
			var encryptedKey = DataRegistry.Instance.RawRegistry.LegacyEncryptedSystemRegistrationKey.Value;

			if (!string.IsNullOrEmpty(encryptedKey))
			{
				var key = Enterprise.ZArchitecture.Core.SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedKey);
				var expiryDate = LicenceExpiryCheck.Create().ExpiryDateTime;
				var timeSpan = expiryDate - EnvProxy.Instance.Time.CurrentLocalDateTime;
				var msg = string.Format("Expiry in {0}d {1}h {2}m\r\nExpiry at: {3} (login branch time)\r\nUnadjusted date: {4}\r\n",
					timeSpan.Days, timeSpan.Hours, timeSpan.Minutes,
					expiryDate.ToString("f"),
					key.SystemExpiryDate.ToShortDateString());
				Globals.Message.Show(msg);
			}
			else
			{
				Globals.Message.Show("System does not have a licence.");
			}
		}

		void OnShowSystemLicence(object sender, EventArgs e)
		{
			ShowSystemLicence();
		}

		void OnEditSystemLicence(object sender, EventArgs e)
		{
			EditSystemLicence();
		}

		void OnSetPRDSystemLicence(object sender, EventArgs e)
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
		}

		void OnSetTSTSystemLicence(object sender, EventArgs e)
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
		}

		void ShowSystemLicence()
		{
			var encryptedKey = DataRegistry.Instance.RawRegistry.LegacyEncryptedSystemRegistrationKey.Value;

			if (!string.IsNullOrEmpty(encryptedKey))
			{
				var xmlKey = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(encryptedKey);

				var key = Enterprise.ZArchitecture.Core.SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedKey);
				var expiryDate = LicenceExpiryCheck.Create().ExpiryDateTime;
				var timeSpan = expiryDate - EnvProxy.Instance.Time.CurrentLocalDateTime;
				var msg = string.Format("Expiry in {0}d {1}h {2}m\r\nExpiry at: {3} (login branch time)\r\nUnadjusted date: {4}\r\n",
					timeSpan.Days, timeSpan.Hours, timeSpan.Minutes,
					expiryDate.ToString("f"),
					key.SystemExpiryDate.ToShortDateString());

				Globals.Message.Show(msg + "\r\n\r\n" + xmlKey);
			}
			else
			{
				Globals.Message.Show("System does not have a licence.");
			}
		}

		void EditSystemLicence()
		{
			var encryptedKey = DataRegistry.Instance.RawRegistry.LegacyEncryptedSystemRegistrationKey.Value;
			var xmlKey = !string.IsNullOrEmpty(encryptedKey)
				? TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(encryptedKey)
				: "";

			var form = new ZChildForm();
			const int FormWidth = 500;
			const int FormHeight = 400;
			const int Margin = 12;
			form.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(FormWidth, FormHeight);

			var editBox = new ZTextBox();
			editBox.CharacterCasing = CharacterCasing.Normal;
			editBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			editBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(Margin, Margin);
			editBox.Multiline = true;
			editBox.Name = "textBox1";
			editBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(FormWidth - Margin * 2, FormHeight - 60);
			editBox.Text = xmlKey;

			var cancelButton = new ZButton();
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			cancelButton.Text = "Cancel";
			cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23);
			cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(FormWidth - 75 - Margin, FormHeight - 23 - Margin);

			var okButton = new ZButton();
			okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			okButton.Text = "OK";
			okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23);
			okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(FormWidth - 75 - Margin - 75 - 6, FormHeight - 23 - Margin);

			form.AcceptButton = okButton;
			form.CancelButton = cancelButton;
			form.Controls.Add(editBox);
			form.Controls.Add(okButton);
			form.Controls.Add(cancelButton);
			form.MainStatusBar.Visible = false;

			using (form)
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					string encryptedXMLKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(editBox.Text);
					form.Close();
					try
					{
						var testKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedXMLKey);
						DataRegistry.Instance.LegacyEncryptedSystemRegistrationKey = encryptedXMLKey;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		void OnShowProductRegistration(object sender, EventArgs e)
		{
			var encryptedKey = DataRegistry.Instance.RawRegistry.EncryptedRegistrationKey.Value;

			if (!string.IsNullOrEmpty(encryptedKey))
			{
				try
				{
					var xmlKey = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(encryptedKey);
					Globals.Message.Show(AddEncodedDatabaseNumber(xmlKey));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.Show("Error decrypting key.\r\n\\r\n" + ex.Message);
				}
			}
			else
			{
				Globals.Message.Show("System does not have a registration.");
			}
		}

		string AddEncodedDatabaseNumber(string xmlKey)
		{
			var xDoc = XDocument.Parse(xmlKey);

			var eleRegistrationKey = xDoc.Element(XName.Get("RegistrationKey"));
			var eleDatabaseNumber = eleRegistrationKey.Element(XName.Get("DatabaseNumber"));
			var eleEncodedDatabaseNumber = new XElement("EncodedDatabaseNumber", ObjectFactory.Get<IProductRegistration>().Key.SystemId);

			eleDatabaseNumber.Remove();
			eleRegistrationKey.AddFirst(eleEncodedDatabaseNumber);
			eleRegistrationKey.AddFirst(eleDatabaseNumber);

			return xDoc.ToString();
		}

		#endregion
	}
}

#endif
