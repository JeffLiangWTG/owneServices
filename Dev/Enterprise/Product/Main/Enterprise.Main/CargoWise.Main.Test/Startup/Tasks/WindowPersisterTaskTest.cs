using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class WindowPersisterTaskTest : TransactionedTestCase
	{
		[UseSnapshotProtection]
		public void TestExecute_RememberOpenedFormsAfterUpgradeIsFalse()
		{
			using var restoreArgs = StaticFieldTestHelper.CacheAndRestoreStaticCommandLineArguments();

			using (var staffForm = new GlbStaffForm(CurrentStaffFromNewFactory))
			{
				staffForm.Show();
				var formUrls = WindowPersister.GetOpenFormUrls();
				CommandLineArguments.UsedToLaunchApplication.OptionalArgs[ApplicationArguments.OptionPersist] = formUrls;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				new WindowPersisterTask().Execute();
				Application.DoEvents();

				Assert(!UnitTestUserNotification.Instance.LastMessage.Contains("Do you want to pick up forms where left?"));

				var forms = ZApplication.GetOpenForms().Where(f => f is GlbStaffForm);
				AssertEquals("there should be 2 glbstaff forms", 2, forms.Count());

				var form = forms.FirstOrDefault(f => f.Handle != staffForm.Handle);
				AssertNotNull(form);
				form.Close();
				Application.DoEvents();
			}
		}

		[UseSnapshotProtection]
		public void TestExecute_RememberOpenedFormsAfterUpgradeIsTrue()
		{
			Env.Registry.RememberOpenedFormsAfterUpgrade = true;
			using var restoreArgs = StaticFieldTestHelper.CacheAndRestoreStaticCommandLineArguments();

			using (var staffForm = new GlbStaffForm(CurrentStaffFromNewFactory))
			{
				staffForm.Show();
				var formUrls = WindowPersister.GetOpenFormUrls();
				CommandLineArguments.UsedToLaunchApplication.OptionalArgs[ApplicationArguments.OptionPersist] = formUrls;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				new WindowPersisterTask().Execute();
				Application.DoEvents();

				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Do you want to pick up forms where left?"));

				var forms = ZApplication.GetOpenForms().Where(f => f is GlbStaffForm);
				AssertEquals("there should be 1 glbstaff form", 1, forms.Count());
				AssertEquals("the only form handle", staffForm.Handle, forms.First().Handle);
			}

			using (var staffForm = new GlbStaffForm(CurrentStaffFromNewFactory))
			{
				staffForm.Show();
				var formUrls = WindowPersister.GetOpenFormUrls();
				CommandLineArguments.UsedToLaunchApplication.OptionalArgs[ApplicationArguments.OptionPersist] = formUrls;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				new WindowPersisterTask().Execute();
				Application.DoEvents();

				Thread.Sleep(1000);

				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Do you want to pick up forms where left?"));

				var forms = ZApplication.GetOpenForms().Where(f => f is GlbStaffForm);
				AssertEquals("there should be 2 glbstaff forms", 2, forms.Count());

				var form = forms.FirstOrDefault(f => f.Handle != staffForm.Handle);
				AssertNotNull(form);
				form.Close();
			}
		}

		GlbStaff CurrentStaffFromNewFactory
		{
			get
			{
				if (currentStaffFromNewFactory == null)
				{
					currentStaffFromNewFactory = new BusinessObjectFactory() { NameForDebugging = "Staff Details Update Form" }.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				}
				return currentStaffFromNewFactory;
			}
		}
		GlbStaff currentStaffFromNewFactory;
	}
}
