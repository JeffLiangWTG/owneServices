using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class NonTransactionedZGridColourCustomiseFormTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSecondUserChangeDoesntCauseException()
		{
			var factory = new BusinessObjectFactory();
			var company1 = factory.NewWithValidTestData<GlbCompany>();
			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			var branch2 = factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			var staff = factory.New<GlbStaff>();
			staff.GS_LoginName = "TST";

			factory.Save();

			var masterScheme = CreateScheme(factory);
			var temp = EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.IsAllowedForAllBranches;
			EnvProxy.Instance.Security.PublishGlobalGridColorSchemes.IsAllowedForAllBranches = true;

			using (OpenForEdit(masterScheme, Env.CurrentUser, branch2, out var tempForm1, out _))
			{
				tempForm1.Show();

				using (OpenForEdit(masterScheme, (IUser)(object)staff, branch1, out var tempForm2, out _))
				{
					tempForm2.Show();

					tempForm2.UncheckPublished();
					tempForm2.UncheckPublishedForAllCompanies();

					tempForm2.FireSaveButton();
				}

				tempForm1.UncheckPublishedForAllCompanies();

				AssertNoExceptionThrown(() => tempForm1.FireSaveButton());
			}
		}

		IDisposable OpenForEdit(GridColourScheme scheme, IUser user, GlbBranch branch, out ZGridColourCustomiseFormTest.ZGridColourCustomiseFormForTest form, out BusinessObjectFactory factory)
		{
			var disposables = new DisposableList(3);
			try
			{
				factory = new BusinessObjectFactory { RefreshEnabled = false };
				var context = new UserContext(user.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK, null, true, factory);
				disposables.Add(EnvProxy.Instance.SetTemporaryUserContext(context));

				var reloadedModuleFilter = factory.Load<StmModuleFilter>(scheme.ColourStrips[0].StmModuleFilter.PK);
				var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest(factory);
				strip.LoadLayout(reloadedModuleFilter);

				var schemeReloaded = factory.Load<GridColourScheme>(scheme.PK);
				schemeReloaded.SetStripsFromFilter(strip, null);

				var control = new FilterStripControlForTest(null, strip);
				disposables.Add(control);

				form = new ZGridColourCustomiseFormTest.ZGridColourCustomiseFormForTest(schemeReloaded, strip, control);
				disposables.Add(form);

				form.Show();
				Application.DoEvents();
				Application.DoEvents();

				form.Focus();

				return disposables;
			}
			catch
			{
				disposables.Dispose();
				throw;
			}
		}

		GridColourScheme CreateScheme(BusinessObjectFactory factory)
		{
			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest(factory);
			var scheme = factory.New<GridColourScheme>();

			var colorStrip = new GridColourStripBusinessObject(strip, null, null);
			colorStrip.FilterStrips.AddNew();
			colorStrip.BGColor = Color.Bisque;
			colorStrip.RuleName = "rule1";

			scheme.S9_ModuleID = "test_module" + StmModuleFilter.ModuleIdSuffix.GridColorScheme;
			scheme.S9_FilterName = "My Test Filter";
			scheme.S9_IsPublished = true;
			scheme.PublishAcrossAllCompanies = true;

			scheme.ColourStrips.Add(colorStrip);
			colorStrip.SaveLayout(colorStrip.RuleName);

			factory.Save();

			return scheme;
		}
	}
}
