using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Main.ModuleTreeLoader.Test
{
	sealed class MainFormModuleTest : TestCaseWithFactory
	{
		public void TestNamedModulePropertiesCanBeOpenedInNonInteractiveEnvironment()
		{
			var originalValue = Globals.IsUserInteractive;
			Globals.IsUserInteractive = false;
			Loader.LoadModules();

			try
			{
				var someModulesWereProcessed = false;
				foreach (var module in MainFormModules)
				{
					var reloaded = new MainFormModule(module.ModuleID, module.RecordKey, module.RecordUrl, module.RecordDescription);
					DoSomethingSoItDoesntGetRemoved(reloaded.SecurityCheckpoint);
					DoSomethingSoItDoesntGetRemoved(reloaded.LicenceCheckpoint);

					someModulesWereProcessed = true;
				}

				Assert("We should have processed the modules", someModulesWereProcessed);
			}
			finally
			{
				Globals.IsUserInteractive = originalValue;
			}
		}

		IEnumerable<MainFormModule> MainFormModules
		{
			get
			{
				foreach (ModuleCategory category in Tree.Categories.Values)
				{
					foreach (ModuleSection section in category.Sections.Values)
					{
						foreach (MainFormModule module in section.Modules.Values)
						{
							yield return module;
						}
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void DoSomethingSoItDoesntGetRemoved(object o) { }

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithModuleIDNull()
		{
			new MainFormModule((ModuleIdentifier)null);
		}

		[ExpectException(typeof(NullReferenceException))]
		public void TestConstructorWithModuleInfoNull()
		{
			new MainFormModule((ModuleInfo)null);
		}

		public void TestConstructorWithModuleShortcut()
		{
			var shortcut = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);
			var module = new LinkMainFormModule(shortcut);

			AssertEquals("Modules match", shortcut.ModuleName, module.ModuleID.Name);
			AssertEquals("Module ID", shortcut.UniqueKey, module.ID);
		}

		public void TestConstructorWithRecordShortcut()
		{
			var shortcut = new LinkWrapper("Dummy", Guid.NewGuid(), "http://", "description");
			var module = new LinkMainFormModule(shortcut);

			AssertEquals("Modules match", shortcut.ModuleName, module.ModuleID.Name);
			AssertEquals("Module ID", shortcut.UniqueKey, module.ID);
			AssertEquals("Record Description", shortcut.RecordDescription, module.ExtendedDescription);
			AssertEquals("Record URL", shortcut.RecordUrl, module.RecordUrl);
			AssertEquals("Record Key", shortcut.RecordKey, module.RecordKey);
		}

		public void TestConstructorWithStringShortcut()
		{
			var shortcut = new LinkWrapper("Dummy", Guid.NewGuid(), "http://", "description");
			var link = Factory.New<StmLink>();
			link.InitializeLink(shortcut);

			var module = new LinkMainFormModule(link);

			AssertEquals("Modules match", shortcut.ModuleName, module.ModuleID.Name);
			AssertEquals("Module ID", shortcut.UniqueKey, module.ID);
			AssertEquals("Record Description", shortcut.RecordDescription, module.ExtendedDescription);
			AssertEquals("Record URL", shortcut.RecordUrl, module.RecordUrl);
			AssertEquals("Record Key", shortcut.RecordKey, module.RecordKey);
		}

		public void TestConstructorWithShortcutParameters()
		{
			var shortcut = new LinkWrapper("Dummy", Guid.NewGuid(), "http://", "description");
			var module = new MainFormModule(DummyModuleIDs.Dummy, shortcut.RecordKey, shortcut.RecordUrl, shortcut.RecordDescription);

			AssertEquals("Modules match", shortcut.ModuleName, module.ModuleID.Name);
			AssertEquals("Module ID", shortcut.UniqueKey, module.ID);
			AssertEquals("Record Description", shortcut.RecordDescription, module.ExtendedDescription);
			AssertEquals("Record URL", shortcut.RecordUrl, module.RecordUrl);
			AssertEquals("Record Key", shortcut.RecordKey, module.RecordKey);
		}

		public void TestExtendedDescription()
		{
			var module = new MainFormModule(DummyModuleIDs.DummyWithExtendedDescription);

			AssertEquals("Extended Description matches", DummyModuleIDs.DummyWithExtendedDescription.ExtendedDescription, module.ExtendedDescription);
		}

		public void TestNameAndNameWithAmpersandOverride()
		{
			ModuleList moduleList = new ModuleList();
			ModuleInfo moduleInfo = moduleList[ModuleIDs.ExportClassification, Core.Constants.CountryCodes.UnitedStates, true];
			MainFormModule formModule = new MainFormModule(moduleInfo);
			AssertEquals("Description", moduleInfo.Description, formModule.Description);
			AssertNotEquals("Description", ModuleIDs.ExportClassification.Description, formModule.Description);

			formModule = new MainFormModule(ModuleIDs.ExportClassification);
			AssertEquals("Description", ModuleIDs.ExportClassification.Description, formModule.Description);
		}
		public void TestSecurityCheckPointDisplayTextShouldBeSameAsModuleInfoDescription()
		{
			Loader.LoadModules();

			var modules = Tree.Categories[ModuleTreeLoaderConstant.Category.Manage.Name].Sections[ModuleTreeLoaderConstant.Section.Payables.Name].Modules.Values.ToList<MainFormModule>();
			var formModule = modules.First(x => Equals(x.ModuleID, ModuleIDs.UnapprovedIntercompanyTransaction));
			AssertEquals("should be same", formModule.SecurityCheckpoint.DisplayText, ModuleIDs.UnapprovedIntercompanyTransaction.Description);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Loader.LoadModules();
			modules = Tree.Categories[ModuleTreeLoaderConstant.Category.Manage.Name].Sections[ModuleTreeLoaderConstant.Section.Payables.Name].Modules.Values.ToList<MainFormModule>();
			formModule = modules.First(x => Equals(x.ModuleID, ModuleIDs.UnapprovedTransaction));
			AssertEquals("should be same", formModule.SecurityCheckpoint.DisplayText, ModuleIDs.UnapprovedTransaction.Description);
		}

		public void TestSecurityCheckPointDisplayTextShouldBeDifferentWithModuleInfoDescription()
		{
			var module = new MainFormModule(DummyModuleIDs.DummyDependent);

			AssertNotEquals("should be different with default.", module.SecurityCheckpoint.DisplayText, DummyModuleIDs.DummyDependent.Description);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Tree = new ModuleTree();
			Loader = new ModuleTreeLoader();
			Loader.Initialise(Tree, Env.Security);
		}

		ModuleTreeLoader Loader;
		ModuleTree Tree;
	}
}
