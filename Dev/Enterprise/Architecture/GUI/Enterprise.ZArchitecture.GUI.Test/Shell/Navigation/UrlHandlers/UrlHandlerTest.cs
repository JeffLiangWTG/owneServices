using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.RemoteDesktopServices;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class UrlHandlerTest : TestCaseWithFactory
	{
		#region Licence

		[ExpectNoExceptions]
		public void TestEnsureLoggedWithCorrectProductKey_WhenLoggedWithCorrectProductKey()
		{
			var currentCompany = StaticCurrentFetcher.Instance.CurrentCompany;

			var url = $"edient:Command=Test&LicenceCode={currentCompany.LicenceKeyIdentifier}&ControllerID=Dummy";
			var queryString = new QueryString();
			queryString.Deserialize(Enterprise.ZArchitecture.UrlHandler.GetQueryStringTextFromUrl(url));

			UrlHandler.Handle(queryString);
		}

		public void TestEnsureLoggedWithCorrectProductKey_WhenLoggedWithWrongEnterpriseCode()
		{
			var currentCompany = StaticCurrentFetcher.Instance.CurrentCompany;
			var serverCode = currentCompany.LicenceServerID;
			var companyCode = currentCompany.GC_Code;

			var url = $"edient:Command=Test&LicenceCode=BAD{companyCode}{serverCode}&ControllerID=Dummy";
			var queryString = new QueryString();
			queryString.Deserialize(Enterprise.ZArchitecture.UrlHandler.GetQueryStringTextFromUrl(url));

			try
			{
				UrlHandler.Handle(queryString);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals(
				$"{Enterprise.Core.Constants.ProductName} is not running with the correct product key or from the correct licensed server installation directory.\r\n" +
				$"Log into the application with enterprise code 'BAD' and server code '{serverCode}', and try again.", ex.Message);
			}
		}

		public void TestEnsureLoggedWithCorrectProductKey_WhenLoggedWithWrongServerCode()
		{
			var currentCompany = StaticCurrentFetcher.Instance.CurrentCompany;
			var enterpriseCode = currentCompany.LicenceEnterpriseCode;
			var companyCode = currentCompany.GC_Code;

			var url = $"edient:Command=Test&LicenceCode={enterpriseCode}{companyCode}BAD&ControllerID=Dummy";
			var queryString = new QueryString();
			queryString.Deserialize(Enterprise.ZArchitecture.UrlHandler.GetQueryStringTextFromUrl(url));

			try
			{
				UrlHandler.Handle(queryString);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals(
				$"{Enterprise.Core.Constants.ProductName} is not running with the correct product key or from the correct licensed server installation directory.\r\n" +
				$"Log into the application with enterprise code '{enterpriseCode}' and server code 'BAD', and try again.", ex.Message);
			}
		}

		public void TestGetCurrentCompanyLicenceKeyIdentifier_WhenCompanyIsNull()
		{
			AssertNoExceptionThrown(() => TestUrlHandler.GetCurrentCompanyLicenceKeyIdentifier(null));
		}

		public void TestEnsureLoggedWithCorrectProductKey_WhenLicenceCodeIsEmpty()
		{
			var url = $"edient:Command=Test&LicenceCode=&ControllerID=Dummy";
			var queryString = new QueryString();
			queryString.Deserialize(Enterprise.ZArchitecture.UrlHandler.GetQueryStringTextFromUrl(url));

			var exceptedExceptionMessage = $"{Enterprise.Core.Constants.ProductName} is not running with the correct product key or from the correct licensed server installation directory.\r\n" +
				"Log into the application with enterprise code '' and server code '', and try again.";

			AssertExceptionThrown<EnterpriseUrlHandlerException>("Expect an exception", exceptedExceptionMessage, () => UrlHandler.Handle(queryString));
		}

		#endregion

		#region Main Form
#if !WINZOR

		public void TestActivateFormShouldAlsoActivateMainFormIfIsRemoteAppSession()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());

			using (var mainForm = new Form())
			using (var editForm = new Form())
			{
				editForm.WindowState = FormWindowState.Minimized;
				var urlHandler = new TestUrlHandler();
				urlHandler.MainTestForm = mainForm;

				AssertEquals("MainFormActivated should be false", false, urlHandler.MainFormActivated);

				urlHandler.ActivateForm_Exposed(editForm);

				AssertEquals("editForm should be un-minimized and brought to the front", FormWindowState.Normal, editForm.WindowState);
				AssertEquals("mainForm should be also activated", true, urlHandler.MainFormActivated);
			}
		}

		public void TestForceMainFormToActivate()
		{
			MainForm.Show();
			MainForm.WindowState = FormWindowState.Minimized;
			Application.DoEvents();

			UrlHandler.ForceFormToActivate_Exposed(MainForm);
			AssertEquals("Window should be un-minimized and brought to the front", FormWindowState.Normal, MainForm.WindowState);
		}

		public void TestForceMainFormToActivate_DontActivateDisposed()
		{
			MainForm.Show();
			MainForm.WindowState = FormWindowState.Minimized;
			Application.DoEvents();

			MainForm.Dispose();

			AssertNoExceptionThrown("Due to Application.DoEvents being called, there is no guarantee this form hasn't been disposed.", () => UrlHandler.ForceFormToActivate_Exposed(MainForm));
		}

#endif

		#endregion

		#region Test Classes

		class TestUrlHandler : UrlHandler
		{
			public TestUrlHandler()
			{
			}

			#region CanHandle / Handle

			protected override string ExpectedCommandText
			{
				get { return "Test"; }
			}

			protected override bool CanHandleCore(QueryString queryString)
			{
				return true;
			}

			protected override bool HandleCore(QueryString queryString)
			{
				return false;
			}

			#endregion

			#region Form activation
#if !WINZOR

			public void ForceFormToActivate_Exposed(Form form)
			{
				ForceFormToActivate(form);
			}

			public void ActivateForm_Exposed(Form form) => ActivateForm(form);

			protected override Form LocateMainForm()
			{
				if (MainTestForm != null)
				{
					return MainTestForm;
				}

				return base.LocateMainForm();
			}
			public Form MainTestForm { get; set; }

			protected override void ActivateMainForm()
			{
				base.ActivateMainForm();
				MainFormActivated = true;
			}

			public bool MainFormActivated { get; private set; }
#endif
			#endregion

			#region Licence/Company

			public void SetCurrentCompany(IGlbCompany value)
			{
				currentCompany = value;
			}

			protected override IGlbCompany CurrentCompany
			{
				get { return currentCompany ?? base.CurrentCompany; }
			}
			IGlbCompany currentCompany;

			#endregion
		}

		#endregion

		#region Implementation

		TestUrlHandler UrlHandler
		{
			get { return urlHandler ?? (urlHandler = new TestUrlHandler()); }
		}
		TestUrlHandler urlHandler;

#if !WINZOR
		Form MainForm
		{
			get { return mainForm ?? (mainForm = new Form()); }
		}
#endif
#pragma warning disable IDE0044 //Object mainForm is getting modified, conflicting readonly property
		Form mainForm;
#pragma warning restore IDE0044

		protected override void TearDown()
		{
			base.TearDown();
			if (mainForm != null)
			{
				mainForm.Dispose();
			}
		}

		#endregion
	}
}
