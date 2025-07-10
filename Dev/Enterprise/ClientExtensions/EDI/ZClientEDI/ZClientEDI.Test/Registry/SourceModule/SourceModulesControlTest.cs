using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(SourceModulesControl))]
	class SourceModulesControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SourceModuleCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SourceModulesControl)control).SourceModulesGrid.ReadOnly;
		}
		#endregion

		public void TestImportAddsSourceModulesAsDetectedMenuItemAndDoesNotImportJumpMenuItems()
		{
			using var form = new ZForm();
			using var control = new SourceModulesControl();

			form.Controls.Add(control);
			form.Show();

			control.SetDataBinding(new SourceModuleCollection(), null);
			var addButton = control.Controls.OfType<ZButton>().First(b => b.Name == "AddCurrentButton");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddYesAnswer();
			addButton.PerformClick();

			var modules = (control.SourceModulesGrid.DataSource as SourceModuleCollection).ToArray<SourceModule>();

			Assert(UnitTestUserNotification.Instance.LastMessage.Contains(
				"Menu Item(s) have been found that are are currently not in this list and will be added."));

			Assert("One or more modules from Jump category detected",
				modules?.All(m => !m.Path.StartsWith("Jump")) ?? false);

			Assert("One or more modules not assigned ModuleListType of DetectedMenuItem",
				modules?.All(m => m.ModuleListType == ModuleListType.DetectedMenuItem) ?? false);
		}

		public void TestImportMergesExistingDetectedSourceModulesRatherThanCreateDuplicateEntries()
		{
			using var form = new ZForm();
			using var control = new SourceModulesControl();

			form.Controls.Add(control);
			form.Show();

			var sourceModule = ModuleTree.Tree.Categories.ValuesIncludingHidden
				.First(x => !x.Name.Contains("Jump") && x.Sections.Count > 0)
				.Sections
				.ValuesIncludingHidden.First(x => x.Modules.Count > 0).Modules.ValuesIncludingHidden.First();

			var sourceModuleCollection = new SourceModuleCollection();
			sourceModuleCollection.AddNew(sourceModule.ModuleTreeID, "description", "path",
				ModuleListType.DetectedMenuItem, null, true, true, "ENT");

			control.SetDataBinding(sourceModuleCollection, null);
			var addButton = control.Controls.OfType<ZButton>().First(b => b.Name == "AddCurrentButton");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddYesAnswer();
			addButton.PerformClick();

			var modules = (control.SourceModulesGrid.DataSource as SourceModuleCollection).ToArray<SourceModule>();
			var existingModule = modules.Single(m => m.Code == sourceModule.ModuleTreeID);

			Assert(UnitTestUserNotification.Instance.LastMessage.Contains(
				"Menu Item(s) have been found that are are currently not in this list and will be added."));
			Assert(UnitTestUserNotification.Instance.LastMessage.Contains(
				"[1] Menu Item(s) have a different Description or Module Tree Path and will be updated"));
			Assert(existingModule.Description == sourceModule.Description.GetUnresolvedString());
			Assert(existingModule.Path == SourceModule.GetPath(sourceModule));
		}

		public void TestImportDoesNotAffectExistingNonDetectedSourceModules()
		{
			using var form = new ZForm();
			using var control = new SourceModulesControl();

			form.Controls.Add(control);
			form.Show();

			var sourceModule = ModuleTree.Tree.Categories.ValuesIncludingHidden
				.First(x => !x.Name.Contains("Jump") && x.Sections.Count > 0)
				.Sections
				.ValuesIncludingHidden.First(x => x.Modules.Count > 0).Modules.ValuesIncludingHidden.First();

			var sourceModuleCollection = new SourceModuleCollection();
			sourceModuleCollection.AddNew(sourceModule.ModuleTreeID, null, null, ModuleListType.MenuSection, null, true,
				true);

			control.SetDataBinding(sourceModuleCollection, null);
			var addButton = control.Controls.OfType<ZButton>().First(b => b.Name == "AddCurrentButton");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddYesAnswer();
			addButton.PerformClick();

			var modules = (control.SourceModulesGrid.DataSource as SourceModuleCollection).ToArray<SourceModule>();
			Assert(modules.Count(m => m.Code == sourceModule.ModuleTreeID) == 2);
			Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(
				"Menu Item(s) have a different Description or Module Tree Path and will be updated"));
		}

		public void TestCleanMenuItemsButton()
		{
			var sourceModules = new SourceModuleCollection();
			var sourceModule1 = sourceModules.AddNew("NotInList1", "source module 1", "Dummies >", ModuleListType.MenuSection, "DUM", isSelectableForOverride: true, isSearchable: true, "ENT");
			var sourceModule2 = sourceModules.AddNew("NotInList1", "source module 2", "Dummies >", ModuleListType.MenuSection, "EDI", isSelectableForOverride: true, isSearchable: true, "ENT");
			var sourceModule3 = sourceModules.AddNew("ActiveUsers", "Active Users", "Maintain > User Admin >", ModuleListType.MenuSection, "SYS", isSelectableForOverride: true, isSearchable: true, "ENT");
			var sourceModule4 = sourceModules.AddNew("ActiveUsers", "Active Users", "Maintain > User Administrator >", ModuleListType.MenuSection, "SYS", isSelectableForOverride: true, isSearchable: true, "ENT");
			var sourceModule5 = sourceModules.AddNew("Quotations", "Quotations", "Manage > Tariffs & Rates >", ModuleListType.MenuSection, "TAR", isSelectableForOverride: true, isSearchable: true, "ENT");
			var sourceModule6 = sourceModules.AddNew("NotInList2", "source module 3", "Dummies2 >", ModuleListType.MenuSection, "DUM", isSelectableForOverride: true, isSearchable: true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			using (var form = new ZForm())
			using (var control = new SourceModulesControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(sourceModules, null);
				control.SourceModulesGrid.SelectAllElements();

				Assert("Clean Up button should be visible", control.CleanupButton_Exposed.Visible);
				control.ClickCleanUp();

				var sourceModulesList = sourceModules.Cast<SourceModule>().ToList();

				var newSourceModule1 = sourceModulesList.FirstOrDefault(c => c.PK == sourceModule1.PK);
				AssertSourceModule(newSourceModule1, ModuleListType.MenuSection, "DUM");

				var newSourceModule2 = sourceModulesList.FirstOrDefault(c => c.PK == sourceModule2.PK);
				AssertSourceModule(newSourceModule2, ModuleListType.MenuSection, "EDI");

				var newSourceModule3 = sourceModulesList.FirstOrDefault(c => c.PK == sourceModule3.PK);
				AssertSourceModule(newSourceModule3, ModuleListType.DetectedMenuItem, "SYS");

				var newSourceModule4 = sourceModulesList.FirstOrDefault(c => c.PK == sourceModule4.PK);
				AssertNull("Should remove duplicated", newSourceModule4);

				var newSourceModule5 = sourceModulesList.FirstOrDefault(c => c.PK == sourceModule5.PK);
				AssertSourceModule(newSourceModule5, ModuleListType.DetectedMenuItem, "TAR");

				var newSourceModule6 = sourceModulesList.FirstOrDefault(c => c.PK == sourceModule6.PK);
				AssertSourceModule(newSourceModule6, ModuleListType.MenuSection, "DUM");

				void AssertSourceModule(SourceModule sourceModule, ModuleListType moduleListType, string defaultModule)
				{
					AssertEquals(moduleListType, sourceModule.ModuleListType);
					AssertEquals(defaultModule, sourceModule.DefaultModule);
				}

				AssertEquals("Should hide Clean Up button when Detected Menu is found in the list", false, control.CleanupButton_Exposed.Visible);
			}
		}

		class SourceModulesControlForTest : SourceModulesControl
		{
			public void ClickCleanUp()
			{
				CleanUpButton_Click(null, null);
			}

			public Button CleanupButton_Exposed => this.CleanUpButton;
		}
	}
}
