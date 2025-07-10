using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Core.GUI.Testing
{
	public class WindowPersisterTest : TestCaseWithFactory
	{
		public void TestWindowsPersisterDoesntTouchTheDbWhenDatabaseUpgradeException()
		{
			var formsToDisplay = new[] {
				ControllerIDs.GlbStaff,
				ControllerIDs.Organisation,
				ControllerIDs.GlbBranch,
			};

			using (var forms = new DisposableList(formsToDisplay.Length))
			{
				foreach (var id in formsToDisplay)
				{
					var controller = (ZController)ZControllerFactory.Instance.CreateNew(id);
					var bizo = Factory.NewWithValidTestData(controller.TypeOfTopLevelBusinessObject);
					Factory.Save();

					forms.Add(controller.ShowEditForm(bizo));
				}

				var dbHits = Db.Connection.ExecutedCommandCount;
				RegistryItemDictionary.Instance.PurgeAll();

				ObjectFactory.Get<IProductRegistration>().ResetKeyToDefault();
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;

				try
				{
					WindowPersister.GetOpenFormUrls();
					AssertEquals("We cannot touch the DB when storing the form states - this may be called by the DbUpgradedException handlers", dbHits, Db.Connection.ExecutedCommandCount);
				}
				finally
				{
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}
			}
		}

		public void TestWindowsShouldBePersisted()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyController();
					using (var form = controller.ShowEditForm(dummy))
					{
						AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));

						WindowPersister.SaveUrlsToRegistry();
						OpenedFormCache.GetInstance().CloseAllCachedForms();
						AssertNull("Should have closed the form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));

						WindowPersister.LoadUrlsFromRegistry();
						AssertNotNull("Should have reopened a form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestWindowsShouldBePersistedForDifferentUsers()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyController();

					using (CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
					{
						var form = controller.ShowEditForm(dummy1) as Form;
						AssertNotNull("Should have opened form 1.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy1.PK));
						WindowPersister.SaveUrlsToRegistry();
						form.Close();
					}

					using (CurrentUserChanger.SwitchToNewUserTemporarily(User.SupportUserName))
					{
						var form = controller.ShowEditForm(dummy2) as Form;
						AssertNotNull("Should have opened form 2.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy2.PK));
						WindowPersister.SaveUrlsToRegistry();
						form.Close();
					}

					using (CurrentUserChanger.SwitchToNewUserTemporarily(User.PostMasterUserName))
					{
						Form form;
						WindowPersister.LoadUrlsFromRegistry();
						AssertNotNull("Should have reopened form 1.", form = Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy1.PK));
						AssertNull("Should not have reopened form 2.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy2.PK));
						form.Close();
					}

					using (CurrentUserChanger.SwitchToNewUserTemporarily(User.SupportUserName))
					{
						Form form;
						WindowPersister.LoadUrlsFromRegistry();
						AssertNull("Should not have reopened form 1.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy1.PK));
						AssertNotNull("Should have reopened form 2.", form = Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy2.PK));
						form.Close();
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestGetOpenFormUrls()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyController();
					using (var form = controller.ShowEditForm(dummy))
					{
						AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));

						var uRL = WindowPersister.GetOpenFormUrls();
						AssertContains("Should have returned the correct URL.", "edient:", uRL);
						AssertContains("Should have returned the correct URL.", "Command=ShowEditForm", uRL);
						AssertContains("Should have returned the correct URL.", "ControllerID=Dummy", uRL);
						AssertContains("Should have returned the correct URL.", "BusinessEntityPK=" + dummy.PK.ToString(), uRL);
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestGetOpenFormUrlsShouldIncludeModuleFormUrlInCWNext()
		{
#if !WINZOR
			using (CWNextFeatureTestHelper.EnableCWNext())
			{
#endif
				try
				{
					using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
					{
						using (var form = module.ShowPopup())
						{
							AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<EmbeddedModulePopup>().FirstOrDefault(x => x.Module.ID == DummyModuleIDs.Dummy));

							var uRL = WindowPersister.GetOpenFormUrls();
							AssertContains("Should have returned the correct URL.", "edient:", uRL);
							AssertContains("Should have returned the correct URL.", "Command=ShowModule", uRL);
							AssertContains("Should have returned the correct URL.", "ModuleID=Dummy", uRL);
						}
					}
				}
				finally
				{
					OpenedFormCache.GetInstance().CloseAllCachedForms();
				}
#if !WINZOR
			}
#endif
		}

#if !WINZOR
		public void TestGetOpenFormUrlsShouldNotIncludeModuleFormUrlInCWOne()
		{
			using (CWNextFeatureTestHelper.DisableCWNext())
			{
				try
				{
					using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
					{
						using (var form = module.ShowPopup())
						{
							AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<EmbeddedModulePopup>().FirstOrDefault(x => x.Module.ID == DummyModuleIDs.Dummy));

							var uRL = WindowPersister.GetOpenFormUrls();
							Assert("Should not inculde module form url.", uRL.Length == 0);
						}
					}
				}
				finally
				{
					OpenedFormCache.GetInstance().CloseAllCachedForms();
				}
			}
		}
#endif

		public void TestGetOpenFormUrlsNoExceptionThrown()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyController();
					using (var form = controller.ShowEditForm(dummy))
					{
						var thread = new Thread(() =>
						{
							AssertNoExceptionThrown(() => WindowPersister.GetOpenFormUrls());
						});

						thread.Start();
						thread.Join();
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		[ExpectNoExceptions]
		public void TestGetOpenFormUrlsNullID()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyController();
					using (var form = controller.ShowEditForm(dummy))
					{
						form.ControllerID = null;
						AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));

						var uRL = WindowPersister.GetOpenFormUrls();
						AssertEquals("URL should be empty", "", uRL);
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		[ExpectNoExceptions]
		public void TestGetOpenFormUrlsNotSupportsHyperlinking()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyControllerNotSupportsHyperlinking();

					using (var form = controller.ShowEditForm(dummy))
					{
						var uRL = WindowPersister.GetOpenFormUrls();
						AssertEquals("URL should be empty", "", uRL);
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestGetOpenFormUrls_WithArgs()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using ((DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var controller = new DummyController();
					using (var form = (ZDummyForm)controller.ShowEditForm(dummy))
					{
						form.FormArgsToPersistOnClose_ForTest = new[] { "one", "two", "THREE!" };
						AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));

						var url = WindowPersister.GetOpenFormUrls();
						AssertContains("The URL should contain the args from the form, and yet...", "Args=one%7ctwo%7cTHREE!", url);
					}
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestOpenFormsFromUrls()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			try
			{
				using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					WindowPersister.OpenFormsFromUrls("edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=" + dummy.PK.ToString());
					AssertNotNull("Should have opened a form.", Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault(x => x.BusinessEntity.Identifier == dummy.PK));
				}
			}
			finally
			{
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}
		}

		public void TestLoadUrlsFromRegistryDoesntTouchTheDbWhenWindowPersisterDataIsEmpty()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				WindowPersister.LoadUrlsFromRegistry();
				var executedCommands = Db.Connection.ExecutedCommands?.ToList();
				AssertEquals(false, executedCommands.Any(cmd => cmd.ToLower().Contains("delete from dbo.stmdata")));
			}
		}
	}
}
