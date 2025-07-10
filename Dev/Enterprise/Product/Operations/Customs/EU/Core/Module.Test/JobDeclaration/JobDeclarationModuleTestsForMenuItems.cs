using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing;

public class JobDeclarationModuleTestsForMenuItems : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestGetNewStandardMenuItems()
	{
		using (var module = new JobDeclarationModule())
		{
			module.SetupAndGetGrid();
			using (var menu = module.NewMenuItem)
			{
				AssertEquals("New Declaration", menu.MenuItems[0].Text);
				AssertContains("Export Wizard", menu.MenuItems[1].Text);
				AssertContains("Import Wizard", menu.MenuItems[2].Text);
				AssertContains("SAD/H", menu.MenuItems[3].Text);
			}
		}
	}

	[RequiresSTA]
	public void TestNewStandardMenuItemsActuallyDoSomething_NewExportWizard()
	{
		RunMenuItemClickTest(1);
	}

	[RequiresSTA]
	public void TestNewStandardMenuItemsActuallyDoSomething_NewImportWizard()
	{
		RunMenuItemClickTest(2);
	}

	[RequiresSTA]
	public void TestNewStandardMenuItemsActuallyDoSomething_SadHWizard()
	{
		RunMenuItemClickTest(3);
	}

	protected void RunMenuItemClickTest(int menuItemIndex)
	{
		AssertEquals(0, Factory.GetDatabaseCount(typeof(JobDeclaration), new ZQuery()));
		using (var module = GetNewJobDeclarationModule(false))
		using (var grid = module.SetupAndGetGrid())
		using (var menu = module.NewMenuItem)
		{
			menu.MenuItems[menuItemIndex].PerformClick();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobDeclaration), new ZQuery()));
		}
		using (var module = GetNewJobDeclarationModule(true))
		using (var grid = module.SetupAndGetGrid())
		using (var menu = module.NewMenuItem)
		{
			menu.MenuItems[menuItemIndex].PerformClick();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration), new ZQuery()));
		}
	}

	[RequiresSTA]
	public void TestSaveExceptionIsHandled()
	{
		using (var module = new TestJobDeclarationModuleWhichThrowsSaveException(true))
		using (var grid = module.SetupAndGetGrid())
		using (var menu = module.NewMenuItem)
		{
			menu.MenuItems[2].PerformClick();
			var lastNotificationText = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("ZSaveException should result in a user notification", "The following error was encountered while saving the changes", lastNotificationText);
			AssertContains("ZSaveException should result in a user notification", "Save Exception Testing", lastNotificationText);
		}
	}

	[RequiresSTA]
	public void TestSaveConcurrencyExceptionIsHandled()
	{
		using (var module = new TestJobDeclarationModuleWhichThrowsSaveConcurrencyException(true))
		using (var grid = module.SetupAndGetGrid())
		using (var menu = module.NewMenuItem)
		{
			menu.MenuItems[2].PerformClick();
			var lastNotificationText = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("ZSaveException should result in a user notification", "The following error was encountered while saving the changes", lastNotificationText);
			AssertContains("ZSaveException should result in a user notification", "Save Concurrency Exception Testing", lastNotificationText);
		}
	}

	protected virtual JobDeclarationModule GetNewJobDeclarationModule(bool p)
	{
		return new JobDeclarationModuleForTest(p);
	}

	protected class JobDeclarationModuleForTest : JobDeclarationModule
	{
		public JobDeclarationModuleForTest(bool simulateUserClickGo)
		{
			this.simulateUserClickGo = simulateUserClickGo;
		}
		protected override SingleLineEntryManager GetNewSingleLineEntryManager(JobDeclaration declaration)
		{
			return new SingleLineEntryManagerForTest(declaration, simulateUserClickGo);
		}
		protected override SADHFormDataManager GetNewSadhManager(JobDeclaration declaration)
		{
			return new SADHFormDataManagerForTest(declaration, simulateUserClickGo);
		}

		readonly bool simulateUserClickGo;
	}

	protected class SingleLineEntryManagerForTest : SingleLineEntryManager
	{
		public SingleLineEntryManagerForTest(JobDeclaration declaration, bool simulateUserClickGo)
			: base(declaration)
		{
			ExecutedSuccessfully = simulateUserClickGo;
		}
	}

	protected class SADHFormDataManagerForTest : SADHFormDataManager
	{
		public SADHFormDataManagerForTest(JobDeclaration declaration, bool simulateUserClickGo)
			: base(declaration)
		{
			ExecutedSuccessfully = simulateUserClickGo;
		}
	}

	protected class TestJobDeclarationModuleWhichThrowsSaveException : JobDeclarationModuleForTest
	{
		public TestJobDeclarationModuleWhichThrowsSaveException(bool simulateUserClickGo)
			: base(simulateUserClickGo)
		{
		}

		protected override void Save() => throw new ZSaveException(new ZDataException(new ApplicationException("Save Exception Testing"), null, Db.Connection), Factory);
	}

	protected class TestJobDeclarationModuleWhichThrowsSaveConcurrencyException : JobDeclarationModuleForTest
	{
		public TestJobDeclarationModuleWhichThrowsSaveConcurrencyException(bool simulateUserClickGo)
			: base(simulateUserClickGo)
		{
		}

		protected override void Save() => throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new ApplicationException("Save Concurrency Exception Testing"), null, Db.Connection), Factory);
	}
}
